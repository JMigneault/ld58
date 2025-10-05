using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour {

  public Grid _grid;

  [SerializeField] private float _speed = 10.0f;
  [SerializeField] private float _homingStrength = 0.1f; // How quickly the bullet turns towards the target
  [SerializeField] private float _homingTolerance = 0.7f;

  void Start() {
    // Bullets delete themselves after 5 seconds
    Destroy(gameObject, 5.0f);
  }

  void Update() {
    if (Placer.inst._paused)
      return;

    // Move forward based on current rotation
    transform.Translate(Vector3.up * _speed * Time.deltaTime);

    // Homing logic
    Module closestModule = FindClosestTargetModule();
    if (closestModule != null) {
      // Calculate direction to the target
      Vector3 directionToTarget = closestModule.transform.position - transform.position;

      // Make sure we're going vaguely in the right direction.
      if (Vector3.Dot(directionToTarget.normalized, transform.up) >= _homingTolerance) {
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _homingStrength * Time.deltaTime);
      }
    }
  }

  // Finds the closest module on a target grid (player for enemy bullets, enemies for player bullets)
  private Module FindClosestTargetModule() {
    Grid playerGrid = Grid._playersGrid;
    List<Grid> targetGrids = new List<Grid>();

    // Determine target grids based on who fired the bullet
    if (_grid == playerGrid) {
      // Bullet from player, target bad guys
      targetGrids.AddRange(BadGuyController.inst._badGuys);
    } else {
      // Bullet from enemy, target player
      targetGrids.Add(playerGrid);
    }

    Module closestTargetModule = null;
    float minDistance = float.MaxValue;

    foreach (Grid targetGrid in targetGrids) {
      if (targetGrid == null || targetGrid._parent == null) continue; // Skip if grid or parent is null (e.g., destroyed ship)

      for (int x = 0; x < targetGrid._dimX; x++) {
        for (int y = 0; y < targetGrid._dimY; y++) {
          Module module = targetGrid.GetModule(new Coord(x, y));
          if (module != null) {
            float distance = Vector3.Distance(transform.position, module.transform.position);
            if (distance < minDistance) {
              minDistance = distance;
              closestTargetModule = module;
            }
          }
        }
      }
    }
    return closestTargetModule;
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
