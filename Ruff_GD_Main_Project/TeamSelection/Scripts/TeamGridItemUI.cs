using Godot;
using System;
using RuffGdMainProject.DataClasses;

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
        }

        public void setCharacterId(int cId)
        {
            characterId = cId;
        }

        public int GetCharcterId()
        {
            return characterId;
        }

        public void SetCharcterStats(int DogId, string CharacterName)
        {
            UnitData search = GlobalData.GD.CharacterList.Find(x => x.UnitID.Equals(Convert.ToInt32(DogId)));
            int strength = search.UnitStats.Strength;
            int size = search.UnitStats.Size;
            int speed = search.UnitStats.Speed;
            int smart = search.UnitStats.Smarts;
            int sorcery = search.UnitStats.Sorcery;
            int science = search.UnitStats.Science;

            this.GetNode<Control>("StatsControl").GetChild<TextureProgress>(0).Value = strength;
            this.GetNode<Control>("StatsControl").GetChild<TextureProgress>(1).Value = size;
            this.GetNode<Control>("StatsControl").GetChild<TextureProgress>(2).Value = speed;
            this.GetNode<Control>("StatsControl").GetChild<TextureProgress>(3).Value = science;
            this.GetNode<Control>("StatsControl").GetChild<TextureProgress>(4).Value = sorcery;
            this.GetNode<Control>("StatsControl").GetChild<TextureProgress>(5).Value = smart;

        }

        public void _on_TeamCharacter_pressed()//Button pressed
        {
            CharacterSelectionManager.instance.RemoveCharacterFromGrid(characterId);
            GetNode<Control>("TextureButton").GetParent().QueueFree();
        }

        public void _on_TextureButton_pressed()//Cross Button
        {
            CharacterSelectionManager.instance.RemoveCharacterFromGrid(characterId);
            GetNode<Control>("TextureButton").GetParent().QueueFree();
        }
    }
}