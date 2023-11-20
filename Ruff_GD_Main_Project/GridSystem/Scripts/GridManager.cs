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
            var array = GetNode("TilesParent").GetChild(0).GetChildren().Cast<Tile>();
            TilesList = array.ToList();
            CreateGrid();
        }

        public async void SpawnPlayers(PlayerDataModel player1Data, PlayerDataModel player2Data)
        {
            var player1 = GetNode<Player>("Player1");
            var player2 = GetNode<Player>("Player2");
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

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
            //spawn dog units for both players
            player1.SpawnUnits(player1Data.Assets.Dogs);
            player2.SpawnUnits(player2Data.Assets.Dogs);
            //add both players to Turn Manager's Players List
            TM.AddPlayers(player1);
            TM.AddPlayers(player2);
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            // Update profile pictures and show cards
            UiController.UpdateProfilePic(1, player1.MyUnits[0]);
            UiController.UpdateProfilePic(2, player2.MyUnits[0]);
             List<Unit> player1RemainingUnits = new List<Unit>();
            if (player1.MyUnits.Count == 3)
            {
                player1RemainingUnits.Add(player1.MyUnits[1]);
                player1RemainingUnits.Add(player1.MyUnits[2]);
                UiController.UpdateUnitsProfilePic(1, player1RemainingUnits);
            }
            else if (player1.MyUnits.Count == 2)
            {
                player1RemainingUnits.Add(player1.MyUnits[1]);
                UiController.UpdateUnitsProfilePic(1, player1RemainingUnits);
            }

             List<Unit> player2RemainingUnits = new List<Unit>();
            if (player2.MyUnits.Count == 3)
            {
                player2RemainingUnits.Add(player2.MyUnits[1]);
                player2RemainingUnits.Add(player2.MyUnits[2]);
                UiController.UpdateUnitsProfilePic(2, player2RemainingUnits);
            }
            else if (player1.MyUnits.Count == 2)
            {
                player2RemainingUnits.Add(player2.MyUnits[1]);
                UiController.UpdateUnitsProfilePic(2, player2RemainingUnits);
            }

            UiController.Init();
            

            await ToSignal(GetTree().CreateTimer(3f), "timeout");
            SoundManager.Instance.PlaySoundByName("PlayerSpawnSound");
            
            UiController.PlayCardAnimation();
            if(player1.IsLocalPlayer)
            {
                UiController.UpdateNameLbls(1);
                CM.LoadCardDeck(player1);
                StartFirstTurn(player1);
            }
            else
            {
                UiController.UpdateNameLbls(2);
                CM.LoadCardDeck(player2);
                StartFirstTurn(player2);
            }

        }

        private void StartFirstTurn(Player player)
        {
            TM.StartTurns();
        }

        private void CreateGrid(){
            var maxRows = 5;
            var maxCols = 7;
            int count = 0;

            for(int row = 0; row < maxRows; row++) {
                if(row == 1 || row == 3){
                    maxCols = 6;
                }
                else {
                    maxCols = 7;
                }

                for(int col = 0; col < maxCols; col++) {
                    TilesList[count].RowNum = row;
                    TilesList[count].ColNum = col;
                    TilesList[count].TileNo = count+1;
                    count++;
                }
            }

            CreateGraph();
        }

        private void CreateGraph() {

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
            return TilesList[tileId-1];
        }

        public void OnUnitSelected(Tile unitTile, Unit unit = null){
            VisualizeMovementRange();
            CanSelectCell = true;
        }
        public void BlockTile(Tile tile, Unit unit){
            HexGridGraph.BlockCells(new Coordinate2D(tile.ColNum, tile.RowNum, OffsetTypes.OddRowsRight));
            tile.CurrentUnit = unit;
            tile.ActivatePlayerhighlight(unit.MyPlayer.IsLocalPlayer);
            tile.IsTaken = true;
        }

        public void BlockTile(Tile tile, Cone cone){
            HexGridGraph.BlockCells(new Coordinate2D(tile.ColNum, tile.RowNum, OffsetTypes.OddRowsRight));
            tile.CurrentCone = cone;
            tile.MarkAsDeHighlighted();
            tile.IsTaken = true;
        }

        public void UnblockTile(Tile tile){
            tile.IsTaken = false;
            tile.CurrentUnit = null;
            tile.CurrentCone = null;
            tile.UnMark();
            tile.DeactivatePlayerHighlights();
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
            if (HexGridGraph.IsWithinMovementRange(unitPos, targetPos, SelectedUnit.MovementPoints, movementType))
            {
                SelectedUnit.ConsumeMovementPoints(totalCost);
                var unt = GlobalData.GD.GetUnitDataModel(SelectedUnit);
                unt.Tile = targetCell.TileNo;
                UnitDataModel[] array = {unt};
                _ = StartupScript.Startup.Multiplayer.MoveUnit(array);
            }
        }

        //multiplayer move
        public void MoveUnits(PlayerDataModel current)
        {
            var player = TM.GetPlayerByID(current.ID);
            foreach (var u in player.MyUnits)
            {
                var tmp = current.Assets.Dogs.Find(x => x.DogId == u.UnitId.ToString());

                if(tmp.Tile != -1 && tmp.Tile != u.Cell.TileNo)
                {
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
                if(!t.IsTaken)
                {
                    t.MarkAsReachable();
                }
            }
        }

        public void VisualizeAttackRange(Unit myUnit)
        {
            var selectedUnitTile = myUnit?.Cell;
            if (selectedUnitTile == null) return;

            var unitPos = new Coordinate2D(selectedUnitTile.ColNum, selectedUnitTile.RowNum, OffsetTypes.OddRowsRight);
            var attackRange = GetUnitAttackRangeValue(myUnit);

            var rangeTiles = HexGridGraph.GetRange(unitPos, attackRange)
                .Select(item => TilesList.FirstOrDefault(x => x.ColNum == item.X && x.RowNum == item.Y))
                .Where(tile => tile != null);

            foreach (var tile in rangeTiles)
            {
                bool hasEnemyUnit = tile.CurrentUnit != null && tile.CurrentUnit.MyPlayer.PlayerNumber != myUnit.MyPlayer.PlayerNumber;
                bool hasEnemyCone = tile.CurrentCone != null && tile.CurrentCone.MyPlayer.PlayerNumber != myUnit.MyPlayer.PlayerNumber;

                if (hasEnemyUnit || hasEnemyCone)
                {
                    tile.MarkAsEnemy();
                }
                else
                {
                    if(!tile.IsTaken)
                    {
                        tile.MarkAsReachable();
                    }
                }
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
            }
            return tiles;
        }

        public bool IsEnemyInAttackRange(Unit enemy)
        {
            var myUnit = TM.GetCurrentUnit();
            var unitTile = myUnit.Cell; // Using ?. to handle null reference
            var enemyTile = enemy.Cell; // Using ?. to handle null reference

            if (unitTile == null || enemyTile == null) return false;

            var myPos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            var enemyPos = new Coordinate2D(enemyTile.ColNum, enemyTile.RowNum, OffsetTypes.OddRowsRight);
            var neighbors = HexGridGraph.GetRange(myPos, GetUnitAttackRangeValue(TM.GetCurrentUnit()));

            return neighbors.Contains(enemyPos);
        }

        public bool IsEnemyNearMe()
        {
            var unitTile = TM.GetCurrentUnit()?.Cell; // Using ?. to handle null reference
            if (unitTile == null) return false;

            var pos = new Coordinate2D(unitTile.ColNum, unitTile.RowNum, OffsetTypes.OddRowsRight);
            var neighbors = HexGridGraph.GetNeighbors(pos, false);

            if(neighbors.Count > 0)
            {
                if (unitTile != null)
                {
                    var hasEnemyNearby = neighbors
                        .Select(item => TilesList.FirstOrDefault(x => x.ColNum == item.X && x.RowNum == item.Y))
                        .Any(p => p != null && p.IsTaken && NoUnitFound(p, unitTile));

                    if (hasEnemyNearby)
                    {
                        return true;
                    }
                }

            }
            
            return false;
        }

        public int GetUnitAttackRangeValue(Unit unit)
        {
            int range;

            switch (unit.data.UnitBreed)
            {
                case BREED.SMALL:
                    range = 3;
                    break;
                case BREED.MEDIUM:
                    range = 2;
                    break;
                default:
                    range = 1;
                    break;
            }

            return range;
        }


        private bool NoUnitFound(Tile p, Tile unitTile)
        {
            return (p.CurrentUnit != null || p.CurrentCone != null) &&
                p.CurrentUnit?.MyPlayer?.PlayerNumber != unitTile.CurrentUnit?.MyPlayer?.PlayerNumber &&
                p.CurrentCone?.MyPlayer?.PlayerNumber != unitTile.CurrentUnit?.MyPlayer?.PlayerNumber;
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

        public Tile GetTileFromRowColNum(int row, int col)
        {
            return TilesList.Find(x=>x.RowNum == row && x.ColNum == col);
        }

        public Tile GetTileBehindMe(int myRow, int myCol, bool isFacingRight = false)
        {
            if(isFacingRight)
            {
                return TilesList.Find(x=>x.RowNum == myCol-1 && x.ColNum == myRow-1);
            }
            else
            {
                return TilesList.Find(x=>x.RowNum == myCol+1 && x.ColNum == myRow+1);
            }
        }

        public void AddRoadBlock(Tile spawnTile, Player myPlayer, bool isMultiplayerRequest = false)
        {
            //spawn Cone Scene
            var cone = (Cone)ConeScene.Instance();
            GetNode<Node2D>("TilesParent").GetChild(0).AddChild(cone);
            ActiveRoadBlocks.Add(cone);
            cone.Init(myPlayer, spawnTile, 10);
            cone.IsSynced = isMultiplayerRequest;
            if(!isMultiplayerRequest)
            {
                ConesAddedTemp++;
                //Check on each turn change that if Cone has a life > 0
                if(ConesAddedTemp >= 3)
                {
                    ConesAddedTemp = 0;
                    if(!isMultiplayerRequest)
                    {
                        CM.RequestApplyCard();
                    }
                }
            }
            
            if(cone.IsSynced) cone.SelfModulate = new Color(5, 5, 5, 1);
        }

        public void ClearUnsavedRoadblocks()
        {
            ConesAddedTemp = 0;
            foreach (var cone in ActiveRoadBlocks)
            {
                if(!cone.IsSynced)
                    cone.DeathRoutine();
            }
        }

        public void ClearConesAddedTemp()
        {
            ConesAddedTemp = 0;
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
            if(SelectedUnit == null) return;
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
