using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TouristMap.Views
{
    public class PopupDetailView : MonoBehaviour
    {
        public event Action OnClose;
        public event Action OnEdit;
        public event Action OnDelete;

        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image pinImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button editButton;
        [SerializeField] private Button deleteButton;

        public Image PinImage => pinImage;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            editButton.onClick.AddListener(() => OnEdit?.Invoke());
            deleteButton.onClick.AddListener(() => OnDelete?.Invoke());
        }

        public void SetData(string title, string description, Sprite sprite = null)
        {
            titleText.text = title;
            descriptionText.text = description;
            if (sprite != null) pinImage.sprite = sprite;
        }

        public void ResetText()
        {
            titleText.text = "";
            descriptionText.text = "";
            // pinImage не трогаем
        }
    }
}





