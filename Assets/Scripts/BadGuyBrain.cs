using UnityEngine;
using System.Collections.Generic;

public class BadGuyBrain : MonoBehaviour {
  [SerializeField] private float _turnSpeed = 25f; // Speed at which the enemy attempts to turn in degrees per second

  private ShipTurner _shipTurner;
  private float _targetAcquisitionInterval = 2.0f; // How often to pick a new target module
  private Vector3 _targetPosition;

  void Awake() {
    _shipTurner = GetComponent<ShipTurner>();
    if (_shipTurner == null) {
      Helpers.Error("BadGuyBrain: ShipTurner component not found on this GameObject.");
      enabled = false; // Disable this brain if no turner is present
      return;
    }
  }

  void Start() {
    Vector3 target = AcquireNewTarget();
  }

  void Update() {
    if (Grid._playersGrid == null || Grid._playersGrid._parent == null) {
      // Player ship destroyed, stop turning
      _shipTurner.SetTargetVelocity(0f);
      return;
    }

    // Calculate direction to the target module
    Vector3 directionToTarget = _targetPosition - transform.position;
    directionToTarget.z = 0; // Ignore Z-axis for 2D rotation

    // Calculate the angle to the target
    float angle = Vector2.SignedAngle(-1 * transform.right, directionToTarget);

    // Set target velocity for ShipTurner
    float targetVelocity = 0f;
    if (Mathf.Abs(angle) > 5f) { // Only turn if angle is significant
      if (angle > 0) {
        targetVelocity = _turnSpeed; // Turn left (positive z-rotation)
      } else {
        targetVelocity = -_turnSpeed; // Turn right (negative z-rotation)
      }
    }
    _shipTurner.SetTargetVelocity(targetVelocity);
  }

  Vector3 AcquireNewTarget() {
    Module currentTargetModule = null;

    List<Module> playerModules = new List<Module>();
    for (int x = 0; x < Grid._playersGrid._dimX; x++) {
      for (int y = 0; y < Grid._playersGrid._dimY; y++) {
        Module module = Grid._playersGrid.GetModule(new Coord(x, y));
        if (module != null) {
          playerModules.Add(module);
        }
      }
    }

    if (playerModules.Count > 0) {
      currentTargetModule = playerModules[Random.Range(0, playerModules.Count)];
    } else {
      currentTargetModule = null;
    }

    return currentTargetModule == null ? new Vector3(0, 0, 0) : currentTargetModule.transform.position;
  }

  void OnDisable() {
    if (_shipTurner != null) {
      _shipTurner.SetTargetVelocity(0f); // Ensure the ship stops turning when brain is disabled
    }
  }
}
