using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BattleUIHandler : MonoBehaviour
{
	//--------------------------------
	// Map参照
	//--------------------------------

	MapManager map;

	//--------------------------------
	// UI参照
	//--------------------------------

	[SerializeField] Button confirmAttackButton;
	[SerializeField] Button cancelAttackButton;
	[SerializeField] TextMeshProUGUI confirmMessageText;
	[SerializeField] TextMeshProUGUI attackerListText;
	[SerializeField] TextMeshProUGUI defenderListText;
	[SerializeField] TextMeshProUGUI powerText;


	//--------------------------------
	// 初期化（重要）
	//--------------------------------

	public void Initialize(
		MapManager mapManager)
	{
		map = mapManager;
	}

	//--------------------------------
	// ボタン処理
	//--------------------------------

	public void OnConfirmAttack()
	{
		map.confirmAttackPanel.SetActive(false);
		map.ConfirmAttackSub();
		//battleManager.TryAttack(
		//	pendingFrom,
		//	pendingTarget,
		//	pendingAttackers);

	}
	public void OnCancelAttack()
	{
		map.confirmAttackPanel.SetActive(false);

		Debug.Log("攻撃キャンセル");
	}
	//public void OnConfirmAction()
	//{
	//	if (selectedNode == null ||
	//		toProvince == null ||
	//		selectedCharacters.Count == 0)
	//	{
	//		Debug.Log("選択不足");
	//		return;
	//	}

	//	map.TryAction(
	//		selectedProvince,
	//		toProvince,
	//		selectedCharacters);

	//	map.ResetSelection();
	//}

	//--------------------------------
	// メッセージ表示
	//--------------------------------
	public void SetMessageText(
		string messageText)
	{
		if (confirmMessageText == null)
		{
			Debug.Log("confirmMessageText Null");
		}

		confirmMessageText.text = "Test MessageText";
		confirmMessageText.text = 
			messageText + " を攻撃しますか？";
	}

	public void SetAttackerListText(
		List<CharacterRuntimeData> attackers)
	{
		string attackerText = "攻撃側：\n";

		foreach (var ch in attackers)
		{
			attackerText +=
				ch.baseData.characterName + "\n";
		}

		attackerListText.text = attackerText;
	}

	public void SetDefenderListText(
		List<CharacterRuntimeData> defenders)
	{
		string defenderText = "防御側：\n";

		foreach (var ch
			in defenders)
		{
			defenderText +=
				ch.baseData.characterName + "\n";
		}

		defenderListText.text = defenderText;
	}

	public void SetPowerText(
		int atkPower,int defPower)
	{
		powerText.text = 
			"攻撃力：" + atkPower +
			"\n防御力：" + defPower;
	}

}
