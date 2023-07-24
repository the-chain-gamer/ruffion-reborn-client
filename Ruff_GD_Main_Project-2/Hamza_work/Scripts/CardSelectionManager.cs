using Godot;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GameScene;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.UiSystem
{
    public class CardSelectionManager : Control
    {
        public static CardSelectionManager instance;
        public int CardsLimit = 9;
        public PackedScene CardNewScene;

        public int returnValue = -1;

        //Code Testing 
        List<int> cardSaveIdsList;
        int x;
        private ConfigFile CF = new ConfigFile();
        [Export]
        public string CFName = "CardSaveIDS.cfg";


         //******************************
        public List<CardButtonUI> CardsList;
        //******************************

        public override void _Ready()
        {

            CardsList = new List<CardButtonUI>();



            instance = this;

            //Code Testing 

            if (CF.Load("user://" + CFName) != Error.Ok)
            {
                Save();
               LoadCardIDS();
            }
            else
            {
                // GD.Print("Every Time");
                LoadCardIDS();
            }

            //init card DB/DataObjects
            IntantiateCards();
            DoneButton();
            LoadDeckWithDelay();
        }

        // public override void _Ready()
        // {

        //     CardsList = new List<CardButtonUI>();



        //     instance = this;

        //     //Code Testing 

        //     if (CF.Load("user://" + CFName) != Error.Ok)
        //     {
        //         // Save();
        //     }
        //     else
        //     {
        //         LoadCardIDS();
        //     }

        //     //init card DB/DataObjects
        //     IntantiateCards();
        //     DoneButton();
        //     LoadDeckWithDelay();
        // }


        public async void LoadDeckWithDelay()
        {
            await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
            LoadDeck();
        }

        public void LoadCardIDS()
        {
            cardSaveIdsList = new List<int>();
            // GD.Print("Loading data from:  " + OS.GetUserDataDir() + "/" + CFName);
            for (int i = 0; i < 9; i++)
            {
                int xValue = (int)CF.GetValue("CardDeck", "CardId" + (i + 1), x);
                cardSaveIdsList.Add(xValue);
            }
            //
            //int yValue = (int)CF.GetValue("Main", "ResolutionHeight", y);

            // //Print list
            // for (int j = 0; j < 9; j++)
            // {
            //     GD.Print("Card Ids in start" + cardSaveIdsList[j]);
            // }
        }



        public void IntantiateCards()
        {
            for (int n = 2; n < StartupScript.Startup.DB.AllCardsData.Count; n++)
            {
                CardNewScene = (PackedScene)GD.Load("res://Hamza_work/SCENES/CardNew.tscn");
                CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
                CardData data = StartupScript.Startup.DB.AllCardsData[n];
               CardItem.GetNode("TextureButton").Connect("pressed", this, "AddToDeck", new Godot.Collections.Array { data.CardID });
                CardItem.SetFullCard(data);
                GetNode<Control>("Control").GetNode<TextureRect>("AllCardsHolderBG").GetNode<TextureRect>("AllCardsHolderFrame").
                    GetNode<ScrollContainer>("ScrollContainer").GetNode<GridContainer>("GridContainer").AddChild(CardItem);
                // GD.Print("Loop index :   " + n);

                CardsList.Add(CardItem);
            }
        }

        public void Testfunc()
        {
            // GD.Print("Hover");
        }

        private int CheckIdExistance(int id)
        {
            GD.Print(id);
            var item = StartupScript.Startup.DB.MyCardDeck.Find(x => x.Equals(id));
            GD.Print(item);
            return item == 0 ? 0 : item;
        }

        public void LoadDeck()
        {
            // GD.Print("Card Ids Count = " + cardSaveIdsList.Count);
            // GD.Print("Button Pressed");
            if(cardSaveIdsList.Count <=0)
                return;
            for (int k = 0; k < cardSaveIdsList.Count; k++)
            {
                if (cardSaveIdsList[k] != 0)
                {
                    CardNewScene = (PackedScene)GD.Load("res://Hamza_work/SCENES/CardNewForDeck.tscn");
                    CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
                    CardData data = StartupScript.Startup.DB.AllCardsData.Find(x => x.CardID.Equals(cardSaveIdsList[k]));
                    // GD.Print("Card ID is" + data.CardID);
                    CardItem.SetShortCard(data);
                    GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG")
                        .GetNode<TextureRect>("DeckHolderFrame").GetNode<ScrollContainer>("ScrollContainer")
                            .GetNode<VBoxContainer>("VBoxContainer").AddChild(CardItem);
                    // GD.Print("data.CardID = " + data.CardID);
                    // GD.Print("ADDED ON ZERO" + CardsDB.CARD_DB.MyCardDeck.Count);
                    // CardsDB.CARD_DB.MyCardDeck.Add(data.CardID);
                    StartupScript.Startup.DB.MyCardDeck.Add(data.CardID);
                }
            }
           
            DoneButton();
        }

        public void AddToDeck(int id)
        {
            GD.Print("Card Ids Count = " + StartupScript.Startup.DB.MyCardDeck.Count);
            GD.Print("Button Pressed");
            if (StartupScript.Startup.DB.MyCardDeck.Count == 0)
            {
                SoundManager.Instance.PlaySoundByName("ButtonSound2");
                CardNewScene = (PackedScene)GD.Load("res://Hamza_work/SCENES/CardNewForDeck.tscn");
                CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
                CardData data = StartupScript.Startup.DB.AllCardsData.Find(x => x.CardID.Equals(id));
                CardItem.SetShortCard(data);
                GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG")
                    .GetNode<TextureRect>("DeckHolderFrame").GetNode<ScrollContainer>("ScrollContainer")
                        .GetNode<VBoxContainer>("VBoxContainer").AddChild(CardItem);
                GD.Print("data.CardID = " + data.CardID);
                GD.Print("ADDED ON ZERO" + StartupScript.Startup.DB.MyCardDeck.Count);
                StartupScript.Startup.DB.MyCardDeck.Add(data.CardID);
            }
            else
            {
                int cardId = CheckIdExistance(id);
                if (cardId == 0)
                {
                    if (StartupScript.Startup.DB.MyCardDeck.Count < 9)
                    {
                        SoundManager.Instance.PlaySoundByName("ButtonSound2");
                        CardNewScene = (PackedScene)GD.Load("res://Hamza_work/SCENES/CardNewForDeck.tscn");
                        CardButtonUI CardItem = (CardButtonUI)CardNewScene.Instance();
                        CardData data = StartupScript.Startup.DB.AllCardsData.Find(x => x.CardID.Equals(id));
                        CardItem.SetShortCard(data);
                        GetNode<Control>("Control").GetNode<TextureRect>("DeckHolderBG")
                            .GetNode<TextureRect>("DeckHolderFrame").GetNode<ScrollContainer>("ScrollContainer")
                                .GetNode<VBoxContainer>("VBoxContainer").AddChild(CardItem);
                        StartupScript.Startup.DB.MyCardDeck.Add(data.CardID);
                    }
                }
                GD.Print("ADDED THROUGH ELSE" + StartupScript.Startup.DB.MyCardDeck.Count);
            }
            DoneButton();
        }

        public void RemoveFromDeck(int id)
        {
            var data = StartupScript.Startup.DB.MyCardDeck.Find(x => x.Equals(id));
            StartupScript.Startup.DB.MyCardDeck.Remove(data);
            DoneButton();
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
            // for (int i = 0; i < CardsDB.CARD_DB.MyCardDeck.Count; i++)
            // {
            //     CF.SetValue("CardDeck", "CardId" + (i + 1), CardsDB.CARD_DB.MyCardDeck[i]);
            // }

            // for (int j = CardsDB.CARD_DB.MyCardDeck.Count; j < 9; j++)
            // {
            //     CF.SetValue("CardDeck", "CardId" + (j + 1), 0);
            // }

            // CF.Save("user://" + CFName);
            // GD.Print("Saving data to:  " + OS.GetUserDataDir() + "/" + CFName);
            if (StartupScript.Startup.DB.MyCardDeck.Count == 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    CF.SetValue("CardDeck", "CardId" + (i + 1), 0);
                }
                CF.Save("user://" + CFName);
                GD.Print("Saving data to:  " + OS.GetUserDataDir() + "/" + CFName);
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
                GD.Print("Saving data to:  " + OS.GetUserDataDir() + "/" + CFName);
            }
        }

    }
}
