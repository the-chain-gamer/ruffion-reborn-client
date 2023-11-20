using Godot;
using RuffGdMainProject.GridSystem;
using System;
using System.IO;

namespace RuffGdMainProject.UiSystem
{
    public class UnitVitalsUi : Control
    {
        // UI elements
        private Control grid;
        private ProgressBar hpBar;
        private Label unitLbl, hpLbl;
        private TextureRect iconsBg;
        private Label saunderLbl, crosbyLbl, apLbl, cyrilLbl, marlonLbl;
        private Control saunder, cyril, crosby, marlon;
        private bool isShowing = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Initialize UI elements
            grid = GetNode<Control>("BuffsUi");
            iconsBg = grid.GetNode<TextureRect>("IconsBg");

            saunder = grid.GetNode<Control>("Saunder");
            cyril = grid.GetNode<Control>("Cyril");
            crosby = grid.GetNode<Control>("Crosby");
            marlon = grid.GetNode<Control>("Marlon");

            saunderLbl = saunder.GetNode<Label>("Label");
            cyrilLbl = cyril.GetNode<Label>("Label");
            marlonLbl = marlon.GetNode<Label>("Label");
            crosbyLbl = crosby.GetNode<Label>("Label");

            hpBar = GetChild(0).GetChild<ProgressBar>(1);
            hpLbl = GetChild(0).GetChild<Label>(2);
            unitLbl = GetChild(0).GetChild<Label>(3);

            // Hide UI elements initially
            saunder.Hide();
            cyril.Hide();
            crosby.Hide();
            marlon.Hide();
            iconsBg.Hide();

            isShowing = false;
        }

        // Show unit vitals UI for a specific unit
        public void ShowVitals(Unit unit)
        {
            if(isShowing)
                return;

            isShowing = true;
            iconsBg.Hide();            
            hpBar.MaxValue = unit.TotalHitPoints;
            hpBar.Value = unit.HitPoints;
            unitLbl.Text = unit.UnitName;
            if (unit.HitPoints >= 90)
            {
                hpBar.SelfModulate = new Color(0,255,0); //new Color("308c0f");//Green
            }
            else if (unit.HitPoints >= 25 || unit.HitPoints <= 89)
            {
                hpBar.SelfModulate = new Color(255, 255, 0); //new Color("ccc22b");//Yellow
            }
            else if (unit.HitPoints < 25)
            {
                hpBar.SelfModulate = new Color(255, 0, 0); //new Color("f23618");//Red
            }
            
            if (unit.HitPoints < 0.0f)
            {
                hpLbl.Text = 0.0f.ToString() + "/" + unit.TotalHitPoints;
            } 
            else
            {
                hpLbl.Text = unit.HitPoints.ToString() + "/" + unit.TotalHitPoints;
            }

            GetChild<Control>(0).Show();
            GetChild<Control>(1).Show();

            if(unit.ConsumableCloak > 0)
            {
                iconsBg.Show();
                if(unit.IsHatTrick)
                {
                    cyrilLbl.Text = unit.ConsumableCloak.ToString();
                    cyril.Show();
                }
                else
                {
                    marlonLbl.Text = unit.ConsumableCloak.ToString();
                    marlon.Show();
                }
            }
            else
            {

                cyril.Hide();
                marlon.Hide();
            }
            
            if(unit.IsSmokescreenApplied)
            {
                iconsBg.Show();
                crosbyLbl.Text = "1";
                crosby.Show();
            }
            else
            {
                crosby.Hide();
            }
            
            if (unit.IsConsumeableShieldApplied)
            {
                iconsBg.Show();
                saunderLbl.Text = unit.ConsumeableShieldValue.ToString();
                saunder.Show();
            }
            else
            {
                saunder.Hide();
            }
        }

        // Show vitals UI for a Cone object
        public void ShowVitals(Cone cone)
        {
            if (isShowing)
                return;

            isShowing = true;

            // Update UI elements with Cone data
            hpBar.MaxValue = 10;
            hpBar.Value = cone.HitPoints;
            unitLbl.Text = "Cone/Roadblock " + cone.Life;

            if (cone.HitPoints < 0.0f)
            {
                hpLbl.Text = "0.0 / 10";
            }
            else
            {
                hpLbl.Text = cone.HitPoints.ToString() + " / 10";
            }

            GetChild<Control>(0).Show();
        }

        // Hide the vitals UI
        public void HideVitals()
        {
            isShowing = false;
            GetChild<Control>(0).Hide();
            GetChild<Control>(1).Hide();
        }

        // Auto-hide the vitals UI after a delay
        public async void AutoHide()
        {
            if (!isShowing)
                return;

            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            isShowing = false;
            GetChild<Control>(0).Hide();
            GetChild<Control>(1).Hide();
        }
    }
}
