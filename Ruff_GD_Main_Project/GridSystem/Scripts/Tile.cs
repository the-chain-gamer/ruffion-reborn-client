using Godot;
using System;

namespace RuffGdMainProject.GridSystem
{
    public class Tile : Node2D
    {
        private bool isHighlighted = false;
        private bool isReachable = false;

        public int RowNum { get; set; }
        public int ColNum { get; set; }
        public int TileNo { get; set; }

        public bool IsTaken { get; set; }
        public Unit CurrentUnit { get; set; }
        public Cone CurrentCone { get; set; }

        /// <summary>
        /// Cost of moving through the cell.
        /// </summary>
        public int MovementCost = 1;
        
        private Sprite PathSprite;
        private Sprite HighlightSprite;
        private Sprite PlayerTileSprite;
        private Sprite EnemyTileSprite;

        public override void _Ready()
        {
            PathSprite = GetChild<Sprite>(4);
            HighlightSprite = GetChild<Sprite>(1);
            PlayerTileSprite = GetChild<Sprite>(2);
            EnemyTileSprite = GetChild<Sprite>(3);
        }


        protected void OnMouseEnter()
        {
            if (!isHighlighted && !IsTaken)
            {
                // GD.Print("Enter");
                MarkAsHighlighted();
            }
        }
        public void OnMouseDown(Viewport viewport, InputEvent _event, int shape_idx)
        {
            if (_event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.Pressed && GridManager.GM.CanSelectCell && !IsTaken)
                {
                    // GD.Print("Down");
                    if(GridManager.GM.CM.PickNDrop)
                    {
                        GridManager.GM.CanSelectCell=false;
                        GridManager.GM.CM.ApplyPickNDrop(GridManager.GM.SelectedUnit, TileNo);
                    }
                    // else if()
                    else
                    {
                        GridManager.GM.MoveToTarget(this);
                    }
                }
                else if (!GridManager.GM.CanSelectCell && !IsTaken && GridManager.GM.CM.CanPlaceCones)
                {
                    if (mouseButton.Pressed)
                    {
                        GD.Print("Place Cones on tile");
                        GridManager.GM.AddRoadBlock(this, GridManager.GM.TM.GetCurrentPlayer());
                    }
                }
            }
            else
            {
                // GD.Print(IsTaken);
            }
        }
        protected void OnMouseExit()
        {
            UnMark();
        }
        public bool isDigAllowed = false;

        /// <summary>
        ///  Method marks the cell to give user an indication that selected unit can reach it.
        /// </summary>
        public void MarkAsReachable(){
            isReachable=true;
            this.PathSprite.Show();
        }
        /// <summary>
        /// Method marks the cell as a part of a path.
        /// </summary>
        public void MarkAsPath(bool isPath){
            if(isPath){
                // GD.Print("it's a path tile now!!");
                this.PathSprite.Show();
                // ishighlighted = true;
            }
            else {
                this.PathSprite.Hide();
                isReachable = false;
            }
        }
        /// <summary>
        /// Method marks the cell as highlighted. It gets called when the mouse is over the cell.
        /// </summary>
        public void MarkAsHighlighted(){
            this.HighlightSprite.Show();
            isHighlighted = true;
        }

        public void MarkAsSelected(){
            this.PathSprite.Show();
            this.HighlightSprite.Hide();
            isHighlighted = false;
        }

        public void MarkAsDeselected(){
            this.PathSprite.Hide();
            this.HighlightSprite.Show();
            isHighlighted = true;
        }

        /// <summary>
        /// Method returns the cell to its base appearance.
        /// </summary>
        public void UnMark(){
            if(!IsTaken) {
                if(isHighlighted){
                    // GD.Print("Unmark");
                    this.HighlightSprite.Hide();
                    isHighlighted = false;
                }
                else if(isReachable){
                    // GD.Print("Unmark");
                    this.PathSprite.Hide();
                    isReachable = false;
                }
            }
        }

        public void UnitDestroyed()
        {
            IsTaken = false;
            PlayerTileSprite.Hide();
            EnemyTileSprite.Hide();
            CurrentCone = null;
            CurrentUnit = null;
            isHighlighted = false;
            HighlightSprite.Hide();
            isReachable = false;
            PathSprite.Hide();
            GridManager.GM.UnblockTile(this);
        }

        public void ActivatePlayerhighlight(bool isPlayer)
        {
            PlayerTileSprite.Visible = isPlayer;
            EnemyTileSprite.Visible = !isPlayer;
        }

        public void DeactivatePlayerHighlights()
        {
            PlayerTileSprite.Hide();
            EnemyTileSprite.Hide();
        }
    }
}