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
        module.transform.position = CoordToPosition(coord, Helpers._modZ);
        module.transform.parent = _parent.transform;

        UpdateStats();

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
    Vector2 parentPos = _parent.transform.position;
    float gridWidth = _dimX * Module._moduleSize;
    float gridHeight = _dimY * Module._moduleSize;
    float startX = parentPos.x - (gridWidth / 2f) + (Module._moduleSize / 2f);
    // For inverted Y, coord.y = 0 is the top. So, startY should be the world Y of the top-most row's center.
    float startY = parentPos.y + (gridHeight / 2f) - (Module._moduleSize / 2f);
    return new Vector2(startX, startY);
  }

  public Vector3 CoordToPosition(Coord coord, float z = 0.0f) {
    Vector2 originOffset = _GetGridOriginOffset();

    float posX = originOffset.x + coord.x * Module._moduleSize;
    // Y-coordinate is inverted: highest world y for coord.y = 0, decreases as coord.y increases
    float posY = originOffset.y - coord.y * Module._moduleSize;

    return new Vector3(posX, posY, z);
  }

  public Coord PositionToCoord(Vector2 position) {
    Vector2 originOffset = _GetGridOriginOffset();

    // Calculate the coordinate by reversing the CoordToPosition logic
    int coordX = Mathf.RoundToInt((position.x - originOffset.x) / Module._moduleSize);
    // Invert the y-coordinate conversion
    int coordY = Mathf.RoundToInt((originOffset.y - position.y) / Module._moduleSize);

    return new Coord(coordX, coordY);
  }

  public void SetEnginePower(int engine) {
    _enginePower = engine;
    if (_enginePower > 5)
      _enginePower = 5;

    if (_players) {
      UIController.inst.SetEnginePower(_enginePower);
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
        for (int dx = -1; dx <= 1; dx++) {
          for (int dy = -1; dy <= 1; dy++) {
            if (dx == 0 && dy == 0) continue; // Skip the module itself

            Coord neighborCoord = new Coord(moduleCoord.x + dx, moduleCoord.y + dy);

            if (ValidCoord(neighborCoord)) {
              Module neighborModule = GetModule(neighborCoord);
              if (neighborModule != null && neighborModule._type == ModuleType.Energy && !neighborModule._recharging) {
                adjacentToEnergy = true;
                break; // Found an energy source, no need to check further neighbors for power
              }
            }
          }
          if (adjacentToEnergy) break;
        }
        // Apply power if the module needs it and is adjacent to an Energy module
        module.SetPower(adjacentToEnergy);
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

}
