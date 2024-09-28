using UnityEngine;
using UnityEngine.EventSystems;

public class TestPointer : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer down detected on " + gameObject.name);
    }
}
