using UnityEngine;
using DG.Tweening; // Import DOTween

public class FloatingModuleGenerator {
  public static FloatingModuleGenerator inst;

  public Module Generate() {
    Module generated = null;

    generated = Module.MakeModule(new ModuleSpec(ModuleType.Connection, new bool[4] { true, true, true, false }) );

    // Place it a bit off screen
    generated.transform.position = new Vector3(0, 25f, Helpers._modZ);

    return generated;
  }

  public void GenerateFloater() {
    if (!FloatingModuleTracker.inst.Available()) {
      Helpers.Log("float slot not available");
      return;
    }

    var m = Generate();

    m.GetComponent<FloatingModule>().EnableFloat(true);

  }


}
