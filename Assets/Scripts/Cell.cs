using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cell {
  public Coord _coord;
  public Module _module;
  public Grid _grid;

  public Module Module {
    get { return _module; }
    set { _module = value; }
  }

  public Cell(Coord coord, Grid grid) {
    _coord = coord;
    _grid = grid;
  }
}
