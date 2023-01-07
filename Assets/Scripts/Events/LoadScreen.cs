using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace FastPolygons.Manager
{
    public class LoadScreen : MonoBehaviour
    {
        [SerializeField] private GameObject fillObj;
        [SerializeField] private Image fillLoading;
        [SerializeField] private GameObject textToContinueObj;

        private AsyncOperation asyncOperation;
        private bool isLoaded = false;

        private void Start()
        {
            if (InputManager.Instance == null) return;
            InputManager.OnInteractPressedEvent += OnPressContinue;

            fillLoading.fillAmount = 0;
            StartCoroutine(ILoader());

            if (!TryGetComponent<RectTransform>(out var rectTransform)) return;
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;

        }

        private void OnDestroy()
        {
            InputManager.OnInteractPressedEvent -= OnPressContinue;
        }

        private void OnPressContinue()
        {
            if (!isLoaded) return;

            asyncOperation.allowSceneActivation = true;
            textToContinueObj.SetActive(false);

            GameManager.Instance.OnChangedState.Invoke(GameManager.EStates.START);
            GameManager.Instance.fadeAnimator.SetTrigger("FadeOut");

            Destroy(this.gameObject);
        }

        private IEnumerator ILoader()
        {
            fillLoading.fillAmount = 0;
            yield return new WaitForSeconds(0.5f);

            asyncOperation = SceneManager.LoadSceneAsync(1);
            asyncOperation.allowSceneActivation = false;
            fillLoading.fillAmount = asyncOperation.progress;

            while (!asyncOperation.isDone)
            {
                fillLoading.fillAmount = Mathf.Lerp(fillLoading.fillAmount, 
                    asyncOperation.progress, Time.deltaTime / 2);

                if (fillLoading.fillAmount >= 0.8) break;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);

            fillLoading.fillAmount = 100;

            yield return new WaitForSeconds(1);

            isLoaded = true;
            textToContinueObj.SetActive(true);
            fillObj.SetActive(false);
        }
    }
}