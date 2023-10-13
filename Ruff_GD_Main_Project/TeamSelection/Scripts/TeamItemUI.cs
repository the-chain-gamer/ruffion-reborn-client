using Godot;
using System;

namespace RuffGdMainProject.UiSystem
{
    public class TeamItemUI : Control
    {
        public int characterId;
        public void SetValue(string _name, string _PlayerPicture)
        {
            GetNode<Label>("Name").Text = _name;
            var texture = GD.Load(_PlayerPicture);
            GetNode("Frame").GetChild<TextureRect>(0).Texture = (Texture)texture;
        }

        public void SetValue(string _name, Texture _PlayerPicture)
        {
            GetNode<Label>("Name").Text = _name;
            GetNode("Frame").GetChild<TextureRect>(0).Texture = (Texture)_PlayerPicture;
        }

        public void setCharacterId(int cId)
        {
            characterId = cId;
        }
    }
}