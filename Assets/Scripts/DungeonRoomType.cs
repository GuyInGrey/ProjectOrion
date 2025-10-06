using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "DungeonRoomType", menuName = "Scriptable Objects/Room Type")]
public class DungeonRoomType : ScriptableObject
{
    public string typeName;
    public int minWidth = 10;
    public int maxWidth = 30;
    public int minHeight = 10;
    public int maxHeight = 15;

    public List<TorchProbability> lighting;
    public int torchDensity = 10;
    public float minTorchDistanceTiles = 5;

    public bool usePerlinBackground;
    public List<TileProbability> backgrounds;
    public List<TileProbability> borders;

    public List<ItemProbability> items;
    public List<TreasureTypeProbability> treasureTypes;
    public int minItems = 1;
    public int maxItems = 3;
    public int treasureDensity = 1000;
    public float minTreasureDistanceTiles = 20;

    public int maxDoors = 3;

    public int randomWeight = 10;
}

[Serializable]
public class TileProbability
{
    public TileBase tile;
    public Color color = Color.white;
    public int randomWeight = 10;
    public float darknessVariation = 0f;

    public List<TileBase> overlays;
    public float overlayChance = 0f;
}

[Serializable]
public class TorchProbability
{
    public Color mainColor = Color.orange;
    public Color undertoneColor = Color.yellow;
    public int randomWeight = 10;
}

[Serializable]
public class ItemProbability
{
    public ItemObject item;
    public int randomWeight = 10;
}

[Serializable]
public class TreasureTypeProbability
{
    public TreasureType treasureType;
    public int randomWeight = 10;
}
