using Godot;
using RuffGdMainProject.DataClasses;
using System;

public class CardViewer : Control
{
    //TextureRect to be used as card Img holder
    private TextureRect cardImg;

    public override void _Ready()
    {
        //get cardImg placeholder
        cardImg = GetNode<TextureRect>("CardImg");
    }

    public void ViewCard(Texture card)
    {
        Logger.UiLogger.Log(Logger.LogLevel.INFO, "in ViewCard");
        cardImg.Texture = card;
        this.Show();
    }

    public void CloseMe()
    {
        this.Hide();
    }
}
