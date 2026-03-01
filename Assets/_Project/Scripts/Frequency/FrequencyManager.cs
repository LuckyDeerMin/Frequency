using UnityEngine;
using System;

/// <summary>
/// 주파수 전환의 중심 관리자.
/// 레이어 마스크 변경, Volume 전환, Renderer Feature 강도 제어를 조율한다.
/// </summary>
public class FrequencyManager : MonoBehaviour
{
    public static FrequencyManager Instance { get; private set; }

    [Header("State")]
    [SerializeField] private FrequencyType m_CurrentFrequency = FrequencyType.Hz440;
    public FrequencyType CurrentFrequency => m_CurrentFrequency;

    // 실제 Unity 레이어 인덱스 — LayerMask.NameToLayer로 런타임에 확인
    private static int LayerPlayer    => LayerMask.NameToLayer("Player");
    private static int LayerEnvShared => LayerMask.NameToLayer("Env_Shared");
    private static int LayerEnv440Hz  => LayerMask.NameToLayer("Env_440Hz");
    private static int LayerEnv20Hz   => LayerMask.NameToLayer("Env_20Hz");
    private static int LayerEnv783Hz  => LayerMask.NameToLayer("Env_783Hz");
    private static int LayerEnv1898Hz => LayerMask.NameToLayer("Env_1898Hz");
    private static int LayerEnv528Hz  => LayerMask.NameToLayer("Env_528Hz");
    private static int LayerResidual  => LayerMask.NameToLayer("Residual");
    private static int LayerInteract  => LayerMask.NameToLayer("Interactable");
    private static int LayerTrigger   => LayerMask.NameToLayer("Trigger");

    public event Action<FrequencyType, FrequencyType> OnFrequencyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplyCullingMask(m_CurrentFrequency);
    }

    public void SwitchFrequency(FrequencyType target)
    {
        if (target == m_CurrentFrequency) return;

        FrequencyType previous = m_CurrentFrequency;
        m_CurrentFrequency = target;

        ApplyCullingMask(target);
        OnFrequencyChanged?.Invoke(previous, target);
    }

    private void ApplyCullingMask(FrequencyType freq)
    {
        if (Camera.main == null) return;

        int baseMask = (1 << 0)                   // Default
                     | (1 << LayerPlayer)
                     | (1 << LayerEnvShared)
                     | (1 << LayerResidual)
                     | (1 << LayerInteract)
                     | (1 << LayerTrigger)
                     | (1 << 5);                   // UI

        int freqLayer = freq switch
        {
            FrequencyType.Hz440  => LayerEnv440Hz,
            FrequencyType.Hz20   => LayerEnv20Hz,
            FrequencyType.Hz783  => LayerEnv783Hz,
            FrequencyType.Hz1898 => LayerEnv1898Hz,
            FrequencyType.Hz528  => LayerEnv528Hz,
            _                    => LayerEnv440Hz,
        };

        Camera.main.cullingMask = baseMask | (1 << freqLayer);
    }
}
