using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cell {
  Coord _coord;
  Module _module;

  public Module Module {
    get { return _module; }
    set { _module = value; }
  }

  public Cell(Coord coord) {
    _coord = coord;
  }
}
