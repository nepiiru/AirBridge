# AirBridge

![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![Windows](https://img.shields.io/badge/Windows-10%20%2F%2011-0078D4)
![License](https://img.shields.io/badge/license-GPL--3.0--or--later-blue)

Windows 上で AirPlay レシーバーとして動作するデスクトップアプリです。iPhone / iPad / Mac から音声、画面ミラーリング、YouTube アプリの HLS 動画を受け取ります。

UI は .NET 10 / WPF、受信エンジンは [UxPlay](https://github.com/FDH2/UxPlay) をプロセス内のネイティブ DLL として組み込んでいます。`uxplay.exe` を別プロセスで起動する方式ではありません。

> [!IMPORTANT]
> v0.1.0 は開発中です。iPhone の音声再生で動作を確認しています。画面ミラーリング、YouTube アプリの HLS、Mac / iTunes は実装済みですが、すべての環境での動作を保証するものではありません。
>
> DRM 保護された映像コンテンツ、ブラウザ版 YouTube の AirPlay 映像、AirPlay 2 のマルチルーム音声には対応していません。Apple 非公式のプロジェクトです。

## 主な機能

| 機能 | 内容 |
| --- | --- |
| 音声 | iPhone / iPad / Mac の AirPlay 音声を受信 |
| 画面ミラーリング | AirPlay の画面ミラーリングをウィンドウ内に表示 |
| YouTube アプリ HLS | YouTube アプリから送られる HLS 動画を再生 |
| フルスクリーン | `F11` で切り替え、`Esc` で終了。全画面時は上部に 40px のバーを表示 |
| テーマ | モダン、空、夜空、夕焼け、フォレスト、モノクロ、桜の 7 種類 |
| レシーバー名 | 実行中でも変更可能。「PC名を付ける」で `名前@PC名` / 名前のみを切り替え |
| アクセス制御 | PIN なし、接続ごとのランダム PIN、固定パスワード（4 文字以上） |
| 設定保存 | 名前、テーマ、ポート、音量などを `%LOCALAPPDATA%\AirBridge\settings.json` に保存 |
| 音声出力 | WASAPI の出力デバイスを選択可能 |
| 自動起動 | アプリ起動時にレシーバーを自動で開始 |

### テーマ

- `modern` — モダン
- `sky` — 空
- `night` — 夜空
- `sunset` — 夕焼け
- `forest` — フォレスト
- `mono` — モノクロ
- `sakura` — 桜

テーマは WPF の `DynamicResource` を実行時に差し替える方式で、選択内容は設定に保存されます。

## 動作環境

- Windows 10 22H2 以降 / Windows 11（x64）
- [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)（ビルドには .NET 10 SDK）
- GStreamer 1.x（MSYS2 UCRT64 の 1.28 系で確認）
  - `gst-plugins-base`
  - `gst-plugins-good`
  - `gst-plugins-bad`
  - `gst-libav`
- iPhone / iPad / Mac と PC が同じ LAN に接続されていること
- Windows のネットワークプロファイルが「プライベート」であること

現時点では x64 のみに対応しています。インストーラーはありません。

## 使い方

1. `AirBridge.exe` を起動します。
2. 必要に応じてレシーバー名、テーマ、ポート、音声出力、PIN などを設定します。
3. 「開始」を押すと、AirPlay の受信を開始します。
4. iPhone / iPad のコントロールセンター、または Mac の AirPlay メニューから AirBridge を選択します。
5. 画面ミラーリングの場合は「画面ミラーリング」、音声の場合は出力先として AirBridge を選択します。
6. YouTube アプリの動画は、動画の AirPlay ボタンから AirBridge を選択すると HLS として再生されます。
7. 全画面表示は `F11`、終了は `Esc` です。

### レシーバー名と PC 名

初期状態では `AirBridge@PC名` のように、Windows のコンピューター名が付加されます。「PC名を付ける（名前@PC名）」をオフにすると、AirPlay のメニューには指定した名前だけが表示されます。

実行中に名前やこのオプションを変更した場合は「再起動して適用」を押してください。接続中のクライアントや PIN 表示がある場合は、確認後にエンジンを再起動します。

### アクセス制御

- **PIN なし** — 同一ネットワーク上のクライアントから自由に接続できます。
- **ランダム PIN** — 接続時に表示される 4 桁の PIN をクライアントに入力します。
- **パスワード** — 4 文字以上の固定パスワードを使用します。パスワードは設定ファイルに保存されません。

## 設定とログ

主な設定項目:

- レシーバー名 / PC 名の付加
- テーマ
- ポート（既定 7000）
- 音声出力デバイス
- HLS の有効 / 無効
- PIN モード / パスワード
- 切断後のウィンドウ保持
- 自動起動
- 音量

| 内容 | 場所 |
| --- | --- |
| 設定 | `%LOCALAPPDATA%\AirBridge\settings.json` |
| ネイティブログ | `%LOCALAPPDATA%\AirBridge\airbridge.log` |
| GStreamer レジストリ | `%LOCALAPPDATA%\AirBridge\gst-registry.bin` |

パスワードは保存されないため、アプリを再起動した場合は再入力が必要です。

### GStreamer の場所

アプリは次の順序で GStreamer のルートを探します。

1. `AirBridge.exe` と同じディレクトリにある `gst` フォルダ
2. 環境変数 `AIRBRIDGE_GSTREAMER_ROOT`
3. 開発環境用の既知のパス

MSYS2 を標準以外の場所にインストールしている場合は、実行前に環境変数を設定してください。

```powershell
$env:AIRBRIDGE_GSTREAMER_ROOT = "C:\msys64\ucrt64"
.\src\AirBridge.App\bin\Debug\net10.0-windows\AirBridge.exe
```

配布用にまとめる場合は、MSYS2 UCRT64 のランタイムを `gst` フォルダとして `AirBridge.exe` の隣に配置できます。

```text
AirBridge.exe
airbridge_core.dll
gst\
  bin\
  lib\gstreamer-1.0\
  libexec\gstreamer-1.0\
```

## ビルド

### 必要なもの

- Git
- .NET 10 SDK
- MSYS2（UCRT64 環境）
- CMake / Ninja / GCC
- GStreamer の開発パッケージ

MSYS2 の UCRT64 ターミナルで、必要なパッケージをインストールします。

```bash
pacman -S --needed \
  mingw-w64-ucrt-x86_64-gcc \
  mingw-w64-ucrt-x86_64-cmake \
  mingw-w64-ucrt-x86_64-ninja \
  mingw-w64-ucrt-x86_64-pkgconf \
  mingw-w64-ucrt-x86_64-libplist \
  mingw-w64-ucrt-x86_64-openssl \
  mingw-w64-ucrt-x86_64-gstreamer \
  mingw-w64-ucrt-x86_64-gst-plugins-base \
  mingw-w64-ucrt-x86_64-gst-plugins-good \
  mingw-w64-ucrt-x86_64-gst-plugins-bad \
  mingw-w64-ucrt-x86_64-gst-libav
```

UxPlay は内部に mDNS 実装を持っているため、Bonjour SDK は不要です。

### 1. リポジトリを取得する

サブモジュールを含めて clone します。

```powershell
git clone --recurse-submodules https://github.com/nepiiru/AirBridge.git
cd AirBridge
```

すでに clone 済みの場合は、サブモジュールを更新します。

```powershell
git submodule update --init --recursive
```

`native/UxPlay` は `nepiiru/UxPlay` の `airbridge` ブランチを参照しています。

### 2. ネイティブエンジンをビルドする

MSYS2 UCRT64 ターミナル、または UCRT64 の `bin` に PATH を通した PowerShell で実行します。

```powershell
$env:PATH = "C:\msys64\ucrt64\bin;C:\msys64\usr\bin;$env:PATH"

cmake -S native/UxPlay -B native/UxPlay/build `
  -G Ninja `
  -DCMAKE_BUILD_TYPE=Release `
  -DAIRBRIDGE_CORE=ON

cmake --build native/UxPlay/build --target airbridge_core
```

成功すると `native/UxPlay/build/airbridge_core.dll` が生成されます。

### 3. .NET アプリをビルドする

```powershell
dotnet build AirBridge.slnx -c Debug
dotnet test AirBridge.slnx -c Debug --no-build
```

`AirBridge.App.csproj` はビルド後に `airbridge_core.dll` を出力ディレクトリへコピーします。実行ファイル名は、既存の Windows ファイアウォール規則を維持するため `AirBridge.exe` です。

### 4. 実行する

```powershell
$env:AIRBRIDGE_GSTREAMER_ROOT = "C:\msys64\ucrt64"
.\src\AirBridge.App\bin\Debug\net10.0-windows\AirBridge.exe
```

アプリを再ビルドする前に、起動中の `AirBridge.exe` を終了してください。実行中の場合は出力ファイルがロックされます。

## アーキテクチャ

```text
WPF UI (AirBridge.App)
        │
        ▼
AirBridgeEngine (C# / P/Invoke)
        │
        ▼
airbridge_core.dll (C ABI ラッパー)
        │
        ▼
UxPlay (プロセス内)
        │
        ├── mDNS / AirPlay プロトコル
        ├── GStreamer（音声・映像）
        └── HWND 埋め込み映像ホスト
```

- `src/AirBridge.App` — WPF UI、テーマ、フルスクリーン、設定、P/Invoke
- `src/AirBridge.Core` — 設定・再生状態などのモデル
- `src/AirBridge.Core.Tests` — Core のテスト
- `native/airbridge_core` — UxPlay を C ABI で包むラッパー
- `native/UxPlay` — AirBridge 用の UxPlay フォーク（サブモジュール）

映像は `AirBridgeVideoHost` というネイティブ子ウィンドウに埋め込まれます。待機中は黒い背景を表示し、`WS_EX_NOACTIVATE` によって映像クリック時に WPF 側のキーボードフォーカスを奪わないようにしています。

## トラブルシューティング

### AirPlay の一覧に AirBridge が表示されない

- PC とクライアントが同じ Wi-Fi / LAN に接続されているか確認してください。
- Windows のネットワークプロファイルを「プライベート」にしてください。
- Windows Defender ファイアウォールで `AirBridge.exe` の受信を許可してください。
- VPN、ゲストネットワーク、クライアント分離（AP isolation）が有効になっていないか確認してください。
- mDNS（UDP 5353）がブロックされていないか確認してください。
- アプリの「開始」が押されているか、ステータスが待機中かを確認してください。

### 音声が出ない

- 設定の音声出力デバイスを確認してください。
- アプリ内の音量と、クライアント側の音量を確認してください。
- GStreamer の `gst-libav` / `gst-plugins-good` が入っているか確認してください。
- `%LOCALAPPDATA%\AirBridge\airbridge.log` にエラーがないか確認してください。

### 映像が表示されない・黒いまま

- `d3d11videosink` が利用できるか確認してください（`gst-plugins-bad`）。
- GPU ドライバーを更新してください。
- フルスクリーン時は `Esc` で戻り、通常ウィンドウで確認してください。
- v0.1 では映像の上に WPF のオーバーレイを重ねられません。

### YouTube アプリの HLS が再生されない

- YouTube アプリからの AirPlay に対応しています。ブラウザ版 YouTube の AirPlay 映像は対象外です。
- ネットワークが不安定な場合、`master.m3u8` の取得に失敗することがあります。アプリを再起動して再試行してください。
- DRM 保護された動画は再生できません。

### ビルドで `airbridge_core.dll` が見つからない

- 先に「2. ネイティブエンジンをビルドする」を実行してください。
- `native/UxPlay/build/airbridge_core.dll` が存在するか確認してください。
- アプリが起動中のまま `dotnet build` していないか確認してください。

## 既知の制限

- **DRM 保護された映像コンテンツ非対応** — 映画や配信サービスの一部など、DRM 保護された映像は再生できません。音声のみの DRM コンテンツも、クライアントやサービスによっては再生できない場合があります。
- **ブラウザ版 YouTube の AirPlay 映像非対応** — YouTube アプリの HLS と画面ミラーリングが対象です。
- **AirPlay 2 のマルチルーム音声非対応** — 複数ルームへの同時音声配信はできません。
- **映像上のオーバーレイ非対応** — v0.1 では映像の上に WPF の UI を重ねられません。
- **Mac / iTunes は未検証** — 実装は共通ですが、すべての組み合わせでの動作確認は完了していません。
- **HLS の再試行** — ネットワーク環境によっては HLS の取得に失敗する場合があります。
- **x64 のみ** — ARM64 版は未提供です。
- **インストーラーなし** — 現時点ではソースからビルドして実行します。

UxPlay は FairPlay 関連の実装を含みます。本プロジェクトは DRM の回避を目的としたものではありません。配布・利用にあたっては、各自の責任と適用法に従ってください。

## ライセンス

AirBridge は **GNU General Public License v3.0 or later** で公開されています。全文は [LICENSE](LICENSE) を参照してください。

```text
SPDX-License-Identifier: GPL-3.0-or-later
```

このプロジェクトは UxPlay（GPLv3）を組み込んでいます。UxPlay のライセンスは [native/UxPlay/LICENSE](https://github.com/nepiiru/UxPlay/blob/airbridge/LICENSE) を参照してください。

主な依存ライブラリ:

- [UxPlay](https://github.com/FDH2/UxPlay) — GPLv3
- [GStreamer](https://gstreamer.freedesktop.org/) — LGPL-2.1-or-later（プラグインは個別ライセンス）
- [NAudio](https://github.com/naudio/NAudio) — MIT
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) — MIT
- [.NET](https://dotnet.microsoft.com/) / WPF — MIT

それぞれのライセンス条件に従ってください。

## 免責事項

- AirBridge は Apple Inc. および Google / YouTube とは関係ありません。
- AirPlay は Apple Inc. の商標です。
- 本ソフトウェアは現状のまま提供され、明示・黙示を問わずいかなる保証も行いません。
- 利用するコンテンツについては、各自が必要な権利を有していることを確認してください。

## クレジット

- [UxPlay](https://github.com/FDH2/UxPlay) とコントリビューターの皆さん
- [GStreamer](https://gstreamer.freedesktop.org/) プロジェクト
- NAudio、CommunityToolkit.Mvvm、.NET / WPF のコントリビューターの皆さん

## リンク

- リポジトリ: <https://github.com/nepiiru/AirBridge>
- UxPlay フォーク: <https://github.com/nepiiru/UxPlay>
- Issue: <https://github.com/nepiiru/AirBridge/issues>
