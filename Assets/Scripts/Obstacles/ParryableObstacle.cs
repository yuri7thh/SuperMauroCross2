using UnityEngine;

public class ParryableObstacle : MonoBehaviour, IParryable
{
    public bool IsParried { get; private set; }

    public void Parry()
    {
        if (IsParried) return;
        IsParried = true;
        Destroy(gameObject);
    }
}