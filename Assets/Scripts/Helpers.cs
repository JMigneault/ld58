using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Helpers {

  public static GameObject Prefab(string name) {
    string path = "prefabs/" + name;
    var res = Resources.Load<GameObject>(path);
    if (res == null) {
      Error("Failed to find prefab resource {0}", path);
    }
    return res;
  }

  public static Sprite Sprite(string name) {
    string path = "sprites/" + name;
    var res = Resources.Load<Sprite>(path);
    if (res == null) {
      Error("Failed to find sprite resource {0}", path);
    }
    return res;
  }

  public static void Log(string message, params object[] args) {
    string toPrint = String.Format(message, args);
    Debug.Log(toPrint);
  }

  public static void Error(string message, params object[] args) {
    string toPrint = String.Format(message, args);
    Debug.LogError(toPrint);
  }

}
