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
    private List<GameObject> _attackButtonList = new List<GameObject>();
    private Queue<GameObject> _attackButtonPool = new Queue<GameObject>();
    private int _poolSize = 6; 
    private BaseHero _previousSelectedHero;

    void Awake() {
        Instance = this;
        InitializeAttackButtonPool();
    }

    private void InitializeAttackButtonPool() {
        for (int i = 0; i < _poolSize; i++) {
            GameObject buttonObj = Instantiate(_attackButtonPrefab);
            Button button = buttonObj.GetComponent<Button>();
            if (button != null) {
                button.interactable = false;
            }
            buttonObj.SetActive(false);
            _attackButtonPool.Enqueue(buttonObj);
        }
    }


    void Start() {
        // Subscribe to the OnHeroSelected event
        if (UnitManager.Instance != null) {
            UnitManager.Instance.OnHeroSelected += ShowSelectedHero;
        }
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
                _tileUnitStats.SetActive(true);
            } else {
                ShowAttackPreview(AttackManager.Instance.CurrentAttack, tile.OccupiedUnit);
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
        // Turn menu object visible + put specific actions available to selected hero
        RefreshAP();
        _actionMenu.SetActive(true);

        Button MoveButton = _moveButton.GetComponent<Button>();
        MoveButton.onClick.AddListener(() => MoveClicked(hero));

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
        HideHeroActions();
        RemoveHeroAttackButtons();
        int buttonHeight = 30;
        int index = 0;

        foreach (Attack attack in hero.AvailableAttacks) {
            GameObject buttonObj = GetAttackButtonFromPool(); 
            Button button = buttonObj.GetComponent<Button>();

            // Store the Attack instance in the button GameObject
            buttonObj.GetComponent<AttackButton>().Attack = attack;
            attack.AttackExecuted += OnAttackExecuted;

            // Set up the text for the button
            GameObject textObj = new GameObject($"{attack}Button");
            textObj.transform.SetParent(buttonObj.transform, false);

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = attack.Name;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.fontSize = 14;
            buttonText.color = Color.black;

            RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.sizeDelta = new Vector2(0, 0);

            // Enable or disable the button based on the hero's available AP
            button.interactable = hero.CurrentAP >= attack.PublicCostAP;
            button.onClick.AddListener(() => attack.Target(hero, GridManager.Instance));

            // Adjust the button's position based on its index.
            RectTransform buttonRectTransform = buttonObj.GetComponent<RectTransform>();
            buttonRectTransform.anchoredPosition = new Vector2(0, (-index * buttonHeight * 1.3f) + 120);
            index++;

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
            AttackButton attackButtonComponent = buttonObj.GetComponent<AttackButton>();
            if (attackButtonComponent != null && attackButtonComponent.Attack != null) {
                attackButtonComponent.Attack.AttackExecuted -= OnAttackExecuted;
            }
            
            ReturnAttackButtonToPool(buttonObj);
        }
        _attackButtonList.Clear();
    }

    public GameObject GetAttackButtonFromPool() {
        GameObject buttonObj = _attackButtonPool.Count > 0 ? _attackButtonPool.Dequeue() : Instantiate(_attackButtonPrefab);
        buttonObj.transform.SetParent(_actionMenu.transform, false);
        Button button = buttonObj.GetComponent<Button>();
        if (button != null) {
            button.interactable = false;
        }
        buttonObj.SetActive(true);
        return buttonObj;
    }


    public void ReturnAttackButtonToPool(GameObject button) {
        button.SetActive(false);
        _attackButtonPool.Enqueue(button);
    }

    public void HideHeroActions() {
        _moveButton.SetActive(false);
        _attackButton.SetActive(false);
    }

    public void MoveClicked(BaseHero hero) {
        GridManager.Instance.HighlightMoveOptions(hero.OccupiedTile, hero.CurrentMovement);
        UnitManager.Instance.HeroMoving = true;
        _cancelButton.SetActive(true);
    }

    public void CancelClicked() {
        AttackManager.Instance.ClearAttack();
        GridManager.Instance.ClearPotentialAttacks();
        GridManager.Instance.ClearPotentialMoves();
        RemoveHeroAttackButtons();
        _cancelButton.SetActive(false);
        _attackButton.SetActive(true);
        _moveButton.SetActive(true);
    }

    public void ShowAttackPreview(Attack attack, BaseUnit target) {
        string preview = $"{attack.Name}\nAP Cost: {attack.PublicCostAP}\n\n";
        //for now just looking at physical attack stats, will have to add property to attack to determine 
        //which offense and defense stats are used
        int noCrit = AttackManager.Instance.RollDamage(attack.PublicDamage, AttackManager.Instance.Attacker.CurrentStrength, target.CurrentGrit, 0, 1);
        int critDamage = AttackManager.Instance.RollDamage(attack.PublicDamage, AttackManager.Instance.Attacker.CurrentStrength, target.CurrentGrit, 100, 2);
        preview += $"{attack.PublicHitChance}% to hit\n\n";
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

}
