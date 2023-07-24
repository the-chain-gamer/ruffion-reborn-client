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

        public void SetCardData(CardData data, bool isZero = false)
        {
            Data = data;
            // GD.Print("CARD IDDDD: " + data.CardID);
            CardID = data.CardID;
            if(!isZero)
            {
                CardImage = GetChild<TextureRect>(0);
                origColor = CardImage.SelfModulate;
                CardImage.Texture = GD.Load<Texture>(data.CardImagePath);
            }
        }
        public override void _Process(float delta)
        {
            if(tempCard != null)
            {
                if (canDrag)
                {
                    tempCard.RectPosition = new Vector2(GetGlobalMousePosition().x - (CardImage.RectSize.x / 2), GetGlobalMousePosition().y - (CardImage.RectSize.y / 2));
                    // GD.Print("DRAgggginggggggg");
                }
                else if(!canDrag && tempCard != null)
                {
                    tempCard.QueueFree();
                    tempCard = null;
                    // GD.Print("DROPPPPPPPPEDDDDDDDDDDDDDDDDDDDDDD");
                    GridManager.GM.CM.DropCard(this);
                    //QueueFree();
                }
            }
        }
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    if(Data.ManaCost <= GridManager.GM.TM.GetCurrentPlayerTotalMana() && !isGreyedOut)
                    {
                        canDrag = true;
                        // GD.Print("DOWNNNNN");
                        // GD.Print("GetCurrentPlayerTotalMana()= " + GridManager.GM.TM.GetCurrentPlayerTotalMana());
                        //create a temp card
                        SpawnDragableCards();
                    }
                }
                if(!mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    // GD.Print("GetCurrentPlayerTotalMana()= " + GridManager.GM.TM.GetCurrentPlayerTotalMana());
                    if(Data.ManaCost <= GridManager.GM.TM.GetCurrentPlayerTotalMana())
                    {
                        canDrag = false;
                    }
                }
            }
        }
        
        public void OnMouseEnter()
        {
            
        }

        public void OnMouseExit() {
            
        }

        public void SpawnDragableCards()
        {
            PackedScene dragableCardItem = (PackedScene)GD.Load("res://CardSystem/Scenes/DragableCard.tscn");
            tempCard = (Control)dragableCardItem.Instance();
            tempCard.GetChild<TextureRect>(0).Texture = CardImage.Texture;
            GridManager.GM.UiController.AddChild(tempCard);
            tempCard.RectPosition = new Vector2(this.RectPosition.x + 450, this.RectPosition.y + 700);
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