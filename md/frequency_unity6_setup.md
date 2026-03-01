# 「FREQUENCY」 — Unity 6 URP 프로젝트 세팅 가이드

---

## 1. 프로젝트 생성

### Unity Hub에서 새 프로젝트

```
Unity Hub → New Project
├── Editor Version: Unity 6000.x LTS (최신 LTS 권장, 6000.0.x 이상)
├── Template: "3D (URP)"  ← 반드시 URP 템플릿 선택
├── Project Name: Frequency
└── Location: 원하는 경로
```

**왜 URP 템플릿인가:**
- Unity 6에서 URP가 기본 렌더러. 새 프로젝트는 이걸로 시작해야 Render Graph가 자동 활성화됨
- Built-in에서 변환하면 Render Graph 호환 문제 발생 가능
- HDRP는 이 프로젝트에 과함 — 1인칭 실내 공포는 URP로 충분

---

## 2. 폴더 구조

프로젝트 생성 후 즉시 폴더를 잡는다. 나중에 바꾸면 메타파일 꼬여서 고생한다.

```
Assets/
├── _Project/                    ← 모든 프로젝트 에셋의 루트 (언더스코어로 상단 고정)
│   ├── Scripts/
│   │   ├── Core/                ← GameManager, SceneLoader, SaveSystem
│   │   ├── Player/              ← FPSController, Interaction, Inventory
│   │   ├── Frequency/           ← FrequencyManager, FrequencyProfile, Timer
│   │   ├── Horror/              ← ResidualBase, Distorter, Amalgam, ChaseSequence
│   │   ├── Puzzle/              ← PuzzleBase, 각 퍼즐 구현
│   │   ├── Narrative/           ← AudioLogSystem, DialogueManager, StoryFlags
│   │   ├── Audio/               ← AudioManager, AmbienceController, HeartbeatSystem
│   │   └── UI/                  ← HUDController, FrequencyDial, MenuSystem
│   │
│   ├── Shaders/
│   │   ├── Base/                ← ToonLit.shader, ToonUnlit.shader
│   │   ├── PostProcessing/      ← 주파수별 풀스크린 셰이더 (5개)
│   │   ├── Residual/            ← 잔류자 타입별 셰이더
│   │   ├── FX/                  ← Glitch, Distortion, Dissolve, Hologram
│   │   ├── ShaderGraph/         ← Shader Graph 기반 셰이더 (.shadergraph)
│   │   └── Include/             ← 공용 HLSL 함수 (.hlsl)
│   │
│   ├── Art/
│   │   ├── Models/
│   │   │   ├── Environment/     ← 모듈형 복도, 방, 문
│   │   │   ├── Props/           ← 가구, 장비, 소품
│   │   │   ├── Residuals/       ← 잔류자 모델
│   │   │   └── Player/          ← 1인칭 팔/손 모델
│   │   ├── Textures/
│   │   │   ├── Environment/
│   │   │   ├── Props/
│   │   │   ├── UI/
│   │   │   └── LUT/             ← 주파수별 컬러 LUT 텍스처
│   │   ├── Materials/
│   │   │   ├── Environment/
│   │   │   ├── Props/
│   │   │   └── Frequency/       ← 주파수별 전용 머티리얼
│   │   └── Animations/
│   │       ├── Player/
│   │       └── Residuals/
│   │
│   ├── Audio/
│   │   ├── BGM/
│   │   ├── Ambience/            ← 주파수별 앰비언스 루프
│   │   ├── SFX/
│   │   ├── VoiceOver/           ← 음성 기록 (수빈, 연구원들)
│   │   └── Mixers/              ← Audio Mixer 프리셋
│   │
│   ├── Prefabs/
│   │   ├── Environment/         ← 모듈 프리팹 (복도, 방 등)
│   │   ├── Interactables/       ← 상호작용 가능 오브젝트
│   │   ├── Residuals/           ← 잔류자 프리팹
│   │   ├── FX/                  ← 파티클, VFX
│   │   └── UI/                  ← UI 프리팹
│   │
│   ├── ScriptableObjects/
│   │   ├── Frequency/           ← FrequencyProfile SO들
│   │   ├── Puzzles/             ← 퍼즐 설정 SO
│   │   ├── Residuals/           ← 잔류자 스폰 설정 SO
│   │   └── Narrative/           ← 음성 기록 데이터 SO
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Game.unity           ← 메인 게임 씬 (단일 씬 구조)
│   │   ├── _Prototype.unity     ← 기술 테스트용
│   │   └── _ShaderTest.unity    ← 셰이더 테스트용
│   │
│   └── Settings/
│       ├── Input/               ← Input System 액션 맵
│       └── Rendering/           ← URP Asset, Renderer, Volume Profile
│
├── Plugins/                     ← 서드파티 플러그인
└── StreamingAssets/              ← 런타임 로드 데이터 (필요 시)
```

---

## 3. URP 렌더링 파이프라인 설정

### 3-1. URP Asset 생성 및 설정

URP 템플릿으로 생성하면 기본 URP Asset이 있지만, FREQUENCY 프로젝트에 맞게 재설정한다.

```
Assets/_Project/Settings/Rendering/ 폴더에 생성:
├── FrequencyURPAsset.asset          (URP Asset)
├── FrequencyRenderer.asset          (Universal Renderer)
├── FrequencyRendererLow.asset       (저사양용 Renderer — 선택)
└── 각종 Volume Profile들
```

**FrequencyURPAsset 설정:**

```
[Rendering]
├── Rendering Path: Forward+          ← Unity 6 신규. 다수 라이트 효율적 처리
├── Depth Texture: ✅ ON              ← 필수! 포스트프로세싱에서 깊이 사용
├── Opaque Texture: ✅ ON             ← 필수! 글라스/왜곡 셰이더에서 사용
├── Opaque Downsampling: None         ← 품질 유지
├── Terrain Holes: OFF                ← 불필요
├── SRP Batcher: ✅ ON                ← 성능 필수
└── GPU Resident Drawer: Instanced Drawing  ← Unity 6 신기능. 드로우콜 감소

[Quality]
├── HDR: ✅ ON                        ← 블룸, 이미시브 효과에 필수
├── HDR Color Buffer Format: R16G16B16A16  ← 알파 채널 포함 (포스트프로세싱)
├── Anti Aliasing (MSAA): 4x          ← 아웃라인 셰이더 품질
├── Render Scale: 1.0                 ← 기본. 최적화 시 낮출 수 있음
└── Upscaling Filter: STP (Spatial-Temporal Post-Processing)  ← Unity 6 신기능

[Lighting]
├── Main Light: Per Pixel
├── Cast Shadows: ✅ ON
├── Shadow Resolution: 2048           ← 실내이므로 2048이면 충분
├── Shadow Distance: 30               ← 실내. 멀리 볼 일 없음
├── Shadow Cascade: 2                 ← 실내. 2개면 충분
├── Additional Lights: Per Pixel
├── Additional Light Count: 8         ← 실내 조명 여러 개
├── Additional Light Shadows: ✅ ON
├── Additional Shadow Resolution: 1024
├── Reflection Probes: ✅ ON
│   ├── Probe Blending: ✅ ON
│   └── Box Projection: ✅ ON         ← 실내 반사에 중요
└── Mixed Lighting: ✅ ON

[Shadows]
├── Soft Shadows: ✅ ON
├── Soft Shadow Quality: Medium
└── Conservative Enclosing Sphere: ✅ ON

[Post-processing]
├── Post Processing: ✅ ON
├── Grading Mode: High Dynamic Range
├── LUT Size: 32
└── Alpha Processing: ✅ ON           ← Unity 6 신기능. 주파수 전환 블렌딩에 활용

[Advanced]
├── Stencil: ✅ ON                    ← 필수! 잔류자/주파수별 오브젝트 마스킹
└── Store Actions: Auto
```

### 3-2. Universal Renderer 설정

```
FrequencyRenderer.asset:
├── Rendering Path: Forward+
├── Depth Priming Mode: Auto
├── Accurate G-buffer Normals: ON
├── Depth Texture Mode: After Opaques
├── Intermediate Texture: Auto
│
├── [Renderer Features]             ← 핵심! 여기에 커스텀 렌더링 추가
│   ├── (나중에 추가) ThermalVision Renderer Feature    — 20Hz
│   ├── (나중에 추가) HologramEffect Renderer Feature   — 7.83Hz
│   ├── (나중에 추가) SpatialDistortion Renderer Feature — 18.98Hz
│   ├── (나중에 추가) StructuralVision Renderer Feature  — 528Hz
│   ├── (나중에 추가) FixationOverlay Renderer Feature   — 고착 게이지 시각 효과
│   └── (나중에 추가) TransitionEffect Renderer Feature  — 주파수 전환 트랜지션
│
└── [Filtering]
    ├── Opaque Layer Mask: Everything
    └── Transparent Layer Mask: Everything
```

### 3-3. Graphics 설정 연결

```
Edit → Project Settings → Graphics
├── Scriptable Render Pipeline Settings: FrequencyURPAsset
└── Render Graph → Compatibility Mode: ❌ OFF (Render Graph 사용!)
    ※ Unity 6의 Render Graph API를 사용해야 커스텀 포스트프로세싱 성능 최적화 가능
    ※ 기존 ScriptableRenderPass는 Render Graph API로 작성

Edit → Project Settings → Quality
├── Default Quality: FrequencyURPAsset
└── (선택) Low Quality 추가: FrequencyURPAsset_Low
```

---

## 4. Render Graph 기반 커스텀 Renderer Feature 구조

Unity 6에서 커스텀 포스트프로세싱을 만드는 핵심. FREQUENCY의 모든 주파수 셰이더가 이 구조를 따른다.

### 기본 템플릿 — 하나 만들면 5개 주파수에 복제/변형

```csharp
// FrequencyPostProcessFeature.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class FrequencyPostProcessFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;              // 주파수별 포스트프로세싱 머티리얼
        public RenderPassEvent renderPassEvent 
            = RenderPassEvent.AfterRenderingPostProcessing;
        [Range(0f, 1f)]
        public float intensity = 1.0f;         // 전환 블렌딩용
    }

    public Settings settings = new Settings();
    private FrequencyPostProcessPass m_Pass;

    public override void Create()
    {
        m_Pass = new FrequencyPostProcessPass(settings);
        m_Pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, 
                                          ref RenderingData renderingData)
    {
        if (settings.material == null) return;
        renderer.EnqueuePass(m_Pass);
    }

    // ── Render Graph 기반 패스 ──
    class FrequencyPostProcessPass : ScriptableRenderPass
    {
        private Settings m_Settings;

        // Render Graph 패스 데이터
        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float intensity;
        }

        public FrequencyPostProcessPass(Settings settings)
        {
            m_Settings = settings;
        }

        // Render Graph API — Unity 6 방식
        public override void RecordRenderGraph(RenderGraph renderGraph, 
                                                ContextContainer frameContext)
        {
            var resourceData = frameContext.Get<UniversalResourceData>();
            
            // 소스 텍스처 (카메라 컬러)
            TextureHandle source = resourceData.activeColorTexture;

            // 임시 텍스처 생성
            var descriptor = renderGraph.GetTextureDesc(source);
            descriptor.name = "FrequencyPostProcess_Temp";
            TextureHandle destination = renderGraph.CreateTexture(descriptor);

            // 패스 등록
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Frequency Post Process", out var passData))
            {
                passData.source = source;
                passData.material = m_Settings.material;
                passData.intensity = m_Settings.intensity;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                builder.SetRenderFunc(static (PassData data, 
                                              RasterGraphContext context) =>
                {
                    data.material.SetFloat("_Intensity", data.intensity);
                    Blitter.BlitTexture(context.cmd, data.source, 
                                        new Vector4(1, 1, 0, 0), 
                                        data.material, 0);
                });
            }

            // 결과를 카메라 컬러로 복사
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(
                "Frequency Post Process Copy Back", out var passData2))
            {
                passData2.source = destination;

                builder.UseTexture(destination, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 
                                            0, AccessFlags.Write);

                builder.SetRenderFunc(static (PassData data, 
                                              RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, 
                                        new Vector4(1, 1, 0, 0), 0);
                });
            }
        }
    }
}
```

**각 주파수별로 이 Feature를 상속/변형하고, Material만 교체하면 된다.**

---

## 5. 레이어 & 태그 설정

```
Edit → Project Settings → Tags and Layers

[Layers]
├── Layer 0:  Default
├── Layer 1:  TransparentFX
├── Layer 2:  Ignore Raycast
├── Layer 3:  (비워둠)
├── Layer 4:  Water
├── Layer 5:  UI
├── Layer 6:  Player                 ← 1인칭 팔/손
├── Layer 7:  Env_Shared             ← 전 주파수 공유 환경 (바닥, 천장, 기본 벽)
├── Layer 8:  Env_440Hz              ← 440Hz 전용 오브젝트 (기본 소품, 문서 등)
├── Layer 9:  Env_20Hz               ← 20Hz 전용 (열 잔류 마커, 구조 약점)
├── Layer 10: Env_783Hz              ← 7.83Hz 전용 (시간 잔향 오브젝트)
├── Layer 11: Env_1898Hz             ← 18.98Hz 전용 (숨겨진 통로, 비밀 문)
├── Layer 12: Env_528Hz              ← 528Hz 전용 (내부 구조 오버레이)
├── Layer 13: Residual               ← 잔류자 (전 종류)
├── Layer 14: Interactable           ← 상호작용 가능 오브젝트
├── Layer 15: Trigger                ← 이벤트 트리거 볼륨
└── Layer 16~31: 여유분

[Tags]
├── Player
├── Residual_Thermal                 ← 20Hz 잔류자
├── Residual_Echo                    ← 7.83Hz 잔류자
├── Residual_Distorter               ← 18.98Hz 잔류자
├── Residual_Amalgam                 ← 수합체
├── Residual_Observer                ← 528Hz 관찰자
├── Interactable
├── AudioLog                         ← 음성 기록 트리거
├── SavePoint                        ← 저장 지점
└── FrequencyGate                    ← 특정 주파수에서만 활성화되는 문/장치
```

### 주파수 전환 시 레이어 마스크 처리

```csharp
// FrequencyManager.cs 에서
public void SwitchFrequency(FrequencyType target)
{
    // 카메라 컬링 마스크 변경
    int baseMask = (1 << 0)  // Default
                 | (1 << 6)  // Player
                 | (1 << 7)  // Env_Shared
                 | (1 << 14) // Interactable
                 | (1 << 15); // Trigger

    int freqLayer = target switch
    {
        FrequencyType.Hz440   => 8,
        FrequencyType.Hz20    => 9,
        FrequencyType.Hz783   => 10,
        FrequencyType.Hz1898  => 11,
        FrequencyType.Hz528   => 12,
        _ => 8
    };

    Camera.main.cullingMask = baseMask | (1 << freqLayer) | (1 << 13); // +Residual
    
    // Renderer Feature 활성화/비활성화
    // Post Processing Volume 전환
    // 오디오 크로스페이드
    // 잔류자 스포너 전환
}
```

---

## 6. Input System 설정

### 6-1. Input System 패키지 확인

```
Window → Package Manager → Input System (이미 포함되어 있을 것)
Edit → Project Settings → Player → Active Input Handling: "Input System Package (New)"
```

### 6-2. Input Action Asset 생성

```
Assets/_Project/Settings/Input/FrequencyInputActions.inputactions

[Action Maps]

─── Player (게임 중)
│   ├── Move          → WASD / 좌스틱          [Vector2]
│   ├── Look          → 마우스 델타 / 우스틱    [Vector2]
│   ├── Interact      → E / 남쪽 버튼           [Button]
│   ├── Sprint        → Shift / 좌스틱 누름     [Button]
│   ├── Crouch        → Ctrl / 우스틱 누름      [Button]
│   ├── FreqUp        → 마우스 휠 위 / RB       [Button]
│   ├── FreqDown      → 마우스 휠 아래 / LB     [Button]
│   ├── FreqDirect1   → 1 키 (440Hz)            [Button]
│   ├── FreqDirect2   → 2 키 (20Hz)             [Button]
│   ├── FreqDirect3   → 3 키 (7.83Hz)           [Button]
│   ├── FreqDirect4   → 4 키 (18.98Hz)          [Button]
│   ├── FreqDirect5   → 5 키 (528Hz)            [Button]
│   ├── Inventory     → Tab / 메뉴 버튼         [Button]
│   └── Pause         → Esc / 시작 버튼         [Button]
│
─── FrequencyDial (다이얼 조작 모드 — 오버스테이 탈출 등)
│   ├── DialRotate    → 마우스 X / 좌스틱 X     [Axis]
│   ├── DialConfirm   → 마우스 클릭 / 남쪽 버튼 [Button]
│   └── DialCancel    → Esc / 동쪽 버튼         [Button]
│
─── UI (메뉴)
    ├── Navigate      → 방향키 / 좌스틱         [Vector2]
    ├── Submit        → Enter / 남쪽 버튼       [Button]
    ├── Cancel        → Esc / 동쪽 버튼         [Button]
    └── Point/Click   → 마우스                  [Position + Button]
```

---

## 7. Post Processing Volume 구조

주파수 전환의 핵심. Global Volume과 주파수별 Volume을 분리한다.

```
Hierarchy 구조:

─── [Volumes]
    ├── GlobalVolume                    ← 항상 활성. 기본 포스트프로세싱
    │   └── Volume Profile: GlobalProfile
    │       ├── Tonemapping (ACES)
    │       ├── Color Adjustments (미세 조정)
    │       ├── Vignette (기본 약한 비네팅)
    │       └── Film Grain (아주 미세)
    │
    ├── Volume_440Hz                    ← 기본 주파수
    │   └── Volume Profile: Profile_440Hz
    │       ├── Bloom (약하게)
    │       ├── Color Adjustments (차갑고 청결한 톤)
    │       └── Chromatic Aberration (OFF)
    │
    ├── Volume_20Hz                     ← 비활성 상태로 시작
    │   └── Volume Profile: Profile_20Hz
    │       ├── Color Adjustments (열화상 팔레트 — LUT 사용)
    │       ├── Vignette (강하게)
    │       └── Film Grain (강한 노이즈)
    │
    ├── Volume_783Hz
    │   └── Volume Profile: Profile_783Hz
    │       ├── Color Adjustments (채도 낮춤, 시안 틴트)
    │       ├── Bloom (강하게 — 홀로그램 글로우)
    │       ├── Chromatic Aberration (중간)
    │       └── Depth of Field (약간 — 몽환감)
    │
    ├── Volume_1898Hz
    │   └── Volume Profile: Profile_1898Hz
    │       ├── Lens Distortion (바렐 디스토션)
    │       ├── Chromatic Aberration (강하게)
    │       ├── Vignette (매우 강하게)
    │       └── Color Adjustments (적녹 편향)
    │
    ├── Volume_528Hz
    │   └── Volume Profile: Profile_528Hz
    │       ├── Color Adjustments (파스텔/수채화 톤)
    │       ├── Bloom (부드럽게 강하게)
    │       └── Depth of Field (틸트 시프트 느낌)
    │
    └── Volume_Fixation                 ← 고착 게이지 연동
        └── Volume Profile: Profile_Fixation
            ├── Chromatic Aberration (게이지 비례)
            ├── Film Grain (게이지 비례)
            ├── Lens Distortion (게이지 비례)
            └── Vignette (게이지 비례)
```

**주파수 전환 시 Volume Weight를 크로스페이드:**

```csharp
// FrequencyVolumeController.cs
IEnumerator TransitionVolumes(FrequencyType from, FrequencyType to, float duration)
{
    Volume volumeFrom = GetVolume(from);
    Volume volumeTo = GetVolume(to);
    
    float elapsed = 0f;
    while (elapsed < duration)
    {
        float t = elapsed / duration;
        float curve = Mathf.SmoothStep(0, 1, t);  // 부드러운 커브
        
        volumeFrom.weight = 1f - curve;
        volumeTo.weight = curve;
        
        // Renderer Feature intensity도 동기화
        SetRendererFeatureIntensity(from, 1f - curve);
        SetRendererFeatureIntensity(to, curve);
        
        elapsed += Time.deltaTime;
        yield return null;
    }
    
    volumeFrom.weight = 0f;
    volumeTo.weight = 1f;
}
```

---

## 8. 물리/충돌 설정

```
Edit → Project Settings → Physics

[Layer Collision Matrix]
모든 레이어 간 충돌을 끄고, 필요한 것만 켠다:

                  Player  Shared  440  20  783  1898  528  Residual  Interact
Player              -       ✅    ✅   ✅   ✅    ✅   ✅     ✅        ✅
Env_Shared         ✅       -      -    -    -     -    -      ✅        -
Env_440Hz          ✅       -      -    -    -     -    -       -        -
Env_20Hz           ✅       -      -    -    -     -    -       -        -
... (주파수 환경끼리는 충돌 불필요)
Residual           ✅      ✅      -    -    -     -    -       -        -
Interactable       ✅       -      -    -    -     -    -       -        -

주파수 전환 시 플레이어의 충돌 레이어도 동적 변경:
├── 440Hz 모드: Player ↔ Shared + 440Hz + Interactable
├── 20Hz 모드:  Player ↔ Shared + 20Hz + Interactable
└── ...

[General]
├── Gravity: (0, -9.81, 0)           ← 기본
├── Default Solver Iterations: 6
├── Default Solver Velocity Iterations: 1
├── Auto Simulation: ✅ ON
└── Reuse Collision Callbacks: ✅ ON   ← GC 절약
```

---

## 9. 오디오 설정

### Audio Mixer 구조

```
Assets/_Project/Audio/Mixers/MasterMixer.mixer

MasterMixer
├── Master                              (메인 볼륨)
│   ├── BGM                             (배경음악)
│   ├── Ambience                        (환경음)
│   │   ├── Amb_440Hz                   (주파수별 앰비언스 서브그룹)
│   │   ├── Amb_20Hz
│   │   ├── Amb_783Hz
│   │   ├── Amb_1898Hz
│   │   └── Amb_528Hz
│   ├── SFX                             (효과음)
│   │   ├── SFX_Environment             (문, 기계, 발걸음)
│   │   ├── SFX_Resonator               (공명기 소리)
│   │   └── SFX_JumpScare               (점프 스케어 전용 — 항상 풀 볼륨)
│   ├── Residual                        (잔류자 소리)
│   ├── Heartbeat                       (심박 — 고착 게이지 연동)
│   ├── VoiceOver                       (음성 기록)
│   └── UI                              (인터페이스 소리)
│
[Exposed Parameters]  ← 스크립트에서 제어
├── masterVol
├── bgmVol
├── amb440Vol, amb20Vol, amb783Vol, amb1898Vol, amb528Vol
├── sfxVol
├── heartbeatBPM          ← 고착 게이지에 따라 BPM 변화
├── residualVol
└── voiceoverVol
```

### Audio Settings

```
Edit → Project Settings → Audio
├── Spatializer Plugin: (기본 or Resonance Audio)
├── Default Speaker Mode: Stereo       ← 서라운드는 추후 고려
├── DSP Buffer Size: Best Latency      ← 점프 스케어 반응성
└── Max Virtual Voices: 64             ← 잔류자 많을 때 대비
```

---

## 10. 기타 Project Settings

### Player Settings

```
Edit → Project Settings → Player
├── Company Name: [본인 이름 or 스튜디오명]
├── Product Name: FREQUENCY
├── Default Icon: (나중에)
├── Resolution and Presentation:
│   ├── Fullscreen Mode: Fullscreen Window
│   ├── Default Resolution: 1920 × 1080
│   └── Resizable Window: ✅
├── Color Space: Linear               ← 반드시 Linear! Gamma 쓰면 셰이더 전부 꼬임
├── Auto Graphics API: ❌ OFF
│   └── Graphics APIs: Vulkan, DX12, DX11 (순서대로)
├── Scripting Backend: IL2CPP          ← 릴리즈 성능. 개발 중엔 Mono도 OK
├── API Compatibility Level: .NET Standard 2.1
└── Active Input Handling: Input System Package (New)
```

### 버전 관리 (Git)

```bash
# 프로젝트 루트에서
git init
```

**.gitignore (Unity 표준):**
```
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
MemoryCaptures/
Recordings/
*.pidb.meta
*.pdb.meta
*.mdb.meta
sysinfo.txt
*.apk
*.aab
*.unitypackage
*.app
crashlytics-build.properties

# IDE
.vs/
.vscode/
*.csproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
.idea/

# OS
.DS_Store
Thumbs.db
```

**.gitattributes (LFS 설정):**
```
# Unity
*.unity merge=unityyamlmerge
*.prefab merge=unityyamlmerge

# LFS — 대용량 바이너리
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.anim filter=lfs diff=lfs merge=lfs -text
*.controller filter=lfs diff=lfs merge=lfs -text
*.asset filter=lfs diff=lfs merge=lfs -text
*.cubemap filter=lfs diff=lfs merge=lfs -text
*.unitypackage filter=lfs diff=lfs merge=lfs -text
```

### Editor Settings

```
Edit → Project Settings → Editor
├── Version Control Mode: Visible Meta Files
├── Asset Serialization Mode: Force Text    ← Git diff 가능하게
└── Enter Play Mode Settings: ✅ ON
    ├── Reload Domain: ❌ OFF               ← 플레이 진입 속도 대폭 향상
    └── Reload Scene: ❌ OFF
    ※ 주의: 이 설정 시 static 변수 초기화를 수동으로 해야 함
```

---

## 11. 필수 패키지 목록

```
Window → Package Manager에서 확인/설치:

[Unity Registry — 필수]
├── Universal RP                       (URP 템플릿에 이미 포함)
├── Shader Graph                       (이미 포함)
├── Input System                       (이미 포함)
├── Cinemachine                        ← 1인칭 카메라, 추격 시퀀스
├── TextMeshPro                        ← UI 텍스트 (이미 포함)
├── Timeline                           ← 서사 연출, 시간 잔향 시퀀스
├── Localization                       ← 한/영 다국어 (출시 전 추가해도 OK)
├── ProBuilder                         ← 그레이박스 레벨 빌딩
├── Addressables                       ← 에셋 관리/로딩 (중후반 추가)
└── Visual Scripting                   ← 선택. 간단한 이벤트 트리거에 유용

[선택]
├── Adaptive Performance               ← 동적 해상도/품질 (최적화 단계에서)
├── Unity Profiler                     ← 포함됨. 적극 사용
└── Memory Profiler                    ← 메모리 최적화
```

---

## 12. 첫날 체크리스트

프로젝트 생성 후 당일에 끝내야 할 것:

```
[ ] Unity 6 LTS + URP 3D 템플릿으로 프로젝트 생성
[ ] 폴더 구조 생성 (위 구조 그대로)
[ ] URP Asset/Renderer 재설정 (위 설정 그대로)
[ ] Color Space: Linear 확인
[ ] Render Graph 활성 확인 (Compatibility Mode OFF)
[ ] Depth Texture, Opaque Texture ON 확인
[ ] HDR ON 확인
[ ] 레이어 16개 설정
[ ] 태그 설정
[ ] Input System 액션 맵 생성
[ ] Audio Mixer 기본 구조 생성
[ ] Git 초기화 + .gitignore + .gitattributes
[ ] 첫 커밋: "Initial project setup"
[ ] Enter Play Mode Options 활성화
[ ] Cinemachine, Timeline, ProBuilder 패키지 확인
[ ] 테스트: 빈 씬에 큐브 + Directional Light → Play → 정상 렌더링 확인
[ ] 테스트: 포스트프로세싱 Global Volume 추가 → Bloom 확인
```

이것으로 **코드 한 줄 치기 전에 필요한 모든 세팅이 완료**된다.

---

## 13. 다음 단계 미리보기

세팅 완료 후 바로 시작할 작업 순서:

```
Day 2~3:   1인칭 컨트롤러 (Cinemachine + Input System)
Day 4~5:   FrequencyManager 기본 구조 (ScriptableObject 프로파일 + 레이어 전환)
Day 6~7:   440Hz 톤 셰이더 (기존 기술 포팅)
Day 8~10:  20Hz 열화상 포스트프로세싱 (첫 Renderer Feature)
Day 11~13: 7.83Hz 홀로그램 셰이더
Day 14~16: 18.98Hz 왜곡 셰이더
Day 17~19: 528Hz 구조 시각화 셰이더
Day 20:    전환 트랜지션 + 5개 주파수 통합 테스트
```
