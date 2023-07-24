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
                AllCardsData.Add(new CardData(0, CardType.Manual,CardSubType.NONE,"Basic Attack","Basic Attack",1,
                    null, 7.0f,0,0));//Basic Attack Card. It has 1 mana cost and 000 CardID
                AllCardsData.Add(new CardData(-1, CardType.Manual,CardSubType.NONE,"Cone Attack","Basic Attack",1,
                    null, 7.0f,0,0));//Basic Attack Card. It has 1 mana cost and 000 CardID
                AllCardsData.Add(new CardData(101, CardType.Manual,CardSubType.DEFEND,"Smolstein's Big Hit","3",3,
                    "res://Hamza_work/CartridgesRobo/1_cartridgeModSmolrobo.png", 3,5,5));
                AllCardsData.Add(new CardData(102, CardType.Auto, CardSubType.DEFEND, "Lalito's Laser", "5", 5,
                    "res://Hamza_work/CartridgesRobo/2_cartridgeLalitoModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(103, CardType.Manual, CardSubType.SCIENCE, "Cyril's Hat Trick", "3", 3,
                    "res://Hamza_work/CartridgesRobo/3_cartridgeCyrilModrobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(104, CardType.Auto, CardSubType.SCIENCE, "Marlon's Smol Smoke Bomb", "4", 4,
                    "res://Hamza_work/CartridgesRobo/4_cartridgeMarlonModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(105, CardType.Auto, CardSubType.SMARTS, "Kenzo's Smol Shield", "4", 4,
                    "res://Hamza_work/CartridgesRobo/5_cartridgeKenzoModrobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(106, CardType.Manual, CardSubType.SCIENCE, "Wiz's Smol Telescope", "6", 6,
                    "res://Hamza_work/CartridgesRobo/6_cartridgeWizModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(107, CardType.Manual, CardSubType.SMARTS, "Bubbles' Pick 'n' Drop", "4", 4,
                    "res://Hamza_work/CartridgesRobo/7_cartridgeBubblesModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(108, CardType.Manual, CardSubType.STRENGTH, "Bethany's Road block", "4", 4,
                    "res://Hamza_work/CartridgesRobo/8_cartridgeBethanyModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(109, CardType.Manual, CardSubType.STRENGTH, "Pinky's Smol Brain", "5", 5,
                    "res://Hamza_work/CartridgesRobo/9_cartridgePinkyModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(110, CardType.Manual, CardSubType.SMARTS, "Hook's Destructor", "10", 10,
                    "res://Hamza_work/CartridgesRobo/10_cartridgeHookModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(111, CardType.Manual, CardSubType.SPEED, "Penelope's Gamble", "2", 2,
                    "res://Hamza_work/CartridgesRobo/11_cartridgepenelopeModrobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(112, CardType.Auto, CardSubType.STRENGTH, "Crosby's Smokescreen", "3", 3,
                    "res://Hamza_work/CartridgesRobo/12_cartridgecrosbyModRobo.png", 3, 5, 5));
                AllCardsData.Add(new CardData(113, CardType.Auto, CardSubType.SMARTS, "Colonel Saunders", "7", 7,
                    "res://Hamza_work/CartridgesRobo/13_Saunders.png", 3, 5, 5));
                AllCardsData.Add(new CardData(114, CardType.Auto, CardSubType.SMARTS, "Catten", "9", 9,
                    "res://Hamza_work/CartridgesRobo/14_Catten.png", 3, 5, 5));
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