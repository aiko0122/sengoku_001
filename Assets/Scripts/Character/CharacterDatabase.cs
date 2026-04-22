using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "CharacterDatabase",
	menuName = "Game/Character Database")]

[System.Serializable]
public class CharacterMasterList
{
	public List<CharacterMasterData>
		characters;
}

public class CharacterDatabase
	: ScriptableObject
{
	public TextAsset characterJson;

	public List<CharacterMasterData>
		characters;

	Dictionary<string,
		CharacterMasterData> lookup;

	public void Initialize()
	{
		LoadFromJson();

		lookup =
			new Dictionary<string,
				CharacterMasterData>();

		foreach (var c in characters)
		{
			lookup[c.characterId] = c;
		}
	}

	void LoadFromJson()
	{
		var data =
			JsonUtility.FromJson<
				CharacterMasterList>(
				characterJson.text);

		characters = data.characters;

		Debug.Log(
			"Characterì«çû: " +
			characters.Count);
	}

	public CharacterMasterData GetCharacter(
		string id)
	{
		if (lookup == null)
			Initialize();

		if (lookup.ContainsKey(id))
			return lookup[id];

		Debug.LogWarning(
			"Character not found: " + id);

		return null;
	}
}