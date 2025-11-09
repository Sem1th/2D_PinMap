using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TouristMap.Data;
using TouristMap.Services;
using TouristMap.Managers;

namespace TouristMap.Views
{
    public class PopupMediaService
    {
        private readonly IImageLoader imageLoader;
        private readonly IAudioLoader audioLoader;

        public PopupMediaService(IImageLoader imgLoader, IAudioLoader audLoader)
        {
            imageLoader = imgLoader;
            audioLoader = audLoader;
        }

        public void LoadImage(string path, Image targetImage, System.Action<Sprite> onComplete = null)
        {
            if (targetImage == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                return;
            }

            // Запускаем корутину загрузчика
            PopupController.Instance.StartCoroutine(LoadImageRoutine(path, targetImage, onComplete));
        }

        private IEnumerator LoadImageRoutine(string path, Image targetImage, System.Action<Sprite> onComplete)
        {
            // Загружаем файл
            yield return imageLoader.LoadImage(path, targetImage);

            // После загрузки fileLoader УЖЕ УСТАНОВИЛ targetImage.sprite
            onComplete?.Invoke(targetImage.sprite);
        }

        public void LoadAudio(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            PopupController.Instance.StartCoroutine(LoadAudioRoutine(path));
        }

        private IEnumerator LoadAudioRoutine(string path)
        {
            yield return audioLoader.LoadAudio(path, PopupController.Instance.GetComponent<AudioSource>());
        }
    }
}