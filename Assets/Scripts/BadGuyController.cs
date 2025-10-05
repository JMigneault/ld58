using UnityEngine;
using DG.Tweening; // Required for DOTween animations

using UnityEngine;
using DG.Tweening; // Required for DOTween animations
using System.Collections.Generic; // Added for List

public enum EnemyShipType {
  Peon,    // 1x3, Difficulty 1
  Scout,   // 2x2, Difficulty 2
  Fighter, // 2x3, Difficulty 5
  Gunship  // 3x3, Difficulty 8
}

public enum SpawnSlot {
  TopLeft,
  TopRight,
  BottomLeft,
  BottomRight,
  None = -1
}

public class ShipSpec {
  public EnemyShipType _type;
  public int _difficulty;
  public int _dimX;
  public int _dimY;
  public SpawnSlot _slot;
  public dir _facing; // Initial rotation of the ship

  public static int TTD(EnemyShipType t) {
    switch (t) {
      case EnemyShipType.Peon:
        return 1;
      case EnemyShipType.Scout:
        return 3;
      case EnemyShipType.Fighter:
        return 5;
      case EnemyShipType.Gunship:
        return 10;
      default:
        Helpers.Error("Unknown EnemyShipType: " + t);
        return 0; // Should not happen
    }
  }

  public ShipSpec(EnemyShipType type, int dimX, int dimY, SpawnSlot slot, dir facing) {
    _type = type;
    _difficulty = TTD(type);
    _dimX = dimX;
    _dimY = dimY;
    _slot = slot;
    _facing = facing;
  }
}



public class BadGuyController : MonoBehaviour {
  public static BadGuyController inst;

  public int _waveNumber = 0;
  int _difficulty = 0;
  float _timePerTick = 5;
  float _t = 0f;
  int _ticks = 0;
  int _ticksBeforeForce = 4;

  int _leftoverDifficulty = 0;

  private SpawnSlot[] _slotOccupancy = new SpawnSlot[4]; // Stores the type of ship in the slot, or None if empty
  private Vector3[] _spawnPositions = new Vector3[4];
  public List<Grid> _badGuys = new System.Collections.Generic.List<Grid>();

  void Awake() {
    inst = this;

    // Initialize slot occupancy to None
    for (int i = 0; i < _slotOccupancy.Length; i++) {
      _slotOccupancy[i] = SpawnSlot.None;
    }

    // Define fixed spawn positions for 4 slots
    // (top left, top right, bot left, bot right) relative to the player ship's area
    // These are world positions
    _spawnPositions[(int)SpawnSlot.TopLeft] = new Vector3(-5.5f, 3.0f, 0); // Example values
    _spawnPositions[(int)SpawnSlot.TopRight] = new Vector3(5.5f, 3.0f, 0);
    _spawnPositions[(int)SpawnSlot.BottomLeft] = new Vector3(-5.5f, -3.0f, 0);
    _spawnPositions[(int)SpawnSlot.BottomRight] = new Vector3(5.5f, -3.0f, 0);
  }

  private void StartNextWave() {
    _ticks = 0;
    _difficulty++;

    // Carry over any unspent difficulty from the previous wave
    int target = _difficulty + _leftoverDifficulty;
    int actual = 0;

    int shipsSpawnedThisAttempt = 0;
    while (target > actual) {
      int remainder = target - actual;

      if (AllSlotsFull()) {
        _leftoverDifficulty = target - actual;
        return;
      }

      ShipSpec shipToSpawn = ChooseShipToSpawn(remainder);
      actual += ShipSpec.TTD(shipToSpawn._type);

      SpawnSlot chosenSlot = GetAvailableSpawnSlot();

      shipToSpawn._slot = chosenSlot;

      // Create a new GameObject to serve as the root for the enemy ship.
      GameObject enemyShipRoot = GameObject.Instantiate(Helpers.Prefab("Enemy"));

      // Generate modules for the enemy ship.
      Grid enemyGrid = GenerateAShip(enemyShipRoot, shipToSpawn, chosenSlot); // Pass chosenSlot

      _badGuys.Add(enemyGrid);
      _slotOccupancy[(int)chosenSlot] = chosenSlot; // Mark slot as occupied with the actual slot value

      // Position the enemy ship:
      Vector3 targetPosition = _spawnPositions[(int)chosenSlot];

      // Initial position: slightly outside the target position for entry animation
      Vector3 startPositionOffset = Vector3.zero;
      // Animate from different directions based on slot
      if (chosenSlot == SpawnSlot.TopLeft || chosenSlot == SpawnSlot.TopRight) {
        startPositionOffset = new Vector3(0, 10f, 0); // Come from above
      } else {
        startPositionOffset = new Vector3(0, -10f, 0); // Come from below
      }
      Vector3 startPosition = targetPosition + startPositionOffset;

      // Set initial position
      enemyShipRoot.transform.position = startPosition;

      // Tween the enemy ship into its target position
      enemyShipRoot.transform.DOMove(targetPosition, 1.5f) // Tween duration 1.5 seconds
                   .SetEase(Ease.OutBack) // Use an easing function for a nice effect
                   .SetLink(enemyShipRoot); // Link to GameObject for automatic killing

    }


  }

  // Called from a MonoBehaviour's Update or similar
  public void GameUpdate() {
    if (Placer.inst._paused) return;

    _t -= Time.deltaTime;
    if (_t <= 0) {
      _t = _timePerTick;
      Tick();
    }

  }
  
  void Tick() {
    _ticks++;

    bool allSlotsEmpty = AllSlotsEmpty();

    if (allSlotsEmpty || _ticks > _ticksBeforeForce) {
      StartNextWave();
    }
  }

  private ShipSpec ChooseShipToSpawn(float neededDifficulty) {
    // Prioritize spawning harder ships if neededDifficulty is high, otherwise easier ones.
    // The order is Peon, Scout, Fighter, Gunship (difficulty 1, 3, 5, 10)

    // Consider ships from hardest to easiest that fit the needed difficulty
    if (neededDifficulty >= ShipSpec.TTD(EnemyShipType.Gunship)) { // Gunship
      return new ShipSpec(EnemyShipType.Gunship, 3, 3, SpawnSlot.None, dir.L); // Dummy slot, facing
    }
    if (neededDifficulty >= ShipSpec.TTD(EnemyShipType.Fighter)) { // Fighter
      return new ShipSpec(EnemyShipType.Fighter, 2, 3, SpawnSlot.None, dir.L);
    }
    if (neededDifficulty >= ShipSpec.TTD(EnemyShipType.Scout)) { // Scout
      return new ShipSpec(EnemyShipType.Scout, 2, 2, SpawnSlot.None, dir.L);
    }
    if (neededDifficulty >= ShipSpec.TTD(EnemyShipType.Peon)) { // Peon
      return new ShipSpec(EnemyShipType.Peon, 1, 3, SpawnSlot.None, dir.L);
    }
    return null; // No suitable ship to spawn
  }

  private SpawnSlot GetAvailableSpawnSlot() {
    List<SpawnSlot> availableSlots = new List<SpawnSlot>();
    for (int i = 0; i < _slotOccupancy.Length; i++) {
      if (_slotOccupancy[i] == SpawnSlot.None) {
        availableSlots.Add((SpawnSlot)i);
      }
    }

    if (availableSlots.Count > 0) {
      return availableSlots[Random.Range(0, availableSlots.Count)];
    }
    return SpawnSlot.None;
  }

  private bool AllSlotsEmpty() {
    for (int i = 0; i < _slotOccupancy.Length; i++) {
      if (_slotOccupancy[i] != SpawnSlot.None) {
        return false;
      }
    }
    return true;
  }

  private bool AllSlotsFull() {
    for (int i = 0; i < _slotOccupancy.Length; i++) {
      if (_slotOccupancy[i] == SpawnSlot.None) {
        return false;
      }
    }
    return true;
  }

  Grid GenerateAShip(GameObject go, ShipSpec spec, SpawnSlot slot) {
    switch (spec._type) {
      case EnemyShipType.Peon:
        return GeneratePeon(go, spec, slot);
      case EnemyShipType.Scout:
        return GenerateScout(go, spec, slot);
      case EnemyShipType.Fighter:
        return GenerateFighter(go, spec, slot);
      case EnemyShipType.Gunship:
        return GenerateGunship(go, spec, slot);
      default:
        Helpers.Error("GenerateAShip: Unknown enemy ship type: " + spec._type);
        return GenerateScout(go, spec, slot); // Fallback to scout
    }
  }

  // 1x3 ship. Very basic.
  Grid GeneratePeon(GameObject go, ShipSpec spec, SpawnSlot slot) {
    go.GetComponent<ShipSizer>().Size(spec._dimX, spec._dimY);
    Grid ship = new Grid(spec._dimX, spec._dimY, go, false, slot); // Pass spawnSlot to Grid constructor

    Module coreModule = Module.MakeModule(new ModuleSpec(ModuleType.Core));
    Coord cc = new Coord(0, 0); // Core at (0,0)
    ship.AddModule(coreModule, cc);

    Module energy = Module.MakeModule(new ModuleSpec(ModuleType.Energy));
    ship.AddModule(energy, cc.Neighbor(dir.D));

    Module weaponModule = Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L));
    ship.AddModule(weaponModule, cc.Neighbor(dir.D).Neighbor(dir.D)); // Weapon below core

    Helpers.Log("BadGuyController: Generated a Peon ship.");
    return ship;
  }


  // 2x2 ship. One gun and connectors.
  Grid GenerateScout(GameObject go, ShipSpec spec, SpawnSlot slot) {
    go.GetComponent<ShipSizer>().Size(spec._dimX, spec._dimY);
    Grid ship = new Grid(spec._dimX, spec._dimY, go, false, slot); // Pass spawnSlot to Grid constructor

    Module coreModule = Module.MakeModule(new ModuleSpec(ModuleType.Core));
    Coord cc = new Coord(1, 1);
    ship.AddModule(coreModule, cc);

    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), cc.Neighbor(dir.L)); // Protrusion left
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), cc.Neighbor(dir.L).Neighbor(dir.U)); // Protrusion right

    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Energy)), cc.Neighbor(dir.U));

    return ship;
  }

  // 2x3 ship. Two guns, shields, and energy.
  Grid GenerateFighter(GameObject go, ShipSpec spec, SpawnSlot slot) {
    go.GetComponent<ShipSizer>().Size(spec._dimX, spec._dimY);
    Grid ship = new Grid(spec._dimX, spec._dimY, go, false, slot); // Pass spawnSlot to Grid constructor

    Module coreModule = Module.MakeModule(new ModuleSpec(ModuleType.Core));
    Coord cc = new Coord(1, 1);
    ship.AddModule(coreModule, cc);

    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), cc.Neighbor(dir.L)); // Protrusion left
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), cc.Neighbor(dir.L).Neighbor(dir.D)); // Protrusion right

    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Shield)), cc.Neighbor(dir.U));
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Energy)), cc.Neighbor(dir.U).Neighbor(dir.L));
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Energy)), cc.Neighbor(dir.D));

    return ship;
  }

  // 3x3 ship. Multiple guns, shields, energy, and engines.
  Grid GenerateGunship(GameObject go, ShipSpec spec, SpawnSlot slot) {
    go.GetComponent<ShipSizer>().Size(spec._dimX, spec._dimY);
    Grid ship = new Grid(spec._dimX, spec._dimY, go, false, slot); // Pass spawnSlot to Grid constructor

    Module coreModule = Module.MakeModule(new ModuleSpec(ModuleType.Core));
    Coord cc = new Coord(2, 1); // Place core offset for larger ships
    ship.AddModule(coreModule, cc);

    // Weapons
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), new Coord(0, 0)); // Protrusion left
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), new Coord(0, 1)); // Protrusion right
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Weapon, dir.L)), new Coord(0, 2)); // Protrusion up

    // Shields and Energy
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Shield)), new Coord(1, 0));
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Shield)), new Coord(1, 2));

    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Energy)), new Coord(1, 1));
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Energy)), new Coord(2, 0));
    ship.AddModule(Module.MakeModule(new ModuleSpec(ModuleType.Energy)), new Coord(2, 2));

    Helpers.Log("BadGuyController: Generated a Gunship.");
    return ship;
  }

  public void BadGuyDied(Grid guy) {
    _badGuys.Remove(guy);

    // Free the specific slot that this bad guy occupied
    if (guy._spawnSlot != SpawnSlot.None) {
      _slotOccupancy[(int)guy._spawnSlot] = SpawnSlot.None;
    } else {
      Helpers.Error("BadGuyDied: Died ship had no valid spawn slot assigned.");
    }

    UIController.inst.IncrEnemiesDestroyed();
  }

  public void GameOver() {
    // Stop spawning new enemies
    // Make existing bad guys fly away or destroy them
    foreach (Grid badGuyGrid in _badGuys) {
      if (badGuyGrid._parent != null) {
        // Tween them off screen, or just destroy them
        badGuyGrid._parent.transform.DOMoveY(20f, 3.0f).SetEase(Ease.InBack).OnComplete(() => {
          UnityEngine.Object.Destroy(badGuyGrid._parent);
        });
      }
    }
    _badGuys.Clear(); // Clear the list after queuing them to fly away

    // Reset difficulty tracking and slots (already done by resetting wave vars)
    for (int i = 0; i < _slotOccupancy.Length; i++) {
      _slotOccupancy[i] = SpawnSlot.None;
    }
  }

}
