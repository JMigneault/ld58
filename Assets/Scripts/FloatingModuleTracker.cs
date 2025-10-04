using UnityEngine;

class FloatingModuleTracker {
  public static FloatingModuleTracker inst;
  int numSlots = 3;
  bool[] slots; // whether each slot is filled

  float vertOff = 4f;
  float sep = 2f;

  public FloatingModuleTracker() {
    inst = this;
    slots = new bool[numSlots];
  }

  public int GrabEmptySlot() {
    for (int i = 0; i < numSlots; i++) {
      if (!slots[i]) {
        slots[i] = true;
        return i;
      }
    }
    return -1; // Should not be reached if an error is thrown
  }

  public void ReleaseSlot(int i) {
    if (i >= 0 && i < numSlots) {
      slots[i] = false;
    } else {
      Helpers.Error("Attempted to release an invalid slot index: {0}", i);
    }
  }

  public Vector2 SlotToPosition(int slot) {
    float y = 0;
    if (slot < 3) { // above ship
      y = vertOff;
    } else {
      y = -1 * vertOff;
    }

    float x = 0;
    if (slot == 0 || slot == 3)
      x -= sep;
    if (slot == 2 || slot == 5)
      x += sep;

    Helpers.Log("slot to position got {0}, {1}", x, y);
      
    return new Vector2(x, y);
  }

  public bool Available() {
    foreach (var s in slots)
      if (!s) return true;
    return false;
  }

}
