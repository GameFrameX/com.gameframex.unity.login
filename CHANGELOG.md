# [1.0.0](https://github.com/gameframex/com.gameframex.unity.login/compare/0.0.0...1.0.0) (2026-10-15)


### ⚠ BREAKING CHANGES

* **login:** login 域收敛为渠道注册表形态（核心包 + 渠道包），取代 per-provider「每渠道一个 Component + 一个 GameApp partial」旧形态

**首版契约：**

| 类型 | 说明 |
|---|---|
| `ILoginManager` / `BaseLoginManager` | 登录管理契约（`Init()` 无参 / `IsReady()` / `Login(LoginParams)` / `Logout()` + `OnLoginSuccess` / `OnLoginFailed` 事件），`BaseLoginManager` 实现 `IGameFrameworkModule` |
| `LoginResult` | 渠道无关结果基类：`ChannelId` / `OpenId` / `Token`（统一承载渠道令牌：Google IdToken、Apple credential、微信/QQ code 或 accessToken）/ `DisplayName` / `PhotoUrl` / `Raw`；渠道子类只扩展渠道特有字段 |
| `LoginParams` | 登录参数（`ChannelId` 路由键 + `Extra`），为空回落平台默认渠道 |
| `LoginChannelRegistry` | 注册表：Register 幂等保首 + Warning、Unregister、Contains、GetManager 工厂懒实例化缓存、GetAllChannelInfos、ResolveManager（空回落默认 + 同类型防双实例）、Reset @ SubsystemRegistration |
| `LoginComponent` | 唯一门面组件（平台默认管理器类型名字符串序列化；missing 回落 `DefaultLoginManager`；事件聚合接线；一次性回调便捷重载；`GetAvailableChannels()`） |
| `DefaultLoginManager` | 未装渠道包的占位实现（方法未实现告警 + `IsReady()` 恒 false） |

渠道接入：渠道包通过自身 Bootstrap（`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`）自注册，未安装渠道包时注册不发生；新增登录渠道不再需要修改 entry 包。
