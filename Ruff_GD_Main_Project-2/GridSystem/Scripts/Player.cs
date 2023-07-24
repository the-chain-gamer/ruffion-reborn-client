using System;
using System.Collections.Generic;
using Godot;
using RuffGdMainProject.DataClasses;



namespace RuffGdMainProject.GridSystem
{
    /// <summary>
    /// Class represents a game participant.
    /// </summary>
    public class Player : Node2D
    {
        public bool IsLocalPlayer { get; private set; }
        public string OnlineID { get; set; }

        [Export]
        public int PlayerNumber;
        [Export]
        public int[] SpawnPos;

        public int TotalTurnsTaken { get; private set; }
        public bool IsMyTurn { get; private set; }

        public int TotalMana { get; private set; }

        public Unit CurrentUnit { get; private set; }
        public List<Unit> MyUnits;
        private PackedScene unitPrefab;
        // private List<UnitData> myUnitsData;

        public override void _Ready(){
            IsLocalPlayer = false;
            MyUnits = new List<Unit>();
            unitPrefab = (PackedScene)GD.Load("res://GridSystem/Scenes/UnitScene.tscn");
            IsMyTurn = false;
            // if(PlayerNumber == 1)
            // {
            //     myUnitsData = new List<UnitData>();
            //     for (int i = 0; i < GlobalData.GD.CharacterList.Count;i++ )
            //     {
            //         for (int j = 0; j < GlobalData.GD.myNum.Count  ; j++)
            //         {
            //             if (GlobalData.GD.CharacterList[i].UnitID == GlobalData.GD.myNum[j])
            //             {
            //                 myUnitsData.Add(
            //                     new UnitData(GlobalData.GD.CharacterList[i].UnitID, GlobalData.GD.CharacterList[i].UnitBreed, GlobalData.GD.CharacterList[i].UnitSubBreed, 
            //                     GlobalData.GD.CharacterList[i].UnitName, GlobalData.GD.CharacterList[i].ProfilePic, GlobalData.GD.CharacterList[i].UnitBodySprite));

            //                 // GD.Print("++++++++++++++  Shiba ID  +++++++++++++++", GlobalData.GD.CharacterList[i].UnitName);
            //                 // GD.Print("========================================myUnitsData========================================", myUnitsData.Count);
            //             }
            //         }
            //     }
            // }
            // else
            // {
            //     myUnitsData = new List<UnitData>
            //     {
            //         new UnitData(4, BREED.SMALL, SUB_BREED.LABRADOR, "Labrador",
            //             "res://CommonAssets/CharacterSprites/Labrador/Labrador_profile.png",
            //             "res://CommonAssets/CharacterSprites/Labrador/labrador4.png"),
            //         new UnitData(5, BREED.MEDIUM, SUB_BREED.POODLE, "Poodle",
            //             "res://CommonAssets/CharacterSprites/Poodle/poodle_profile.png",
            //             "res://CommonAssets/CharacterSprites/Poodle/poodle3.png"),
            //         new UnitData(7, BREED.LARGE, SUB_BREED.AFGHANHOUND, "Hound",
            //             "res://CommonAssets/CharacterSprites/Hound/Hound_profile.png",
            //             "res://CommonAssets/CharacterSprites/Hound/hound3.png")
            //     };
            // }
        }

        public void MakeLocalPlayer()
        {
            IsLocalPlayer = true;
        }
        public void SpawnUnits(List<UnitDataModel> unitLst)
        {
            Node2D tilesParent = GridManager.GM.GetNode<Node2D>("TilesParent");
            // GD.Print("UNITLST.Count >>>> " + unitLst.Count);

            // foreach (var item in unitLst)
            // {
            //     UnitData search = GlobalData.GD.CharacterList.Find(x => x.UnitID.Equals(Convert.ToInt32(item.DogId)));
            //     Logger.UiLogger.Log(Logger.LogLevel.INFO, "Search Dog ID = " + search.UnitID);
            //     GD.Print("search item ID = " + search.UnitID);
            // }

            for (int i = 0; i < unitLst.Count; i++)
            {
                var unit = (Unit)unitPrefab.Instance();
                tilesParent.GetChild(0).AddChild(unit);
                MyUnits.Add(unit);
                // GD.Print("DOG ID  = " + unitLst[i].DogId);
                // GD.Print("DOG Data  = " + myUnitsData[i].UnitID);
                UnitData search = GlobalData.GD.CharacterList.Find(x => x.UnitID.Equals(Convert.ToInt32(unitLst[i].DogId)));
                Logger.UiLogger.Log(Logger.LogLevel.INFO, "Search Dog ID = " + search.UnitID);
                unit.Init(this, GridManager.GM.GetTilePos(SpawnPos[i]), search);
            }
        }

        public void MarkMyTurn()
        {
            TotalMana = TotalTurnsTaken;
            GD.Print("Player" + PlayerNumber + " Total Mana = " + TotalMana);
            foreach(Unit u in MyUnits)
            {
                u.Reset();
            }
            IsMyTurn = true;
            var lst = MyUnits.FindAll(x => !x.IsDead);
            // var random = new RandomNumberGenerator();
            // random.Randomize();
            // var num = random.RandiRange(1, lst.Count);
            CurrentUnit = MyUnits[1];
            CurrentUnit.MarkMyTurn();
        }

        public void SkipUnit()
        {
            GridManager.GM.CanSelectCell = false;
            // GD.Print("Skipping " + MyUnits[MyUnits.Count-1] + " 's turn = ");
            CurrentUnit.MarkAsFinished();
            var unitsLst = MyUnits.FindAll(x=> !x.HasTakenTurn);
            if(unitsLst.Count > 0)
            {
                CurrentUnit = unitsLst[unitsLst.Count-1];
                CurrentUnit.MarkMyTurn();
                GridManager.GM.UiController.ToggleCardsList(false);
            }
            else
            {
                GridManager.GM.TM.RequestChangeTurn();
            }
        }

        public void SwitchUnit(Unit newUnit)
        {
            GridManager.GM.CanSelectCell = false;
            CurrentUnit.MarkAsFinished();
            CurrentUnit.Cell.MarkAsDeselected();
            GridManager.GM.ResetGridVisuals();
            CurrentUnit = newUnit;
            GridManager.GM.SelectedUnit = CurrentUnit;
            // GridManager.GM.OnUnitSelected(CurrentUnit.Cell);
            CurrentUnit.MarkMyTurn(true);
            GridManager.GM.UiController.UpdateProfilePic(PlayerNumber, CurrentUnit);
        }

        public void SkipPlayerTurn()
        {
            GridManager.GM.CanSelectCell = false;
            // GD.Print("Skipping Current Player's turn = ");
            GridManager.GM.TM.RequestChangeTurn();
        }

        public void OnTurnEnded()
        {
            TotalTurnsTaken++;
            IsMyTurn = false;
            // GD.Print("OnTurnEnded() ) " + MyUnits[MyUnits.Count-1] + " 's turn = ");
            GridManager.GM.CheckConeLife(PlayerNumber);
            foreach (var u in MyUnits)
            {
                u.MarkAsFinished();
                u.ConsumeCloak();
            }
        }

        public void ConsumeMana(int mana)
        {
            TotalMana -= mana;
            if(TotalMana < 0)
            {
                TotalMana = 0;
            }
            GridManager.GM.UiController.UpdateMana(PlayerNumber, TotalMana);
        }

        public void DoAllyAttack()
        {
            foreach(Unit u in MyUnits)
            {
                if(!u.IsAttackedByMadDog)
                {
                    //make Mad Dog!
                    u.DealDamage(1, true);
                }
            }
        }

        public void ApplyRandomShieldDefence(int oppCardsNum)
        {
            var random = new System.Random();
            var units = MyUnits.FindAll(x=>x.UnitId != CurrentUnit.UnitId);
            var id = random.Next(units.Count);
            var buff = units[id].data.UnitStats.Smarts * oppCardsNum;
            units[id].AddConsumableShield(buff, 2);
        }
    }
}