// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 渠道无关的登录结果基类，各渠道可继承并扩展自有字段
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class LoginResult
    {
        /// <summary>
        /// 获取或设置渠道唯一标识（如 "Google"）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ChannelId { get; set; }

        /// <summary>
        /// 获取或设置渠道用户唯一标识（Google PlayerId、Apple user identifier、微信/QQ openid）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string OpenId { get; set; }

        /// <summary>
        /// 获取或设置统一承载的渠道令牌，服务器校验方以此字段为准
        /// </summary>
        /// <remarks>
        /// 统一令牌语义：Google 存 IdToken、Apple 存 credential、微信/QQ 存 code 或 accessToken。
        /// 渠道子类不再单独保留同名属性（Google 渠道原独立 IdToken 字段已收敛到本字段），业务侧登录流程可完全基于 Token 编写。
        /// </remarks>
        [UnityEngine.Scripting.Preserve]
        public string Token { get; set; }

        /// <summary>
        /// 获取或设置用户展示名称（渠道提供时）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string DisplayName { get; set; }

        /// <summary>
        /// 获取或设置用户头像地址（渠道提供时）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string PhotoUrl { get; set; }

        /// <summary>
        /// 获取或设置渠道原始数据（JSON 格式，供排查与扩展解析）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string Raw { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"ChannelId:{ChannelId} OpenId:{OpenId} DisplayName:{DisplayName} PhotoUrl:{PhotoUrl} Token:{Token}";
        }
    }
}
