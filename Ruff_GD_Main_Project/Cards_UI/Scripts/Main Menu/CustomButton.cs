using Godot;
using System;

public class CustomButton : Godot.TextureButton
{
    [Export]
    public Color NormalColor;
    [Export]
    public Color HoverColor;
    [Export]
    public Color PressedColor;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Modulate = NormalColor;
    }

    void HoverOut()
    {
        this.Modulate = NormalColor;
    }

    void Hover()
    {
        this.Modulate = HoverColor;
    }

    void Pressed()
    {
        this.Modulate = PressedColor;
    }

    void Up()
    {
        this.Modulate = NormalColor;
    }
}
