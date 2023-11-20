using Godot;
using System;

namespace RuffGdMainProject.GridSystem
{
    public class PlayerInfo : Control
    {
        private Texture activeBg, inactiveBg, activeFrame, inactiveFrame;
        private string activeTxtColor = "d8b83c";
        private string inactiveTxtColor = "3cd8cd";
        private TextureRect nameBg;
        private TextureRect frameBg;
        private TextureRect profilePic;
        private TextureRect turnHighlight;
        private Label pNameLbl;
        private TextureRect spark;
        private ProgressBar SelectedUnitHpBar;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            activeBg = (Texture)GD.Load("res://GridSystem/Assets/Gameplay/Group 108.png");
            inactiveBg = (Texture)GD.Load("res://GridSystem/Assets/Gameplay/Group 107.png");
            activeFrame = (Texture)GD.Load("res://GridSystem/Assets/Gameplay/Avatar.png");
            inactiveFrame = (Texture)GD.Load("res://GridSystem/Assets/Gameplay/Avatar (1).png");
            nameBg = GetNode<TextureRect>("NameBg");
            frameBg = GetNode<TextureRect>("ProfileBg");
            pNameLbl = GetNode<Label>("NameBg/PLabel");
            turnHighlight = GetNode<TextureRect>("TurnHighlight");
            profilePic = GetNode<TextureRect>("ProfileBg/ProfilePic");
            spark = GetNode<TextureRect>("Spark");
            spark.Hide();
            turnHighlight.Hide();
            SelectedUnitHpBar = GetNode<ProgressBar>("UnitProgressBar");
        }

        public void UpdatePic(string picPath)
        {
            profilePic.Texture = (Texture)GD.Load(picPath);
        }



        public void Highlight(bool isMyTurn)
        {
            if (isMyTurn)
            {
                spark.Show();
                turnHighlight.Show();
                nameBg.Texture = activeBg;
                frameBg.Texture = activeFrame;
                pNameLbl.SelfModulate = new Color(activeTxtColor);
            }
            else
            {
                spark.Hide();
                turnHighlight.Hide();
                nameBg.Texture = inactiveBg;
                frameBg.Texture = inactiveFrame;
                pNameLbl.SelfModulate = new Color(inactiveTxtColor);
            }
        }

        public void SetPlayerName(string nm)
        {
            pNameLbl.Text = nm;
        }

        public void ShowUnitVitals(Unit unit)
        {
            SelectedUnitHpBar.MaxValue = unit.TotalHitPoints;
            SelectedUnitHpBar.Value = unit.HitPoints;

           if (unit.HitPoints >= 90)
            {
                SelectedUnitHpBar.SelfModulate = new Color(0, 255, 0); //new Color("14e928");//Green
            }
            else if (unit.HitPoints >= 25 || unit.HitPoints <= 89)
            {
                SelectedUnitHpBar.SelfModulate = new Color(255, 255, 0); //new Color("ccc22b");//Yellow
            }
            else if (unit.HitPoints < 25)
            {
                SelectedUnitHpBar.SelfModulate = new Color(225, 0, 0); //new Color("e10000");//Red
            }
        }


    }
}
