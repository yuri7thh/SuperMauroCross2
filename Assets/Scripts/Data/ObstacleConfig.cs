using System;
using System.Collections.Generic;
using UnityEngine;

public enum HitboxShape { Box, Triangle }

[CreateAssetMenu(fileName = "ObstacleConfig", menuName = "SMC/ObstacleConfig")]
public class ObstacleConfig : ScriptableObject
{
    [Serializable]
    public class ObstacleEntry
    {
        [Tooltip("Tipo de obstaculo.")]
        public ObstacleKind kind;

        [Header("Sprite")]
        [Tooltip("Sprite estatico. A animacao (abaixo) tem prioridade sobre ele.")]
        public Sprite sprite;
        [Tooltip("Tamanho visual do sprite em unidades (independente do hitbox).")]
        public Vector2 spriteSize = Vector2.one;
        [Tooltip("Cor/tinta do sprite.")]
        public Color color = Color.white;

        [Header("Animacao (tem prioridade sobre o sprite)")]
        [Tooltip("Controller de animacao completo. Prioridade maxima sobre o resto.")]
        public RuntimeAnimatorController animatorController;
        [Tooltip("Clip de animacao (loop). Se definido e sem controller, vira a animacao do obstaculo.")]
        public AnimationClip idle;

        [Header("Hitbox")]
        [Tooltip("Forma do hitbox.")]
        public HitboxShape hitboxShape = HitboxShape.Box;
        [Tooltip("Tamanho do hitbox em unidades (independente do sprite).")]
        public Vector2 hitboxSize = Vector2.one;
        [Tooltip("Deslocamento do hitbox em relacao ao pivot.")]
        public Vector2 hitboxOffset = Vector2.zero;
        [Tooltip("Mostra o contorno verde do hitbox (Game/Scene view).")]
        public bool showHitbox = true;
    }

    [Tooltip("Mostra o contorno verde do hitbox (Scene view) para debug.")]
    public bool showHitboxes = true;

    [Tooltip("Configuracao de sprite/animacao/hitbox por tipo de obstaculo.")]
    public List<ObstacleEntry> obstacles = new List<ObstacleEntry>
    {
        new ObstacleEntry
        {
            kind = ObstacleKind.Spike,
            hitboxShape = HitboxShape.Triangle,
            hitboxSize = new Vector2(1f, 0.5f),
            spriteSize = new Vector2(1f, 0.5f),
            color = new Color(0.9f, 0.25f, 0.25f)
        },
        new ObstacleEntry
        {
            kind = ObstacleKind.Wall,
            hitboxSize = new Vector2(1f, 1.8f),
            spriteSize = new Vector2(1f, 1.8f),
            color = new Color(0.95f, 0.6f, 0.2f)
        },
        new ObstacleEntry
        {
            kind = ObstacleKind.GroundEnemy,
            hitboxSize = new Vector2(1f, 1.5f),
            spriteSize = new Vector2(1f, 1.5f),
            color = new Color(0.7f, 0.3f, 0.8f)
        },
    };

    public ObstacleEntry Get(ObstacleKind kind)
    {
        if (obstacles == null) return null;
        foreach (var e in obstacles)
            if (e != null && e.kind == kind) return e;
        return null;
    }
}
