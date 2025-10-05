using UnityEngine;
using System.Collections.Generic;

class FloatingModuleTracker {
  public static FloatingModuleTracker inst;
  int _numSlots = 9;
  public FloatingModule[] _slots; // stores references to the FloatingModule in each slot

  // Grid layout parameters for 3x3 slots
  private float _slotSpacing = 2f; // Distance between centers of adjacent slots
  private float _gridCenterX = 0f; // Center of the slot grid (world X)
  private float _gridCenterY = 6f; // Center of the slot grid (world Y)
  private float _offsetX; // Offset for the leftmost column
  private float _offsetY; // Offset for the bottommost row

  public FloatingModuleTracker() {
    inst = this;
    _slots = new FloatingModule[_numSlots];

    // Calculate initial offsets to center the 3x3 grid around (0,0)
    // For a 3x3 grid where 0,0 is the center point, the "first" slot (0,0 in grid coords) would be at (-_slotSpacing, -_slotSpacing)
    _offsetX = _gridCenterX - _slotSpacing; // Leftmost column center
    _offsetY = _gridCenterY - _slotSpacing; // Bottommost row center
  }

  // Grabs the lowest available slot and assigns the module to it
  public int GrabEmptySlot(FloatingModule module) {
    for (int i = 0; i < _numSlots; i++) {
      if (_slots[i] == null) {
        _slots[i] = module;
        return i;
      }
    }
    Helpers.Error("No available floating module slots!");
    return -1; // Should not be reached if Available() is checked first
  }

  // Releases a slot and triggers reshuffling
  public void ReleaseSlot(int i) {
    if (i >= 0 && i < _numSlots) {
      if (_slots[i] != null) {
        _slots[i] = null;
        ReshuffleSlots(); // Trigger reshuffle after a slot is freed
      }
    } else {
      Helpers.Error("Attempted to release an invalid slot index: {0}", i);
    }
  }

  // Calculates the world position for a given slot index
  public Vector2 SlotToPosition(int slotIdx) {
    int col = slotIdx % 3; // 0, 1, 2 for columns
    int row = slotIdx / 3; // 0, 1, 2 for rows (bottom to top)

    float x = _offsetX + col * _slotSpacing;
    float y = _offsetY + row * _slotSpacing;

    return new Vector2(x, y);
  }

  // Checks if there's any available slot
  public bool Available() {
    for (int i = 0; i < _numSlots; i++) {
      if (_slots[i] == null) return true;
    }
    return false;
  }

  // Floating modules fill in the rows below them when slots open up.
  private void ReshuffleSlots() {
    // Iterate through rows from bottom to top
    for (int row = 0; row < 3; row++) {
      for (int col = 0; col < 3; col++) {
        int currentSlotIdx = row * 3 + col;

        if (_slots[currentSlotIdx] == null) {
          bool foundOne = false;
          for (int rAbove = row + 1; rAbove < 3; rAbove++) {
            for (int cAbove = 0; cAbove < 3; cAbove++) {
              int aboveSlotIdx = rAbove * 3 + cAbove;
              FloatingModule moduleToMove = _slots[aboveSlotIdx];

              if (moduleToMove != null) {
                _slots[currentSlotIdx] = moduleToMove; // Place module in the lower slot
                _slots[aboveSlotIdx] = null; // Clear the higher slot

                // Update the module's slot index and trigger its floating animation
                moduleToMove._slotIdx = currentSlotIdx;
                moduleToMove.StartFloating();
                foundOne = true;
                break;
              }
            }
            if (foundOne)
              break;
          }
        }
      }
    }

  }
}
