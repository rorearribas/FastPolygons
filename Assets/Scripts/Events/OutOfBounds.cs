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
            Bounds bounds = m_boxCollider.bounds;

            Vector3 vRight = bounds.center;
            vRight.x += bounds.extents.x;
            vRight.y = vPlayerPos.y;
            vRight.z = vPlayerPos.z;

            Vector3 vLeft = bounds.center;
            vLeft.x -= bounds.extents.x;
            vLeft.y = vPlayerPos.y;
            vLeft.z = vPlayerPos.z;

            Vector3 vUp = vPlayerPos;
            vUp.z = bounds.center.z + bounds.extents.z;

            Vector3 vDown = vPlayerPos;
            vDown.z = bounds.center.z - bounds.extents.z;

            Vector3[] vPoints = { vRight, vLeft, vUp, vDown };
            return vPoints;
        }

        private float GetClosestDistanceToEdge(Player pPlayer)
        {
            Vector3[] customEdges = GetCustomEdges(pPlayer);
            if (customEdges.Length == 0)
                return -1f;

            float fDistance = float.MaxValue;
            Vector3 playerPosition = pPlayer.transform.position;

            for (int i = 0; i < customEdges.Length; i++)
            {
                float fCurrentDist = Vector3.Distance(customEdges[i], playerPosition);
                fDistance = Mathf.Min(fDistance, fCurrentDist);
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
            Player pPlayer = coll.GetComponent<Player>();
            if (!pPlayer || isRespawn) return;

            float minDistance = 25f;
            float currentDist = GetClosestDistanceToEdge(pPlayer);
            float Value = (minDistance - currentDist) / minDistance * 1f;

            m_vignette.intensity.value = Mathf.Clamp(Value, 0.2f, 0.8f);
            m_grain.intensity.value = Mathf.Clamp(Value, 0.1f, 0.5f);
            m_chromaticAberration.intensity.value = Mathf.Clamp(Value, 0.05f, 0.8f);
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
