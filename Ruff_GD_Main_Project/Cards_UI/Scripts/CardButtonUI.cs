using Godot;
using RuffGdMainProject.DataClasses;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.UiSystem
{
    public class CardButtonUI : Control
    {

        public int CardID;

        public CardSubType CardCategory;


        public void SetFullCard(CardData data)
        {
            CardID = data.CardID;

            CardCategory = data.CardSubType;


            var texture = GD.Load(data.CardImagePath);
            GetNode("TextureButton").GetNode<TextureRect>("TextureRect").Texture = (Texture)texture;
        }

        public void SetShortCard(CardData data)
        {
            CardID = data.CardID;
            GetNode("TextureButton").GetNode<Label>("Name").Text = data.CardName;
            GetNode("TextureButton").GetNode<Label>("Description").Text = data.CardDescription;
            var texture = GD.Load("res://Cards_UI/New UI/Ruffion Reborn (4)/Group 144.png");
            GetNode("TextureButton").GetNode<TextureRect>("TextureRect").Texture = (Texture)texture;
        }

        public void _on_TextureButton1_pressed()
        {
            GD.Print("Button Pressed");
            SoundManager.Instance.PlaySoundByName("ButtonSound2");
            CardSelectionManager.instance.RemoveFromDeck(CardID);
            GetNode<Control>("TextureButton").GetParent().QueueFree();
        }


        public void _on_TextureButton_pressed()
        {
            GD.Print("Entered on Button");
            SoundManager.Instance.PlaySoundByName("ButtonSound2");
            //  GetNode<Control>("TextureButton").RectScale = new Vector2(0.5f, 0.5f);
            CardSelectionManager.instance.CardZoomIn(CardID);
        }

        
    }
}