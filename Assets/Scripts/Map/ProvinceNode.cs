using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceNode : MonoBehaviour
{
	public ProvinceData provinceData;

	private MapManager mapManager;

	//Image image;
	//Outline outline;

	[SerializeField] Image baseImage;
	[SerializeField] Image highlightOverlay;
	[SerializeField] Image lockedOverlay;

	//[SerializeField] Image defenseOverlay;

	[SerializeField]
		List<Sprite>
		defenseOverlaySprites;

	TextMeshProUGUI countText;
	TextMeshProUGUI placeText;

	[SerializeField]
	Color selectedColor = Color.yellow;

	[SerializeField]
	Color moveColor = Color.green;

	[SerializeField]
	Color attackColor = Color.red;

	[SerializeField]
	Color lockedColor;

	void Awake()
	{
		mapManager =
			FindObjectOfType<MapManager>();

		if (baseImage == null)
		{
			Debug.LogError(
				gameObject.name +
				" BaseImageñ¢ê›íË");
		}

		//if (defenseOverlay == null)
		//{
		//	Debug.LogError(
		//		gameObject.name +
		//		" DefenseOverlayñ¢ê›íË");
		//}

		var count =
			transform.Find("CountText");

		if (count != null)
		{
			countText =
				count.GetComponent<
					TextMeshProUGUI>();
		}

		var place =
			transform.Find("PlaceText");

		if (place != null)
		{
			placeText =
				place.GetComponent<
					TextMeshProUGUI>();

			UpdatePlaceName();
		}

		UpdateDefenseOverlay(provinceData.initialDefenseLevel);
	}
	public void UpdatePlaceName()
	{
		if (placeText == null)
			return;

		if (provinceData == null)
			return;

		placeText.text =
			provinceData.provinceName;
	}

	public void UpdateColor(
		ProvinceRuntimeData runtime)
		//FactionData faction)
	{
		if (baseImage == null)
		{
			Debug.LogError(
				gameObject.name +
				" baseImageÇ™null");
			return;
		}

		FactionData faction = runtime.ownerFaction;

		if (faction == null)
		{
			Debug.LogError(
				gameObject.name +
				" factionÇ™null");
			return;
		}

		baseImage.color =
			faction.factionColor;

		if (!runtime.isUnlocked)
		{
			
			baseImage.color = lockedColor;
			return;
		}
		// ògê¸è¡Ç∑
		//HideOutline();
		//ClearHighlight();

	}
	public void UpdateCount(int count)
	{
		if (countText == null)
		{
			Debug.LogError(
				gameObject.name +
				" countTextÇ™null");

			return;
		}

		countText.text =
			count.ToString();
	}

	public void SetSelected(
		bool selected)
	{
		if (highlightOverlay == null)
			return;

		if (selected)
		{
			SetHighlight(
				selectedColor, 0.4f);

			Debug.Log("selected = True");
		}
		else
		{
			SetHighlightAlpha(0f);
			Debug.Log("selected = False");
		}
	}
	public void SetMoveHighlight()
	{
		SetHighlight(
			moveColor,
				0.3f);
	}

	public void SetAttackHighlight()
	{
		SetHighlight(
			attackColor,
				0.3f);
	}

	public void ClearHighlight()
	{
		SetHighlightAlpha(0f);
	}

	//public void ShowOutline(Color color)
	//{
	//	if (outline == null)
	//		return;

	//	outline.enabled = true;

	//	outline.effectColor = color;
	//}

	//public void HideOutline()
	//{
	//	//if (outline == null)
	//	//	return;

	//	//outline.enabled = false;

	//	//highlightOverlay;
	//}

	public void UpdateDefenseOverlay(
		int defenseLevel)
	{
		int stkDefenseLevel = 0;

		//if (defenseOverlay == null)
		//	return;

		if (defenseOverlaySprites == null)
			return;

		if (defenseLevel < 3) { stkDefenseLevel = 0; }
		else if (defenseLevel < 8) { stkDefenseLevel = 1; }
		else { stkDefenseLevel = 2; }

		Debug.Log(provinceData.provinceName + stkDefenseLevel);

		if (stkDefenseLevel >=
			defenseOverlaySprites.Count)
		{
			stkDefenseLevel =
				defenseOverlaySprites.Count - 1;
		}

		baseImage.sprite =
			defenseOverlaySprites[
				stkDefenseLevel];
		//defenseOverlay.sprite =
		//	defenseOverlaySprites[
		//		stkDefenseLevel];
	}

	public void SetHighlight(
		Color color,
		float alpha)
	{
		if (highlightOverlay == null)
			return;

		color.a = alpha;

		highlightOverlay.color =
			color;
	}

	public void SetHighlightAlpha(
		float alpha)
	{
		if (highlightOverlay == null)
			return;

		var color =
			highlightOverlay.color;

		color.a = alpha;

		highlightOverlay.color =
			color;
	}

	public void OnClick()
	{
		//mapManager.OnProvinceClicked(
		//	provinceData);
		Debug.Log("Province Click");

		mapManager.OnProvinceClicked(
			this);

	}
}