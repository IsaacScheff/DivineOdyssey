using UnityEngine;
using UnityEngine.UI;

// public class AttackButton : MonoBehaviour {
//     public Attack Attack;
// }

public class AttackButton : MonoBehaviour {
    public Attack Attack;
    private Attack _attack;
    private Button _button;

    void Awake() {
        _button = GetComponent<Button>();
    }

    public void Setup(Attack attack, BaseHero hero) {
        _attack = attack;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => _attack.Target(hero, GridManager.Instance));
        _button.interactable = hero.CurrentAP >= _attack.PublicCostAP;
    }
}


