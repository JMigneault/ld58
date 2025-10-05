using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Placer {
  public static Placer inst;

  public bool _paused = false;

  bool _lock = false;
  public Grid _grid;
  public TileHighlight[,] _highlightTiles; // Changed to a 2D array of TileHighlight

  public Module _currentModule;

  public Coord _lastHoveredCoord;

  public GameObject _parentGo;

  public Placer(Grid grid) {
    inst = this;
    _grid = grid;

    _parentGo = new GameObject("PlacerParent");
    _parentGo.transform.SetParent(_grid._parent.transform); // Parent to ship's root
    _parentGo.transform.localPosition = Vector3.zero; // Local position to match ship root
    _parentGo.transform.localRotation = Quaternion.identity; // No extra local rotation for highlight parent

    _lastHoveredCoord = new Coord(-1, -1);

    // Create the grid of tile highlights once, so we can enable and disable later as needed
    _highlightTiles = new TileHighlight[_grid._dimX, _grid._dimY];
    for (int x = 0; x < _grid._dimX; x++) {
      for (int y = 0; y < _grid._dimY; y++) {
        GameObject highlightGameObject = GameObject.Instantiate(Helpers.Prefab("TileBackground"), _parentGo.transform);
        highlightGameObject.transform.position = _grid.CoordToPosition(new Coord(x, y), Helpers._highlightZ);
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

  public void EnableHighlights() {
    for (int x = 0; x < _grid._dimX; x++) {
      for (int y = 0; y < _grid._dimY; y++) {
        if (_highlightTiles[x, y] != null && _grid.GetModule(new Coord(x, y)) == null) {
          _highlightTiles[x, y].gameObject.SetActive(true);
          _highlightTiles[x, y].SetHighlightMode(CanPlaceModule(_currentModule, _grid, new Coord(x, y)) ? HighlightMode.Valid : HighlightMode.Invalid);
        }
      }
    }
  }

  public void StartPlacing(Module module) {
    if (_lock) return;
    // Clear existing highlights before finding new ones
    StopPlacing();

    SetPaused(true);

    Helpers.Log("Placing a mod: {0}", module);
    _currentModule = module;
    _currentModule.transform.SetParent(_parentGo.transform); // Parent to Placer's parent for sync
    _currentModule.transform.localRotation = Quaternion.identity; // Module should appear unrotated relative to grid

    // Find all valid placements and highlight them
    EnableHighlights();

    module.GetComponentInChildren<TileHighlight>().SetHighlightMode(HighlightMode.Placing);

    UIController.inst.SetTooltip(module.GetTooltip());
  }

  public void StopPlacing() {
    SetPaused(false);

    UIController.inst.SetTooltip("");

    for (int x = 0; x < _grid._dimX; x++) {
      for (int y = 0; y < _grid._dimY; y++) {
        if (_highlightTiles[x, y] != null) {
          _highlightTiles[x, y].gameObject.SetActive(false);
        }
      }
    }
    if (_currentModule != null) {
      _currentModule.GetComponentInChildren<TileHighlight>().SetHighlightMode(HighlightMode.None);
      if (_currentModule._cell == null) {
        _currentModule.GetComponent<FloatingModule>().EnableFloat(true);
        _currentModule.transform.SetParent(null); // Unparent when floating again
      }
      _currentModule = null; // Also clear the current module when stopping placement
    }
  }

  public void Hover(Vector2 mousePosition) {
    if (_currentModule != null) {
      Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosition);
      mouseWorldPos.z = _grid._parent.transform.position.z; // Align Z with the ship's plane
      Coord currentHoveredCoord = _grid.PositionToCoord(new Vector2(mouseWorldPos.x, mouseWorldPos.y));

      // Determine the coordinate where the module is effectively placed
      Coord hoverCoord = new Coord(-1, -1);
      if (_grid.ValidCoord(currentHoveredCoord)) {
        Coord placementCandidate = ChoosePlacementCandidate(mousePosition, _currentModule);
        _currentModule.StopProjecting();
        if (placementCandidate.x != -1) { // A valid candidate exists
          hoverCoord = placementCandidate;
          _currentModule.ProjectingCoord(hoverCoord);
        } else { // No valid candidate, place at current hovered coord
          hoverCoord = currentHoveredCoord;
        }
        _currentModule.transform.position = _grid.CoordToPosition(hoverCoord, Helpers._modZ); // This uses the updated Grid.CoordToPosition
        _currentModule.transform.localRotation = Quaternion.identity; // Explicitly ensure local rotation is identity
      } else {
        // Completely out of grid bounds, follow raw mouse world position
        _currentModule.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, Helpers._modZ);
        _currentModule.transform.localRotation = Quaternion.identity; // Match ship's local rotation (since _currentModule is parented to _parentGo)
      }

      // Handle highlight updates only if the effective placement coordinate has changed
      if (!hoverCoord.Equals(_lastHoveredCoord)) {
        // Restore the highlight and scale for the previously hovered/placed tile
        if (_grid.ValidCoord(_lastHoveredCoord)) {
          TileHighlight lastTileHighlight = _highlightTiles[_lastHoveredCoord.x, _lastHoveredCoord.y];
          if (lastTileHighlight != null) {
            // Recalculate and restore its original highlight mode set by StartPlacing
            lastTileHighlight.Scale(false); // Make it small again
          }
        }

        _lastHoveredCoord = hoverCoord;
        if (_grid.ValidCoord(hoverCoord))
          _highlightTiles[hoverCoord.x, hoverCoord.y].Scale(true); // Make it big
      }
    } else {
      // If no module is being placed, ensure no highlight is stuck in 'Placing' mode
      if (_grid.ValidCoord(_lastHoveredCoord)) {
        TileHighlight lastTileHighlight = _highlightTiles[_lastHoveredCoord.x, _lastHoveredCoord.y];
        if (lastTileHighlight != null) {
          lastTileHighlight.SetHighlightMode(HighlightMode.None);
          lastTileHighlight.Scale(false); // Make it small if module placement stopped
        }
      }
      _lastHoveredCoord = new Coord(-1, -1);
    }
  }

  public static bool CanPlaceModule(Module mod, Grid grid, Coord coord) {
    if (mod == null)
      Helpers.Error("Null mod!");

    // 1. Initial validity: coord within bounds and empty.
    if (!grid.ValidCoord(coord) || grid.GetModule(coord) != null) {
      return false;
    }

    // connection is required
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
      Coord candidateCoord = ChoosePlacementCandidate(mousePosition, _currentModule);

      if (candidateCoord.x == -69 && !_currentModule.GetComponent<FloatingModule>().startPlacingThisFrame) { // HACK - clicked outside the grid - stop placing
        Helpers.Log("Try placing hitting hack!");
        StopPlacing();
        return false;
      }

      if (candidateCoord.y != -1) {
        // A valid placement candidate was found.
        // Now, try to add the current module to the grid at this coordinate.
        var success = _grid.AddModule(_currentModule, candidateCoord);
        if (success) {
          _currentModule.GetComponent<FloatingModule>().ReleaseSlot();
          StopPlacing();
        }
        return success;
      }

    }
    // No valid placement candidate found, or AddModule failed.
    return false;
  }

  public Coord ChoosePlacementCandidate(Vector2 mousePosition, Module module) {
    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosition);
    Coord hoveredCoord = _grid.PositionToCoord(new Vector2(mouseWorldPos.x, mouseWorldPos.y));

    int closestCandDist = 100000;
    Coord closestCand = new Coord(-1, -1);

    if (!_grid.ValidCoord(hoveredCoord))
      return new Coord(-69, -1); // huge HACK - this signals that we clicked out of the ship grid

    foreach (dir d in hoveredCoord.allDirs) {
      Coord cursor = hoveredCoord;
      int dist = 0;
      while ((d == dir.U && cursor.y >= 0) ||
             (d == dir.R && cursor.x < _grid._dimX) ||
             (d == dir.D && cursor.y < _grid._dimY) ||
             (d == dir.L && cursor.x >= 0)) {

        if (_grid.GetModule(cursor) != null)
          break; // hit a wall

        if (dist < closestCandDist && CanPlaceModule(module, _grid, cursor)) {
          closestCandDist = dist;
          closestCand = cursor;
          break;
        }

        Coord next = cursor.Neighbor(d);
        cursor = next;
        dist++;
      }
    }

    return closestCand;
  }

  public void RotateCurrent() {
    if (_currentModule != null) {
      _currentModule.Rotate();
      EnableHighlights();
    }
  }

  public void GameOver() {
    StopPlacing();
    _lock = true;
  }

  public void SetPaused(bool paused) {
    _paused = paused;
  }

}
