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

    // Apply scaling to the first two children.
    var c0s = transform.GetChild(0).localScale;
    transform.GetChild(0).localScale = new Vector3(c0s.x * propX, c0s.y * propY, 1.0f);
    var c1s = transform.GetChild(1).localScale;
    transform.GetChild(1).localScale = new Vector3(c1s.x * propX, c1s.y * propY, 1.0f);

    // Ensure the root object (which is the Grid's parent) remains unscaled for correct grid calculations.
    // This line effectively resets the root scale to 1, ensuring Module._moduleSize acts as a world unit.
    transform.localScale = Vector3.one;
  }
  

}
