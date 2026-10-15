// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Login.Runtime;
using NUnit.Framework;

namespace GameFrameX.Login.Tests
{
    public class LoginChannelRegistryTests
    {
        private class StubLoginManager : ILoginManager
        {
            public event System.EventHandler<LoginEventArgs> OnLoginSuccess;
            public event System.EventHandler<LoginEventArgs> OnLoginFailed;

            public int InitCallCount;
            public int LoginCallCount;
            public int LogoutCallCount;

            public void Init() { InitCallCount++; }
            public virtual bool IsReady() { return true; }
            public void Login(LoginParams loginParams) { LoginCallCount++; }
            public void Logout() { LogoutCallCount++; }

            public void RaiseSuccess(LoginEventArgs e) { OnLoginSuccess?.Invoke(this, e); }
            public void RaiseFailed(LoginEventArgs e) { OnLoginFailed?.Invoke(this, e); }
        }

        private sealed class OtherLoginManager : StubLoginManager
        {
        }

        private sealed class NotReadyLoginManager : StubLoginManager
        {
            public override bool IsReady() { return false; }
        }

        [SetUp]
        public void SetUp()
        {
            LoginChannelRegistry.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            LoginChannelRegistry.Reset();
        }

        [Test]
        public void Register_ThenGetManager_CreatesInstanceLazilyAndCaches()
        {
            var factoryCallCount = 0;
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () =>
            {
                factoryCallCount++;
                return new StubLoginManager();
            }));

            Assert.IsTrue(LoginChannelRegistry.Contains("stub"));
            Assert.AreEqual(0, factoryCallCount, "工厂在首次获取前不应被调用");

            var first = LoginChannelRegistry.GetManager("stub");
            var second = LoginChannelRegistry.GetManager("stub");

            Assert.IsNotNull(first);
            Assert.IsInstanceOf<StubLoginManager>(first);
            Assert.AreSame(first, second, "同一渠道应复用实例");
            Assert.AreEqual(1, factoryCallCount);
        }

        [Test]
        public void Register_DuplicateChannelId_KeepsFirstRegistration()
        {
            var first = new StubLoginManager();
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "第一个", () => first));
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "第二个", () => new OtherLoginManager()));

            Assert.AreSame(first, LoginChannelRegistry.GetManager("stub"), "重复注册应保留首个");
        }

        [Test]
        public void GetManager_UnknownChannel_ReturnsNull()
        {
            Assert.IsNull(LoginChannelRegistry.GetManager("not-exists"));
            Assert.IsFalse(LoginChannelRegistry.Contains("not-exists"));
        }

        [Test]
        public void Register_NullArguments_AreIgnoredOrThrown()
        {
            LoginChannelRegistry.Register(null);
            Assert.IsFalse(LoginChannelRegistry.Contains("stub"));

            Assert.Throws<System.ArgumentNullException>(() => new LoginChannelRegistration(null, "无效", () => new StubLoginManager()));
            Assert.Throws<System.ArgumentNullException>(() => new LoginChannelRegistration("stub", "无效", null));
        }

        [Test]
        public void Unregister_RemovesChannelAndCachedInstance()
        {
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => new StubLoginManager()));
            var manager = LoginChannelRegistry.GetManager("stub");
            Assert.IsNotNull(manager);

            LoginChannelRegistry.Unregister("stub");
            Assert.IsFalse(LoginChannelRegistry.Contains("stub"));
            Assert.IsNull(LoginChannelRegistry.GetManager("stub"));

            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => new StubLoginManager()));
            Assert.IsInstanceOf<StubLoginManager>(LoginChannelRegistry.GetManager("stub"), "重新注册后可再次获取");
        }

        [Test]
        public void GetAllChannelInfos_ReturnsRegistrationWithAvailability()
        {
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => new StubLoginManager()));
            LoginChannelRegistry.Register(new LoginChannelRegistration("notready", "未就绪渠道", () => new NotReadyLoginManager()));

            var infos = LoginChannelRegistry.GetAllChannelInfos();

            Assert.AreEqual(2, infos.Count);
            Assert.IsTrue(infos[0].ChannelId == "stub" && infos[0].DisplayName == "存根渠道" && infos[0].IsAvailable, "StubLoginManager.IsReady 恒为 true");
            Assert.IsTrue(infos[1].ChannelId == "notready" && !infos[1].IsAvailable, "NotReadyLoginManager.IsReady 恒为 false");
        }

        [Test]
        public void ResolveManager_NoRegistrations_ReturnsNull()
        {
            Assert.IsNull(LoginChannelRegistry.ResolveManager());
            Assert.IsNull(LoginChannelRegistry.ResolveManager(null));
            Assert.IsNull(LoginChannelRegistry.ResolveManager(string.Empty));
        }

        [Test]
        public void ResolveManager_EmptyChannel_ResolvesFirstRegistered()
        {
            LoginChannelRegistry.Register(new LoginChannelRegistration("first", "第一渠道", () => new StubLoginManager()));
            LoginChannelRegistry.Register(new LoginChannelRegistration("second", "第二渠道", () => new OtherLoginManager()));

            var resolved = LoginChannelRegistry.ResolveManager();
            var resolvedAgain = LoginChannelRegistry.ResolveManager(null);

            Assert.IsInstanceOf<StubLoginManager>(resolved, "无显式渠道标识时按注册顺序取首个");
            Assert.AreSame(resolved, resolvedAgain, "解析结果懒实例化并缓存");
        }

        [Test]
        public void ResolveManager_RegisteredChannel_ReturnsChannelManager()
        {
            LoginChannelRegistry.Register(new LoginChannelRegistration("other", "其他渠道", () => new OtherLoginManager()));

            var resolved = LoginChannelRegistry.ResolveManager("other");

            Assert.IsInstanceOf<OtherLoginManager>(resolved);
        }

        [Test]
        public void ResolveManager_UnknownChannel_ReturnsNull()
        {
            Assert.IsNull(LoginChannelRegistry.ResolveManager("not-exists"));
        }

        [Test]
        public void LoginParams_ChannelId_RoutesAndEmptyResolvesFirstRegistered()
        {
            LoginChannelRegistry.Register(new LoginChannelRegistration("first", "第一渠道", () => new StubLoginManager()));
            LoginChannelRegistry.Register(new LoginChannelRegistration("other", "其他渠道", () => new OtherLoginManager()));

            var routedParams = new LoginParams("other");
            Assert.IsInstanceOf<OtherLoginManager>(LoginChannelRegistry.ResolveManager(routedParams.ChannelId));

            var defaultParams = new LoginParams();
            Assert.IsNull(defaultParams.ChannelId, "默认构造的 LoginParams 不携带渠道标识");
            Assert.IsNull(defaultParams.Extra);
            Assert.IsInstanceOf<StubLoginManager>(LoginChannelRegistry.ResolveManager(defaultParams.ChannelId));
        }


        [Test]
        public void LoginParams_Extra_IsCarriedThrough()
        {
            var loginParams = new LoginParams("stub", "server-context");
            Assert.AreEqual("stub", loginParams.ChannelId);
            Assert.AreEqual("server-context", loginParams.Extra);

            loginParams.ChannelId = null;
            loginParams.Extra = null;
            Assert.IsNull(loginParams.ChannelId);
            Assert.IsNull(loginParams.Extra);
        }

        [Test]
        public void LoginResult_BaseContract_CarriesUnifiedToken()
        {
            var result = new LoginResult
            {
                ChannelId = "Google",
                OpenId = "player-1",
                Token = "id-token",
                DisplayName = "name",
                PhotoUrl = "http://photo",
                Raw = "{}",
            };

            Assert.AreEqual("Google", result.ChannelId);
            Assert.AreEqual("player-1", result.OpenId);
            Assert.AreEqual("id-token", result.Token);
            Assert.AreEqual("name", result.DisplayName);
            Assert.AreEqual("http://photo", result.PhotoUrl);
            Assert.AreEqual("{}", result.Raw);
        }

        [Test]
        public void DefaultLoginManager_NoChannel_IsNotReadyAndLoginNoOps()
        {
            var manager = new DefaultLoginManager();

            Assert.IsFalse(manager.IsReady());
            Assert.DoesNotThrow(() => manager.Login(new LoginParams()));
            Assert.IsInstanceOf<ILoginManager>(manager);
        }

        [Test]
        public void DefaultLoginManager_ForwardsToRegisteredChannel()
        {
            var stub = new StubLoginManager();
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => stub));

            var manager = new DefaultLoginManager();
            manager.Init();
            var isReady = manager.IsReady();
            manager.Logout();

            Assert.IsTrue(isReady);
            Assert.AreEqual(1, stub.InitCallCount);
            Assert.AreEqual(1, stub.LogoutCallCount);
        }

        [Test]
        public void DefaultLoginManager_LoginEmptyChannel_ForwardsAndRelaysEvents()
        {
            var stub = new StubLoginManager();
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => stub));

            var manager = new DefaultLoginManager();
            var raised = new System.Collections.Generic.List<LoginEventArgs>();
            manager.OnLoginSuccess += (sender, e) => raised.Add(e);

            manager.Login(new LoginParams());
            Assert.AreEqual(1, stub.LoginCallCount, "空 ChannelId 经互斥解析转发到注册渠道");

            stub.RaiseSuccess(new LoginEventArgs { ChannelId = "stub" });
            Assert.AreEqual(1, raised.Count, "渠道结果事件经模块默认实现聚合转发");
        }

        [Test]
        public void DefaultLoginManager_LoginExplicitChannel_RoutesToChannel()
        {
            var stub = new StubLoginManager();
            var other = new OtherLoginManager();
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => stub));
            LoginChannelRegistry.Register(new LoginChannelRegistration("other", "其他渠道", () => other));

            new DefaultLoginManager().Login(new LoginParams("other"));

            Assert.AreEqual(0, stub.LoginCallCount);
            Assert.AreEqual(1, other.LoginCallCount);
        }

        [Test]
        public void Reset_ClearsAllRegistrations()
        {
            LoginChannelRegistry.Register(new LoginChannelRegistration("stub", "存根渠道", () => new StubLoginManager()));
            LoginChannelRegistry.Reset();

            Assert.IsFalse(LoginChannelRegistry.Contains("stub"));
            Assert.IsNull(LoginChannelRegistry.GetManager("stub"));
        }

        [Test]
        public void GoogleBootstrap_Register_RegistersGoogleChannel()
        {
            // 反射调用渠道包 Bootstrap 的注册方法，验证自注册产物（渠道标识与实例类型）
            var bootstrapType = System.Type.GetType("GameFrameX.Login.Google.Runtime.GoogleLoginBootstrap, GameFrameX.Login.Google.Runtime");
            Assume.That(bootstrapType, Is.Not.Null, "未安装 login.google 渠道包时跳过");
            var registerMethod = bootstrapType.GetMethod("Register", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            registerMethod.Invoke(null, null);

            Assert.IsTrue(LoginChannelRegistry.Contains("Google"));
            Assert.AreEqual("GameFrameX.Login.Google.Runtime.GoogleLoginManager", LoginChannelRegistry.GetManager("Google").GetType().FullName);
        }
    }
}
