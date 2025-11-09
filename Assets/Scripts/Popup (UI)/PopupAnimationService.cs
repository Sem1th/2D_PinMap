using UnityEngine;
using DG.Tweening;

namespace TouristMap.Views
{
    public class PopupAnimationService
    {
        public void ShowView(GameObject viewObject, System.Action onComplete = null)
        {
            if (viewObject == null) return;

            viewObject.SetActive(true);
            var transform = viewObject.transform;
            transform.localScale = Vector3.zero;

            transform.DOScale(1f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void HideView(GameObject viewObject, System.Action onComplete = null)
        {
            if (viewObject == null || !viewObject.activeSelf)
            {
                onComplete?.Invoke();
                return;
            }

            viewObject.transform.DOScale(0f, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    viewObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        public void KillTweens(params GameObject[] viewObjects)
        {
            foreach (var viewObject in viewObjects)
            {
                if (viewObject != null)
                    viewObject.transform.DOKill();
            }
        }
    }
}
