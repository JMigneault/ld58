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
  Shield,
  Engine
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
  public GameObject _uiShielded;
  public GameObject _uiGenericBar;
  public GameObject _uiHalo;
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

  public Color _powerHaloColor;
  public Color _shieldHaloColor;
  public Color _engineLabelColor;
  public Color _coreLabelColor;
  public Color _weaponLabelColor;
  public Color _energyLabelColor;
  public Color _shieldLabelColor;

  public Color _connectorConnectsColor;
  public Color _connectorDoesntConnectColor;
  public Color _connectorNormalColor;

  [Header("Attributes")]
  public int _maxHp;
  public bool _needsPower;
  public bool _hasProtrusion = false;
  public dir _protrusionDir;

  [Header("Stats")]
  public int _hp;
  public Module _poweredBy;
  public Module _shieldedBy;
  public bool _powered;

  [Header("Type Specific")]
  public ModuleType _type;
  public Weapon _weapon;
  public Shields _shields;
  public Battery _battery;
  public bool _recharging = false;

  public bool[] _connects = new bool[4]; // Array to store connections for U, R, D, L

  public Cell _cell; // null if not in ship

  // Fields to save current UI values
  private float _currentHealthFill = 1.0f;
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
    mod._maxHp = 5;
    mod._uiHpBar.SetActive(false);
    mod._uiGenericBar.SetActive(false);
    mod._uiPowered.SetActive(false);
    mod._uiShielded.SetActive(false);
    mod._uiHalo.SetActive(false);
    mod._hasProtrusion = false;

    switch (spec._type) {
      case ModuleType.Core:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "C";
          mod._uiLabel.GetComponent<TMP_Text>().color = mod._coreLabelColor;
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
          mod._uiLabel.GetComponent<TMP_Text>().text = "P";
          mod._uiLabel.GetComponent<TMP_Text>().color = mod._energyLabelColor;
        }
        mod._battery = mod.gameObject.AddComponent<Battery>();
        break;
      case ModuleType.Weapon:
        mod._hasProtrusion = true;
        mod._protrusionDir = spec._protrusionDir;
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "G";
          mod._uiLabel.GetComponent<TMP_Text>().color = mod._weaponLabelColor;
        }
        mod._needsPower = true;
        mod._uiPowered.SetActive(true);
        mod._weapon = mod.gameObject.AddComponent<Weapon>();
        break;
      case ModuleType.Shield:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "S";
          mod._uiLabel.GetComponent<TMP_Text>().color = mod._shieldLabelColor;
        }
        mod._needsPower = true;
        mod._uiPowered.SetActive(true);
        mod._shields = mod.gameObject.AddComponent<Shields>();
        break;
      case ModuleType.Engine:
        if (mod._uiLabel != null) {
          mod._uiLabel.GetComponent<TMP_Text>().text = "E";
          mod._uiLabel.GetComponent<TMP_Text>().color = mod._engineLabelColor;
        }
        mod._needsPower = true;
        mod._uiPowered.SetActive(true);
        // TODO: Initialize Engine module specifics
        break;
      default:
        Helpers.Error("Unknown ModuleType: {0}", mod._type);
        break;
    }

    mod.SetHealth(mod._maxHp);

    mod.UpdateRotatables(); // Initial connection update

    // Set all connector colors to normal.
    foreach (dir d in Enum.GetValues(typeof(dir))) {
      mod.SetConnectorColor(d, true, false);
    }

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
    float prop = hp / (1.0f * _maxHp);
    if (prop < 0 || prop > 1f)
      Helpers.Error("Invalid health prop: {0}", prop);

    Helpers.Log("health {0} prop {1}", hp, prop);
    
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

  public void SetPower(Module poweredBy) {
    _poweredBy = poweredBy;
    _powered = poweredBy != null;
    _uiPowered.SetActive(true); // Ensure the power indicator is active when setting power state

    UIBar poweredBar = _uiPowered.GetComponentInChildren<UIBar>();
    if (poweredBar != null) {
      poweredBar.SetFill(_powered ? 1.0f : 0.0f);
    } else {
      Helpers.Error("UIBar component not found in _uiPowered children.");
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

  public void Damage() {
    if (_shieldedBy != null) {
      _shieldedBy._shields.TakeHit();
    } else {
      SetHealth(_hp - 1); // Lower hp and update UI
      if (_hp <= 0) {
        // Destroy the module's GameObject
        if (_type == ModuleType.Core) {
          _cell._grid.DestroyShip();
          Helpers.Log("Ship destroyed!");
        } else {
          DestroyModule();
        }
      }
    }
  }

  public void SetShowHalo(bool show) {
    if (_uiHalo != null) {
      _uiHalo.SetActive(show);
      if (show) {
        Renderer haloRenderer = _uiHalo.GetComponent<Renderer>();
        if (haloRenderer != null && haloRenderer.material != null) {
          Color haloColor;
          if (_type == ModuleType.Shield) {
            haloColor = _shieldHaloColor;
          } else if (_type == ModuleType.Energy) {
            // Default to power halo color for other powered modules (Core, Energy, Weapon, Engine)
            haloColor = _powerHaloColor;
          } else {
            _uiHalo.SetActive(false);
            return;
          }
          haloRenderer.material.SetColor("_Color", haloColor);
        } else {
          Helpers.Error("Halo UI element is missing Renderer or Material.");
        }
      }
    }
  }

  public bool GoodToScale() {
    return GetComponent<FloatingModule>()._floating || (_cell != null && _cell._grid._players);
  }

  public void SetScaled(bool scaled) {
    if (GoodToScale()) {
      if (scaled)
        transform.DOScale(1.1f, 0.1f);
      else
        transform.DOScale(1.0f, 0.1f);
    }
  }

  public void Hover() {
    SetScaled(true);
    if (_cell != null) {
      SetShowHalo(true);
    }
    UIController.inst.SetTooltip(GetTooltip());
  }

  public void UnHover() {
    SetScaled(false);
    SetShowHalo(false);
    UIController.inst.SetTooltip("");
  }

  public void ProjectingCoord(Coord coord) {
    SetShowHalo(true);
  }

  public void StopProjecting() {
    SetShowHalo(false);
  }
  
  public void SetShielded(Module shieldedBy) {
    _uiShielded.SetActive(shieldedBy != null);
    _shieldedBy = shieldedBy;
  }

  public void SetRecharging(bool recharging) {
    _recharging = recharging;
    if (_cell != null && _cell._grid != null) {
      _cell._grid.UpdateStats();
    }

    if (_uiGenericBar != null) {
      UIBar genericBar = _uiGenericBar.GetComponentInChildren<UIBar>();
      if (genericBar != null) {
        genericBar.SetBarMode(recharging ? BarMode.Regen : BarMode.Normal);
      } else {
        Helpers.Error("UIBar component not found in _uiGenericBar children.");
      }
    }
  }

  public void DestroyModule() {
    bool players = _cell._grid._players;

    Grid grid = null;
    if (_cell != null) {
      grid = _cell._grid;
      _cell.Module = null; // Clear the module from its cell
      _cell = null; // Clear the cell reference in the module
    }

    if (!players && _type != ModuleType.Core && UnityEngine.Random.value < 0.3f) {
      // 30% chance for this module to start floating
      FloatingModule floatingModule = GetComponent<FloatingModule>();
      if (floatingModule != null) {
        SetPower(null);
        SetShielded(null);
        floatingModule.EnableFloat(true);
        transform.parent = null;
        Helpers.Log("Module is now floating!");
      } else {
        // Fallback: If FloatingModule component is missing, destroy it
        Helpers.Error("FloatingModule component not found on module. Destroying instead of floating.");
        GameObject.Destroy(gameObject);
      }
    } else {
      // Otherwise, destroy the module
      GameObject.Destroy(gameObject);
      Helpers.Log("Module was destroyed.");
    }

    if (grid != null) {
      grid.UpdateStats();
    }
  }

  public void SetHp() {
    bool core = _type == ModuleType.Core;
    if (_cell._grid._players) {
      _maxHp = core ? 7 : 3;
    } else {
      _maxHp = 3;
    }

    SetHealth(_maxHp);
  }

  public string GetTooltip() {

    switch (_type) {
      case ModuleType.Core:
        return "Core (C)\nKills the ship when destroyed.";
      case ModuleType.Connection:
        return "Connector\nNo special effects.";
      case ModuleType.Energy:
        return "Power (P)\nPowers adjacent modules.";
      case ModuleType.Weapon:
        return "Gun (G)\nFires regularly.\nREQUIRES POWER";
      case ModuleType.Shield:
        return "Shield (S)\nShields adjacent modules.\nREQUIRES POWER";
      case ModuleType.Engine:
        return "Engine (E)\nImproves turn speed. Raises rate of random module spawns.\nREQUIRES POWER";
      default:
        return "Unknown Module";
    }

  }

  public void SetConnectorColor(dir d, bool normal, bool connects) {
    Color targetColor;
    if (normal) {
      targetColor = _connectorNormalColor;
    } else if (connects) {
      targetColor = _connectorConnectsColor;
    } else {
      targetColor = _connectorDoesntConnectColor;
    }

    GameObject innerConnector = _uiConnectorsInner[(int)d];
    GameObject outerConnector = _uiConnectorsOuter[(int)d];

    if (innerConnector != null) {
      Renderer innerRenderer = innerConnector.GetComponent<Renderer>();
      if (innerRenderer != null && innerRenderer.material != null) {
        innerRenderer.material.SetColor("_Color", targetColor);
      }
    }

    if (outerConnector != null) {
      Renderer outerRenderer = outerConnector.GetComponent<Renderer>();
      if (outerRenderer != null && outerRenderer.material != null) {
        outerRenderer.material.SetColor("_Color", targetColor);
      }
    }
  }

}
