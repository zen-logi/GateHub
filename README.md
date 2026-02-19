# GateHub

家庭内LAN環境における複数マシン間のPCSX2セーブデータ同期システム。

PCSX2のメモリーカードデータ（Folder Mcd形式）を1台の中央サーバーで一元管理し、ゲーム起動前後にオンデマンドで差分同期を行います。バックグラウンド常駐プロセスは一切不要です。

## 特徴

- **ゼロ・オーバーヘッド** — ゲームプレイ中のリソース消費ゼロ。起動時と終了時のみ同期
- **差分同期** — SHA256ハッシュによるマニフェスト比較で変更ファイルのみ転送
- **アトミック更新** — 一時ファイル経由の書き込みで中断耐性を確保
- **競合保護** — 意図しない上書きを検知しコンフリクトファイルとして退避
- **オフラインフォールバック** — サーバー不達時はローカルデータでそのままプレイ可能
- **クロスプラットフォーム** — Windows 11 / macOS (Apple Silicon) 対応
- **初回セットアップウィザード** — 設定ファイルの手動編集不要

## アーキテクチャ

```
┌─────────────┐         ┌─────────────┐
│  Client A   │◄──LAN──►│   Server    │
│ (Windows)   │         │  (Hub)      │
└─────────────┘         └──────┬──────┘
                               │
┌─────────────┐                │
│  Client B   │◄──LAN──────────┘
│  (macOS)    │
└─────────────┘
```

- **Server**: ASP.NET Core Web API — 正データの保持・マニフェスト提供・ファイル送受信
- **Client**: コンソールアプリ（PCSX2ラッパー） — ゲーム選択 → Pull → PCSX2起動 → Push

## 必要要件

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)（ビルド時のみ。公開バイナリは自己完結型）
- [PCSX2](https://pcsx2.net/)（Folder Mcd形式で運用）

## クイックスタート

### 1. ビルド

```bash
git clone https://github.com/zen-logi/GateHub.git
cd GateHub
dotnet build --configuration Release
```

### 2. 公開（自己完結型シングルファイル）

```bash
# Windows
dotnet publish src/GateHub.Server -c Release -r win-x64
dotnet publish src/GateHub.Client -c Release -r win-x64

# macOS (Apple Silicon)
dotnet publish src/GateHub.Server -c Release -r osx-arm64
dotnet publish src/GateHub.Client -c Release -r osx-arm64
```

### 3. サーバー起動

```bash
# サーバーマシンで実行
./GateHub.Server
```

サーバーは `http://0.0.0.0:5123` でリッスンします。

`appsettings.json` で以下を設定してください：
- `GateHub.StoragePath` — セーブデータの保存先ディレクトリ
- `GateHub.ApiTokens` — 許可するAPIトークンのリスト

### 4. クライアント起動

```bash
# クライアントマシンで実行
./GateHub.Client
```

初回起動時にセットアップウィザードが起動し、以下の設定を対話的に入力できます：
- サーバーURL
- APIトークン
- PCSX2実行ファイルのパス
- メモリーカードディレクトリのパス
- ゲーム一覧

設定は以下に保存されます：
- **Windows**: `%APPDATA%\GateHub\settings.json`
- **macOS**: `~/.config/gatehub/settings.json`

## API

| メソッド | エンドポイント | 説明 |
|---------|---------------|------|
| GET | `/api/sync/manifest` | ファイルマニフェスト取得 |
| GET | `/api/sync/files/{path}` | ファイルダウンロード |
| POST | `/api/sync/files/{path}` | ファイルアップロード |
| DELETE | `/api/sync/files/{path}` | ファイル削除 |

すべてのリクエストに `X-Api-Token` ヘッダーが必要です。

## ライセンス

[MIT License](LICENSE)
