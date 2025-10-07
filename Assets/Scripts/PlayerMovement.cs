using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private static List<ItemObject> inventoryState = new();

    [Header("Objects")]
    public Animator animator;
    public List<SpriteRenderer> spriteLayers;
    public GameObject grabbablePrefab;
    public ItemInventory inventory;
    public AudioSource walkingAudioSource;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Sampling")]
    public LayerMask wallMask;
    public float sampleRadius = 0.6f;
    public int sampleRays = 12;

    [Header("Movement")]
    public float stickForce = 15f;
    public float acceleration = 200f;
    public float normalSmoothing = 0.6f;    // 0..1 (higher = smoother)
    public float mouseInfluenceRadius = 6f; // used for distance scale
    public float secondsBetweenJumps = 0.5f;
    public float jumpForce = 100f;
    public float maxSpeed = 2f;

    [Header("Clamping")]
    public float maxAngleDeg = 60f; // angle cone around each sample line

    [Header("Debug")]
    public bool debugDraw = true;
    private bool leftClickInput = false;
    private bool rightClickInput = false;
    private float sinceLastJump = 0f;
    private bool touchedWallsSinceJump = true;

    Rigidbody2D rb;
    Vector2 smoothedNormal = Vector2.up;

    void Start()
    {

        // Disable VSync to allow targetFrameRate to take effect
        QualitySettings.vSyncCount = 0;
        // Set the target frame rate
        Application.targetFrameRate = 60;

        rb = GetComponent<Rigidbody2D>();
        animator.SetInteger("State", 0);

        foreach (var itemObject in inventoryState)
        {
            inventory.AddItemObject(itemObject);
        }
    }

    void Update()
    {
        sinceLastJump += Time.deltaTime;

        // ------------- mouse input (world space) -------------
        Vector2 mouseWorld = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 toMouse = mouseWorld - (Vector2)transform.position;
        float mouseDist = toMouse.magnitude;
        Vector2 desiredDir = mouseDist > 1e-6f ? toMouse.normalized : Vector2.zero;
        float distanceScale = Mathf.Clamp01(mouseDist / mouseInfluenceRadius);

        // --- sample normals and gather sample ray directions ---
        List<Vector2> hitDirs;
        Vector2? maybeAvgNormal = SampleNormalsAndDirs((Vector2)transform.position, sampleRadius * transform.localScale.x, sampleRays, wallMask, out hitDirs);
        if (!maybeAvgNormal.HasValue || hitDirs.Count == 0)
        {
            // no wall nearby -> normal physics fallback
            rb.AddForce(Physics2D.gravity * rb.mass, ForceMode2D.Force);
            return;
        }

        Vector2 avgNormal = maybeAvgNormal.Value.normalized;
        smoothedNormal = Vector2.Lerp(smoothedNormal, avgNormal, 1f - normalSmoothing).normalized;
        Vector2 tangent = new Vector2(-smoothedNormal.y, smoothedNormal.x);

        // --- clamp desiredDir to be within maxAngleDeg of one of the sample lines ---
        if (desiredDir.sqrMagnitude > 1e-6f)
            desiredDir = ClampDirectionToSampleLines(desiredDir, hitDirs, maxAngleDeg);

        // --- apply stick and movement forces ---
        rb.AddForce(-smoothedNormal * stickForce, ForceMode2D.Force); // stick inwards
        if (leftClickInput && sinceLastJump > .1f && !Utils.IsPointerOverUI() && ItemHolder.Holding == null)
        {
            rb.AddForce(acceleration * distanceScale * desiredDir, ForceMode2D.Force); // move toward (clamped) mouse dir
            animator.SetInteger("State", 1);
            walkingAudioSource.enabled = true;
        }
        else if (touchedWallsSinceJump)
        {
            animator.SetInteger("State", 0);
            walkingAudioSource.enabled = false;
        }

        if (sinceLastJump > secondsBetweenJumps && rightClickInput && !Utils.IsPointerOverUI() && ItemHolder.Holding == null)
        {
            sinceLastJump = 0f;
            touchedWallsSinceJump = false;
            rb.linearVelocity = smoothedNormal.normalized * jumpForce;
            animator.SetInteger("State", 2);

            AudioSource.PlayClipAtPoint(jumpClip, Camera.main.transform.position, 1);
            walkingAudioSource.enabled = false;
        }

        if (rb.linearVelocity.magnitude > maxSpeed && touchedWallsSinceJump && sinceLastJump > 0.1f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // --- debug draws ---
        if (debugDraw)
        {
            Debug.DrawRay(transform.position, -smoothedNormal * 0.6f, Color.green);
            Debug.DrawRay(transform.position, tangent * 0.6f, Color.yellow);
            Debug.DrawRay(transform.position, desiredDir * 1.0f, Color.cyan);
            foreach (var d in hitDirs)
                Debug.DrawRay(transform.position, d * sampleRadius * transform.localScale.x, Color.red);
        }

        float targetAngle = Mathf.Atan2(smoothedNormal.y, smoothedNormal.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 8f * Time.deltaTime);

        Vector2 slimeRight = new Vector2(-smoothedNormal.y, smoothedNormal.x);

        // Project movement direction onto slime's local right axis
        float movementDot = Vector2.Dot(desiredDir, slimeRight);

        // Flip depending on which side of the slime the movement is on
        if (movementDot > 0.1f)
            spriteLayers.ForEach(sr => sr.flipX = true);
        else if (movementDot < -0.1f)
            spriteLayers.ForEach(sr => sr.flipX = false);

        var targetScale = Mathf.Max(4 * inventory.GetBoundarySize(), 1) * Vector3.one;
        transform.localScale = transform.localScale - ((transform.localScale - targetScale) * Time.deltaTime * 3f);

        inventoryState.Clear();
        foreach (var item in inventory.GetItemObjects())
        {
            inventoryState.Add(item);
        }
    }

    Vector2 ClampDirectionToSampleLines(Vector2 dir, List<Vector2> sampleLines, float maxAngleDeg)
    {
        float absMax = Mathf.Abs(maxAngleDeg);
        float bestAngle = float.MaxValue;
        Vector2 bestSample = Vector2.right;
        int bestSign = 1;

        // If any sample line already is within cone, keep original dir
        foreach (var s in sampleLines)
        {
            float angle = Vector2.SignedAngle(s, dir); // angle from sample->dir
            if (Mathf.Abs(angle) <= absMax)
                return dir; // good; within at least one cone

            float absAngle = Mathf.Abs(angle);
            if (absAngle < bestAngle)
            {
                bestAngle = absAngle;
                bestSample = s;
                bestSign = angle >= 0f ? 1 : -1;
            }
        }

        // Not within any cone: clamp to nearest sample line rotated by ±maxAngleDeg toward dir
        float clampAngle = bestSign * absMax;
        Vector2 clamped = (Quaternion.Euler(0f, 0f, clampAngle) * bestSample).normalized;
        return clamped;
    }

    Vector2? SampleNormalsAndDirs(Vector2 origin, float radius, int samples, LayerMask mask, out List<Vector2> hitDirections)
    {
        hitDirections = new List<Vector2>();
        float totalWeight = 0f;
        Vector2 weightedNormal = Vector2.zero;

        for (int i = 0; i < samples; i++)
        {
            float angle = (float)i / samples * Mathf.PI * 2f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var hit = Physics2D.Raycast(origin, dir, radius, mask);
            if (hit.collider != null)
            {
                // record the sample line direction (from origin toward the hit)
                hitDirections.Add(dir.normalized);

                // weight normal by proximity (closer => bigger weight)
                float w = Mathf.Clamp01(1f - (hit.distance / radius));
                weightedNormal += hit.normal * w;
                totalWeight += w;

                if (debugDraw)
                    Debug.DrawLine(origin, hit.point, Color.red, 0.02f);
            }
        }

        if (totalWeight <= 0f)
            return null;
        Vector2 avg = (weightedNormal / totalWeight).normalized;
        return avg;
    }

    public void OnMove(InputAction.CallbackContext context)
    {

    }

    public void OnLeftClick(InputAction.CallbackContext context)
    {
        leftClickInput = context.ReadValue<float>() == 1;
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        rightClickInput = context.ReadValue<float>() == 1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var obj = collision.gameObject;
        if (!obj.CompareTag("GrabbableItem"))
        {
            if (!touchedWallsSinceJump)
            {
                AudioSource.PlayClipAtPoint(landClip, Camera.main.transform.position, 1);
            }

            touchedWallsSinceJump = true;
        }
    }
}
