<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Login

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761

<br />

[English](../../README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 項目簡介

**登入組件** - GameFrameX 登入渠道註冊表核心包。提供渠道無關的登入契約（`ILoginManager`）、統一結果基類（`LoginResult`）、`LoginChannelRegistry`（渠道包啟動時經各自 Bootstrap 自註冊）與唯一的 `LoginComponent` 門面組件。新增登入渠道不再需要修改 entry 包。

### 功能特性

- 渠道無關登入契約：`Init()` / `IsReady()` / `Login(LoginParams)` / `Logout()` + 聚合的 `OnLoginSuccess` / `OnLoginFailed` 事件
- 統一結果基類 `LoginResult`：`ChannelId` / `OpenId` / `Token`（統一承載渠道令牌：Google IdToken、Apple credential、微信/QQ code 或 accessToken）/ `DisplayName` / `PhotoUrl` / `Raw`；渠道子類只擴展渠道特有欄位
- 顯式渠道路由：`LoginParams.ChannelId` 經註冊表路由；為空經註冊表互斥解析（先註冊渠道優先，註冊順序即優先級）；模組槽位由核心包 `DefaultLoginManager` 轉發器佔用
- 渠道註冊表：Register 冪等保首 + 告警、工廠懶實例化快取、`Reset` 掛 `SubsystemRegistration`、互斥解析取首個註冊渠道（渠道的平台排除由各渠道 Bootstrap 的宏牆保證）
- UI 渠道列舉：`GetAvailableChannels()` 返回渠道標識 / 展示名 / 可用性
- 一次性回呼便捷重載 `Login(onSuccess, onFailed)`（訂閱即棄）

## 快速開始

### 安裝

選擇以下任意一種安裝方式：

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

   `scopes` 控制哪些包會通過該註冊表解析。只有名稱以 `com.gameframex` 開頭的包才會從該註冊表獲取。

2. Add to `manifest.json` dependencies:
   ```json
   {
      "com.gameframex.unity.login": "https://github.com/gameframex/com.gameframex.unity.login.git"
   }
   ```
3. Use **Package Manager** in Unity with **Git URL**: `https://github.com/gameframex/com.gameframex.unity.login.git`
4. Clone the repository into your Unity project's `Packages` directory. It will be loaded automatically.

## 使用範例

```csharp
using GameFrameX.Login.Runtime;
using UnityEngine;

public class LoginDemo : MonoBehaviour
{
    void Start()
    {
        // 1) GameApp 快捷存取（entry 包）或 GameEntry.GetComponent<LoginComponent>() 兩種寫法等價
        //    var login = GameApp.Login;
        var login = GameEntry.GetComponent<LoginComponent>();

        // 訂閱聚合事件（互斥解析預設渠道與路由渠道的結果都會觸發）
        login.OnLoginSuccess += (sender, e) =>
        {
            // 渠道無關：伺服器校驗方統一使用 e.Result.Token
            Debug.Log($"登入成功 channel={e.Result.ChannelId} openId={e.Result.OpenId} token={e.Result.Token}");
        };
        login.OnLoginFailed += (sender, e) => Debug.LogWarning($"登入失敗 channel={e.ChannelId} error={e.ErrorMessage}");

        // 2) 初始化預設渠道（互斥解析；渠道配置由渠道包自讀）
        login.Init();

        // 3a) 一次性回呼重載（訂閱即棄）
        login.Login(
            result => Debug.Log($"登入成功 token={result.Token}"),
            error => Debug.LogWarning($"登入失敗: {error}"));

        // 3b) 或顯式路由：ChannelId 為空走互斥解析預設，"Google" 走註冊表路由
        login.Login(new LoginParams());                    // 預設渠道（互斥解析）
        login.Login(new LoginParams("Google"));            // 顯式渠道路由

        // 4) 列舉可用渠道（登入 UI 並列展示）
        foreach (var channel in login.GetAvailableChannels())
        {
            Debug.Log($"channel={channel.ChannelId} name={channel.DisplayName} available={channel.IsAvailable}");
        }
    }
}
```

## API 參考

### `Init()`

初始化登入渠道（轉發給註冊表解析出的管理器）。渠道配置由渠道包自行讀取。

### `IsReady()`

登入渠道是否準備好（轉發給註冊表解析出的管理器）。

### `Login(LoginParams loginParams)`

執行登入。`ChannelId` 為空經註冊表互斥解析（註冊順序即優先級）；非空經 `LoginChannelRegistry` 路由（未註冊渠道告警 no-op）。結果觸發聚合的 `OnLoginSuccess` / `OnLoginFailed` 事件。

### `Login(Action<LoginResult> onSuccess, Action<string> onFailed)`

預設渠道的一次性回呼便捷重載（訂閱即棄）。

### `Logout()`

退出登入（轉發給註冊表解析出的管理器）。

### `GetAvailableChannels()`

返回已註冊渠道列表（`ChannelId` / `DisplayName` / `IsAvailable`），供 UI 展示。

### `IsChannelAvailable(string channelId)`

渠道是否已註冊且當前可用。

### `LoginResult`

渠道無關結果基類：`ChannelId`、`OpenId`、`Token`（統一承載渠道令牌——伺服器校驗方以此欄位為準）、`DisplayName`、`PhotoUrl`、`Raw`。渠道子類（如 `GoogleLoginResult`）只擴展渠道特有欄位。

## 架構概覽

### 渠道註冊表拓撲（v1.0.0）

- 核心包 `com.gameframex.unity.login`（本包）：契約（`ILoginManager` / `LoginResult` / `LoginParams` / `LoginEventArgs`）+ 註冊表（`LoginChannelRegistry`）+ 唯一 `LoginComponent` 門面；模組槽位由核心包 `DefaultLoginManager` 佔用，將呼叫（含 Shutdown）與結果事件轉發給註冊表解析出的渠道。
- 渠道包（如 `com.gameframex.unity.login.google`）：實現 `BaseLoginManager`，經各自 Bootstrap 在 `BeforeSceneLoad` 自註冊（宏牆限定支援平台），package.json 只依賴核心包。未安裝的渠道包代碼被裁剪——註冊不會發生。
- 新增登入渠道 = 新渠道包 + Bootstrap 自註冊；entry 包永久只保留單一 `GameApp.Login` 存取面。

### 渠道支援表

| 渠道 | 包 | 狀態 |
|------|-----|------|
| Google | `com.gameframex.unity.login.google` | 已支援（3.0.0） |
| Apple / FaceBook / QQ / WeChat | `com.gameframex.unity.login.*` | 規劃中（各自後續 change） |

### 遷移說明（自 per-provider 組件形態）

| 舊形態 | 新形態 |
|--------|--------|
| `GoogleLoginComponent`（場景組件 + Inspector） | `LoginComponent`（唯一門面；Google 經宏牆在 Android / iOS 註冊為預設渠道） |
| `GameApp.GoogleLogin` | `GameApp.Login`（宏 `ENABLE_GAME_FRAME_X_GOOGLE_LOGIN` → `ENABLE_GAME_FRAME_X_LOGIN`） |
| `IGoogleLoginManager` | `ILoginManager` / `BaseLoginManager` |
| `GoogleLoginSuccess`（含 `IdToken`） | `GoogleLoginResult : LoginResult`（IdToken 收斂到 `Token`） |
| `loginSuccess` / `loginFail` 回呼 | 聚合 `OnLoginSuccess` / `OnLoginFailed` 事件或一次性回呼重載 |

## 平台支援

| 平台 | 支援 |
|------|------|
| Android | 支援（渠道包） |
| iOS | 支援（渠道包） |
| Editor | 支援（渠道包樁資料） |
| WebGL / PC / macOS | 無已註冊渠道：告警 no-op（`DefaultLoginManager` 無渠道可解析） |

## 文檔與資源

- [文檔](https://gameframex.doc.alianblank.com)
- [GitHub 倉庫](https://github.com/GameFrameX/com.gameframex.unity.login)

## 社區與支援

- QQ 群：透過[二維碼](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)加入

## 依賴

| 包 | 版本 |
|-----|------|
| `com.gameframex.unity` | 2.5.4 |

## 更新日誌

詳見 [Releases](https://github.com/gameframex/com.gameframex.unity.login/releases) 更新日誌。

## 開源協議

詳見 [LICENSE.md](LICENSE.md) 檔案。
