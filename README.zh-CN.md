<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Login

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

<br />

[English](../../README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 项目简介

**登录组件** - GameFrameX 登录渠道注册表核心包。提供渠道无关的登录契约（`ILoginManager`）、统一结果基类（`LoginResult`）、`LoginChannelRegistry`（渠道包启动时经各自 Bootstrap 自注册）与唯一的 `LoginComponent` 门面组件。新增登录渠道不再需要修改 entry 包。

### 功能特性

- 渠道无关登录契约：`Init()` / `IsReady()` / `Login(LoginParams)` / `Logout()` + 聚合的 `OnLoginSuccess` / `OnLoginFailed` 事件
- 统一结果基类 `LoginResult`：`ChannelId` / `OpenId` / `Token`（统一承载渠道令牌：Google IdToken、Apple credential、微信/QQ code 或 accessToken）/ `DisplayName` / `PhotoUrl` / `Raw`；渠道子类只扩展渠道特有字段
- 显式渠道路由：`LoginParams.ChannelId` 经注册表路由；为空回落平台默认管理器（序列化类型名字符串；缺失回落 `DefaultLoginManager`）
- 渠道注册表：Register 幂等保首 + 告警、工厂懒实例化缓存、`Reset` 挂 `SubsystemRegistration`、路由结果与默认同类型时复用默认实例（防双实例）
- UI 渠道枚举：`GetAvailableChannels()` 返回渠道标识 / 展示名 / 可用性
- 一次性回调便捷重载 `Login(onSuccess, onFailed)`（订阅即弃）

## 快速开始

### 安装

选择以下任意一种安装方式：

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

   `scopes` 控制哪些包会通过该注册表解析。只有名称以 `com.gameframex` 开头的包才会从该注册表获取。

2. Add to `manifest.json` dependencies:
   ```json
   {
      "com.gameframex.unity.login": "https://github.com/gameframex/com.gameframex.unity.login.git"
   }
   ```
3. Use **Package Manager** in Unity with **Git URL**: `https://github.com/gameframex/com.gameframex.unity.login.git`
4. Clone the repository into your Unity project's `Packages` directory. It will be loaded automatically.

## 使用示例

```csharp
using GameFrameX.Login.Runtime;
using UnityEngine;

public class LoginDemo : MonoBehaviour
{
    void Start()
    {
        // 1) GameApp 快捷访问（entry 包）或 GameEntry.GetComponent<LoginComponent>() 两种写法等价
        //    var login = GameApp.Login;
        var login = GameEntry.GetComponent<LoginComponent>();

        // 订阅聚合事件（平台默认渠道与路由渠道的结果都会触发）
        login.OnLoginSuccess += (sender, e) =>
        {
            // 渠道无关：服务器校验方统一使用 e.Result.Token
            Debug.Log($"登录成功 channel={e.Result.ChannelId} openId={e.Result.OpenId} token={e.Result.Token}");
        };
        login.OnLoginFailed += (sender, e) => Debug.LogWarning($"登录失败 channel={e.ChannelId} error={e.ErrorMessage}");

        // 2) 初始化平台默认渠道（渠道配置由渠道包自读）
        login.Init();

        // 3a) 一次性回调重载（订阅即弃）
        login.Login(
            result => Debug.Log($"登录成功 token={result.Token}"),
            error => Debug.LogWarning($"登录失败: {error}"));

        // 3b) 或显式路由：ChannelId 为空走平台默认，"Google" 走注册表路由
        login.Login(new LoginParams());                    // 平台默认渠道
        login.Login(new LoginParams("Google"));            // 显式渠道路由

        // 4) 枚举可用渠道（登录 UI 并列展示）
        foreach (var channel in login.GetAvailableChannels())
        {
            Debug.Log($"channel={channel.ChannelId} name={channel.DisplayName} available={channel.IsAvailable}");
        }
    }
}
```

## API 参考

### `Init()`

初始化平台默认登录渠道。渠道配置由渠道包自行读取。

### `IsReady()`

平台默认登录渠道是否准备好。

### `Login(LoginParams loginParams)`

执行登录。`ChannelId` 为空路由到平台默认管理器；非空经 `LoginChannelRegistry` 路由（未注册渠道告警 no-op）。结果触发聚合的 `OnLoginSuccess` / `OnLoginFailed` 事件。

### `Login(Action<LoginResult> onSuccess, Action<string> onFailed)`

平台默认渠道的一次性回调便捷重载（订阅即弃）。

### `Logout()`

退出平台默认渠道登录。

### `GetAvailableChannels()`

返回已注册渠道列表（`ChannelId` / `DisplayName` / `IsAvailable`），供 UI 展示。

### `IsChannelAvailable(string channelId)`

渠道是否已注册且当前可用。

### `LoginResult`

渠道无关结果基类：`ChannelId`、`OpenId`、`Token`（统一承载渠道令牌——服务器校验方以此字段为准）、`DisplayName`、`PhotoUrl`、`Raw`。渠道子类（如 `GoogleLoginResult`）只扩展渠道特有字段。

## 架构概览

### 渠道注册表拓扑（v1.0.0）

- 核心包 `com.gameframex.unity.login`（本包）：契约（`ILoginManager` / `LoginResult` / `LoginParams` / `LoginEventArgs`）+ 注册表（`LoginChannelRegistry`）+ 唯一 `LoginComponent` 门面与 `DefaultLoginManager` 兜底。
- 渠道包（如 `com.gameframex.unity.login.google`）：实现 `BaseLoginManager`，经各自 Bootstrap 在 `BeforeSceneLoad` 自注册，package.json 只依赖核心包。未安装的渠道包代码被裁剪——注册不会发生。
- 新增登录渠道 = 新渠道包 + Bootstrap 自注册；entry 包永久只保留单一 `GameApp.Login` 访问面。

### 渠道支持表

| 渠道 | 包 | 状态 |
|------|-----|------|
| Google | `com.gameframex.unity.login.google` | 已支持（3.0.0） |
| Apple / FaceBook / QQ / WeChat | `com.gameframex.unity.login.*` | 规划中（各自后续 change） |

### 迁移说明（自 per-provider 组件形态）

| 旧形态 | 新形态 |
|--------|--------|
| `GoogleLoginComponent`（场景组件 + Inspector） | `LoginComponent`（唯一门面；Google 为 Android / iOS 平台默认渠道） |
| `GameApp.GoogleLogin` | `GameApp.Login`（宏 `ENABLE_GAME_FRAME_X_GOOGLE_LOGIN` → `ENABLE_GAME_FRAME_X_LOGIN`） |
| `IGoogleLoginManager` | `ILoginManager` / `BaseLoginManager` |
| `GoogleLoginSuccess`（含 `IdToken`） | `GoogleLoginResult : LoginResult`（IdToken 收敛到 `Token`） |
| `loginSuccess` / `loginFail` 回调 | 聚合 `OnLoginSuccess` / `OnLoginFailed` 事件或一次性回调重载 |

## 平台支持

| 平台 | 支持 |
|------|------|
| Android | 支持（渠道包） |
| iOS | 支持（渠道包） |
| Editor | 支持（渠道包桩数据） |
| WebGL / PC / macOS | 兜底（`DefaultLoginManager`） |

## 文档与资源

- [文档](https://gameframex.doc.alianblank.com)
- [GitHub 仓库](https://github.com/GameFrameX/com.gameframex.unity.login)

## 社区与支持

- QQ 群：通过[二维码](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)加入

## 依赖

| 包 | 版本 |
|-----|------|
| `com.gameframex.unity` | 2.5.4 |

## 更新日志

详见 [Releases](https://github.com/gameframex/com.gameframex.unity.login/releases) 更新日志。

## 开源协议

详见 [LICENSE.md](LICENSE.md) 文件。
