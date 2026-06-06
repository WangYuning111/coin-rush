using UnityEngine;
using UnityEngine.SceneManagement;

public static class RuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureGlobalManagers()
    {
        if (GameManager.Instance == null)
        {
            GameObject go = new GameObject("GlobalManagers");
            var gm = go.AddComponent<GameManager>();
            go.AddComponent<AudioManager>();
            var pm = go.AddComponent<PauseMenu>();

            var scene = SceneManager.GetActiveScene();
            gm.OnSceneLoaded(scene, LoadSceneMode.Single);
            pm.OnSceneLoaded(scene, LoadSceneMode.Single);
        }
    }
}
