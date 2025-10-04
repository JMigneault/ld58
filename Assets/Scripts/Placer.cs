
using UnityEngine;


public class Placer {
  public static Placer inst;
  public Grid _grid;

  public Module _currentModule;

  public Placer(Grid grid) {
    inst = this;
    _grid = grid;
  }

  public void Hover(Vector2 mousePosition) {
    if (_currentModule != null) {
      Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosition);
      Coord hoveredCoord = _grid.PositionToCoord(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
      
      // If the hovered coord is valid, snap the module to its center
      if (_grid.ValidCoord(hoveredCoord)) {
        _currentModule.transform.position = _grid.CoordToPosition(hoveredCoord);
      } else {
        // If not on a valid grid coordinate, just follow the raw mouse world position (but keep z at 0)
        _currentModule.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
      }
    }
  }

  public bool TryPlacing(Vector2 mousePosition) {
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

  public void SetCurrentModule(Module module) {
    _currentModule = module;
  }

}
