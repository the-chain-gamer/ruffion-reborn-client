using Godot;
using System;

namespace RuffGdMainProject.GridSystem
{
    public class UnitInfo : Control
    {
        private TextureRect profilePic;
        public override void _Ready()
        {
            profilePic = GetNode<TextureRect>("ProfilePic");
        }

        public void UpdateUnitPic(string picPath)
        {
            profilePic.Texture = (Texture)GD.Load(picPath);
        }

    }
}
