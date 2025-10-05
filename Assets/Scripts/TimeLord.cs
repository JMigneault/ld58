using UnityEngine;
using DG.Tweening; // Import DOTween

public class TimeLord : MonoBehaviour {
  public static TimeLord inst;

  public bool _started = false;

  public float _timeToNextFloater;

  void Start() {
    inst = this;
    SetTimeToNextFloater();
  }

  void Update() {
    if (!_started) return;

    _timeToNextFloater -= Time.deltaTime;
    if (_timeToNextFloater < 0) {
      FloatingModuleGenerator.inst.GenerateFloater();
      SetTimeToNextFloater();
    }

    // Call the BadGuyController's update method
    BadGuyController.inst.GameUpdate();
  }

  void SetTimeToNextFloater() {
    _timeToNextFloater = Random.value * 5f + 15f; // 5-15
  }

}
