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

        public override void _Ready()
        {
            // Get references to child nodes
            bg = GetChild<TextureRect>(0);
            lbl = GetChild<Label>(1);
            turnTimer = GetChild<Timer>(2);

            // Set initial values
            seconds = 30;
            HideTimer();
        }

        public void StartTurnTimer(int playerId)
        {
            // Reset timer values based on the player ID
            seconds = 30;
            if (playerId == 1)
                prefix = "Player1 Turn: 00:";
            else
                prefix = "Player2 Turn: 00:";

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
        }

        public void HideTimer()
        {
            // Hide the timer elements
            bg.Hide();
            lbl.Hide();
        }
    }
}
