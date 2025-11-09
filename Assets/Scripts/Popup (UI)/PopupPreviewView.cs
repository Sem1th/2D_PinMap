using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TouristMap.Views
{
    public class PopupPreviewView : MonoBehaviour
    {
        public event Action OnClose;
        public event Action OnReadMore;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image pinImage;

        [SerializeField] private Button closeButton;
        [SerializeField] private Button readMoreButton;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            readMoreButton.onClick.AddListener(() => OnReadMore?.Invoke());
        }

        public void SetData(string title, string description, Sprite sprite = null)
        {
            titleText.text = title;
            descriptionText.text = description.Length > 50 ? description.Substring(0, 50) + "..." : description;
            if (sprite != null) pinImage.sprite = sprite;
        }

        public void ResetText()
        {
            titleText.text = "";
            descriptionText.text = "";
            // pinImage не трогаем
        }

        public Image PinImage => pinImage;
    }
}






