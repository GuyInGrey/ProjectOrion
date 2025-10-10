using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class CameraLetterbox : MonoBehaviour
{
    [Tooltip("Target aspect ratio (width / height). Example: 16:9 = 1.7777f")]
    public float targetAspect = 16f / 9f;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyLetterbox();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            ApplyLetterbox();
        }
    }

    void ApplyLetterbox()
    {
        if (cam == null)
        {
            cam = GetComponent<Camera>();
        }

        var windowAspect = (float)Screen.width / Screen.height;
        var scale = windowAspect / targetAspect;

        if (scale < 1f)
        {
            var normalizedHeight = scale;
            var barHeight = (1f - normalizedHeight) / 2f;
            cam.rect = new Rect(0f, barHeight, 1f, normalizedHeight);
        }
        else
        {
            var normalizedWidth = 1f / scale;
            var barWidth = (1f - normalizedWidth) / 2f;
            cam.rect = new Rect(barWidth, 0f, normalizedWidth, 1f);
        }

        cam.backgroundColor = Color.black;
        cam.clearFlags = CameraClearFlags.SolidColor;

        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }
}
