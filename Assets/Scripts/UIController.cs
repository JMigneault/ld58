using UnityEngine;

public class UIController {
  
  public static UIController inst;

  public GameObject _enginePower;
  public GameObject _score;
  public GameObject _tooltip;

  public UIController() {
    inst = this;
  }

  public void SetEnginePower(int power) {
    int maxPower = 5;
    float prop = (1.0f * power) / maxPower;

    // TODO XXX
  }

  public void SetTooltip(string text) {
    // TODO XXX
  }

}
