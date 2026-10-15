<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Login

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.login)](https://github.com/GameFrameX/com.gameframex.unity.login/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · QQ 그룹: 467608841 / 233840761

<br />

[English](../../README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 프로젝트 개요

**로그인 컴포넌트** - GameFrameX 로그인 채널 레지스트리 핵심 패키지. 채널 독립 로그인 계약(`ILoginManager`), 통합 결과 기반 클래스(`LoginResult`), `LoginChannelRegistry`(채널 패키지가 시작 시 각자의 Bootstrap으로 자동 등록), 유일한 `LoginComponent` 파사드를 제공합니다. 새 로그인 채널 추가 시 entry 패키지를 수정할 필요가 없습니다.

### 기능

- 채널 독립 로그인 계약: `Init()` / `IsReady()` / `Login(LoginParams)` / `Logout()` + 통합된 `OnLoginSuccess` / `OnLoginFailed` 이벤트
- 통합 결과 기반 클래스 `LoginResult`: `ChannelId` / `OpenId` / `Token`(채널 자격 증명 통합 전달: Google IdToken, Apple credential, WeChat/QQ code 또는 accessToken) / `DisplayName` / `PhotoUrl` / `Raw`. 채널 하위 클래스는 채널 고유 필드만 추가
- 명시적 채널 라우팅: `LoginParams.ChannelId`로 레지스트리 라우팅. 비어 있으면 플랫폼 기본 관리자로 폴백(직렬화된 타입 이름 문자열, 누락 시 `DefaultLoginManager`)
- 채널 레지스트리: Register 멱등(선등록 우선 + 경고), 팩토리 지연 인스턴스화 및 캐싱, `SubsystemRegistration`에서 `Reset`, 동일 타입 라우팅 결과는 기본 인스턴스 재사용(이중 인스턴스 방지)
- UI용 채널 열거: `GetAvailableChannels()`가 채널 ID / 표시 이름 / 가용성 반환
- 일회성 콜백 편의 오버로드 `Login(onSuccess, onFailed)`(구독 후 즉시 해제)

## 빠른 시작

### 설치

아래 방법 중 하나를 선택하세요:

1. Edit your Unity project's `Packages/manifest.json` and add the `scopedRegistries` section:
   ```json
   {
     "scopedRegistries": [
       {
         "name": "GameFrameX",
         "url": "https://gameframex.upm.alianblank.uk",
         "scopes": [
           "com.gameframex"
         ]
       }
     ],
     "dependencies": {
       "com.gameframex.unity.login": "1.0.0"
     }
   }
   ```

   `scopes`는 이 레지스트리를 통해 해석될 패키지를 제어합니다. `com.gameframex`로 시작하는 패키지만 이 레지스트리에서 가져옵니다.

2. Add to `manifest.json` dependencies:
   ```json
   {
      "com.gameframex.unity.login": "https://github.com/gameframex/com.gameframex.unity.login.git"
   }
   ```
3. Use **Package Manager** in Unity with **Git URL**: `https://github.com/gameframex/com.gameframex.unity.login.git`
4. Clone the repository into your Unity project's `Packages` directory. It will be loaded automatically.

## 사용 예시

```csharp
using GameFrameX.Login.Runtime;
using UnityEngine;

public class LoginDemo : MonoBehaviour
{
    void Start()
    {
        // 1) GameApp 바로가기(entry 패키지) 또는 GameEntry.GetComponent<LoginComponent>()
        //    var login = GameApp.Login;
        var login = GameEntry.GetComponent<LoginComponent>();

        // 통합 이벤트 구독(플랫폼 기본 채널과 라우팅 채널 결과 모두 발생)
        login.OnLoginSuccess += (sender, e) =>
        {
            // 채널 독립: 서버 검증은 e.Result.Token 사용
            Debug.Log($"로그인 성공 channel={e.Result.ChannelId} openId={e.Result.OpenId} token={e.Result.Token}");
        };
        login.OnLoginFailed += (sender, e) => Debug.LogWarning($"로그인 실패 channel={e.ChannelId} error={e.ErrorMessage}");

        // 2) 플랫폼 기본 채널 초기화(채널 설정은 채널 패키지가 직접 읽음)
        login.Init();

        // 3a) 일회성 콜백 오버로드(구독 후 즉시 해제)
        login.Login(
            result => Debug.Log($"로그인 성공 token={result.Token}"),
            error => Debug.LogWarning($"로그인 실패: {error}"));

        // 3b) 명시적 라우팅: ChannelId가 비어 있으면 플랫폼 기본, "Google"이면 레지스트리 라우팅
        login.Login(new LoginParams());                    // 플랫폼 기본 채널
        login.Login(new LoginParams("Google"));            // 명시적 채널 라우팅

        // 4) 사용 가능한 채널 열거(로그인 UI 병렬 표시)
        foreach (var channel in login.GetAvailableChannels())
        {
            Debug.Log($"channel={channel.ChannelId} name={channel.DisplayName} available={channel.IsAvailable}");
        }
    }
}
```

## API 참조

### `Init()`

플랫폼 기본 로그인 채널을 초기화합니다. 채널 설정은 채널 패키지가 직접 읽습니다.

### `IsReady()`

플랫폼 기본 로그인 채널 준비 여부.

### `Login(LoginParams loginParams)`

로그인을 실행합니다. `ChannelId`가 비어 있으면 플랫폼 기본 관리자로, 비어 있지 않으면 `LoginChannelRegistry`로 라우팅합니다(미등록 채널은 경고 후 no-op). 결과는 통합된 `OnLoginSuccess` / `OnLoginFailed` 이벤트로 발생합니다.

### `Login(Action<LoginResult> onSuccess, Action<string> onFailed)`

플랫폼 기본 채널용 일회성 콜백 편의 오버로드(구독 후 즉시 해제).

### `Logout()`

플랫폼 기본 채널에서 로그아웃합니다.

### `GetAvailableChannels()`

등록된 채널 목록(`ChannelId` / `DisplayName` / `IsAvailable`)을 UI 표시용으로 반환합니다.

### `IsChannelAvailable(string channelId)`

채널이 등록되어 있고 현재 사용 가능한지 여부.

### `LoginResult`

채널 독립 결과 기반 클래스: `ChannelId`, `OpenId`, `Token`(채널 자격 증명 통합 전달. 서버 검증 기준 필드), `DisplayName`, `PhotoUrl`, `Raw`. 채널 하위 클래스(예: `GoogleLoginResult`)는 채널 고유 필드만 추가합니다.

## 아키텍처

### 채널 레지스트리 토폴로지(v1.0.0)

- 핵심 패키지 `com.gameframex.unity.login`(이 패키지): 계약(`ILoginManager` / `LoginResult` / `LoginParams` / `LoginEventArgs`) + 레지스트리(`LoginChannelRegistry`) + 유일한 `LoginComponent` 파사드와 `DefaultLoginManager` 폴백.
- 채널 패키지(예: `com.gameframex.unity.login.google`): `BaseLoginManager` 구현, 각자의 Bootstrap으로 `BeforeSceneLoad`에 자동 등록. package.json은 핵심 패키지만 의존. 설치하지 않은 채널 패키지 코드는 스트리핑되어 등록이 발생하지 않습니다.
- 새 로그인 채널 추가 = 새 채널 패키지 + Bootstrap 자동 등록. entry 패키지는 영구적으로 단일 `GameApp.Login` 접근자만 유지합니다.

### 채널 지원 표

| 채널 | 패키지 | 상태 |
|------|--------|------|
| Google | `com.gameframex.unity.login.google` | 지원됨(3.0.0) |
| Apple / FaceBook / QQ / WeChat | `com.gameframex.unity.login.*` | 계획 중(각각 후속 change) |

### 마이그레이션 안내(per-provider 컴포넌트 형태에서)

| 이전 | 새 |
|------|-----|
| `GoogleLoginComponent`(씬 컴포넌트 + Inspector) | `LoginComponent`(유일한 파사드. Google은 Android / iOS 플랫폼 기본) |
| `GameApp.GoogleLogin` | `GameApp.Login`(정의 `ENABLE_GAME_FRAME_X_GOOGLE_LOGIN` -> `ENABLE_GAME_FRAME_X_LOGIN`) |
| `IGoogleLoginManager` | `ILoginManager` / `BaseLoginManager` |
| `GoogleLoginSuccess`(`IdToken` 포함) | `GoogleLoginResult : LoginResult`(IdToken은 `Token`으로 통합) |
| `loginSuccess` / `loginFail` 콜백 | 통합된 `OnLoginSuccess` / `OnLoginFailed` 이벤트 또는 일회성 콜백 오버로드 |

## 플랫폼 지원

| 플랫폼 | 지원 |
|----------|-----------|
| Android | 지원(채널 패키지) |
| iOS | 지원(채널 패키지) |
| Editor | 지원(채널 패키지 스텁) |
| WebGL / PC / macOS | 폴백(`DefaultLoginManager`) |

## 문서 및 자료

- [문서](https://gameframex.doc.alianblank.com)
- [GitHub 저장소](https://github.com/GameFrameX/com.gameframex.unity.login)

## 커뮤니티 및 지원

- QQ 그룹: [QR 코드](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)로 참여

## 의존성

| 패키지 | 버전 |
|---------|---------|
| `com.gameframex.unity` | 2.5.4 |

## 변경 로그

변경 로그는 [Releases](https://github.com/gameframex/com.gameframex.unity.login/releases)를 참조하세요.

## 라이선스

자세한 내용은 [LICENSE.md](LICENSE.md) 파일을 참조하세요.
