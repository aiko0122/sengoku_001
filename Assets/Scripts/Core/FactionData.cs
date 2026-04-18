using UnityEngine;

// 勢力データ
// プレイヤー、敵1、中立など

[CreateAssetMenu(
	fileName = "FactionData",
	menuName = "Game/Faction")]
public class FactionData : ScriptableObject
{
	[Header("基本情報")]

	// 勢力名
	public string factionName;

	[Header("表示色")]

	// マップ表示用の色
	public Color factionColor;
}