using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 15;
    public int cols = 10;
    public float spacing = 1.1f;

    [Header("Grid Visual Size")]
    public float cellSize = 0.9f;

    [Header("Prefabs")]
    public GridCube cubePrefab;

    [Header("Cube Materials")]
    public Material[] cubeMaterials;

    public GridCube[,] grid;

    public System.Action OnGridReady;

    void Start()
    {
        grid = new GridCube[rows, cols];
        GenerateGrid();

        OnGridReady?.Invoke();

        DebugFinalGridData();
    }

    // ==============================
    // GRID GENERATION
    // ==============================
    void GenerateGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 localPos = new Vector3(c * spacing, r * spacing, 0);
                Vector3 worldPos = transform.TransformPoint(localPos);

                GridCube cube = Instantiate(
                    cubePrefab,
                    worldPos,
                    transform.rotation,
                    transform
                );

                cube.row = r;
                cube.col = c;

                Material mat = cubeMaterials[Random.Range(0, cubeMaterials.Length)];
                cube.cubeMaterial = mat;
                cube.GetComponent<Renderer>().sharedMaterial = mat;


                grid[r, c] = cube;
            }
        }
    }

    // ==============================
    // TARGET
    // ==============================
    public GridCube GetFirstRowTarget(Material shooterMaterial)
    {
        for (int c = 0; c < cols; c++)
        {
            GridCube cube = grid[0, c];
            if (cube == null) continue;

            if (cube.cubeMaterial != null &&
                cube.cubeMaterial == shooterMaterial)
            {
                return cube;
            }
        }
        return null;
    }
    public int GetTotalCubeCount()
    {
        int count = 0;

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                if (grid[r, c] != null)
                    count++;

        return count;
    }


    // ==============================
    // REMOVE + GRAVITY
    // ==============================
    public void RemoveCube(GridCube cube)
    {
        if (cube == null) return;

        grid[cube.row, cube.col] = null;
        Destroy(cube.gameObject);

        StartCoroutine(ApplyGravity());
    }

    IEnumerator ApplyGravity()
    {
        yield return new WaitForSeconds(0.05f);

        for (int r = 1; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] != null && grid[r - 1, c] == null)
                {
                    GridCube fallingCube = grid[r, c];

                    grid[r - 1, c] = fallingCube;
                    grid[r, c] = null;

                    fallingCube.row -= 1;
                    fallingCube.transform.position += -transform.up * spacing;
                }
            }
        }
    }

    // ==============================
    // FINAL SHOOTER DATA (IMPORTANT)
    // ==============================
    public Dictionary<Material, List<int>> GetShooterSpawnData()
    {
        Dictionary<Material, int> cubeCounts = new();
        Dictionary<Material, List<int>> shooterData = new();

        // Count cubes per material
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (grid[r, c] == null) continue;

                Material mat = grid[r, c].cubeMaterial;

                if (!cubeCounts.ContainsKey(mat))
                    cubeCounts[mat] = 0;

                cubeCounts[mat]++;
            }
        }

        // Convert to shooter values
        foreach (var pair in cubeCounts)
        {
            int total = pair.Value;
            int fullShooters = total / 20;
            int leftover = total % 20;

            shooterData[pair.Key] = new List<int>();

            for (int i = 0; i < fullShooters; i++)
                shooterData[pair.Key].Add(20);

            if (leftover > 0)
                shooterData[pair.Key].Add(leftover);
        }

        return shooterData;
    }

    // ==============================
    // DEBUG (TRUTH LOG)
    // ==============================
    public void DebugFinalGridData()
    {
        var data = GetShooterSpawnData();

        Debug.Log("====== FINAL SHOOTER DATA ======");

        foreach (var mat in data.Keys)
        {
            foreach (int value in data[mat])
            {
                Debug.Log(
                    $"Material: {mat.name} | Shooter Value: {value}"
                );
            }
        }

        Debug.Log("================================");
    }

    // ==============================
    // GIZMOS
    // ==============================
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 localPos = new Vector3(c * spacing, r * spacing, 0);
                Vector3 worldPos = transform.TransformPoint(localPos);

                Gizmos.matrix = Matrix4x4.TRS(
                    worldPos,
                    transform.rotation,
                    Vector3.one
                );

                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * cellSize);
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
    }
}
