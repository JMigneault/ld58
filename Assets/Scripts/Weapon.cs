using UnityEngine;

public class Weapon : MonoBehaviour {

  public float _timePerShot = 2.0f;
  public float _timeToNextShot;
  public Module _module;

  void Start() {
    _module = GetComponent<Module>();
    SetTimeToNextShot();
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

  void Shoot() {
    GameObject bulletPrefab = Helpers.Prefab("Bullet");
    if (bulletPrefab == null) {
      Helpers.Error("Bullet prefab not found!");
      return;
    }

    GameObject bulletGO = GameObject.Instantiate(bulletPrefab);

    Vector3 protrusionVector = GetProtrusionVector(_module._protrusionDir);
    // Spawn slightly outside the module, adding a small buffer (0.1f)
    Vector3 spawnOffset = protrusionVector * (Module._moduleSize / 2f + 0.1f); 
    
    bulletGO.transform.position = _module.transform.position + spawnOffset;
    bulletGO.transform.rotation = GetProtrusionRotation(_module._protrusionDir);
  }
  
}
