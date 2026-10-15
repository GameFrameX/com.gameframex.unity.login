// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 登录参数基类，仅承载路由信息，各渠道可继承并扩展自有字段
    /// </summary>
    /// <remarks>
    /// 与 payment 域的差异：purchase 的渠道差异在入参子类（PurchaseParams），login 的渠道差异在出参子类（<see cref="LoginResult"/>），
    /// 入参基类只承载路由键与透传扩展数据。
    /// </remarks>
    [UnityEngine.Scripting.Preserve]
    public class LoginParams
    {
        /// <summary>
        /// 初始化登录参数
        /// </summary>
        /// <param name="channelId">渠道唯一标识；为空时回落 <see cref="LoginComponent"/> 的平台默认渠道</param>
        /// <param name="extra">透传扩展数据（如服务器下单上下文），渠道自行解释</param>
        [UnityEngine.Scripting.Preserve]
        public LoginParams(string channelId = null, string extra = null)
        {
            ChannelId = channelId;
            Extra = extra;
        }

        /// <summary>
        /// 获取或设置渠道唯一标识（路由键，对齐 PurchaseParams.ChannelId）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ChannelId { get; set; }

        /// <summary>
        /// 获取或设置透传扩展数据
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string Extra { get; set; }
    }
}
