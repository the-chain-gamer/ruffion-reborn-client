using Godot;
using System;

namespace RuffGdMainProject.UiSystem
{
    public class TeamGridItemUI : Control
    {
        public int characterId;
        public void SetValue(string _name, string _PlayerPicture)
        {
            GetNode<Label>("Name").Text = _name;

            var texture = GD.Load(_PlayerPicture);
            GetNode("Frame").GetChild<TextureRect>(0).Texture = (Texture)texture;

            // if (_name == "Beagle")
            // {
            //     GetNode("Frame").GetChild<TextureRect>(0).SetScale(new Vector2(0.6f, 0.7f));
            //     GetNode("Frame").GetChild<TextureRect>(0).SetPosition(new Vector2(2f, 10f));
            // }
            // if (_name == "Shiba")
            // {
            //     GetNode("Frame").GetChild<TextureRect>(0).SetScale(new Vector2(0.55f, 0.6f));
            //     GetNode("Frame").GetChild<TextureRect>(0).SetPosition(new Vector2(8f, 13f));
            // }
            // if (_name == "Golden")
            // {
            //     GetNode("Frame").GetChild<TextureRect>(0).SetScale(new Vector2(0.6f, 0.6f));
            //     GetNode("Frame").GetChild<TextureRect>(0).SetPosition(new Vector2(3f, 10f));
            // }
            // if (_name == "Mastiff")
            // {
            //     GetNode("Frame").GetChild<TextureRect>(0).SetScale(new Vector2(0.18f, 0.2f));
            //     GetNode("Frame").GetChild<TextureRect>(0).SetPosition(new Vector2(-10f, 0f));
            // }
        }

        public void setCharacterId(int cId)
        {
            characterId = cId;
        }

        public int GetCharcterId()
        {
            return characterId;
        }

        public void _on_TeamCharacter_pressed()//Button pressed
        {
            //GD.Print("team character pressed");
            //CharacterSelectionManager.instance.ShowCharacterBody(characterId); //To show Character Body


            //GD.Print("Script name is:"+ GetTree().Root.GetNode<CharacterSelectionManager>("PlayerSelectionControlNode").Name);
            CharacterSelectionManager.instance.RemoveCharacterFromGrid(characterId);
            // GetTree().Root.GetNode<CharacterSelectionManager>("PlayerSelectionControlNode").RemoveCharacterFromGrid(characterId);
            // GetParent<CharacterSelectionManager>().RemoveCharacterFromGrid(characterId);
            GD.Print("Button Pressed");
            GD.Print("Node" + GetNode<Control>("TextureButton").GetParent().Name);
            // TeamGridItemUI test = GetNode<Control>("TextureButton").GetParent().GetScript();
            GetNode<Control>("TextureButton").GetParent().QueueFree();
        }

        public void _on_TextureButton_pressed()//Cross Button
        {
            //GD.Print("Script name is:"+ GetTree().Root.GetNode<CharacterSelectionManager>("PlayerSelectionControlNode").Name);
            CharacterSelectionManager.instance.RemoveCharacterFromGrid(characterId);
            // GetTree().Root.GetNode<CharacterSelectionManager>("PlayerSelectionControlNode").RemoveCharacterFromGrid(characterId);
            // GetParent<CharacterSelectionManager>().RemoveCharacterFromGrid(characterId);
            GD.Print("Button Pressed");
            GD.Print("Node" + GetNode<Control>("TextureButton").GetParent().Name);
            // TeamGridItemUI test = GetNode<Control>("TextureButton").GetParent().GetScript();
            GetNode<Control>("TextureButton").GetParent().QueueFree();

        }




    }
}