// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Runtime;

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 默认登录管理器（占位实现，用于未安装渠道包或平台默认类型缺失时的兜底）
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultLoginManager : BaseLoginManager
    {
        private const string NotImplWarning = "You are using DefaultLoginManager which is a stub. Please integrate a channel-specific implementation (e.g. login.google).";

        [UnityEngine.Scripting.Preserve]
        public DefaultLoginManager()
        {
        }

        /// <summary>
        /// 初始化登录渠道（占位实现，仅告警）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public override void Init()
        {
            Log.Warning(NotImplWarning);
        }

        /// <summary>
        /// 登录渠道是否准备好（占位实现恒为false）
        /// </summary>
        /// <returns>恒返回false</returns>
        [UnityEngine.Scripting.Preserve]
        public override bool IsReady()
        {
            return false;
        }

        /// <summary>
        /// 执行登录（占位实现，仅告警）
        /// </summary>
        /// <param name="loginParams">登录参数</param>
        [UnityEngine.Scripting.Preserve]
        public override void Login(LoginParams loginParams)
        {
            Log.Warning(NotImplWarning);
        }

        /// <summary>
        /// 退出登录（占位实现，仅告警）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public override void Logout()
        {
            Log.Warning(NotImplWarning);
        }
    }
}
