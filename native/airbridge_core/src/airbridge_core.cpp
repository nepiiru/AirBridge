/*
 * AirBridge Core - C ABI wrapper around UxPlay
 * Copyright (C) 2026 Nepiiru
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

#include "airbridge_core.h"

#include <atomic>
#include <cstring>
#include <string>
#include <thread>
#include <vector>

extern "C" void video_renderer_set_window_handle(uintptr_t handle);

namespace {

struct Config {
    std::string receiver_name;
    std::string audio_device;
    std::string video_sink;
    std::string pin;
    int port = 7000;
    int hls_enabled = 0;
    int pin_mode = AIRBRIDGE_PIN_NONE;
    int nohold = 0;
    int log_level = 1;
    bool append_hostname = true;
};

Config g_config;
AirBridgeCallbacks g_callbacks{};
std::thread g_thread;
std::atomic<bool> g_running{false};

std::string copy_string(const char *value, const char *fallback = "") {
    return value ? std::string(value) : std::string(fallback);
}

void emit_state(const char *state) {
    if (g_callbacks.on_state) {
        g_callbacks.on_state(g_callbacks.user_data, state);
    }
}

void emit_client(const char *name, const char *model, const char *device_id) {
    if (g_callbacks.on_client) {
        g_callbacks.on_client(g_callbacks.user_data, name, model, device_id);
    }
}

void emit_metadata(const char *title, const char *artist, const char *album) {
    if (g_callbacks.on_metadata) {
        g_callbacks.on_metadata(g_callbacks.user_data, title, artist, album);
    }
}

void emit_artwork(const unsigned char *data, int len) {
    if (g_callbacks.on_artwork) {
        g_callbacks.on_artwork(g_callbacks.user_data, data, len);
    }
}

void emit_progress(double position, double duration) {
    if (g_callbacks.on_progress) {
        g_callbacks.on_progress(g_callbacks.user_data, position, duration);
    }
}

void emit_volume(double db, double linear) {
    if (g_callbacks.on_volume) {
        g_callbacks.on_volume(g_callbacks.user_data, db, linear);
    }
}

void emit_pin(const char *pin) {
    if (g_callbacks.on_pin) {
        g_callbacks.on_pin(g_callbacks.user_data, pin);
    }
}

void emit_video_size(int width, int height) {
    if (g_callbacks.on_video_size) {
        g_callbacks.on_video_size(g_callbacks.user_data, width, height);
    }
}

void emit_error(int code, const char *message) {
    if (g_callbacks.on_error) {
        g_callbacks.on_error(g_callbacks.user_data, code, message);
    }
}

void emit_log(int level, const char *message) {
    if (g_callbacks.on_log) {
        g_callbacks.on_log(g_callbacks.user_data, level, message);
    }
}

std::vector<std::string> build_arguments() {
    std::vector<std::string> args;
    args.emplace_back("airbridge-engine");

    args.emplace_back("-n");
    args.emplace_back(g_config.receiver_name.empty() ? "AirBridge"
                                                     : g_config.receiver_name);

    if (!g_config.append_hostname) {
        args.emplace_back("-nh");
    }

    args.emplace_back("-p");
    args.emplace_back(std::to_string(g_config.port > 0 ? g_config.port : 7000));

    if (!g_config.audio_device.empty()) {
        args.emplace_back("-as");
        args.emplace_back("wasapisink device=\"" + g_config.audio_device + "\"");
    } else {
        args.emplace_back("-as");
        args.emplace_back("wasapisink");
    }

    args.emplace_back("-vs");
    args.emplace_back(g_config.video_sink.empty() ? "d3d11videosink"
                                                  : g_config.video_sink);

    if (g_config.hls_enabled) {
        args.emplace_back("-hls");
    }

    if (g_config.pin_mode == AIRBRIDGE_PIN_RANDOM) {
        args.emplace_back("-pin");
    } else if (g_config.pin_mode == AIRBRIDGE_PIN_PASSWORD) {
        args.emplace_back("-pw");
        args.emplace_back(g_config.pin);
    }

    if (g_config.nohold) {
        args.emplace_back("-nohold");
    }

    return args;
}

}  // namespace

extern "C" {

AB_API const char *airbridge_version(void) {
    return "0.1.0";
}

AB_API int airbridge_init(const AirBridgeConfig *config,
                          const AirBridgeCallbacks *callbacks) {
    if (!config) {
        return -1;
    }
    if (g_running.load()) {
        return -2;
    }

    if (g_thread.joinable()) {
        g_thread.join();
    }

    g_config = Config{};
    g_config.receiver_name = copy_string(config->receiver_name, "AirBridge");
    g_config.audio_device = copy_string(config->audio_device);
    g_config.video_sink = copy_string(config->video_sink, "d3d11videosink");
    g_config.pin = copy_string(config->pin);
    g_config.port = config->port;
    g_config.hls_enabled = config->hls_enabled;
    g_config.pin_mode = config->pin_mode;
    g_config.nohold = config->nohold;
    g_config.log_level = config->log_level;
    g_config.append_hostname = config->append_hostname != 0;

    if (callbacks) {
        g_callbacks = *callbacks;
    } else {
        std::memset(&g_callbacks, 0, sizeof(g_callbacks));
    }

    return 0;
}

AB_API int airbridge_start(void) {
    if (g_running.load()) {
        return -2;
    }
    if (g_thread.joinable()) {
        g_thread.join();
    }

    const std::vector<std::string> args = build_arguments();
    const std::string receiver_name = g_config.receiver_name;

    g_running.store(true);
    g_thread = std::thread([args, receiver_name]() {
        if (!receiver_name.empty()) {
            airbridge_engine_set_receiver_name(receiver_name.c_str());
        }

        std::vector<char *> argv;
        argv.reserve(args.size() + 1);
        for (const auto &arg : args) {
            argv.push_back(const_cast<char *>(arg.c_str()));
        }
        argv.push_back(nullptr);

        airbridge_engine_main(static_cast<int>(args.size()), argv.data());
        g_running.store(false);
    });

    return 0;
}

AB_API void airbridge_set_window_handle(uintptr_t hwnd) {
    video_renderer_set_window_handle(hwnd);
}

AB_API void airbridge_set_volume(double db) {
    airbridge_engine_set_volume(db);
}

AB_API void airbridge_stop(void) {
    if (g_running.load()) {
        airbridge_engine_request_stop();
    }
    if (g_thread.joinable()) {
        g_thread.join();
    }
    g_running.store(false);
}

AB_API void airbridge_shutdown(void) {
    airbridge_stop();
    std::memset(&g_callbacks, 0, sizeof(g_callbacks));
}

/* Hooks called by UxPlay. */

void airbridge_emit_state(const char *state) {
    emit_state(state);
}

void airbridge_emit_client(const char *name, const char *model,
                           const char *device_id) {
    emit_client(name, model, device_id);
}

void airbridge_emit_metadata(const char *title, const char *artist,
                             const char *album) {
    emit_metadata(title, artist, album);
}

void airbridge_emit_artwork(const unsigned char *data, int len) {
    if (data && len > 0) {
        emit_artwork(data, len);
    }
}

void airbridge_emit_progress(double position, double duration) {
    emit_progress(position, duration);
}

void airbridge_emit_volume(double db, double linear) {
    emit_volume(db, linear);
}

void airbridge_emit_pin(const char *pin) {
    emit_pin(pin);
}

void airbridge_emit_video_size(int width, int height) {
    emit_video_size(width, height);
}

void airbridge_emit_error(int code, const char *message) {
    emit_error(code, message);
}

void airbridge_emit_log(int level, const char *message) {
    emit_log(level, message);
}

}  // extern "C"
