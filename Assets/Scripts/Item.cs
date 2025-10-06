using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemObject itemObject;
    public ItemInventory inventory;

    [SerializeField] float pullForce = 50f;

    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    //PlayerMovement playerMovement;

    float timeSinceCollected = 0;
    bool beingHeld;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.tag = "GrabbableItem";
        if (ItemHolder.Holding == gameObject)
        {
            beingHeld = true;
        }
        else if (beingHeld)
        {
            timeSinceCollected = 0f;
            beingHeld = false;
        }

        if (inventory != null)
        {
            gameObject.layer = 6;
            GetComponent<CircleCollider2D>().isTrigger = beingHeld;
            timeSinceCollected += Time.deltaTime;
        }

        spriteRenderer.sprite = itemObject.sprite;
        var divider = timeSinceCollected > 0 ? transform.parent.parent.localScale.x : 1f;
        transform.localScale = new Vector3(.25f * itemObject.itemSize, .25f * itemObject.itemSize, 1) / divider;

        if (timeSinceCollected > 0 && !beingHeld)
        {
            var boundary = inventory.GetBoundarySize();

            rb.gravityScale = 0;

            var diff = transform.parent.position - transform.position;
            var pull = pullForce;
            pull *= boundary / Mathf.Max(boundary - diff.magnitude, 0.05f);
            rb.AddForce(new Vector2(diff.x * pull, diff.y * pull));

            if (diff.magnitude > boundary && timeSinceCollected > 0.25f)
            {
                transform.position = transform.parent.position - (diff.normalized * boundary);
            }

            spriteRenderer.color = new Color(1, 1, 1, diff.magnitude <= boundary || timeSinceCollected > .25f ? 0.85f : 1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForInventory(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckForInventory(collision.gameObject);
    }

    void CheckForInventory(GameObject obj)
    {
        var inventory = obj.GetComponent<ItemInventory>();
        if (inventory != null)
        {
            inventory.AddItemObject(gameObject);
        }
    }
}
