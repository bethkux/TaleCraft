using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TaleCraft.Example
{
    /// <summary>
    /// Shifts GameObject (camera) to the required position.
    /// </summary>
    public class CameraShifter : MonoBehaviour
    {
        [SerializeField] private List<GameObject> switches = new();
        [SerializeField] private Image fadePanel;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float shiftDuration = 0f;
        private int current = 0;


        void Start()
        {
            if (fadePanel != null)
            {
                SetAlpha(1f);
                fadePanel.gameObject.SetActive(true);
            }
        }

        public void FadeOutAndIn()
        {
            StartCoroutine(FadeSequence());
        }

        private IEnumerator FadeSequence()
        {
            // Fade to black
            fadePanel.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f));

            Switch();

            // Fade back to clear
            FadeIn();
        }

        public void FadeIn()
        {
            StartCoroutine(FadeInSequence());
        }

        private IEnumerator FadeInSequence()
        {
            // Fade back to clear
            yield return StartCoroutine(Fade(1f, 0f));
            fadePanel.gameObject.SetActive(false);
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            float elapsed = 0f;
            Color color = fadePanel.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
                color.a = alpha;
                fadePanel.color = color;
                yield return null;
            }

            // Ensure exact final alpha
            color.a = endAlpha;
            fadePanel.color = color;
        }

        private void Switch()
        {
            if (switches.Count > 0)
            {
                current = (current + 1) % switches.Count;
                ShiftTo(switches[current]);
            }
        }

        private void ShiftTo(GameObject newTransform)
        {
            var pos = transform.position;
            pos.x = newTransform.transform.position.x;
            pos.y = newTransform.transform.position.y;
            transform.position = pos;
        }

        public void ShiftTo(Vector2 newTransform)
        {
            var pos = transform.position;
            pos.x = newTransform.x;
            pos.y = newTransform.y;
            transform.position = pos;
        }

        public void Shift(GameObject to)
        {
            StartCoroutine(Shift(transform.position.x, to.transform.position.x));
        }

        private IEnumerator Shift(float startPos, float endPos)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startPos, endPos, elapsed / shiftDuration);
                var p = transform.position;
                p.x = alpha;
                transform.position = p;
                yield return null;
            }

            // Ensure exact final alpha
            var po = transform.position;
            po.x = endPos;
            transform.position = po;
        }

        private void SetAlpha(float alpha)
        {
            Color color = fadePanel.color;
            color.a = alpha;
            fadePanel.color = color;
        }
    }
}