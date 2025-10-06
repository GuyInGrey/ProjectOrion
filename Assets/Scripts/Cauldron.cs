using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Cauldron : MonoBehaviour
{
    public ItemInventory inventory;
    public List<PotionType> craftablePotionTypes;
    public List<CompletedPotion> completedPotionSlots;

    public static List<(float craftedAt, string potName, ItemObject vial)> CraftedPotions = new();

    private static List<ItemObject> inventoryState = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var itemObject in inventoryState)
        {
            inventory.AddItemObject(itemObject);
        }

        foreach (var c in CraftedPotions)
        {
            var matching = completedPotionSlots.First(cp => cp.PotionType.potionName == c.potName);
            matching.vial = c.vial;
            matching.craftedAt = c.craftedAt;
        }
    }

    // Update is called once per frame
    void Update()
    {
        inventoryState.Clear();
        foreach (var item in inventory.GetItemObjects())
        {
            inventoryState.Add(item);
        }

        var inv = inventory.GetItemObjects();

        var vials = inv.Where(i => i.type == ItemType.Vial).ToList();
        if (vials.Count() != 1)
        {
            return;
        }
        var vial = vials[0];

        var ingredients = inv.Where(i => i.type == ItemType.Ingredient).ToList();
        var ingredientCounts = ingredients.GroupBy(i => i.itemName).Select(i => (i.First(), i.Count())).ToDictionary(d => d.Item1, d => d.Item2);

        foreach (var p in craftablePotionTypes)
        {
            if (p.ingredients.Count != ingredients.Count)
            {
                continue;
            }
            
            var potCounts = p.ingredients.GroupBy(i => i.itemName).Select(i => (i.First(), i.Count())).ToDictionary(d => d.Item1, d => d.Item2);

            var matches = true;
            foreach (var c in potCounts)
            {
                if (!ingredientCounts.ContainsKey(c.Key))
                {
                    matches = false;
                    break;
                }
                if (ingredientCounts[c.Key] != potCounts[c.Key])
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                inventory.Clear();
                Debug.Log("Crafted: " + p.potionName + "; " + vial.itemName);

                if (CraftedPotions.Any(t => t.potName == p.potionName))
                {
                    return;
                }

                CraftedPotions.Add((Time.time, p.potionName, vial));

                var matching = completedPotionSlots.First(cp => cp.PotionType.potionName == p.potionName);
                matching.vial = vial;
                matching.craftedAt = Time.time;
            }
        }
    }
}
