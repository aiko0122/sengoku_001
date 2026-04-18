using UnityEngine;

// 武将データ（ScriptableObject）

[CreateAssetMenu(
	fileName = "CharacterData",
	menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
	[Header("基本情報")]

	// 武将名
	public string characterName;

	[Header("能力")]

	// 攻撃力
	public int attackPower = 5;

	// 防御力
	public int defensePower = 3;
}