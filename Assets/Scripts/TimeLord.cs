using UnityEngine;
using DG.Tweening; // Import DOTween

public class TimeLord : MonoBehaviour {

  float _timeToNextFloater;

  void Start() {
    SetTimeToNextFloater();
  }

  void Update() {
    if (_timeToNextFloater < 0) {
      FloatingModuleGenerator.inst.GenerateFloater();
      SetTimeToNextFloater();
    }
  }

  void SetTimeToNextFloater() {
    _timeToNextFloater = Random.value * 10f + 10f; // 10-20 secs
  }

}
