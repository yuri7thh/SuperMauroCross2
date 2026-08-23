using UnityEngine;

public static class PlaceholderFactory
{
    private static Sprite _whiteSprite;

    public static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            }
            return _whiteSprite;
        }
    }

    public static GameObject CreateBox(string name, Color color, Vector2 size, bool trigger)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = WhiteSprite;
        sr.color = color;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        col.isTrigger = trigger;
        return go;
    }
}
