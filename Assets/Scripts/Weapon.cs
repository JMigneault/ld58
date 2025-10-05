using UnityEngine;

using UnityEngine;
using System;

public class Weapon : MonoBehaviour {

  public int _maxShots = 5;
  public int _shotsRemaining = 0;

  public float _timePerShot = 1.0f;
  private float _timeToNextShot; // Time until the next shot can be fired

  public float _restoreTime = 0.5f;
  private float _timeToNextRestore; // Time until the next shot is restored

  public Module _module;

  void Start() {
    _module = GetComponent<Module>();
    _shotsRemaining = _maxShots;
    _timeToNextShot = _timePerShot;
    _timeToNextRestore = _restoreTime;
    _module.SetBar(_shotsRemaining / (1.0f * _maxShots)); // Initial full bar
  }

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
    if (_module._powered) {
      if (_shotsRemaining <= 0) {
        _module.SetRecharging(true);
      }

      if (_module._recharging) {
        if (_shotsRemaining < _maxShots) {
          _timeToNextRestore -= Time.deltaTime;
          if (_timeToNextRestore <= 0) {
            _shotsRemaining++;
            _shotsRemaining = Mathf.Min(_shotsRemaining, _maxShots);
            _timeToNextRestore = _restoreTime;
            _module.SetBar(_shotsRemaining / (1.0f * _maxShots));
            Helpers.Log("shots remaining charged to: {0}", _shotsRemaining);
            if (_shotsRemaining == _maxShots) {
              _module.SetRecharging(false);
              _timeToNextShot = _timePerShot; // Reset shooting timer when fully recharged
            }
          }
        }
      } else { // Not recharging, ready to fire
        if (_shotsRemaining > 0) {
          _timeToNextShot -= Time.deltaTime;
          if (_timeToNextShot <= 0) {
            Shoot();
            _timeToNextShot = _timePerShot; // Reset shooting timer
          }
        }
      }
    } else {
      // If not powered, weapon is empty and not doing anything
      if (_shotsRemaining > 0) {
        _shotsRemaining = 0;
      }
      _module.SetBar(0.0f);
      if (_module._recharging) {
        _module.SetRecharging(false);
      }
    }
  }

  void Shoot() {
    Helpers.Log("pew");
    if (_shotsRemaining > 0) {
      if (_module._poweredBy != null) {
        Helpers.Log("pew pew");
        _module._poweredBy._battery.UseUnit();
        _shotsRemaining--;
        _shotsRemaining = Mathf.Max(0, _shotsRemaining); // Ensure shots don't go below zero
        _module.SetBar(_shotsRemaining / (1.0f * _maxShots));

        GameObject bulletPrefab = Helpers.Prefab("Bullet");
        if (bulletPrefab == null) {
          Helpers.Error("Bullet prefab not found!");
          return;
        }

        GameObject bulletGO = GameObject.Instantiate(bulletPrefab);
        bulletGO.GetComponent<Bullet>()._grid = _module._cell._grid;

        Vector3 protrusionVector = GetProtrusionVector(_module._protrusionDir);
        // Spawn slightly outside the module, adding a small buffer (0.1f)
        Vector3 spawnOffset = protrusionVector * (Module._moduleSize / 2f + 0.1f); 
        
        bulletGO.transform.position = _module.transform.position + spawnOffset;
        bulletGO.transform.rotation = GetProtrusionRotation(_module._protrusionDir);

        _timeToNextRestore = _restoreTime; // Reset restore timer after firing
      } else {
        Helpers.Log("Weapon cannot shoot: Not powered by a battery with units.");
        _module.SetRecharging(true); // Force recharging if tried to use when empty or unpowered
      }
    } else {
      Helpers.Log("Weapon cannot shoot: No shots remaining.");
      _module.SetRecharging(true);
    }
  }
  
}
