using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemHolderDeleteZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsHovering;

    public Button button;

    public void OnPointerEnter(PointerEventData _)
    {
        IsHovering = true;
    }
    public void OnPointerExit(PointerEventData _)
    {
        IsHovering = false;
    }

    private void Update()
    {
        button.interactable = ItemHolder.Holding != null;
    }
}
