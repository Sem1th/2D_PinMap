using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TouristMap.Data;

namespace TouristMap.Views
{
    public class PopupEditView : MonoBehaviour
    {
        public event Action<PinData> OnSave;
        public event Action OnCancel;
        public event Action OnChooseImage;

        [SerializeField] private TMP_InputField titleInput;
        [SerializeField] private TMP_InputField descriptionInput;
        [SerializeField] private Image pinImage;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button selectImageButton;

        private PinData currentPin;
        public Image PinImage => pinImage;

        private void Awake()
        {
            saveButton.onClick.AddListener(() =>
            {
                currentPin.name = titleInput.text;
                currentPin.description = descriptionInput.text;
                OnSave?.Invoke(currentPin);
            });

            cancelButton.onClick.AddListener(() => OnCancel?.Invoke());
            selectImageButton.onClick.AddListener(() => OnChooseImage?.Invoke());
        }

        public void SetData(PinData pin, Sprite sprite = null)
        {
            currentPin = pin;
            titleInput.text = pin.name;
            descriptionInput.text = pin.description;
            if (sprite != null) pinImage.sprite = sprite;
        }

        public void ResetText()
        {
            titleInput.text = "";
            descriptionInput.text = "";
            // pinImage не трогаем
        }
    }
}





