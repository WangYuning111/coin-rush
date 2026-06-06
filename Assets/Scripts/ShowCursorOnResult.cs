using UnityEngine;

public class ShowCursorOnResult : MonoBehaviour
{
    [SerializeField] private bool showCursor = true;
    [SerializeField] private CursorLockMode lockState = CursorLockMode.None;

    void Start()
    {
        Cursor.visible = showCursor;
        Cursor.lockState = lockState;
        Debug.Log($"[{gameObject.scene.name}] Mouse cursor visible: {Cursor.visible}");
    }
}
