using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[System.Serializable]
public class CharacterRuntimeData
{
	//--------------------------------
	// 元データ
	//--------------------------------
	public CharacterMasterData baseData;

	//public string characterId;
	//public string characterName;

	//public int leadership;
	public int soldierCount;

	public List<string> skillIds;
	public string equipmentId;

	//public float levelUpRate;

	public int attack;
	public int defense;

	public int battleCount;

	public FactionRuntimeData faction;

	//--------------------------------
	// コンストラクタ（追加）
	//--------------------------------

	public CharacterRuntimeData(
		CharacterMasterData data)
	{
		baseData = data;

		//--------------------------------
		// 初期ステータス
		//--------------------------------

		attack = baseData.initialAttack;
		defense = baseData.initialDefense;

		soldierCount =
			data.maxSoldier;

		skillIds =
			new List<string>(
				baseData.initialSkillIds);

		equipmentId =
			baseData.initialEquipmentId;
	}

	// 攻撃値取得
	public int GetAttack()
	{
		int ret = 0;
		//return baseData.attackPower;

		ret = attack + (soldierCount * baseData.leadership / 100);
		return ret;
	}

	// 防御値取得
	public int GetDefense()
	{
		int ret = 0;
		//return baseData.defensePower;

		ret = defense + (soldierCount * baseData.leadership / 100);
		return ret;

	}

	// 統率力取得
	public int GetLeadership()
	{
		return baseData.leadership;
	}

}