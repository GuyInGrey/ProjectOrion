using UnityEngine;

[CreateAssetMenu(fileName = "ItemObject", menuName = "Scriptable Objects/ItemObject")]
public class ItemObject : ScriptableObject
{
    public string itemName;
    public string description;
    public RarityWeight rarity = RarityWeight.Common;
    public ItemType type = ItemType.Ingredient;
    public float itemSize = 1;
    public Sprite sprite;
}

public enum ItemType
{
    Ingredient,
    Vial,
    Stopper,
    HeroRelic,
    Currency,
    SlimeTablet,
}

public enum RarityWeight
{
    Common = 1000,
    Rare = 50,
    Relic = 10,
    Mythic = 5,
}
