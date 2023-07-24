using Godot;
using RuffGdMainProject.GridSystem;
using System;

namespace RuffGdMainProject.UiSystem
{
    public class UnitVitalsUi : Control
    {
        // UI elements
        private ProgressBar hpBar;
        private Label unitLbl, hpLbl;
        private Label shieldLbl, defLbl, apLbl, smokeLbl, hatLbl;
        private Control shield, hat, smoke;
        private bool isShowing = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Initialize UI elements
            shield = GetChild(1).GetChild<Control>(0);
            hat = GetChild(1).GetChild<Control>(1);
            smoke = GetChild(1).GetChild<Control>(2);

            hpBar = GetChild(0).GetChild<ProgressBar>(1);
            hpLbl = GetChild(0).GetChild<Label>(2);
            unitLbl = GetChild(0).GetChild<Label>(3);
            shieldLbl = shield.GetChild<Label>(1);
            hatLbl = hat.GetChild<Label>(1);
            smokeLbl = smoke.GetChild<Label>(1);

            // Hide UI elements initially
            shield.Hide();
            hat.Hide();
            smoke.Hide();
            isShowing = false;
        }

        // Show unit vitals UI for a specific unit
        public void ShowVitals(Unit unit)
        {
            if(isShowing)
                return;

            isShowing = true;
            GD.Print(">>>>>>> vital on");
            // GetChild<Control>(0).Hide();
            hpBar.MaxValue = unit.TotalHitPoints;
            hpBar.Value = unit.HitPoints;
            // GD.Print("MY HP + +++ = " + unit.HitPoints);
            unitLbl.Text = unit.UnitName;
            if (unit.HitPoints < 0.0f)
            {
                hpLbl.Text = 0.0f.ToString() + "/" + unit.TotalHitPoints;
            } 
            else
            {
                hpLbl.Text = unit.HitPoints.ToString() + "/" + unit.TotalHitPoints;
            }
            // ShieldLbl = unit.is
            // DefLbl.Text = "Def " + unit.ConsumableDefenceFactor;
            // ApLbl.Text = "AP " + unit.ConsumeableAttackFactor;
            // CloakLbl.Text = "Cloak " + unit.ConsumableCloak;
            GetChild<Control>(0).Show();
            GetChild<Control>(1).Show();

            if(unit.ConsumableCloak > 0)
            {
                GD.Print("Ali k kehne pe ******************************************");
                if(unit.IsHatTrick)
                {
                    hatLbl.Text = unit.ConsumableCloak.ToString();
                    hat.Show();
                }
                else
                {
                    smokeLbl.Text = unit.ConsumableCloak.ToString();
                    smoke.Show();
                }
            }
            else if (unit.ConsumeableShieldValue > 0)
            {
                GD.Print("SHIELDDDDDDDDDDDDDD");
                shieldLbl.Text = unit.ConsumeableShieldValue.ToString();
                shield.Show();
            }
            else
            {
                hat.Hide();
                smoke.Hide();
                shield.Hide();
            }
            // AutoHide();
            GD.Print("unit.ConsumeableShieldValue ************************", unit.ConsumeableShieldValue);
        }

        // Show vitals UI for a Cone object
        public void ShowVitals(Cone cone)
        {
            if (isShowing)
                return;

            isShowing = true;
            GD.Print(">>>>>>> vital on");

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
            GD.Print(">>>>>>> vital off");
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
            GD.Print(">>>>>>> vital off");
            GetChild<Control>(0).Hide();
            GetChild<Control>(1).Hide();
        }
    }
}
