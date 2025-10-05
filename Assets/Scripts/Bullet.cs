using UnityEngine;

public class Bullet : MonoBehaviour {

  [SerializeField] private float _speed = 10.0f; // AI: Bullet speed

  void Update() {
    transform.Translate(Vector3.up * _speed * Time.deltaTime);
  }

  void OnTriggerEnter2D(Collider2D other) {
    Module hitModule = other.GetComponent<Module>();
    if (hitModule != null) {
      if (hitModule._cell != null) { // Only damage modules that are placed on the grid
        hitModule.Damage();
      }
      Destroy(gameObject); // Destroy the bullet after hitting a module
    }
  }

}
