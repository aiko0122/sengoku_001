using System.Collections.Generic;
using UnityEngine;

// 実行中の地域状態
// ScriptableObjectとは別管理する

public class ProvinceRuntimeData
{
	// 設計データ
	public ProvinceData baseData;

	// 所有勢力（後でFaction追加予定）
	public FactionData ownerFaction;
	//public FactionData faction;

	// 配置キャラ
	public List<CharacterRuntimeData>
		stationedCharacters;

	public const int MAX_CHARACTERS = 3;

	// コンストラクタ
	public ProvinceRuntimeData(
		ProvinceData data)
	{
		baseData = data;

		// リスト初期化（忘れるとエラー）
		stationedCharacters =
			new List<CharacterRuntimeData>();

		// 初期所有者（仮）
		//ownerFaction = "Neutral";
	}

	public bool CanAddCharacter()
	{
		return stationedCharacters.Count
			< MAX_CHARACTERS;
	}

	public bool AddCharacter(
	CharacterRuntimeData character)
	{
		if (!CanAddCharacter())
		{
			Debug.Log(
				baseData.provinceName +
				" は満員");

			return false;
		}

		stationedCharacters.Add(character);

		Debug.Log(
			baseData.provinceName +
			" に追加：" +
			character.baseData.characterName);

		return true;
	}
}