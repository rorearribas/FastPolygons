using UnityEngine;

namespace FastPolygons.Manager
{
    public class SwitchFade : MonoBehaviour
    {
        public void LoadLevel()
        {
            GameManager.Instance.OnChangedState?.Invoke(GameManager.EStates.LOADSCREEN);
            AudioManager.Instance.OnMusicChangedEvent?.Invoke(GameManager.Instance.State);
        }
    }
}