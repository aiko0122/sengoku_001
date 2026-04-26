using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
	public int turnCount;

	public List<FactionSaveData>
		factions =
		new List<FactionSaveData>();

	public List<ProvinceSaveData>
		provinces =
		new List<ProvinceSaveData>();

	public List<CharacterSaveData>
		characters =
		new List<CharacterSaveData>();
}

[Serializable]
public class FactionSaveData
{
	public string factionId;

	public int gold;
}

[Serializable]
public class ProvinceSaveData
{
	public string provinceId;

	public string ownerFactionId;

	public int defenseLevel;
}

[Serializable]
public class CharacterSaveData
{
	public string characterId;

	public string provinceId;

	public int soldierCount;
}