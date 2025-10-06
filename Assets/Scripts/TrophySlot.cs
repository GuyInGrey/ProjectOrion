using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TrophySlot : MonoBehaviour
{
    public ItemObject itemToHold;
    public bool isHolding;
    public float collectedAt;

    SpriteRenderer spriteRenderer;
    Light2D light2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        light2d = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isHolding)
        {
            spriteRenderer.sprite = itemToHold.sprite;
            light2d.intensity = QuickRiseSlowFallNormalized((Time.time - collectedAt) * 5f) * 15f;
        }
    }

    public static float QuickRiseSlowFall(float x, float k = 5f)
    {
        return Mathf.Exp(-x) * (1f - Mathf.Exp(-k * x));
    }
    public static float QuickRiseSlowFallNormalized(float x, float k = 5f)
    {
        float peak = Mathf.Pow(k, 1f / (k - 1f)) * (1f - 1f / k);
        return QuickRiseSlowFall(x, k) / peak;
    }
}
