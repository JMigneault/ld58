using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Init {
  public static Init _inst;
  public Grid _grid;

  public GameObject _globalScripts;

  public Init() {
    _inst = this;

    _globalScripts = new GameObject("GlobalScripts");
    var inputHandler = _globalScripts.AddComponent<InputHandler>();
    var timeLord = _globalScripts.AddComponent<TimeLord>();

    var ship = GameObject.Instantiate(Helpers.Prefab("Ship"));

    _grid = new Grid(5, 5, ship);
    _grid.AddModule(Module.MakeModule(ModuleType.Core), new Coord(2, 2));

    Placer placer = new Placer(_grid);

    var fmg = new FloatingModuleGenerator();
    fmg.GenerateFloater();
    fmg.GenerateFloater();
    fmg.GenerateFloater();
  }

}
