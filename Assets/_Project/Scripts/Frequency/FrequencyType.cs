/// <summary>
/// FREQUENCY 프로젝트에서 사용하는 5개 주파수 열거형.
/// 레이어 마스크, Volume Weight, Renderer Feature 활성화에 모두 사용된다.
/// </summary>
public enum FrequencyType
{
    Hz440  = 0,  // 기본 주파수 — 정상 시야
    Hz20   = 1,  // 열화상 주파수
    Hz783  = 2,  // 7.83Hz — 슈만 공명, 홀로그램/시간 잔향
    Hz1898 = 3,  // 18.98Hz — 공간 왜곡, 숨겨진 통로
    Hz528  = 4,  // 528Hz — 내부 구조 시각화
}
