using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Spawner : ISharedComponentData, IEquatable<Spawner>
{
	public float cooldown;
	public GameObject prefab;

    public bool Equals(Spawner other)
    {
        bool is_equal = (cooldown == other.cooldown && prefab == other.prefab);
        return is_equal;
    }

    public override int GetHashCode()
    {
        int hash = cooldown.GetHashCode();

        if (!ReferenceEquals(prefab, null))
            hash ^= prefab.GetHashCode();

        return hash;
    }
}
