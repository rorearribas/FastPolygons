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

        private ChromaticAberration m_chromaticAberration;
        private Vignette m_vignette;
        private Grain m_grain;

        void Awake()
        {
            m_boxCollider = GetComponent<BoxCollider>();
            m_postProcessVolume = FindObjectOfType<PostProcessVolume>();

            if (m_postProcessVolume == null) return;
            m_chromaticAberration = m_postProcessVolume.profile.GetSetting<ChromaticAberration>();
            m_vignette = m_postProcessVolume.profile.GetSetting<Vignette>();
            m_grain = m_postProcessVolume.profile.GetSetting<Grain>();
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentPlayer)
            {
                Player pPlayer = GameManager.Instance.CurrentPlayer;

                float currentDist = GetClosestDistanceToEdge(pPlayer.transform.position);
                float minDistance = 25f;
                float Value = (minDistance - currentDist) / minDistance * 1f;

                m_vignette.intensity.value = Mathf.Clamp(Value, 0.2f, 0.8f);
                m_grain.intensity.value = Mathf.Clamp(Value, 0.1f, 0.5f);
                m_chromaticAberration.intensity.value = Mathf.Clamp(Value, 0.05f, 0.8f);
            }
        }

        private Vector3[] GetCustomEdges()
        {
            if (!GameManager.Instance.CurrentPlayer) 
                return null;

            Player pPlayer = GameManager.Instance.CurrentPlayer;
            Vector3 vPos = pPlayer.transform.position;

            Vector3 vCenter = m_boxCollider.bounds.center;
            Vector3 vExtents = m_boxCollider.bounds.extents;

            Vector3 vRight = new(vCenter.x + vExtents.x, vPos.y, vPos.z);
            Vector3 vLeft = new(vCenter.x - vExtents.x, vPos.y, vPos.z);
            Vector3 vUp = new(vPos.x, vPos.y, vCenter.z + vExtents.z);
            Vector3 vDown = new(vPos.x, vPos.y, vCenter.z - vExtents.z);

            Vector3[] vPoints = { vRight, vLeft, vUp, vDown };
            return vPoints;
        }

        private float GetClosestDistanceToEdge(Vector3 _Position)
        {
            if (GetCustomEdges().Length == 0)
                return -1f;

            float fDistance = float.MaxValue;
            Vector3[] vPoints = GetCustomEdges();

            for (int i = 0; i < vPoints.Length; i++) {
                float fCurrentDist = Vector3.Distance(vPoints[i], _Position);
                if (fDistance > fCurrentDist) {
                    fDistance = fCurrentDist;
                }
            }
            return fDistance;
        }

        private void OnTriggerExit(Collider coll)
        {
            if(coll.GetComponent<Player>())
            {
                Player pPlayer = coll.GetComponent<Player>();
                pPlayer.OnAccident?.Invoke(this, System.EventArgs.Empty);
            }
        }
    }
}
