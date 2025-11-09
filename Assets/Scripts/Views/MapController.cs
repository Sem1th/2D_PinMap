using UnityEngine;
using UnityEngine.EventSystems;
using TouristMap.Managers;
using TouristMap.Data;
using System.Collections.Generic;

namespace TouristMap.Views
{
    public class MapController : MonoBehaviour
    {
        [SerializeField] private GameObject pinPrefab;
        [SerializeField] private Transform pinsContainer;
        [SerializeField] private float updateInterval = 0.5f;

        private Dictionary<string, PinView> activePins = new Dictionary<string, PinView>();
        private Camera cam;
        private float timer;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Start()
        {
            PinManager.Instance.OnPinsUpdated += RefreshVisiblePins;
            RefreshVisiblePins();
        }

        private void OnDestroy()
        {
            if (PinManager.Instance)
                PinManager.Instance.OnPinsUpdated -= RefreshVisiblePins;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0;
                RefreshVisiblePins();
            }

            if (Input.GetMouseButtonDown(0))
            {
                // Единая проверка всего UI за один Raycast
                if (IsPointerOverUIOrPin())
                    return;

                Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                var pin = new PinData { position = worldPos };
                PopupController.Instance.ShowEditPopup(pin, true);
            }
        }

        /// <summary>
        /// Единая проверка - находится ли курсор над любым UI-элементом ИЛИ пином
        /// Заменяет два отдельных метода одной проверкой
        /// </summary>
        private bool IsPointerOverUIOrPin()
        {
            // Если нет EventSystem, считаем что UI нет
            if (EventSystem.current == null)
                return false;

            PointerEventData pointer = new PointerEventData(EventSystem.current) 
            { 
                position = Input.mousePosition 
            };
            
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            // Проверяем все результаты за один проход
            foreach (var result in results)
            {
                // Если нашли пин - сразу возвращаем true
                if (result.gameObject.GetComponent<PinView>() != null)
                    return true;
                    
                // Или если нашли любой другой UI-элемент (кнопки, панели, и т.д.)
                if (result.gameObject.GetComponentInParent<UnityEngine.UI.Graphic>() != null)
                    return true;
            }

            return false;
        }

        private void RefreshVisiblePins()
        {
            if (PinView.AnyDragging) return; // FULL STOP — не обновляем пины пока игрок двигает один из них

            Vector2 center = cam.transform.position;
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            Vector2 worldSize = cam.ScreenToWorldPoint(screen) - cam.ScreenToWorldPoint(Vector2.zero);
            Vector2 min = center - worldSize * 0.6f;
            Vector2 max = center + worldSize * 0.6f;

            // Получаем актуальные пины
            var pins = PinManager.Instance.GetPinsInView(min, max);

            // Отмечаем какие id были обновлены
            HashSet<string> updatedPins = new HashSet<string>();
            
            foreach (var pin in pins)
            {
                // Уже есть активный визуальный пин?
                if (activePins.TryGetValue(pin.id, out var view))
                {
                    // Если его НЕ тащат — обновляем позицию
                    if (!view.IsDragging)
                    {
                        view.transform.position = new Vector3(pin.position.x, pin.position.y, 0);
                    }
                    updatedPins.Add(pin.id);
                }
                else
                {
                    // Создаём новый
                    var obj = Instantiate(pinPrefab, pinsContainer);
                    obj.transform.position = new Vector3(pin.position.x, pin.position.y, 0);
                    var newView = obj.GetComponent<PinView>();
                    newView.data = pin;
                    activePins.Add(pin.id, newView);
                    updatedPins.Add(pin.id);
                }
            }

            // Удаляем пины, которых больше нет в актуальном списке
            List<string> toRemove = new List<string>();
            foreach (var kvp in activePins)
            {
                if (!updatedPins.Contains(kvp.Key))
                {
                    Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var id in toRemove)
                activePins.Remove(id);
        }
    }
}

