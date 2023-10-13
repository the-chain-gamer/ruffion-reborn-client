using Godot;
using RuffGdMainProject.DataClasses;
using System;

namespace RuffGdMainProject.UiSystem
{
    public class Dragable : Control
    {
        public int cardID;
        public TextureRect CardImage;

        private CardData myData;
        public override void _Ready()
        {
            CardImage = GetChild<TextureRect>(0);
        }

        public void SetCardData(CardData data)
        {
            myData = data;
            // GD.Print("CARD IDDDD: " + data.CardID);
            cardID = data.CardID;
            var texture = GD.Load<Texture>(data.CardImagePath);
            GetChild<TextureRect>(0).Texture = texture;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    // GD.Print("Mouse Pressed Down");
                }
                else if (!mouseButton.IsPressed() && mouseButton.ButtonIndex == (int)ButtonList.Left)
                {
                    // GD.Print("Mouse Pressed Up:" + cardID);
                    // GD.Print("Name of parent is" + this.GetParent().GetParent().Name);
                    spawnDragableCards(cardID);
                    this.QueueFree();
                }
            }
        }

        public void spawnDragableCards(int cardId)
        {
            PackedScene dragableCardItem = (PackedScene)GD.Load("res://CardSystem/Scenes/DragableCard.tscn");
            DragableCard CardItem = (DragableCard)dragableCardItem.Instance();
            CardItem.SetCardData(myData);
            // GD.Print("Name of Node is instantiated" + RuffGdMainProject.GridSystem.GridManager.GM.UiController.Name);
            RuffGdMainProject.GridSystem.GridManager.GM.UiController.AddChild(CardItem);
            CardItem.RectPosition = new Vector2(this.RectPosition.x + 450, this.RectPosition.y + 700);
        }


    }
}