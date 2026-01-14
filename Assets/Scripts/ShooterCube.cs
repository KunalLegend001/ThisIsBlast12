using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class ShooterCube : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("Shoot")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Projectile projectilePrefab;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldown = 0.12f;

    private int shots;
    private Material shooterMaterial;
    private ShooterManager shooterManager;
    private GridManager gridManager;
    private EmptySpaceManager emptySpaceManager;

    private bool isActivated = false;
    private bool isWaitingForHit = false;

    private Transform occupiedSlot;
    public int row;
    public int col;

    // ==============================
    // SETUP
    // ==============================
    public void Setup(
        Material material,
        int shotCount,
        ShooterManager shooterMgr,
        GridManager gridMgr,
        EmptySpaceManager spaceMgr
    )
    {
        shooterMaterial = material;
        shots = shotCount;
        shooterManager = shooterMgr;
        gridManager = gridMgr;
        emptySpaceManager = spaceMgr;

      GetComponent<Renderer>().sharedMaterial = material;


        if (!text)
            text = GetComponentInChildren<TextMeshProUGUI>(true);

        text.text = shots.ToString();
    }

    // ==============================
    // INPUT
    // ==============================
    private void OnMouseDown()
    {
        // 🚫 Ignore click if pointer is over UI
      //  if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
         //   return;

        AudioManager.Instance.PlayCubeTouch();

        if (shots <= 0 || isActivated)
            return;

        TryMoveToEmptySpace();
    }



    // ==============================
    // MOVE
    // ==============================
    void TryMoveToEmptySpace()
    {
        if (!emptySpaceManager) return;

        if (emptySpaceManager.TryGetEmptySlot(out Transform slot))
        {
            occupiedSlot = slot;
            isActivated = true;

            // 🔥 INFORM MANAGER BEFORE MOVING
            shooterManager.OnShooterMoved(row, col);

            StartCoroutine(MoveToSlotAndShoot(slot.position));
        }

    }

    IEnumerator MoveToSlotAndShoot(Vector3 targetPos)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        shooterManager.OnShooterActivated(shooterMaterial);

        if (shooterManager.RequestShootPermission(this, shooterMaterial))
        {
            BeginShooting();
        }

    }

    // ==============================
    // FIRE LOOP (IMPORTANT FIX)
    // ==============================
    IEnumerator FireLoop()
    {
        while (shots > 0)
        {
            if (isWaitingForHit)
            {
                yield return null;
                continue;
            }

            GridCube target =
                gridManager.GetFirstRowTarget(shooterMaterial);

            if (target == null)
            {
                yield return null;
                continue;
            }

            isWaitingForHit = true;
            Shoot(target);

            yield return new WaitForSeconds(0.5f);
        }
    }

    void Shoot(GridCube target)
    {
        Projectile p = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.identity
        );

        p.Init(target, gridManager, this);
    }

    // ==============================
    // HIT CALLBACK
    // ==============================
    public void OnSuccessfulHit()
    {
        isWaitingForHit = false;

        shots--;
        text.text = shots.ToString();

        if (shots <= 0)
            DestroySelf();
    }

    // ==============================
    // CLEANUP
    // ==============================
    void DestroySelf()
    {
        shooterManager.NotifyShooterFinished(shooterMaterial);

        if (occupiedSlot && emptySpaceManager)
            emptySpaceManager.ReleaseSlot(occupiedSlot);

        Destroy(gameObject);
    }

    public void BeginShooting()
    {
        StartCoroutine(FireLoop());
    }
    public Material GetShooterMaterial()
    {
        return shooterMaterial;
    }

}
