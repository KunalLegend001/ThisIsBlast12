using UnityEngine;

public class EmptySpaceManager : MonoBehaviour
{
    [SerializeField] private Transform[] slots;

    private bool[] occupied;

    void Awake()
    {
        occupied = new bool[slots.Length];
    }

    /// <summary>
    /// Returns first empty slot from left to right
    /// </summary>
    public bool TryGetEmptySlot(out Transform slot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!occupied[i])
            {
                occupied[i] = true;
                slot = slots[i];
                return true;
            }
        }

        slot = null;
        return false;
    }

    public void ReleaseSlot(Transform slot)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slot)
            {
                occupied[i] = false;
                return;
            }
        }
    }
}
