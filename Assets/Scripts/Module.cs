using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ModuleType {
  Core,
  Connection,
  Energy,
  Weapon,
  Shield
}


public class Module : MonoBehaviour {
  public static float _moduleSize = 1.0f;
/*
  public int _hp;

  public int _statHull;
  public int _statEnergy;

  // At most, one of these is non-null;
  public Weapon _weapon;
  public Shield _shield;
  public Engine _engine;
  */
  public ModuleType _type;

  public bool[] _connects = new bool[4]; // Array to store connections for U, R, D, L

  public Cell _cell; // null if not in ship

  public static Module MakeModule(ModuleType type) {
    var mod = GameObject.Instantiate(Helpers.Prefab("Module")).GetComponent<Module>();
    mod._type = type;
    
    // Initialize all connections to true by default
    for (int i = 0; i < mod._connects.Length; i++) {
      mod._connects[i] = true;
    }
    
    // TODO: set sprite based on type
    return mod;
  }

  public void Rotate() {
    bool[] oldConnects = (bool[])_connects.Clone();

    // Rotate clockwise: U -> R, R -> D, D -> L, L -> U
    _connects[(int)dir.R] = oldConnects[(int)dir.U];
    _connects[(int)dir.D] = oldConnects[(int)dir.R];
    _connects[(int)dir.L] = oldConnects[(int)dir.D];
    _connects[(int)dir.U] = oldConnects[(int)dir.L];

    // TODO: better way to do this?
    transform.Rotate(0, 0, -90); // Rotate -90 degrees around Z-axis for clockwise visual rotation
  }

}
