#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 유니티 메뉴 Frequency ▶ 씬 빌드 — Simple Level 로 실행.
/// 기본 1인칭 플레이어 + 지형 + 장애물로 구성된 테스트 씬을 자동 생성한다.
/// </summary>
public static class SimpleSceneBuilder
{
    const float AREA = 25f; // 맵 반경

    [MenuItem("Frequency/씬 빌드 \u2500\u2500 Simple Level")]
    static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Ground ───────────────────────────────────────────────
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(AREA * 2f / 10f, 1f, AREA * 2f / 10f);

        // ── 경계 벽 ──────────────────────────────────────────────
        CreateBox("Wall_North", new Vector3(0f, 2f,  AREA), new Vector3(AREA * 2f, 4f, 0.5f));
        CreateBox("Wall_South", new Vector3(0f, 2f, -AREA), new Vector3(AREA * 2f, 4f, 0.5f));
        CreateBox("Wall_East",  new Vector3( AREA, 2f, 0f), new Vector3(0.5f, 4f, AREA * 2f));
        CreateBox("Wall_West",  new Vector3(-AREA, 2f, 0f), new Vector3(0.5f, 4f, AREA * 2f));

        // ── 장애물 / 플랫폼 ──────────────────────────────────────
        CreateBox("Box_A",      new Vector3(  5f, 0.5f,   5f), new Vector3(1f, 1f, 1f));
        CreateBox("Box_B",      new Vector3( -6f, 1.0f,   3f), new Vector3(2f, 2f, 2f));
        CreateBox("Box_C",      new Vector3(  0f, 0.75f, 10f), new Vector3(4f, 1.5f, 1f));
        CreateBox("Platform_A", new Vector3(  8f, 1.5f,  -8f), new Vector3(4f, 3f, 4f));
        CreateBox("Platform_B", new Vector3(-10f, 0.5f,  -5f), new Vector3(3f, 1f, 3f));
        CreateBox("Ramp",       new Vector3( 12f, 0.5f,   4f), new Vector3(5f, 0.2f, 3f));

        // ── Directional Light ─────────────────────────────────────
        var lightGO = new GameObject("Directional Light");
        var dl = lightGO.AddComponent<Light>();
        dl.type      = LightType.Directional;
        dl.intensity = 1.2f;
        dl.shadows   = LightShadows.Soft;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // ── Player ────────────────────────────────────────────────
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 1f, 0f);

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;
        cc.center = Vector3.zero;

        player.AddComponent<FirstPersonController>();

        // ── Camera (Player 자식) ───────────────────────────────────
        var camGO = new GameObject("Camera");
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();
        camGO.tag = "MainCamera";

        // ── 씬 저장 ───────────────────────────────────────────────
        const string scenePath = "Assets/_Project/Scenes/SimpleLevel.unity";
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
            scenePath);

        Selection.activeGameObject = player;
        Debug.Log($"[SimpleSceneBuilder] {scenePath} 생성 완료!");
    }

    static void CreateBox(string objName, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.position = pos;
        go.transform.localScale = scale;
    }
}
#endif
