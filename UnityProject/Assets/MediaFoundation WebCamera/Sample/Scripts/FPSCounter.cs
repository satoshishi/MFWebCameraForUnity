using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public Vector2 position = new(10, 10);
    public int fontSize = 16;
    public Color fontColor = Color.white;
    public bool showMilliseconds = false;

    private float deltaTime = 0.0f;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new();

        Rect rect = new(position.x, position.y, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = fontSize;
        style.normal.textColor = fontColor;

        float fps = 1.0f / deltaTime;
        string text = showMilliseconds
            ? $"{fps:F1} FPS ({deltaTime * 1000.0f:F1} ms)"
            : $"{fps:F1} FPS";

        GUI.Label(rect, text, style);
    }
}
