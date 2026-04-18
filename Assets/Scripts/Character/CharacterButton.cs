using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterButton : MonoBehaviour
{
	[SerializeField]
	TextMeshProUGUI nameText;

	[SerializeField]
	Image backgroundImage;

	CharacterRuntimeData character;

	MapManager mapManager;

	bool isSelected = false;

	// 色設定
	Color normalColor = Color.white;
	Color selectedColor = Color.yellow;

	public void Setup(
		CharacterRuntimeData ch,
		MapManager manager)
	{
		character = ch;

		mapManager = manager;

		nameText.text =
			ch.baseData.characterName;

		// 初期色
		SetNormal();

	}

	public void OnClick()
	{
		// ★ トグル動作
		isSelected = !isSelected;

		if (isSelected)
		{
			SetSelected();

			mapManager.AddSelectedCharacter(
				character);
		}
		else
		{
			SetNormal();

			mapManager.RemoveSelectedCharacter(
				character);
		}
	}

	public void SetSelected()
	{
		backgroundImage.color =
			selectedColor;
	}

	public void SetNormal()
	{
		backgroundImage.color =
			normalColor;
	}
	public void ResetSelection()
	{
		isSelected = false;
		SetNormal();
	}
}