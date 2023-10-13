using Godot;
using System;
using RuffGdMainProject.GridSystem;

namespace RuffGdMainProject.UiSystem
{
    public class TurnTimer : Control
    {
        // Signal emitted when the timer counts down
        [Signal]
        private delegate void counted_down(int number);

        private Timer turnTimer;
        private TextureRect bg;
        private Label lbl;
        private string prefix = "Player1 Turn: 00:";
        private int seconds;
        private TextureProgress timerImage;

        public override void _Ready()
        {
            // Get references to child nodes
            bg = GetChild<TextureRect>(0);
            lbl = GetChild<Label>(1);
            turnTimer = GetChild<Timer>(2);
            timerImage = GetChild<TextureProgress>(3);

            // Set initial values
            seconds = 30;
            HideTimer();
        }

        public void StartTurnTimer(int playerId)
        {
            // Reset timer values based on the player ID
            seconds = 30;
            /*  if (playerId == 1)
                  prefix = "Player1 Turn: 00:";
              else
                  prefix = "Player2 Turn: 00:";*/
            if (playerId == 1)
                prefix = "";
            else
                prefix = "";
            // Show timer elements
            bg.Show();
            lbl.Text = prefix + seconds;
            lbl.Show();
            turnTimer.Start();
            
        }

        public void StopTurnTimer()
        {
            turnTimer.Stop();
        }

        public void ProccessTimer()
        {
            seconds--;
            // Update the timer display
            if (seconds < 10)
                lbl.Text = prefix + "0" + seconds;
            else
                lbl.Text = prefix + seconds;

            if (seconds < 0)
            {
                // Hide the timer and notify the grid manager about the timeout
                HideTimer();
                GridManager.GM.UiController.TimeOutMsg();
                GridManager.GM.TM.RequestChangeTurn();
            }
            else
            {
                // Restart the timer
                turnTimer.Start();
            }
            
            timerImageFlow();
        }

        public void timerImageFlow()
        {
            float progress = seconds;
            progress = Mathf.Max(progress, 0.0f);
            timerImage.Value = progress;
            if(progress <= 15)
            {
                timerImage.TextureProgress_ = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/New Gameplay elements/timer red.png");
            }
            else
            {
                timerImage.TextureProgress_ = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/New Gameplay elements/timer green.png");
            }
        }

        public void HideTimer()
        {
            // Hide the timer elements
            bg.Hide();
            lbl.Hide();
        }

        public int GetTimerValue()
        {
            return seconds;
        }
    }
}
