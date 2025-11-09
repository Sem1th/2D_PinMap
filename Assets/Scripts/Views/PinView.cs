using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using TouristMap.Data;
using TouristMap.Views;
using TouristMap.Managers;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class PinView : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, 
    IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public PinData data;
    private Camera mainCam;
    private float pressStartTime;
    private bool isPressed = false;
    private bool isDragging = false;
    public bool IsDragging => isDragging;
    public static bool AnyDragging = false;

    private Vector3 originalScale;  // Исходный масштаб пина

    private void Awake()
    {
        mainCam = Camera.main;
        originalScale = transform.localScale;   // Сохраняем исходный масштаб пина
    }

    private void OnEnable()
    {
        // Возвращаем масштаб в исходное состояние при повторной активации
        transform.localScale = originalScale;

        if (data != null)
        {
            transform.position = new Vector3(data.position.x, data.position.y, 0);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = false;
        pressStartTime = Time.time;
        isPressed = true;

        transform.DOKill();   // Сбрас всех старых анимаций
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isDragging = true;
            AnyDragging = true;

            transform.DOKill(); // Без этого будет дёргаться
            transform.DOScale(originalScale * 1.4f, 0.2f).SetEase(Ease.OutQuad);
        }

        Vector3 worldPos = mainCam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragging();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
        {
            // Короткое нажатие — открыть пин
            Debug.Log($"Pin clicked: {data.name}");
            
            // Способ 1: Прямой вызов PopupController (надежный)
            if (PopupController.Instance != null)
            {
                Debug.Log("Direct call to PopupController.ShowPreview");
                PopupController.Instance.ShowPreview(data);
            }
            else
            {
                Debug.LogError("PopupController.Instance is NULL!");
            }
            
            // Способ 2: Через PinManager (для других подписчиков)
            PinManager.Instance.SelectPin(data);
        }
    }

    private void EndDragging()
    {
        isDragging = false;
        AnyDragging = false;

        transform.DOKill();
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);

        // Сохраняем позицию
        data.position = transform.position;
        PinManager.Instance.AddOrUpdatePin(data);
    }

    // Пульсация при наведении курсора
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;

        transform.DOKill();
        transform.DOScale(originalScale * 1.15f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;

        transform.DOKill();
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuad);
    }
}


