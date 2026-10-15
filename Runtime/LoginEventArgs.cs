// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 登录事件参数
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class LoginEventArgs : EventArgs
    {
        /// <summary>
        /// 获取或设置渠道唯一标识
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ChannelId { get; set; }

        /// <summary>
        /// 获取或设置登录结果（登录成功时非空，渠道子类实例携带渠道特有字段）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public LoginResult Result { get; set; }

        /// <summary>
        /// 获取或设置错误消息（登录失败时非空）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ErrorMessage { get; set; }
    }
}
