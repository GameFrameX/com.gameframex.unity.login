<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Login

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援

<br />

[ドキュメント](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · QQグループ: 467608841 / 233840761

<br />

[English](../../README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>

## プロジェクト概要

**ログインコンポーネント** - GameFrameX ログインチャネルレジストリの中核パッケージ。チャネル非依存のログイン契約（`ILoginManager`）、統一結果基底クラス（`LoginResult`）、`LoginChannelRegistry`（チャネルパッケージが起動時に各自の Bootstrap で自己登録）、唯一の `LoginComponent` ファサードを提供します。新しいログインチャネルの追加で entry パッケージを変更する必要はもうありません。

### 機能概要

- チャネル非依存のログイン契約：`Init()` / `IsReady()` / `Login(LoginParams)` / `Logout()` + 集約された `OnLoginSuccess` / `OnLoginFailed` イベント
- 統一結果基底クラス `LoginResult`：`ChannelId` / `OpenId` / `Token`（チャネル資格情報を統一的に保持：Google IdToken、Apple credential、WeChat/QQ code または accessToken）/ `DisplayName` / `PhotoUrl` / `Raw`。チャネルサブクラスはチャネル固有のフィールドのみ追加
- 明示的チャネルルーティング：`LoginParams.ChannelId` でレジストリ経由のルーティング。空の場合はレジストリの排他解決（先着登録チャネル優先、登録順が優先度）。モジュールスロットは中核パッケージの `DefaultLoginManager` フォワーダーが占めます
- チャネルレジストリ：Register 冪等（先着優先 + 警告）、ファクトリの遅延インスタンス化とキャッシュ、`SubsystemRegistration` での `Reset`、先着登録チャネルを取る排他解決（チャネルのプラットフォーム除外は各チャネル Bootstrap のマクロ壁で保証）
- UI 向けチャネル列挙：`GetAvailableChannels()` がチャネル ID / 表示名 / 可用性を返す
- 一回限りのコールバック簡易オーバーロード `Login(onSuccess, onFailed)`（購読即破棄）

## クイックスタート

### インストール

以下のいずれかの方法を選択してください：

1. Edit your Unity project's `Packages/manifest.json` and add the `scopedRegistries` section:
   ```json
   {
     "scopedRegistries": [
       {
         "name": "GameFrameX",
         "url": "https://gameframex.upm.alianblank.uk",
         "scopes": [
           "com.gameframex"
         ]
       }
     ],
     "dependencies": {
       "com.gameframex.unity.login": "1.0.0"
     }
   }
   ```

   `scopes` はこのレジストリから解決されるパッケージを制御します。`com.gameframex` で始まる名前のパッケージのみがこのレジストリから取得されます。

2. Add to `manifest.json` dependencies:
   ```json
   {
      "com.gameframex.unity.login": "https://github.com/gameframex/com.gameframex.unity.login.git"
   }
   ```
3. Use **Package Manager** in Unity with **Git URL**: `https://github.com/gameframex/com.gameframex.unity.login.git`
4. Clone the repository into your Unity project's `Packages` directory. It will be loaded automatically.

## 使用例

```csharp
using GameFrameX.Login.Runtime;
using UnityEngine;

public class LoginDemo : MonoBehaviour
{
    void Start()
    {
        // 1) GameApp ショートカット（entry パッケージ）または GameEntry.GetComponent<LoginComponent>()
        //    var login = GameApp.Login;
        var login = GameEntry.GetComponent<LoginComponent>();

        // 集約イベントの購読（排他解決の既定チャネルとルーティングチャネルの結果がどちらも発火）
        login.OnLoginSuccess += (sender, e) =>
        {
            // チャネル非依存：サーバー検証は e.Result.Token を使用
            Debug.Log($"ログイン成功 channel={e.Result.ChannelId} openId={e.Result.OpenId} token={e.Result.Token}");
        };
        login.OnLoginFailed += (sender, e) => Debug.LogWarning($"ログイン失敗 channel={e.ChannelId} error={e.ErrorMessage}");

        // 2) 既定チャネルの初期化（排他解決。チャネル設定はチャネルパッケージが自読）
        login.Init();

        // 3a) 一回限りのコールバックオーバーロード（購読即破棄）
        login.Login(
            result => Debug.Log($"ログイン成功 token={result.Token}"),
            error => Debug.LogWarning($"ログイン失敗: {error}"));

        // 3b) 明示的ルーティング：ChannelId 空なら排他解決の既定、"Google" ならレジストリルーティング
        login.Login(new LoginParams());                    // 既定チャネル（排他解決）
        login.Login(new LoginParams("Google"));            // 明示的チャネルルーティング

        // 4) 利用可能なチャネルの列挙（ログイン UI での並列表示）
        foreach (var channel in login.GetAvailableChannels())
        {
            Debug.Log($"channel={channel.ChannelId} name={channel.DisplayName} available={channel.IsAvailable}");
        }
    }
}
```

## API リファレンス

### `Init()`

ログインチャネルを初期化します（レジストリから解決されたマネージャーへ転送）。チャネル設定はチャネルパッケージが自読します。

### `IsReady()`

ログインチャネルが準備済みかどうか（レジストリから解決されたマネージャーへ転送）。

### `Login(LoginParams loginParams)`

ログインを実行します。`ChannelId` が空ならレジストリの排他解決（登録順が優先度）。非空なら `LoginChannelRegistry` 経由でルーティング（未登録チャネルは警告して no-op）。結果は集約された `OnLoginSuccess` / `OnLoginFailed` イベントで発火します。

### `Login(Action<LoginResult> onSuccess, Action<string> onFailed)`

既定チャネル向けの一回限りコールバック簡易オーバーロード（購読即破棄）。

### `Logout()`

ログアウトします（レジストリから解決されたマネージャーへ転送）。

### `GetAvailableChannels()`

登録済みチャネルの一覧（`ChannelId` / `DisplayName` / `IsAvailable`）を UI 表示用に返します。

### `IsChannelAvailable(string channelId)`

チャネルが登録済みかつ現在利用可能かどうか。

### `LoginResult`

チャネル非依存の結果基底クラス：`ChannelId`、`OpenId`、`Token`（チャネル資格情報を統一的に保持。サーバー検証はこのフィールド基準）、`DisplayName`、`PhotoUrl`、`Raw`。チャネルサブクラス（例：`GoogleLoginResult`）はチャネル固有フィールドのみ追加します。

## アーキテクチャ

### チャネルレジストリトポロジ（v1.0.0）

- 中核パッケージ `com.gameframex.unity.login`（本パッケージ）：契約（`ILoginManager` / `LoginResult` / `LoginParams` / `LoginEventArgs`）+ レジストリ（`LoginChannelRegistry`）+ 唯一の `LoginComponent` ファサード。モジュールスロットは中核パッケージの `DefaultLoginManager` が占め、呼び出し（Shutdown 含む）と結果イベントをレジストリ解決されたチャネルへ転送します。
- チャネルパッケージ（例：`com.gameframex.unity.login.google`）：`BaseLoginManager` を実装し、各自の Bootstrap で `BeforeSceneLoad` に自己登録（マクロ壁が対応プラットフォームを限定）。package.json は中核パッケージのみに依存。未導入のチャネルパッケージのコードはストリップされ、登録は発生しません。
- 新しいログインチャネルの追加 = 新しいチャネルパッケージ + Bootstrap 自己登録。entry パッケージは永久に単一の `GameApp.Login` アクセサのみ保持します。

### チャネル対応表

| チャネル | パッケージ | 状態 |
|----------|------------|------|
| Google | `com.gameframex.unity.login.google` | 対応済み（3.0.0） |
| Apple / FaceBook / QQ / WeChat | `com.gameframex.unity.login.*` | 計画中（個別の後続 change） |

### 移行ガイド（per-provider コンポーネント形態から）

| 旧 | 新 |
|----|----|
| `GoogleLoginComponent`（シーンコンポーネント + Inspector） | `LoginComponent`（唯一のファサード。Google はマクロ壁経由で Android / iOS の既定チャネルとして登録） |
| `GameApp.GoogleLogin` | `GameApp.Login`（定義 `ENABLE_GAME_FRAME_X_GOOGLE_LOGIN` -> `ENABLE_GAME_FRAME_X_LOGIN`） |
| `IGoogleLoginManager` | `ILoginManager` / `BaseLoginManager` |
| `GoogleLoginSuccess`（`IdToken` 含む） | `GoogleLoginResult : LoginResult`（IdToken は `Token` に統合） |
| `loginSuccess` / `loginFail` コールバック | 集約された `OnLoginSuccess` / `OnLoginFailed` イベントまたは一回限りコールバックオーバーロード |

## プラットフォーム対応

| プラットフォーム | 対応 |
|----------|-----------|
| Android | 対応（チャネルパッケージ） |
| iOS | 対応（チャネルパッケージ） |
| Editor | 対応（チャネルパッケージのスタブ） |
| WebGL / PC / macOS | 登録チャネルなし：警告して no-op（`DefaultLoginManager` は解決できるチャネルがない） |

## ドキュメントとリソース

- [ドキュメント](https://gameframex.doc.alianblank.com)
- [GitHub リポジトリ](https://github.com/GameFrameX/com.gameframex.unity.login)

## コミュニティとサポート

- QQ グループ：[QR コード](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)から参加

## 依存関係

| パッケージ | バージョン |
|---------|---------|
| `com.gameframex.unity` | 2.5.4 |

## 変更履歴

変更履歴は [Releases](https://github.com/gameframex/com.gameframex.unity.login/releases) を参照してください。

## ライセンス

詳しくは [LICENSE.md](LICENSE.md) をご参照ください。
