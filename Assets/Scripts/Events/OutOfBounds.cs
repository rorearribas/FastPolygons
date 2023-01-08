using FastPolygons.Manager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace FastPolygons
{
    public class OutOfBounds : MonoBehaviour
    {
        private PostProcessVolume m_postProcessVolume;
        private BoxCollider m_boxCollider;

        void Awake()
        {
            m_boxCollider = GetComponent<BoxCollider>();
            m_postProcessVolume = GameObject.FindObjectOfType<PostProcessVolume>();
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentPlayer)
            {
                Player pPlayer = GameManager.Instance.CurrentPlayer;

                m_postProcessVolume.profile.TryGetSettings(out Vignette vignette);
                m_postProcessVolume.profile.TryGetSettings(out Grain grain);
                m_postProcessVolume.profile.TryGetSettings(out ChromaticAberration chromaticAberration);

                float dist = GetClosestDistanceToEdge(pPlayer.transform.position);
                float minDistance = 25f;
                float Value = (minDistance - dist) / minDistance * 1f;

                vignette.intensity.value = Mathf.Clamp(Value, 0.25f, 0.5f);
                grain.intensity.value = Mathf.Clamp(Value, 0.1f, 0.5f);
                chromaticAberration.intensity.value = Mathf.Clamp(Value, 0.05f, 0.8f);
            }
        }

        private List<Vector3> GetCustomEdges()
        {
            if (GameManager.Instance == null) return null;

            Player player = GameManager.Instance.CurrentPlayer;
            Vector3 vPos = player.transform.position;

            Vector3 center = m_boxCollider.bounds.center;
            Vector3 extents = m_boxCollider.bounds.extents;

            Vector3 vRight = new(center.x + extents.x, vPos.y, vPos.z);
            Vector3 vLeft = new(center.x - extents.x, vPos.y, vPos.z);
            Vector3 vUp = new(vPos.x, vPos.y, center.z + extents.z);
            Vector3 vDown = new(vPos.x, vPos.y, center.z - extents.z);

            List<Vector3> Points = new() { vRight, vLeft, vUp, vDown };
            return Points;
        }

        private float GetClosestDistanceToEdge(Vector3 _Position)
        {
            if (GetCustomEdges().Count == 0)
                return -1f;

            float Distance = float.MaxValue;
            List<Vector3> Points = GetCustomEdges();

            for (int i = 0; i < Points.Count; i++) {
                float dist = Vector3.Distance(Points[i], _Position);
                if (Distance > dist) {
                    Distance = dist;
                }
            }
            return Distance;
        }

        private void OnTriggerExit(Collider coll)
        {
            if(coll.GetComponent<Player>())
            {
                Player p = coll.GetComponent<Player>();
                p.OnAccident?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }
}
