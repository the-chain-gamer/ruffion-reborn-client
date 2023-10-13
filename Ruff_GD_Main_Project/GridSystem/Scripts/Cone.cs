using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.UiSystem;

namespace RuffGdMainProject.GridSystem
{
    public class Cone : Node2D
    {
        public Tile Cell { get; private set; }
        public int Life = 3;
        public float HitPoints { get; private set; }
        public Player MyPlayer { get; private set; }
        private bool isDead = false;

        public bool IsSynced = false;
        public int TileNo { get; private set; }

        public void Init(Player player, Tile spawnTile, float hp)
        {
            TileNo = spawnTile.TileNo;
            Life = 3;
            MyPlayer = player;
            Cell = spawnTile;
            Cell.IsTaken = true;
            Cell.CurrentCone = this;
            HitPoints = hp;
            GlobalPosition = spawnTile.GlobalPosition;
            GetChild<AnimatedSprite>(0).Play("default", true);
            SoundManager.Instance.PlaySoundByName("BubbleSound");
            GridManager.GM.BlockTile(Cell, this);
            // Cell.ActivatePlayerhighlight(MyPlayer.IsLocalPlayer);
            // IsSynced = true;
        }

        public void OnMouseEnter()
        {
            GridManager.GM.UiController.ShowUnitVitals(this);
        }
        public void OnMouseDown(Viewport viewport, InputEvent _event, int shape_idx)
        {
            if (_event is InputEventMouseButton mouseButton)
            {
                // GD.Print("Targetting CONE----------------");
                if(!isDead && !MyPlayer.IsMyTurn
                    && GridManager.GM.CanSelectTarget && !GridManager.GM.CM.CanPlaceCones)
                {
                    GridManager.GM.CanSelectTarget = false;
                    // DealDamage(10);
                    GridManager.GM.CM.AttackOnCone(this);
                }
            }
        }
        public void OnMouseExit()
        {
            GridManager.GM.UiController.HideUnitVitals();
        }

        public void DealDamage(float dmg)
        {
            // GD.Print(">>>>>>> Damaging CONE");
            HitPoints -= dmg;
            GridManager.GM.UiController.ShowDmgAtUnit(this, dmg, TextColorType.Damage);
            GridManager.GM.camShake.Shake(0.35f);
            if(HitPoints <= 0f)
            {
                DeathRoutine();
            }
        }

        public void OnTurnChanged(int pNum)
        {
            if(Life <=0)
            {
                DeathRoutine();
            }
            Life--;
        }

        public void DeathRoutine()
        {
            isDead = true;
            GridManager.GM.RemoveConeFromList(this);
            if (Cell != null)
            {
                GridManager.GM.UnblockTile(Cell);
                Cell.IsTaken = false;
                Cell.CurrentCone = null;
                Cell.MarkAsDeselected();
                Cell.DeactivatePlayerHighlights();
                Cell.UnMark();
            }
            this.QueueFree();
        }
    }
}