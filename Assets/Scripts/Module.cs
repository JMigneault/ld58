using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using DG.Tweening;

public enum ModuleType {
  Core,
  Connection,
  Energy,
  Weapon,
  Shield
}

public class ModuleSpec {
  public ModuleType _type;
  public bool[] _connects; // An array of 4 booleans for U, R, D, L
  public dir _protrusionDir; // The direction of the protrusion

  // Primary constructor
  public ModuleSpec(ModuleType type, bool[] connects, dir protrusionDir = dir.U) {
    _type = type;
    _connects = connects;
    _protrusionDir = protrusionDir;
  }

  // Constructor that defaults connections to false
  public ModuleSpec(ModuleType type, dir protrusionDir = dir.U)
      : this(type, new bool[4] { true, true, true, true }, protrusionDir) {
  }
}

public class Module : MonoBehaviour {
  public static float _moduleSize = 1.0f;
  
  [Header("UI")]
  public GameObject _uiLabel;
  public GameObject _uiHpBar;
  public GameObject _uiPowered;
  public GameObject _uiGenericBar;
  public GameObject _uiWireU;
  public GameObject _uiWireR;
  public GameObject _uiWireD;
  public GameObject _uiWireL;
  public GameObject _uiConnectorUInner;
  public GameObject _uiConnectorUOuter;
  public GameObject _uiConnectorRInner;
  public GameObject _uiConnectorROuter;
  public GameObject _uiConnectorDInner;
  public GameObject _uiConnectorDOuter;
  public GameObject _uiConnectorLInner;
  public GameObject _uiConnectorLOuter;
  public GameObject _uiProtrusionU;
  public GameObject _uiProtrusionR;
  public GameObject _uiProtrusionD;
  public GameObject _uiProtrusionL;

  // New array fields for UI elements by direction
  private GameObject[] _uiWires = new GameObject[4];
  private GameObject[] _uiConnectorsInner = new GameObject[4];
  private GameObject[] _uiConnectorsOuter = new GameObject[4];
  private GameObject[] _uiProtrusions = new GameObject[4];

  [Header("Attributes")]
  public int _maxHp;
  public bool _needsPower;
  public bool _hasProtrusion = false;
  public dir _protrusionDir;

  [Header("Stats")]
  public int _hp;
  public bool _powered;

  [Header("Type Specific")]
  public ModuleType _type;
  // public Weapon _weapon;

  public bool[] _connects = new bool[4]; // Array to store connections for U, R, D, L

  public Cell _cell; // null if not in ship

  // Fields to save current UI values
  private float _currentHealthFill = 1.0f;
  private bool _isPowered = false;
  private float _currentGenericBarFill = 0.0f;

  public static Module MakeModule(ModuleType type) {
    return MakeModule(new ModuleSpec(type));
  }

  public static Module MakeModule(ModuleSpec spec) {
    var mod = GameObject.Instantiate(Helpers.Prefab("Module")).GetComponent<Module>();
    mod._type = spec._type;

    for (int i = 0; i < mod._connects.Length; i++)
      mod._connects[i] = spec._connects[i];

    // Assign existing UI GameObjects to the new arrays by direction
    mod._uiWires[(int)dir.U] = mod._uiWireU;
    mod._uiWires[(int)dir.R] = mod._uiWireR;
    mod._uiWires[(int)dir.D] = mod._uiWireD;
    mod._uiWires[(int)dir.L] = mod._uiWireL;

    mod._uiConnectorsInner[(int)dir.U] = mod._uiConnectorUInner;
    mod._uiConnectorsOuter[(int)dir.U] = mod._uiConnectorUOuter;
    mod._uiConnectorsInner[(int)dir.R] = mod._uiConnectorRInner;
    mod._uiConnectorsOuter[(int)dir.R] = mod._uiConnectorROuter;
    mod._uiConnectorsInner[(int)dir.D] = mod._uiConnectorDInner;
    mod._uiConnectorsOuter[(int)dir.D] = mod._uiConnectorDOuter;
    mod._uiConnectorsInner[(int)dir.L] = mod._uiConnectorLInner;
    mod._uiConnectorsOuter[(int)dir.L] = mod._uiConnectorLOuter;

    // Assign existing UI GameObjects for protrusions to the new array
    mod._uiProtrusions[(int)dir.U] = mod._uiProtrusionU;
    mod._uiProtrusions[(int)dir.R] = mod._uiProtrusionR;
    mod._uiProtrusions[(int)dir.D] = mod._uiProtrusionD;
    mod._uiProtrusions[(int)dir.L] = mod._uiProtrusionL;

    // Disable all protrusions
    for (int i = 0; i < mod._uiProtrusions.Length; i++) {
      if (mod._uiProtrusions[i] != null) {
        mod._uiProtrusions[i].SetActive(false);
      }
    }

    // Set HP to 1.0f and disable its UI initially
    mod._maxHp = 2;
    mod.SetHealth(2);
    mod._uiHpBar.SetActive(false);
    mod._uiGenericBar.SetActive(false);
    mod._uiPowered.SetActive(false);
    mod._hasProtrusion = false;

    switch (spec._type) {
      case ModuleType.Core:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "C";
        }
        // TODO: Initialize Core module specifics
        break;
      case ModuleType.Connection:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "";
        }
        // TODO: Initialize Connection module specifics
        break;
      case ModuleType.Energy:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "E";
        }
        // TODO: Initialize Energy module specifics
        break;
      case ModuleType.Weapon:
        mod._hasProtrusion = true;
        mod._protrusionDir = spec._protrusionDir;
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "W";
        }
        mod._needsPower = true;
        mod._uiPowered.SetActive(true);
        // TODO: Initialize Weapon module specifics
        break;
      case ModuleType.Shield:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "S";
        }
        mod._needsPower = true;
        mod._uiPowered.SetActive(true);
        // TODO: Initialize Shield module specifics
        break;
      default:
        Helpers.Error("Unknown ModuleType: {0}", mod._type);
        break;
    }

    mod.UpdateRotatables(); // Initial connection update
    return mod;
  }

  public void Rotate() {
    bool[] oldConnects = (bool[])_connects.Clone();

    // Rotate clockwise: U -> R, R -> D, D -> L, L -> U
    _connects[(int)dir.R] = oldConnects[(int)dir.U];
    _connects[(int)dir.D] = oldConnects[(int)dir.R];
    _connects[(int)dir.L] = oldConnects[(int)dir.D];
    _connects[(int)dir.U] = oldConnects[(int)dir.L];

    // Rotate protrusion direction
    _protrusionDir = Coord.Turn90Degrees(_protrusionDir);

    UpdateRotatables(); // Update connections after rotation
  }

  public void UpdateRotatables() {
    for (int i = 0; i < _connects.Length; i++) {
      dir d = (dir)i;
      bool connects = _connects[i];

      _uiConnectorsInner[(int)d].SetActive(connects);
      _uiConnectorsOuter[(int)d].SetActive(connects);
    }

    for (int i = 0; i < _uiProtrusions.Length; i++) {
      _uiProtrusions[i].SetActive(_hasProtrusion && (dir)i == _protrusionDir);
    }
  }

  public void SetHealth(int hp) {
    _hp = hp;
    float prop = hp / _maxHp;
    if (prop < 0 || prop > 1f)
      Helpers.Error("Invalid health prop: {0}", prop);
    
    _currentHealthFill = prop;

    if (_uiHpBar != null) {
      // Ensure the UI bar is active when health is set, unless it's being fully filled to 1.0f
      if (prop < 1.0f) {
        _uiHpBar.SetActive(true);
        // Kill any pending disable tweens if health is no longer full
        _uiHpBar.transform.DOKill(true);
      }
      
      UIBar hpBar = _uiHpBar.GetComponentInChildren<UIBar>();
      if (hpBar != null) {
        hpBar.SetFill(prop);
      } else {
        Helpers.Error("UIBar component not found in _uiHpBar children.");
      }

      // If health is full, disable the UI bar after a delay
      if (prop == 1.0f) {
        DOTween.Sequence().AppendInterval(5.0f).OnComplete(() => {
          if (_currentHealthFill == 1.0f) { // Re-check condition in case health changed during delay
            _uiHpBar.SetActive(false);
          }
        });
      }
    }
  }

  public void SetPower(bool powered) {
    _isPowered = powered;
    if (_uiPowered != null) {
      _uiPowered.SetActive(powered);
    }
  }

  public void SetBar(float prop) {
    if (prop < 0 || prop > 1f)
      Helpers.Error("Invalid bar prop: {0}", prop);

    _currentGenericBarFill = prop;

    if (_uiGenericBar != null) {
      // Ensure the UI bar is active when bar is set, unless it's being fully emptied to 0.0f
      if (prop > 0.0f) {
        _uiGenericBar.SetActive(true);
        // Kill any pending disable tweens if bar is no longer empty
        _uiGenericBar.transform.DOKill(true);
      }

      UIBar genericBar = _uiGenericBar.GetComponentInChildren<UIBar>();
      if (genericBar != null) {
        genericBar.SetFill(prop);
      } else {
        Helpers.Error("UIBar component not found in _uiGenericBar children.");
      }

      // If bar is empty, disable the UI bar after a delay
      if (prop == 0.0f) {
        DOTween.Sequence().AppendInterval(5.0f).OnComplete(() => {
          if (_currentGenericBarFill == 0.0f) { // Re-check condition in case bar value changed during delay
            _uiGenericBar.SetActive(false);
          }
        });
      }
    }
  }
}
