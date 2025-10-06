using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Treasure : MonoBehaviour
{
    public TreasureType Type = TreasureType.Chest;
    public List<ItemProbability> items;
    public int minItems;
    public int maxItems;

    public GameObject grabbablePrefab;
    public ItemInventory playerInventory;

    Animator animator;
    Light2D light2d;

    bool collected;
    float timeSinceCollected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        light2d = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsCollected", collected);
        animator.SetInteger("TreasureType", (int)Type);

        if (collected)
        {
            timeSinceCollected += Time.deltaTime;
            light2d.enabled = true;
            light2d.intensity = QuickRiseSlowFallNormalized(timeSinceCollected * 5f) * 15f;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name != "Player" || animator == null || collected)
        {
            return;
        }

        Debug.Log("Treasure collected");

        collected = true;

        var itemCount = Random.Range(minItems, maxItems + 1);
        for (var i = 0; i < itemCount; i++)
        {
            var itemObj = items.GetRandomByWeights(i => i.randomWeight);
            var grabbable = Instantiate(grabbablePrefab);
            grabbable.transform.position = transform.position;
            var item = grabbable.GetComponent<Item>();
            item.itemObject = itemObj.item;
            playerInventory.AddItemObject(grabbable);
        }
    }
}

public enum TreasureType
{
    Chest = 0,
    Pedestal = 1,
    Pot = 2,
}
