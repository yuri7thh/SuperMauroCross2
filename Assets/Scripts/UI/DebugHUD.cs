using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    void OnGUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 340, 300));
        GUILayout.Label("Estado: " + gm.State);
        GUILayout.Label("Distancia: " + gm.Distance.ToString("F0") + " m");
        GUILayout.Label("Score: " + gm.Score.ToString("F0"));
        GUILayout.Label("Recorde: " + gm.BestScore.ToString("F0"));
        GUILayout.Label("Combo: x" + gm.Combo + "  (mult x" + gm.Multiplier.ToString("F1") + ")");

        if (gm.State == GameState.Menu)
        {
            if (GUILayout.Button("INICIAR")) gm.StartGame();
            GUILayout.Label("Espaco = pular   |   K = parry (no ar)   |   L = parry-pulo");
        }
        else if (gm.State == GameState.GameOver)
        {
            GUILayout.Label("GAME OVER");
            if (GUILayout.Button("REINICIAR (R)")) gm.Restart();
        }
        GUILayout.EndArea();
    }
}
