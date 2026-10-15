// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using GameFrameX.Runtime;

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 登录默认管理器，主包模块槽位的默认实现。
    /// </summary>
    /// <remarks>
    /// Default login manager occupying the module slot of the main package.
    /// 渠道发现与解析不再依赖 Inspector 序列化的实现类型全名字符串：渠道包经 Bootstrap 自注册进
    /// <see cref="LoginChannelRegistry"/>（引用即注册），本管理器把调用转发给注册表互斥解析到的渠道管理器
    /// （未携带渠道标识时按注册顺序取首个；显式渠道标识时按注册表路由），并把渠道触发的结果事件聚合转发；
    /// 框架关闭时把 Shutdown 一并转发给解析到的渠道管理器（渠道自身实现 <c>IShutdownableModule</c> 时生效）。
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultLoginManager : BaseLoginManager, IShutdownableModule
    {
        private bool _noChannelWarned;
        private readonly HashSet<ILoginManager> _wiredChannels = new HashSet<ILoginManager>();

        [UnityEngine.Scripting.Preserve]
        public DefaultLoginManager()
        {
        }

        /// <summary>
        /// 初始化登录渠道，转发给注册表互斥解析到的渠道管理器。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public override void Init()
        {
            ResolveChannel()?.Init();
        }

        /// <summary>
        /// 登录渠道是否准备好，转发给注册表互斥解析到的渠道管理器。
        /// </summary>
        /// <returns>渠道就绪返回true；无注册渠道返回false</returns>
        [UnityEngine.Scripting.Preserve]
        public override bool IsReady()
        {
            return ResolveChannel()?.IsReady() ?? false;
        }

        /// <summary>
        /// 执行登录：参数携带渠道标识时按注册表路由到指定渠道，否则转发给互斥解析到的渠道管理器。
        /// </summary>
        /// <param name="loginParams">登录参数</param>
        [UnityEngine.Scripting.Preserve]
        public override void Login(LoginParams loginParams)
        {
            var channel = !string.IsNullOrEmpty(loginParams?.ChannelId)
                ? LoginChannelRegistry.ResolveManager(loginParams.ChannelId)
                : ResolveChannel();
            if (channel == null)
            {
                if (!string.IsNullOrEmpty(loginParams?.ChannelId))
                {
                    Log.Warning("Login channel '{0}' is not registered.", loginParams.ChannelId);
                }

                return;
            }

            WireChannelEvents(channel);
            channel.Login(loginParams);
        }

        /// <summary>
        /// 退出登录，转发给注册表互斥解析到的渠道管理器。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public override void Logout()
        {
            ResolveChannel()?.Logout();
        }

        /// <summary>
        /// 关闭模块：把关闭时机转发给注册表互斥解析到的渠道管理器（渠道自身实现 <c>IShutdownableModule</c> 时生效）。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public void Shutdown()
        {
            (LoginChannelRegistry.ResolveManager() as IShutdownableModule)?.Shutdown();
        }

        /// <summary>
        /// 互斥解析当前渠道管理器；无注册渠道时告警（每次会话只告警一次）并返回 null。
        /// </summary>
        /// <returns>渠道管理器；无可用渠道返回null</returns>
        private ILoginManager ResolveChannel()
        {
            var manager = LoginChannelRegistry.ResolveManager();
            if (manager == null && !_noChannelWarned)
            {
                _noChannelWarned = true;
                Log.Warning("No login channel registered. Install a channel package (e.g. login.google) or register one via LoginChannelRegistry.");
            }

            return manager;
        }

        /// <summary>
        /// 接线渠道结果事件到本管理器聚合转发（同一渠道只接线一次）
        /// </summary>
        /// <param name="channel">渠道管理器</param>
        private void WireChannelEvents(ILoginManager channel)
        {
            if (channel == null || !_wiredChannels.Add(channel))
            {
                return;
            }

            channel.OnLoginSuccess += (sender, e) => RaiseLoginSuccess(e);
            channel.OnLoginFailed += (sender, e) => RaiseLoginFailed(e);
        }
    }
}
