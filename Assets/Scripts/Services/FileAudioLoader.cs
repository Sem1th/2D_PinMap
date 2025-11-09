using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TouristMap.Services
{
    public class FileAudioLoader : MonoBehaviour, IAudioLoader
    {
        private readonly Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, Coroutine> activeCoroutines = new Dictionary<string, Coroutine>();
        private readonly Dictionary<AudioSource, string> audioSourceToRequestMap = new Dictionary<AudioSource, string>();

        public IEnumerator LoadAudio(string path, AudioSource targetAudioSource)
        {
            if (targetAudioSource == null) yield break;

            // Отменяем предыдущую загрузку для этого AudioSource
            CancelLoad(targetAudioSource);

            // Останавливаем текущее воспроизведение и очищаем клип
            targetAudioSource.Stop();
            SafeSetAudioClip(targetAudioSource, null);

            if (string.IsNullOrEmpty(path) || !File.Exists(path)) yield break;

            // Проверяем кэш
            if (audioCache.TryGetValue(path, out var cachedClip))
            {
                SafeSetAudioClip(targetAudioSource, cachedClip);
                targetAudioSource.Play();
                yield break;
            }

            // Запускаем новую загрузку
            var coroutine = StartCoroutine(LoadAudioCoroutine(path, targetAudioSource));
            activeCoroutines[path] = coroutine;
            audioSourceToRequestMap[targetAudioSource] = path;
        }

        private IEnumerator LoadAudioCoroutine(string path, AudioSource targetAudioSource)
        {
            using var request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.UNKNOWN);
            yield return request.SendWebRequest();

            // Убираем из активных запросов
            activeCoroutines.Remove(path);
            if (audioSourceToRequestMap.TryGetValue(targetAudioSource, out var currentPath) && currentPath == path)
                audioSourceToRequestMap.Remove(targetAudioSource);

            // Проверяем, не был ли target уничтожен во время загрузки
            if (targetAudioSource == null) yield break;

            if (request.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(request);

                // Сохраняем в кэш
                if (!audioCache.ContainsKey(path))
                    audioCache[path] = clip;

                SafeSetAudioClip(targetAudioSource, clip);
                targetAudioSource.Play();
            }
        }

        // Добавляем публичный метод для отмены загрузки
        public void CancelLoad(AudioSource targetAudioSource)
        {
            if (targetAudioSource != null && audioSourceToRequestMap.TryGetValue(targetAudioSource, out var path))
            {
                if (activeCoroutines.TryGetValue(path, out var coroutine))
                {
                    StopCoroutine(coroutine);
                    activeCoroutines.Remove(path);
                }
                audioSourceToRequestMap.Remove(targetAudioSource);

                // Останавливаем воспроизведение
                targetAudioSource.Stop();
            }
        }

        private void SafeSetAudioClip(AudioSource targetAudioSource, AudioClip newClip)
        {
            if (targetAudioSource == null) return;

            // Очищаем предыдущий клип, если он не закэширован
            var currentClip = targetAudioSource.clip;
            if (currentClip != null && currentClip != newClip && !IsAudioCached(currentClip))
            {
                Destroy(currentClip);
            }

            targetAudioSource.clip = newClip;
        }

        private bool IsAudioCached(AudioClip clip)
        {
            foreach (var cachedClip in audioCache.Values)
            {
                if (cachedClip == clip)
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
            audioSourceToRequestMap.Clear();

            // Останавливаем все AudioSource, которые мы контролируем
            foreach (var audioSource in audioSourceToRequestMap.Keys)
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                    SafeSetAudioClip(audioSource, null);
                }
            }

            // Очищаем кэш аудио
            foreach (var clip in audioCache.Values)
            {
                if (clip != null)
                    Destroy(clip);
            }
            audioCache.Clear();
        }

        private void OnDestroy()
        {
            ClearCache();
        }
    }
}


