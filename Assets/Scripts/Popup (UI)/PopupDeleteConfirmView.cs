using System;
using UnityEngine;
using UnityEngine.UI;

namespace TouristMap.Views
{
    public class PopupDeleteConfirmView : MonoBehaviour
    {
        public event Action OnYes;
        public event Action OnNo;

        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            yesButton.onClick.AddListener(() => OnYes?.Invoke());
            noButton.onClick.AddListener(() => OnNo?.Invoke());
            closeButton.onClick.AddListener(() => OnNo?.Invoke());
        }
    }
}



