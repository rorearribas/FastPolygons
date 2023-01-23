using FastPolygons.Manager;
using System.Collections;
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

        private bool isRespawn = false;

        void Awake()
        {
            m_boxCollider = GetComponent<BoxCollider>();
            m_postProcessVolume = FindObjectOfType<PostProcessVolume>();

            if (m_postProcessVolume == null) return;
            m_chromaticAberration = m_postProcessVolume.profile.GetSetting<ChromaticAberration>();
            m_vignette = m_postProcessVolume.profile.GetSetting<Vignette>();
            m_grain = m_postProcessVolume.profile.GetSetting<Grain>();
        }

        private Vector3[] GetCustomEdges(Player pPlayer)
        {
            if (pPlayer == null)
                return null;

            Vector3 vPlayerPos = pPlayer.transform.position;

            Vector3 vCenter = m_boxCollider.bounds.center;
            Vector3 vExtents = m_boxCollider.bounds.extents;

            Vector3 vRight = new(vCenter.x + vExtents.x, vPlayerPos.y, vPlayerPos.z);
            Vector3 vLeft = new(vCenter.x - vExtents.x, vPlayerPos.y, vPlayerPos.z);
            Vector3 vUp = new(vPlayerPos.x, vPlayerPos.y, vCenter.z + vExtents.z);
            Vector3 vDown = new(vPlayerPos.x, vPlayerPos.y, vCenter.z - vExtents.z);

            Vector3[] vPoints = { vRight, vLeft, vUp, vDown };
            return vPoints;
        }

        private float GetClosestDistanceToEdge(Player pPlayer)
        {
            if (GetCustomEdges(pPlayer)?.Length == 0)
                return -1f;

            float fDistance = float.MaxValue;
            Vector3[] vPoints = GetCustomEdges(pPlayer);
            Vector3 playerPosition = pPlayer.transform.position;

            for (int i = 0; i < vPoints.Length; i++) {
                float fCurrentDist = Vector3.Distance(vPoints[i], playerPosition);
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
                StartCoroutine(Respawn());
            }
        }

        private void OnTriggerStay(Collider coll)
        {
            if (coll.GetComponent<Player>())
            {
                if (isRespawn) return;

                Player pPlayer = coll.GetComponent<Player>();

                float currentDist = GetClosestDistanceToEdge(pPlayer);
                float minDistance = 25f;
                float Value = (minDistance - currentDist) / minDistance * 1f;

                m_vignette.intensity.value = Mathf.Clamp(Value, 0.2f, 0.8f);
                m_grain.intensity.value = Mathf.Clamp(Value, 0.1f, 0.5f);
                m_chromaticAberration.intensity.value = Mathf.Clamp(Value, 0.05f, 0.8f);
            };
        }

        private IEnumerator Respawn()
        {
            isRespawn = true;
            yield return new WaitForSeconds(1f);

            float Value = 1f;
            while (Value > 0f)
            {
                m_vignette.intensity.value = Mathf.Clamp(Value, 0.2f, 0.8f);
                m_grain.intensity.value = Mathf.Clamp(Value, 0.1f, 0.5f);
                m_chromaticAberration.intensity.value = Mathf.Clamp(Value, 0.05f, 0.8f);

                Value -= 0.25f * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            isRespawn = false;
        }
    }
}
