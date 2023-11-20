using Godot;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GameScene;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.UiSystem
{
    public class CardSelectionManager : Control
    {
        private int zoomCardID;
        CardButtonUI CardZoomItem;

        public static CardSelectionManager instance;
        public int CardsLimit = 9;
        public PackedScene CardNewScene;

        List<int> savedCardsList;
        int x;
        private ConfigFile CF = new ConfigFile();
        [Export]
        public string CFName = "CardSaveIDS.cfg";
        public List<CardButtonUI> CardsList;
        public int ZoomCardClickedId;

        public override void _Ready()
        {
            CardsList = new List<CardButtonUI>();
            instance = this;

            if (CF.Load("user://" + CFName) != Error.Ok)
            {
                Save();
            }
            LoadCardIDS();
            //init card DB/DataObjects
            InstantiateCards();
            DoneButton();
            LoadDeckWithDelay();
        }

        public async void LoadDeckWithDelay()
        {
            await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
            LoadDeck();
        }

        public void LoadCardIDS()
        {
            savedCardsList = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                int xValue = (int)CF.GetValue("CardDeck", "CardId" + (i + 1), x);
                savedCardsList.Add(xValue);
            }
        }

        public void InstantiateCards()
        {
            var container = GetNode<Control>("Control").GetNode<TextureRect>("AllCardsHolderBG").GetNode<TextureRect>("AllCardsHolderFrame").
                    GetNode<ScrollContainer>("ScrollContainer").GetNode<GridContainer>("GridContainer");

            for (int n = 2; n < StartupScript.Startup.DB.AllCardsData.Count; n++)
            {
                CardNewScene = (PackedScene)GD.Load("res://Cards_UI/SCENES/CardNew.tscn");
                CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
                CardData data = StartupScript.Startup.DB.AllCardsData[n];
                CardItem.SetFullCard(data);
                container.AddChild(CardItem);
                CardsList.Add(CardItem);
            }
        }

        private int CheckIdExistance(int id)
        {
            var item = StartupScript.Startup.DB.MyCardDeck.Find(x => x.Equals(id));           
            return item == 0 ? 0 : item;
        }

        public void LoadDeck()
        {
            if (savedCardsList.Count <= 0)
                return;
            
            var container = GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG")
                        .GetNode<TextureRect>("DeckHolderFrame").GetNode<ScrollContainer>("ScrollContainer")
                            .GetNode<VBoxContainer>("VBoxContainer");
            for (int k = 0; k < savedCardsList.Count; k++)
            {
                if (savedCardsList[k] != 0)
                {
                    CardNewScene = (PackedScene)GD.Load("res://Cards_UI/SCENES/CardNewForDeck.tscn");
                    CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
                    CardData data = StartupScript.Startup.DB.AllCardsData.Find(x => x.CardID.Equals(savedCardsList[k]));
                    CardItem.SetShortCard(data);
                    container.AddChild(CardItem);

                    StartupScript.Startup.DB.MyCardDeck.Add(data.CardID);
                }
            }

            DoneButton();
            GreyoutCards();
        }

        public void AddToDeck(int id)
        {
            // Common setup for loading scene and creating card item
            CardNewScene = (PackedScene)GD.Load("res://Cards_UI/SCENES/CardNewForDeck.tscn");
            CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
            CardData data = StartupScript.Startup.DB.AllCardsData.Find(x => x.CardID.Equals(id));
            CardItem.SetShortCard(data);

            // Play sound
            SoundManager.Instance.PlaySoundByName("ButtonSound2");

            if (StartupScript.Startup.DB.MyCardDeck.Count == 0)
            {
                // Deck is empty
                GD.Print("data.CardID = " + data.CardID);
                GD.Print("ADDED ON ZERO" + StartupScript.Startup.DB.MyCardDeck.Count);
                StartupScript.Startup.DB.MyCardDeck.Add(data.CardID);

                // Add card to UI
                GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG")
                    .GetNode<TextureRect>("DeckHolderFrame").GetNode<ScrollContainer>("ScrollContainer")
                    .GetNode<VBoxContainer>("VBoxContainer").AddChild(CardItem);

                GreyoutCards();
            }
            else
            {
                // Deck is not empty
                int cardId = CheckIdExistance(id);

                if (cardId == 0 && StartupScript.Startup.DB.MyCardDeck.Count < 9)
                {
                    // Card doesn't exist in the deck, and the deck size is less than 9
                    StartupScript.Startup.DB.MyCardDeck.Add(data.CardID);

                    // Add card to UI
                    GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG")
                        .GetNode<TextureRect>("DeckHolderFrame").GetNode<ScrollContainer>("ScrollContainer")
                        .GetNode<VBoxContainer>("VBoxContainer").AddChild(CardItem);

                    GreyoutCards();
                }
            }

            // Call DoneButton regardless of conditions
            DoneButton();
        }

        public void GreyoutCards()
        {
            for (int i = 0; i < CardsList.Count; i++)
            {
                for (int y = 0; y < StartupScript.Startup.DB.MyCardDeck.Count; y++)
                {
                    if (CardsList[i].CardID == StartupScript.Startup.DB.MyCardDeck[y])
                    {
                        CardsList[i].GetChild(0).GetNode<TextureRect>("TextureRect").SelfModulate = new Color("6e808080");
                        
                    }                   
                }               
            }
        }

        public void revertGreyout(int id)
        {
            for (int i = 0; i < CardsList.Count; i++)
            {
                for (int y = 0; y <= StartupScript.Startup.DB.MyCardDeck.Count; y++)
                {
                    if (CardsList[i].CardID == id)
                    {
                        CardsList[i].GetChild(0).GetNode<TextureRect>("TextureRect").SelfModulate = new Color("ffffff");
                    }
                }
            }
        }

        public void RemoveFromDeck(int id)
        {
            var data = StartupScript.Startup.DB.MyCardDeck.Find(x => x.Equals(id));
            StartupScript.Startup.DB.MyCardDeck.Remove(data);
            DoneButton();
            revertGreyout(id);
        }

        public void DoneButton()
        {
            if (StartupScript.Startup.DB.MyCardDeck.Count == 9)
            {
                GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG").GetNode<Control>("DoneButton").Show();
            }
            else
            {
                GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG").GetNode<Control>("DoneButton").Hide();
            }
        }

        public void Save()
        {

            if (StartupScript.Startup.DB.MyCardDeck.Count == 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    CF.SetValue("CardDeck", "CardId" + (i + 1), 0);
                }
                CF.Save("user://" + CFName);
            }
            else
            {
                for (int i = 0; i < StartupScript.Startup.DB.MyCardDeck.Count; i++)
                {
                    CF.SetValue("CardDeck", "CardId" + (i + 1), StartupScript.Startup.DB.MyCardDeck[i]);
                }

                for (int j = StartupScript.Startup.DB.MyCardDeck.Count; j < 9; j++)
                {
                    CF.SetValue("CardDeck", "CardId" + (j + 1), 0);
                }

                CF.Save("user://" + CFName);
            }
        }

        public void CardZoomIn(int CardId)
        {      
            GetNode<Control>("BackButton").Hide();
            GetNode<Control>("CardZoomPanel").Show();
            CardNewScene = (PackedScene)GD.Load("res://Cards_UI/SCENES/CardNewZoom.tscn");
            CardZoomItem = (CardButtonUI)CardNewScene.Instance();
            CardData data = StartupScript.Startup.DB.AllCardsData.Find(x => x.CardID.Equals(CardId));
            CardZoomItem.SetFullCard(data);
            SetZoomCardId(data.CardID);
            GetNode<Control>("CardZoomPanel").GetNode<TextureRect>("TextureRect").GetNode<Control>("ZoomButtons").AddChild(CardZoomItem);
            CardZoomItem.RectPosition = new Vector2(180f, 0f);
            CardZoomItem.RectScale = new Vector2(2.5f, 2.5f);
            HideAddBtn();
        }

        public async void _on_AddButton_pressed()
        {
            AddToDeck(GetZoomCardID());
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            GetNode<Control>("CardZoomPanel").Hide();
            GetNode<Control>("BackButton").Show();
            foreach (CardButtonUI child in GetNode<Control>("CardZoomPanel").GetNode<TextureRect>("TextureRect").GetNode<Control>("ZoomButtons").GetChildren())
            {
                child.QueueFree();
            }
        }

        public void _on_CancelButton_pressed()
        {
            var zoomBtns = GetNode<Control>("CardZoomPanel").GetNode<TextureRect>("TextureRect").GetNode<Control>("ZoomButtons");
            foreach (CardButtonUI child in zoomBtns.GetChildren())
            {
                child.QueueFree();
            }
            GetNode<Control>("CardZoomPanel").Hide();
            GetNode<Control>("BackButton").Show();
        }

        public void HideAddBtn()
        {
            for (int y = 0; y < StartupScript.Startup.DB.MyCardDeck.Count; y++)
            {
                if ( GetZoomCardID() == StartupScript.Startup.DB.MyCardDeck[y])
                {
                    GetNode<Control>("CardZoomPanel").GetNode<TextureRect>("TextureRect").GetNode<Control>("AddButton").Hide();
                    break;
                }
                else
                {
                    GetNode<Control>("CardZoomPanel").GetNode<TextureRect>("TextureRect").GetNode<Control>("AddButton").Show();
                }
            }
        }

        private void SetZoomCardId(int cardID)
        {
            zoomCardID = cardID;           
        }

        private int GetZoomCardID()
        {
            return zoomCardID;
        }



    }
}
