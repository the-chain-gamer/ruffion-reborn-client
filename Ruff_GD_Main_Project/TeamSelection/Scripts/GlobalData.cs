using Godot;
using System;
using System.Collections.Generic;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GridSystem;

public class GlobalData
{
    [Export] public List<int> myNum = new List<int>();
    [Export] public int[] SpawnCellIndexs = { 11, 21, 29, 14, 23, 32 };
    [Export] public Camera2D camera;

    public List<UnitData> CharacterList;

    public static GlobalData GD ;

    public GlobalData()
    {
        CharacterList = new List<UnitData>();
        GD = this;
        AddToList();
    }
    public void addItem(int num)
    {
        myNum.Add(num);
    }

    public List<int> characterStoreIds()
    {
        return myNum;
    }
    public void AddToList()
    {
        CharacterList.Add(new UnitData(1, BREED.MEDIUM, SUB_BREED.BEAGLE, "Beagle",
                "res://CommonAssets/CharacterSprites/Beagle/Beagle_profile.png",
                "res://CommonAssets/CharacterSprites/Beagle/beagle2.png"));
        CharacterList.Add(new UnitData(2, BREED.LARGE, SUB_BREED.CORGI, "Corgi",
                "res://CommonAssets/CharacterSprites/Corgi/Corgi_profile.png",
                "res://CommonAssets/CharacterSprites/Corgi/corgis2 .png"));
        CharacterList.Add(new UnitData(3, BREED.MEDIUM, SUB_BREED.GOLDENRETRIEVER, "Golden",
                "res://CommonAssets/CharacterSprites/GoldenR/GoldenRetriever_profile.png",
                "res://CommonAssets/CharacterSprites/GoldenR/golden retriver3 .png"));
        CharacterList.Add(new UnitData(4, BREED.SMALL, SUB_BREED.LABRADOR, "Labrador",
                "res://CommonAssets/CharacterSprites/Labrador/Labrador_profile.png",
                "res://CommonAssets/CharacterSprites/Labrador/labrador4.png"));
        CharacterList.Add(new UnitData(5, BREED.MEDIUM, SUB_BREED.POODLE, "Poodle",
                "res://CommonAssets/CharacterSprites/Poodle/poodle_profile.png",
                "res://CommonAssets/CharacterSprites/Poodle/poodle3.png"));
        CharacterList.Add(new UnitData(6, BREED.SMALL, SUB_BREED.SHIBA, "Shiba",
                "res://CommonAssets/CharacterSprites/Shiba/shiba_profile.png",
                "res://CommonAssets/CharacterSprites/Shiba/shiba idle.png"));
        CharacterList.Add(new UnitData(7, BREED.LARGE, SUB_BREED.AFGHANHOUND, "Hound",
                "res://CommonAssets/CharacterSprites/Hound/Hound_profile.png",
                "res://CommonAssets/CharacterSprites/Hound/hound3.png"));
        CharacterList.Add(new UnitData(8, BREED.LARGE, SUB_BREED.MALAMUTE, "Malamute",
                "res://CommonAssets/CharacterSprites/Malamute/Malamute_profile.png",
                "res://CommonAssets/CharacterSprites/Malamute/malamute3.png"));
        CharacterList.Add(new UnitData(9, BREED.LARGE, SUB_BREED.MASTIFF, "Mastiff",
                "res://CommonAssets/CharacterSprites/Mastiff/mastiff-profile-1.png",
                "res://CommonAssets/CharacterSprites/Mastiff/mastiff-1.png"));
       }

        public UnitData GetDataByID(int id)
        {
            return CharacterList.Find(x=>x.UnitID.Equals(id));
        }

        public List<UnitDataModel> GetUnitDataModel()
        {
            var lst = new List<UnitDataModel>();
            foreach (var c in myNum)
            {
                var d = GetDataByID(c);
                lst.Add(new UnitDataModel(d.UnitID.ToString(), d.UnitName, d.UnitStats.HP));
            }
            return lst;
        }

        public UnitDataModel GetUnitDataModel(Unit unit)
        {
            var model = new UnitDataModel(unit.UnitId.ToString(), unit.UnitName, (int)unit.TotalHitPoints, unit.Cell.TileNo);
            return model;
        }
}
