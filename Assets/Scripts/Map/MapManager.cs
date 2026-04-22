using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;

// マップ全体の制御

public class MapManager : MonoBehaviour
{
	[Header("初期所有者設定")]

	[SerializeField] FactionData playerFaction;
	public FactionData enemyFaction;
	public FactionData neutralFaction;

	[SerializeField]
	TextMeshProUGUI goldText;

	//[Header("初期武将")]

	[SerializeField]
	CharacterDatabase characterDatabase;

	[Header("勝敗条件")]

	[SerializeField]
	ProvinceData victoryProvince;

	[SerializeField]
	ProvinceData defeatProvince;

	[Header("補充設定")]

	[SerializeField]
	int reinforceAmount = 50;

	[SerializeField]
	int reinforceCost = 100;

	[Header("全地域データ")]

	// InspectorでA-1〜A-7登録
	public List<ProvinceData>
		allProvinceData =
		new List<ProvinceData>();

	// 実行中データ管理
	private Dictionary<
		ProvinceData,
		ProvinceRuntimeData>
		runtimeData;

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
			if (province.ownerFaction
				!= playerFaction)
			{
				result.Add(province);
			}
		}

		return result;
	}

	void Start()
	{
		characterDatabase.Initialize();
		InitializeRuntimeData();
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
			if (province.ownerFaction
				== playerFaction)
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

	void CancelSelection()
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
	void InitializeRuntimeData()
	{
		InitializeFactionRuntimeData();

		runtimeData =
			new Dictionary<
				ProvinceData,
				ProvinceRuntimeData>();

		foreach (var data
			in allProvinceData)
		{
			var runtime =
				new ProvinceRuntimeData(data);

			runtimeData[data] = runtime;
		}

		// 初期所有設定
		SetInitialOwners();
		SetInitialCharacters();
		UpdateAllNodeColors();

		UpdateGoldUI();

	}

	void InitializeFactionRuntimeData()
	{
		factionRuntimeData =
			new Dictionary<
				FactionData,
				FactionRuntimeData>();

		foreach (var faction
			in allFactions)
		{
			var runtime =
				new FactionRuntimeData(
					faction);

			factionRuntimeData
				[faction] = runtime;
		}

		// プレイヤー取得
		playerFactionRuntime =
			factionRuntimeData
				[playerFaction];
	}

	void SetInitialOwners()
	{
		SetOwner("A-1", playerFaction);
		SetOwner("A-2", neutralFaction);
		SetOwner("A-3", enemyFaction);
		SetOwner("A-4", enemyFaction);
		SetOwner("A-5", neutralFaction);
		SetOwner("A-6", enemyFaction);
		SetOwner("A-7", enemyFaction);

		//var enemyProvince =
		//	runtimeData[provinceA3];

		//enemyProvince.AddCharacter(
		//	new CharacterRuntimeData(enemy1));

		//enemyProvince.AddCharacter(
		//	new CharacterRuntimeData(enemy2));

	}

	void SetInitialCharacters()
	{
		foreach (var province
			in runtimeData.Values)
		{
			var idList =
				province.baseData
					.initialCharacterIds;

			if (idList == null)
				continue;

			foreach (var charId
				in idList)
			{
				//var charData =
				//	characterDatabase
				//		.GetCharacter(charId);

				//if (charData == null)
				//{
				//	Debug.LogWarning(
				//		"武将IDが見つかりません: "
				//		+ charId);
				//	continue;
				//}

				//var runtime =
				//	new CharacterRuntimeData(
				//		charData);

				//runtime.soldierCount =
				//	charData.maxSoldier;

				//province.stationedCharacters
				//	.Add(runtime);

				//Debug.Log(
				//	province.baseData.provinceName +
				//	" に生成：" +
				//	charData.characterName);

				//var runtime =
				//	CreateCharacter(charId);

				//if (runtime == null)
				//	continue;

				//province.stationedCharacters
				//	.Add(runtime);

				//Debug.Log(
				//	province.baseData.provinceName +
				//	" に配置: " +
				//	runtime.baseData.characterName);

				AddCharacterToProvince(
					province,
					charId);
			}
		}
	}

	CharacterRuntimeData CreateCharacter(
		string characterId)
	{
		var master =
			characterDatabase
				.GetCharacter(characterId);

		if (master == null)
		{
			Debug.LogWarning(
				"武将生成失敗: " +
				characterId);

			return null;
		}

		var runtime =
			new CharacterRuntimeData();

		runtime.characterId		= master.characterId;
		runtime.characterName	= master.characterName;

		runtime.attack			= master.initialAttack;
		runtime.defense			= master.initialDefense;
		runtime.leadership		= master.leadership;
		runtime.soldierCount	= master.maxSoldier;

		runtime.skillIds =
			new List<string>(
				master.initialSkillIds);

		runtime.equipmentId =
			master.initialEquipmentId;

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

	void ReinforceProvince(
		ProvinceRuntimeData province,
		string characterId)
	{
		var character =
			CreateCharacter(characterId);

		if (character == null)
			return;

		province.stationedCharacters
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

		province.stationedCharacters
			.Add(runtime);

		Debug.Log(
			province.baseData.provinceName +
			" に武将追加: " +
			runtime.characterName);
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
			character.characterName +
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
	FactionData faction)
	{
		foreach (var pair in runtimeData)
		{
			if (pair.Key.provinceName
				== provinceName)
			{
				pair.Value.ownerFaction =
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
			if (province.stationedCharacters.Count == 0)
				continue;

			var attacker =
				province.stationedCharacters[0];

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
							province.stationedCharacters);

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
				//if (target.ownerFaction
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
			runtimeData[node.provinceData];

		// =========================
		// ■1回目クリック（出発地選択）
		// =========================
		if (selectedNode == null)
		{
			selectedProvince = runtime;
			SetSelectedNode(node);

			UpdateAllNodeColors();

			if (selectedProvince.ownerFaction == playerFaction)
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
					node.provinceData.provinceName);

			Debug.Log(
				"地域情報メニュー表示：" +
				node.provinceData.provinceName);

			return;
		}

		if (selectedProvince.ownerFaction != playerFaction)
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
				node.provinceData.provinceName);

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
				node.provinceData.provinceName);

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
		//var attackers =
		//	new List<CharacterRuntimeData>(
		//	selectedCharacters);

		// 同勢力 → 移動
		if (from.ownerFaction
			== target.ownerFaction)
		{
			//MoveOneCharacter(from, target, attackers);
			ShowMoveConfirm(from, target, attackers);

			Debug.Log(
				"移動：" +
				from.baseData.provinceName +
				" → " +
				target.baseData.provinceName);

			return;
		}

		// 敵勢力 → 攻撃
		//TryAttack(from, target, selectedCharacters);
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
				ch.characterName + "\n";
		}

		attackerListText.text =
			attackerText;

		// 防御側表示
		string defenderText = "防御側：\n";

		foreach (var ch
			in target.stationedCharacters)
		{
			defenderText +=
				ch.characterName + "\n";
		}

		defenderListText.text =
			defenderText;

		battleManager.addBattleCount(attackers);
		battleManager.addBattleCount(target.stationedCharacters);

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
				ch.characterName +
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
			if (node.provinceData == null)
			{
				Debug.LogError(
					node.name +
					" provinceDataが未設定");

				continue;
			}

			var runtime =
				runtimeData[node.provinceData];

			if (runtime.ownerFaction == null)
			{
				Debug.LogError(
					node.name +
					" ownerFactionがnull");

				continue;
			}

			// 色更新
			node.UpdateColor(runtime);
				//runtime.ownerFaction);

			node.ClearHighlight();

			// 人数更新
			int count =
				runtime.stationedCharacters.Count;

			node.UpdateCount(count);

		}


		// 選択中ノードの上書き
		if (selectedNode != null)
		{
			Debug.Log("test True");
			// 色更新
			selectedNode.SetSelected(true);
		}
		else
		{
			Debug.Log("test False");
		}
	}

	public void MoveOneCharacter(
	ProvinceRuntimeData from,
	ProvinceRuntimeData to,
	List<CharacterRuntimeData> attackers)
	{
		// 移動元に武将がいない
		if (from.stationedCharacters.Count == 0)
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
				from.stationedCharacters.Count);

			// 移動元から削除
			from.stationedCharacters
				.Remove(attacker);
			// 移動先へ追加
			to.AddCharacter(attacker);

			Debug.Log(
				"退避：" +
				attacker.characterName);
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
				if (neighborRuntime.ownerFaction
					== from.ownerFaction)
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
	ProvinceData data)
	{
		var nodes =
			FindObjectsOfType<ProvinceNode>();

		foreach (var node in nodes)
		{
			if (node.provinceData == data)
			{
				return node;
			}
		}

		return null;
	}

	//void SelectCharacter(
	//ProvinceRuntimeData province)
	//{
	//	//	if (province.stationedCharacters.Count == 0)
	//	//	{
	//	//		Debug.Log("武将なし");
	//	//		selectedCharacter = null;
	//	//		return;
	//	//	}
	//	//	// 仮：先頭武将を選択
	//	//	selectedCharacter =
	//	//		province.stationedCharacters[0];

	//	//	Debug.Log(
	//	//		"武将選択：" +
	//	//		selectedCharacter
	//	//		.baseData
	//	//		.characterName);
	//}

	public void MoveCharacterToFriendlyProvince(
	CharacterRuntimeData character,
	FactionData oldFaction,
	ProvinceRuntimeData lostProvince)
	{
		foreach (var province
			in runtimeData.Values)
		{
			// ★ 占領された地域は除外
			if (province == lostProvince)
				continue;

			if (province.ownerFaction
				== oldFaction)
			{
				province.stationedCharacters
					.Add(character);

				Debug.Log(
					"退避：" +
					character.characterName + 
					" → " +
					province.baseData.provinceName);
				return;
			}
		}

		Debug.Log(
			"退避先なし：" +
			character.characterName);
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
			in province.stationedCharacters)
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

	public void OnCharacterSelected(
		CharacterRuntimeData character,
		CharacterButton button)
	{
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
	}

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
		var victory =
			runtimeData[victoryProvince];

		var defeat =
			runtimeData[defeatProvince];

		// 勝利判定
		if (victory.ownerFaction
			== playerFaction)
		{
			OnGameWin();
			return;
		}

		// 敗北判定
		if (defeat.ownerFaction
			!= playerFaction)
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

			bool allCaptured = true;

			foreach (var req
				in province.baseData.requiredProvinces)
			{
				var reqProvince =
					runtimeData[req];

				if (reqProvince.ownerFaction
					!= playerFaction)
				{
					allCaptured = false;
					break;
				}
			}

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
}