
using UnityEngine;

using UnityEngine;
using System;

public class Battery : MonoBehaviour {
  public int _maxUnits = 10;
  public int _currentUnits = 0;

  public float _restoreTime = 0.25f;
  private float _timeToNextRestore;
  public Module _module;

  void Start() {
    _module = GetComponent<Module>();
    _timeToNextRestore = _restoreTime;
  }

  void Update() {
    if (Placer.inst._paused)
      return;

    if (_module._cell != null) { // placed
      if (_currentUnits <= 0)
        _module.SetRecharging(true);

      Helpers.Log("Updating placed batter. recharging: {0} cu: {1}", _module._recharging, _currentUnits);

      if (_module._recharging && _currentUnits < _maxUnits) {
        _timeToNextRestore -= Time.deltaTime;
        if (_timeToNextRestore <= 0) {
          _currentUnits++;
          _currentUnits = Mathf.Min(_currentUnits, _maxUnits);
          _timeToNextRestore = _restoreTime; // Reset timer for next unit or full
          _module.SetBar(_currentUnits / (1.0f * _maxUnits));
          if (_currentUnits == _maxUnits) {
            _module.SetRecharging(false);
          }
        }
      }
    }
  }

  public void UseUnit(int amount = 1) {
    if (_currentUnits >= amount) {
      _currentUnits -= amount;
      _currentUnits = Mathf.Max(0, _currentUnits); // Ensure units don't go below zero
      _module.SetBar(_currentUnits / (1.0f * _maxUnits));
      if (_currentUnits == 0) {
        _module.SetRecharging(true);
      }
      _timeToNextRestore = _restoreTime; // Reset timer upon using a unit
    } else {
      Helpers.Log("Attempted to use battery unit when none available.");
      _module.SetRecharging(true); // Force recharging if tried to use when empty
    }
  }
}
