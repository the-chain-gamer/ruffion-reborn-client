using Godot;
using System;

namespace RuffGdMainProject.GridSystem
{
    public class OptionsScreen : Control
    {
        private Control MsgPopup;
        private Label MsgLbl;

        private TextureRect overlay;

        private Control AppliedCardPopup;
        private Control PauseMenu;
        private Label EndGameMsgLbl;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            PauseMenu = GetNode<Control>("PauseMenu");
            PauseMenu.Hide();
            MsgPopup = GetChild<Control>(1);
            MsgPopup.Hide();
            overlay = GetChild<TextureRect>(0);
            overlay.Hide();
            MsgLbl = GetChild(1).GetChild(0).GetNode<Label>("MsgLbl");
            EndGameMsgLbl = GetChild(3).GetChild(0).GetNode<Label>("MsgLbl2");
            AppliedCardPopup = GetChild<Control>(2);
            AppliedCardPopup.Hide();
        }

        public void ShowMsgPopup(string msg, bool HideMsg)
        {
            MsgLbl.Text = msg;
            overlay.Show();
            MsgPopup.Show();
            var vsScreen = GetNode("AnimationPlayer") as AnimationPlayer;
            vsScreen.Play("PopUpAnim");
            // SoundManager.Instance.PlaySoundByName("TurnChangeSound");
            // tw.TweenProperty($, Modulate, 255.0f, 2.5f);
            if(HideMsg){
                HideMsgWithDelay(false);
            }
        }

        public void ShowMsgPopup(bool turnMsg, string msg = null)
        {
            if(turnMsg)
            {
                MsgLbl.Text = msg;
            }
            overlay.Show();
            MsgPopup.Show();
            var vsScreen = GetNode("AnimationPlayer") as AnimationPlayer;
            vsScreen.Play("PopUpAnim");
            HideMsgWithDelay(true);
        }

        public void ShowMsgPopup(Texture cardimg)
        {
            AppliedCardPopup.Show();
            overlay.Show();
            AppliedCardPopup.GetChild<TextureRect>(0).Texture = cardimg;
            HideMsgWithDelay(false, 1.5f);
        }

        public async void HideMsgWithDelay(bool isTurnEnd, float timeOut = 3.0f)
        {
            if(isTurnEnd) SoundManager.Instance.PlaySoundByName("TurnChangeSound");
            await ToSignal(GetTree().CreateTimer(timeOut), "timeout");
            MsgPopup.Hide();
            overlay.Hide();
            AppliedCardPopup.Hide();
            // if(isTurnEnd)
            // {
            //     GridManager.GM.UiController.StartTimer();
            // }
            // GD.Print("PauseMenu.GetNode(TextureRect-ResumeBtn) = "+PauseMenu.GetNode("TextureRect/ResumeBtn"));

        }

        public async void HideImmediate()
        {
            MsgPopup.Hide();
            overlay.Hide();
            AppliedCardPopup.Hide();
        }

        public void PauseGame()
        {
            EndGameMsgLbl.Hide();
            PauseMenu.Show();
            overlay.Show();
            GridManager.GM.GetTree().Paused = true;
            // GD.Print("GAME PAUSED :::::::::::::::::::");
        }

        public void ResumeGame()
        {
            // GD.Print("GAME RESUMED :::::::::::::::::::");
            GridManager.GM.GetTree().Paused = false;
            PauseMenu.Hide();
            overlay.Hide();
        }

        public void QuitToMenu()
        {
            GridManager.GM.GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }

        public async void GameEndScreen(string msg)
        {
            // ShowMsgPopup(msg, true);
            EndGameMsgLbl.Text = msg;
            EndGameMsgLbl.Show();
            await ToSignal(GetTree().CreateTimer(2.5f), "timeout");
            // GD.Print("PauseMenu.GetNode<TextureButton>(TextureRect-ResumeBtn) = "+PauseMenu.GetNode("TextureRect/ResumeBtn"));
            PauseMenu.GetNode<Control>("TextureRect/ResumeBtn").Hide();
            // btn.Hide();
            PauseMenu.Show();
            overlay.Show();
        }
    }
}
