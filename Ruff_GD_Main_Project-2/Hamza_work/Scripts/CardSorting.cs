using Godot;
using System;

public class CardSorting : HBoxContainer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    


    public void AllCards()
    {
        GD.Print("All");
    }
    public void _on_Smarts_pressed()
    {
        GD.Print("All");
    }

    public void _on_Science_pressed()
    {
        GD.Print("Science");
    }

    public void _on_Sorcery_pressed()
    {
        GD.Print("Sorcery");
    }

    public void _on_Speed_pressed()
    {
        GD.Print("Speed");
    }

    public void _on_Defend_pressed()
    {
        GD.Print("Defend");
    }

    public void _on_Strength_pressed()
    {
        GD.Print("Strength");
    }




    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
