using System;
using System.Collections.Generic;
using Godot;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GridSystem;
namespace RuffGdMainProject.UiSystem
{
    public class GameplayUiController : Control
    {
        public PlayerInfo p1Info, p2Info;
        public UnitInfo u1Info, u2Info, u3Info, u4Info;
        private OptionsScreen options;
        private TurnTimer timer;
        public Control moveBtn, attackBtn, skipTurn, cardsList, tempCardList;
        private ManaUi ManaSystem;
        private UnitVitalsUi UnitVitals;
        private PackedScene FloatingTxtScene;
        private Label manaLbl;
        private int p1Mana = 0, p2Mana = 0;

        private AnimationPlayer AnimationPlayerDeck;

        private CardViewer viewCardPanel;
        

        public override void _Ready()
        {
            FloatingTxtScene = GD.Load<PackedScene>("res://GridSystem/Scenes/FloatingText.tscn");
            viewCardPanel = GetNode<CardViewer>("CardZoomPanel");

            p1Info = this.GetChild<PlayerInfo>(0);
            p2Info = this.GetChild<PlayerInfo>(1);
            u1Info = this.GetChild<UnitInfo>(2);
            u2Info = this.GetChild<UnitInfo>(3);
            u3Info = this.GetChild<UnitInfo>(4);
            u4Info = this.GetChild<UnitInfo>(5);
            p1Info = this.GetChild<PlayerInfo>(0);
            p2Info = this.GetChild<PlayerInfo>(1);
            
            options = this.GetNode<OptionsScreen>("OptionsScreen");
            timer = this.GetNode<TurnTimer>("TurnTimer");
            moveBtn = this.GetNode<Control>("ActionPanel").GetChild<Control>(0);
            attackBtn = this.GetNode<Control>("ActionPanel").GetChild<Control>(1);
            skipTurn = this.GetNode<Control>("ActionPanel").GetChild<Control>(2);
            UnitVitals = GetNode<UnitVitalsUi>("UnitVitals");
            cardsList = this.GetNode<Control>("CardList");
            tempCardList = this.GetNode<Control>("TempCardList");
            
            ManaSystem = GetNode<Control>("Mana").GetNode<ManaUi>("ManaCrystalsRoot");
            manaLbl = GetNode<Control>("Mana").GetNode<TextureRect>("manaTextBg").GetChild<Label>(0);

            cardsList.GetChild<GridContainer>(0).Hide();
            tempCardList.GetChild<GridContainer>(0).Hide();
            skipTurn.Hide();
            HideUnitVitals();
            moveBtn.Hide();
            attackBtn.Hide();

            AnimationPlayerDeck = this.GetNode<AnimationPlayer>("AnimationPlayer");
        }

        public void Init()
        {
            SoundManager.Instance.PlaySoundByName("VsSound");
            GridManager.GM.TM.Connect("TurnChangeSignal", this, "OnTurnChanged");
            options.ShowMsgPopup("Start Battle", true);
        }

        public async void PlayCardAnimation()
        {
            await ToSignal(GetTree().CreateTimer(2f), "timeout");
            cardsList.Show();
            cardsList.GetChild<GridContainer>(0).Show();
            AnimationPlayerDeck.Play("forGameplayGrid");
        }

        public void TimeOutMsg()
        {
            options.ShowMsgPopup(true, GridManager.GM.TM.IsLocalPlayerTurn(), "Time Out!");
        }

        public async void OnTurnChanged(int currentPlayer, int playerTotalTurns, int matchTotalTurns)
        {
            skipTurn.Hide();
            viewCardPanel.CloseMe();
            timer.HideTimer();
            moveBtn.Hide();
            attackBtn.Hide();
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            ManaSystem.ShowZeroMana();
            manaLbl.Text = "0";
            if(currentPlayer == 1)
            {
                if(matchTotalTurns == 1)
                {
                    p1Mana = 2;
                    p2Mana = 3;
                }
                p1Info.Highlight(true);
                p2Info.Highlight(false);
            }
            else
            {
                if(matchTotalTurns == 1)
                {
                    p1Mana = 3;
                    p2Mana = 2;
                }
                p2Info.Highlight(true);
                p1Info.Highlight(false);
            }
            
            foreach (var item in GridManager.GM.CM.GetCardsInDeck())
            {
                item.Show();
                item.GreyoutEffect(false);
                item.MouseFilter = Control.MouseFilterEnum.Stop;
            }
            if(GridManager.GM.TM.IsLocalPlayerTurn())
            {
                options.ShowMsgPopup(true, true, "Your Turn!");
                var units = GridManager.GM.TM.GetCurrentPlayer().MyUnits;
                UpdateProfilePic(currentPlayer, units[units.Count-1]);
                List<Unit> playerRemainingUnits = new List<Unit>();
                for (int i = 0; i < units.Count - 1; i++)
                {
                    playerRemainingUnits.Add(units[i]);
                }
                UpdateUnitsProfilePic(currentPlayer, playerRemainingUnits);
                if(currentPlayer == 1)
                {
                    GridManager.GM.TM.GetNextPlayer().MarkMyTurn(p1Mana);
                    ManaSystem.ShowMana(p1Mana);
                    manaLbl.Text = p1Mana.ToString();
                    p1Mana += 2;
                }
                else
                {
                    GridManager.GM.TM.GetNextPlayer().MarkMyTurn(p2Mana);
                    ManaSystem.ShowMana(p2Mana);
                    manaLbl.Text = p2Mana.ToString();
                    p2Mana += 2;
                }
                
                GridManager.GM.CM.ShowRandomCardsInContainer();
            }
            else
            {
                options.ShowMsgPopup(true, false, "Enemy's Turn...");
                GridManager.GM.TM.GetNextPlayer().MarkMyTurnOnClient();
            }
            StartTimer(currentPlayer);
        }

        public void UpdateProfilePic(int pNum, Unit unit)
        {
            if(pNum == 1)
            {
                p1Info.UpdatePic(unit.data.ProfilePic);
            }
            else
            {
                p2Info.UpdatePic(unit.data.ProfilePic);
            }
        }

      public void UpdateUnitsProfilePic(int pNum, List<Unit> units)
        {
            if (pNum == 1)
            {
                if (units.Count == 2)
                {
                    u1Info.UpdateUnitPic(null);
                    u2Info.UpdateUnitPic(null);
                    u1Info.UpdateUnitPic(units[0].data.ProfilePic);
                    u2Info.UpdateUnitPic(units[1].data.ProfilePic);
                }
                else if (units.Count == 1)
                {
                    u1Info.UpdateUnitPic(null);
                    u2Info.UpdateUnitPic(null);
                    u1Info.UpdateUnitPic(units[0].data.ProfilePic);
                }
                else if (units.Count == 0)
                {
                    u1Info.UpdateUnitPic(null);
                    u2Info.UpdateUnitPic(null);
                }

            }
            else if(pNum == 2)
            {
                if (units.Count == 2)
                {
                    u3Info.UpdateUnitPic(null);
                    u4Info.UpdateUnitPic(null);
                    u3Info.UpdateUnitPic(units[0].data.ProfilePic);
                    u4Info.UpdateUnitPic(units[1].data.ProfilePic);
                }
                else if (units.Count == 1)
                {
                    u3Info.UpdateUnitPic(null);
                    u4Info.UpdateUnitPic(null);
                    u3Info.UpdateUnitPic(units[0].data.ProfilePic);
                }
                else if (units.Count == 0)
                {
                    u3Info.UpdateUnitPic(null);
                    u4Info.UpdateUnitPic(null);
                }
            }
        }

        public void UpdateUnitsOnPlayerId(Player player, Unit currentUnit)
        {
            List<Unit> playerRemainingUnits = new List<Unit>();
            int currentUnitIndex = player.MyUnits.IndexOf(currentUnit);

            if (currentUnitIndex == 0)
            {
                playerRemainingUnits.Add(player.MyUnits[1]);
                playerRemainingUnits.Add(player.MyUnits[2]);
                UpdateUnitsProfilePic(player.PlayerNumber, playerRemainingUnits);
            }
            else if (currentUnitIndex == 1)
            {
                playerRemainingUnits.Add(player.MyUnits[0]);
                playerRemainingUnits.Add(player.MyUnits[2]);
                UpdateUnitsProfilePic(player.PlayerNumber, playerRemainingUnits);
            }
            else
            {
                playerRemainingUnits.Add(player.MyUnits[0]);
                playerRemainingUnits.Add(player.MyUnits[1]);
                UpdateUnitsProfilePic(player.PlayerNumber, playerRemainingUnits);
            }

        }

        public void UpdateNameLbls(int localPlayerNumber)
        {
            if(localPlayerNumber == 1)
            {
                p1Info.SetPlayerName("You");
                p2Info.SetPlayerName("Enemy");
            }
            else
            {
                p2Info.SetPlayerName("You");
                p1Info.SetPlayerName("Enemy");
            }
        }

        public void UpdateMana(int Mana)
        {
            ManaSystem.ShowMana(Mana);
        }

        public void ShowSelectedUnitVitals(Unit unit)
        {
            if (unit.MyPlayer.PlayerNumber == 1)
            {
                p1Info.ShowUnitVitals(unit);
            }
            else
            {
                p2Info.ShowUnitVitals(unit);
            }
        }

        public void StartTimer(int playerNum){
            timer.StartTurnTimer(playerNum);
            if(GridManager.GM.TM.IsLocalPlayerTurn())
            {
                skipTurn.Show();
                moveBtn.Show();
                attackBtn.Show();
            }
        }

        public void StopTimer()
        {
            timer.StopTurnTimer();
        }

        public int GetTime()
        {
            return timer.GetTimerValue();
        }

        public void _on_Button_mouse_entered(string btn)
        {
            if(btn.Equals("MoveBtn"))
            {
                moveBtn.RectScale = new Vector2(1f, 1f);
            }
            else
            {
                attackBtn.RectScale = new Vector2(1f, 1f);
            }
        }

        public void _on_Button_mouse_exited(string btn)
        {
            if(btn.Equals("MoveBtn"))
            {
                moveBtn.RectScale = new Vector2(1, 1);
            }
            else
            {
                attackBtn.RectScale = new Vector2(1, 1);
            }
        }

        public void _on_ActionButton_pressed(string btnName)
        {
            if (btnName.Equals("MoveBtn"))
            {
                GridManager.GM.CanSelectCell = false;
                GridManager.GM.TM.GetCurrentUnit().MarkAsSelected(false, true);
            }
            else if(btnName.Equals("Attackbtn"))
            {
                GridManager.GM.UnmarkPath();
                GridManager.GM.TM.GetCurrentUnit().Attack();
            }
        }

        public void GreyoutButtons(string btnName, bool greyOut = true)
        {
            if(btnName.Equals("MoveBtn"))
            {
                moveBtn.GetChild<TextureButton>(0).Disabled = greyOut;
            }
            else if(btnName.Equals("Attackbtn"))
            {
                attackBtn.GetChild<TextureButton>(0).Disabled = greyOut;
            }
        }

        public void PauseGame()
        {
            options.PauseGame();
        }

        public void _on_SkipUnitTurn_mouse_entered()
        {
            InitSkipTurn();
        }

        private async void InitSkipTurn()
        {
            timer.StopTurnTimer();
            skipTurn.Hide();
            timer.HideTimer();
            moveBtn.Hide();
            attackBtn.Hide();
            timer.HideTimer();

            await ToSignal(GetTree().CreateTimer(1f), "timeout");
            GridManager.GM.TM.SkipUnitTurn();
        }

        public void ShowCardMessage(Texture cardImg)
        {
            options.ShowMsgPopup(cardImg);
        }
        public void ShowMessage(string msg)
        {
            options.ShowMsgPopup(msg, true);
        }

        public void HideCardMessage()
        {
            options.HideImmediate();
        }

        public void GameEnded(string msg, bool lost)
        {
            viewCardPanel.CloseMe();
            options.GameEndScreen(msg, lost);
        }

        public void ToggleCardsList(bool hide =  true)
        {
            if(hide)
            {
                cardsList.GetChild<GridContainer>(0).Hide();
            }
            else
            {
                cardsList.GetChild<GridContainer>(0).Show();
            }
        }

        public void ShowUnitVitals(Unit unit)
        {
            UnitVitals.ShowVitals(unit);
            UnitVitals.SetGlobalPosition(new Vector2(unit.GlobalPosition.x, unit.GlobalPosition.y-170f));
        }

        public void ShowUnitVitals(Cone cone)
        {
            UnitVitals.ShowVitals(cone);
            UnitVitals.SetGlobalPosition(new Vector2(cone.GlobalPosition.x, cone.GlobalPosition.y-170f));
        }

        public void HideUnitVitals()
        {
            UnitVitals.HideVitals();
        }

        public void ShowDmgAtUnit(Unit unt, float dmg, TextColorType colorType)
        {
            SoundManager.Instance.PlaySoundByName("AttackSound");
            unt.GetNode<FloatingText>("FloatingText").Init((int)dmg, colorType);
        }
        public void ShowDmgAtUnit(Cone cn, float dmg, TextColorType colorType)
        {
            SoundManager.Instance.PlaySoundByName("AttackSound");
            cn.GetChild<FloatingText>(cn.GetChildCount()-1).Init((int)dmg, colorType);
        }

        public async void ShowOpponentCards(List<Card> cards)
        {
            PackedScene tempCard = (PackedScene)GD.Load("res://CardSystem/Scenes/TempCardStatic.tscn");
            foreach (var item in cards)
            {
                Control card = (Control)tempCard.Instance();
                card.GetChild<TextureRect>(0).Texture = item.CardImage.Texture;
                tempCardList.GetChild(0).AddChild(card);
            }
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            tempCardList.GetChild<GridContainer>(0).Show();
            await ToSignal(GetTree().CreateTimer(2.5f), "timeout");
            SoundManager.Instance.PlaySoundByName("TelescopeSound");
            tempCardList.GetChild<GridContainer>(0).Hide();
            while(tempCardList.GetChild<GridContainer>(0).GetChildCount() > 0)
            {
                var item = tempCardList.GetChild<GridContainer>(0).GetChild<Control>(0);
                tempCardList.GetChild<GridContainer>(0).RemoveChild(tempCardList.GetChild<GridContainer>(0).GetChild<Control>(0));
                item.QueueFree();
            }
        }

        public void ShowCard(string texturePath)
        {
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "In Show Card Function");
            Texture tempCardImg = (Texture)GD.Load(texturePath);
            viewCardPanel.ViewCard(tempCardImg);
        }


    }
}