using UnityEngine;
using DG.Tweening; // Import DOTween

public class TimeLord : MonoBehaviour {

  public float _timeToNextFloater;
  public float _timeToNextEnemy;

  void Start() {
    SetTimeToNextFloater();
    SetTimeToNextEnemy();
  }

  void Update() {
    _timeToNextFloater -= Time.deltaTime;
    if (_timeToNextFloater < 0) {
      FloatingModuleGenerator.inst.GenerateFloater();
      SetTimeToNextFloater();
    }

    _timeToNextEnemy -= Time.deltaTime;
    if (_timeToNextEnemy < 0) {
      BadGuyController.inst.Spawn();
      SetTimeToNextEnemy();
    }
  }

  void SetTimeToNextFloater() {
    _timeToNextFloater = 1f; // XXX TODO Random.value * 10f + 10f; // 10-20 secs
  }

  void SetTimeToNextEnemy() {
    _timeToNextEnemy = 5f; // Random.value * 15f + 15f; // Example: 15-30 secs for enemies
  }

}
