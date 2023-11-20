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
        private TextureRect PopupBg;
        private Texture BlueMsgImg;
        private Texture RedMsgimg;
        private Texture GreenMsgimg;

        private TextureRect PauseMenuBg;
        private Texture DefaultTexture;
        private Texture WinTexture;
        private Texture LoseTexture;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            PauseMenu = GetNode<Control>("PauseMenu");
            PauseMenu.Hide();
            PauseMenuBg = PauseMenu.GetNode<TextureRect>("TextureRect");
            MsgPopup = GetChild<Control>(1);
            PopupBg = MsgPopup.GetNode<TextureRect>("PopupBg");
            MsgPopup.Hide();
            overlay = GetChild<TextureRect>(0);
            overlay.Hide();
            MsgLbl = GetChild(1).GetChild(0).GetNode<Label>("MsgLbl");
            EndGameMsgLbl = GetChild(3).GetChild(0).GetNode<Label>("MsgLbl2");
            AppliedCardPopup = GetChild<Control>(2);
            AppliedCardPopup.Hide();

            BlueMsgImg = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/popup.png");
            RedMsgimg = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/popup-red.png");
            GreenMsgimg = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/popup-green.png");

            DefaultTexture = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/Frame.png");
            WinTexture = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/Win.png");
            LoseTexture = GD.Load<Texture>("res://GridSystem/Assets/Gameplay/Lost.png");
        }

        public void ShowMsgPopup(string msg, bool HideMsg)
        {
            PopupBg.Texture = BlueMsgImg;
            MsgLbl.Text = msg;
            overlay.Show();
            MsgPopup.Show();
            var vsScreen = GetNode("AnimationPlayer") as AnimationPlayer;
            vsScreen.Play("PopUpAnim");
            if(HideMsg){
                HideMsgWithDelay(false);
            }
        }

        public void ShowMsgPopup(bool turnMsg, bool isMyTurn, string msg = null)
        {
            if(turnMsg)
            {
                if(isMyTurn)
                {
                    PopupBg.Texture = GreenMsgimg;
                }
                else
                {
                    PopupBg.Texture = RedMsgimg;
                }
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
        }

        public void ResumeGame()
        {
            GridManager.GM.GetTree().Paused = false;
            PauseMenu.Hide();
            overlay.Hide();
        }

        public void QuitToMenu()
        {
            GridManager.GM.GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }

        public async void GameEndScreen(string msg, bool isGameLost)
        {
            if(isGameLost)
            {
                PauseMenuBg.Texture = LoseTexture;
            }
            else
            {
                PauseMenuBg.Texture = WinTexture;
            }

            EndGameMsgLbl.Text = msg;
            EndGameMsgLbl.Show();
            await ToSignal(GetTree().CreateTimer(2.5f), "timeout");
            PauseMenu.GetNode<Control>("TextureRect/ResumeBtn").Hide();
            PauseMenu.Show();
            overlay.Show();
        }
    }
}
