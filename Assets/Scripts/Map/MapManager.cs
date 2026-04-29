//using System;
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
	//public const string playerFactionId = "001";
	string enemyFactionId = "002";
	string neutralFactionId = "100";

	[Header("UI設定")]
	[SerializeField] TextMeshProUGUI turnText;
	[SerializeField] TextMeshProUGUI goldText;

	// キャラクター選択パネル
	[SerializeField] public GameObject characterPanel;
	[SerializeField] TextMeshProUGUI selectedCountText;
	[SerializeField] Transform characterListParent;
	[SerializeField] GameObject characterButtonPrefab;

	// 攻撃確認パネル
	[SerializeField] public GameObject confirmAttackPanel;
	//[SerializeField] Button confirmAttackButton;
	//[SerializeField] Button cancelAttackButton;
	//[SerializeField] TextMeshProUGUI attackerListText;
	//[SerializeField] TextMeshProUGUI defenderListText;
	//[SerializeField] TextMeshProUGUI powerText;
	//[SerializeField] TextMeshProUGUI confirmMessageText;
	
	// 移動確認パネル
	[SerializeField] public GameObject confirmMovePanel;
	[SerializeField] TextMeshProUGUI moveMessageText;
	[SerializeField] TextMeshProUGUI moveCharacterListText;

	[SerializeField] GameObject winPanel;
	[SerializeField] GameObject losePanel;

	//[Header("初期武将")]

	//[SerializeField]
	//private CharacterDatabase characterDatabase;

	//[Header("勝敗条件")]

	//[SerializeField] string victoryProvinceId;
	//[SerializeField] string defeatProvinceId;

	[Header("補充設定")]

	[SerializeField] int reinforceAmount = 50;
	[SerializeField] int reinforceCost = 100;

	[Header("全地域データ")]

	// Dictionary
	Dictionary<string, ProvinceRuntimeData>
		runtimeData =
			new Dictionary<string, ProvinceRuntimeData>();

	Dictionary<string, ProvinceNode>
		provinceNodeDict =
			new Dictionary<string, ProvinceNode>();

	Dictionary<FactionData, FactionRuntimeData>
		factionRuntimeData;

	Dictionary<string, FactionRuntimeData>
		factionRuntimeDataDict =
			new Dictionary<string, FactionRuntimeData>();

	[SerializeField]
	BattleManager battleManager;

	[SerializeField]
	List<FactionData>
		allFactions;

	FactionRuntimeData
		playerFactionRuntime;

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
	ProvinceNode[] provinceNodes;

	ProvinceRuntimeData pendingFrom;
	ProvinceRuntimeData pendingTarget;
	List<CharacterRuntimeData> pendingAttackers;

	VictoryManager victoryManager;
	ProvinceSelectionManager provinceSelectionManager;
	CharacterManager characterManager;
	SaveManager saveManager;
	[SerializeField] BattleUIHandler battleUIHandler;


	void Start()
	{
		CharacterDatabase.Load();

		victoryManager =
			new VictoryManager(this);
		provinceSelectionManager =
			new ProvinceSelectionManager(this);
		characterManager =
			new CharacterManager(this);
		saveManager = new SaveManager(this);

		battleUIHandler.Initialize(this);

		InitializeRuntimeData();
		//InitializeProvinceNodes();

		//Debug.Log("Gold " + playerFactionRuntime.gold);

		UpdateGoldUI();
		battleManager.Initialize(this);

		StartPlayerTurn();
	}

	void Update()
	{
		// 右クリック
		if (Input.GetMouseButtonDown(1))
		{
			provinceSelectionManager.CancelSelection();
		}
	}

	void StartPlayerTurn()
	{
		isPlayerTurn = true;
		if (currentTurn > 1)
		{
			AddTurnIncome();
		}

		turnText.text =
			"ターン数：" + currentTurn;

		//Debug.Log(
		//	"ターン " +
		//	currentTurn);

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
				== Constants.playerFactionId)
			{
				totalIncome +=
					province.income;
			}
		}

		playerFactionRuntime.gold
			+= totalIncome;

		UpdateGoldUI();

		Debug.Log(
			"収入：" + totalIncome);
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
		InitializeProvinceNodes();
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

			if (master.factionId == Constants.playerFactionId)
			{
				playerFactionRuntime = runtime;
			}
		}
	}
	void InitializeProvinceNodes()
	{
		provinceNodes =
			FindObjectsOfType<ProvinceNode>();

		Debug.Log(
			"ProvinceNode数: "
			+ provinceNodes.Length);
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
				//Debug.LogError(
				//	"Character create Non "
				//	);

				continue;
			}

			//--------------------------------
			// 武将生成
			//--------------------------------

			foreach (var charId
				in master.initialCharacterIds)
			{
				//Debug.Log("Set Init Char" + charId);
				var character =
				CharacterFactory
					.CreateCharacter(
							charId);

				if (character == null)
				{
					//Debug.LogError(
					//	"Character create failed: "
					//	+ charId);

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

				//Debug.Log(
				//	"Added: "
				//	+ character.baseData.characterName
				//	+ " → "
				//	+ province.baseData.provinceName);
			}
		}
	}

	public CharacterRuntimeData CreateCharacter(
		string characterId)
	{
		characterManager.CreateCharacter(characterId);
		return null;
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
		provinceSelectionManager.OnProvinceClicked(node);
	}

	public void ResetSelection()
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

		//Debug.Log("選択リセット");
	}

	public void TryAction(
		ProvinceRuntimeData from,
		ProvinceRuntimeData target,
		List<CharacterRuntimeData> attackers)
		{

			// 同勢力 → 移動
			if (from.owner
				== target.owner)
			{
				ShowMoveConfirm(from, target, attackers);

				//Debug.Log(
				//	"移動：" +
				//	from.baseData.provinceName +
				//	" → " +
				//	target.baseData.provinceName);

				return;
			}

			// 敵勢力 → 攻撃
			ShowAttackConfirm(
				from,
				target,
				attackers);
			//Debug.Log(
			//	"攻撃：" +
			//	from.baseData.provinceName +
			//	" → " +
			//	target.baseData.provinceName);
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
		battleUIHandler.SetMessageText(target.baseData.provinceName);

		// 攻撃側表示
		battleUIHandler.SetAttackerListText(attackers);

		// 防御側表示
		battleUIHandler.SetDefenderListText(target.characterList);

		battleManager.addBattleCount(attackers);
		battleManager.addBattleCount(target.characterList);

		// 戦力表示
		int attackPower =
			battleManager.GetAttackPower(attackers);

		int defensePower =
			battleManager.GetDefensePower(target);

		battleUIHandler.SetPowerText(attackPower,defensePower);

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
		provinceSelectionManager.UpdateAllNodeColors();
	}

	public void SetMoveOneCharacter()
	{
		MoveOneCharacter(
			pendingFrom,
			pendingTarget,
			pendingAttackers);
	}

	public void MoveOneCharacter(
		ProvinceRuntimeData from,
		ProvinceRuntimeData to,
		List<CharacterRuntimeData> attackers)
	{
		provinceSelectionManager.MoveOneCharacter(from, to, attackers);
	}

	public void HighlightNeighbors(
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
		provinceSelectionManager.MoveCharacterToFriendlyProvince(
			character,
			oldFaction,
			lostProvince);
	}

	public void ShowCharacterList(
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

	public void SelectedCountUpdate()
	{
		selectedCountText.text =
			"選択：" +
			selectedCharacters.Count +
			"人";

		//Debug.Log(
		//	"選択人数：" +
		//	selectedCharacters.Count);
	}

	public void AddSelectedCharacter(
		CharacterRuntimeData character)
	{
		//characterManager.AddSelectedCharacter(character);

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
		//characterManager.RemoveSelectedCharacter(character);
		//Debug.Log("SelectedCharacters Count:" + selectedCharacters.Count);

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
		victoryManager.CheckVictoryDefeat();
	}

	public void CheckCheckpointUnlocks()
	{
		victoryManager.CheckCheckpointUnlocks();
	}

	public void ConfirmAttackSub()
	{
		//confirmAttackPanel.SetActive(false);

		battleManager.TryAttack(
			pendingFrom,
			pendingTarget,
			pendingAttackers);

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


	public void OnConfirmMove()
	{
		provinceSelectionManager.OnConfirmMove();
	}
	public void OnCancelMove()
	{
		provinceSelectionManager.OnCancelMove();
	}
	public void OnEndTurnButton()
	{
		if (!isPlayerTurn)
			return;

		Debug.Log("プレイヤーターン終了");

		isPlayerTurn = false;

		StartEnemyTurn();
	}

	public void OnGameWin()
	{
		Debug.Log("勝利！");
		winPanel.SetActive(true);
		isGameOver = true;
	}
	public void OnGameLose()
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

	public void ShowDevelopmentMenu(
	ProvinceRuntimeData province)
	{
		Debug.Log(
			province.baseData.provinceName +
			" の開発メニュー");

		// 仮：UI表示
	}


	public void ClearSelectedNode()
	{
		if (selectedNode != null)
		{
			selectedNode.SetSelected(false);
		}

		selectedNode = null;
	}

	public void ClearAllHighlights()
	{
		var allNodes =
			FindObjectsOfType<ProvinceNode>();

		foreach (var node in allNodes)
		{
			node.ClearHighlight();
		}
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

	public ProvinceRuntimeData
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
					!= Constants.playerFactionId)
				{
					result.Add(province);
				}
			}

			return result;
		}

	public List<CharacterRuntimeData>
		GetSelectedCharacters()
		{
			return selectedCharacters;
		}

	public void AddSelectedCharacters(
		CharacterRuntimeData ch)
	{
		selectedCharacters.Add(ch);
	}
	public void RemoveSelectedCharacters(
		CharacterRuntimeData ch)
	{
		selectedCharacters.Remove(ch);
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

	public bool GetisGameOver()
	{
		return isGameOver;
	}

	public bool GetisPlayerTurn()
	{
		return isPlayerTurn;
	}
	public void SetSelectedNode(
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
	public void SetSelectedNodeFlag(bool flag)
	{
		selectedNode.SetSelected(flag);
	}
	public ProvinceNode GetselectedNode()
	{
		return selectedNode;
	}

	public void SetselectedProvince(
		ProvinceRuntimeData p)
	{
		selectedProvince = p;
	}
	public ProvinceRuntimeData GetselectedProvince()
	{
		return selectedProvince;
	}

	public void SettoNode(
		ProvinceNode node)
	{
		toNode = node;
	}
	public ProvinceNode GettoNode()
	{
		return toNode;
	}
	public void SettoPrpvince(
		ProvinceRuntimeData runtimeData)
	{
		toProvince = runtimeData;
	}

	public ProvinceNode[]
		GetProvinceNodes()
		{
			return provinceNodes;
		}
}