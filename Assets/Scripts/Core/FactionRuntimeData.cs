using System.Collections.Generic;
using UnityEngine;

public class FactionRuntimeData
{
	//--------------------------------
	// 元データ
	//--------------------------------

	public FactionMasterData
		baseData;

	//--------------------------------
	// 現在資金
	//--------------------------------
	public int gold;

	//--------------------------------
	// 色（Unity Color）
	//--------------------------------

	public Color color;

	//--------------------------------
	// コンストラクタ
	//--------------------------------


	public FactionRuntimeData(
		FactionMasterData data)
	{
		baseData = data;

		gold =
			data.initialGold;

		//--------------------------------
		// 色変換
		//--------------------------------

		ColorUtility.TryParseHtmlString(
			data.colorHex,
			out color);

		//Debug.Log(color);
	}
}