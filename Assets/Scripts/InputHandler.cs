using UnityEngine;

public class InputHandler : MonoBehaviour {

  void Update() {
    if (Input.GetKeyDown(KeyCode.P)) { // TODO: XXX for testing
      if (Placer.inst._currentModule == null) {
        Placer.inst.StartPlacing(Module.MakeModule(ModuleType.Connection));
      } else {
        Placer.inst.StopPlacing();
      }
    }

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
