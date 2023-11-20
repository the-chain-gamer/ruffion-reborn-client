using Godot;
using System;
using RuffGdMainProject.GridSystem;
using RuffGdMainProject.DataClasses;

namespace RuffGdMainProject.UiSystem
{
    public class Card : Control
    {
        public int CardID { get; private set; }
        public TextureRect CardImage;
        public CardData Data { get; private set; }
        private bool canDrag = false;
        private Control tempCard;
        private Color origColor;
        private bool isGreyedOut = false;
        private bool isZeroCard = false;

        public void SetCardData(CardData data, bool isZero = false)
        {
            isZeroCard = isZero;
            Data = data;
            CardID = data.CardID;
            if(!isZero)
            {
                CardImage = GetChild<TextureRect>(0);
                origColor = CardImage.SelfModulate;
                CardImage.Texture = GD.Load<Texture>(data.CardImagePath);
            }
            GridManager.GM.TM.Connect("TurnChangeSignal", this, "OnTurnChanged");
        }
        public override void _Process(float delta)
        {
            if(tempCard != null)
            {
                if (canDrag)
                {
                    tempCard.RectPosition = new Vector2(GetGlobalMousePosition().x - (CardImage.RectSize.x / 2), GetGlobalMousePosition().y - (CardImage.RectSize.y / 2));
                }
                else if(!canDrag && tempCard != null)
                {
                    tempCard.QueueFree();
                    tempCard = null;
                    GridManager.GM.CM.DropCard(this);
                }
            }
        }
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    if(GridManager.GM.TM.IsLocalPlayerTurn())
                    {
                        if(Data.ManaCost <= GridManager.GM.TM.GetCurrentPlayerTotalMana() && !isGreyedOut)
                        {
                            canDrag = true;
                            //create a temp card
                            SpawnDragableCards();
                        }
                    }
                    else
                    {
                        //show card in a window
                        Logger.UiLogger.Log(Logger.LogLevel.INFO, Data.CardImagePath);
                        GridManager.GM.UiController.ShowCard(Data.CardImagePath);
                    }
                }

                if(!mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    if(Data.ManaCost <= GridManager.GM.TM.GetCurrentPlayerTotalMana())
                    {
                        canDrag = false;
                    }
                }
            }
        }
        
        public void OnMouseEnter()
        {
            // GridManager.GM.VisualizeAttackRange(GridManager.GM.TM.GetCurrentUnit());
        }

        public void OnMouseExit()
        {
            // GridManager.GM.ResetGridVisuals();
        }

        public void SpawnDragableCards()
        {
            PackedScene dragableCardItem = (PackedScene)GD.Load("res://CardSystem/Scenes/DragableCard.tscn");
            tempCard = (Control)dragableCardItem.Instance();
            tempCard.GetChild<TextureRect>(0).Texture = CardImage.Texture;
            GridManager.GM.UiController.AddChild(tempCard);
            tempCard.RectPosition = new Vector2(this.RectPosition.x + 450, this.RectPosition.y + 700);
        }

        public async void OnTurnChanged(int currentPlayer, int playerTotalTurns, int matchTotalTurns)
        {
            if(!isZeroCard)
            {
                if(tempCard != null)
                {
                    tempCard.QueueFree();
                    tempCard = null;
                }
                CardImage.RectScale =new Vector2(1f, 1f);
            }
        }

        public void ZoomInCard()
        {
            GridManager.GM.UiController.ShowCardMessage(CardImage.Texture);
        }

        public void ZoomOutCard()
        {
            GridManager.GM.UiController.HideCardMessage();
        }

        public void GreyoutEffect(bool greyout)
        {
            isGreyedOut=greyout;
            if(greyout)
            {
                CardImage.SelfModulate = new Color("6e808080");
            }
            else
            {
                CardImage.SelfModulate = new Color("ffffff");
            }
        }

        public void ShowCard()
        {
            this.Show();
        }

        public void HideCard()
        {
            this.Hide();
        }
    }
}