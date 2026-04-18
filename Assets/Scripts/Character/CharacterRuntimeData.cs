// キャラの実行中データ

public class CharacterRuntimeData
{
	public CharacterData baseData;

	// コンストラクタ
	public CharacterRuntimeData(
		CharacterData data)
	{
		baseData = data;
	}

	// 攻撃値取得
	public int GetAttack()
	{
		return baseData.attackPower;
	}

	// 防御値取得
	public int GetDefense()
	{
		return baseData.defensePower;
	}
}