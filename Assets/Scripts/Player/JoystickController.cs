using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image joystickBackgroundImage;
    public Image joystickImage;
    private Vector2 inputVector;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        inputVector = Vector2.zero;
        joystickImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBackgroundImage.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / joystickBackgroundImage.rectTransform.sizeDelta.x * 1.5f);
            pos.y = 0;

            inputVector = new Vector2(pos.x, pos.y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            joystickImage.rectTransform.anchoredPosition = new Vector2(inputVector.x * (joystickBackgroundImage.rectTransform.sizeDelta.x / 2), inputVector.y * (joystickBackgroundImage.rectTransform.sizeDelta.y / 2));
        }
    }

    public Vector2 InputVector()
    {
        return new Vector2(Horizontal(), Vertical());
    }

    public float Horizontal()
    {
        if (inputVector.x != 0)
            return (inputVector.x < 0) ? -1 : 1;
        else return Input.GetAxis("Horizontal");
    }

    public float Vertical()
    {
        if (inputVector.y != 0)
            return (inputVector.y < 0) ? -1 : 1;
        else return Input.GetAxis("Vertical"); ;
    }
}
