using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	MapManager mapManager;

	[Header("戦闘ダメージ")]

	[SerializeField]
	int attackerDamage = 20;

	[SerializeField]
	int defenderDamage = 20;

	public void Initialize(
		MapManager manager)
	{
		mapManager = manager;
	}

	public void TryAttack(
		ProvinceRuntimeData from,
		ProvinceRuntimeData target,
		List<CharacterRuntimeData> attackers)
	{
		// 隣接チェック
		if (!from.baseData.neighbors
			.Contains(target.baseData.provinceId))
		{
			Debug.Log("隣接していない");
			return;
		}

		Debug.Log(
			from.baseData.provinceName
			+ " → "
			+ target.baseData.provinceName);

		// 同勢力なら移動
		if (from.owner
			== target.owner)
		{
			mapManager.MoveOneCharacter(from, target, attackers);
			return;
		}

		if (attackers.Count == 0)
		{
			Debug.Log("武将未選択");
			return;
		}


		// 敵なら戦闘

		// 仮：常に勝利
		//bool win = true;
		int attackPower =
			GetAttackPower(attackers);

		int defensePower =
			GetDefensePower(target);

		Debug.Log(
			"攻撃:" + attackPower +
			" 防御:" + defensePower);

		// ★ ダメージ適用（勝敗前でもOK）
		ApplyBattleDamage(
			attackers,
			target);

		RemoveDeadCharacters(from);
		RemoveDeadCharacters(target);

		// 勝敗判定
		bool win =
			CalculateBattle(
				attackPower,
				defensePower);

		if (win)
		{
			CaptureProvince(from, target, attackers);
		}
		else
		{
			Debug.Log("撤退");
		}
	}

	void CaptureProvince(
		ProvinceRuntimeData from,
		ProvinceRuntimeData target,
		List<CharacterRuntimeData> attackers)
		{
			Debug.Log(
				"占領成功：" +
				target.baseData.provinceName);

			// 旧勢力保存（重要）
			FactionRuntimeData oldFaction =
				target.owner;

			// 防衛側コピー
			var defenders =
				new List<CharacterRuntimeData>(
					target.characterList);

			// =========================
			// 防衛側退避処理
			// =========================

			foreach (var defender in defenders)
			{
				Debug.Log(
					"削除前：" +
					target.characterList.Count);

				// ★ 先に削除（重要）
				target.characterList
					.Remove(defender);
				// ★ その後退避
				mapManager.MoveCharacterToFriendlyProvince(
					defender,
					oldFaction,
					target);

				Debug.Log(
					"退避：" +
					defender.baseData.characterName);
			}

			// =========================
			// 勢力変更
			// =========================

			target.owner =
				from.owner;

			// =========================
			// 攻撃側前進
			// =========================

			// ★ attackers のコピーを作る
			var movingAttackers =
				new List<CharacterRuntimeData>(
					attackers);

			foreach (var attacker in movingAttackers)
			{
				from.characterList
					.Remove(attacker);

				target.characterList
					.Add(attacker);

				//Debug.Log(
				//	"前進：" +
				//	attacker.baseData.characterName);
			}

			mapManager.CheckCheckpointUnlocks();
			mapManager.CheckVictoryDefeat();

			mapManager.UpdateAllNodeColors();
		}


	bool CalculateBattle(
		int attack,
		int defense)
	{
		// 3倍差ルール

		if (attack >= defense * 3)
		{
			Debug.Log("圧勝（戦闘なし）");
			return true;
		}

		if (defense >= attack * 3)
		{
			Debug.Log("大敗（戦闘なし）");
			return false;
		}

		// 通常戦闘（仮）
		//float chance =
		//	(float)attack /
		//	(attack + defense);

		float winRate =
			CalculateWinRate(
				attack,
				defense);

		float roll =
			Random.value;

		Debug.Log(
			"勝率:" +
				Mathf.RoundToInt(
				winRate * 100) + "%");
		Debug.Log(
			"判定値：" +
			roll.ToString("F2"));

		if (roll <= winRate)
		{
			Debug.Log("結果：勝利");
			return true;
			//CaptureProvince(from,target);
		}
		else
		{
			Debug.Log("結果：撤退");
			return false;
		}

		//return roll < winRate;
	}

	float CalculateWinRate(
		int attackPower,
		int defensePower)
		{
			if (attackPower <= 0)
				return 0f;

			float rate =
				(float)attackPower /
				(attackPower + defensePower);

			return rate;
		}

	public int GetAttackPower(
		List<CharacterRuntimeData> characters)
		{
			int total = 0;

			foreach (var ch
				in characters)
			{
				total += ch.GetAttack();

				//total += ch.GetLeadership() * ch.soldierCount / 100;
			}

			return total;
		}

	public int GetDefensePower(
		ProvinceRuntimeData province)
	{
		int total =
			province.defense;

		// 武将防御を加算
		foreach (var ch
			in province.characterList)
		{
			total += ch.GetDefense();

			//total += ch.GetLeadership() * ch.soldierCount / 100;
		}

		return total;
	}

	public void addBattleCount(
		List<CharacterRuntimeData> characters)
	{
		foreach (var ch
			in characters)
		{
			ch.battleCount++;

			if (ch.battleCount == 10)
			{
				if (Random.value > ch.baseData.levelUpRate)
				{
					ch.attack++;
					Debug.Log("attack++");
				}
				else
				{
					ch.defense++;
					Debug.Log("defense++");
				}

				ch.battleCount = 0;
			}
		}
	}

	void ApplyBattleDamage(
		List<CharacterRuntimeData> attackers,
		ProvinceRuntimeData defenderProvince)
	{
		// 攻撃側ダメージ
		foreach (var attacker
			in attackers)
		{
			attacker.soldierCount
				-= attackerDamage;

			if (attacker.soldierCount < 1)
				attacker.soldierCount = 1;

			Debug.Log(
				attacker.baseData.characterName +
				" 攻撃側ダメージ -" +
				attackerDamage);
		}

		// 防御側ダメージ
		foreach (var defender
			in defenderProvince.characterList)
		{
			defender.soldierCount
				-= defenderDamage;

			if (defender.soldierCount < 1)
				defender.soldierCount = 1;

			Debug.Log(
				defender.baseData.characterName +
				" 防御側ダメージ -" +
				defenderDamage);
		}
	}

	void RemoveDeadCharacters(
	ProvinceRuntimeData province)
	{
		province.characterList
			.RemoveAll(
				c => c.soldierCount <= 0);
	}

}