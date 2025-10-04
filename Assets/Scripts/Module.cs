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

  public bool _connectsUp = true;
  public bool _connectsRight = true;
  public bool _connectsDown = true;
  public bool _connectsLeft = true;

  public Cell _cell; // null if not in ship

  public static Module MakeModule(ModuleType type) {
    var mod = GameObject.Instantiate(Helpers.Prefab("Module")).GetComponent<Module>();
    // TODO: set sprite based on type
    mod._type = type;
    return mod;
  }

}
