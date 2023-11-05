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



    void Awake() {
        Instance = this;
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
        if(hero == null){ 
            _selectedHeroObject.SetActive(false);
            _actionMenu.SetActive(false);
            return;
        }
        _selectedHeroObject.GetComponentInChildren<TextMeshProUGUI>().text = hero.UnitName;
        _selectedHeroObject.SetActive(true);
    }

    public void ShowHeroActions(BaseHero hero) {
        //turn menu object visible + put specific actions available to selected hero
        RefreshAP(hero);
        _actionMenu.SetActive(true);
        Button MoveButton = _moveButton.GetComponent<Button>();
        MoveButton.onClick.AddListener(() => MoveClicked(hero));

        Button AttackButton = _attackButton.GetComponent<Button>();
        AttackButton.onClick.AddListener(() => ShowHeroAttacks(hero)); 

        Button CancelButton = _cancelButton.GetComponent<Button>();
        CancelButton.onClick.AddListener(() => CancelClicked());
    }

    public void ShowHeroAttacks(BaseHero hero) {
        HideHeroActions();
        int buttonHeight = 30;
        int index = 0;
        foreach (Attack attack in hero.AvailableAttacks) {
            GameObject buttonObj = Object.Instantiate(_attackButtonPrefab, _actionMenu.transform);
            
            Button button = buttonObj.GetComponent<Button>();

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

            button.onClick.AddListener(() => attack.Target(hero));
            // Adjust the button's position based on its index.
            RectTransform buttonRectTransform = buttonObj.GetComponent<RectTransform>();
            buttonRectTransform.anchoredPosition = new Vector2(0, (-index * buttonHeight * 1.3f) + 120);
            index++;

            _attackButtonList.Add(buttonObj);
        }
        _cancelButton.SetActive(true);
    }

    public void RemoveHeroAttackButtons() {
        foreach (GameObject buttonObj in  _attackButtonList) {
            GameObject.Destroy(buttonObj);
        }
        _attackButtonList.Clear();
    }

    public void HideHeroActions() {
        _moveButton.SetActive(false);
        _attackButton.SetActive(false);
    }

    public void MoveClicked(BaseHero hero) {
        UnitManager.Instance.ShowMoves(hero.OccupiedTile, hero.CurrentMovement);
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
        string preview = $"{attack.Name}\n\n";
        //for now just looking at physical attack stats, will have to add property to attack to determine 
        //which offense and defense stats are used
        int noCrit = AttackManager.Instance.RollDamage(attack.damage, AttackManager.Instance.Attacker.CurrentStrength, target.CurrentGrit, 0, 1);
        int critDamage = AttackManager.Instance.RollDamage(attack.damage, AttackManager.Instance.Attacker.CurrentStrength, target.CurrentGrit, 100, 2);
        preview += $"{attack.hitChance}% to hit\n\n";
        preview += $"Normal Damage: \n{noCrit}\n\n";
        preview += $"Crit Chance: \n{attack.critChance}%\n\n";
        preview += $"Crit Damage: \n{critDamage}";
        _attackPreview.GetComponentInChildren<TextMeshProUGUI>().text = preview;
        _attackPreview.SetActive(true);
    }

    public void RefreshAP(BaseHero hero) {
        TextMeshProUGUI remainingAP = _actionMenuAP.GetComponent<TextMeshProUGUI>();
        remainingAP.text = $"AP: {hero.CurrentAP}";
    }

}
