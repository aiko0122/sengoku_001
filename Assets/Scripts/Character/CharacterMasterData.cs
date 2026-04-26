using System;
using System.Collections.Generic;

[Serializable]
public class CharacterMasterData
{
	//--------------------------------
	// 基本
	//--------------------------------
	public string characterId;
	public string characterName;

	//--------------------------------
	// ステータス
	//--------------------------------
	public int initialAttack;
	public int initialDefense;
	public int leadership;
	public int maxSoldier;

	//--------------------------------
	// 初期設定
	//--------------------------------

	public string initialFactions;
	public List<string> initialSkillIds;
	public string initialEquipmentId;

	public float levelUpRate;
}