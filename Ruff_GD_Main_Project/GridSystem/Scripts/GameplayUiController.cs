using System.Collections.Generic;
using Godot;
using RuffGdMainProject.GridSystem;
namespace RuffGdMainProject.UiSystem
{
    public class GameplayUiController : Control
    {
        public PlayerInfo p1Info, p2Info;
        private OptionsScreen options;
        private TurnTimer timer;
        public Control moveBtn, attackBtn, skipTurn, cardsList, tempCardList;
        private Label p1ManaLbl;
        private UnitVitalsUi UnitVitals;
        private PackedScene FloatingTxtScene;
        

        public override void _Ready()
        {
            FloatingTxtScene = GD.Load<PackedScene>("res://GridSystem/Scenes/FloatingText.tscn");
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
            p1ManaLbl = this.GetNode("Mana").GetChild<Label>(1);
            cardsList.GetChild<GridContainer>(0).Hide();
            tempCardList.GetChild<GridContainer>(0).Hide();
            skipTurn.Hide();
            HideUnitVitals();
            moveBtn.Hide();
            attackBtn.Hide();
        }

        public void Init()
        {
            SoundManager.Instance.PlaySoundByName("VsSound");
            GridManager.GM.TM.Connect("TurnChangeSignal", this, "OnTurnChanged");
            options.ShowMsgPopup("Start Battle", true);
        }

        public void TimeOutMsg()
        {
            options.ShowMsgPopup(true, "Time Out!");
        }

        public async void OnTurnChanged(int currentPlayer, int playerTotalTurns, int matchTotalTurns)
        {
            // var items =cardsList.GetChild(0).GetChildren().Count;
            // for (int i = 0; i < items; i++)
            // {
            //     cardsList.GetChild(0).GetChild<Card>(i).CardImage.RectScale = new Vector2(1, 1);
            // }
            cardsList.GetChild<GridContainer>(0).Hide();
            skipTurn.Hide();
            timer.HideTimer();
            moveBtn.Hide();
            attackBtn.Hide();
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
            p1ManaLbl.Text = "0";
            if(currentPlayer == 1)
            {
                p1Info.Highlight(true);
                p2Info.Highlight(false);
            }
            else
            {
                p2Info.Highlight(true);
                p1Info.Highlight(false);
            }
            // GD.Print(" GridManager.GM.TM.IsLocalPlayerTurn() = " + GridManager.GM.TM.IsLocalPlayerTurn());
            if(GridManager.GM.TM.IsLocalPlayerTurn())
            {
                options.ShowMsgPopup(true, "Your Turn!");
                p1ManaLbl.Text = playerTotalTurns.ToString();
                var units = GridManager.GM.TM.GetCurrentPlayer().MyUnits;
                UpdateProfilePic(currentPlayer, units[units.Count-1]);
                GridManager.GM.TM.GetNextPlayer().MarkMyTurn(playerTotalTurns);
                GridManager.GM.CM.ShowRandomCardsInContainer();
            }
            else
            {
                options.ShowMsgPopup(true, "Enemy's Turn...");
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
            p1ManaLbl.Text = Mana.ToString();
            GD.Print("Consuming Mana from UI Controller = " + Mana);
        }

        public void StartTimer(int playerNum){
            timer.StartTurnTimer(playerNum);
            if(GridManager.GM.TM.IsLocalPlayerTurn())
            {
                skipTurn.Show();
                moveBtn.Show();
                attackBtn.Show();
                cardsList.GetChild<GridContainer>(0).Show();
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
                // moveBtn.RectPosition = new Vector2(-33, -13);
            }
            else
            {
                attackBtn.RectScale = new Vector2(1f, 1f);
                // attackBtn.RectPosition = new Vector2(62, -13);
            }
        }

        public void _on_Button_mouse_exited(string btn)
        {
            if(btn.Equals("MoveBtn"))
            {
                moveBtn.RectScale = new Vector2(1, 1);
                // moveBtn.RectPosition = new Vector2(11, 11);
            }
            else
            {
                attackBtn.RectScale = new Vector2(1, 1);
                // attackBtn.RectPosition = new Vector2(68, 11);
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
                if(GridManager.GM.IsEnemyNearMe())
                {
                    // ToggleCardsList();
                    GridManager.GM.TM.GetCurrentUnit().Attack();
                }
                else
                {
                    ShowMessage("Not in Range!");
                }
            }
            // else
            // {
            //     if (GridManager.GM.CanSelectCell)
            //     {
            //         GridManager.GM.UnmarkPath();
            //         GridManager.GM.CanSelectCell = false;
            //     }

            //     // ToggleCardsList();
            //     GridManager.GM.TM.GetCurrentUnit().Attack();
            // }
        }

        public void GreyoutButtons(string btnName, bool greyOut = true)
        {
            if(btnName.Equals("MoveBtn"))
            {
                // moveBtn.GetChild<TextureButton>(0).Disabled = greyOut;
            }
            else if(btnName.Equals("Attackbtn"))
            {
                // attackBtn.GetChild<TextureButton>(0).Disabled = greyOut;
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
            // if(GridManager.GM.CM.CanPlaceCones)
            // {
            //     GridManager.GM.CM.RequestApplyCard();
            // }
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

        public void GameEnded(string msg)
        {
            options.GameEndScreen(msg);
            // options.GetNode<AnimationPlayer>("GameEndAnim").Play("GameEndPanelAnim");
            // GridManager.GM.GetTree().Paused = true;
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
            // if(dmg < 0) dmg = 0;
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
    }
}