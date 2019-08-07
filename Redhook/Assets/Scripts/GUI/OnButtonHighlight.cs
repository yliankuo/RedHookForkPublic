using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Sprite sprite;
    private Sprite original;

    public void Start()
    {
        original = image.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.sprite = sprite;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = original;
    }
}