<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Login

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## Project Overview

**Login Component** - The core package of the GameFrameX login channel registry. It provides the channel-agnostic login contract (`ILoginManager`), the unified result base class (`LoginResult`), `LoginChannelRegistry` (channel packages self-register via their Bootstrap at startup) and the single `LoginComponent` facade. Adding a new login channel no longer requires touching the entry package.

### Features

- Channel-agnostic login contract: `Init()` / `IsReady()` / `Login(LoginParams)` / `Logout()` plus aggregated `OnLoginSuccess` / `OnLoginFailed` events
- Unified result base class `LoginResult`: `ChannelId` / `OpenId` / `Token` (carries the channel credential: Google IdToken, Apple credential, WeChat/QQ code or accessToken) / `DisplayName` / `PhotoUrl` / `Raw`; channel subclasses only add channel-specific fields
- Explicit channel routing: `LoginParams.ChannelId` routes through the registry; empty resolves via mutual-exclusion (first registered channel wins), with the module slot occupied by the core-package `DefaultLoginManager` forwarder
- Channel registry: idempotent Register (first wins + warning), lazy factory instantiation with caching, `Reset` on `SubsystemRegistration`, mutual-exclusion resolution taking the first registered channel (channel platform exclusion guaranteed by the macro wall in each channel Bootstrap)
- Channel enumeration for UI: `GetAvailableChannels()` returns channel id / display name / availability
- One-shot callback convenience overload `Login(onSuccess, onFailed)` (subscribe-and-dispose)

## Quick Start

### Installation

Choose one of the following methods:

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

   `scopes` controls which packages are resolved through this registry. Only packages whose names start with `com.gameframex` will be fetched from it.

2. Add to `manifest.json` dependencies:
   ```json
   {
      "com.gameframex.unity.login": "https://github.com/gameframex/com.gameframex.unity.login.git"
   }
   ```
3. Use **Package Manager** in Unity with **Git URL**: `https://github.com/gameframex/com.gameframex.unity.login.git`
4. Clone the repository into your Unity project's `Packages` directory. It will be loaded automatically.

## Usage Examples

```csharp
using GameFrameX.Login.Runtime;
using UnityEngine;

public class LoginDemo : MonoBehaviour
{
    void Start()
    {
        // 1) GameApp shortcut (entry package) or GameEntry.GetComponent<LoginComponent>()
        //    var login = GameApp.Login;
        var login = GameEntry.GetComponent<LoginComponent>();

        // Subscribe to aggregated events (mutual-exclusion default channel and routed channels all fire here)
        login.OnLoginSuccess += (sender, e) =>
        {
            // Channel-agnostic: server verification should use e.Result.Token
            Debug.Log($"Login ok channel={e.Result.ChannelId} openId={e.Result.OpenId} token={e.Result.Token}");
        };
        login.OnLoginFailed += (sender, e) => Debug.LogWarning($"Login failed channel={e.ChannelId} error={e.ErrorMessage}");

        // 2) Initialize the default channel (channel config is read by the channel package itself)
        login.Init();

        // 3a) One-shot callback overload (subscribe-and-dispose)
        login.Login(
            result => Debug.Log($"Login ok token={result.Token}"),
            error => Debug.LogWarning($"Login failed: {error}"));

        // 3b) Or route explicitly: empty ChannelId -> mutual-exclusion default, "Google" -> registry routing
        login.Login(new LoginParams());                    // default channel (mutual-exclusion)
        login.Login(new LoginParams("Google"));            // explicit channel routing

        // 4) Enumerate available channels for the login UI
        foreach (var channel in login.GetAvailableChannels())
        {
            Debug.Log($"channel={channel.ChannelId} name={channel.DisplayName} available={channel.IsAvailable}");
        }
    }
}
```

## API Reference

### `Init()`

Initialize the login channel (forwarded to the manager resolved from the registry). Channel config is read by the channel package itself.

### `IsReady()`

Whether the login channel is ready (forwarded to the manager resolved from the registry).

### `Login(LoginParams loginParams)`

Perform a login. `ChannelId` empty resolves via registry mutual exclusion (registration order is the priority); non-empty routes through the registry (unregistered channels warn and no-op). Results fire the aggregated `OnLoginSuccess` / `OnLoginFailed` events.

### `Login(Action<LoginResult> onSuccess, Action<string> onFailed)`

One-shot callback convenience overload for the default channel (subscribe-and-dispose).

### `Logout()`

Log out (forwarded to the manager resolved from the registry).

### `GetAvailableChannels()`

List registered channels with availability (`ChannelId` / `DisplayName` / `IsAvailable`) for UI display.

### `IsChannelAvailable(string channelId)`

Whether a channel is registered and currently available.

### `LoginResult`

Channel-agnostic result base class: `ChannelId`, `OpenId`, `Token` (unified channel credential - server verification should rely on this field), `DisplayName`, `PhotoUrl`, `Raw`. Channel subclasses (e.g. `GoogleLoginResult`) add channel-specific fields only.

## Architecture

### Channel Registry Topology (v1.0.0)

- Core package `com.gameframex.unity.login` (this package): contract (`ILoginManager`, `LoginResult`, `LoginParams`, `LoginEventArgs`), registry (`LoginChannelRegistry`), and the single `LoginComponent` facade; the module slot is occupied by the core-package `DefaultLoginManager`, which forwards calls (incl. Shutdown) and result events to the registry-resolved channel.
- Channel packages (e.g. `com.gameframex.unity.login.google`): implement `BaseLoginManager`, self-register at `BeforeSceneLoad` via their own Bootstrap (macro wall limits supported platforms), and only depend on the core package. Uninstalled packages are stripped - registration simply does not happen.
- Adding a new login channel = a new channel package + Bootstrap self-registration. The entry package keeps a single `GameApp.Login` accessor forever.

### Channel Support

| Channel | Package | Status |
|---------|---------|--------|
| Google | `com.gameframex.unity.login.google` | Available (3.0.0) |
| Apple / FaceBook / QQ / WeChat | `com.gameframex.unity.login.*` | Planned (separate changes) |

### Migration (from per-provider components)

| Old | New |
|-----|-----|
| `GoogleLoginComponent` (scene component + Inspector) | `LoginComponent` (single facade; Google registers as the default channel on Android / iOS via the macro wall) |
| `GameApp.GoogleLogin` | `GameApp.Login` (define `ENABLE_GAME_FRAME_X_GOOGLE_LOGIN` -> `ENABLE_GAME_FRAME_X_LOGIN`) |
| `IGoogleLoginManager` | `ILoginManager` / `BaseLoginManager` |
| `GoogleLoginSuccess` (incl. `IdToken`) | `GoogleLoginResult : LoginResult` (IdToken carried by `Token`) |
| `loginSuccess` / `loginFail` callbacks | aggregated `OnLoginSuccess` / `OnLoginFailed` events or the one-shot callback overload |

## Platform Support

| Platform | Supported |
|----------|-----------|
| Android | Yes (channel packages) |
| iOS | Yes (channel packages) |
| Editor | Yes (stub sign-in via channel packages) |
| WebGL / PC / macOS | No registered channel: warn and no-op (`DefaultLoginManager` has no channel to resolve) |

## Documentation & Resources

- [Documentation](https://gameframex.doc.alianblank.com)
- [GitHub Repository](https://github.com/GameFrameX/com.gameframex.unity.login)

## Community & Support

- QQ Group: Join via [QR Code](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)

## Dependencies

| Package | Version |
|---------|---------|
| `com.gameframex.unity` | 2.5.4 |

## Changelog

See [Releases](https://github.com/gameframex/com.gameframex.unity.login/releases) for changelog.

## License

See [LICENSE.md](LICENSE.md) for license information.
