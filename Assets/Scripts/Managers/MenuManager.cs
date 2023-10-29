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
    [SerializeField] private GameObject _actionMenu;
    [SerializeField] private GameObject _moveButton;
    [SerializeField] private GameObject _attackButton;
    [SerializeField] private GameObject _attackButtonPrefab;


    void Awake() {
        Instance = this;
    }

    public void ShowTileInfo(Tile tile) {
        if(tile == null){ 
            _tileObject.SetActive(false);
            _tileUnitObject.SetActive(false);
            _tileUnitStats.SetActive(false);
            return;
        }
        _tileObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.TileName;
        _tileObject.SetActive(true);

        if(tile.OccupiedUnit) {
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
        _actionMenu.SetActive(true);
        Button MoveButton = _moveButton.GetComponent<Button>();
        MoveButton.onClick.AddListener(() => UnitManager.Instance.ShowMoves(hero.OccupiedTile, 5)); 

        Button AttackButton = _attackButton.GetComponent<Button>();
        AttackButton.onClick.AddListener(() => ShowHeroAttacks(hero)); 
    }

    public void ShowHeroAttacks(BaseHero hero) {
        HideHeroActions();
        int buttonHeight = 30;
        int index = 0;
        foreach (string attack in hero.AvailableAttacks) {
            GameObject buttonObj = Object.Instantiate(_attackButtonPrefab, _actionMenu.transform);
            
            Button button = buttonObj.GetComponent<Button>();

            GameObject textObj = new GameObject($"{attack}Button");
            textObj.transform.SetParent(buttonObj.transform, false);

            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = attack;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            buttonText.fontSize = 14;
            buttonText.color = Color.black;

            RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = new Vector2(0, 0);
            textRectTransform.anchorMax = new Vector2(1, 1);
            textRectTransform.sizeDelta = new Vector2(0, 0);

            button.onClick.AddListener(() => {
                Debug.Log(attack + " button was clicked!");
            });

            // Adjust the button's position based on its index.
            RectTransform buttonRectTransform = buttonObj.GetComponent<RectTransform>();
            buttonRectTransform.anchoredPosition = new Vector2(0, -index * buttonHeight);
            index++;
        }
    }

    public void HideHeroActions() {
        _moveButton.SetActive(false);
        _attackButton.SetActive(false);
    }

}
