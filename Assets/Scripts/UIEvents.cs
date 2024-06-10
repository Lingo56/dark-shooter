using System;
using UnityEngine;

public static class UIEvents
{
    public static event Action<int> OnHitCountChanged;

    public static void HitCountChanged(int newHitCount)
    {
        OnHitCountChanged?.Invoke(newHitCount);
    }
}
