using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemObject itemObject;
    [SerializeField] float pullForce = 50f;

    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    PlayerMovement playerMovement;

    float timeSinceCollected = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent != null && transform.parent.name == "Inventory")
        {
            timeSinceCollected += Time.deltaTime;

            if (playerMovement == null)
            {
                playerMovement = transform.parent.parent.GetComponent<PlayerMovement>();
            }
        }

        spriteRenderer.sprite = itemObject.sprite;
        var divider = timeSinceCollected > 0 ? transform.parent.parent.localScale.x : 1f;
        transform.localScale = new Vector3(.25f * itemObject.itemSize, .25f * itemObject.itemSize, 1) / divider;

        if (timeSinceCollected > 0)
        {
            var boundary = playerMovement.GetStomachRadius();

            rb.gravityScale = 0;

            var diff = transform.parent.position - transform.position;
            var pull = pullForce;
            pull *= boundary / Mathf.Max(boundary - diff.magnitude, 0.05f);
            rb.AddForce(new Vector2(diff.x * pull, diff.y * pull));

            if (diff.magnitude > boundary && timeSinceCollected > 0.25f)
            {
                transform.position = transform.parent.position - (diff.normalized * boundary);
            }

            spriteRenderer.color = new Color(1, 1, 1, diff.magnitude <= boundary || timeSinceCollected > .25f ? 0.5f : 1f);
        }
    }
}
