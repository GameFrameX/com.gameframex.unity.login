# 1.0.0 (2026-10-15)


### Features

* **login:** 平台默认管理器改为注册表互斥解析 ([4a0aa08](https://github.com/gameframex/com.gameframex.unity.login/commit/4a0aa082eff488ccd5bc5d68975f6d4e88151be6))
* **login:** 新增登录渠道注册表核心包 1.0.0 ([83e4c7c](https://github.com/gameframex/com.gameframex.unity.login/commit/83e4c7cc5c55c17d60286dc3422659aa9087bc1d))

# [1.0.0](https://github.com/gameframex/com.gameframex.unity.login/compare/0.0.0...1.0.0) (2026-10-15)


### ⚠ BREAKING CHANGES

* **login:** login 域收敛为渠道注册表形态（核心包 + 渠道包），取代 per-provider「每渠道一个 Component + 一个 GameApp partial」旧形态

**首版契约：**

| 类型 | 说明 |
|---|---|
| `ILoginManager` / `BaseLoginManager` | 登录管理契约（`Init()` 无参 / `IsReady()` / `Login(LoginParams)` / `Logout()` + `OnLoginSuccess` / `OnLoginFailed` 事件），`BaseLoginManager` 实现 `IGameFrameworkModule` |
| `LoginResult` | 渠道无关结果基类：`ChannelId` / `OpenId` / `Token`（统一承载渠道令牌：Google IdToken、Apple credential、微信/QQ code 或 accessToken）/ `DisplayName` / `PhotoUrl` / `Raw`；渠道子类只扩展渠道特有字段 |
| `LoginParams` | 登录参数（`ChannelId` 路由键 + `Extra`），为空走注册表互斥解析 |
| `LoginChannelRegistry` | 注册表：Register 幂等保首 + Warning、Unregister、Contains、GetManager 工厂懒实例化缓存、GetAllChannelInfos、ResolveManager 互斥解析（无显式渠道标识按注册顺序取首个；显式标识按注册表路由）、Reset @ SubsystemRegistration |
| `LoginComponent` | 唯一门面组件（模块槽位固定注册主包 `DefaultLoginManager`；事件聚合接线；一次性回调便捷重载；`GetAvailableChannels()`） |
| `DefaultLoginManager` | 主包模块槽位默认实现：把调用（含 Shutdown）与结果事件转发给注册表互斥解析到的渠道管理器；无注册渠道时告警 no-op |

渠道接入：渠道包通过自身 Bootstrap（`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`，宏墙限定支持平台）自注册，未安装渠道包或平台不在宏墙内时注册不发生；新增登录渠道不再需要修改 entry 包。
