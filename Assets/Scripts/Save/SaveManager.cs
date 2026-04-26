using System.IO;
using UnityEngine;

public class SaveManager :
	MonoBehaviour
{
	public static SaveManager Instance;

	string savePath;

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}

		savePath =
			Application.persistentDataPath +
			"/save.json";
	}

	//--------------------------------
	// SAVE
	//--------------------------------

	public void SaveGame(
		MapManager mapManager)
	{
		SaveData data =
			new SaveData();

		//--------------------------------
		// Factionï€ë∂
		//--------------------------------
		//foreach (var faction
		//	in mapManager
		//		.factionRuntimeDataDict
		//		.Values)
			foreach (var faction
			in mapManager
				.GetAllFactions())
		{
			FactionSaveData f =
				new FactionSaveData();

			f.factionId =
				faction.baseData.factionId;

			f.gold =
				faction.gold;

			data.factions.Add(f);
		}

		//--------------------------------
		// Provinceï€ë∂
		//--------------------------------
		//foreach (var province
		//	in mapManager
		//		.runtimeData
		//		.Values)
			foreach (var province
			in mapManager
				.GetAllProvinces())
		{
			ProvinceSaveData p =
				new ProvinceSaveData();

			p.provinceId =
				province.baseData.provinceId;

			p.ownerFactionId =
				province.owner
					.baseData
					.factionId;

			p.defenseLevel =
				province.defenseLevel;

			data.provinces.Add(p);
		}

		//--------------------------------
		// Characterï€ë∂
		//--------------------------------

		foreach (var province
			in mapManager
				.GetAllProvinces())
		{
			foreach (var ch
				in province
					.characterList)
			{
				CharacterSaveData c =
					new CharacterSaveData();

				c.characterId =
					ch.baseData.characterId;

				c.provinceId =
					province
						.baseData
						.provinceId;

				c.soldierCount =
					ch.soldierCount;

				data.characters.Add(c);
			}
		}

		//--------------------------------
		// JSONï€ë∂
		//--------------------------------

		string json =
			JsonUtility.ToJson(
				data,
				true);

		File.WriteAllText(
			savePath,
			json);

		Debug.Log(
			"Save Complete: " +
			savePath);
	}

	//--------------------------------
	// LOAD
	//--------------------------------

	public void LoadGame(
		MapManager mapManager)
	{
		if (!File.Exists(savePath))
		{
			Debug.Log("No Save File");

			return;
		}

		string json =
			File.ReadAllText(
				savePath);

		SaveData data =
			JsonUtility.FromJson<SaveData>(
				json);

		//--------------------------------
		// èâä˙âª
		//--------------------------------

		mapManager
			.InitializeRuntimeData();

		//--------------------------------
		// Factionïúå≥
		//--------------------------------

		foreach (var f
			in data.factions)
		{
			var faction =
				mapManager
					.GetFactionById(
						f.factionId);

			faction.gold =
				f.gold;
		}

		//--------------------------------
		// Provinceïúå≥
		//--------------------------------

		foreach (var p
			in data.provinces)
		{
			var province =
				mapManager
					.GetProvinceById(
						p.provinceId);

			var faction =
				mapManager
					.GetFactionById(
						p.ownerFactionId);

			province.owner =
				faction;

			province.defenseLevel =
				p.defenseLevel;
		}

		//--------------------------------
		// Characterïúå≥
		//--------------------------------

		foreach (var c
			in data.characters)
		{
			var province =
				mapManager
					.GetProvinceById(
						c.provinceId);

			var ch =
				mapManager
					.CreateCharacter(
						c.characterId);

			ch.soldierCount =
				c.soldierCount;

			province.characterList
				.Add(ch);
		}

		Debug.Log("Load Complete");
	}
}