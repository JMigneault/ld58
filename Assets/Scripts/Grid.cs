using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Grid {
  public GameObject _parent;

  Cell[,] _cells;

  public Grid(int dimX, int dimY, GameObject parent) {
    _parent = parent;
    _cells = new Cell[dimX, dimY];
    for (int x = 0; x < dimX; x++) {
      for (int y = 0; y < dimY; y++) {
        _cells[x, y] = new Cell(new Coord(x, y));
      }
    }
  }

  public bool ValidCoord(Coord coord) {
    return coord.x >= 0 && coord.x < _cells.GetLength(0) &&
           coord.y >= 0 && coord.y < _cells.GetLength(1);
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
        module.transform.position = GetCoordPosition(coord);
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

  public Vector2 GetCoordPosition(Coord coord) {
    float gridWidth = _cells.GetLength(0) * Module._moduleSize;
    float gridHeight = _cells.GetLength(1) * Module._moduleSize;

    Vector2 parentPos = _parent.transform.position;
    float startX = parentPos.x - (gridWidth / 2f) + (Module._moduleSize / 2f);
    float startY = parentPos.y - (gridHeight / 2f) + (Module._moduleSize / 2f);

    float posX = startX + coord.x * Module._moduleSize;
    float posY = startY + coord.y * Module._moduleSize;

    Helpers.Log("Chose position {0} for coord {1}", new Vector2(posX, posY), coord);

    return new Vector2(posX, posY);
  }

}
