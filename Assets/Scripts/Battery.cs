
using UnityEngine;

using UnityEngine;
using System;

public class Battery : MonoBehaviour {
  public int _maxUnits = 10;
  public int _currentUnits = 0;

  public float _restoreTime = 5f;
  private float _timeToNextRestore;
  public Module _module;

  void Start() {
    _module = GetComponent<Module>();
    _currentUnits = _maxUnits;
    _timeToNextRestore = _restoreTime;
    _module.SetBar(1.0f); // Initial full bar
  }

  void Update() {
    if (_module._powered) {
      if (_currentUnits <= 0)
        _module.SetRecharging(true);

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
    } else {
      // If not powered, battery is empty and not recharging
      if (_currentUnits > 0) {
        _currentUnits = 0;
      }
      _module.SetBar(0.0f);
      if (_module._recharging) {
        _module.SetRecharging(false);
      }
    }
  }

  public bool HasUnits(int amount = 1) {
    return _currentUnits >= amount;
  }

  public void UseUnit(int amount = 1) {
    if (HasUnits(amount)) {
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
