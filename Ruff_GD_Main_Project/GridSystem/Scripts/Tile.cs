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
        private Sprite ActiveSprite;
        private Sprite FriendlySprite;
        private Sprite EnemySprite;

        public override void _Ready()
        {
            PathSprite = GetNode<Sprite>("PathSprite");
            HighlightSprite = GetNode<Sprite>("HighlightSprite");
            ActiveSprite = GetNode<Sprite>("ActiveSprite");
            FriendlySprite = GetNode<Sprite>("FriendlySprite");
            EnemySprite = GetNode<Sprite>("EnemySprite");
        }


        protected void OnMouseEnter()
        {
            if (!isHighlighted && !IsTaken)
            {
                MarkAsHighlighted();
            }
        }
        public void OnMouseDown(Viewport viewport, InputEvent _event, int shape_idx)
        {
            if (_event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.Pressed && GridManager.GM.CanSelectCell && !IsTaken)
                {
                    if(GridManager.GM.CM.PickNDrop)
                    {
                        GridManager.GM.CanSelectCell=false;
                        GridManager.GM.CM.ApplyPickNDrop(GridManager.GM.SelectedUnit, TileNo);
                    }
                    else
                    {
                        GridManager.GM.MoveToTarget(this);
                    }
                }
                else if (!GridManager.GM.CanSelectCell && !IsTaken && GridManager.GM.CM.CanPlaceCones)
                {
                    if (mouseButton.Pressed)
                    {
                        GridManager.GM.AddRoadBlock(this, GridManager.GM.TM.GetCurrentPlayer());
                    }
                }
            }
        }
        protected void OnMouseExit()
        {
            MarkAsDeHighlighted();
        }
        public bool isDigAllowed = false;

        /// <summary>
        ///  Method marks the cell to give user an indication that selected unit can reach it.
        /// </summary>
        public void MarkAsReachable(){
            isReachable=true;
            PathSprite.Show();
        }
        /// <summary>
        /// Method marks the cell as a part of a path.
        /// </summary>
        public void MarkAsPath(bool isPath){
            if(isPath){
                this.PathSprite.Show();
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
            HighlightSprite.Show();
            isHighlighted = true;
        }

        public void MarkAsDeHighlighted(){
            HighlightSprite.Hide();
            isHighlighted = false;
        }

        public void MarkAsFriendly(){
            FriendlySprite.Show();
            ActiveSprite.Hide();
        }

        public void MarkAsEnemy(){
            EnemySprite.Show();
            ActiveSprite.Hide();
        }

        public void MarkAsSelected(){
            ActiveSprite.Show();
            HighlightSprite.Hide();
            FriendlySprite.Hide();
            PathSprite.Hide();
            isHighlighted = false;
        }

        public void MarkAsDeselected(){
            PathSprite.Hide();
            ActiveSprite.Hide();
            FriendlySprite.Visible = CurrentUnit?.IsMyTurn ==  true;
            EnemySprite.Hide();
            isHighlighted = true;
        }

        /// <summary>
        /// Method returns the cell to its base appearance.
        /// </summary>
        public void UnMark(){
            if(!IsTaken) {
                if(isHighlighted){
                    this.HighlightSprite.Hide();
                    isHighlighted = false;
                    EnemySprite.Hide();
                }
                else if(isReachable){
                    this.PathSprite.Hide();
                    isReachable = false;
                    EnemySprite.Hide();
                }
            }
            else
            {
                EnemySprite.Hide();
            }
        }

        public void UnitDestroyed()
        {
            IsTaken = false;
            ActiveSprite.Hide();
            EnemySprite.Hide();
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
            ActiveSprite.Visible = isPlayer;
        }

        public void ActivateEnemyHighlights()
        {
            EnemySprite.Visible = true;
        }

        public void DeactivatePlayerHighlights()
        {
            ActiveSprite.Hide();
            EnemySprite.Hide();
        }
    }
}