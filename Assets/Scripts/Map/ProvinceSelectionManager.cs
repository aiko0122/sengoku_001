using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvinceSelectionManager
{
	MapManager map;
	ProvinceRuntimeData selectedP;

	public ProvinceSelectionManager(
		MapManager mapManager)
	{
		map = mapManager;
	}

	public void OnProvinceClicked(
		ProvinceNode node)
	{
		//Debug.Log("OnProvinceClicked");

		if (map.GetisGameOver()) return;

		if (!map.GetisPlayerTurn())
		{
			Debug.Log("敵ターン中");
			return;
		}

		var runtime =
			map.GetProvinceRuntime(
				node.provinceId);

		if (runtime == null)
		{
			Debug.LogError(
				"Province not found: "
				+ node.provinceId);

			return;
		}

		// =========================
		// ■1回目クリック（出発地選択）
		// =========================
		if (map.GetselectedNode() == null)
		{
			map.SetselectedProvince(runtime);
			map.SetSelectedNode(node);

			UpdateAllNodeColors();

			if (runtime.owner.baseData.factionId
				== Constants.playerFactionId)
			{
				//Debug.Log("勢力：Player");
				map.HighlightNeighbors(runtime);
			}
			else
			{
				Debug.Log("勢力：Player以外");
			}

			//Debug.Log(
			//	"出発地選択：" +
			//	runtime.baseData.provinceName);

			//Debug.Log(
			//	"地域情報メニュー表示：" +
			//	runtime.baseData.provinceName);

			return;
		}

		selectedP = map.GetselectedProvince();

		if (selectedP.owner.baseData.factionId
			!= Constants.playerFactionId)
		{
			map.ClearSelectedNode();
			UpdateAllNodeColors();
			map.ClearAllHighlights();
			return;
		}

		// =========================
		// ■同じ地域クリック → 開発メニュー
		// =========================
		if (map.GetselectedNode() == node)
		{
			map.SetselectedProvince(null);
			map.ClearSelectedNode();
			map.ClearAllHighlights();

			Debug.Log("選択解除");

			Debug.Log(
				"開発メニュー表示：" +
				runtime.baseData.provinceName);

			map.ShowDevelopmentMenu(runtime);

			return;
		}

		// =========================
		// ■2回目クリック（目的地選択）
		// =========================
		if (map.GettoNode() == null)
		{

			var targetProvince = runtime;

			if (!targetProvince.isUnlocked)
			{
				Debug.Log(
					targetProvince.baseData.provinceName +
					" は通行不可");
				return;
			}

			map.SettoNode(node);
			map.SettoPrpvince(targetProvince);

			//Debug.Log(
			//	"目的地選択：" +
			//	runtime.baseData.provinceName);

			// ★ここで武将選択を表示
			map.ShowCharacterList(map.GetselectedProvince());

			// ■必ずここで解除
			// ClearSelectedNode();
			// selectedProvince = null;

			UpdateAllNodeColors();
			map.ClearAllHighlights();

			return;
		}

		// =========================
		// ■3回目クリック（再選択 or リセット）
		// =========================
		map.ResetSelection();

	}

	public void CancelSelection()
	{
		if (map.GetselectedNode() == null)
			return;
		map.ResetSelection();
		//map.SetSelectedNodeFlag(false);

		// CharacterPanel を閉じる
		if (map.characterPanel != null)
		{
			map.characterPanel.SetActive(false);
		}

		map.ClearSelectedNode();
		map.SetselectedProvince(null);

		UpdateAllNodeColors();

		//Debug.Log("選択キャンセル");
	}

	public void UpdateAllNodeColors()
	{
		//Debug.Log("UpdateAllNodeColors");

		var nodes =
			map.GetProvinceNodes();

		foreach (var node in nodes)
		{

			var runtime =
				map.GetProvinceRuntime(node.provinceId);
				//runtimeData[node.provinceId];

			// 色更新
			node.UpdateColor(runtime);
			//runtime.owner);

			node.ClearHighlight();

			// 人数更新
			int count =
				runtime.characterList.Count;

			node.UpdateCount(count);

		}

		// 選択中ノードの上書き
		if (map.GetselectedNode() != null)
		{
			//Debug.Log("test True");
			// 色更新
			map.SetSelectedNodeFlag(true);
		}
		else
		{
			//Debug.Log("test False");
		}
	}

	public void OnConfirmMove()
	{
		//Debug.Log("OnConfirmMove");
		map.confirmMovePanel.SetActive(false);

		map.SetMoveOneCharacter();

	}

	public void OnCancelMove()
	{
		//Debug.Log("OnCancelMove");
		map.confirmMovePanel.SetActive(false);
		CancelSelection();

		Debug.Log("移動キャンセル");

	}
	public void MoveCharacterToFriendlyProvince(
		CharacterRuntimeData character,
		FactionRuntimeData oldFaction,
		ProvinceRuntimeData lostProvince)
	{
		Debug.Log("MoveCharacterToFriendlyProvince");

		foreach (var province
			in map.GetAllProvinces())
		{
			// ★ 占領された地域は除外
			if (province == lostProvince)
				continue;

			if (province.owner
				== oldFaction)
			{
				province.characterList
					.Add(character);

				//Debug.Log(
				//	"退避：" +
				//	character.baseData.characterName +
				//	" → " +
				//	province.baseData.provinceName);
				return;
			}

		}

		//Debug.Log(
		//	"退避先なし：" +
		//	character.baseData.characterName);
	}

	public void MoveOneCharacter(
		ProvinceRuntimeData from,
		ProvinceRuntimeData to,
		List<CharacterRuntimeData> attackers)
	{
		Debug.Log("MoveOneCharacter");
		// 移動元に武将がいない
		if (from.characterList.Count == 0)
		{
			Debug.Log("移動元に武将なし");
			return;
		}

		// 移動先が満員
		if (!to.CanAddCharacter())
		{
			Debug.Log("移動先は満員");
			return;
		}

		// 武将を取得
		var movingAttackers =
			new List<CharacterRuntimeData>(
				attackers);

		foreach (var attacker in movingAttackers)
		{
			//Debug.Log(
			//	"削除前：" +
			//	from.characterList.Count);

			// 移動元から削除
			from.characterList
				.Remove(attacker);
			// 移動先へ追加
			to.AddCharacter(attacker);

			//Debug.Log(
			//	"退避：" +
			//	attacker.baseData.characterName);
		}

		//// 移動元から削除
		//from.stationedCharacters.Remove(character);

		//// 移動先へ追加
		//to.AddCharacter(character);

		map.SetSelectedNode(null);
		UpdateAllNodeColors();

	}
}
