// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 登录渠道注册描述
    /// </summary>
    /// <remarks>
    /// 由渠道包在启动时通过 <see cref="LoginChannelRegistry.Register(LoginChannelRegistration)"/> 显式注册，
    /// 工厂委托在首次使用时才实例化渠道管理器。
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public sealed class LoginChannelRegistration
    {
        /// <summary>
        /// 初始化登录渠道注册描述
        /// </summary>
        /// <param name="channelId">渠道唯一标识</param>
        /// <param name="displayName">渠道展示名称（用于 UI 展示）</param>
        /// <param name="factory">渠道管理器工厂，编译期类型安全，首次使用时调用</param>
        [UnityEngine.Scripting.Preserve]
        public LoginChannelRegistration(string channelId, string displayName, Func<ILoginManager> factory)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                throw new ArgumentNullException(nameof(channelId));
            }

            ChannelId = channelId;
            DisplayName = displayName;
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// 获取渠道唯一标识
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ChannelId { get; }

        /// <summary>
        /// 获取渠道展示名称
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string DisplayName { get; }

        /// <summary>
        /// 获取渠道管理器工厂
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public Func<ILoginManager> Factory { get; }
    }
}
