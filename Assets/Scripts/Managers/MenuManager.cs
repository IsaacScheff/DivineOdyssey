using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance;
    [SerializeField] private GameObject _selectedHeroObject;
    [SerializeField] private GameObject _tileObject;
    [SerializeField] private GameObject _tileUnitObject;
    [SerializeField] private GameObject _tileUnitStats;
    [SerializeField] private GameObject _attackPreview;
    [SerializeField] private GameObject _actionMenu;
    [SerializeField] private GameObject _moveButton;
    [SerializeField] private GameObject _attackButton;
    [SerializeField] private GameObject _attackButtonPrefab;
    [SerializeField] private GameObject _cancelButton;
    [SerializeField] private GameObject _actionMenuAP;
    [SerializeField] private GameObject _generalMenu;
    [SerializeField] private GameObject _endTurnButton;
    [SerializeField] private GameObject _menuButton;
    [SerializeField] private GameObject _attackResult;
    private bool _isMenuOpen = false;
    private List<GameObject> _attackButtonList = new List<GameObject>();
    private BaseHero _previousSelectedHero;

    void Awake() {
        Instance = this;
    }
    void Start() {
        // Subscribe to the OnHeroSelected event
        if (UnitManager.Instance != null) {
            UnitManager.Instance.OnHeroSelected += ShowSelectedHero;
        }
        Button MenuButton = _menuButton.GetComponent<Button>();
        MenuButton.onClick.AddListener(() => ToggleMenu());

        Button EndTurnButton = _endTurnButton.GetComponent<Button>();
        EndTurnButton.onClick.AddListener(() => EndTurn());
    }
    void OnDestroy() {
        // Unsubscribe to prevent memory leaks
        if (UnitManager.Instance != null) {
            UnitManager.Instance.OnHeroSelected -= ShowSelectedHero;
        }
        if (_previousSelectedHero != null) {
            _previousSelectedHero.UnsubscribeFromAPChange(RefreshAP);
        }
    }
    public void ShowTileInfo(Tile tile) {
        if(tile == null){ 
            _tileObject.SetActive(false);
            _tileUnitObject.SetActive(false);
            _tileUnitStats.SetActive(false);
            _attackPreview.SetActive(false);
            return;
        }
        _tileObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.TileName;
        _tileObject.SetActive(true);

        if(tile.OccupiedUnit) {
            if(AttackManager.Instance.CurrentAttack == null) {
                _tileUnitObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.OccupiedUnit.UnitName;
                _tileUnitObject.SetActive(true);

                BaseUnit u = tile.OccupiedUnit;
                _tileUnitStats.GetComponentInChildren<TextMeshProUGUI>().text = 
                    $"AP: {u.CurrentAP} \n" +
                    $"Movement: {u.CurrentMovement} \n\n" +
                    $"Health: {u.CurrentHealth} \n" +
                    $"Psyche: {u.CurrentPsyche} \n\n\n" +
                    $"Strength: {u.CurrentStrength} \n" +
                    $"Ego: {u.CurrentEgo} \n" +
                    $"Grit: {u.CurrentGrit} \n" +
                    $"Resilience: {u.CurrentResilience} \n" +
                    $"Accuracy: {u.CurrentAccuracy} \n" +
                    $"Evasion: {u.CurrentEvasion}";
                _attackResult.SetActive(false);
                _tileUnitStats.SetActive(true);
            } else {
                if(tile.IsPotentialAttackNotNull) {
                    Debug.Log(tile);
                    ShowAttackPreview(AttackManager.Instance.CurrentAttack, tile.OccupiedUnit);
                }
            }
        }
    }
    public void ShowSelectedHero(BaseHero hero) {
        if (_previousSelectedHero != null) {
            _previousSelectedHero.UnsubscribeFromAPChange(RefreshAP);
            _previousSelectedHero.OnAPChanged -= OnHeroAPChanged;
        }
        if(hero == null) {
            _selectedHeroObject.SetActive(false);
            _actionMenu.SetActive(false);
            return;
        } else {
            hero.OnAPChanged += OnHeroAPChanged;
            _previousSelectedHero = hero;
        }

        _selectedHeroObject.GetComponentInChildren<TextMeshProUGUI>().text = hero.UnitName;
        _selectedHeroObject.SetActive(true);

        hero.SubscribeToAPChange(RefreshAP);
        _previousSelectedHero = hero;
    }
    public void ShowHeroActions(BaseHero hero) {
        if (GameManager.Instance.GameState != GameState.HeroesTurn) return;
        if (_isMenuOpen) {
            CloseMenu();
        }
        // First, remove any existing attack buttons
        RemoveHeroAttackButtons();

        // Turn menu object visible + put specific actions available to selected hero
        RefreshAP();
        _actionMenu.SetActive(true);

        Button MoveButton = _moveButton.GetComponent<Button>();
        MoveButton.onClick.AddListener(() => MoveClicked());

        // Enable the Move button only if the hero has at least 1 AP
        _moveButton.GetComponent<Button>().interactable = hero.CurrentAP >= 1;

        // Set up listeners for other buttons
        Button AttackButton = _attackButton.GetComponent<Button>();
        AttackButton.onClick.AddListener(() => ShowHeroAttacks(hero)); 

        Button CancelButton = _cancelButton.GetComponent<Button>();
        CancelButton.onClick.AddListener(() => CancelClicked());
    }
    private void OnHeroAPChanged() {
        if (UnitManager.Instance.SelectedHero != null) {
            _moveButton.GetComponent<Button>().interactable = UnitManager.Instance.SelectedHero.CurrentAP >= 1;
        }
    }
    public void ShowHeroAttacks(BaseHero hero) {
        if (GameManager.Instance.GameState != GameState.HeroesTurn) return;
        if(UnitManager.Instance.HeroMoving == true) {
            CancelClicked();
            return;
        }
        HideHeroActions();
        RemoveHeroAttackButtons(); // This will now destroy the old buttons
        int buttonHeight = 30;
        int index = 0;

        foreach (Attack attack in hero.AvailableAttacks) {
            // Instantiate a new button for each attack
            GameObject buttonObj = Instantiate(_attackButtonPrefab, _actionMenu.transform);
            Button button = buttonObj.GetComponent<Button>();
            
            // Create a new TextMeshProUGUI GameObject as a child of the button
            GameObject textObj = new GameObject("ButtonText");
            textObj.transform.SetParent(buttonObj.transform, false);

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = attack.Name;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontSize = 14;
            buttonText.color = Color.black;

            // Enable or disable the button based on the hero's available AP
            button.interactable = hero.CurrentAP >= attack.PublicCostAP;
            button.onClick.AddListener(() => attack.Target(hero, GridManager.Instance));

            // Adjust the button's position based on its index.
            RectTransform buttonRectTransform = buttonObj.GetComponent<RectTransform>();
            buttonRectTransform.anchoredPosition = new Vector2(0, (-index * buttonHeight * 1.3f) + 120);
            index++;

            // Add the button to the list of active buttons
            _attackButtonList.Add(buttonObj);
        }

        _cancelButton.SetActive(true);
    }
    private void OnAttackExecuted(object sender, AttackEventArgs e) {
        string resultText = $"Attack: {e.Attack.Name}\n" +
                            $"Success: {e.IsHit}\n" +
                            $"Damage Dealt: {e.DamageDealt}";
        _attackPreview.GetComponentInChildren<TextMeshProUGUI>().text = resultText;
        _attackPreview.SetActive(true);
    }
    public void RemoveHeroAttackButtons() {
        foreach (GameObject buttonObj in _attackButtonList) {
            Destroy(buttonObj);
        }
        _attackButtonList.Clear();
    }
    public void HideHeroActions() {
        _moveButton.SetActive(false);
        _attackButton.SetActive(false);
    }
    public void MoveClicked() {
        if (GameManager.Instance.GameState != GameState.HeroesTurn) return;
        GridManager.Instance.HighlightMoveOptions(UnitManager.Instance.SelectedHero.OccupiedTile, UnitManager.Instance.SelectedHero.CurrentMovement);
        UnitManager.Instance.HeroMoving = true;
        _cancelButton.SetActive(true);
    }
    public void CancelClicked() {
        AttackManager.Instance.ClearAttack();
        GridManager.Instance.ClearPotentialAttacks();
        GridManager.Instance.ClearPotentialMoves();
        UnitManager.Instance.HeroMoving = false;
        RemoveHeroAttackButtons();
        _cancelButton.SetActive(false);
        _attackButton.SetActive(true);
        _moveButton.SetActive(true);
    }
    public void ShowAttackPreview(Attack attack, BaseUnit target) {
        _attackResult.SetActive(false);
        string preview = $"{attack.Name}\nAP Cost: {attack.PublicCostAP}\n\n";
        //for now just looking at physical attack stats, will have to add property to attack to determine 
        //which offense and defense stats are used
        int noCrit = AttackManager.Instance.RollDamage(attack.PublicDamage, AttackManager.Instance.Attacker.CurrentStrength, target.CurrentGrit, 0, 1);
        int critDamage = AttackManager.Instance.RollDamage(attack.PublicDamage, AttackManager.Instance.Attacker.CurrentStrength, target.CurrentGrit, 100, 2);
        preview += $"{attack.PublicHitChance + AttackManager.Instance.Attacker.CurrentAccuracy - target.CurrentEvasion}% to hit\n\n";
        preview += $"Normal Damage: \n{noCrit}\n\n";
        preview += $"Crit Chance: \n{attack.PublicCritChance}%\n\n";
        preview += $"Crit Damage: \n{critDamage}";
        _attackPreview.GetComponentInChildren<TextMeshProUGUI>().text = preview;
        _attackPreview.SetActive(true);
    }
    public void RefreshAP() {
        if (UnitManager.Instance.SelectedHero != null) {
            TextMeshProUGUI remainingAP = _actionMenuAP.GetComponent<TextMeshProUGUI>();
            remainingAP.text = $"AP: {UnitManager.Instance.SelectedHero.CurrentAP}";
        }
    }
    private void ToggleMenu() {
        if (_isMenuOpen) {
            CloseMenu();
        } else {
            OpenMenu();
        }
    }
    private void OpenMenu() {
        if(GameManager.Instance.GameState != GameState.HeroesTurn) return;
        CancelClicked();
        _actionMenu.SetActive(false);
        _generalMenu.SetActive(true);
        _isMenuOpen = true;
    }
    private void CloseMenu() {
        _generalMenu.SetActive(false);
        _isMenuOpen = false; // Ensure consistency of the menu state
    }
    private void EndTurn() {
        if (GameManager.Instance.GameState != GameState.HeroesTurn) return;
        Debug.Log("Ending player turn");
        CloseMenu();
        TurnManager.Instance.EndHeroTurn();
    }
    public void ShowAttackResult(object sender, AttackEventArgs e) {
        _attackPreview.SetActive(false);
        string result = "";

        if (e.IsHit) {
            result += $"The {e.Attack.Name} was a hit!\n\n";
            // if (e.IsCritical) {
            //     result += "It was a critical hit!\n\n"; //event needs to send whether crit or not
            // }
            result += $"{e.Defender.UnitName} took {e.DamageDealt} damage.\n\n";
        } else {
            result += $"The {e.Attack.Name} was a miss.\n\n";
        }
        result += $"{e.Defender.UnitName} has {e.Defender.CurrentHealth} health remaining.";
        //Debug.Log(result);
        _attackResult.GetComponentInChildren<TextMeshProUGUI>().text = result;
        _attackResult.SetActive(true);
    }
}
