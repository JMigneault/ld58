using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Init {
  public static Init _inst;

  public Init() {
    _inst = this;

    var ship = GameObject.Instantiate(Helpers.Prefab("Ship"));

    Grid grid = new Grid(5, 5, ship);
    grid.AddModule(Module.MakeModule(ModuleType.Core), new Coord(2, 2));
    grid.AddModule(Module.MakeModule(ModuleType.Connection), new Coord(1, 2));
    grid.AddModule(Module.MakeModule(ModuleType.Connection), new Coord(3, 2));
    grid.AddModule(Module.MakeModule(ModuleType.Connection), new Coord(3, 3));

  }

}
