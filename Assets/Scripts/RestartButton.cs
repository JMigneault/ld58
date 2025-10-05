using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class RestartButton : MonoBehaviour {

  void OnMouseDown() {
    // This method is called when the mouse button is pressed while over the collider.
    // Ensure the GameObject has a BoxCollider2D component.
    Helpers.Log("Restarting scene...");
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
}
