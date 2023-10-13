using Godot;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GridSystem;
using System;

namespace RuffGdMainProject.UiSystem
{
    public class DragableCard : Control
    {
        public int cardID;
        bool canDrag = false;
        public TextureRect CardImage;
        Control tempNode;

        public override void _Ready()
        {
            CardImage = GetChild<TextureRect>(0);
        }

        public void SetCardData(CardData data)
        {
            // GD.Print("CARD IDDDD: " + data.CardID);
            cardID = data.CardID;
            var texture = GD.Load<Texture>(data.CardImagePath);
            GetChild<TextureRect>(0).Texture = texture;
        }

        public override void _Process(float delta)
        {
            if (canDrag)
            {
                Vector2 newPos = new Vector2(GetGlobalMousePosition().x - (CardImage.RectSize.x / 2), GetGlobalMousePosition().y - (CardImage.RectSize.y / 2));
                RectPosition = newPos;
            }
            else
            {
                if (tempNode != null)
                {
                    tempNode = null;
                    GridManager.GM.CM.AddCardsInGrid(cardID);
                    //GridManager.GM.CM.DropCard(this);
                    this.QueueFree();
                }
            }
        }


        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    canDrag = true;
                    tempNode = this;
                }
                else
                {
                    canDrag = false;
                }

            }
        }
    }
}