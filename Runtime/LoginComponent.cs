// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.Login.Runtime
{
    /// <summary>
    /// 登录组件（login 域唯一门面，取代 per-provider 登录组件）
    /// </summary>
    /// <remarks>
    /// 渠道发现与解析：模块槽位注册为主包默认实现 <see cref="DefaultLoginManager"/>（渠道实现不再各自直接占用模块槽位），
    /// 实际的渠道管理器由 <see cref="LoginChannelRegistry"/> 在调用期互斥解析（渠道包经 Bootstrap 自注册，引用即注册；
    /// 渠道平台互斥由渠道包 Bootstrap 的宏墙保证），不再依赖 Inspector 序列化的实现类型全名字符串反射发现。
    /// 新增登录渠道 = 新渠道包 + Bootstrap 自注册，entry 包不再需要修改。
    /// </remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GameFrameXLoginCroppingHelper))]
    [AddComponentMenu("GameFrameX/Login")]
    [UnityEngine.Scripting.Preserve]
    [GameFrameXAutoComponent(800)]
    public class LoginComponent : GameFrameworkComponent
    {
        private ILoginManager _loginManager;

        /// <summary>
        /// 登录成功事件（互斥解析的默认渠道与路由渠道的结果均会触发）
        /// </summary>
        public event EventHandler<LoginEventArgs> OnLoginSuccess;

        /// <summary>
        /// 登录失败事件（互斥解析的默认渠道与路由渠道的结果均会触发）
        /// </summary>
        public event EventHandler<LoginEventArgs> OnLoginFailed;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            ImplementationComponentType = typeof(DefaultLoginManager);
            InterfaceComponentType = typeof(ILoginManager);
            base.Awake();

            _loginManager = GameFrameworkEntry.GetModule<ILoginManager>();
            if (_loginManager == null)
            {
                Log.Fatal("Login Manager is invalid.");
                return;
            }

            _loginManager.OnLoginSuccess += (sender, e) => OnLoginSuccess?.Invoke(this, e);
            _loginManager.OnLoginFailed += (sender, e) => OnLoginFailed?.Invoke(this, e);
        }

        /// <summary>
        /// 获取已注册的登录渠道列表（含可用性），用于 UI 展示可用登录方式
        /// </summary>
        /// <returns>渠道信息列表</returns>
        [UnityEngine.Scripting.Preserve]
        public IReadOnlyList<LoginChannelInfo> GetAvailableChannels()
        {
            return LoginChannelRegistry.GetAllChannelInfos();
        }

        /// <summary>
        /// 渠道是否已注册且当前可用
        /// </summary>
        /// <param name="channelId">渠道唯一标识</param>
        /// <returns>可用返回true，否则返回false</returns>
        [UnityEngine.Scripting.Preserve]
        public bool IsChannelAvailable(string channelId)
        {
            var manager = LoginChannelRegistry.GetManager(channelId);
            return manager != null && manager.IsReady();
        }

        /// <summary>
        /// 初始化登录渠道（转发给注册表互斥解析到的渠道）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public void Init()
        {
            _loginManager.Init();
        }

        /// <summary>
        /// 登录渠道是否准备好（转发给注册表互斥解析到的渠道）
        /// </summary>
        /// <returns>准备好返回true，否则返回false</returns>
        [UnityEngine.Scripting.Preserve]
        public bool IsReady()
        {
            return _loginManager.IsReady();
        }

        /// <summary>
        /// 执行登录（推荐使用）
        /// </summary>
        /// <param name="loginParams">登录参数；ChannelId 为空走注册表互斥解析（注册顺序即优先级），非空按注册表路由（未注册告警 no-op）</param>
        [UnityEngine.Scripting.Preserve]
        public void Login(LoginParams loginParams)
        {
            if (loginParams == null)
            {
                Log.Warning("Login params is null.");
                return;
            }

            _loginManager.Login(loginParams);
        }

        /// <summary>
        /// 执行默认渠道登录（一次性回调便捷重载，内部订阅即弃）
        /// </summary>
        /// <param name="onSuccess">登录成功回调，携带渠道无关登录结果</param>
        /// <param name="onFailed">登录失败回调，携带错误消息</param>
        [UnityEngine.Scripting.Preserve]
        public void Login(Action<LoginResult> onSuccess, Action<string> onFailed)
        {
            if (onSuccess == null && onFailed == null)
            {
                Log.Warning("Login callbacks are both null.");
                return;
            }

            EventHandler<LoginEventArgs> successHandler = null;
            EventHandler<LoginEventArgs> failedHandler = null;
            successHandler = (sender, e) =>
            {
                OnLoginSuccess -= successHandler;
                OnLoginFailed -= failedHandler;
                onSuccess?.Invoke(e.Result);
            };
            failedHandler = (sender, e) =>
            {
                OnLoginSuccess -= successHandler;
                OnLoginFailed -= failedHandler;
                onFailed?.Invoke(e.ErrorMessage);
            };

            OnLoginSuccess += successHandler;
            OnLoginFailed += failedHandler;

            Login(new LoginParams());
        }

        /// <summary>
        /// 退出登录（转发给注册表互斥解析到的渠道）
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public void Logout()
        {
            _loginManager.Logout();
        }
    }
}
