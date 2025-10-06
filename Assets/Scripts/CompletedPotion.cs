using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CompletedPotion : MonoBehaviour
{
    public PotionType PotionType;
    public SpriteRenderer Fore;
    public SpriteRenderer Back;

    public ItemObject vial;
    public float craftedAt = 0f;

    Light2D light2d;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Back.color = PotionType.color;
        if (light2d == null)
        {
            light2d = gameObject.AddComponent<Light2D>();
            light2d.lightType = Light2D.LightType.Point;
            light2d.color = Color.white;
            light2d.pointLightInnerRadius = 1f;
            light2d.pointLightOuterRadius = 2f;
            light2d.intensity = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (vial != null)
        {
            Fore.sprite = vial.sprite;
            Back.sprite = vial.vialLiquidSprite;
            Fore.enabled = true;
            Back.enabled = true;

            light2d.intensity = QuickRiseSlowFallNormalized((Time.time - craftedAt) * 5f) * 15f;

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
