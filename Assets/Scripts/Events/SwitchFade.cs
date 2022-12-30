using UnityEngine;

namespace FastPolygons.Manager
{
    public class SwitchFade : MonoBehaviour
    {
        public void LoadLevel()
        {
            GameManager.Instance.State = GameManager.EStates.LOADSCREEN;
            AudioManager.Instance.musicChanged?.Invoke(GameManager.Instance.State);
        }
    }
}