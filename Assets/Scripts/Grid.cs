using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid {
  public GameObject _parent;

  public int _dimX;
  public int _dimY;
  Cell[,] _cells;

  public Grid(int dimX, int dimY, GameObject parent) {
    _parent = parent;
    _dimX = dimX;
    _dimY = dimY;
    _cells = new Cell[dimX, dimY];
    for (int x = 0; x < dimX; x++) {
      for (int y = 0; y < dimY; y++) {
        _cells[x, y] = new Cell(new Coord(x, y));
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
        module.transform.position = CoordToPosition(coord);
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
    float startY = parentPos.y - (gridHeight / 2f) + (Module._moduleSize / 2f);
    return new Vector2(startX, startY);
  }

  public Vector2 CoordToPosition(Coord coord) {
    Vector2 originOffset = _GetGridOriginOffset();

    float posX = originOffset.x + coord.x * Module._moduleSize;
    // Invert the y-coordinate: highest world y for coord.y = 0, decreases as coord.y increases
    float posY = originOffset.y - coord.y * Module._moduleSize;

    return new Vector2(posX, posY);
  }

  public Coord PositionToCoord(Vector2 position) {
    Vector2 originOffset = _GetGridOriginOffset();

    // Calculate the coordinate by reversing the CoordToPosition logic
    int coordX = Mathf.RoundToInt((position.x - originOffset.x) / Module._moduleSize);
    // Invert the y-coordinate conversion
    int coordY = Mathf.RoundToInt((originOffset.y - position.y) / Module._moduleSize);

    return new Coord(coordX, coordY);
  }

}
