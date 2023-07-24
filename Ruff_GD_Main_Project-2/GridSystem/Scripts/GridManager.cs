using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using HexCore;
using System.Collections;
using RuffGdMainProject.UiSystem;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GameScene;

namespace RuffGdMainProject.GridSystem
{
    public class GridManager : Node2D
    {
        public static GridManager GM;
        public  TurnManager TM;
        public CardsManager CM;
        public GameplayUiController UiController;

        public bool CanSelectCell = false;
        public bool CanSelectTarget = false;
        public Unit SelectedUnit = null;
        public CameraShake camShake;
        public int ConesAddedTemp = 0;

        [Export(PropertyHint.PlaceholderText, "Tile List (ReadOnly)")]
        private List<Tile> TilesList;
        private TerrainType tileType;
        private MovementType movementType;
        private MovementTypes movementTypes;
        private Graph HexGridGraph;
        private PackedScene ConeScene;
        private List<Cone> ActiveRoadBlocks;
        private List<Coordinate2D> theShortestPath;
        // Called when the node enters the scene tree for the first time.
        public void Init()
        {
            StartEverything();
        }

        private async void StartEverything()
        {
            GM = this;
            TM = GetNode<TurnManager>("TurnManager");
            CM = GetNode<CardsManager>("CardsManager");
            UiController = GetNode<CanvasLayer>("CanvasLayer").GetChild<GameplayUiController>(0);
            camShake = GetNode<CameraShake>("Camera2D");
            // GD.Print("GetNode<CanvasLayer>('CanvasLayer').GetChild<GameplayUiController>(0) = " + GetNode<CanvasLayer>("CanvasLayer").GetChild<GameplayUiController>(0));
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            TilesList = new List<Tile>();
            ActiveRoadBlocks = new List<Cone>();
            tileType = new TerrainType(1, "Ground");
            movementType = new MovementType(1, "Walking");
            movementTypes = new MovementTypes(
                new[] {tileType},
                new Dictionary<MovementType, Dictionary<TerrainType, int>>
                {
                    [movementType] = new Dictionary<TerrainType, int>
                    {
                        [tileType] = 1
                    }
                }
            );
            ConeScene = (PackedScene)GD.Load("res://GridSystem/Scenes/Cone.tscn");
            // var array = new Godot.Collections.Array<Node>();
            // array = GetNode("TilesParent").GetChildren();
            var array = GetNode("TilesParent").GetChild(0).GetChildren().Cast<Tile>();
            TilesList = array.ToList();
            CreateGrid();
        }

        public async void SpawnPlayers(PlayerDataModel player1Data, PlayerDataModel player2Data, PlayerDataModel currentPlayer)
        {
            var player1 = GetNode<Player>("Player1");
            var player2 = GetNode<Player>("Player2");
            GD.Print("Player1 ID = " + player1Data.ID);
            GD.Print("Player2 ID = " + player2Data.ID);
            GD.Print("StartupScript.Startup.Multiplayer.Player ID = " + StartupScript.Startup.Multiplayer.PlayerId);

            if(player1Data.ID.Equals(StartupScript.Startup.Multiplayer.PlayerId))
            {
                player1.MakeLocalPlayer();
            }
            else if(player2Data.ID.Equals(StartupScript.Startup.Multiplayer.PlayerId))
            {
                player2.MakeLocalPlayer();
            }
            //set onlineID that server created
            player1.OnlineID = player1Data.ID;
            player2.OnlineID = player2Data.ID;

            // GD.Print("data.GameUpdates.Player1.Dogs = " + data.GameUpdates.Player1.Assets.Dogs.Count);
            // GD.Print("data.GameUpdates.Player2.Dogs = " + data.GameUpdates.Player2.Assets.Dogs.Count);

            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            player1.SpawnUnits(player1Data.Assets.Dogs);
            // await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            player2.SpawnUnits(player2Data.Assets.Dogs);

            TM.AddPlayers(player1);
            TM.AddPlayers(player2);
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            // Update profile pictures and show cards
            UiController.UpdateProfilePic(1, player1.MyUnits[0]);
            UiController.UpdateProfilePic(2, player1.MyUnits[0]);
            UiController.Init();

            if(player1.IsLocalPlayer)
            {
                CM.LoadCardDeck(player1);
                CM.ShowRandomCardsInContainer();
                TM.StartTurns(currentPlayer);
            }
            else
            {
                CM.LoadCardDeck(player2);
                CM.ShowRandomCardsInContainer();
                TM.StartTurns(currentPlayer);
            }
        }

        private void CreateGrid(){
            var maxRows = 5;
            var maxCols = 7;
            int count = 0;


            for(int row = 0; row < maxRows; row++) {
                // GD.Print("row " + row);
                if(row == 1 || row == 3){
                    maxCols = 6;
                }
                else {
                    maxCols = 7;
                }

                for(int col = 0; col < maxCols; col++) {
                    // GD.Print("col " + col);
                    TilesList[count].RowNum = row;
                    TilesList[count].ColNum = col;
                    TilesList[count].TileNo = count+1;
                    count++;
                }
            }

            CreateGraph();
        }

        private void CreateGraph() {

            // GD.Print("it's Create GRAPH :) :)");
            List<CellState> cellStates = new List<CellState>();
            foreach (var item in TilesList)
            {
                var state = new CellState(false, new Coordinate2D(item.ColNum, item.RowNum, OffsetTypes.OddRowsRight), tileType);
                cellStates.Add(state);
            }
            HexGridGraph = new Graph(cellStates, movementTypes);
        }
        private List<Tile> VisualizePath(Coordinate2D start, Coordinate2D goal, List<Coordinate2D> lst) {
            ResetGridVisuals();
            lst.Add(start);
            lst.Add(goal);
            List<Tile> tiles = new List<Tile>();
            foreach (var item in lst)
            {
                var p = TilesList.Find(x => x.ColNum.Equals(item.X) && x.RowNum.Equals(item.Y));
                tiles.Add(p);
                // p.MarkAsReachable();
            }
            return tiles;
        }

        private List<Tile> VisualizePath(Coordinate2D origion, List<Coordinate2D> lst) {
            ResetGridVisuals();
            List<Tile> tiles = new List<Tile>();
            foreach (var item in lst)
            {
                var p = TilesList.Find(x => x.ColNum.Equals(item.X) && x.RowNum.Equals(item.Y));
                tiles.Add(p);
                p.MarkAsReachable();
            }

            return tiles;
        }

        private int PathMovementCost(Coordinate2D start, Coordinate2D goal, List<Coordinate2D> lst)
        {
            lst.Add(start);
            lst.Add(goal);
            var cost = 0;
            foreach (var item in lst)
            {
                var p = TilesList.Find(x => x.ColNum.Equals(item.X) && x.RowNum.Equals(item.Y));
                cost += p.MovementCost;
                // p.MarkAsReachable();
            }
            return cost;
        }

        public void ResetGridVisuals() {
            foreach (var t in TilesList)
            {
                t.UnMark();
            }
        }

        public Tile GetTilePos(int tileId){
            // GD.Print("TilesList[tileId] NODE = " + (TilesList[tileId-1]));
            return TilesList[tileId-1];//TilesList[tileId].GetOwner<Node2D>().Position;
        }

        public void OnUnitSelected(Tile unitTile, Unit unit = null){
            // var pos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            // var neighbors = HexGridGraph.GetNeighbors(pos, unit == null);
            VisualizeMovementRange();
            CanSelectCell = true;
        }
        public void BlockTile(Tile tile, Unit unit){
            HexGridGraph.BlockCells(new Coordinate2D(tile.ColNum, tile.RowNum, OffsetTypes.OddRowsRight));
            tile.CurrentUnit = unit;
            tile.MarkAsHighlighted();
            tile.IsTaken = true;
        }

        public void UnblockTile(Tile tile){
            tile.IsTaken = false;
            tile.CurrentUnit = null; 
            tile.UnMark();
            tile.MarkAsPath(false);
            HexGridGraph.UnblockCells(new Coordinate2D(tile.ColNum, tile.RowNum, OffsetTypes.OddRowsRight));
        }

        public void DrawPathToTarget(Tile targetCell) {
            var unitPos = new Coordinate2D(SelectedUnit.Cell.ColNum, SelectedUnit.Cell.RowNum, OffsetTypes.OddRowsRight);
            var targetPos = new Coordinate2D(targetCell.ColNum, targetCell.RowNum, OffsetTypes.OddRowsRight);
            var path = HexGridGraph.GetShortestPath(unitPos, targetPos, movementType);
            VisualizePath(unitPos, targetPos, path);
        }

        public void MoveToTarget(Tile targetCell)
        {
            var unitPos = new Coordinate2D(SelectedUnit.Cell.ColNum, SelectedUnit.Cell.RowNum, OffsetTypes.OddRowsRight);
            var targetPos = new Coordinate2D(targetCell.ColNum, targetCell.RowNum, OffsetTypes.OddRowsRight);
            int totalCost;
            if (IsNeigbourTile(SelectedUnit.Cell, targetCell))
            {
                totalCost = 1;
            }
            else
            {
                var path = HexGridGraph.GetShortestPath(unitPos, targetPos, movementType);
                totalCost = PathMovementCost(unitPos, targetPos, path);
            }
            // GD.Print("TOTAL MOVEMENT COST = " + totalCost);
            if (HexGridGraph.IsWithinMovementRange(unitPos, targetPos, SelectedUnit.MovementPoints, movementType))
            {
                var unt = GlobalData.GD.GetUnitDataModel(SelectedUnit);
                unt.Tile = targetCell.TileNo;
                UnitDataModel[] array = {unt};
                _ = StartupScript.Startup.Multiplayer.MoveUnit(array);
                // SelectedUnit.Cell.TileNo = tmpNo;
                // SelectedUnit.Move(targetCell, totalCost);
            }
        }

        //multiplayer move
        public void MoveUnits(PlayerDataModel current)
        {
            GD.Print("MOVING UNITS @@@@");
            var player = TM.GetPlayerByID(current.ID);
            GD.Print("Player that we want to move belongs to playr " + player.PlayerNumber);
            GD.Print("Current Player Unit[0] tile  = " + current.Assets.Dogs[0].Tile);
            GD.Print("Current Player Unit[1] tile  = " + current.Assets.Dogs[1].Tile);
            GD.Print("Current Player Unit[2] tile  = " + current.Assets.Dogs[2].Tile);

            foreach (var u in player.MyUnits)
            {
                GD.Print("UnitID = " + u.UnitId + " Tile = " + u.Cell.TileNo);
                var tmp = current.Assets.Dogs.Find(x => x.DogId == u.UnitId.ToString());
                GD.Print("Iterating in loop}}}}}}}}} ");
                GD.Print("Target tile = " + tmp.Tile);
                if(tmp.Tile != -1 && tmp.Tile != u.Cell.TileNo)
                {
                    GD.Print("Moving my unit now ^^^^ ");
                    u.Move(GetTileFromNumber(tmp.Tile));
                    break;
                }
            }
        }

        public void VisualizeMovementRange()
        {
            var unitPos = new Coordinate2D(SelectedUnit.Cell.ColNum, SelectedUnit.Cell.RowNum, OffsetTypes.OddRowsRight);
            var myRange = HexGridGraph.GetMovementRange(unitPos, SelectedUnit.MovementPoints, movementType);
            List<Tile> rangeTiles = new List<Tile>();
            foreach (var item in myRange)
            {
                rangeTiles.Add(TilesList.Find(x=>x.ColNum == item.X && x.RowNum == item.Y));
            }
            foreach (var t in rangeTiles)
            {
                t.MarkAsReachable();
            }
        }

        public List<Tile> GetMyRow(Tile myTile)
        {
            List<Tile> row = new List<Tile>();
            return TilesList.FindAll(x => x.Position.y == myTile.Position.y);
        }
        public List<Tile> GetMyRow(Tile myTile, bool rightToLeft = true)
        {
            List<Tile> row = new List<Tile>();
            if(rightToLeft)
            {
                return TilesList.FindAll(x => x.Position.y == myTile.Position.y && x.Position.x <= myTile.Position.x);
            }
            else
            {
                return TilesList.FindAll(x => x.Position.y == myTile.Position.y && x.Position.x >= myTile.Position.x);
            }
        }

        public List<Tile> GetNeigbourTiles(Tile unitTile, Unit unit = null)
        {
            var pos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            var neighbors = HexGridGraph.GetNeighbors(pos, unit == null);
            List<Tile> tiles = new List<Tile>();
            foreach (var item in neighbors)
            {
                var p = TilesList.Find(x => x.ColNum.Equals(item.X) && x.RowNum.Equals(item.Y));
                tiles.Add(p);
                // p.MarkAsReachable();
            }
            return tiles;
        }

        public bool IsEnemyNearMe()
        {
            var unitTile = TM.GetCurrentUnit().Cell;
            var pos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            var neighbors = HexGridGraph.GetNeighbors(pos, false);
            foreach (var item in neighbors)
            {
                var p = TilesList.Find(x => x.ColNum.Equals(item.X) && x.RowNum.Equals(item.Y));
                if (p != null && p.IsTaken && NoUnitFound(p, unitTile))
                {
                    return true;
                }
            }
            return false;
}

        private bool NoUnitFound(Tile p, Tile unitTile)
        {
            bool found = false;

            if(p.CurrentUnit != null &&
                    p.CurrentUnit.MyPlayer != null &&
                    p.CurrentUnit.MyPlayer.PlayerNumber != unitTile.CurrentUnit.MyPlayer.PlayerNumber)
            {
                found = true;
            }
            else if(p.CurrentCone != null &&
                    p.CurrentCone.MyPlayer != null &&
                    p.CurrentCone.MyPlayer.PlayerNumber != unitTile.CurrentUnit.MyPlayer.PlayerNumber)
            {
                found = true;
            }

            return found;
        }

        public bool IsNeigbourTile(Tile unitTile, Tile targetTile)
        {
            var pos = new Coordinate2D(targetTile.ColNum, targetTile.RowNum, OffsetTypes.OddRowsRight);
            var unitPos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            var neighbors = HexGridGraph.GetNeighbors(unitPos, false);
            return neighbors.Contains(pos);
        }

        public bool IsNeigbourTile(Tile targetTile)
        {
            var unitTile = TM.GetCurrentUnit().Cell;
            var pos = new Coordinate2D(targetTile.ColNum, targetTile.RowNum, OffsetTypes.OddRowsRight);
            var unitPos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            var neighbors = HexGridGraph.GetNeighbors(unitPos, false);
            return neighbors.Contains(pos);
        }

        public List<Tile> GetTilesList()
        {
            return TilesList;
        }

        public Tile GetTileFromNumber(int num)
        {
            return TilesList.Find(x=>x.TileNo.Equals(num));
        }

        public void AddRoadBlock(Tile spawnTile, Player myPlayer, bool isMultiplayerRequest = false)
        {            
            //spawn Cone Scene
            GD.Print("****************** Adding Cone **************");
            var cone = (Cone)ConeScene.Instance();
            GD.Print("****************** Instance Cone **************");
            GetNode<Node2D>("TilesParent").GetChild(0).AddChild(cone);
            ActiveRoadBlocks.Add(cone);
            cone.Init(myPlayer, spawnTile, 10);
            ConesAddedTemp++;
            //Check on each turn change that if Cone has a life > 0
            if(ConesAddedTemp >= 3)
            {
                ConesAddedTemp = 0;
                if(!isMultiplayerRequest)
                {
                    CM.RequestApplyCard();
                }
                else
                {
                    cone.IsSynced=true;
                }
            }
        }

        public List<Cone> GetMyRoadBlocks(int playerNumber)
        {
            return ActiveRoadBlocks.FindAll(x=>x.MyPlayer.PlayerNumber == playerNumber && !x.IsSynced);
        }

        public void RemoveConeFromList(Cone cone)
        {
            ActiveRoadBlocks = ActiveRoadBlocks.FindAll(x=>x != cone);
        }

        public void CheckConeLife(int playerNum)
        {
            // GD.Print("CheckConeLife");
            if(ActiveRoadBlocks.Count > 0)
            {
                foreach (var item in ActiveRoadBlocks)
                {
                    item.OnTurnChanged(playerNum);
                }
            }
        }

        public void UnmarkPath()
        {
            var unitPos = new Coordinate2D(SelectedUnit.Cell.ColNum, SelectedUnit.Cell.RowNum, OffsetTypes.OddRowsRight);
            var myRange = HexGridGraph.GetMovementRange(unitPos, SelectedUnit.MovementPoints, movementType);
            List<Tile> rangeTiles = new List<Tile>();
            foreach (var item in myRange)
            {
                rangeTiles.Add(TilesList.Find(x => x.ColNum == item.X && x.RowNum == item.Y));
            }
            foreach (var t in rangeTiles)
            {
                t.UnMark();
            }
        }
    }
}
