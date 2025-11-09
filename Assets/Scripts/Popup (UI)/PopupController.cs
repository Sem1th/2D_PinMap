using UnityEngine;
using TouristMap.Data;
using TouristMap.Managers;
using TouristMap.Services;
using System;
using UnityEngine.UI;

namespace TouristMap.Views
{
    public class PopupController : MonoBehaviour
    {
        public static PopupController Instance { get; private set; }

        [Header("UI Views")]
        [SerializeField] private PopupPreviewView previewView;
        [SerializeField] private PopupDetailView detailView;
        [SerializeField] private PopupEditView editView;
        [SerializeField] private PopupDeleteConfirmView deleteConfirmView;

        [Header("Services")]
        [SerializeField] private Sprite defaultPinSprite;
        [SerializeField] private AudioSource audioSource;

        private PopupStateMachine stateMachine;
        private PopupAnimationService animationService;
        private PopupViewManager viewManager;
        private PopupMediaService mediaService;
        private IImageLoader imageLoader;
        private IAudioLoader audioLoader;

        private void Awake()
        {
            InitializeSingleton();
            InitializeServices();
            SetupEventSubscriptions();
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void InitializeServices()
        {
            // Инициализация загрузчиков
            var fileImageLoader = gameObject.AddComponent<FileImageLoader>();
            fileImageLoader.Initialize(defaultPinSprite);
            imageLoader = fileImageLoader;
            audioLoader = gameObject.AddComponent<FileAudioLoader>();

            // Сервисы
            stateMachine = new PopupStateMachine();
            animationService = new PopupAnimationService();
            viewManager = new PopupViewManager(previewView, detailView, editView, deleteConfirmView, defaultPinSprite);
            mediaService = new PopupMediaService(imageLoader, audioLoader);

            stateMachine.OnStateChanged += OnStateChanged;
        }

        private void SetupEventSubscriptions()
        {
            previewView.OnClose += HideAll;
            previewView.OnReadMore += ShowDetails;

            detailView.OnClose += HideAll;
            detailView.OnEdit += ShowEdit;
            detailView.OnDelete += ShowDeleteConfirm;

            editView.OnCancel += HideAll;
            editView.OnChooseImage += ChooseImage;
            editView.OnSave += OnPinSaved;

            deleteConfirmView.OnYes += ConfirmDelete;
            deleteConfirmView.OnNo += ShowPreviewAfterCancelDelete;
        }

        #region Public API

        public void ShowPreview(PinData pin)
        {
            stateMachine.ChangeState(PopupState.Preview, pin);
        }

        public void ShowEditPopup(PinData pin, bool isNew)
        {
            stateMachine.ChangeState(PopupState.Edit, pin);
        }

        public void HideAll()
        {
            stateMachine.Reset();
        }

        #endregion

        #region Event Handlers

        private void OnStateChanged(PopupState previousState, PopupState newState)
        {
            var previousView = GetViewObject(previousState);
            var newView = GetViewObject(newState);

            if (previousView != null)
            {
                animationService.HideView(previousView, () =>
                {
                    if (newView != null)
                        ShowState(newState);
                });
            }
            else
            {
                if (newView != null)
                    ShowState(newState);
            }
        }

        private void ShowState(PopupState state)
        {
            var viewObject = GetViewObject(state);
            if (viewObject == null) return;

            // Сразу заполняем текст и дефолтное изображение перед анимацией
            SetupStateContent(state);

            animationService.ShowView(viewObject, () => { });
        }

        private void SetupStateContent(PopupState state)
        {
            var pin = stateMachine.CurrentPin;
            if (pin == null) return;

            var targetImage = viewManager.GetPinImage(state);
            Sprite spriteToShow = string.IsNullOrEmpty(pin.imagePath)
                ? defaultPinSprite
                : null;

            switch (state)
            {
                case PopupState.Preview:
                    viewManager.SetupPreviewView(pin.name, pin.description);
                    viewManager.SetPinImage(PopupState.Preview, spriteToShow);

                    if (!string.IsNullOrEmpty(pin.imagePath))
                        mediaService.LoadImage(pin.imagePath, targetImage, loadedSprite =>
                        {
                            if (loadedSprite != null)
                                viewManager.SetPinImage(PopupState.Preview, loadedSprite);
                        });

                    mediaService.LoadAudio(pin.audioPath);
                    break;

                case PopupState.Detail:
                    viewManager.SetupDetailView(pin.name, pin.description, spriteToShow);

                    if (!string.IsNullOrEmpty(pin.imagePath))
                        mediaService.LoadImage(pin.imagePath, targetImage, loadedSprite =>
                        {
                            if (loadedSprite != null)
                                viewManager.SetPinImage(PopupState.Detail, loadedSprite);
                        });

                    mediaService.LoadAudio(pin.audioPath);
                    break;

                case PopupState.Edit:
                    viewManager.SetupEditView(pin, spriteToShow);

                    if (!string.IsNullOrEmpty(pin.imagePath))
                        mediaService.LoadImage(pin.imagePath, targetImage, loadedSprite =>
                        {
                            if (loadedSprite != null)
                                viewManager.SetPinImage(PopupState.Edit, loadedSprite);
                        });
                    break;
            }
        }

        private void ShowDetails() => stateMachine.ChangeState(PopupState.Detail, stateMachine.CurrentPin);
        private void ShowEdit() => stateMachine.ChangeState(PopupState.Edit, stateMachine.CurrentPin);
        private void ShowDeleteConfirm() => stateMachine.ChangeState(PopupState.DeleteConfirm, stateMachine.CurrentPin);

        private void OnPinSaved(PinData savedPin)
        {
            PinManager.Instance.AddOrUpdatePin(savedPin);
            stateMachine.ChangeState(PopupState.Preview, savedPin);
        }

        private void ConfirmDelete()
        {
            var pinToDelete = stateMachine.CurrentPin;
            if (pinToDelete != null)
                PinManager.Instance.RemovePin(pinToDelete);

            stateMachine.Reset();
        }

        private void ShowPreviewAfterCancelDelete() =>
            stateMachine.ChangeState(PopupState.Preview, stateMachine.CurrentPin);

        private void ChooseImage()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel("Выберите изображение", "", "png,jpg,jpeg");
            if (!string.IsNullOrEmpty(path))
            {
                stateMachine.CurrentPin.imagePath = path;
                var targetImage = viewManager.GetPinImage(PopupState.Edit);

                mediaService.LoadImage(path, targetImage, loadedSprite =>
                {
                    if (loadedSprite != null)
                        viewManager.SetPinImage(PopupState.Edit, loadedSprite);
                });
            }
#else
            Debug.Log("Выбор изображения в билде требует нативного плагина");
#endif
        }

        private GameObject GetViewObject(PopupState state)
        {
            return state switch
            {
                PopupState.Preview => previewView?.gameObject,
                PopupState.Detail => detailView?.gameObject,
                PopupState.Edit => editView?.gameObject,
                PopupState.DeleteConfirm => deleteConfirmView?.gameObject,
                _ => null
            };
        }

        #endregion
    }
}









