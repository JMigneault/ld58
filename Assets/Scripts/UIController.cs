using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour {
  
  public static UIController inst;

  bool _lockTT = false;

  public GameObject _enginePower;
  public GameObject _scoreModules;
  public GameObject _scoreEnemies;
  public GameObject _tooltip;

  public int _modulesCollected = -1; // offset initial core

  void Awake() {
    inst = this;
    SetTooltip("");
  }

  public void SetTooltip(string text) {
    if (_lockTT) return;
    if (text == "") {
      // Default text
      text = "LClick floating modules\nRClick to rotate selected module\nA and D to rotate ship";
    }
    if (_tooltip != null) {
      TMP_Text tooltipText = _tooltip.GetComponentInChildren<TMP_Text>();
      if (tooltipText != null) {
        tooltipText.text = text;
      } else {
        Helpers.Error("TMP_Text component not found in _tooltip children.");
      }
    }
  }

  public void SetEngineStrength(int strength) {
    if (_enginePower != null) {
      TMP_Text engineText = _enginePower.GetComponentInChildren<TMP_Text>();
      if (engineText != null) {
        engineText.text = $"Engine\nStrength:\n{strength}";
      } else {
        Helpers.Error("TMP_Text component not found in _enginePower children.");
      }
    }
  }

  public void IncrModulesCollected() {
    _modulesCollected++;
    if (_scoreModules != null) {
      TMP_Text modulesText = _scoreModules.GetComponentInChildren<TMP_Text>();
      if (modulesText != null) {
        modulesText.text = $"Modules\nCollected:\n{_modulesCollected}";
      } else {
        Helpers.Error("TMP_Text component not found in _scoreModules children.");
      }
    }
  }

  public void SetEnemiesDestroyed(int enemiesDestroyed) {
    if (_scoreEnemies != null) {
      TMP_Text enemiesText = _scoreEnemies.GetComponentInChildren<TMP_Text>();
      if (enemiesText != null) {
        enemiesText.text = $"Enemies\nDestroyed:\n{enemiesDestroyed}";
      } else {
        Helpers.Error("TMP_Text component not found in _scoreEnemies children.");
      }
    }
  }

  public void GameOver() {
    SetTooltip("GAME OVER");
    _lockTT = true;
  }

}
