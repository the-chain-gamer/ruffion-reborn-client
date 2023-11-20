using Godot;
using RuffGdMainProject.GridSystem;
using System;

public class ShaderTest : AnimatedSprite
{
    public Color LineColor;
    public Tween TweenObj;
    public Area2D Area2DObj;

    private Material NormalMat;
    private Material HighlightMat;

    private Material OrigonalMat;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TweenObj = GetChild<Tween>(0);
        LineColor.a = 0f;
        OrigonalMat = Material;
    }

    public void OnMouseEnter(Color newColor)
    {
        // newColor.a = 0.0f;
        LineColor = newColor;
        TweenObj.InterpolateMethod(this, "OutlineEffect",
            LineColor.a, 1.0f, 0.25f, Tween.TransitionType.Linear,
                Tween.EaseType.In, 0f);
        TweenObj.Start();
        
    }

    public void OnMouseExit()
    {
        TweenObj.InterpolateMethod(this, "OutlineEffect",
            LineColor.a, 0f, 0.25f, Tween.TransitionType.Linear,
                Tween.EaseType.Out, 0f);
        TweenObj.Start();
        
    }

    public void OutlineEffect(float value)
    {
        LineColor.a = value;
        var mat = ((ShaderMaterial)Material);
        mat.SetShaderParam("line_color", LineColor);
    }
}
