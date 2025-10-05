using UnityEngine;
using DG.Tweening; // Import DOTween

public class FloatingModule : MonoBehaviour {
  private Module _module;

  public bool _floating = false;

  public int _slotIdx = -1;

  public bool startPlacingThisFrame = false;

  [Header("Floating Animation Settings")]
  [SerializeField] private float _floatAmplitude = 0.05f;
  [SerializeField] private float _floatSpeed = 1.5f;
  [SerializeField] private float _rotationSpeed = 5f;

  private Tween _floatTween;
  private Tween _rotateTween;
  private Vector3 _originalScale;

  private Vector3 _slotBasePosition;

  void Awake() {
    _originalScale = transform.localScale;

    _slotIdx = -1;

    _module = GetComponent<Module>();
    if (_module == null) {
      Helpers.Error("FloatingModule requires a Module component.");
      return;
    }
    
    EnableFloat(false);
  }

  void OnMouseDown() {
    if (!_floating) return;

    if (Placer.inst._currentModule == null) {
      Placer.inst.StopPlacing(); // release current mod
    }

    _module.SetScaled(false);
    EnableFloat(false);
    Placer.inst.StartPlacing(_module);
  }

  public void EnableFloat(bool enabled) {
    _floating = enabled;
    startPlacingThisFrame = true;
    if (enabled) {
      StartFloating();
    } else {
      StopFloatingAnimation();
    }
  }

  public void StartFloating() {
    StopFloatingAnimation(); // Ensure no previous tweens are running

    // Set module connector highlight to normal.
    foreach (dir d in System.Enum.GetValues(typeof(dir))) {
      _module.SetConnectorColor(d, true, false);
    }

    transform.parent = null;

    if (_slotIdx == -1) {
      _slotIdx = FloatingModuleTracker.inst.GrabEmptySlot(this); // Pass 'this' FloatingModule instance
    }
    // Recalculate _slotBasePosition. This is crucial if _slotIdx changed due to a reshuffle.
    Vector2 slotPos2D = FloatingModuleTracker.inst.SlotToPosition(_slotIdx);
    _slotBasePosition = new Vector3(slotPos2D.x, slotPos2D.y, Helpers._modZ);

    Sequence sequence = DOTween.Sequence();

    // First, tween to the base slot position if not already there.
    // This handles cases where the module might be returned from the grid.
    if (transform.position != _slotBasePosition) {
        sequence.Append(transform.DOMove(_slotBasePosition, 0.25f).SetEase(Ease.OutQuad));
    }

    // Then, append the continuous vertical floating animation around the _slotBasePosition.y
    sequence.Append(
        transform.DOMoveY(_slotBasePosition.y + _floatAmplitude, _floatSpeed)
                 .SetEase(Ease.InOutSine)
                 .SetLoops(-1, LoopType.Yoyo)
    );

    _floatTween = sequence; // Store the main sequence tween

    // Start the continuous rotation tween
    _rotateTween = transform.DORotate(new Vector3(0, 0, 360), _rotationSpeed, RotateMode.LocalAxisAdd)
                           .SetEase(Ease.Linear)
                           .SetLoops(-1, LoopType.Incremental)
                           .SetLink(gameObject); // Link to GameObject lifecycle for automatic killing
  }

  private void StopFloatingAnimation() {
    _floatTween.Kill();
    _rotateTween.Kill();
    transform.DORotateQuaternion(Quaternion.identity, 0.2f).SetEase(Ease.OutQuad); // Tween back to identity rotation
  }

  public void ReleaseSlot() {
    if (_slotIdx != -1) {
      FloatingModuleTracker.inst.ReleaseSlot(_slotIdx); // Pass current slot index for release
      _slotIdx = -1; // Clear internal slot index
    }

  }

  void OnMouseEnter() {
    _module.Hover();
  }

  void OnMouseExit() {
    _module.UnHover();
  }

  void LateUpdate() {
    startPlacingThisFrame = false;
  }

}
