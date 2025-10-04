using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIBar : MonoBehaviour
{
  public float _fill = 0.0f;

  public float _bgHeight;
  public float _bgWidth;
  public float _barHeight;
  public float _barWidth;

  public Color _bgColor;
  public Color _leftColor;
  public Color _rightColor;
  public Color _highlightColor;
  public Color _glowColor;
  public float _glowWidth = 0.1f;

  private void OnValidate() {
    // This method is called in the editor when the script is loaded or a value is changed in the Inspector.
    // Use it to perform validation or update UI elements in the editor.
    // SetMaterialProps();
  }

  private void Update() {
    SetMaterialProps();
    if (Input.GetKeyDown(KeyCode.K)) {
      SetFill(0.5f);
    }
  }

  void Awake() {
    SetFill(_fill);
  }

  private void SetMaterialProps() {
    if (transform.childCount >= 2) {
      Renderer backgroundRenderer = transform.GetChild(0).GetComponent<Renderer>();
      Renderer fillRenderer = transform.GetChild(1).GetComponent<Renderer>();

      // Set _Width and _Height material props for each
      backgroundRenderer.material.SetFloat("_Width", _bgWidth);
      backgroundRenderer.material.SetFloat("_Height", _bgHeight);

      // Using _bgWidth for the fill bar's width as _barWidth is not defined.
      fillRenderer.material.SetFloat("_Width", _barWidth);
      fillRenderer.material.SetFloat("_Height", _barHeight);

      // Set Color properties
      backgroundRenderer.material.SetColor("_GradientTop", _bgColor);
      backgroundRenderer.material.SetColor("_GradientBot", _bgColor);
      backgroundRenderer.material.SetColor("_GlowColor", _glowColor);
      backgroundRenderer.material.SetFloat("_GlowWidth", _glowWidth);

      fillRenderer.material.SetColor("_LeftColor", _leftColor);
      fillRenderer.material.SetColor("_HighlightColor", _highlightColor);
      fillRenderer.material.SetColor("_RightColor", _rightColor);

    } else {
      Helpers.Error("UIBar expects at least two child GameObjects for its visual elements.");
    }
  }

  public void SetFill(float prop) {
    if (prop < 0 || prop > 1f)
      Helpers.Error("Invalid prop: {0}", prop);

    _fill = prop;

    Renderer fillRenderer = transform.GetChild(1).GetComponent<Renderer>();
    if (fillRenderer != null) {
      // Use DoTween to tween the _FilledProp to the new value
      fillRenderer.material.DOFloat(prop, "_FilledProp", 0.2f); // Tween over 0.2 seconds
    } else {
      Helpers.Error("UIBar fill renderer not found.");
    }
  }

}
