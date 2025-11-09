using UnityEngine;
using TouristMap.Data;

namespace TouristMap.Views
{
    public class PopupViewManager
    {
        private readonly PopupPreviewView previewView;
        private readonly PopupDetailView detailView;
        private readonly PopupEditView editView;
        private readonly PopupDeleteConfirmView deleteConfirmView;
        private readonly Sprite defaultPinSprite;

        public PopupViewManager(
            PopupPreviewView preview,
            PopupDetailView detail,
            PopupEditView edit,
            PopupDeleteConfirmView deleteConfirm,
            Sprite defaultSprite)
        {
            previewView = preview;
            detailView = detail;
            editView = edit;
            deleteConfirmView = deleteConfirm;
            defaultPinSprite = defaultSprite;
        }

        #region Reset Views

        public void ResetView(PopupState state)
        {
            switch (state)
            {
                case PopupState.Preview: previewView.ResetText(); break;
                case PopupState.Detail: detailView.ResetText(); break;
                case PopupState.Edit: editView.ResetText(); break;
            }
        }

        #endregion

        #region Setup Views

        public void SetupPreviewView(string title, string description) =>
            previewView.SetData(title, description);

        public void SetupDetailView(string title, string description, Sprite sprite) =>
            detailView.SetData(title, description, sprite);

        public void SetupEditView(PinData pin, Sprite sprite) =>
            editView.SetData(pin, sprite);

        #endregion

        #region Image Handling

        public UnityEngine.UI.Image GetPinImage(PopupState state) =>
            state switch
            {
                PopupState.Preview => previewView.PinImage,
                PopupState.Detail => detailView.PinImage,
                PopupState.Edit => editView.PinImage,
                _ => null
            };

        public void SetPinImage(PopupState state, UnityEngine.Sprite sprite)
        {
            var image = GetPinImage(state);
            if (image != null)
                image.sprite = sprite ?? defaultPinSprite;
        }

        #endregion
    }
}






