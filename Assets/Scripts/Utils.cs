using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class Utils
{
    public class WeightedItem<T>
    {
        public T Item { get; set; }
        public int RandomWeight { get; set; }
    }

    private static T GetRandomByWeights<T>(this IEnumerable<WeightedItem<T>> valsEnumerable, float? random = null)
    {
        random = random.HasValue ? Mathf.Clamp01(random.Value) : random;

        var vals = valsEnumerable.ToList();
        if (vals == null || vals.Count == 0)
        {
            throw new System.ArgumentException("List cannot be empty.");
        }

        var totalWeight = 0;
        foreach (var v in vals)
        {
            totalWeight += v.RandomWeight;
        }
        var selectedWeight = random.HasValue ? (random * totalWeight) : Random.Range(1, totalWeight + 1);
        var passedWeight = 0;
        foreach (var v in vals)
        {
            passedWeight += v.RandomWeight;
            if (passedWeight >= selectedWeight)
            {
                return v.Item;
            }
        }

        throw new System.Exception("Uh... this error should not be possible.");
    }

    public static T GetRandomByWeights<T>(this List<T> values, System.Func<T, int> getWeight, float? random = null)
    {
        var weights = values.Select(v => new WeightedItem<T>
        {
            Item = v,
            RandomWeight = getWeight(v)
        });

        return GetRandomByWeights(weights, random);
    }

    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return EventSystem.current.IsPointerOverGameObject(Touchscreen.current.primaryTouch.touchId.ReadValue());
        }

        return EventSystem.current.IsPointerOverGameObject(-1);
    }
}

