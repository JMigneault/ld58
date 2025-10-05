
using UnityEngine;

public class Battery : MonoBehaviour {
  public int _maxUnits = 10;
  int _currentUnits = 0;

  public float _restoreTime = 5f;
  public Module _module;

  void Start() {
    _module = GetComponent<Module>();
    _currentUnits = _maxUnits;
  }

  public void UseUnit() {
    _currentUnits--;
    // XXX TODO: set bar UI
    if (_currentUnits <= 0) {
      _module.SetRecharging(true);
      // TODO: regenerate
    }
  }

/*

  private Vector3 GetProtrusionVector(dir d) {
    switch (d) {
      case dir.U: return new Vector3(0, 1, 0);
      case dir.R: return new Vector3(1, 0, 0);
      case dir.D: return new Vector3(0, -1, 0);
      case dir.L: return new Vector3(-1, 0, 0);
      default: return Vector3.zero;
    }
  }

  private Quaternion GetProtrusionRotation(dir d) {
    switch (d) {
      case dir.U: return Quaternion.Euler(0, 0, 0);   // Up
      case dir.R: return Quaternion.Euler(0, 0, -90);  // Right (90 degrees clockwise)
      case dir.D: return Quaternion.Euler(0, 0, -180); // Down (180 degrees clockwise)
      case dir.L: return Quaternion.Euler(0, 0, 90);   // Left (90 degrees counter-clockwise)
      default: return Quaternion.identity;
    }
  }

  void Update() {
    if (_module._cell != null && _module._powered) { // we're placed
      _module._uiGenericBar.SetActive(true);
      _timeToNextShot -= Time.deltaTime;
      if (_timeToNextShot < 0) {
        Shoot();
        SetTimeToNextShot();
      }
      _module.SetBar((_timePerShot - _timeToNextShot) / _timePerShot);
    } else {
      _module.SetBar(0f);
    }
  }

  void SetTimeToNextShot() {
    _timeToNextShot = _timePerShot;
  }
  */
  
}
