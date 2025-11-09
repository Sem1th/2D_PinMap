using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TouristMap.Services
{
    public class FileImageLoader : MonoBehaviour, IImageLoader
    {
        private Sprite defaultSprite;
        private readonly Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
        private readonly Dictionary<Image, string> imageToRequestMap = new Dictionary<Image, string>();

        // Добавляем метод Initialize вместо конструктора с параметром
        public void Initialize(Sprite defaultSprite)
        {
            this.defaultSprite = defaultSprite;
        }

        public IEnumerator LoadImage(string path, Image targetImage, Sprite fallback = null)
        {
            if (targetImage == null) yield break;

            // Отменяем предыдущую загрузку для этого target
            CancelLoad(targetImage);

            // Устанавливаем fallback или default спрайт
            var fallbackSprite = fallback != null ? fallback : defaultSprite;
            SafeSetSprite(targetImage, fallbackSprite, null);

            if (string.IsNullOrEmpty(path)) yield break;

            // Проверяем кэш
            if (imageCache.TryGetValue(path, out var cachedSprite))
            {
                SafeSetSprite(targetImage, cachedSprite, fallbackSprite);
                yield break;
            }

            // Запускаем новую загрузку
            var coroutine = StartCoroutine(LoadImageCoroutine(path, targetImage, fallbackSprite));
            activeCoroutines[path] = coroutine;
            imageToRequestMap[targetImage] = path;
        }

        private IEnumerator LoadImageCoroutine(string path, Image targetImage, Sprite fallbackSprite)
        {
            using var request = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return request.SendWebRequest();

            // Убираем из активных запросов
            activeCoroutines.Remove(path);
            if (imageToRequestMap.TryGetValue(targetImage, out var currentPath) && currentPath == path)
                imageToRequestMap.Remove(targetImage);

            // Проверяем, не был ли target уничтожен во время загрузки
            if (targetImage == null) yield break;

            if (request.result == UnityWebRequest.Result.Success)
            {
                var tex = DownloadHandlerTexture.GetContent(request);
                var sprite = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    Vector2.one * 0.5f
                );

                // Сохраняем в кэш
                if (!imageCache.ContainsKey(path))
                    imageCache[path] = sprite;

                SafeSetSprite(targetImage, sprite, fallbackSprite);
            }
            else
            {
                SafeSetSprite(targetImage, fallbackSprite, null);
            }
        }

        public void CancelLoad(Image targetImage)
        {
            if (targetImage != null && imageToRequestMap.TryGetValue(targetImage, out var path))
            {
                if (activeCoroutines.TryGetValue(path, out var coroutine))
                {
                    StopCoroutine(coroutine);
                    activeCoroutines.Remove(path);
                }
                imageToRequestMap.Remove(targetImage);
            }
        }

        private void SafeSetSprite(Image targetImage, Sprite newSprite, Sprite previousSprite)
        {
            if (targetImage == null) return;

            // Очищаем предыдущий спрайт, если он не дефолтный и не закэширован
            var currentSprite = targetImage.sprite;
            if (currentSprite != null && 
                currentSprite != defaultSprite && 
                currentSprite != previousSprite && 
                !IsSpriteCached(currentSprite))
            {
                Destroy(currentSprite);
            }

            targetImage.sprite = newSprite;
        }

        private bool IsSpriteCached(Sprite sprite)
        {
            foreach (var cachedSprite in imageCache.Values)
            {
                if (cachedSprite == sprite)
                    return true;
            }
            return false;
        }

        public void ClearCache()
        {
            // Останавливаем все активные корутины
            foreach (var coroutine in activeCoroutines.Values)
            {
                StopCoroutine(coroutine);
            }
            activeCoroutines.Clear();
            imageToRequestMap.Clear();

            // Очищаем кэш спрайтов
            foreach (var sprite in imageCache.Values)
            {
                if (sprite != null && sprite != defaultSprite)
                    Destroy(sprite);
            }
            imageCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }
    }
}



