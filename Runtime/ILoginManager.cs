// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 登录管理接口
    /// </summary>
    /// <remarks>
    /// 渠道无关的登录契约。渠道包（如 login.google）通过 Bootstrap 自注册进 <see cref="LoginChannelRegistry"/>，
    /// 业务侧经 <see cref="LoginComponent"/> 门面访问，不直接依赖渠道实现类型。
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public interface ILoginManager
    {
        /// <summary>
        /// 登录成功事件
        /// </summary>
        event EventHandler<LoginEventArgs> OnLoginSuccess;

        /// <summary>
        /// 登录失败事件
        /// </summary>
        event EventHandler<LoginEventArgs> OnLoginFailed;

        /// <summary>
        /// 初始化登录渠道。渠道配置（projectId / appId 等）由渠道包自行读取，核心包不建配置中心。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        void Init();

        /// <summary>
        /// 登录渠道是否准备好
        /// </summary>
        /// <returns>准备好返回true，否则返回false</returns>
        bool IsReady();

        /// <summary>
        /// 执行登录
        /// </summary>
        /// <param name="loginParams">登录参数，ChannelId 为空时由门面回落平台默认渠道；渠道子类可扩展自有字段</param>
        [UnityEngine.Scripting.Preserve]
        void Login(LoginParams loginParams);

        /// <summary>
        /// 退出登录
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        void Logout();
    }
}
