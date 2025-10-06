using NUnit.Framework;

using System.Collections.Generic;

using UnityEngine;

public class TrophyHandler : MonoBehaviour
{
    public ItemInventory playerInventory;

    public static List<(string name, float collectedAt)> trophiesCollected = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var t in trophiesCollected)
        {
            foreach (Transform child in transform)
            {
                var trophySlot = child.GetComponent<TrophySlot>();
                if (trophySlot.itemToHold.itemName == t.name)
                {
                    trophySlot.collectedAt = t.collectedAt;
                    trophySlot.isHolding = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        var inv = playerInventory.GetItemObjects();

        foreach (var item in inv)
        {
            foreach (Transform child in transform)
            {
                var trophySlot = child.GetComponent<TrophySlot>();
                if (trophySlot.itemToHold.itemName == item.itemName)
                {
                    playerInventory.RemoveItem(item.itemName);

                    if (!trophySlot.isHolding)
                    {
                        trophySlot.isHolding = true;
                        trophySlot.collectedAt = Time.time;
                        trophiesCollected.Add((item.itemName, Time.time));
                    }
                }
            }
        }
    }
}
