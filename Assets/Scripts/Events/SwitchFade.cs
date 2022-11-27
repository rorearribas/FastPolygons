using UnityEngine;
using FastPolygons.Manager;

public class SwitchFade : MonoBehaviour
{
    public void LoadLevel() 
    {
        GameManager.gM.state = GameManager.States.LoadScreen;
        AudioManager.audioM.musicChanged?.Invoke(GameManager.gM.state);
    }
}
