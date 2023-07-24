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


            // GetNode("TextureButton").GetNode<Label>("Name").Text = data.CardName;
            // GetNode("TextureButton").GetNode<Label>("Description").Text = data.CardDescription;
            var texture = GD.Load(data.CardImagePath);
            GetNode("TextureButton").GetNode<TextureRect>("TextureRect").Texture = (Texture)texture;
        }

        public void SetShortCard(CardData data)
        {
            CardID = data.CardID;
            GetNode("TextureButton").GetNode<Label>("Name").Text = data.CardName;
            GetNode("TextureButton").GetNode<Label>("Description").Text = data.CardDescription;
            var texture = GD.Load("res://Hamza_work/New UI/Ruffion Reborn (4)/Group 144.png");
            GetNode("TextureButton").GetNode<TextureRect>("TextureRect").Texture = (Texture)texture;
        }

        public void _on_TextureButton_pressed()
        {
            SoundManager.Instance.PlaySoundByName("ButtonSound2");
            CardSelectionManager.instance.RemoveFromDeck(CardID);
            GetNode<Control>("TextureButton").GetParent().QueueFree();
        }

        //CharacterSelectionManager.instance.RemoveCharacterFromGrid(characterId);
        // GetTree().Root.GetNode<CharacterSelectionManager>("PlayerSelectionControlNode").RemoveCharacterFromGrid(characterId);
        // GetParent<CharacterSelectionManager>().RemoveCharacterFromGrid(characterId);
        //   GD.Print("Button Pressed");
        // GD.Print("Node" + GetNode<Control>("TextureButton").GetParent().Name);
        // TeamGridItemUI test = GetNode<Control>("TextureButton").GetParent().GetScript();
        // GetNode<Control>("TextureButton").GetParent().QueueFree();
        //

    }
}