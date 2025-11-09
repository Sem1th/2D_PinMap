using System;
using UnityEngine;
using System.Collections.Generic;
using TouristMap.Data;
using TouristMap.Storage;

namespace TouristMap.Managers
{
    public class PinManager : MonoBehaviour
    {
        public static PinManager Instance { get; private set; }

        private Dictionary<string, PinData> pinsCache = new Dictionary<string, PinData>();
        private ChunkedGZipStorage storage;

        public event Action<PinData> OnPinSelected;
        public event Action OnPinsUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            storage = new ChunkedGZipStorage();
            storage.ChunkSize = 500;
        }

        public void AddOrUpdatePin(PinData pin)
        {
            pinsCache[pin.id] = pin;
            storage.SavePin(pin);
            OnPinsUpdated?.Invoke();
        }

        public void RemovePin(PinData pin)
        {
            storage.DeletePin(pin);
            OnPinsUpdated?.Invoke();
        }

        public void SelectPin(PinData pin)
        {
            //Debug.Log($"PinManager.SelectPin called: {pin?.name}");
            //Debug.Log($"OnPinSelected subscribers: {OnPinSelected?.GetInvocationList()?.Length ?? 0}");
            OnPinSelected?.Invoke(pin);
        }

        public PinData[] GetPinsInView(Vector2 min, Vector2 max)
        {
            var pins = storage.GetPinsInBounds(min, max);

            for (int i = 0; i < pins.Length; i++)
            {
                var id = pins[i].id;

                if (pinsCache.TryGetValue(id, out var cachedPin))
                {
                    pins[i] = cachedPin;
                }
                else
                {
                    pinsCache[id] = pins[i];
                }
            }

            return pins;
        }
    }
}
