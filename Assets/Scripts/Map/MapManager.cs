using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

// マップ全体の制御

public class MapManager : MonoBehaviour
{
	[Header("初期所有者設定")]

	//[SerializeField] FactionData playerFaction;
	string playerFactionId = "001";
	public FactionData enemyFaction;
	public FactionData neutralFaction;

	[SerializeField]
	TextMeshProUGUI goldText;

	//[Header("初期武将")]

	//[SerializeField]
	//private CharacterDatabase characterDatabase;

	[Header("勝敗条件")]

	[SerializeField]
		string victoryProvinceId;

	[SerializeField]
		string defeatProvinceId;

	[Header("補充設定")]

	[SerializeField]
	int reinforceAmount = 50;

	[SerializeField]
	int reinforceCost = 100;

	[Header("全地域データ")]

	// InspectorでA-1〜A-7登録
	//public List<ProvinceData>
	//	allProvinceData =
	//	new List<ProvinceData>();
	List<ProvinceData>
	allProvinceData =
		new List<ProvinceData>();

	// 実行中データ管理
	Dictionary<
		string,
		ProvinceRuntimeData>
		runtimeData =
			new Dictionary<
				string,
				ProvinceRuntimeData>();

	Dictionary<
		string,
		ProvinceNode>
		provinceNodeDict =
			new Dictionary<
				string,
				ProvinceNode>();

	[SerializeField]
	BattleManager battleManager;

	[SerializeField]
		List<FactionData>
		allFactions;

	Dictionary<
		FactionData,
		FactionRuntimeData>
		factionRuntimeData;

	FactionRuntimeData
		playerFactionRuntime;

	Dictionary<
		string,
		FactionRuntimeData>
		factionRuntimeDataDict =
			new Dictionary<
				string,
				FactionRuntimeData>();

	[SerializeField]
	GameObject characterPanel;

	[SerializeField]
	TextMeshProUGUI selectedCountText;

	[SerializeField]
	Transform characterListParent;

	[SerializeField]
	GameObject characterButtonPrefab;

	[SerializeField]
	GameObject confirmAttackPanel;

	[SerializeField]
	TextMeshProUGUI confirmMessageText;

	[SerializeField]
	Button confirmAttackButton;

	[SerializeField]
	Button cancelAttackButton;

	[SerializeField]
	TextMeshProUGUI attackerListText;

	[SerializeField]
	TextMeshProUGUI defenderListText;

	[SerializeField]
	TextMeshProUGUI powerText;

	[SerializeField]
	GameObject confirmMovePanel;

	[SerializeField]
	TextMeshProUGUI moveMessageText;

	[SerializeField]
	TextMeshProUGUI moveCharacterListText;

	[SerializeField]
	GameObject winPanel;

	[SerializeField]
	GameObject losePanel;

	// 現在選択中地域
	ProvinceRuntimeData selectedProvince;
	ProvinceNode selectedNode;

	//CharacterRuntimeData selectedCharacter;
	List<CharacterRuntimeData>
	selectedCharacters =
		new List<CharacterRuntimeData>();

	//CharacterButton selectedCharacterButton;
	List<CharacterButton>
	characterButtons =
		new List<CharacterButton>();

	int currentTurn = 1;
	bool isPlayerTurn = true;
	bool isGameOver = false;
	// ProvinceRuntimeData fromProvince;
	ProvinceRuntimeData toProvince;

	// ProvinceNode fromNode;
	ProvinceNode toNode;

	ProvinceRuntimeData pendingFrom;
	ProvinceRuntimeData pendingTarget;
	List<CharacterRuntimeData> pendingAttackers;


	void Start()
	{
		CharacterDatabase.Load();
		InitializeRuntimeData();

		Debug.Log("Gold " + playerFactionRuntime.gold);

		UpdateGoldUI();
		battleManager.Initialize(this);

		StartPlayerTurn();
	}
	void Update()
	{
		// 右クリック
		if (Input.GetMouseButtonDown(1))
		{
			CancelSelection();
		}
	}

	void StartPlayerTurn()
	{
		isPlayerTurn = true;
		if (currentTurn > 1)
		{
			AddTurnIncome();
		}

		Debug.Log(
			"ターン " +
			currentTurn);

		Debug.Log(
			"プレイヤーターン開始");

	}

	void StartEnemyTurn()
	{
		Debug.Log("敵ターン開始");

		EnemyAction();

		EndEnemyTurn();
	}

	void EndEnemyTurn()
	{
		Debug.Log("敵ターン終了");

		currentTurn++;

		StartPlayerTurn();
	}

	void AddTurnIncome()
	{
		int totalIncome = 0;

		foreach (var province
			in runtimeData.Values)
		{
			if (province.owner.baseData.factionId
				== playerFactionId)
			{
				totalIncome +=
					province.baseData.income;
			}
		}

		playerFactionRuntime.gold
			+= totalIncome;

		UpdateGoldUI();

		Debug.Log(
			"収入：" + totalIncome);
	}

	public void CancelSelection()
	{
		if (selectedNode == null)
			return;

		selectedNode.SetSelected(false);

		// CharacterPanel を閉じる
		if (characterPanel != null)
		{
			characterPanel.SetActive(false);
		}

		ClearSelectedNode();
		selectedProvince = null;

		UpdateAllNodeColors();

		Debug.Log("選択キャンセル");
	}

	// 実行用データ作成
	public void InitializeRuntimeData()
	{
		//--------------------------------
		// Jsonロード
		//--------------------------------
		InitializeFactionRuntimeData();
		ProvinceDatabase.Load();

		runtimeData.Clear();

		//--------------------------------
		// Node登録
		//--------------------------------

		RegisterProvinceNodes();

		//--------------------------------
		// Runtime生成
		//--------------------------------

		foreach (var master
			in ProvinceDatabase
				.provinces)
		{
			var runtime =
				new ProvinceRuntimeData(
					master);

			runtimeData.Add(
				master.provinceId,
				runtime);

			//--------------------------------
			// Node紐付け
			//--------------------------------

			if (provinceNodeDict
				.TryGetValue(
					master.provinceId,
					out var node))
			{
				runtime.node =
					node;

				//--------------------------------
				// 名前表示
				//--------------------------------

				node.SetNameText(
					master.provinceName);

				node.UpdateDefenseOverlay(
					master.initialDefenseLevel);
			}
			else
			{
				Debug.LogError(
					"Node not found: "
					+ master.provinceId);
			}
		}

		//--------------------------------
		// 初期Owner設定
		//--------------------------------

		SetInitialOwners();
		SetInitialCharacters();
		//--------------------------------
		// 色更新
		//--------------------------------

		UpdateAllNodeColors();
	}
	void InitializeFactionRuntimeData()
	{
		FactionDatabase.Load();

		factionRuntimeDataDict.Clear();

		foreach (var master
			in FactionDatabase.factions)
		{
			var runtime =
				new FactionRuntimeData(
					master);

			factionRuntimeDataDict
				.Add(
					master.factionId,
					runtime);

			if (master.factionId == playerFactionId)
			{
				playerFactionRuntime = runtime;
			}
		}
	}

	void SetInitialOwners()
	{
		foreach (var master
			in ProvinceDatabase
				.provinces)
		{
			var province =
				GetProvinceById(
					master.provinceId);

			string factionId =
				province
					.baseData
					.initialOwner;

			var faction =
			//GetFaction(factionId);
				GetFactionById(
					province.baseData
						.initialOwner);

			province.owner =
				faction;
			if (faction == null)
			{
				Debug.LogError(
					"Faction設定失敗: " +
					province.baseData.provinceName);
			}
		}
	}

	void SetInitialCharacters()
	{
		foreach (var province
			in runtimeData.Values)
		{
			var master =
				province.baseData;

			//--------------------------------
			// 初期武将が無い場合
			//--------------------------------

			if (master
				.initialCharacterIds == null)
			{
				Debug.LogError(
					"Character create Non "
					);

				continue;
			}

			//--------------------------------
			// 武将生成
			//--------------------------------

			foreach (var charId
				in master.initialCharacterIds)
			{
				Debug.Log("Set Init Char" + charId);
				var character =
				CharacterFactory
					.CreateCharacter(
							charId);

				if (character == null)
				{
					Debug.LogError(
						"Character create failed: "
						+ charId);

					continue;
				}

				//--------------------------------
				// 所属Faction設定
				//--------------------------------

				character.faction =
					province.owner;

				//--------------------------------
				// Provinceに追加
				//--------------------------------

				province.characterList
					.Add(character);

				Debug.Log(
					"Added: "
					+ character.baseData.characterName
					+ " → "
					+ province.baseData.provinceName);
			}
		}
	}

	public CharacterRuntimeData CreateCharacter(
		string characterId)
	{
		var master =
			CharacterDatabase
				.GetCharacter(characterId);

		if (master == null)
		{
			Debug.LogWarning(
				"武将生成失敗: " +
				characterId);

			return null;
		}

		var runtime =
			new CharacterRuntimeData(
				master);

		//runtime.characterId		= master.characterId;
		//runtime.characterName	= master.characterName;

		//runtime.attack			= master.initialAttack;
		//runtime.defense			= master.initialDefense;
		//runtime.leadership		= master.leadership;
		//runtime.soldierCount	= master.maxSoldier;

		//runtime.skillIds =
		//	new List<string>(
		//		master.initialSkillIds);

		//runtime.equipmentId =
		//	master.initialEquipmentId;

		// ★画像読み込み
		//runtime.portrait =
		//	Resources.Load<Sprite>(
		//		"Portraits/" +
		//		master.portraitId);

		Debug.Log(
			"武将生成: " +
			master.characterName);

		return runtime;
	}

	//public static class CharacterFactory
	//{
	//	public static CharacterRuntimeData
	//		CreateCharacter(
	//			string characterId)
	//	{
	//		var master =
	//			CharacterDatabase
	//				.GetCharacter(
	//					characterId);

	//		if (master == null)
	//		{
	//			Debug.LogError(
	//				"Character not found: "
	//				+ characterId);

	//			return null;
	//		}

	//		return new CharacterRuntimeData(
	//			master);
	//	}
	//}
	void ReinforceProvince(
		ProvinceRuntimeData province,
		string characterId)
	{
		var character =
			CreateCharacter(characterId);

		if (character == null)
			return;

		province.characterList
			.Add(character);
	}

	void AddCharacterToProvince(
	ProvinceRuntimeData province,
	string characterId)
	{
		var runtime =
			CreateCharacter(characterId);

		if (runtime == null)
			return;

		province.characterList
			.Add(runtime);

		Debug.Log(
			province.baseData.provinceName +
			" に武将追加: " +
			runtime.baseData.characterName);
	}

	void UpdateGoldUI()
	{
		goldText.text =
			"資金：" +
			playerFactionRuntime.gold;
	}

	public void ReinforceCharacter(
	CharacterRuntimeData character)
	{
		Debug.Log("ReinforceCharacter 呼ばれた");

		//// 資金消費
		if (SpendGold(reinforceCost) == false)
			return;

		// 兵数増加
		if (character.soldierCount < reinforceAmount)
		{
			character.soldierCount = reinforceAmount;
		} else {
			character.soldierCount += reinforceAmount;
		}

		Debug.Log(
			character.baseData.characterName +
			" を補充 +" +
			reinforceAmount);

		// UI更新
		UpdateGoldUI();
		RefreshCharacterList();
	}
	void RefreshCharacterList()
	{
		if (selectedProvince
			!= null)
		{
			ShowCharacterList(
				selectedProvince);
		}
	}

	bool SpendGold(int amount)
	{
		// 資金不足チェック
		if (playerFactionRuntime.gold
			< amount)
		{
			Debug.Log("資金不足");
			return false;
		}

		//// 資金消費
		playerFactionRuntime.gold
			-= amount;

		return true;
		//UpdateGoldUI();
	}

	void SetOwner(
		string provinceName,
		FactionRuntimeData faction)
	{
		foreach (var pair in runtimeData)
		{
			if (pair.Value.baseData.provinceName
				== provinceName)
			{
				pair.Value.owner =
					faction;
			}
		}
	}

	void EnemyAction()
	{
		var enemyProvinces =
			GetEnemyProvinces();

		foreach (var province
			in enemyProvinces)
		{
			if (province.characterList.Count == 0)
				continue;

			var attacker =
				province.characterList[0];

			int toNeighbor = Random.Range(0, 
				(province.baseData.neighbors.Count + 1));

			int loopCount = 0;

			// 隣接地域を確認
			foreach (var neighbor
				in province.baseData.neighbors)
			{
				var target =
					runtimeData[neighbor];


				if (loopCount == toNeighbor)
				{
					var attackers =
						new List<CharacterRuntimeData>(
							province.characterList);

					if (attackers.Count != 0)
					{
						battleManager.TryAttack(
							province, target, attackers);
						//province, target, province.stationedCharacters);

						Debug.Log(
							"敵攻撃人数：" +
							attackers.Count);

						return;
					}
				}

				loopCount++;

				// プレイヤー地域なら攻撃
				//if (target.owner
				//	== playerFaction)
				//{
				//	Debug.Log(
				//		"敵攻撃：" + attacker.baseData.name +
				//		province.baseData.provinceName +
				//		" → " +
				//		target.baseData.provinceName);

				//	var attackers =
				//		new List<CharacterRuntimeData>(
				//			province.stationedCharacters);

				//	TryAttack(
				//		province,target,province.stationedCharacters);

				//	Debug.Log(
				//		"敵攻撃人数：" +
				//		attackers.Count);
				//	// ★1回攻撃したら終了
				//	return;
				//}
			}
		}

		//Debug.Log("敵は攻撃できない");
	}

	// 地域クリック処理
	public void OnProvinceClicked(
		ProvinceNode node)
	{
		if (isGameOver) return;

		if (!isPlayerTurn)
		{
			Debug.Log("敵ターン中");
			return;
		}

		var runtime =
			runtimeData[
				node.provinceId]; 

		if (runtime == null)
		{
			Debug.LogError(
				"Province not found: "
				+ node.provinceId);

			return;
		}

		// =========================
		// ■1回目クリック（出発地選択）
		// =========================
		if (selectedNode == null)
		{
			selectedProvince = runtime;
			SetSelectedNode(node);

			UpdateAllNodeColors();

			if (selectedProvince.owner.baseData.factionId
				== playerFactionId)
			{
				Debug.Log("勢力：Player");
				HighlightNeighbors(runtime);
			}
			else
			{
				Debug.Log("勢力：Player以外");
			}

			Debug.Log(
				"出発地選択：" +
				runtime.baseData.provinceName);

			Debug.Log(
				"地域情報メニュー表示：" +
				runtime.baseData.provinceName);

			return;
		}

		if (selectedProvince.owner.baseData.factionId
			!= playerFactionId)
		{
			ClearSelectedNode();
			UpdateAllNodeColors();
			ClearAllHighlights();
			return;
		}

		// =========================
		// ■同じ地域クリック → 開発メニュー
		// =========================
		if (selectedNode == node)
		{
			selectedProvince = null;
			ClearSelectedNode();
			ClearAllHighlights();

			Debug.Log("選択解除"); 
			
			Debug.Log(
				"開発メニュー表示：" +
				runtime.baseData.provinceName);

			ShowDevelopmentMenu(runtime);

			return;
		}

		// =========================
		// ■2回目クリック（目的地選択）
		// =========================
		if (toNode == null)
		{

			var targetProvince = runtime;

			if (!targetProvince.isUnlocked)
			{
				Debug.Log(
					targetProvince.baseData.provinceName +
					" は通行不可");
				return;
			}

			toNode = node;
			toProvince = targetProvince;

			Debug.Log(
				"目的地選択：" +
				runtime.baseData.provinceName);

			// ★ここで武将選択を表示
			ShowCharacterList(selectedProvince);

			// ■必ずここで解除
			// ClearSelectedNode();
			// selectedProvince = null;

			UpdateAllNodeColors();
			ClearAllHighlights();

			return;
		}

		// =========================
		// ■3回目クリック（再選択 or リセット）
		// =========================
		ResetSelection();

	}

	void ResetSelection()
	{
		
		ClearSelectedNode();
		// fromNode = null;
		toNode = null;

		// fromProvince = null;
		toProvince = null;

		selectedCharacters.Clear();

		if (characterPanel != null)
		{
			characterPanel.SetActive(false);
		}

		UpdateAllNodeColors();

		Debug.Log("選択リセット");
	}

	void TryAction(
	ProvinceRuntimeData from,
	ProvinceRuntimeData target,
	List<CharacterRuntimeData> attackers)
	{

		// 同勢力 → 移動
		if (from.owner
			== target.owner)
		{
			ShowMoveConfirm(from, target, attackers);

			Debug.Log(
				"移動：" +
				from.baseData.provinceName +
				" → " +
				target.baseData.provinceName);

			return;
		}

		// 敵勢力 → 攻撃
		ShowAttackConfirm(
			from,
			target,
			attackers);
		Debug.Log(
			"攻撃：" +
			from.baseData.provinceName +
			" → " +
			target.baseData.provinceName);
	}

	void ShowAttackConfirm(
		ProvinceRuntimeData from,
		ProvinceRuntimeData target,
		List<CharacterRuntimeData> attackers)
	{
		pendingFrom = from;
		pendingTarget = target;

		pendingAttackers =
			new List<CharacterRuntimeData>(
				attackers);

		// タイトル
		confirmMessageText.text =
			target.baseData.provinceName +
			" を攻撃しますか？";

		// 攻撃側表示
		string attackerText = "攻撃側：\n";

		foreach (var ch in attackers)
		{
			attackerText +=
				ch.baseData.characterName + "\n";
		}

		attackerListText.text =
			attackerText;

		// 防御側表示
		string defenderText = "防御側：\n";

		foreach (var ch
			in target.characterList)
		{
			defenderText +=
				ch.baseData.characterName + "\n";
		}

		defenderListText.text =
			defenderText;

		battleManager.addBattleCount(attackers);
		battleManager.addBattleCount(target.characterList);

		// 戦力表示
		int attackPower =
			battleManager.GetAttackPower(attackers);

		int defensePower =
			battleManager.GetDefensePower(target);

		powerText.text =
			"攻撃力：" + attackPower +
			"\n防御力：" + defensePower;

		confirmAttackPanel.SetActive(true);
	}

	void ShowMoveConfirm(
	ProvinceRuntimeData from,
	ProvinceRuntimeData target,
	List<CharacterRuntimeData> characters)
	{
		pendingFrom = from;
		pendingTarget = target;

		pendingAttackers =
			new List<CharacterRuntimeData>(
				characters);

		moveMessageText.text =
			target.baseData.provinceName +
			" に移動しますか？";

		string listText = "";

		foreach (var ch in characters)
		{
			listText +=
				ch.baseData.characterName +
				"\n";
		}

		moveCharacterListText.text =
			listText;

		confirmMovePanel.SetActive(true);
	}

	// 攻撃処理

	public void UpdateAllNodeColors()
	{
		var nodes =
			FindObjectsOfType<ProvinceNode>();

		foreach (var node in nodes)
		{

			var runtime =
				runtimeData[node.provinceId];

			// 色更新
			node.UpdateColor(runtime);
				//runtime.owner);

			node.ClearHighlight();

			// 人数更新
			int count =
				runtime.characterList.Count;

			node.UpdateCount(count);

		}


		// 選択中ノードの上書き
		if (selectedNode != null)
		{
			//Debug.Log("test True");
			// 色更新
			selectedNode.SetSelected(true);
		}
		else
		{
			//Debug.Log("test False");
		}
	}

	public void MoveOneCharacter(
	ProvinceRuntimeData from,
	ProvinceRuntimeData to,
	List<CharacterRuntimeData> attackers)
	{
		// 移動元に武将がいない
		if (from.characterList.Count == 0)
		{
			Debug.Log("移動元に武将なし");
			return;
		}

		// 移動先が満員
		if (!to.CanAddCharacter())
		{
			Debug.Log("移動先は満員");
			return;
		}

		// 武将を取得
		var movingAttackers =
			new List<CharacterRuntimeData>(
				attackers);

		foreach (var attacker in movingAttackers)
		{
			Debug.Log(
				"削除前：" +
				from.characterList.Count);

			// 移動元から削除
			from.characterList
				.Remove(attacker);
			// 移動先へ追加
			to.AddCharacter(attacker);

			Debug.Log(
				"退避：" +
				attacker.baseData.characterName);
		}

		//// 移動元から削除
		//from.stationedCharacters.Remove(character);

		//// 移動先へ追加
		//to.AddCharacter(character);

		selectedNode = null;
		UpdateAllNodeColors();

	}

	void HighlightNeighbors(
	ProvinceRuntimeData from)
	{
		foreach (var neighborData
			in from.baseData.neighbors)
		{
			// 対応するNode取得
			var neighborNode =
				GetNodeByProvinceData(
					neighborData);

			var neighborRuntime =
				runtimeData[neighborData];

			if (neighborRuntime.isUnlocked)
			{
				// 同勢力 → 緑（移動）
				if (neighborRuntime.owner
					== from.owner)
				{
					neighborNode.SetMoveHighlight();
				}
				else
				{
					// 敵 → 赤（攻撃）
					neighborNode.SetAttackHighlight();
				}
			}
		}
	}

	ProvinceNode GetNodeByProvinceData(
		string data)
		//ProvinceData data)
	{
		var nodes =
			FindObjectsOfType<ProvinceNode>();

		foreach (var node in nodes)
		{
			if (node.provinceId == data)
			{
				return node;
			}
		}

		return null;
	}

	public void MoveCharacterToFriendlyProvince(
		CharacterRuntimeData character,
		FactionRuntimeData oldFaction,
		ProvinceRuntimeData lostProvince)
	{
		foreach (var province
			in runtimeData.Values)
		{
			// ★ 占領された地域は除外
			if (province == lostProvince)
				continue;

			if (province.owner
				== oldFaction)
			{
				province.characterList
					.Add(character);

				Debug.Log(
					"退避：" +
					character.baseData.characterName + 
					" → " +
					province.baseData.provinceName);
				return;
			}
		}

		Debug.Log(
			"退避先なし：" +
			character.baseData.characterName);
	}

	void ShowCharacterList(
	ProvinceRuntimeData province)
	{
		if (characterListParent == null)
		{
			Debug.LogError("characterListParent が未設定！");
			return;
		}

		if (characterButtonPrefab == null)
		{
			Debug.LogError("characterButtonPrefab が未設定！");
			return;
		}

		if (characterPanel == null)
		{
			Debug.LogError("characterPanel が未設定！");
			return;
		} else
		{
			characterPanel.SetActive(false);
		}

		// 古いボタン削除
		foreach (Transform child
			in characterListParent)
		{
			Destroy(child.gameObject);
		}

		characterButtons.Clear();
		selectedCharacters.Clear();
		SelectedCountUpdate();

		// 武将作成
		foreach (var character
			in province.characterList)
		{
			var obj =
				Instantiate(
					characterButtonPrefab,
					characterListParent);

			var button =
				obj.GetComponent<CharacterButton>();

			button.Setup(
				character,
				this);

			characterButtons.Add(button);
		}

		characterPanel.SetActive(true);
	}

	void SelectedCountUpdate()
	{
		selectedCountText.text =
			"選択：" +
			selectedCharacters.Count +
			"人";

		Debug.Log(
			"選択人数：" +
			selectedCharacters.Count);
	}

	//public void OnCharacterSelected(
	//	CharacterRuntimeData character,
	//	CharacterButton button)
	//{
		//// 前の選択を戻す
		//if (selectedCharacterButton != null)
		//{
		//	selectedCharacterButton.SetNormal();
		//}

		//// 新しい選択
		//selectedCharacter = character;

		//selectedCharacterButton = button;

		//// 色変更
		//selectedCharacterButton.SetSelected();

		//Debug.Log(
		//	"武将選択：" +
		//	character.baseData.characterName);
	//}

	public void AddSelectedCharacter(
	CharacterRuntimeData character)
	{
		if (selectedCharacters
			.Count >= 3)
		{
			Debug.Log(
				"最大3人まで");

			return;
		}

		if (!selectedCharacters
			.Contains(character))
		{
			selectedCharacters
				.Add(character);
		}

		SelectedCountUpdate();

	}

	public void RemoveSelectedCharacter(
		CharacterRuntimeData character)
	{
		if (selectedCharacters
			.Contains(character))
		{
			selectedCharacters
				.Remove(character);
		}

		SelectedCountUpdate();

	}

	public void CheckVictoryDefeat()
	{
		//--------------------------------
		// nullチェック
		//--------------------------------

		if (victoryProvinceId == null ||
			string.IsNullOrEmpty(
				victoryProvinceId))
		{
			Debug.LogError(
				"victoryProvince 未設定");

			return;
		}

		if (defeatProvinceId == null ||
			string.IsNullOrEmpty(
				defeatProvinceId))
		{
			Debug.LogError(
				"defeatProvince 未設定");

			return;
		}

		//--------------------------------
		// 存在チェック
		//--------------------------------

		if (!runtimeData.ContainsKey(
			victoryProvinceId))
		{
			Debug.LogError(
				"存在しない victoryProvince: "
				+ victoryProvinceId);

			return;
		}

		if (!runtimeData.ContainsKey(
			defeatProvinceId))
		{
			Debug.LogError(
				"存在しない defeatProvince: "
				+ defeatProvinceId);

			return;
		}

		//--------------------------------
		// 取得
		//--------------------------------

		var victory =
			runtimeData[
				victoryProvinceId];

		var defeat =
			runtimeData[
				defeatProvinceId];

		//--------------------------------
		// 勝利判定
		//--------------------------------

		if (victory.owner.baseData.factionId
			== playerFactionId)
		{
			OnGameWin();
			return;
		}

		//--------------------------------
		// 敗北判定
		//--------------------------------

		if (defeat.owner.baseData.factionId
			!= playerFactionId)
		{
			OnGameLose();
			return;
		}
	}

	public void CheckCheckpointUnlocks()
	{
		foreach (var province
			in runtimeData.Values)
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

			Debug.Log("Test CheckCheckpointUnlocks " +
				province.baseData.requiredProvinces.Count);

			//--------------------------------
			// 必須拠点チェック
			//--------------------------------

			foreach (var req
				in province.baseData.requiredProvinces)
			{
				//--------------------------------
				// req null 対策（重要）
				//--------------------------------
				//var reqProvince =
				//runtimeData[req.provinceId];

				//if (reqProvince.owner.baseData.factionId
				//	!= playerFactionId)
				//{
				//	allCaptured = false;
				//	break;
				//}

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

				if (!runtimeData.ContainsKey(
					req.provinceId))
				{
					Debug.LogError(
						"存在しないprovinceId: " +
						req.provinceId);

					allCaptured = false;
					break;
				}

				var reqProvince =
					runtimeData[req.provinceId];

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
					!= playerFactionId)
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

	public void OnConfirmAttack()
	{
		confirmAttackPanel.SetActive(false);

		battleManager.TryAttack(
			pendingFrom,
			pendingTarget,
			pendingAttackers);

	}
	public void OnCancelAttack()
	{
		confirmAttackPanel.SetActive(false);

		Debug.Log("攻撃キャンセル");
	}

	public void OnConfirmMove()
	{
		confirmMovePanel.SetActive(false);

		MoveOneCharacter(
			pendingFrom,
			pendingTarget,
			pendingAttackers);

	}
	public void OnCancelMove()
	{
		confirmMovePanel.SetActive(false);

		Debug.Log("移動キャンセル");
	}
	public void OnEndTurnButton()
	{
		if (!isPlayerTurn)
			return;

		Debug.Log("プレイヤーターン終了");

		isPlayerTurn = false;

		StartEnemyTurn();
	}


	void OnGameWin()
	{
		Debug.Log("勝利！");
		winPanel.SetActive(true);
		isGameOver = true;
	}
	void OnGameLose()
	{
		Debug.Log("敗北…");
		losePanel.SetActive(true);
		isGameOver = true;
	}
	public void OnRetryButton()
	{
		Debug.Log("リトライ");

		// 現在のシーンを取得
		Scene currentScene =
			SceneManager.GetActiveScene();

		// シーン再読み込み
		SceneManager.LoadScene(
			currentScene.name);
	}

	public void OnConfirmAction()
	{
		if (selectedNode == null ||
			toProvince == null ||
			selectedCharacters.Count == 0)
		{
			Debug.Log("選択不足");
			return;
		}

		TryAction(
			selectedProvince,
			toProvince,
			selectedCharacters);

		ResetSelection();
	}

	void ShowDevelopmentMenu(
	ProvinceRuntimeData province)
	{
		Debug.Log(
			province.baseData.provinceName +
			" の開発メニュー");

		// 仮：UI表示
	}

	void SetSelectedNode(
	ProvinceNode node)
	{
		// 旧選択解除
		if (selectedNode != null)
		{
			selectedNode.SetSelected(false);
		}

		selectedNode = node;

		if (selectedNode != null)
		{
			selectedNode.SetSelected(true);
		}
	}

	void ClearSelectedNode()
	{
		if (selectedNode != null)
		{
			selectedNode.SetSelected(false);
		}

		selectedNode = null;
	}

	void ClearAllHighlights()
	{
		var allNodes =
			FindObjectsOfType<ProvinceNode>();

		foreach (var node in allNodes)
		{
			node.ClearHighlight();
		}
	}

	public ProvinceRuntimeData
	GetProvinceById(
		string provinceID)
	{
		foreach (var p
			in runtimeData.Values)
		{
			if (p.baseData
				.provinceName
				== provinceID)
			{
				return p;
			}
		}

		return null;
	}

	public FactionRuntimeData
	GetFactionById(
		string factionId)
	{
		foreach (var f
			in factionRuntimeDataDict
				.Values)
		{
			if (f.baseData
				.factionId
				== factionId)
			{
				return f;
			}
		}

		return null;
	}
	public void OnClickSave()
	{
		SaveManager.Instance
			.SaveGame(this);
	}

	public void OnClickLoad()
	{
		SaveManager.Instance
			.LoadGame(this);
	}

	public IEnumerable<ProvinceRuntimeData>
	GetAllProvinces()
	{
		return runtimeData.Values;
	}

	public IEnumerable<FactionRuntimeData>
	GetAllFactions()
	{
		return factionRuntimeDataDict.Values;
	}

	void RegisterProvinceNodes()
	{
		provinceNodeDict.Clear();

		var nodes =
			FindObjectsOfType<
				ProvinceNode>();

		foreach (var node in nodes)
		{
			if (provinceNodeDict
				.ContainsKey(
					node.provinceId))
			{
				Debug.LogError(
					"Duplicate ProvinceNode ID: "
					+ node.provinceId);

				continue;
			}

			provinceNodeDict
				.Add(
					node.provinceId,
					node);
		}
	}

	ProvinceRuntimeData
	GetProvinceRuntime(
		string provinceId)
	{
		if (runtimeData
			.TryGetValue(
				provinceId,
				out var runtime))
		{
			return runtime;
		}

		Debug.LogError(
			"Province not found: "
			+ provinceId);

		return null;
	}

	List<ProvinceRuntimeData>
	GetEnemyProvinces()
	{
		List<ProvinceRuntimeData>
			result =
			new List<
				ProvinceRuntimeData>();

		foreach (var province
			in runtimeData.Values)
		{
			// プレイヤー以外
			if (province.owner.baseData.factionId
				!= playerFactionId)
			{
				result.Add(province);
			}
		}

		return result;
	}

	public FactionRuntimeData
	GetFaction(string factionId)
	{
		if (factionRuntimeDataDict
			.TryGetValue(
				factionId,
				out var faction))
		{
			return faction;
		}

		Debug.LogError(
			"Faction not found: "
			+ factionId);

		return null;
	}

}