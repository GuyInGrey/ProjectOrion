using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float acceleration = 2f;
    [SerializeField] float maxSpeed = 2f;
    [SerializeField] float jumpPower = 100f;

    [SerializeField] Transform inventory;

    Vector2 moveInput = Vector2.zero;
    Rigidbody2D rb;
    BoxCollider2D boxCollider;
    int jumpCooldown = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (jumpCooldown > 0)
        {
            jumpCooldown -= 1;
        }

        if (boxCollider.IsTouchingLayers() && rb.linearVelocityY < 3f)
        {
            if (moveInput.y > 0f && jumpCooldown <= 0)
            {
                rb.AddForce(new Vector2(0, jumpPower));
                jumpCooldown += 30;
            }
        }

        rb.AddForce(new Vector2(acceleration * moveInput.x, 0).normalized);
        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocityX, -maxSpeed, maxSpeed), rb.linearVelocityY);

        var targetScale = Mathf.Max(4 * GetStomachRadius(), 1) * Vector3.one;
        transform.localScale = transform.localScale - ((transform.localScale - targetScale) * Time.deltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var obj = collision.gameObject;
        if (obj.CompareTag("GrabbableItem"))
        {
            obj.layer = 6;
            obj.transform.parent = inventory;
        }
    }

    public float GetStomachRadius()
    {
        var sizeAvg = AvgInvItemSize();
        return EstimateEnclosingRadius(sizeAvg * .25f / 2f, inventory.childCount, 0.5f);
    }

    public float AvgInvItemSize()
    {
        if (inventory.childCount <= 0)
        {
            return 0f;
        }

        var sizes = new List<float>();
        foreach (Transform child in inventory)
        {
            var item = child.GetComponent<Item>();
            sizes.Add(item.itemObject.itemSize);
        }

        var sizeAvg = sizes.Average();
        return sizeAvg;
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
}
