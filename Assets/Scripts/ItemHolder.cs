using UnityEngine;
using UnityEngine.InputSystem;

public class ItemHolder : MonoBehaviour
{
    private Camera cam;
    private Rigidbody2D grabbedBody;
    private Vector2 grabOffset;

    public LayerMask grabFilterLayer;

    void Awake() => cam = Camera.main;

    void Update()
    {
        Holding = grabbedBody == null ? null : grabbedBody.gameObject;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            var worldPos = (Vector2)cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 100, grabFilterLayer);
            if (hit.collider && hit.collider.attachedRigidbody)
            {
                grabbedBody = hit.collider.attachedRigidbody;
                grabOffset = (Vector2)grabbedBody.transform.position - worldPos;
            }
        }

        if (Mouse.current.leftButton.isPressed && grabbedBody)
        {
            var target = (Vector2)cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) + grabOffset;
            grabbedBody.transform.position = Vector2.Lerp(grabbedBody.position, target, 15f * Time.deltaTime);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (ItemHolderDeleteZone.IsHovering)
            {
                Destroy(grabbedBody.gameObject);
            }

            grabbedBody = null;
        }
    }

    public static GameObject Holding;
}
