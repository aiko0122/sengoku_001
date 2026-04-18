using UnityEngine;
using System.Collections.Generic;

// í“¬ŒvZê—pƒNƒ‰ƒX

public class BattleManager
{
	// í“¬Às
	public bool ExecuteBattle(
		List<CharacterRuntimeData>
			attackers,
		ProvinceRuntimeData target)
	{
		// UŒ‚—ÍŒvZ
		int attack =
			CalculateAttackPower(
				attackers);

		// –hŒä—ÍŒvZ
		int defense =
			CalculateDefensePower(
				target);

		Debug.Log(
			"UŒ‚F" + attack +
			" –hŒäF" + defense);

		// 3”{·ƒ`ƒFƒbƒN
		if (attack >= defense * 3)
		{
			Debug.Log(
				"©“®Ÿ—˜i3”{·j");

			return true;
		}

		// ƒ‰ƒ“ƒ_ƒ€’l
		int random =
			Random.Range(0, 6);

		int result =
			attack - defense + random;

		Debug.Log(
			"Œ‹‰ÊF" + result);

		// Ÿ”s”»’è
		return result > 0;
	}

	// UŒ‚—Í‡Œv
	int CalculateAttackPower(
		List<CharacterRuntimeData>
			attackers)
	{
		int total = 0;

		foreach (var ch in attackers)
		{
			total += ch.GetAttack();
		}

		return total;
	}

	// –hŒä—Í‡Œv
	int CalculateDefensePower(
		ProvinceRuntimeData target)
	{
		int total =
			target.baseData.defenseValue;

		foreach (var ch
			in target.stationedCharacters)
		{
			total += ch.GetDefense();
		}

		return total;
	}
}