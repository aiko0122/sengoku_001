using UnityEngine;

public static class CharacterFactory
{
	public static CharacterRuntimeData
		CreateCharacter(
			string characterId)
	{
		//--------------------------------
		// Masteræ“¾
		//--------------------------------

		var master =
			CharacterDatabase
				.GetCharacter(characterId);

		if (master == null)
		{
			//Debug.LogWarning(
			//	"•«¶¬¸”s: " +
			//	characterId);

			return null;
		}

		var runtime =
			new CharacterRuntimeData(
				master);

		//runtime.characterId		= master.characterId;
		//runtime.characterName	= master.characterName;

		//runtime.attack			= master.initialAttack;
		//runtime.defense			= master.initialDefense;
		//runtime.leadership		= master.leadership;
		//runtime.soldierCount	= master.maxSoldier;

		//runtime.skillIds =
		//	new List<string>(
		//		master.initialSkillIds);

		//runtime.equipmentId =
		//	master.initialEquipmentId;

		// š‰æ‘œ“Ç‚İ‚İ
		//runtime.portrait =
		//	Resources.Load<Sprite>(
		//		"Portraits/" +
		//		master.portraitId);

		//Debug.Log(
		//	"•«¶¬: " +
		//	master.characterName);

		//--------------------------------
		// Runtime¶¬
		//--------------------------------

		return new CharacterRuntimeData(
			master);
	}
}