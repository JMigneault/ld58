using UnityEngine;
using DG.Tweening;

public class ShipInputHandler : MonoBehaviour {
  ShipTurner _turner;

  [SerializeField] private float _baseRotationSpeed = 25f; // Max degrees per second
  [SerializeField] private float _maxRotationSpeedPerEngine = 10f; // Max degrees per second

  void Awake() {
    _turner = GetComponent<ShipTurner>();
  }

  void Update() {
    HandleRotationInput();
  }

  void HandleRotationInput() {
    float targetVelocity = 0f;

    if (Input.GetKey(KeyCode.A)) {
      _turner.SetTargetVelocity(_maxRotationSpeedPerEngine * Grid._playersGrid._enginePower + _baseRotationSpeed);
    } else if (Input.GetKey(KeyCode.D)) {
      _turner.SetTargetVelocity(-1 * (_maxRotationSpeedPerEngine * Grid._playersGrid._enginePower + _baseRotationSpeed));
    } else {
      _turner.SetTargetVelocity(0);
    }
  }
}
