using UnityEngine;
using DG.Tweening;

public class ShipInputHandler : MonoBehaviour {
  ShipTurner _turner;

  [SerializeField] private float _maxRotationSpeed = 100f; // Max degrees per second

  void Awake() {
    _turner = GetComponent<ShipTurner>();
  }

  void Update() {
    HandleRotationInput();
  }

  void HandleRotationInput() {
    float targetVelocity = 0f;

    if (Input.GetKey(KeyCode.A)) {
      _turner.SetTargetVelocity(_maxRotationSpeed);
    } else if (Input.GetKey(KeyCode.D)) {
      _turner.SetTargetVelocity(-_maxRotationSpeed);
    } else {
      _turner.SetTargetVelocity(0);
    }
  }
}
