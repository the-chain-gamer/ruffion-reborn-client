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
                    else
                    {
                        GridManager.GM.MoveToTarget(this);
                    }
                }
                else if (!GridManager.GM.CanSelectCell && !IsTaken && GridManager.GM.CM.CanPlaceCones)
                {
                    if (mouseButton.Pressed)
                    {
                        // GD.Print("Place Cones on tile");
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
            GetNode<Sprite>("PathSprite").Show();
        }
        /// <summary>
        /// Method marks the cell as a part of a path.
        /// </summary>
        public void MarkAsPath(bool isPath){
            if(isPath){
                // GD.Print("it's a path tile now!!");
                GetNode<Sprite>("PathSprite").Show();
                // ishighlighted = true;
            }
            else {
                GetNode<Sprite>("PathSprite").Hide();
                isReachable = false;
            }
        }
        /// <summary>
        /// Method marks the cell as highlighted. It gets called when the mouse is over the cell.
        /// </summary>
        public void MarkAsHighlighted(){
            GetNode<Sprite>("HighlightSprite").Show();
            isHighlighted = true;
        }

        public void MarkAsSelected(){
            GetNode<Sprite>("PathSprite").Show();
            GetNode<Sprite>("HighlightSprite").Hide();
            isHighlighted = false;
        }

        public void MarkAsDeselected(){
            GetNode<Sprite>("PathSprite").Hide();
            GetNode<Sprite>("HighlightSprite").Show();
            isHighlighted = true;
        }

        /// <summary>
        /// Method returns the cell to its base appearance.
        /// </summary>
        public void UnMark(){
            if(!IsTaken) {
                if(isHighlighted){
                    // GD.Print("Unmark");
                    GetNode<Sprite>("HighlightSprite").Hide();
                    isHighlighted = false;
                }
                else if(isReachable){
                    // GD.Print("Unmark");
                    GetNode<Sprite>("PathSprite").Hide();
                    isReachable = false;
                }
            }
        }


    }
}