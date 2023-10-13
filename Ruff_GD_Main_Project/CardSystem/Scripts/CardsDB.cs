using Godot;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.DataClasses
{
    public class CardsDB
    {
        public List<CardData> AllCardsData = new List<CardData>(); //all game cards
        public List<int> MyCardDeck = new List<int>(); // my selected cards deck

        public CardsDB()
        {
            CreateCardsData();
        }

        private void CreateCardsData()
        {
                CreateSmolDeck();
                CreateSwolDeck();
        }

        private void CreateSwolDeck()
        {
            AllCardsData.Add(new CardData(201, CardType.Manual, CardSubType.STRENGTH, "HBL's Takedown", "9", 9,
                    "res://Cards_UI/Swol-Cards-Red/HBL-Takedown.png", 3, 5, 5));
            AllCardsData.Add(new CardData(202, CardType.Manual, CardSubType.STRENGTH, "Mittens - The Gloves Are Off", "3", 3,
                    "res://Cards_UI/Swol-Cards-Red/Mittens - The Gloves Are Off.png", 3, 5, 5));
            AllCardsData.Add(new CardData(203, CardType.Manual, CardSubType.STRENGTH, "Champ's Surprise", "4", 4,
                    "res://Cards_UI/Swol-Cards-Red/Champ-Surprise.png", 3, 5, 5));
            AllCardsData.Add(new CardData(204, CardType.Manual, CardSubType.STRENGTH, "Blondie Breaks A Heart Of Glass", "6", 6,
                    "res://Cards_UI/Swol-Cards-Red/Blondie-Breaks.png", 3, 5, 5));
            AllCardsData.Add(new CardData(205, CardType.Manual, CardSubType.STRENGTH, "Mr. Stache's Gloves of Doom", "4", 4,
                    "res://Cards_UI/Swol-Cards-Red/Mr. Stache's Gloves of Doom.png", 3, 5, 5));
            AllCardsData.Add(new CardData(206, CardType.Manual, CardSubType.STRENGTH, "Babeeee's Chair Toss", "3", 3,
                    "res://Cards_UI/Swol-Cards-Red/Babee-Chair-Toss.png", 3, 5, 5));
            AllCardsData.Add(new CardData(207, CardType.Manual, CardSubType.STRENGTH, "The Wolf's Lair", "4", 4,
                    "res://Cards_UI/Swol-Cards-Red/The Wolf's Lair.png", 3, 5, 5));
            AllCardsData.Add(new CardData(208, CardType.Manual, CardSubType.STRENGTH, "Miss Cake By The Ocean", "4", 4,
                    "res://Cards_UI/Swol-Cards-Red/Miss Cake By The Ocean.png", 3, 5, 5));
            AllCardsData.Add(new CardData(209, CardType.Auto, CardSubType.STRENGTH, "Viv's Weights", "6", 6,
                    "res://Cards_UI/Swol-Cards-Red/Viv's Weights.png", 3, 5, 5));
            AllCardsData.Add(new CardData(210, CardType.Manual, CardSubType.STRENGTH, "SwolMask's SWOL Mask of EEEE!", "6", 6,
                    "res://Cards_UI/Swol-Cards-Red/SwolMask's SWOL Mask of EEEE!.png", 3, 5, 5));
            AllCardsData.Add(new CardData(211, CardType.Auto, CardSubType.STRENGTH, "Mustachio's Throwdown", "4", 4,
                    "res://Cards_UI/Swol-Cards-Red/MustachioThrowdown.png", 3, 5, 5));
                    AllCardsData.Add(new CardData(212, CardType.Manual, CardSubType.STRENGTH, "Jock Smash", "10", 10,
                    "res://Cards_UI/Swol-Cards-Red/JockSmash.png", 3, 5, 5));
        }

        private void CreateSmolDeck()
        {
                AllCardsData.Add(new CardData(0, CardType.Manual,CardSubType.NONE,"Basic Attack","Basic Attack",1,
                    null, 7.0f,0,0));//Basic Attack Card. It has 1 mana cost and 000 CardID
                AllCardsData.Add(new CardData(-1, CardType.Manual,CardSubType.NONE,"Cone Attack","Basic Attack",1,
                    null, 7.0f,0,0));//Basic Attack Card. It has 1 mana cost and 000 CardID
                AllCardsData.Add(new CardData(101, CardType.Manual,CardSubType.DEFEND,"Smolstein's Big Hit","3",3,
                    "res://Cards_UI/CartridgesRobo/1_cartridgeModSmolrobo.png", 3,5,5));
                AllCardsData.Add(new CardData(102, CardType.Auto, CardSubType.DEFEND, "Lalito's Laser", "5", 5,
                    "res://Cards_UI/CartridgesRobo/2_cartridgeLalitoModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(103, CardType.Manual, CardSubType.SCIENCE, "Cyril's Hat Trick", "3", 3,
                    "res://Cards_UI/CartridgesRobo/3_cartridgeCyrilModrobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(104, CardType.Auto, CardSubType.SCIENCE, "Marlon's Smol Smoke Bomb", "4", 4,
                    "res://Cards_UI/CartridgesRobo/4_cartridgeMarlonModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(105, CardType.Auto, CardSubType.SMARTS, "Kenzo's Smol Shield", "4", 4,
                    "res://Cards_UI/CartridgesRobo/5_cartridgeKenzoModrobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(106, CardType.Manual, CardSubType.SCIENCE, "Wiz's Smol Telescope", "6", 6,
                    "res://Cards_UI/CartridgesRobo/6_cartridgeWizModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(107, CardType.Manual, CardSubType.SMARTS, "Bubbles' Pick 'n' Drop", "4", 4,
                    "res://Cards_UI/CartridgesRobo/7_cartridgeBubblesModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(108, CardType.Manual, CardSubType.STRENGTH, "Bethany's Road block", "4", 4,
                    "res://Cards_UI/CartridgesRobo/8_cartridgeBethanyModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(109, CardType.Manual, CardSubType.STRENGTH, "Pinky's Smol Brain", "5", 5,
                    "res://Cards_UI/CartridgesRobo/9_cartridgePinkyModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(110, CardType.Manual, CardSubType.SMARTS, "Hook's Destructor", "10", 10,
                    "res://Cards_UI/CartridgesRobo/10_cartridgeHookModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(111, CardType.Manual, CardSubType.SPEED, "Penelope's Gamble", "2", 2,
                    "res://Cards_UI/CartridgesRobo/11_cartridgepenelopeModrobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(112, CardType.Auto, CardSubType.STRENGTH, "Crosby's Smokescreen", "3", 3,
                    "res://Cards_UI/CartridgesRobo/12_cartridgecrosbyModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(113, CardType.Auto, CardSubType.SMARTS, "Colonel Saunders", "7", 7,
                    "res://Cards_UI/CartridgesRobo/13_Saunders.png", 3, 5, 5));
                AllCardsData.Add(new CardData(114, CardType.Auto, CardSubType.SMARTS, "Catten", "9", 9,
                    "res://Cards_UI/CartridgesRobo/14_Catten.png", 3, 5, 5));
        }

        public CardData GetDataByID(int id)
        {
            return AllCardsData.Find(x=>x.CardID.Equals(id));
        }

        public List<CardDataModel> GetCardDataModel()
        {
            var lst = new List<CardDataModel>();
            foreach (var c in MyCardDeck)
            {
                var d = GetDataByID(c);
                lst.Add(new CardDataModel(d.CardID, d.CardName));
            }
            return lst;
        }
    }
}