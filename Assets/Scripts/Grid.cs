using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid {
  public bool _players;

  public static Grid _playersGrid;
  public GameObject _parent;

  public int _dimX;
  public int _dimY;
  Cell[,] _cells;

  public int _enginePower = 1;

  public Grid(int dimX, int dimY, GameObject parent, bool players = false) {
    _players = players;
    if (players)
      _playersGrid = this;
    _parent = parent;
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
    foreach (Module module in allModules) {
      if (module._type == ModuleType.Engine)
        SetEnginePower(_enginePower + 1);

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

}
