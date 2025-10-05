using UnityEngine;
using DG.Tweening;

public class ShipInputHandler : MonoBehaviour {
  [Header("Rotation Settings")]
  [SerializeField] private float _maxRotationSpeed = 100f; // Max degrees per second
  [SerializeField] private float _rotationAccelerationTime = 0.5f; // Time to reach max speed
  [SerializeField] private float _rotationDecelerationTime = 0.3f; // Time to stop rotation

  private float _currentRotationVelocity = 0f; // Current rotation velocity in degrees per second
  private float _lastTargetVelocity = 0f; // Last target velocity applied to the tween
  private Tween _rotationTween;

  void Update() {
    HandleRotationInput();
    transform.Rotate(Vector3.forward, _currentRotationVelocity * Time.deltaTime);
  }

  void HandleRotationInput() {
    float targetVelocity = 0f;

    if (Input.GetKey(KeyCode.A)) {
      targetVelocity = _maxRotationSpeed;
    } else if (Input.GetKey(KeyCode.D)) {
      targetVelocity = -_maxRotationSpeed;
    }

    // Only create or update the tween if the target velocity has changed
    if (targetVelocity != _lastTargetVelocity) {
      _lastTargetVelocity = targetVelocity; // Update last target velocity

      // Kill existing tween to avoid conflicts
      _rotationTween?.Kill();

      if (targetVelocity != 0f) {
        // Accelerate to target velocity
        _rotationTween = DOTween.To(() => _currentRotationVelocity, x => _currentRotationVelocity = x, targetVelocity, _rotationAccelerationTime)
                                .SetEase(Ease.OutQuad)
                                .SetUpdate(UpdateType.Normal, true)
                                .SetLink(gameObject);
      } else {
        // Decelerate to zero velocity
        _rotationTween = DOTween.To(() => _currentRotationVelocity, x => _currentRotationVelocity = x, 0f, _rotationDecelerationTime)
                                .SetEase(Ease.OutCubic)
                                .SetUpdate(UpdateType.Normal, true)
                                .OnComplete(() => _currentRotationVelocity = 0f) // Ensure velocity is exactly zero at the end
                                .SetLink(gameObject);
      }
    }
    // If targetVelocity is 0 and current velocity is already 0, and no tween is active, ensure nothing is happening
    else if (targetVelocity == 0f && _currentRotationVelocity == 0f && (_rotationTween == null || !_rotationTween.IsActive())) {
      _rotationTween?.Kill(); // Just in case a lingering tween without an active state
    }
  }

  void OnDisable() {
    _rotationTween?.Kill(); // Kill tween if script is disabled
  }
}
