using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.TextCore.Text;

//[System.Serializable]
//public class CharacterMasterData
//{
//	//--------------------------------
//	// 基本情報
//	//--------------------------------

//	public string characterId;
//	public string characterName;

//	//--------------------------------
//	// ステータス
//	//--------------------------------

//	public int leadership;
//	public int attack;
//	public int defense;

//	//--------------------------------
//	// 初期兵数
//	//--------------------------------

//	public int initialSoldiers;
//}

[System.Serializable]
public class CharacterMasterDataList
{
	public List<CharacterMasterData>
		characters;
}

public static class CharacterDatabase
{
	//--------------------------------
	// キャラ一覧
	//--------------------------------

	public static List<CharacterMasterData>
		characters =
			new List<CharacterMasterData>();

    //--------------------------------
    // Jsonロード
    //--------------------------------
	public static void Load()
	{
		string path =
			Path.Combine(
				Application.streamingAssetsPath,
				"characters.json");

		if (!File.Exists(path))
		{
			Debug.LogError(
				"Character json not found: "
				+ path);

			return;
		}

		string json =
			File.ReadAllText(path);

		var data =
			JsonUtility.FromJson
			<CharacterMasterDataList>(json);

		characters = data.characters;

		Debug.Log(
			"Characters Loaded: "
			+ characters.Count);
	}

	//--------------------------------
	// ID検索（重要）
	//--------------------------------

	public static CharacterMasterData
		GetCharacter(string characterId)
	{
		foreach (var ch in characters)
		{
			if (ch.characterId
				== characterId)
			{
				return ch;
			}
		}

		Debug.LogError(
			"Character not found: "
			+ characterId);

		return null;
	}
	//public TextAsset characterJson;

	//public List<CharacterMasterData>
	//	characters;

	//Dictionary<string,
	//	CharacterMasterData> lookup;

	//public void Initialize()
	//{
	//	LoadFromJson();

	//	lookup =
	//		new Dictionary<string,
	//			CharacterMasterData>();

	//	foreach (var c in characters)
	//	{
	//		lookup[c.characterId] = c;
	//	}
	//}

	//void LoadFromJson()
	//{
	//	var data =
	//		JsonUtility.FromJson<
	//			CharacterMasterList>(
	//			characterJson.text);

	//	characters = data.characters;

	//	Debug.Log(
	//		"Character読込: " +
	//		characters.Count);
	//}

	//public CharacterMasterData GetCharacter(
	//	string id)
	//{
	//	if (lookup == null)
	//		Initialize();

	//	if (lookup.ContainsKey(id))
	//		return lookup[id];

	//	Debug.LogWarning(
	//		"Character not found: " + id);

	//	return null;
	//}
}