using UnityEngine;

// Resizes and recolors GOs for different dimension ships.

public class ShipSizer : MonoBehaviour {
  public Color _outerColorTop;
  public Color _outerColorBot;
  public Color _glowColor;

  public void Awake() {
    if (transform.childCount > 0) {
      Renderer childRenderer = transform.GetChild(1).GetComponent<Renderer>();
      if (childRenderer != null && childRenderer.material != null) {
        childRenderer.material.SetColor("_GradientTop", _outerColorTop);
        childRenderer.material.SetColor("_GradientBottom", _outerColorBot);
        childRenderer.material.SetColor("_GlowColor", _glowColor);
      } else {
        Helpers.Error("ShipSizer: Second child does not have a Renderer or its material is null.");
      }
    } else {
      Helpers.Error("ShipSizer: No children found to apply material properties to.");
    }
  }

  public void Size(int x, int y) {
    int maxSize = 5;

    float propX = x / (1.0f * maxSize);
    float propY = y / (1.0f * maxSize);

    transform.localScale = new Vector3(propX, propY, 1.0f);
  }
  

}
