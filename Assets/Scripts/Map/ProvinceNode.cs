using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvinceNode : MonoBehaviour
{
	//--------------------------------
	// Jsonと接続するID
	//--------------------------------

	[Header("Province ID")]
	public string provinceId;

	//--------------------------------
	// UI参照
	//--------------------------------

	[Header("UI References")]
	[SerializeField] Image baseImage;
	[SerializeField]
		List<Sprite>
		defenseOverlaySprites;
	[SerializeField] Image highlightOverlay;
	[SerializeField] Image lockedOverlay;

	[SerializeField] TextMeshProUGUI countText;
	[SerializeField] TextMeshProUGUI placeText;

	[SerializeField] Color selectedColor = Color.yellow;
	[SerializeField] Color moveColor = Color.green;
	[SerializeField] Color attackColor = Color.red;
	[SerializeField] Color lockedColor;

	//--------------------------------
	// 内部
	//--------------------------------

	private MapManager mapManager;

	//--------------------------------
	// 初期化
	//--------------------------------
	void Awake()
	{
		mapManager =
			FindObjectOfType<MapManager>();

		//if (baseImage == null)
		//{
		//	Debug.LogError(
		//		gameObject.name +
		//		" BaseImage未設定");
		//}

		//var count =
		//	transform.Find("CountText");

		//if (count != null)
		//{
		//	countText =
		//		count.GetComponent<
		//			TextMeshProUGUI>();
		//}

		//var place =
		//	transform.Find("PlaceText");

		//if (place != null)
		//{
		//	placeText =
		//		place.GetComponent<
		//			TextMeshProUGUI>();

		//	UpdatePlaceName();
		//}

		//UpdateDefenseOverlay(provinceData.initialDefenseLevel);
	}

	//--------------------------------
	// クリック
	//--------------------------------

	public void OnClick()
	{
		if (mapManager != null)
		{
			mapManager
				.OnProvinceClicked(this);
		}
	}
	//--------------------------------
	// ロック表示
	//--------------------------------

	public void SetLocked(
		bool locked)
	{
		if (lockedOverlay != null)
		{
			lockedOverlay
				.gameObject
				.SetActive(locked);
		}
	}

	//--------------------------------
	// 所有色変更
	//--------------------------------

	public void SetFactionColor(
		Color color)
	{
		if (baseImage != null)
		{
			baseImage.color =
				color;
		}
	}

	//--------------------------------
	// 名前表示
	//--------------------------------

	public void SetNameText(
		string name)
	{
		if (placeText != null)
		{
			placeText.text =
				name;
		}
	}

	public void UpdateColor(
		ProvinceRuntimeData runtime)
		//FactionData faction)
	{
		if (baseImage == null)
		{
			Debug.LogError(
				gameObject.name +
				" baseImageがnull");
			return;
		}

		FactionRuntimeData faction = runtime.owner;

		if (faction == null)
		{
			Debug.LogError(
				gameObject.name +
				" factionがnull");
			return;
		}

		Debug.Log("Test 色" +
			faction.baseData.factionName +
			faction.color);

		baseImage.color =
			faction.color;

		if (!runtime.isUnlocked)
		{
			
			baseImage.color = lockedColor;
			return;
		}
		// 枠線消す
		//HideOutline();
		//ClearHighlight();

	}
	public void UpdateCount(int count)
	{
		if (countText == null)
		{
			Debug.LogError(
				gameObject.name +
				" countTextがnull");

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

		//Debug.Log(provinceData.provinceName + stkDefenseLevel);

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

	//--------------------------------
	// ハイライト制御
	//--------------------------------

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

}