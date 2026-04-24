using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class PlayerConfig
{
    // Defaults if JSON is missing fields.
    public int PlayerSprite = 1;
    public float JumpMultiplier = 1f;
    public float SpeedMultiplier = 1f;
}

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private GameObject[] sprites;
    [SerializeField] private Transform groundDetect;
    [SerializeField] private bool isGrounded; // Just so we can see in Editor.
    [SerializeField] private float moveForce;
    [SerializeField] private float jumpForce;

    public int ActiveSprite { get; set; }
    public LayerMask groundLayer;

    private const string ConfigFileName = "player_config.json";

    private float groundCheckWidth = 0.75f;
    private float groundCheckHeight = 0.1f;

    private Animator an;
    private Rigidbody2D rb;

    private float baseMoveForce;
    private float baseJumpForce;

    private float lastX;
    private float currX;

    private void Awake()
    {
        // Cache base values so multipliers always apply consistently.
        baseMoveForce = moveForce;
        baseJumpForce = jumpForce;
    }

    private IEnumerator Start()
    {
        // Loads config (no coroutine). If it fails, defaults to 1s.
        yield return StartCoroutine(LoadPlayerConfigOrDefaultOnes());

        // Activate correct sprite + animator.
        SetActiveSprite(ActiveSprite);

        // Always start in air.
        isGrounded = false;

        rb = GetComponent<Rigidbody2D>();

        lastX = currX = transform.position.x;
    }

    private void Update()
    {
        GroundedCheck();

        lastX = transform.position.x;

        // Horizontal movement.
        float moveInput = Input.GetAxis("Horizontal");
        float moveInputRaw = Input.GetAxisRaw("Horizontal"); // -1, 0, or 1.

        if (an != null)
            an.SetBool("isMoving", Mathf.Abs(moveInputRaw) > 0f);

        // Apply horizontal velocity (keep current vertical velocity).
        if (rb != null)
            rb.linearVelocity = new Vector2(moveInput * moveForce * Time.fixedDeltaTime, rb.linearVelocity.y);

        currX = transform.position.x;

        // Flip the player.
        if (moveInputRaw != 0)
            transform.localScale = new Vector3(moveInputRaw, 1, 1);

        // Jump.
        if (isGrounded && Input.GetButtonDown("Jump") && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * Time.fixedDeltaTime);
            Game.Instance.SOMA.PlaySound("Jump");
        }

        // Swap between sprites.
        if (Input.GetKey(KeyCode.Alpha1))
            SetActiveSprite(0);
        else if (Input.GetKey(KeyCode.Alpha2))
            SetActiveSprite(1);
        else if (Input.GetKey(KeyCode.Alpha3))
            SetActiveSprite(2);
    }

    private void GroundedCheck()
    {
        if (groundDetect == null) return;

        isGrounded = Physics2D.OverlapBox(
            groundDetect.position,
            new Vector2(groundCheckWidth, groundCheckHeight),
            0f,
            groundLayer
        );

        if (an != null)
            an.SetBool("isJumping", !isGrounded);
    }

    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0f;
    }

    public Vector2 GetVelocity()
    {
        var body = GetComponent<Rigidbody2D>();
        return body != null ? body.linearVelocity : Vector2.zero;
    }

    public bool IsStopped()
    {
        return currX == lastX;
    }

    public void SetActiveSprite(int num)
    {
        if (sprites == null || sprites.Length == 0) return;

        if (num < 0 || num >= sprites.Length) return;

        ActiveSprite = num;

        foreach (GameObject g in sprites)
        {
            if (g != null) g.SetActive(false);
        }

        if (sprites[ActiveSprite] != null)
        {
            sprites[ActiveSprite].SetActive(true);
            an = sprites[ActiveSprite].GetComponent<Animator>();
        }
        else
        {
            an = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // 2D.
    {
        if (other.gameObject.tag == "Ring")
        {
            Game.Instance.SOMA.PlaySound("Pickup");
            Game.Instance.AddScore();
            Destroy(other.gameObject);
        }
    }


    private IEnumerator LoadPlayerConfigOrDefaultOnes()
    {
        // Defaults if anything fails.
        ActiveSprite = 1;
        moveForce = baseMoveForce * 1f;
        jumpForce = baseJumpForce * 1f;

        string path = Path.Combine(Application.streamingAssetsPath, ConfigFileName);

        using UnityWebRequest req = UnityWebRequest.Get(path);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Could not load player config. Using defaults " + req.error);
            ApplyConfig(new PlayerConfig());
            yield break;
        }

        PlayerConfig cfg;
        TryLoadConfigFromPath(req, out cfg);
    }

    private void TryLoadConfigFromPath(UnityWebRequest req, out PlayerConfig cfg)
    {
        cfg = null;

        try
        {
            cfg = JsonUtility.FromJson<PlayerConfig>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"player_config.json load/parse failed. Using defaults. {e.Message}");
        }
        if (cfg == null) 
        {
            cfg = new PlayerConfig();
        }
        ApplyConfig(new PlayerConfig());

    }

    private void ApplyConfig(PlayerConfig cfg)
    {
        if (sprites != null && sprites.Length > 0)
            ActiveSprite = Mathf.Clamp(cfg.PlayerSprite, 1, sprites.Length) - 1;
        else
            ActiveSprite = cfg.PlayerSprite - 1;

        moveForce = baseMoveForce * Mathf.Max(0f, cfg.SpeedMultiplier);
        jumpForce = baseJumpForce * Mathf.Max(0f, cfg.JumpMultiplier);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundDetect == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundDetect.position, new Vector3(groundCheckWidth, groundCheckHeight, 0f));
    }
}
