using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private readonly Dictionary<GameObject, Stack<GameObject>> _pools =
        new Dictionary<GameObject, Stack<GameObject>>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject Spawn(GameObject template, Vector3 position, Quaternion rotation)
    {
        if (template == null) return null;

        if (!_pools.TryGetValue(template, out var stack))
        {
            stack = new Stack<GameObject>();
            _pools[template] = stack;
        }

        GameObject obj;
        if (stack.Count > 0)
        {
            obj = stack.Pop();
        }
        else
        {
            obj = Instantiate(template);
            var marker = obj.GetComponent<PooledObject>();
            if (marker == null) marker = obj.AddComponent<PooledObject>();
            marker.Template = template;
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        var pooled = obj.GetComponents<IPoolable>();
        foreach (var p in pooled) p.OnSpawned();

        return obj;
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        var marker = obj.GetComponent<PooledObject>();
        if (marker == null || marker.Template == null)
        {
            Destroy(obj);
            return;
        }

        var pooled = obj.GetComponents<IPoolable>();
        foreach (var p in pooled) p.OnDespawned();

        obj.SetActive(false);

        if (!_pools.TryGetValue(marker.Template, out var stack))
        {
            stack = new Stack<GameObject>();
            _pools[marker.Template] = stack;
        }
        stack.Push(obj);
    }
}
