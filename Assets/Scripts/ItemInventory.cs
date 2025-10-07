using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    public BoundaryType boundaryType;
    public Transform inventoryParent;
    public GameObject grabbablePrefab;
    public AudioClip soundToPlayOnEnter;

    public float GetBoundarySize()
    {
        if (boundaryType == BoundaryType.Cauldron)
        {
            return GetComponent<CircleCollider2D>().radius;
        }
        else if (boundaryType == BoundaryType.Player)
        {
            var items = GetItemObjects();

            var sizeAvg = AvgItemSize(items);
            return EstimateEnclosingRadius(sizeAvg * .25f / 2f, items.Count, 0.5f);
        }

        throw new System.Exception("Should not be possible to hit this");
    }

    public void RemoveItem(string itemName)
    {
        foreach (Transform child in inventoryParent)
        {
            var item = child.gameObject.GetComponent<Item>().itemObject;
            if (item.itemName == itemName)
            {
                Destroy(child.gameObject);
                break;
            }
        }
    }

    public List<ItemObject> GetItemObjects()
    {
        var toReturn = new List<ItemObject>();
        foreach (Transform child in inventoryParent)
        {
            toReturn.Add(child.gameObject.GetComponent<Item>().itemObject);
        }
        return toReturn;
    }

    public float AvgItemSize(List<ItemObject> items)
    {
        if (items.Count <= 0)
        {
            return 0f;
        }

        return items.Average(i => i.itemSize);
    }

    public void AddItemObject(ItemObject itemObject)
    {
        var grabbable = Instantiate(grabbablePrefab, inventoryParent);
        grabbable.transform.position = inventoryParent.position;
        var item = grabbable.GetComponent<Item>();
        item.itemObject = itemObject;
        item.inventory = this;
        PlaySound();
    }

    public void AddItemObject(GameObject item)
    {
        if (item.transform.parent == inventoryParent)
        {
            return;
        }
        item.transform.parent = inventoryParent;
        item.GetComponent<Item>().inventory = this;
        PlaySound();
    }

    void PlaySound()
    {
        if (soundToPlayOnEnter == null)
        {
            return;
        }
        Debug.Log("Played sound");
        AudioSource.PlayClipAtPoint(soundToPlayOnEnter, Camera.main.transform.position, 1);
    }

    private float EstimateEnclosingRadius(float smallRadius, int count, float marginInSmallRadii = 0.5f)
    {
        if (count <= 0 || smallRadius <= 0f)
        {
            return 0f;
        }

        var hexFactory = 1.0500751358f;
        var R = (hexFactory * smallRadius * Mathf.Sqrt(count)) + (marginInSmallRadii * smallRadius);
        return (float)R;
    }

    public void Clear()
    {
        foreach (Transform child in inventoryParent)
        {
            Destroy(child.gameObject);
        }
    }
}

public enum BoundaryType
{
    Player = 0,
    Cauldron = 1,
}
