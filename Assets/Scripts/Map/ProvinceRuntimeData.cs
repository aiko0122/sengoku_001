using System.Collections.Generic;
using UnityEngine;

public class ProvinceRuntimeData
{
	//--------------------------------
	// 元データ（Json）
	//--------------------------------

	public ProvinceMasterData baseData;

	//--------------------------------
	// 表示ノード
	//--------------------------------

	public ProvinceNode node;

	//--------------------------------
	// 所有Faction
	//--------------------------------
	//public FactionData ownerFaction;
	public FactionRuntimeData owner;

	//--------------------------------
	// 武将
	//--------------------------------

	public List<CharacterRuntimeData>
		characterList =
			new List<CharacterRuntimeData>();
	//characterList;

	public const int MAX_CHARACTERS = 3;


	//--------------------------------
	// 隣接地域
	//--------------------------------

	public List<ProvinceRuntimeData>
		neighbors =
			new List<ProvinceRuntimeData>();

	//--------------------------------
	// 防御レベル
	//--------------------------------

	//public int defenseLevel = 1;
	public int defense;
	public int defenseLevel;
	public int income;
	//--------------------------------
	// ロック状態
	//--------------------------------

	//public bool isUnlocked = true;
	public bool isUnlocked;

	//--------------------------------
	// コンストラクタ
	//--------------------------------	
	public ProvinceRuntimeData(
		ProvinceMasterData data)
	{
		baseData = data;

		defense = baseData.defenseValue;
		defenseLevel = baseData.initialDefenseLevel;
		income = baseData.initialIncome;

		// 通常地域は最初から解放
		isUnlocked =
			!data.isGate;

		// リスト初期化（忘れるとエラー）
		var characterList = baseData.initialCharacterIds;
			//new List<CharacterRuntimeData>();

		//Debug.Log("Test Load " +
		//	baseData.provinceName + "  " +
		//	defenseLevel);

		// 初期所有者（仮）
		//ownerFaction = "Neutral";
	}

	public bool CanAddCharacter()
	{
		return characterList.Count
			< MAX_CHARACTERS;
	}

	public bool AddCharacter(
		CharacterRuntimeData ch)
	{
		if (!CanAddCharacter())
		{
			Debug.Log(
				baseData.provinceName +
				" は満員");

			return false;
		}

		characterList.Add(ch);

		//Debug.Log(
		//	baseData.provinceName +
		//	" に追加：" +
		//	ch.baseData.characterName);

		return true;
	}
	//--------------------------------
	// 武将削除
	//--------------------------------

	public void RemoveCharacter(
		CharacterRuntimeData ch)
	{
		characterList.Remove(ch);
	}
}