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
    /// 登录渠道注册表
    /// </summary>
    /// <remarks>
    /// 渠道包通过自身的 Bootstrap（RuntimeInitializeOnLoadMethod）在启动时显式注册，未被引用的渠道包代码会被裁剪，天然满足"引用即注册"。
    /// 互斥解析语义：同一时刻只解析出一个渠道管理器——无显式渠道标识时按注册顺序取首个（渠道平台互斥由渠道包 Bootstrap 的宏墙保证），
    /// 这与支付域 <c>PaymentChannelRegistry</c> 的"一笔单 → 一个渠道"参数路由、统计域 <c>GameAnalyticsRegistry</c> 的事件广播（fan-out 到全部渠道）语义均不同。
    /// 显式路由覆盖：<see cref="ResolveManager"/> 携带渠道标识时按注册表路由到指定渠道（未注册返回 null）。
    /// 注册表在 SubsystemRegistration 阶段自动清空（早于各渠道的 BeforeSceneLoad 注册），关闭 Domain Reload 时不会产生重复注册。
    /// 注册表实例化的渠道管理器不参与 GameFrameworkEntry 的模块 Update/Shutdown 生命周期；
    /// 模块槽位由主包默认实现 <see cref="DefaultLoginManager"/> 占据，并把调用转发给注册表解析结果（含 Shutdown 转发）。
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public static class LoginChannelRegistry
    {
        private static readonly Dictionary<string, LoginChannelRegistration> Registrations = new Dictionary<string, LoginChannelRegistration>(8);
        private static readonly Dictionary<string, ILoginManager> Managers = new Dictionary<string, ILoginManager>(8);

        /// <summary>
        /// 重置登录渠道注册表
        /// </summary>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        [UnityEngine.Scripting.Preserve]
        public static void Reset()
        {
            Registrations.Clear();
            Managers.Clear();
        }

        /// <summary>
        /// 注册登录渠道（同标识重复注册时保留首个并告警）
        /// </summary>
        /// <param name="registration">渠道注册描述</param>
        [UnityEngine.Scripting.Preserve]
        public static void Register(LoginChannelRegistration registration)
        {
            if (registration == null)
            {
                return;
            }

            if (Registrations.ContainsKey(registration.ChannelId))
            {
                Log.Warning("Login channel '{0}' is already registered. The duplicate registration is ignored.", registration.ChannelId);
                return;
            }

            Registrations.Add(registration.ChannelId, registration);
        }

        /// <summary>
        /// 注销登录渠道
        /// </summary>
        /// <param name="channelId">渠道唯一标识</param>
        [UnityEngine.Scripting.Preserve]
        public static void Unregister(string channelId)
        {
            Registrations.Remove(channelId);
            Managers.Remove(channelId);
        }

        /// <summary>
        /// 渠道是否已注册
        /// </summary>
        /// <param name="channelId">渠道唯一标识</param>
        /// <returns>已注册返回true，否则返回false</returns>
        [UnityEngine.Scripting.Preserve]
        public static bool Contains(string channelId)
        {
            return !string.IsNullOrEmpty(channelId) && Registrations.ContainsKey(channelId);
        }

        /// <summary>
        /// 获取渠道管理器（首次获取时通过工厂实例化并缓存）
        /// </summary>
        /// <param name="channelId">渠道唯一标识</param>
        /// <returns>渠道管理器；未注册返回null</returns>
        [UnityEngine.Scripting.Preserve]
        public static ILoginManager GetManager(string channelId)
        {
            if (string.IsNullOrEmpty(channelId) || !Registrations.TryGetValue(channelId, out var registration))
            {
                return null;
            }

            if (!Managers.TryGetValue(channelId, out var manager))
            {
                manager = registration.Factory();
                Managers[channelId] = manager;
            }

            return manager;
        }

        /// <summary>
        /// 获取全部已注册渠道的信息（含可用性，会实例化渠道管理器以查询 IsReady）
        /// </summary>
        /// <returns>渠道信息列表</returns>
        [UnityEngine.Scripting.Preserve]
        public static IReadOnlyList<LoginChannelInfo> GetAllChannelInfos()
        {
            var infos = new List<LoginChannelInfo>(Registrations.Count);
            foreach (var registration in Registrations.Values)
            {
                var manager = GetManager(registration.ChannelId);
                infos.Add(new LoginChannelInfo
                {
                    ChannelId = registration.ChannelId,
                    DisplayName = registration.DisplayName,
                    IsAvailable = manager != null && manager.IsReady(),
                });
            }

            return infos;
        }

        /// <summary>
        /// 解析当前使用的登录渠道管理器
        /// </summary>
        /// <remarks>
        /// 互斥解析：未指定渠道标识时按注册顺序取首个注册渠道（注册顺序即优先级；渠道平台互斥由渠道包 Bootstrap 的宏墙保证），
        /// 多渠道并存且未显式指定时输出告警；显式渠道标识时按注册表路由到指定渠道（未注册返回 null）。
        /// 解析结果懒实例化并缓存，重复调用返回同一实例。
        /// </remarks>
        /// <param name="channelId">渠道唯一标识，为空时走互斥解析</param>
        /// <returns>渠道管理器；无可用渠道或未注册返回null</returns>
        [UnityEngine.Scripting.Preserve]
        public static ILoginManager ResolveManager(string channelId = null)
        {
            if (!string.IsNullOrEmpty(channelId))
            {
                return GetManager(channelId);
            }

            foreach (var registration in Registrations.Values)
            {
                if (Registrations.Count > 1)
                {
                    Log.Warning("Login has {0} registered channels but no explicit channel id given. Resolving by registration priority: '{1}'.", Registrations.Count, registration.ChannelId);
                }

                return GetManager(registration.ChannelId);
            }

            return null;
        }
    }
}
