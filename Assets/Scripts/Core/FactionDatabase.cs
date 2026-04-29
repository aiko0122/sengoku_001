using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionMasterDataList
{
	public List<FactionMasterData>
		factions;
}

public static class FactionDatabase
{
	//--------------------------------
	// ì«Ç›çûÇ‹ÇÍÇΩFactionàÍóó
	//--------------------------------

	public static List<FactionMasterData>
		factions =
			new List<FactionMasterData>();

	//--------------------------------
	// JsonÉçÅ[Éh
	//--------------------------------

	public static void Load()
	{
		string path =
			Path.Combine(
				Application.streamingAssetsPath,
				"factions.json");

		if (!File.Exists(path))
		{
			Debug.LogError(
				"Faction json not found: "
				+ path);

			return;
		}

		string json =
			File.ReadAllText(path);

		var data =
			JsonUtility.FromJson
			<FactionMasterDataList>(json);

		factions = data.factions;

		//Debug.Log(
		//	"Faction loaded: "
		//	+ factions.Count);
	}

	//--------------------------------
	// IDåüçı
	//--------------------------------

	public static FactionMasterData
		GetFaction(string factionId)
	{
		foreach (var f in factions)
		{
			if (f.factionId
				== factionId)
			{
				return f;
			}
		}

		return null;
	}
}