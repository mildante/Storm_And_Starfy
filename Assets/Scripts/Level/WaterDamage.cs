using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterDamage : MonoBehaviour
{
    [SerializeField] private float damageDelay = 3f;
    [SerializeField] private float sinkSpeed = 0.65f;
    [SerializeField] private float waterSlowdown = 8f;
    [SerializeField] private float horizontalDamping = 8f;
    [SerializeField] private Tilemap waterTilemap;

    private readonly Dictionary<GameObject, Coroutine> damageCoroutines =
        new Dictionary<GameObject, Coroutine>();
    private readonly Dictionary<GameObject, WaterState> waterStates =
        new Dictionary<GameObject, WaterState>();
    private readonly HashSet<GameObject> detectedPlayers =
        new HashSet<GameObject>();

    private sealed class WaterState
    {
        public Rigidbody2D Rigidbody;
        public float GravityScale;
        public PlayerMovement PlayerMovement;
        public bool PlayerMovementWasEnabled;
        public StarfyMovement StarfyMovement;
        public bool StarfyMovementWasEnabled;
    }

    private void Awake()
    {
        if (waterTilemap == null)
        {
            waterTilemap = GetComponent<Tilemap>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryStartWaterEffect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryStartWaterEffect(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (TryGetPlayer(other, out GameObject player))
        {
            StopDamageTimer(player);
            ExitWater(player);
        }
    }

    private void FixedUpdate()
    {
        DetectPlayersByTilemap();

        foreach (WaterState state in waterStates.Values)
        {
            if (state.Rigidbody == null)
                continue;

            Vector2 velocity = state.Rigidbody.linearVelocity;
            float targetVerticalVelocity = -Mathf.Abs(sinkSpeed);

            velocity.x = Mathf.MoveTowards(
                velocity.x,
                0f,
                horizontalDamping * Time.fixedDeltaTime);

            velocity.y = Mathf.MoveTowards(
                velocity.y,
                targetVerticalVelocity,
                waterSlowdown * Time.fixedDeltaTime);

            state.Rigidbody.gravityScale = 0f;
            state.Rigidbody.linearVelocity = velocity;
        }
    }

    private void OnDisable()
    {
        foreach (Coroutine damageCoroutine in damageCoroutines.Values)
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
        }

        damageCoroutines.Clear();

        foreach (GameObject player in new List<GameObject>(waterStates.Keys))
        {
            ExitWater(player);
        }
    }

    private IEnumerator DamageAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(damageDelay);

        ExitWater(player);

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage();
        }

        damageCoroutines.Remove(player);
    }

    private void TryStartWaterEffect(Collider2D other)
    {
        if (!TryGetPlayer(other, out GameObject player) || !IsOwnedByLocalPlayer(player))
            return;

        StartWaterEffect(player);
    }

    private void DetectPlayersByTilemap()
    {
        if (waterTilemap == null)
            return;

        detectedPlayers.Clear();

        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsInactive.Exclude);

        foreach (PlayerHealth health in players)
        {
            if (health == null)
                continue;

            GameObject player = health.gameObject;

            if (!IsPlayer(player) || !IsOwnedByLocalPlayer(player))
                continue;

            if (IsPlayerInsideWaterTile(player))
            {
                detectedPlayers.Add(player);
                StartWaterEffect(player);
            }
        }

        foreach (GameObject player in new List<GameObject>(waterStates.Keys))
        {
            if (player == null || detectedPlayers.Contains(player))
                continue;

            StopDamageTimer(player);
            ExitWater(player);
        }
    }

    private void StartWaterEffect(GameObject player)
    {
        EnterWater(player);

        if (!damageCoroutines.ContainsKey(player))
        {
            damageCoroutines[player] =
                StartCoroutine(DamageAfterDelay(player));
        }
    }

    private bool IsPlayerInsideWaterTile(GameObject player)
    {
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        if (playerCollider == null)
            return HasWaterTileAt(player.transform.position);

        Bounds bounds = playerCollider.bounds;
        float insetX = bounds.extents.x * 0.45f;
        float insetY = bounds.extents.y * 0.25f;

        return
            HasWaterTileAt(bounds.center) ||
            HasWaterTileAt(new Vector2(bounds.center.x, bounds.min.y + insetY)) ||
            HasWaterTileAt(new Vector2(bounds.center.x - insetX, bounds.center.y)) ||
            HasWaterTileAt(new Vector2(bounds.center.x + insetX, bounds.center.y));
    }

    private bool HasWaterTileAt(Vector3 worldPosition)
    {
        Vector3Int cellPosition = waterTilemap.WorldToCell(worldPosition);

        return waterTilemap.HasTile(cellPosition);
    }

    private void EnterWater(GameObject player)
    {
        if (waterStates.ContainsKey(player))
            return;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        StarfyMovement starfyMovement = player.GetComponent<StarfyMovement>();

        WaterState state = new WaterState
        {
            Rigidbody = rb,
            GravityScale = rb != null ? rb.gravityScale : 1f,
            PlayerMovement = playerMovement,
            PlayerMovementWasEnabled = playerMovement != null && playerMovement.enabled,
            StarfyMovement = starfyMovement,
            StarfyMovementWasEnabled = starfyMovement != null && starfyMovement.enabled
        };

        waterStates[player] = state;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (starfyMovement != null)
        {
            starfyMovement.enabled = false;
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f));
        }
    }

    private void ExitWater(GameObject player)
    {
        if (!waterStates.TryGetValue(player, out WaterState state))
            return;

        waterStates.Remove(player);

        if (state.Rigidbody != null)
        {
            state.Rigidbody.gravityScale = state.GravityScale;
        }

        if (state.PlayerMovement != null)
        {
            state.PlayerMovement.enabled = state.PlayerMovementWasEnabled;
        }

        if (state.StarfyMovement != null)
        {
            state.StarfyMovement.enabled = state.StarfyMovementWasEnabled;
        }
    }

    private void StopDamageTimer(GameObject player)
    {
        if (!damageCoroutines.TryGetValue(player, out Coroutine damageCoroutine))
            return;

        StopCoroutine(damageCoroutine);
        damageCoroutines.Remove(player);
    }

    private bool TryGetPlayer(Collider2D other, out GameObject player)
    {
        player = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        return IsPlayer(player);
    }

    private bool IsPlayer(GameObject player)
    {
        return player.CompareTag("Storm") || player.CompareTag("Starfy");
    }

    private bool IsOwnedByLocalPlayer(GameObject player)
    {
        PhotonView playerView = player.GetComponent<PhotonView>();

        return playerView == null || playerView.IsMine;
    }
}
