using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProvinceDatabase
{
	public static List<
		ProvinceMasterData>
		provinces;

	//--------------------------------

	public static void Load()
	{
		string path =
			Path.Combine(
				Application.streamingAssetsPath,
				"provinces.json");

		//Debug.Log(
		//	"Loading path: "
		//	+ path);

		string json =
			File.ReadAllText(path);


		var data =
			JsonUtility
			.FromJson<
				ProvinceMasterDataList>(
					json);

		provinces =
			data.provinces;

		ValidateIds();
	}

	//--------------------------------
	// ID重複チェック
	//--------------------------------

	static void ValidateIds()
	{
		HashSet<string> ids =
			new HashSet<string>();

		foreach (var p
			in provinces)
		{
			if (!ids.Add(
				p.provinceId))
			{
				Debug.LogError(
					"ProvinceID重複: "
					+ p.provinceId);
			}
		}
	}
}