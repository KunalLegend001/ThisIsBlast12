using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShooterManager : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject shooterPrefab;

    [Header("Grid Size")]
    public int columns = 5;
    public int rowsPerColumn = 3;

    [Header("Layout")]
    public float columnSpacing = 1.2f;
    public float verticalSpacing = 1.1f;
    public float cellSize = 1f;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private EmptySpaceManager emptySpaceManager;

    private ShooterCube[,] shooterGrid;
    private readonly List<GameObject> spawned = new();

    // 🔥 NEW: COLOR-BASED CONTROL
    private Dictionary<Material, ShooterCube> activeShooterByColor = new();
    private Dictionary<Material, Queue<ShooterCube>> waitingShootersByColor = new();

    void Start()
    {
        StartCoroutine(SpawnGrid());
    }

    IEnumerator SpawnGrid()
    {
        yield return new WaitForSeconds(3f);

        if (!gridManager)
        {
            Debug.LogError("GridManager reference missing!");
            yield break;
        }

        SpawnFromGridData(gridManager.GetShooterSpawnData());
    }

    // ==============================
    // SPAWN SHOOTERS (UNCHANGED LOGIC)
    // ==============================
    public void SpawnFromGridData(Dictionary<Material, List<int>> shooterData)
    {
        if (!shooterPrefab || shooterData == null || shooterData.Count == 0)
            return;

        Clear();

        shooterGrid = new ShooterCube[rowsPerColumn, columns];

        List<(Material mat, int shots)> spawnQueue =
            BuildInterleavedShooterList(shooterData);

        int index = 0;

        for (int row = 0; row < rowsPerColumn; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (index >= spawnQueue.Count)
                    return;

                Vector3 localPos =
                    transform.right * col * columnSpacing +
                    -transform.up * row * verticalSpacing;

                GameObject cube = Instantiate(shooterPrefab, transform);
                cube.transform.localPosition = localPos;
                cube.transform.localRotation = Quaternion.identity;
                cube.transform.localScale = Vector3.one * cellSize;

                ShooterCube shooter = cube.GetComponent<ShooterCube>();
                if (!shooter)
                {
                    Debug.LogError("ShooterCube missing on prefab!");
                    Destroy(cube);
                    continue;
                }

                shooter.row = row;
                shooter.col = col;

                var data = spawnQueue[index];

                shooter.Setup(
                    data.mat,
                    data.shots,
                    this,
                    gridManager,
                    emptySpaceManager
                );

                shooterGrid[row, col] = shooter;
                spawned.Add(cube);
                index++;
            }
        }
    }

    // ==============================
    // SHOOTER MOVEMENT CALLBACK
    // ==============================
    public void OnShooterMoved(int row, int col)
    {
        shooterGrid[row, col] = null;
        StartCoroutine(ApplyShooterGravity(col));
    }

    // ==============================
    // SHOOTER GRAVITY (UNCHANGED)
    // ==============================
    IEnumerator ApplyShooterGravity(int col)
    {
        yield return new WaitForSeconds(0.05f);

        for (int r = 1; r < rowsPerColumn; r++)
        {
            if (shooterGrid[r, col] != null &&
                shooterGrid[r - 1, col] == null)
            {
                ShooterCube falling = shooterGrid[r, col];

                shooterGrid[r - 1, col] = falling;
                shooterGrid[r, col] = null;

                falling.row -= 1;

                Vector3 targetPos =
                    transform.right * col * columnSpacing +
                    -transform.up * (r - 1) * verticalSpacing;

                StartCoroutine(MoveShooterTo(falling.transform, targetPos));
            }
        }
    }

    IEnumerator MoveShooterTo(Transform t, Vector3 target)
    {
        Vector3 start = t.localPosition;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * 5f;
            t.localPosition = Vector3.Lerp(start, target, time);
            yield return null;
        }

        t.localPosition = target;
    }

    // ==============================
    // 🔥 COLOR-BASED SHOOT CONTROL
    // ==============================
    public bool RequestShootPermission(ShooterCube shooter, Material mat)
    {
        if (!activeShooterByColor.ContainsKey(mat))
        {
            activeShooterByColor[mat] = shooter;
            return true; // allowed to shoot immediately
        }

        if (!waitingShootersByColor.ContainsKey(mat))
            waitingShootersByColor[mat] = new Queue<ShooterCube>();

        waitingShootersByColor[mat].Enqueue(shooter);
        return false; // must wait
    }

    public void NotifyShooterFinished(Material mat)
    {
        if (!activeShooterByColor.ContainsKey(mat))
            return;

        activeShooterByColor.Remove(mat);

        if (waitingShootersByColor.ContainsKey(mat) &&
            waitingShootersByColor[mat].Count > 0)
        {
            ShooterCube next = waitingShootersByColor[mat].Dequeue();
            activeShooterByColor[mat] = next;
            next.BeginShooting(); // 🔥 resume automatically
        }
    }

    // ==============================
    // INTERLEAVING (UNCHANGED)
    // ==============================
    List<(Material, int)> BuildInterleavedShooterList(
        Dictionary<Material, List<int>> data
    )
    {
        List<(Material, int)> result = new();
        Dictionary<Material, Queue<int>> temp = new();

        foreach (var pair in data)
            temp[pair.Key] = new Queue<int>(pair.Value);

        while (temp.Count > 0)
        {
            List<Material> keys = new(temp.Keys);

            foreach (Material mat in keys)
            {
                result.Add((mat, temp[mat].Dequeue()));

                if (temp[mat].Count == 0)
                    temp.Remove(mat);
            }
        }

        return result;
    }

    // ==============================
    // UTILITIES
    // ==============================
    void Clear()
    {
        foreach (var go in spawned)
            if (go) Destroy(go);

        spawned.Clear();

        activeShooterByColor.Clear();
        waitingShootersByColor.Clear();
    }

    // ==============================
    // DEBUG
    // ==============================
    public void OnShooterActivated(Material mat)
    {
        Debug.Log($"Shooter activated: {mat.name}");
    }

    // ==============================
    // GIZMOS
    // ==============================
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rowsPerColumn; row++)
            {
                Vector3 localPos =
                    transform.right * col * columnSpacing +
                    -transform.up * row * verticalSpacing;

                Gizmos.DrawWireCube(localPos, Vector3.one * cellSize);
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}
