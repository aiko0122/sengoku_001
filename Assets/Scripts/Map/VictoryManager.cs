using UnityEngine;


public class VictoryManager
{
	MapManager map;

	//ProvinceRuntimeData runtimeData;

	public VictoryManager(
		MapManager mapManager)
	{
		map = mapManager;
	}

	public void CheckVictoryDefeat()
	{
		//Debug.Log("Victory check");

		//--------------------------------
		// nullチェック
		//--------------------------------

		if (Constants.victoryProvinceId == null ||
			string.IsNullOrEmpty(
				Constants.victoryProvinceId))
		{
			Debug.LogError(
				"victoryProvince 未設定");

			return;
		}

		if (Constants.defeatProvinceId == null ||
			string.IsNullOrEmpty(
				Constants.defeatProvinceId))
		{
			Debug.LogError(
				"defeatProvince 未設定");

			return;
		}

		//--------------------------------
		// 存在チェック
		//--------------------------------

		if (map.GetProvinceRuntime(Constants.victoryProvinceId) == null)
		{
			Debug.LogError(
				"存在しない victoryProvince: "
				+ Constants.victoryProvinceId);

			return;
		}

		if (map.GetProvinceRuntime(Constants.defeatProvinceId) == null)
		{
			Debug.LogError(
				"存在しない defeatProvince: "
				+ Constants.defeatProvinceId);

			return;
		}

		//--------------------------------
		// 取得
		//--------------------------------

		var victory =
			map.GetProvinceRuntime(
				Constants.victoryProvinceId);

		var defeat =
			map.GetProvinceRuntime(
				Constants.defeatProvinceId);

		//--------------------------------
		// 勝利判定
		//--------------------------------

		if (victory.owner.baseData.factionId
			== Constants.playerFactionId)
		{
			map.OnGameWin();
			return;
		}

		//--------------------------------
		// 敗北判定
		//--------------------------------

		if (defeat.owner.baseData.factionId
			!= Constants.playerFactionId)
		{
			map.OnGameLose();
			return;
		}
	}

	public void CheckCheckpointUnlocks()
	{
		Debug.Log("Unlock check");


		foreach (var province
			in map.GetAllProvinces())
		{
			if (!province.baseData.isGate)
				continue;

			if (province.isUnlocked)
				continue;

			//--------------------------------
			// required null 対策（重要）
			//--------------------------------

			if (province.baseData.requiredProvinces == null)
				continue;

			if (province.baseData.requiredProvinces.Count == 0)
				continue;

			bool allCaptured = true;

			//Debug.Log("Test CheckCheckpointUnlocks " +
			//	province.baseData.requiredProvinces.Count);

			//--------------------------------
			// 必須拠点チェック
			//--------------------------------

			foreach (var req
				in province.baseData.requiredProvinces)
			{
				//--------------------------------
				// req null 対策（重要）
				//--------------------------------
				if (req.provinceId == null)
				{
					Debug.LogError("req null");
					allCaptured = false;
					break;
				}

				//--------------------------------
				// 空IDチェック（今回の原因）
				//--------------------------------

				if (string.IsNullOrEmpty(
					req.provinceId))
				{
					Debug.LogError(
						province.baseData.provinceName +
						" provinceId が空");

					continue;
				}
				//--------------------------------
				// runtimeDataに存在するか
				//--------------------------------
				
				if (map.GetProvinceRuntime(req.provinceId) == null)
				{
					Debug.LogError(
						"存在しないprovinceId: " +
						req.provinceId);

					allCaptured = false;
					break;
				}

				var reqProvince =
					map.GetProvinceRuntime(req.provinceId);

				//--------------------------------
				// ownerチェック（最重要）
				//--------------------------------

				if (reqProvince.owner == null)
				{
					Debug.LogError(
						"owner null: " +
						req);

					allCaptured = false;
					break;
				}

				//--------------------------------
				// 所属チェック
				//--------------------------------

				if (reqProvince.owner.baseData == null)
				{
					Debug.LogError(
						"owner.baseData null: " +
						req);

					allCaptured = false;
					break;
				}

				if (reqProvince.owner.baseData.factionId
					!= Constants.playerFactionId)
				{
					allCaptured = false;
					break;
				}
			}

			//--------------------------------
			// 解放処理
			//--------------------------------
			if (allCaptured)
			{
				province.isUnlocked = true;
				//UpdateAllNodeColors();

				Debug.Log(
					province.baseData.provinceName +
					" が解放された！");
			}
		}
	}


}