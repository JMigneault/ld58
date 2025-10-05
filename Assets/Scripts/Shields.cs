
using UnityEngine;

using UnityEngine;
using System;

public class Shields : MonoBehaviour {

  public int _maxHits = 2;
  public int _hitsRemaining = 0;

  public float _restoreTime = 1f;
  private float _timeToNextRestore;
  public Module _module;

  void Start() {
    _module = GetComponent<Module>();
    _timeToNextRestore = _restoreTime;
    _module.SetBar(1.0f); // Initial full bar
  }

  void Update() {
    if (Placer.inst._paused)
      return;

    if (_module._powered) {
      if (_hitsRemaining <= 0)
        _module.SetRecharging(true);
      // Only recharge if the module is marked as recharging
      if (_module._recharging && _hitsRemaining < _maxHits) {
        _timeToNextRestore -= Time.deltaTime;
        if (_timeToNextRestore <= 0) {
          _hitsRemaining++;
          _hitsRemaining = Mathf.Min(_hitsRemaining, _maxHits);
          _timeToNextRestore = _restoreTime; // Reset timer for next hit or full
          _module.SetBar(_hitsRemaining / (1.0f * _maxHits));
          if (_hitsRemaining == _maxHits) {
            _module.SetRecharging(false);
          }
        }
      }
    } else {
      // If not powered, shields are down and not recharging
      if (_hitsRemaining > 0) {
        _hitsRemaining = 0;
      }
      _module.SetBar(0.0f);
      if (_module._recharging) {
        _module.SetRecharging(false);
      }
    }
  }

  public void TakeHit() {
    if (_hitsRemaining > 0) {
      if (_module._poweredBy != null) {
        _module._poweredBy._battery.UseUnit(); // Consume unit from battery
        _hitsRemaining--;
        _hitsRemaining = Mathf.Max(0, _hitsRemaining); // Ensure hits don't go below zero
        _module.SetBar(_hitsRemaining / (1.0f * _maxHits));
        if (_hitsRemaining == 0) {
          _module.SetRecharging(true);
        }
        _timeToNextRestore = _restoreTime; // Reset timer upon taking a hit
      } else {
        Helpers.Log("Shield cannot absorb hit: Not powered by a battery or battery depleted.");
        _module.SetRecharging(true); // Force recharging if unpowered by battery or battery component missing/depleted
      }
    } else {
      Helpers.Log("Shield cannot absorb hit: No hits remaining.");
      _module.SetRecharging(true); // Force recharging if no hits left
    }
  }
}
