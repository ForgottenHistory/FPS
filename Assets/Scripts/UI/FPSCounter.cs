using TMPro;
using UnityEngine;


public class FPSCounter : MonoBehaviour
{
    public float updateInterval = 0.5f;
    
    private TextMeshProUGUI fpsText;
    private float deltaTime = 0.0f;
    private float nextUpdate = 0.0f;

    void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // FPS Counter
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        if (Time.unscaledTime > nextUpdate)
        {
            float fps = 1.0f / deltaTime;
            fpsText.text = string.Format("{0:0.} fps", fps);

            nextUpdate = Time.unscaledTime + updateInterval;
        }
    }
}
