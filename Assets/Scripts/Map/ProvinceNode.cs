using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProvinceNode : MonoBehaviour
{
	public ProvinceData provinceData;

	private MapManager mapManager;

	Image image;
	Outline outline;

	TextMeshProUGUI countText;
	TextMeshProUGUI placeText;

	[SerializeField]
	Color selectedColor = Color.yellow;

	[SerializeField]
	Color moveColor = Color.green;

	[SerializeField]
	Color attackColor = Color.red;


	void Awake()
	{
		mapManager =
			FindObjectOfType<MapManager>();

		// 修正：子オブジェクトも探す
		image = 
			GetComponent<Image>();

		//normalColor = image.color;

		if (image == null)
		{
			Debug.LogError(
				gameObject.name +
				" にImageが見つかりません");
		}

		countText =
			transform.Find("CountText")
			.GetComponent<TextMeshProUGUI>();

		Transform t1 =
			transform.Find("CountText");

		if (t1 != null)
		{
			countText =
				t1.GetComponent<
					TextMeshProUGUI>();
		}

		if (countText == null)
		{
			Debug.LogError(
				gameObject.name +
				" CountTextが見つかりません");
		}

		Transform t2 =
			transform.Find("PlaceText");

		if (t2 != null)
		{
			placeText =
				t2.GetComponent<
					TextMeshProUGUI>();

			UpdatePlaceName();
		}

		if (placeText == null)
		{
			Debug.LogError(
				gameObject.name +
				" PlaceTextが見つかりません");
		}

		outline =
			GetComponent<Outline>();

		if (outline != null)
		{
			outline.enabled = false;
		}

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
		FactionData faction)
	{
		if (image == null)
		{
			Debug.LogError(
				gameObject.name +
				" imageがnull");
			return;
		}

		if (faction == null)
		{
			Debug.LogError(
				gameObject.name +
				" factionがnull");
			return;
		}

		image.color =
			faction.factionColor;

		// 枠線消す
		HideOutline();
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

	public void SetSelected(bool selected)
	{
		if (image == null)
			return;

		if (selected)
		{
			image.color = selectedColor;

			ShowOutline(selectedColor);
		}
		else
		{

		}
	}

	public void SetMoveHighlight()
	{
		if (image != null)
		{
			image.color = moveColor;
			ShowOutline(moveColor);
		}
	}

	public void SetAttackHighlight()
	{
		if (image != null)
		{
			image.color = attackColor;
			ShowOutline(attackColor);
		}
	}
	public void ShowOutline(Color color)
	{
		if (outline == null)
			return;

		outline.enabled = true;

		outline.effectColor = color;
	}

	public void HideOutline()
	{
		if (outline == null)
			return;

		outline.enabled = false;
	}

	public void OnClick()
	{
		//mapManager.OnProvinceClicked(
		//	provinceData);
		mapManager.OnProvinceClicked(
			this);

	}
}