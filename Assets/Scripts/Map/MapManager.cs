using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

// マップ全体の制御

public class MapManager : MonoBehaviour
{
	[Header("初期所有者設定")]

	[SerializeField] FactionData playerFaction;
	public FactionData enemyFaction;
	public FactionData neutralFaction;

	[Header("初期武将")]

	public CharacterData hero1;
	public CharacterData hero2;
	public CharacterData hero3;

	public CharacterData enemy1;
	public CharacterData enemy2;

	[Header("地域参照")]

	public ProvinceData provinceA1;
	public ProvinceData provinceA2;
	public ProvinceData provinceA3;
	public ProvinceData provinceA4;

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
	GameObject characterPanel;

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
		InitializeRuntimeData();
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

		Debug.Log(
			"ターン " +
			currentTurn);

		Debug.Log(
			"プレイヤーターン開始");
	}

	void CancelSelection()
	{
		if (selectedNode == null)
		{
			return;
		}
		else
		{

		}

		selectedNode.SetSelected(false);

		// CharacterPanel を閉じる
		if (characterPanel != null)
		{
			characterPanel.SetActive(false);
		}

		selectedNode = null;
		selectedProvince = null;

		UpdateAllNodeColors();

		Debug.Log("選択キャンセル");
	}

	// 実行用データ作成
	void InitializeRuntimeData()
	{
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

		var enemyProvince =
			runtimeData[provinceA3];

		enemyProvince.AddCharacter(
			new CharacterRuntimeData(enemy1));

		enemyProvince.AddCharacter(
			new CharacterRuntimeData(enemy2));

	}
	//void SetInitialCharacters()
	//{
	//	var province =
	//		runtimeData[provinceA1];

	//	province.AddCharacter(
	//		new CharacterRuntimeData(hero1));

	//	province.AddCharacter(
	//		new CharacterRuntimeData(hero2));

	//	province.AddCharacter(
	//		new CharacterRuntimeData(hero3));

	//	//province.AddCharacter(
	//	//	new CharacterRuntimeData(hero1));

	//	Debug.Log("A-1 に武将3人配置");
	//}
	void SetInitialCharacters()
	{
		foreach (var pair in runtimeData)
		{
			var province = pair.Value;

			// 念のため初期化
			province.stationedCharacters
				.Clear();

			// ★ baseData から生成
			foreach (var chData
				in province.baseData
					.initialCharacters)
			{
				// ★ 必ず new（超重要）
				var runtimeCharacter =
					new CharacterRuntimeData(
						chData);

				province.stationedCharacters
					.Add(runtimeCharacter);

				Debug.Log(
					"初期配置：" +
					chData.characterName +
					" → " +
					province.baseData.provinceName +
					" ID=" +
					runtimeCharacter
						.GetHashCode());
			}
		}
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

	public void OnEndTurnButton()
	{
		if (!isPlayerTurn)
			return;

		Debug.Log("プレイヤーターン終了");

		isPlayerTurn = false;

		StartEnemyTurn();
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

			// 隣接地域を確認
			foreach (var neighbor
				in province.baseData.neighbors)
			{
				var target =
					runtimeData[neighbor];

				// プレイヤー地域なら攻撃
				if (target.ownerFaction
					== playerFaction)
				{
					Debug.Log(
						"敵攻撃：" + attacker.baseData.name +
						province.baseData.provinceName +
						" → " +
						target.baseData.provinceName);

					var attackers =
						new List<CharacterRuntimeData>(
							province.stationedCharacters);

					TryAttack(
						province,target,province.stationedCharacters);

					Debug.Log(
						"敵攻撃人数：" +
						attackers.Count);
					// ★1回攻撃したら終了
					return;
				}
			}
		}

		Debug.Log("敵は攻撃できない");
	}

	// 地域クリック処理
	public void OnProvinceClicked(
		ProvinceNode node)
	{
		if (!isPlayerTurn)
		{
			Debug.Log("敵ターン中");

			return;
		}

		var runtime =
			runtimeData[node.provinceData];

		// ■1回目クリック
		if (selectedNode == null)
		{
			selectedNode = node;
			selectedProvince = runtime;

			ShowCharacterList(runtime);

			//SelectCharacter(runtime);

			//node.SetSelected(true);
			UpdateAllNodeColors();

			HighlightNeighbors(runtime);

			Debug.Log(
				"選択：" +
				node.provinceData.provinceName);

			return;
		}

		// ■同じ地域クリック → 解除
		if (selectedNode == node)
		{
			selectedNode.SetSelected(false);

			selectedNode = null;
			selectedProvince = null;

			// CharacterPanel を閉じる
			if (characterPanel != null)
			{
				characterPanel.SetActive(false);
			}

			UpdateAllNodeColors();
			Debug.Log("選択解除");

			return;
		}

		// ■2回目クリック → 行動
		var targetProvince = runtime;

		TryAction(
			selectedProvince,
			targetProvince);

		// 選択解除
		selectedNode.SetSelected(false);

		selectedNode = null;
		selectedProvince = null;

		// CharacterPanel を閉じる
		if (characterPanel != null)
		{
			characterPanel.SetActive(false);
		}

		UpdateAllNodeColors();
	}

	void TryAction(
	ProvinceRuntimeData from,
	ProvinceRuntimeData target)
	{
		var attackers =
			new List<CharacterRuntimeData>(
			selectedCharacters);

		// 同勢力 → 移動
		if (from.ownerFaction
			== target.ownerFaction)
		{
			MoveOneCharacter(from, target, attackers);

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

		confirmMessageText.text =
			target.baseData.provinceName +
			" を攻撃しますか？";

		confirmAttackPanel.SetActive(true);
	}

	// 攻撃処理
	void TryAttack(
		ProvinceRuntimeData from,
		ProvinceRuntimeData target,
		List<CharacterRuntimeData> attackers)
	{
		// 隣接チェック
		if (!from.baseData.neighbors
			.Contains(target.baseData))
		{
			Debug.Log("隣接していない");
			return;
		}

		Debug.Log(
			from.baseData.provinceName
			+ " → "
			+ target.baseData.provinceName);

		// 同勢力なら移動
		if (from.ownerFaction
			== target.ownerFaction)
		{
			MoveOneCharacter(from, target, attackers);
			return;
		}

		if (selectedCharacters.Count == 0)
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

		bool win =
			CalculateBattle(
				attackPower,
				defensePower);

		if (win)
		{
			//target.ownerFaction =
			//	from.ownerFaction;
			
			Debug.Log(
				"占領成功！");

			Debug.Log(attackers);

			//CaptureProvince(from, target, selectedCharacters);
			CaptureProvince(from, target, attackers);

			Debug.Log(attackers);

			//UpdateAllNodeColors();
		}
		else
		{
			Debug.Log("撤退");
		}
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

	int GetAttackPower(
	List<CharacterRuntimeData> characters)
	{
		int total = 0;

		foreach (var ch
			in characters)
		{
			total += ch.GetAttack();
		}

		return total;
	}

	int GetDefensePower(
	ProvinceRuntimeData province)
	{
		int total =
			province.baseData.defenseValue;

		// 武将防御を加算
		foreach (var ch
			in province.stationedCharacters)
		{
			total +=
				ch.baseData.defensePower;
		}

		return total;
	}

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
			node.UpdateColor(
				runtime.ownerFaction);

			// 人数更新
			int count =
				runtime.stationedCharacters.Count;

			node.UpdateCount(count);

		}
		// 選択中ノードの上書き
		if (selectedNode != null)
		{
			// 色更新
			selectedNode.SetSelected(true);

		}
	}

	void MoveOneCharacter(
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

		// 最後の武将を取得
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
				attacker.baseData.characterName);
		}

		//// 移動元から削除
		//from.stationedCharacters.Remove(character);

		//// 移動先へ追加
		//to.AddCharacter(character);

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

	void SelectCharacter(
	ProvinceRuntimeData province)
	{
		//	if (province.stationedCharacters.Count == 0)
		//	{
		//		Debug.Log("武将なし");
		//		selectedCharacter = null;
		//		return;
		//	}
		//	// 仮：先頭武将を選択
		//	selectedCharacter =
		//		province.stationedCharacters[0];

		//	Debug.Log(
		//		"武将選択：" +
		//		selectedCharacter
		//		.baseData
		//		.characterName);
	}

	void MoveCharacterToFriendlyProvince(
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
		}

		if (characterPanel != null)
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

		Debug.Log(
			"選択人数：" +
			selectedCharacters.Count);
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

		Debug.Log(
			"選択人数：" +
			selectedCharacters.Count);
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
		FactionData oldFaction =
			target.ownerFaction;

		// 防衛側コピー
		var defenders =
			new List<CharacterRuntimeData>(
				target.stationedCharacters);

		// =========================
		// 防衛側退避処理
		// =========================

		foreach (var defender in defenders)
		{
			Debug.Log(
				"削除前：" +
				target.stationedCharacters.Count);

			// ★ 先に削除（重要）
			target.stationedCharacters
				.Remove(defender);
			// ★ その後退避
			MoveCharacterToFriendlyProvince(
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

		target.ownerFaction =
			from.ownerFaction;

		// =========================
		// 攻撃側前進
		// =========================

		// ★ attackers のコピーを作る
		var movingAttackers =
			new List<CharacterRuntimeData>(
				attackers);

		foreach (var attacker in movingAttackers)
		{
			from.stationedCharacters
				.Remove(attacker);

			target.stationedCharacters
				.Add(attacker);

			Debug.Log(
				"前進：" +
				attacker.baseData.characterName);
		}

		UpdateAllNodeColors();
	}
	public void OnConfirmAttack()
	{
		confirmAttackPanel.SetActive(false);

		TryAttack(
			pendingFrom,
			pendingTarget,
			pendingAttackers);
	}
	public void OnCancelAttack()
	{
		confirmAttackPanel.SetActive(false);

		Debug.Log("攻撃キャンセル");
	}
}