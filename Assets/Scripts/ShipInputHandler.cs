using UnityEngine;
using DG.Tweening;

public class ShipInputHandler : MonoBehaviour {
  ShipTurner _turner;

  [SerializeField] private float _maxRotationSpeedPerEngine = 20f; // Max degrees per second

  void Awake() {
    _turner = GetComponent<ShipTurner>();
  }

  void Update() {
    HandleRotationInput();
  }

  void HandleRotationInput() {
    float targetVelocity = 0f;

    if (Input.GetKey(KeyCode.A)) {
      _turner.SetTargetVelocity(_maxRotationSpeedPerEngine * Grid._playersGrid._enginePower);
    } else if (Input.GetKey(KeyCode.D)) {
      _turner.SetTargetVelocity(-_maxRotationSpeedPerEngine * Grid._playersGrid._enginePower);
    } else {
      _turner.SetTargetVelocity(0);
    }
  }
}
