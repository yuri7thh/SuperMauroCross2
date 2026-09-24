using UnityEngine;

public class ObstacleHitboxGizmo : MonoBehaviour
{
    [Tooltip("Mostra o contorno do hitbox.")]
    public bool show = true;
    [Tooltip("Cor do contorno do hitbox.")]
    public Color color = Color.green;

    private LineRenderer _lr;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        if (_lr == null) _lr = gameObject.AddComponent<LineRenderer>();
        _lr.useWorldSpace = false;
        _lr.loop = true;
        _lr.startWidth = 0.05f;
        _lr.endWidth = 0.05f;
        _lr.startColor = color;
        _lr.endColor = color;
        _lr.sortingOrder = 999;
        var mat = GetUnlitMaterial();
        if (mat != null) _lr.material = mat;
        Rebuild();
        _lr.enabled = show;
    }

    void Update()
    {
        if (_lr != null) _lr.enabled = show;
    }

    public void SetShow(bool value)
    {
        show = value;
        if (_lr != null) _lr.enabled = value;
    }

    private void Rebuild()
    {
        if (_lr == null) return;

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Vector2 o = box.offset;
            Vector2 half = box.size * 0.5f;
            _lr.positionCount = 4;
            _lr.SetPosition(0, new Vector3(o.x - half.x, o.y - half.y, 0f));
            _lr.SetPosition(1, new Vector3(o.x + half.x, o.y - half.y, 0f));
            _lr.SetPosition(2, new Vector3(o.x + half.x, o.y + half.y, 0f));
            _lr.SetPosition(3, new Vector3(o.x - half.x, o.y + half.y, 0f));
            return;
        }

        var poly = GetComponent<PolygonCollider2D>();
        if (poly != null && poly.points.Length > 0)
        {
            Vector2[] pts = poly.points;
            _lr.positionCount = pts.Length;
            for (int i = 0; i < pts.Length; i++)
            {
                _lr.SetPosition(i, new Vector3(pts[i].x + poly.offset.x, pts[i].y + poly.offset.y, 0f));
            }
            return;
        }

        _lr.positionCount = 0;
    }

    void OnDrawGizmos()
    {
        if (!show) return;

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.color = color;
            var prev = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.offset, box.size);
            Gizmos.matrix = prev;
            return;
        }

        var poly = GetComponent<PolygonCollider2D>();
        if (poly != null && poly.points.Length > 0)
        {
            Gizmos.color = color;
            Vector2[] pts = poly.points;
            Vector2 off = poly.offset;
            for (int i = 0; i < pts.Length; i++)
            {
                Vector3 a = transform.TransformPoint(new Vector3(pts[i].x + off.x, pts[i].y + off.y, 0f));
                Vector3 b = transform.TransformPoint(new Vector3(pts[(i + 1) % pts.Length].x + off.x, pts[(i + 1) % pts.Length].y + off.y, 0f));
                Gizmos.DrawLine(a, b);
            }
        }
    }

    private static Material _mat;
    private static Material GetUnlitMaterial()
    {
        if (_mat != null) return _mat;
        var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null) _mat = new Material(shader);
        return _mat;
    }
}