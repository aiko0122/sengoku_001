using UnityEngine;
using UnityEngine.EventSystems;

public class MapDragHandler :
	MonoBehaviour,
	IDragHandler
{
	RectTransform rect;

	void Awake()
	{
		rect =
			GetComponent<RectTransform>();
	}

	public void OnDrag(
		PointerEventData eventData)
	{
		rect.anchoredPosition +=
			eventData.delta;
	}
}