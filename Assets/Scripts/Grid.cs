using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid {
  public bool _players;
  public SpawnSlot _spawnSlot = SpawnSlot.None; // Added to track the slot this grid occupies

  int _modsAdded = 0;

  public static Grid _playersGrid;
  public GameObject _parent;

  public int _dimX;
  public int _dimY;
  Cell[,] _cells;

  public int _enginePower = 1;

  public Grid(int dimX, int dimY, GameObject parent, bool players = false, SpawnSlot spawnSlot = SpawnSlot.None) {
    _players = players;
    if (players)
      _playersGrid = this;
    _parent = parent;
    _spawnSlot = spawnSlot; // Assign the spawn slot
    _dimX = dimX;
    _dimY = dimY;
    _cells = new Cell[dimX, dimY];
    for (int x = 0; x < dimX; x++) {
      for (int y = 0; y < dimY; y++) {
        _cells[x, y] = new Cell(new Coord(x, y), this);
      }
    }
  }

  public bool ValidCoord(Coord coord) {
    return coord.x >= 0 && coord.x < _dimX &&
           coord.y >= 0 && coord.y < _dimY;
  }

  public Cell GetCell(Coord coord) {
    return _cells[coord.x, coord.y];
  }

  public bool AddModule(Module module, Coord coord) {

    if (++_modsAdded == 4) // HACK: don't start time based actions until the player has put down their three initial modules
      TimeLord.inst._started = true;

    // Check if coord is within bounds
    if (ValidCoord(coord)) {
      Cell targetCell = _cells[coord.x, coord.y];
      
      // Check if the cell does not already have a module
      if (targetCell.Module == null) {
        targetCell.Module = module;
        module._cell = targetCell;
        module.transform.SetParent(_parent.transform); // Set parent to grid's parent
        module.transform.position = CoordToPosition(coord, Helpers._modZ); // Use updated CoordToPosition
        module.transform.localRotation = Quaternion.identity; // Align module with grid's local orientation
        module.SetHp();

        UpdateStats();

        if (_players)
          UIController.inst.IncrModulesCollected();

        return true;
      }
    }

    // If out of bounds or cell already has a module
    return false;
  }

  public Module GetModule(Coord coord) {
    if (ValidCoord(coord)) {
      Cell targetCell = _cells[coord.x, coord.y];
      return targetCell.Module;
    }
    return null;
  }

  private Vector2 _GetGridOriginOffset() {
    // Assuming the _parent's pivot is at the center of the ship,
    // (0,0) grid cell is top-left.
    float totalGridWidth = _dimX * Module._moduleSize;
    float totalGridHeight = _dimY * Module._moduleSize;

    float offsetX = -totalGridWidth / 2f + Module._moduleSize / 2f;
    float offsetY = totalGridHeight / 2f - Module._moduleSize / 2f; // Top-left of the grid visually

    return new Vector2(offsetX, offsetY);
  }

  public Vector3 CoordToPosition(Coord coord, float z = 0.0f) {
    Vector2 localGridOffset = _GetGridOriginOffset();
    float localX = localGridOffset.x + coord.x * Module._moduleSize;
    float localY = localGridOffset.y - coord.y * Module._moduleSize;

    Vector3 localCellPosition = new Vector3(localX, localY, z);
    return _parent.transform.TransformPoint(localCellPosition); // Convert local to world
  }

  public Coord PositionToCoord(Vector2 worldPosition) {
    Vector3 localPosition = _parent.transform.InverseTransformPoint(worldPosition); // Convert world to local

    Vector2 localGridOffset = _GetGridOriginOffset();

    int coordX = Mathf.RoundToInt((localPosition.x - localGridOffset.x) / Module._moduleSize);
    int coordY = Mathf.RoundToInt((localGridOffset.y - localPosition.y) / Module._moduleSize);

    return new Coord(coordX, coordY);
  }

  public void SetEnginePower(int engine) {
    _enginePower = engine;
    if (_enginePower > 5)
      _enginePower = 5;

    if (_players) {
      UIController.inst.SetEngineStrength(_enginePower);
    }

  }

  public void UpdateStats() {
    _enginePower = 1; // always at least 1

    // Collect all modules first to avoid issues with modifying the grid while iterating
    List<Module> allModules = new List<Module>();
    for (int x = 0; x < _dimX; x++) {
      for (int y = 0; y < _dimY; y++) {
        Module module = _cells[x, y].Module;
        if (module != null) {
          allModules.Add(module);
        }
      }
    }

    // First pass: Determine which modules are powered.
    foreach (Module module in allModules) {
      bool adjacentToEnergy = false;

      // Check all 8 neighboring tiles (cardinal and diagonal) for energy sources
      if (module._needsPower) {
        module._powered = false; // Reset internal powered state
        Coord moduleCoord = module._cell._coord;
        Module poweredBy = null;
        for (int dx = -1; dx <= 1; dx++) {
          for (int dy = -1; dy <= 1; dy++) {
            if (dx == 0 && dy == 0) continue; // Skip the module itself

            Coord neighborCoord = new Coord(moduleCoord.x + dx, moduleCoord.y + dy);

            if (ValidCoord(neighborCoord)) {
              Module neighborModule = GetModule(neighborCoord);
              if (neighborModule != null && neighborModule._type == ModuleType.Energy && !neighborModule._recharging) {
                adjacentToEnergy = true;
                poweredBy = neighborModule;
                break; // Found an energy source, no need to check further neighbors for power
              }
            }
          }
          if (adjacentToEnergy) break;
        }
        // Apply power if the module needs it and is adjacent to an Energy module
        module.SetPower(poweredBy);
      }

    }

    // Second pass: Determine which modules are shielded (adjacent to powered shield module).
    int nEngines = 1;
    foreach (Module module in allModules) {
      if (module._type == ModuleType.Engine)
        nEngines++;

      bool adjacentToPoweredShield = false;
      Module shieldedBy = null;
      Coord moduleCoord = module._cell._coord;
      for (int dx = -1; dx <= 1; dx++) {
        for (int dy = -1; dy <= 1; dy++) {
          if (dx == 0 && dy == 0) continue; // Skip the module itself

          Coord neighborCoord = new Coord(moduleCoord.x + dx, moduleCoord.y + dy);

          if (ValidCoord(neighborCoord)) {
            Module neighborModule = GetModule(neighborCoord);
            if (neighborModule != null && neighborModule._type == ModuleType.Shield && neighborModule._powered && !neighborModule._recharging) {
              shieldedBy = neighborModule;
              adjacentToPoweredShield = true;
              break; // Found a powered shield source, no need to check further neighbors for shielding
            }
          }
        }
        if (adjacentToPoweredShield) break;
      }

      module.SetShielded(shieldedBy);
    }

    SetEnginePower(nEngines);
  }

  public void DestroyShip() {
    if (_players) {
      UIController.inst.GameOver();
      BadGuyController.inst.GameOver();
      Placer.inst.GameOver();
    } else {
      BadGuyController.inst.BadGuyDied(this);
    }

    // Destroy all modules in the grid
    for (int x = 0; x < _dimX; x++) {
      for (int y = 0; y < _dimY; y++) {
        Module module = _cells[x, y].Module;
        if (module != null) {
          module.DestroyModule();
        }
      }
    }

    // Destroy the parent GameObject of the grid
    if (_parent != null) {
      UnityEngine.Object.Destroy(_parent);
    }
  }

  public void ConnectorHighlights() {
    var allDirs = new Coord(-1, -1).allDirs;
    for (int x = 0; x < _dimX; x++) {
      for (int y = 0; y < _dimY; y++) {
        Module module = _cells[x, y].Module;
        if (module != null) {
          foreach (dir d in allDirs) {
            module.SetConnectorColor(d, true, false); // Set to normal, not connected
          }
        }
      }
    }
  }

  public void HighlightConnectorsForHover(Module hovering, Coord hoverCoord) {
    if (hovering == null) {
      ConnectorHighlights();
      return;
    }

    var allDirs = new Coord(-1, -1).allDirs;
    // 1. Reset all connectors on currently placed modules to 'does not connect'.
    for (int x = 0; x < _dimX; x++) {
      for (int y = 0; y < _dimY; y++) {
        Module module = _cells[x, y].Module;
        if (module != null) {
          foreach (dir d in allDirs) {
            module.SetConnectorColor(d, false, false); // Default to 'does not connect'
          }
        }
      }
    }

    // 2. Reset all connectors on the hovering module to 'does not connect'.
    if (hovering != null) {
      foreach (dir d in allDirs) {
        hovering.SetConnectorColor(d, false, false);
      }
    }

    // 3. Check for connections between the hovering module and placed modules.
    // If a connection is found, mark both sides as 'connects'.
    if (hovering != null && ValidCoord(hoverCoord)) {
      foreach (dir d in allDirs) {
        // If the hovering module does not have a connector in this direction, skip.
        // Its color is already set to 'does not connect'.
        if (!hovering._connects[(int)d]) {
          continue;
        }

        Coord neighborCoord = hoverCoord.Neighbor(d);

        if (ValidCoord(neighborCoord)) {
          Module neighborModule = GetModule(neighborCoord);

          if (neighborModule != null) {
            // If the neighbor module also has a connector in the opposite direction, they connect.
            if (neighborModule._connects[(int)Coord.OppDir(d)]) {
              hovering.SetConnectorColor(d, false, true);
              neighborModule.SetConnectorColor(Coord.OppDir(d), false, true);
            }
          }
        }
      }
    }

    // 4. Check for connections between already placed modules.
    for (int x = 0; x < _dimX; x++) {
      for (int y = 0; y < _dimY; y++) {
        Module currentModule = _cells[x, y].Module;

        if (currentModule != null) {
          foreach (dir d in allDirs) {
            // If the current module does not have a connector in this direction, skip.
            if (!currentModule._connects[(int)d]) {
              continue;
            }

            Coord neighborCoord = currentModule._cell._coord.Neighbor(d);

            if (ValidCoord(neighborCoord)) {
              Module neighborModule = GetModule(neighborCoord);

              // Ensure the neighbor module is not the hovering module (it's already handled)
              // and that it's a placed module, and it has a connector in the opposite direction.
              if (neighborModule != null && neighborModule != hovering && neighborModule._connects[(int)Coord.OppDir(d)]) {
                currentModule.SetConnectorColor(d, false, true);
                neighborModule.SetConnectorColor(Coord.OppDir(d), false, true);
              }
            }
          }
        }
      }
    }
  }

}
