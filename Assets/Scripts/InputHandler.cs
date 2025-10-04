using UnityEngine;

public class InputHandler : MonoBehaviour {

  void Update() {
    Placer.inst.Hover(Input.mousePosition);
    if (Input.GetMouseButtonDown(0)) {
      Placer.inst.TryPlacing(Input.mousePosition);
    } else if (Input.GetMouseButtonDown(1)) {
      Placer.inst.RotateCurrent();
    }

    if (Input.GetKeyDown(KeyCode.Escape))
      Placer.inst.StopPlacing();

  }

}
