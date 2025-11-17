using UnityEngine;
using UnityEngine.EventSystems;

public class AudioButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound("ButtonHover");
        Debug.Log("Hovering over button");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound("ButtonClick");
        Debug.Log("Button clicked");
    }
}