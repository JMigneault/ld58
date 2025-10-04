
using UnityEngine;

public enum HighlightMode {
  Attention,
  Valid,
  Invalid,
  Placing, // New highlight mode for placing
  None
}

public class TileHighlight : MonoBehaviour {
  [Header("Highlight Colors - Attention")]
  public Color _attentionBorderColor = Color.yellow;
  public Color _attentionColor = new Color(1f, 1f, 0f, 0.2f); // Yellow with transparency

  [Header("Highlight Colors - Valid")]
  public Color _validBorderColor = Color.green;
  public Color _validColor = new Color(0f, 1f, 0f, 0.2f); // Green with transparency

  [Header("Highlight Colors - Invalid")]
  public Color _invalidBorderColor = Color.red;
  public Color _invalidColor = new Color(1f, 0f, 0f, 0.2f); // Red with transparency

  [Header("Highlight Colors - Placing")]
  public Color _placingBorderColor = Color.blue; // Example color
  public Color _placingColor = new Color(0f, 0f, 1f, 0.2f); // Example color, blue with transparency

  [Header("Highlight Colors - None")]
  public Color _noneBorderColor = Color.clear;
  public Color _noneColor = Color.clear;

  private Renderer _renderer;
  private Material _materialInstance;

  void Awake() {
    _renderer = GetComponent<Renderer>();
    if (_renderer == null) {
      Helpers.Error("TileHighlight requires a Renderer component.");
      return;
    }
    // Create a material instance to avoid modifying the shared material asset
    _materialInstance = _renderer.material;
    SetHighlightMode(HighlightMode.None);
  }

  public void SetHighlightMode(HighlightMode mode) {
    Color borderColor;
    Color fillColor;

    switch (mode) {
      case HighlightMode.Attention:
        borderColor = _attentionBorderColor;
        fillColor = _attentionColor;
        break;
      case HighlightMode.Valid:
        borderColor = _validBorderColor;
        fillColor = _validColor;
        break;
      case HighlightMode.Invalid:
        borderColor = _invalidBorderColor;
        fillColor = _invalidColor;
        break;
      case HighlightMode.Placing:
        borderColor = _placingBorderColor;
        fillColor = _placingColor;
        break;
      case HighlightMode.None:
      default:
        borderColor = _noneBorderColor;
        fillColor = _noneColor;
        break;
    }

    if (_materialInstance != null) {
      _materialInstance.SetColor("_BorderColor", borderColor);
      _materialInstance.SetColor("_Color", fillColor);
    }
  }

  public void Scale(bool big) {
    transform.localScale = big ? Vector3.one * 1.05f : Vector3.one;
  }
}
