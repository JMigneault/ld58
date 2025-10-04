using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placer {
  public static Placer inst;
  public Grid _grid;
  public TileHighlight[,] _highlightTiles; // Changed to a 2D array of TileHighlight

  public Module _currentModule;

  public Placer(Grid grid) {
    inst = this;
    _grid = grid;

    // Create the grid of tile highlights once, so we can enable and disable later as needed
    _highlightTiles = new TileHighlight[_grid._dimX, _grid._dimY];
    for (int x = 0; x < _grid._dimX; x++) {
      for (int y = 0; y < _grid._dimY; y++) {
        GameObject highlightGameObject = GameObject.Instantiate(Helpers.Prefab("TileBackground"));
        highlightGameObject.transform.position = _grid.CoordToPosition(new Coord(x, y));
        TileHighlight tileHighlight = highlightGameObject.GetComponent<TileHighlight>();
        if (tileHighlight != null) {
          highlightGameObject.SetActive(false);
          _highlightTiles[x, y] = tileHighlight;
        } else {
          Helpers.Error("TileHighlight component not found on HighlightTile prefab at creation.");
        }
      }
    }
  }

  public void StartPlacing(Module module) {
    _currentModule = module;

    // Clear existing highlights before finding new ones
    StopPlacing();

    // Find all valid placements and highlight them
    for (int x = 0; x < _grid._dimX; x++) {
      for (int y = 0; y < _grid._dimY; y++) {
        Coord coord = new Coord(x, y);
        TileHighlight tileHighlight = _highlightTiles[x, y];
        if (tileHighlight != null) {
          _highlightTiles[x, y].gameObject.SetActive(true);
          tileHighlight.SetHighlightMode(CanPlaceModule(_currentModule, _grid, coord) ? HighlightMode.Valid : HighlightMode.Invalid);
        }
      }
    }
  }

  public void StopPlacing() {
    for (int x = 0; x < _grid._dimX; x++) {
      for (int y = 0; y < _grid._dimY; y++) {
        if (_highlightTiles[x, y] != null) {
          _highlightTiles[x, y].gameObject.SetActive(false);
        }
      }
    }
    _currentModule = null; // Also clear the current module when stopping placement
  }

  public void Hover(Vector2 mousePosition) {
    if (_currentModule != null) {
      Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosition);
      Coord hoveredCoord = _grid.PositionToCoord(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
      
      // If the hovered coord is valid, snap the module to its center
      if (_grid.ValidCoord(hoveredCoord)) {
        _currentModule.transform.position = _grid.CoordToPosition(hoveredCoord, Helpers._modZ);
      } else {
        // If not on a valid grid coordinate, just follow the raw mouse world position (but keep z at 0)
        _currentModule.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
      }
    }
  }


  public static bool CanPlaceModule(Module mod, Grid grid, Coord coord) {
    // 1. Initial validity: coord within bounds and empty.
    if (!grid.ValidCoord(coord) || grid.GetModule(coord) != null) {
      return false;
    }

    // Check if the grid is empty.
    bool gridHasExistingModules = false;
    for (int x = 0; x < grid._dimX; x++) {
      for (int y = 0; y < grid._dimY; y++) {
        if (grid.GetModule(new Coord(x, y)) != null) {
          gridHasExistingModules = true;
          break;
        }
      }
      if (gridHasExistingModules) break;
    }

    // 2. Handle first module placement (Core only, no connection required)
    if (!gridHasExistingModules && mod._type == ModuleType.Core) {
      return true; // Can place the first Core module.
    }

    // For any other module, or if the grid is not empty, connection is required.
    bool foundValidModuleConnection = false;

    foreach (dir d in coord.allDirs) {
      Coord neighborCoord = coord.Neighbor(d);

      if (grid.ValidCoord(neighborCoord)) {
        Module neighborModule = grid.GetModule(neighborCoord);

        if (neighborModule != null) { // There's an existing module at this neighbor
          // Check 3: Does placing 'mod' block an existing module's protrusion?
          if (neighborModule._hasProtrusion && neighborModule._protrusionDir == Coord.OppDir(d)) {
            return false; // Existing neighbor's protrusion would be blocked.
          }

          // Check 4: Does 'mod' connect to the existing neighbor module?
          bool modConnectsThisWay = mod._connects[(int)d];
          bool neighborConnectsBack = neighborModule._connects[(int)Coord.OppDir(d)];

          if (modConnectsThisWay && neighborConnectsBack) {
            foundValidModuleConnection = true;
          }
        }
      }
    }

    // If we couldn't find any valid connection, it's an invalid placement.
    if (!foundValidModuleConnection) {
      return false;
    }

    // Check 5: Does 'mod's own protrusion get blocked by an existing module?
    if (mod._hasProtrusion) {
      Coord protrusionTargetCoord = coord.Neighbor(mod._protrusionDir);
      // If the target for the new module's protrusion is valid AND occupied by an existing module, then it's blocked.
      if (grid.ValidCoord(protrusionTargetCoord) && grid.GetModule(protrusionTargetCoord) != null) {
        return false;
      }
    }

    // All checks passed.
    return true;
  }

  public bool TryPlacing(Vector2 mousePosition) {
    if (_currentModule != null) {
      Coord candidateCoord = ChoosePlacementCandidate(mousePosition);

      if (candidateCoord != new Coord(-1, -1)) {
        // A valid placement candidate was found.
        // Now, try to add the current module to the grid at this coordinate.
        var success = _grid.AddModule(_currentModule, candidateCoord);
        if (success) {
          _currentModule = null;
          // TODO: XXX: TEMP
          _currentModule = Module.MakeModule(ModuleType.Connection);
        }
        return success;
      }
    }
    // No valid placement candidate found, or AddModule failed.
    return false;
  }

  public Coord ChoosePlacementCandidate(Vector2 mousePosition) {
    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosition);
    Coord hoveredCoord = _grid.PositionToCoord(new Vector2(mouseWorldPos.x, mouseWorldPos.y));

    int closestCandDist = 100000;
    Coord closestCand = new Coord(-1, -1);

    if (_grid.ValidCoord(hoveredCoord) && _grid.GetModule(hoveredCoord) != null)
      return closestCand;

    foreach (dir d in hoveredCoord.allDirs) {
      Coord cursor = hoveredCoord;
      int dist = 0;
      while ((d == dir.U && cursor.y >= 0) ||
             (d == dir.R && cursor.x < _grid._dimX) ||
             (d == dir.D && cursor.y < _grid._dimY) ||
             (d == dir.L && cursor.x >= 0)) {
        Coord next = cursor.Neighbor(d);
        Module nextModule = _grid.GetModule(next);
        if (_grid.ValidCoord(next) && nextModule != null) {
          // Found an existing module in this direction.
          // The current 'cursor' position is a candidate if it's empty.
          if (_grid.GetModule(cursor) == null && dist < closestCandDist) {
            bool canConnect = nextModule._connects[(int)Coord.OppDir(d)];

            if (canConnect) {
              closestCandDist = dist;
              closestCand = cursor;
            }
          }
          break; // Stop looking in this direction, found the edge
        }
        cursor = next;
        dist++;
      }
    }

    return closestCand;
  }

  public void RotateCurrent() {
    if (_currentModule != null) {
      _currentModule.Rotate();
    }
  }

}
