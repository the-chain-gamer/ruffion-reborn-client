using System;

namespace RuffGdMainProject.DataClasses
{
    public class UnitData
    {
        // Properties of the unit data
        public int UnitID { get; }
        public BREED UnitBreed { get; }
        public SUB_BREED UnitSubBreed { get; }
        public string UnitName { get; }
        public string ProfilePic { get; }
        public string UnitBodySprite { get; }
        public Stats UnitStats { get; }

        // Constructor to initialize the unit data
        public UnitData(int id, BREED breed, SUB_BREED subBreed, string uName, string profileSprite, string bodySprite)
        {
            this.UnitID = id;
            this.UnitBreed = breed;
            this.UnitSubBreed = subBreed;
            this.UnitName = uName;
            this.ProfilePic = profileSprite;
            this.UnitBodySprite = bodySprite;
            this.UnitStats = SetupStats(subBreed);
        }

        // Sets up the stats based on the sub-breed
        private Stats SetupStats(SUB_BREED subBread)
        {
            if (subBread == SUB_BREED.CORGI)
            {
                return new Stats(2, 6, 5, 2, 5, 6, 100, 5);
            }
            else if (subBread == SUB_BREED.SHIBA)
            {
                return new Stats(3, 6, 7, 2, 7, 5, 100, 5);
            }
            else if (subBread == SUB_BREED.BEAGLE)
            {
                return new Stats(2, 6, 5, 2, 6, 5, 100, 5);
            }
            else if (subBread == SUB_BREED.GOLDENRETRIEVER)
            {
                return new Stats(4, 3, 6, 4, 5, 3, 100, 5);
            }
            else if (subBread == SUB_BREED.LABRADOR)
            {
                return new Stats(3, 4, 5, 4, 4, 5, 100, 5);
            }
            else if (subBread == SUB_BREED.POODLE)
            {
                return new Stats(3, 5, 5, 3, 5, 4, 100, 5);
            }
            else if (subBread == SUB_BREED.MALAMUTE)
            {
                return new Stats(5, 3, 3, 6, 4, 4, 100, 5);
            }
            else if (subBread == SUB_BREED.MASTIFF)
            {
                return new Stats(6, 3, 4, 6, 4, 2, 100, 5);
            }
            else if (subBread == SUB_BREED.AFGHANHOUND)
            {
                return new Stats(4, 5, 4, 5, 5, 2, 100, 5);
            }

            return null;
        }
    }

    // Enum to represent the breed of the unit
    public enum BREED
    {
        SMALL = 0,
        MEDIUM = 1,
        LARGE = 2
    }

    // Enum to represent the sub-breed of the unit
    public enum SUB_BREED
    {
        SHIBA = 0,
        CORGI = 1,
        BEAGLE = 2,
        POODLE = 3,
        GOLDENRETRIEVER = 4,
        LABRADOR = 5,
        MALAMUTE = 6,
        AFGHANHOUND = 7,
        MASTIFF = 8
    }

    public class Stats
    {
        // Properties of the unit's stats
        public int Strength { get; }
        public int Speed { get; }
        public int Science { get; }
        public int Size { get; }
        public int Smarts { get; }
        public int Sorcery { get; }
        public int HP { get; }
        public int BasePhyDmg { get; }

        // Constructor to initialize the unit's stats
        public Stats(int str, int spd, int sci, int size, int smart, int sorcery, int hp, int basePhyDmg)
        {
            this.Strength = str;
            this.Speed = spd;
            this.Science = sci;
            this.Size = size;
            this.Smarts = smart;
            this.Sorcery = sorcery;
            this.HP = hp;
            this.BasePhyDmg = basePhyDmg;
        }
    }
}
