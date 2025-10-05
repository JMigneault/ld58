using UnityEngine;

public class Bullet : MonoBehaviour {

  public Grid _grid;

  [SerializeField] private float _speed = 10.0f;

  void Update() {
    transform.Translate(Vector3.up * _speed * Time.deltaTime);
  }

  void OnTriggerEnter2D(Collider2D other) {
    Helpers.Log("TriggerEnter2D");
    Module hitModule = other.GetComponent<Module>();
    if (hitModule != null) {
      if (hitModule._cell != null && hitModule._cell._grid != _grid) { // Only damage modules that are placed on a different grid
        hitModule.Damage();
        Destroy(gameObject); // Destroy the bullet after hitting a module
      }
    }
  }

}
