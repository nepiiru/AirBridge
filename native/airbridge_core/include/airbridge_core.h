/*
 * AirBridge Core - C ABI wrapper around UxPlay
 * Copyright (C) 2026 Nepiiru
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

#ifndef AIRBRIDGE_CORE_H
#define AIRBRIDGE_CORE_H

#include <stdint.h>

#ifdef _WIN32
#  ifdef AIRBRIDGE_CORE_EXPORTS
#    define AB_API __declspec(dllexport)
#  else
#    define AB_API __declspec(dllimport)
#  endif
#else
#  define AB_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* ------------------------------------------------------------------------- */
/* Public API                                                                */
/* ------------------------------------------------------------------------- */

typedef enum AirBridgePinMode {
    AIRBRIDGE_PIN_NONE = 0,     /* no access control */
    AIRBRIDGE_PIN_RANDOM = 1,   /* show a random 4-digit PIN per connection */
    AIRBRIDGE_PIN_PASSWORD = 2  /* fixed password */
} AirBridgePinMode;

typedef struct AirBridgeConfig {
    const char *receiver_name;   /* UTF-8 display name, NULL = "AirBridge" */
    const char *audio_device;    /* WASAPI endpoint id, NULL = default */
    const char *video_sink;      /* GStreamer video sink, NULL = d3d11videosink */
    int         port;            /* AirPlay port, 0 = 7000 */
    int         hls_enabled;     /* non-zero = accept HLS (YouTube app) streams */
    int         pin_mode;        /* AirBridgePinMode */
    const char *pin;             /* password for AIRBRIDGE_PIN_PASSWORD */
    int         nohold;          /* non-zero = do not keep window after disconnect */
    int         log_level;       /* 0 = errors only, 1 = info, 2 = debug */
    int         append_hostname; /* non-zero = append "@PC-name" to receiver name */
} AirBridgeConfig;

typedef struct AirBridgeCallbacks {
    void *user_data;

    /* "idle" | "connected" | "audio" | "mirror" | "hls" */
    void (*on_state)(void *user_data, const char *state);
    /* client identification; any of the strings may be NULL */
    void (*on_client)(void *user_data, const char *name, const char *model,
                      const char *device_id);
    /* now-playing metadata; any of the strings may be NULL */
    void (*on_metadata)(void *user_data, const char *title, const char *artist,
                        const char *album);
    /* raw cover art bytes (JPEG/PNG); valid only for the duration of the call */
    void (*on_artwork)(void *user_data, const unsigned char *data, int len);
    /* seconds */
    void (*on_progress)(void *user_data, double position, double duration);
    /* AirPlay volume in dB and the linear gain actually applied */
    void (*on_volume)(void *user_data, double db, double linear);
    /* PIN to display for the current connection */
    void (*on_pin)(void *user_data, const char *pin);
    void (*on_video_size)(void *user_data, int width, int height);
    void (*on_error)(void *user_data, int code, const char *message);
    /* level: 0=error, 1=warning, 2=info, 3=debug */
    void (*on_log)(void *user_data, int level, const char *message);
} AirBridgeCallbacks;

AB_API const char *airbridge_version(void);

/* Configure the engine. Must be called before airbridge_start().
 * The config strings are copied, so the caller may free them afterwards. */
AB_API int airbridge_init(const AirBridgeConfig *config,
                          const AirBridgeCallbacks *callbacks);

/* Start the engine on a dedicated native thread. Returns 0 on success. */
AB_API int airbridge_start(void);

/* Set the HWND used by the GStreamer video sink. Call before start (or while
 * no video pipeline is active). */
AB_API void airbridge_set_window_handle(uintptr_t hwnd);

/* Set local playback volume in dB, range [-144, 0].  -144 = mute. */
AB_API void airbridge_set_volume(double db);

/* Ask the engine to stop and wait for its thread to finish. */
AB_API void airbridge_stop(void);

/* Stop the engine and release callbacks. */
AB_API void airbridge_shutdown(void);

/* ------------------------------------------------------------------------- */
/* Internal hooks (defined by airbridge_core.cpp, called from UxPlay)        */
/* ------------------------------------------------------------------------- */

void airbridge_emit_state(const char *state);
void airbridge_emit_client(const char *name, const char *model,
                           const char *device_id);
void airbridge_emit_metadata(const char *title, const char *artist,
                             const char *album);
void airbridge_emit_artwork(const unsigned char *data, int len);
void airbridge_emit_progress(double position, double duration);
void airbridge_emit_volume(double db, double linear);
void airbridge_emit_pin(const char *pin);
void airbridge_emit_video_size(int width, int height);
void airbridge_emit_error(int code, const char *message);
void airbridge_emit_log(int level, const char *message);

/* ------------------------------------------------------------------------- */
/* Internal hooks (defined in UxPlay's uxplay.cpp, called by the wrapper)    */
/* ------------------------------------------------------------------------- */

int  airbridge_engine_main(int argc, char **argv);
void airbridge_engine_request_stop(void);
void airbridge_engine_set_receiver_name(const char *utf8_name);
void airbridge_engine_set_volume(double db);

#ifdef __cplusplus
}
#endif

#endif /* AIRBRIDGE_CORE_H */
