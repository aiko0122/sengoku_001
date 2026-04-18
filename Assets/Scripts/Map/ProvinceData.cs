using System.Collections.Generic;
using UnityEngine;

// ScriptableObjectとして地域データを作成する
// Unity右クリック → Create → Game → Province で作れる

[CreateAssetMenu(
	fileName = "ProvinceData",
	menuName = "Game/Province")]
public class ProvinceData : ScriptableObject
{
	[Header("基本情報")]

	// 地域名（A-1など）
	public string provinceName;

	// ノード表示用アイコン
	public Sprite icon;

	[Header("接続情報")]

	// 隣接地域
	// ここに接続先のProvinceDataを登録する
	public List<ProvinceData> neighbors =
		new List<ProvinceData>();

	[Header("関所設定")]

	// 関所かどうか
	public bool isGate;

	// 攻略に必要な地域
	// （例：A-6ならA-1〜A-5）
	public List<ProvinceData> requiredProvinces =
		new List<ProvinceData>();

	[Header("戦闘設定")]

	// 地域の防御力
	public int defenseValue = 5;

	public List<CharacterData>
	initialCharacters =
		new List<CharacterData>();
}