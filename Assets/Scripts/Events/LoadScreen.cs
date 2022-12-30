using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FastPolygons.Manager
{
    public class LoadScreen : MonoBehaviour
    {
        [SerializeField] GameObject fillObj;
        [SerializeField] Image fillLoading;
        [SerializeField] GameObject textToContinueObj;
        private AsyncOperation asyncOperation;
        public static LoadScreen instance;
        public static bool isLoading;
        public static bool loadLevel;
        public static float timer;
        private bool reset;

        private void Awake()
        {
            instance = this;
        }
        void Start()
        {
            fillLoading.fillAmount = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (!reset)
            {
                Reset();
                reset = true;
            }

            if (!isLoading)
            {
                StartCoroutine(NextLevelLoading());
            }

            if (loadLevel)
            {
                timer += Time.deltaTime;

                if (timer >= 1)
                {
                    textToContinueObj.SetActive(true);
                    fillObj.SetActive(false);

                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        asyncOperation.allowSceneActivation = true;
                        textToContinueObj.SetActive(false);
                        GameManager.Instance.State = GameManager.EStates.START;
                        GameManager.Instance.fadeAnimator.SetTrigger("FadeOut");
                        reset = false;
                    }
                }
            }
        }

        IEnumerator NextLevelLoading()
        {
            isLoading = true;
            fillLoading.fillAmount = 0;
            yield return new WaitForSeconds(0.5f);

            asyncOperation = SceneManager.LoadSceneAsync(1);
            asyncOperation.allowSceneActivation = false;
            fillLoading.fillAmount = asyncOperation.progress;

            while (!asyncOperation.isDone)
            {
                fillLoading.fillAmount = Mathf.Lerp(fillLoading.fillAmount, asyncOperation.progress, Time.deltaTime / 2);

                if (fillLoading.fillAmount >= 0.8)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(1);

            fillLoading.fillAmount = 100;

            yield return new WaitForSeconds(1);

            loadLevel = true;
        }

        public void Reset()
        {
            fillObj.SetActive(true);
            fillLoading.fillAmount = 0;
            isLoading = false;
            loadLevel = false;
            timer = 0;
        }
    }
}