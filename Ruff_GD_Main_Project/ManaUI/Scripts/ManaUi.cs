using Godot;
using System;
using System.Collections.Generic;

public class ManaUi : Control
{
    //Array to store ManaCrystal Sprites
    private List<Sprite> ManaCrystals;
    private Texture GreyTexture;
    private Texture BlueTexture;

    public override void _Ready()
    {
        //Load Blue and Grey Textures from Resources
        GreyTexture = (Texture)GD.Load("res://ManaUI/Graphics/CrystalGrey.png");
        BlueTexture = (Texture)GD.Load("res://ManaUI/Graphics/Crystal.png");

        ManaCrystals = new List<Sprite>();

        for (int i = 0; i < 10; i++)
        {
            ManaCrystals.Add(GetChild<Sprite>(i));
        }
    }

    private async void TestCrystals()
    {
        await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
        ShowMana(6);

    }

    public void ShowMana(int mana)
    {
        Reset();
        int newMana = 10 - mana;
        GD.Print("NEw Mana = " + newMana);
        if (newMana > 0)
        {
            for (int i = 9; i >= mana; i--)
            {
                GD.Print("i = " + i);
                ManaCrystals[i].Texture = GreyTexture;
            }
        }
    }


    public void Reset()
    {
        foreach (var crystal in ManaCrystals)
        {
            crystal.Texture = BlueTexture;
        }
    }

    public void ShowZeroMana()
    {
        foreach (var crystal in ManaCrystals)
        {
            crystal.Texture = GreyTexture;
        }
    }
}
