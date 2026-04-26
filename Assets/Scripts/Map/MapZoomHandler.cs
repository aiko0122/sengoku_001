using UnityEngine;

public class MapZoomHandler :
	MonoBehaviour
{
	RectTransform rect;

	public float zoomSpeed = 0.1f;

	public float minScale = 0.5f;
	public float maxScale = 2.0f;

	void Awake()
	{
		rect =
			GetComponent<RectTransform>();
	}

	void Update()
	{
		float scroll =
			Input.mouseScrollDelta.y;

		if (scroll == 0)
			return;

		float scale =
			rect.localScale.x;

		scale += scroll * zoomSpeed;

		scale =
			Mathf.Clamp(
				scale,
				minScale,
				maxScale);

		rect.localScale =
			new Vector3(
				scale,
				scale,
				1f);
	}
}