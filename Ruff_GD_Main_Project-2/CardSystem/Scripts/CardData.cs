using Godot;
using System;

namespace RuffGdMainProject.DataClasses
{
    public class CardData
    {
        public int CardID { get; private set; }
        public CardType CardType { get; private set; }
        public CardSubType CardSubType { get; private set; }
        public string CardName { get; private set; }
        public string CardDescription { get; private set; }
        public int ManaCost { get; private set; }
        public string CardImagePath { get; private set; } //Sprite Path

        public float BaseDamage { get; private set; }
        public float BaseHp { get; private set; }
        public float BaseEvasion { get; private set; }
        
        public CardData()
        {
            CardID = 0;
            CardType = CardType.Manual;
            CardSubType = CardSubType.STRENGTH;
            CardName = "Basic Attack";
            CardDescription = "Performs Basic Attack";
            ManaCost = 1;
            CardImagePath = null;
            BaseDamage = 1;
            BaseHp = 0;
            BaseEvasion = 0;
        }

        public CardData(int cardID, CardType cardType, CardSubType cardSubType, string cardName,
            string description, int cost, string cardImagePath, float baseDamage, float baseHp, float baseEvasion)
        {
            CardID = cardID;
            CardType = cardType;
            CardSubType = cardSubType;
            CardName = cardName;
            CardDescription = description;
            ManaCost = cost;
            CardImagePath = cardImagePath;
            BaseDamage = baseDamage;
            BaseHp = baseHp;
            BaseEvasion = baseEvasion;
        }

        public void MakeConeAttack()
        {
            CardName = "Cone Attack";
            CardID=-1;
        }
    }
}