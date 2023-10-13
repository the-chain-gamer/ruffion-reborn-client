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

   
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // GD.Print("helllllooooooo");

        this.Modulate = NormalColor;
       // this.SelfModulate = NormalColor;

        //this.Modulate = Color(1,0,1,1)
        // GD.Print("helllllooooooo13123123");
        //this.Connect("pressed", this, "Pressed");

        //this.Connect("mouse_entered", this, "Hover");

        //this.Connect("mouse_exited", this, "HoverOut");

        //this.Connect("button_up", this, "Up");
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

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
