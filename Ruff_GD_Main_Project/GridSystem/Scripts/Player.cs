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
        }

        public void MakeLocalPlayer()
        {
            IsLocalPlayer = true;
        }
        public void SpawnUnits(List<UnitDataModel> unitLst)
        {
            Node2D tilesParent = GridManager.GM.GetNode<Node2D>("TilesParent");
            
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

        public void MarkMyTurn(int turnMana)
        {
            TotalMana = turnMana;
            GD.Print("Player" + PlayerNumber + " Total Mana = " + TotalMana);
            foreach(Unit u in MyUnits)
            {
                u.Reset();
            }
            IsMyTurn = true;
            var lst = MyUnits.FindAll(x => !x.IsDead);

            CurrentUnit = MyUnits[MyUnits.Count-1];
            CurrentUnit.MarkMyTurn();
        }

        public void MarkMyTurnOnClient()
        {
            foreach(Unit u in MyUnits)
            {
                u.Reset();
            }
        }

        public void SwitchUnit(Unit newUnit)
        {
            GridManager.GM.CanSelectCell = false;
            GridManager.GM.CanSelectTarget = false;
            GridManager.GM.CM.ClearEverything();
            // CurrentUnit.MarkAsFinished();
            CurrentUnit.Cell.MarkAsDeselected();
            GridManager.GM.ResetGridVisuals();
            CurrentUnit = newUnit;
            GridManager.GM.SelectedUnit = CurrentUnit;
            // GridManager.GM.OnUnitSelected(CurrentUnit.Cell);
            CurrentUnit.MarkMyTurn(true);
            GridManager.GM.UiController.UpdateProfilePic(PlayerNumber, CurrentUnit);
            if(newUnit.MovementPoints <= 0)
            {
                GridManager.GM.UiController.GreyoutButtons("MoveBtn", true);
            }
            else
            {
                GridManager.GM.UiController.GreyoutButtons("MoveBtn", false);
            }
        }

        public void SkipPlayerTurn()
        {
            GridManager.GM.CanSelectCell = false;
            // GD.Print("Skipping Current Player's turn = ");
            GridManager.GM.TM.RequestChangeTurn();
        }

        public void OnTurnEnded()
        {
            GridManager.GM.CM.ClearEverything();
            IsMyTurn = false;
            // GD.Print("OnTurnEnded() ) " + MyUnits[MyUnits.Count-1] + " 's turn = ");
            GridManager.GM.CheckConeLife(PlayerNumber);
            foreach (var u in MyUnits)
            {
                u.MarkAsFinished();
                // u.ConsumeCloak();
            }
        }

        public void ConsumeMana(int mana)
        {
            TotalMana -= mana;
            if(TotalMana < 0)
            {
                TotalMana = 0;
            }
            GridManager.GM.UiController.UpdateMana(TotalMana);
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