using UnityEngine;
using FastPolygons.Manager;

public class SwitchFade : MonoBehaviour
{
    public void LoadLevel() 
    {
        GameManager.Instance.state = GameManager.States.LOADSCREEN;
        AudioManager.Instance.musicChanged?.Invoke(GameManager.Instance.state);
    }
}
