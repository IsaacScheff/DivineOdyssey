using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance;

    [SerializeField] private GameObject _selectedHeroObject, _tileObject, _tileUnitObject, _actionMenu, _moveButton;

    void Awake() {
        Instance = this;
    }

    public void ShowTileInfo(Tile tile) {
        if(tile == null){ 
            _tileObject.SetActive(false);
            _tileUnitObject.SetActive(false);
            return;
        }
        _tileObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.TileName;
        _tileObject.SetActive(true);

        if(tile.OccupiedUnit) {
            _tileUnitObject.GetComponentInChildren<TextMeshProUGUI>().text = tile.OccupiedUnit.UnitName;
            _tileUnitObject.SetActive(true);
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
        MoveButton.onClick.AddListener(() => UnitManager.Instance.MoveHero(hero));
        //btn.onClick.AddListener(() => TaskOnClickWithArguments("Hello", 42));
    }

}