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

    // Call the BadGuyController's update method
    BadGuyController.inst.GameUpdate();
  }

  void SetTimeToNextFloater() {
    _timeToNextFloater = Random.value * 5f + 15f; // 5-15
  }

  void SetTimeToNextEnemy() {
    _timeToNextEnemy = 5f; // Random.value * 15f + 15f; // Example: 15-30 secs for enemies
  }

}
