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
    // Determine the local rotation for the protrusion direction
    Quaternion localRotation;
    switch (d) {
      case dir.U: localRotation = Quaternion.Euler(0, 0, 0); break;   // Up
      case dir.R: localRotation = Quaternion.Euler(0, 0, -90); break;  // Right (90 degrees clockwise)
      case dir.D: localRotation = Quaternion.Euler(0, 0, -180); break; // Down (180 degrees clockwise)
      case dir.L: localRotation = Quaternion.Euler(0, 0, 90); break;   // Left (90 degrees counter-clockwise)
      default: localRotation = Quaternion.identity; break;
    }
    // Combine with the module's current world rotation
    return _module.transform.rotation * localRotation;
  }

  void Update() {
    if (Placer.inst._paused)
      return;

    if (_module._powered) {
      if (_shotsRemaining <= 0) {
        _module.SetRecharging(true);
      }

      if (_module._recharging) {

        // massage the balance in the players favor
        if (_module._cell._grid._players) {
          _maxShots = 5;
          _restoreTime = 0.5f;
        } else {
          _maxShots = 2;
          _restoreTime = 3f;
        }

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
        // Transform the local protrusion vector by the module's world rotation to get the world-space offset direction.
        Vector3 rotatedProtrusionVector = _module.transform.rotation * protrusionVector;
        // Spawn slightly outside the module, adding a small buffer (0.1f)
        Vector3 spawnOffset = rotatedProtrusionVector * (Module._moduleSize / 2f + 0.1f); 
        
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
