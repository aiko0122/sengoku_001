using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterRuntimeData
{
	public string characterId;
	public string characterName;
	//public Sprite portrait; // ©’Ç‰Á

	public int leadership;
	public int soldierCount;

	public List<string> skillIds;
	public string equipmentId;

	public float levelUpRate;

	public int attack;
	public int defense;

	public int battleCount;

	public FactionRuntimeData faction;

	// UŒ‚’læ“¾
	public int GetAttack()
	{
		int ret = 0;
		//return baseData.attackPower;

		ret = attack + (soldierCount * leadership / 100);
		return ret;
	}

	// –hŒä’læ“¾
	public int GetDefense()
	{
		int ret = 0;
		//return baseData.defensePower;

		ret = defense + (soldierCount * leadership / 100);
		return ret;

	}

	// “—¦—Íæ“¾
	public int GetLeadership()
	{
		return leadership;
	}

}