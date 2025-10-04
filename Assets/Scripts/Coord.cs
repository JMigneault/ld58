using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum dir {
  U, R, D, L
}

public class Coord {
  public dir[] allDirs = {dir.U, dir.R, dir.D, dir.L};
  public int x;
  public int y;

  public Coord(int x, int y) { this.x = x; this.y = y; }

  public Coord Neighbor(dir d) {
    int nx = x;
    int ny = y;
    switch (d) {
      case dir.R:
        nx++;
        break;
      case dir.L:
        nx--;
        break;
      case dir.U:
        ny--;
        break;
      case dir.D:
        ny++;
        break;
    }
    return new Coord(nx, ny);
  }

  public static dir OppDir(dir d) {
    switch (d) {
      case dir.R:
        return dir.L;
      case dir.L:
        return dir.R;
      case dir.U:
        return dir.D;
      case dir.D:
        return dir.U;
    }

    Helpers.Error("OppDir() got bad dir");
    return dir.R;
  }

  public static dir Turn90Degrees(dir toTurn) {
    switch (toTurn) {
      case dir.R:
        return dir.D;
      case dir.L:
        return dir.U;
      case dir.U:
        return dir.R;
      case dir.D:
        return dir.L;
    }

    return dir.U;
  }

  public static dir Turn(dir toTurn, int numTurns) {
    int turns = numTurns % 4;

    dir toRet = toTurn;
    for (int i = 0; i < turns; i++) {
      toRet = Turn90Degrees(toRet);
    }

    return toRet;
  }

  // Note: may return either of the dirs for diagonally placed coords
  public static dir DirTo(Coord ths, Coord that) {
    if (that.x > ths.x) {
      return dir.R;
    }
    if (that.x < ths.x) {
      return dir.L;
    }
    if (that.y < ths.y) {
      return dir.U;
    }
    if (that.y > ths.y) {
      return dir.D;
    }
    return dir.R;
  }

  public static int Dist(Coord ths, Coord that) {
    return ths.Dist(that);
  }
  public int Dist(Coord that) {
    return Mathf.Abs(x - that.x) + Mathf.Abs(y - that.y);
  }

  public static bool Equals(Coord ths, Coord that) {
    return ths.Equals(that);
  }

  public bool Equals(Coord that) {
    return (x == that.x) && (y == that.y);
  }

  public override bool Equals(object obj) {
    var that = obj as Coord;
    if (that == null) return false;
    return this.Equals(that);
  }

  public override int GetHashCode() {
    return HashCode.Combine(x, y);
  }

  public override string ToString() {
    return "Coord(" + x + ", " + y + ")";
  }

  public static Coord operator +(Coord a, Coord b) {
    return new Coord(a.x + b.x, a.y + b.y);
  }

  public static Coord operator -(Coord a, Coord b) {
    return new Coord(a.x - b.x, a.y - b.y);
  }

  public static string Name(dir d) {
    switch (d) {
      case dir.U:
        return "up";
      case dir.R:
        return "right";
      case dir.D:
        return "down";
      case dir.L:
        return "left";
    }
    return "INVALID DIR";
  }

}
