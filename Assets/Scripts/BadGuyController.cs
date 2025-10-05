using UnityEngine;
using DG.Tweening; // Required for DOTween animations

public class ShipSpec {
  // TODO: size, difficulty, facing, etc
}

public class BadGuyController {
  public static BadGuyController inst;

  bool _rightFull = false;
  bool _leftFull = false;
  private System.Collections.Generic.List<Grid> _badGuys = new System.Collections.Generic.List<Grid>();

  public BadGuyController() {
    inst = this;
  }

  public Grid Spawn() {
    if (_leftFull)
      return null;
    _leftFull = true;

    // Create a new GameObject to serve as the root for the enemy ship.
    // This allows us to move and tween the entire ship.
    GameObject enemyShipRoot = GameObject.Instantiate(Helpers.Prefab("Enemy"));

    // Generate modules for the enemy ship.
    Grid enemyGrid = GenerateAShip(enemyShipRoot);

    _badGuys.Add(enemyGrid);

    // Position the enemy ship:
    // 1. To the right of the player's ship.
    // 2. Initial position below the screen, then tween up.

    // Get player ship's grid reference for positioning
    Grid playerGrid = Init._inst._grid;
    GameObject playerShipRoot = playerGrid._parent;

    Vector3 targetPosition = new Vector3(5.5f, 0f, 0);

    // Initial position: below the target position (off-screen)
    Vector3 startPosition = targetPosition + new Vector3(0, -10f, 0); // 10 units below

    // Set initial position
    enemyShipRoot.transform.position = startPosition;

    // Tween the enemy ship into its target position
    enemyShipRoot.transform.DOMove(targetPosition, 1.5f) // Tween duration 1.5 seconds
                 .SetEase(Ease.OutBack) // Use an easing function for a nice effect
                 .SetLink(enemyShipRoot); // Link to GameObject for automatic killing

    Helpers.Log("BadGuyController: Spawned enemy ship at {0}", targetPosition);

    return enemyGrid;
  }

  Grid GenerateAShip(GameObject go) {
    return GenerateScout(go);
  }

  // 2x3 ship. One gun and connectors.
  Grid GenerateScout(GameObject go) {
    go.GetComponent<ShipSizer>().Size(2, 3);

    Grid ship = new Grid(2, 3, go);

    Module coreModule = Module.MakeModule(new ModuleSpec(ModuleType.Core));
    Coord cc = new Coord(1, 1);
    ship.AddModule(coreModule, cc);

    // Add some connection modules around the core
    Module gun = Module.MakeModule(new ModuleSpec(ModuleType.Weapon, new bool[] {true,true,true,false}, dir.L));
    ship.AddModule(gun, cc.Neighbor(dir.L));

    // Add some connection modules around the core
    Module power = Module.MakeModule(new ModuleSpec(ModuleType.Energy, new bool[] {true,true,true,true}));
    ship.AddModule(power, cc.Neighbor(dir.U));

    var connSpec = new ModuleSpec(ModuleType.Connection);
    ship.AddModule(Module.MakeModule(connSpec), new Coord(0, 0));
    ship.AddModule(Module.MakeModule(connSpec), new Coord(0, 2));
    ship.AddModule(Module.MakeModule(connSpec), new Coord(1, 2));

    Helpers.Log("BadGuyController: Generated a scout ship.");

    return ship;
  }

  public void BadGuyDied(Grid guy) {
    _badGuys.Remove(guy);
  }

  public void GameOver() {
    // TODO: bad guys fly away, don't spawn any more
  }

}
