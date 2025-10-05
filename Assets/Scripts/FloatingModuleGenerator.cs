using UnityEngine;
using DG.Tweening; // Import DOTween
using System.Collections.Generic;

public class FloatingModuleGenerator {
  public static FloatingModuleGenerator inst;

  public FloatingModuleGenerator() {
    inst = this;
  }

  public Module Generate() {
    Module generated = null;

    float v = Random.value;


    Grid playerGrid = Init._inst._grid;
    int energyCount = 0;
    int weaponCount = 0;
    int shieldCount = 0;

    for (int x = 0; x < playerGrid._dimX; x++) {
      for (int y = 0; y < playerGrid._dimY; y++) {
        Module module = playerGrid.GetModule(new Coord(x, y));
        if (module != null) {
          if (module._type == ModuleType.Energy) energyCount++;
          else if (module._type == ModuleType.Weapon) weaponCount++;
          else if (module._type == ModuleType.Shield) shieldCount++;
        }
      }
    }

    // Include floating modules in your count too.

    for (int i = 0; i < 9; i++) {
      var floatingMod = FloatingModuleTracker.inst._slots[i];
      if (floatingMod != null) {
        Module module = floatingMod.GetComponent<Module>();
        if (module != null) {
          if (module._type == ModuleType.Energy) energyCount++;
          else if (module._type == ModuleType.Weapon) weaponCount++;
          else if (module._type == ModuleType.Shield) shieldCount++;
        }
      }
    }

    ModuleType typeToGenerate;

    // 1. Force if zero count (prioritize Energy, Weapon, Shield)
    if (energyCount == 0) {
      typeToGenerate = ModuleType.Energy;
    } else if (weaponCount == 0) {
      typeToGenerate = ModuleType.Weapon;
    } else if (shieldCount == 0) {
      typeToGenerate = ModuleType.Shield;
    } else {
      // 2. If no zeros, 50% chance for completely random module
      if (Random.value < 0.5f) {
        // Generate a truly random module from all types
        ModuleType[] allModuleTypes = new ModuleType[] {
            ModuleType.Connection, ModuleType.Energy,
            ModuleType.Weapon, ModuleType.Shield, ModuleType.Engine
        };
        typeToGenerate = allModuleTypes[Random.Range(0, allModuleTypes.Length)];
      } else {
        // 3. Otherwise (not zero, not 50% random), generate whichever we have the least of (Energy, Weapon, Shield)
        // Break ties in order: Energy, Weapon, Shield
        if (energyCount <= weaponCount && energyCount <= shieldCount) {
          typeToGenerate = ModuleType.Energy;
        } else if (weaponCount < energyCount && weaponCount <= shieldCount) {
          typeToGenerate = ModuleType.Weapon;
        } else { // shieldCount is the least
          typeToGenerate = ModuleType.Shield;
        }
      }
    }

    // Create the module based on the determined type
    switch (typeToGenerate) {
      case ModuleType.Energy:
      case ModuleType.Weapon:
      case ModuleType.Shield:
      case ModuleType.Connection:
      case ModuleType.Core:
      case ModuleType.Engine:
        generated = Module.MakeModule(RandomSpec(typeToGenerate));
        break;
      default:
        Helpers.Error("FloatingModuleGenerator: Unhandled module type for generation: " + typeToGenerate);
        generated = Module.MakeModule(RandomSpec(ModuleType.Connection)); // Fallback to a random connection module
        break;
    }

    // Place it a bit off screen
    generated.transform.position = new Vector3(0, 25f, Helpers._modZ);

    return generated;
  }

  // Generates a random ModuleSpec for a given module type, randomizing connections.
  ModuleSpec RandomSpec(ModuleType mType) {
    bool[] connects = new bool[4];
    dir protrusionDir = dir.U; // Default protrusion direction

    switch (mType) {
      case ModuleType.Core:
        connects = new bool[] { true, true, true, true }; // Fixed connections for Core
        break;
      case ModuleType.Connection:
        connects = GenerateRandomConnections(3);
        break;
      case ModuleType.Energy:
        connects = GenerateRandomConnections(3);
        break;
      case ModuleType.Weapon:
        connects = new bool[] { false, true, true, true }; // Fixed connections for Weapon (R, D, L)
        connects = GenerateRandomConnections(2, new List<int> { 1, 2, 3 } );
        protrusionDir = dir.U;
        break;
      case ModuleType.Shield:
        connects = GenerateRandomConnections(2);
        break;
      case ModuleType.Engine:
        connects = GenerateRandomConnections(2);
        break;
      default:
        Helpers.Error("RandomSpec: Unknown ModuleType: " + mType);
        connects = new bool[] { true, true, true, true }; // Fallback to all connections
        break;
    }

    return new ModuleSpec(mType, connects, protrusionDir);
  }

  // Helper function to generate a bool array for connections with a specified number of random connections
  bool[] GenerateRandomConnections(int numberOfConnections, List<int> availableDirs=null) {
    bool[] connections = new bool[4];
    if (availableDirs == null) {
      availableDirs = new List<int> { 0, 1, 2, 3 }; // Represents U, R, D, L
    }

    for (int i = 0; i < numberOfConnections; i++) {
      if (availableDirs.Count == 0) break;

      int chosenIndex = Random.Range(0, availableDirs.Count);
      connections[availableDirs[chosenIndex]] = true;
      availableDirs.RemoveAt(chosenIndex);
    }
    return connections;
  }

  // This method is no longer used for generating module specs, but kept for context if needed elsewhere.
  bool[] ConnectorsForModule(ModuleType mType) {
    switch (mType) {
      case ModuleType.Core:
        return new bool[4] { true, true, true, true }; // All directions
      case ModuleType.Connection:
        return new bool[4] { true, true, true, true }; // All directions (default for old usage)
      case ModuleType.Energy:
        return new bool[4] { false, true, true, false }; // R, D
      case ModuleType.Weapon:
        return new bool[4] { false, true, true, true }; // R, D, L
      case ModuleType.Shield:
        return new bool[4] { true, false, false, true }; // U, L
      case ModuleType.Engine:
        return new bool[4] { true, true, true, true }; // All directions
      default:
        Helpers.Error("ConnectorsForModule: Unknown ModuleType: " + mType);
        return new bool[4] { true, true, true, true }; // Fallback to all connections
    }
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
