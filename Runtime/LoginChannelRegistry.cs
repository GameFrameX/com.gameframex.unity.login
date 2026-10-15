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
    /// 注册表在 SubsystemRegistration 阶段自动清空（早于各渠道的 BeforeSceneLoad 注册），关闭 Domain Reload 时不会产生重复注册。
    /// 注意：经注册表实例化的渠道管理器不参与 GameFrameworkEntry 的模块 Update/Shutdown 生命周期，仅适合回调驱动的渠道。
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
        /// 解析登录参数对应的渠道管理器
        /// </summary>
        /// <remarks>
        /// 路由规则：参数携带 ChannelId 时按注册表路由（未注册返回null）；未携带时回落到平台默认管理器；
        /// 路由结果与平台默认管理器同类型时返回平台默认实例，避免同渠道双实例。
        /// </remarks>
        /// <param name="defaultManager">平台默认登录管理器</param>
        /// <param name="channelId">登录参数携带的渠道标识，可为空</param>
        /// <returns>渠道管理器；无法路由返回null</returns>
        [UnityEngine.Scripting.Preserve]
        public static ILoginManager ResolveManager(ILoginManager defaultManager, string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                return defaultManager;
            }

            var manager = GetManager(channelId);
            if (manager == null)
            {
                return null;
            }

            if (defaultManager != null && manager.GetType() == defaultManager.GetType())
            {
                return defaultManager;
            }

            return manager;
        }
    }
}
