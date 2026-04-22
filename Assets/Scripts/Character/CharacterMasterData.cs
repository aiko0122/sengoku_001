using System;
using System.Collections.Generic;

[Serializable]
public class CharacterMasterData
{
	public string characterId;
	public string characterName;
	//public string portraitId; // Å© í«â¡

	public int initialAttack;
	public int initialDefense;
	public int leadership;
	public int maxSoldier;

	public string initialFactions;
	public List<string> initialSkillIds;
	public string initialEquipmentId;

	public float levelUpRate;
}