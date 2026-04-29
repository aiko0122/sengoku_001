using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager
{
	MapManager map;

	public CharacterManager(
		MapManager mapManager)
	{
		map = mapManager;
	}

	public CharacterRuntimeData CreateCharacter(
		string characterId)
	{
		Debug.Log("CreateCharacter");
		//var master =
		//	CharacterDatabase
		//		.GetCharacter(characterId);

		//if (master == null)
		//{
		//	Debug.LogWarning(
		//		"ïêè´ê∂ê¨é∏îs: " +
		//		characterId);

		return null;
		//}

		//var runtime =
		//	new CharacterRuntimeData(
		//		master);

		//Debug.Log(
		//	"ïêè´ê∂ê¨: " +
		//	master.characterName);

		//return runtime;
	}

	void ReinforceProvince(
	ProvinceRuntimeData province,
	string characterId)
	{
		var character =
			CreateCharacter(characterId);

		if (character == null)
			return;

		province.characterList
			.Add(character);
	}

	public void AddSelectedCharacter(
		CharacterRuntimeData character)
	{
		//Debug.Log("AddSelectedCharacter");

		var characters = map.GetSelectedCharacters();
		if (characters.Count >= 3)
		{
			//Debug.Log(
			//	"ç≈ëÂ3êlÇ‹Ç≈");

			return;
		}

		if (!characters
			.Contains(character))
		{
			map.AddSelectedCharacters(character);
		}

		map.SelectedCountUpdate();

	}
	public void RemoveSelectedCharacter(
	CharacterRuntimeData character)
	{
		//Debug.Log("RemoveSelectedCharacter");

		var characters = map.GetSelectedCharacters();
		if (!characters
			.Contains(character))
		{
			Debug.Log("Test");
			map.RemoveSelectedCharacter(character);
		}

		map.SelectedCountUpdate();


	}

}
