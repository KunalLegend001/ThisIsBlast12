using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;

    private GridCube target;
    private GridManager gridManager;
    private ShooterCube owner;

    private bool hasHit = false;

    // ==============================
    // INIT
    // ==============================
    public void Init(GridCube t, GridManager gm, ShooterCube shooter)
    {
        target = t;
        gridManager = gm;
        owner = shooter;
    }

    // ==============================
    // UPDATE (MOVE)
    // ==============================
    void Update()
    {
        if (hasHit) return;

        // Target destroyed before hit
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPos = target.transform.position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            speed * Time.deltaTime
        );
    }

    // ==============================
    // HIT
    // ==============================
    void OnTriggerEnter(Collider other)
    {
        GridCube cube = other.GetComponent<GridCube>();
        if (!cube) return;

        // 🔥 MATCH COLOR
        if (cube.cubeMaterial != owner.GetShooterMaterial())
            return;

        Vector3 hitPos = cube.transform.position;

        gridManager.RemoveCube(cube);
        owner.OnSuccessfulHit();

        AudioManager.Instance.PlayCollision();

        // ✅ PLAY PARTICLE AT HIT POSITION
        ParticleManager.Instance.PlayHitEffect(hitPos);

        Destroy(gameObject);
    }





}
