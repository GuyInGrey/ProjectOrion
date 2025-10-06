using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "Potion Type", menuName = "Scriptable Objects/Potion Type")]
public class PotionType : ScriptableObject
{
    public string potionName = "Generic Potion";
    public List<ItemObject> ingredients;
    public Color color = Color.red;
}
