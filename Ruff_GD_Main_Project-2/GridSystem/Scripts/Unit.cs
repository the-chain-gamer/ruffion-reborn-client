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
    public class Unit : Node2D
    {
        public int UnitId { get; private set; }
        public string UnitName { get; private set; }
        [Export]public bool IsFacingRight = true;
        public bool IsMyTurn { get; private set; }
        public bool HasTakenTurn { get; private set; }
        public bool CanDoRangedAttack = false;
        public float HitPoints { get; private set; }
        public float TotalHitPoints { get; private set; }
        public int MovementPoints { get; protected set; }
        public int ConsumableMovementPoints { get; protected set; }
        public int TotalMovementPoints { get; private set; }
        public float TotalActionPoints { get; private set; }
        public int AttackRange { get; private set; }
        public int AttackFactor { get; private set; }
        public int ConsumeableAttackFactor { get; private set; }
        public int DefenceFactor { get; private set; }
        public int ConsumableDefenceFactor { get; private set; }
        public int ConsumeableShieldLife { get; private set; }
        public int ConsumeableShieldValue { get; private set; }
        public bool IsConsumeableShieldApplied { get; private set; }
        public bool IsDead { get; private set; }
        public int ConsumableCloak { get; private set; }
        public bool IsHatTrick { get; private set; }
        public int TrappedInplace { get; private set; } = 0;
        public int ActionPoints { get; protected set; } = 1;
        public bool IsSmokescreenApplied = false;
        public bool IsBlackSheep { get; private set; }
        public int BlackSheepLife { get; private set; }
        public bool IsMadDog { get; private set; } //will attack it's allies
        public bool IsAttackedByMadDog { get; private set; } //if it is attacked by an ally or not

        /// <summary>
        /// Indicates if movement animation is playing.
        /// </summary>
        public bool IsMoving { get; set; }

        // A Reference to the Player
        public Player MyPlayer { get; private set; }

        public UnitData data { get; private set; }
        private Color HighlightColor = new Color(1f, 1f, 1f, 1f);
        private Color SelectedColor = new Color("ccc222");
        /// <summary>
        /// Cell that the unit is currently occupying.
        /// </summary>
        private Tile cell;
        public Tile Cell
        {
            get
            {
                return cell;
            }
            set
            {
                cell = value;
                cell.CurrentUnit = this;
                GridManager.GM.BlockTile(cell, this);
            }
        }

        public void Init(Player player, Tile spawnTile, UnitData dt)
        {
            string[] animations = {
                "Beagle Idle",
                "Corgi Idle",
                "Golden Retriever Idle",
                "Labrador Idle",
                "Poodle Idle",
                "Shiba Idle",
                "Hound Idle",
                "Malamute Idle",
                "Mastiff Idle"
            };
            data = dt;
            UnitId = dt.UnitID;
            UnitName = dt.UnitName;
            MyPlayer = player;
            HasTakenTurn = false;
            DefenceFactor = 0;
            ConsumableDefenceFactor = 0;
            ConsumeableAttackFactor = 0;
            ConsumeableShieldLife = 0;
            ConsumeableShieldValue = 0;
            IsConsumeableShieldApplied = false;
            ConsumableCloak = 0;
            ConsumableMovementPoints = 0;
            HitPoints = dt.UnitStats.HP;
            TotalHitPoints = HitPoints;
            MovementPoints = 2;
            IsBlackSheep = false;
            BlackSheepLife = 0;
            if(player.PlayerNumber == 2)
            {
                IsFacingRight = false;
                this.Scale = new Vector2(-1, 1);
                GetChild<FloatingText>(GetChildCount()-1).Scale = new Vector2(-1, 1);
            }
            Cell = spawnTile;
            Position = spawnTile.Position;
            IsSmokescreenApplied = false;
            GetNode<AnimatedSprite>("Body").SetAnimation(animations[UnitId - 1]);
            GetNode<AnimatedSprite>("Body").GetNode<Area2D>(UnitName).Show();
        }

        public void TrapInplace(int trapVal)
        {
            TrappedInplace = trapVal;
        }

        public void AddHP(float hp)
        {
            if(HitPoints+hp > TotalHitPoints)
            {
                TotalHitPoints = HitPoints + hp;
            }
            HitPoints += hp;
            GridManager.GM.UiController.ShowDmgAtUnit(this, hp, TextColorType.Heal);
        }

        public void AddDefenceFactor(int factor)
        {
            DefenceFactor += factor;
            GridManager.GM.UiController.ShowDmgAtUnit(this, DefenceFactor, TextColorType.PowerUp);
        }

        public void AddConsumableDefenceFactor(int factor)
        {
            ConsumableDefenceFactor += factor;
            GridManager.GM.UiController.ShowDmgAtUnit(this, ConsumableDefenceFactor, TextColorType.PowerUp);
        }

        public void AddConsumableAttackFactor(int factor)
        {
            ConsumeableAttackFactor += factor;
            GridManager.GM.UiController.ShowDmgAtUnit(this, ConsumeableAttackFactor, TextColorType.PowerUp);
        }

        public void AddConsumableShield(int val, int life)
        {
            IsConsumeableShieldApplied = true;
            ConsumeableShieldLife = life;
            ConsumeableShieldValue = val;
            GridManager.GM.UiController.ShowDmgAtUnit(this, ConsumeableShieldLife, TextColorType.PowerUp);
        }

        public void AddConsumableCloak(int val, bool isHatTrick = false)
        {
            IsHatTrick = isHatTrick;
            ConsumableCloak += val;
        }

        public void ConsumeCloak()
        {
            ConsumableCloak = 0;
            IsHatTrick = false;
        }

        public void AddConsumableMovementPoints(int points)
        {
            ConsumableMovementPoints += points;
        }

        public void MakeDeceptiveBlackSheep()
        {
            IsBlackSheep = true;
            BlackSheepLife = 2;
        }

        public void MakeCattenMadDog()
        {
            IsMadDog = true;
        }

        public async void Move(Tile target, int cost = 0)
        {
            if(GridManager.GM.CM.PickNDrop)
            {
                MarkAsDefending();
            }
            MovementPoints += ConsumableMovementPoints;
            MovementPoints -= cost;
            ConsumableMovementPoints = 0;
            GridManager.GM.CanSelectCell = false;
            GridManager.GM.UnblockTile(Cell);
            GridManager.GM.ResetGridVisuals();
            IsMoving = true;
            Cell = target;
            GD.Print("Dog Position BEFORE = " + Position);
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            SoundManager.Instance.PlaySoundByName("TeleportSimpleSound");
            Position = target.Position;
            // target.UnMark();
            if(MyPlayer.IsLocalPlayer)
            {
                if(GridManager.GM.CM.PickNDrop)
                {
                    UnMark(false);
                }
                else
                {
                    target.MarkAsSelected();
                }
            }
            IsMoving = false;
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            // UnMark(true);
            GD.Print("Dog Position AFTER = " + Position);
        }

        public void OnMouseDown(Viewport viewport, InputEvent _event, int shape_idx)
        {
            if (_event is InputEventMouseButton mouseButton)
            {
                GD.Print("Dog Tile No. = " + cell.TileNo);
                if (MyPlayer.IsLocalPlayer && mouseButton.Pressed && MyPlayer.IsMyTurn)
                {
                    MarkAsSelected();
                }
                else if(!MyPlayer.IsMyTurn && GridManager.GM.CanSelectTarget && ConsumableCloak == 0)
                {
                    if(GridManager.GM.CM.CanPlaceCones) return;
                    GridManager.GM.CanSelectTarget = false;

                    if(GridManager.GM.CM.CurrentCard.CardName.Equals("Basic Attack"))
                    {
                        if(GridManager.GM.IsNeigbourTile(cell))
                        {
                            MarkAsDefending();
                        }
                        else
                        {
                            GridManager.GM.UiController.ShowMessage("Not in Range!");
                        }
                    }
                    else if(GridManager.GM.CM.PickNDrop)
                    {
                        GD.Print("Bubble's Pick and Drop");
                        MarkAsSelected(false, true);
                    }
                    else
                    {
                        MarkAsDefending();
                    }
                }
            }
        }

        public void OnMouseEnter()
        {
            GetNode<ShaderTest>("Body").OnMouseEnter(HighlightColor);
            GridManager.GM.UiController.ShowUnitVitals(this);
        }
        public void OnMouseExit()
        {
            if(!IsMyTurn)
            {
                GetNode<ShaderTest>("Body").OnMouseExit();
            }
            else
            {
                GetNode<ShaderTest>("Body").OnMouseEnter(SelectedColor);
            }
            GridManager.GM.UiController.HideUnitVitals();
        }
        public void MarkAsDefending()
        {
            GridManager.GM.SelectedUnit = this;
            GridManager.GM.CM.RequestApplyCard();
            // GridManager.GM.CM.ApplyCard();
        }

        public async void DealDamage(float dmg, bool isAllyAttack = false)
        {
            IsAttackedByMadDog = isAllyAttack;
            if(HitPoints > 0f)
            {
                if(ConsumeableShieldValue > 0)
                {
                    var temp = dmg;

                    dmg -= ConsumeableShieldValue;
                    ConsumeableShieldValue -= (int)temp;

                    if (dmg < 0) dmg = 0;

                    if (ConsumeableShieldValue <= 0)
                    {
                        ConsumeableShieldValue = 0;
                        ConsumeableShieldLife = 0;
                        IsConsumeableShieldApplied = false;
                    }
                }

                HitPoints -= dmg;
            }
            else
            {
                HitPoints = 0.0f;
                //Dead
                IsDead = true;
            }
            GetNode<AnimatedSprite>("Body").SelfModulate = new Color("ff0000");
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "Damage  =   =  " + dmg.ToString());
            GridManager.GM.UiController.ShowDmgAtUnit(this, dmg, TextColorType.Damage);
            GridManager.GM.camShake.Shake(0.35f);
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            GetNode<AnimatedSprite>("Body").SelfModulate = new Color("ffffff");

            if (HitPoints <= 0)
            {
                IsDead = true;
                OnDestroy();
                await ToSignal(GetTree().CreateTimer(1.75f), "timeout");
                this.GetNode<AnimatedSprite>("Body").Visible = false;
            }
        }

        public float PreCalcDamage(float dmg, bool isAllyAttack = false)
        {
            var hp = HitPoints;
            IsAttackedByMadDog = isAllyAttack;
            if(hp > 0f)
            {
                if(ConsumeableShieldValue > 0)
                {
                    var temp = dmg;

                    dmg -= ConsumeableShieldValue;
                    ConsumeableShieldValue -= (int)temp;

                    if (dmg < 0) dmg = 0;

                    if (ConsumeableShieldValue <= 0)
                    {
                        ConsumeableShieldValue = 0;
                        ConsumeableShieldLife = 0;
                        IsConsumeableShieldApplied = false;
                    }
                }

                hp -= dmg;
            }
            else
            {
                hp = 0.0f;
            }

            return hp;
        }

        /// <summary>
        /// Method marks unit as current players unit.
        /// </summary>
        public void MarkAsFriendly()
        {
        }
        /// <summary>
        /// Method mark units to indicate user that the unit is in range and can be attacked.
        /// </summary
        public void MarkAsReachableEnemy()
        {
        }
        /// <summary>
        /// Method marks unit as currently selected, to distinguish it from other units.
        /// </summary>
        public void MarkAsSelected(bool isForAttack = false, bool isForMove = false)
        {
            if(!GridManager.GM.CM.PickNDrop && !MyPlayer.IsLocalPlayer) return;
            GetNode<ShaderTest>("Body").OnMouseEnter(SelectedColor);
            if(isForAttack)
            {
                GridManager.GM.OnUnitSelected(Cell, this);
            }
            else if(isForMove)
            {
                if(TrappedInplace > 0) return;
                //MyPlayer.SwitchUnit(this);
                GridManager.GM.SelectedUnit = this;
                GD.Print("Moving it");
                GridManager.GM.OnUnitSelected(Cell);
            }
            else
            {
                // if(TrappedInplace > 0) return;
                MyPlayer.SwitchUnit(this);
                // GridManager.GM.SelectedUnit = this;
                // GridManager.GM.OnUnitSelected(Cell);
            }
        }
        public void Attack()
        {
             GD.Print("MyPlayer.TotalMana = " + MyPlayer.TotalMana);
            if(MyPlayer.TotalMana <= 0)
            {
                GridManager.GM.UiController.ShowMessage("Not Enough Mana");
            }
            else
            {
                if(IsSmokescreenApplied)
                {
                    GridManager.GM.UiController.ShowMessage("Can't Attack");
                    return;
                }
                Card card = new Card();
                card.SetCardData(new CardData(), true);
                GridManager.GM.CM.DropCard(card);
            }
        }
        /// <summary>
        /// Method marks unit to indicate user that he can't do anything more with it this turn.
        /// </summary>
        public void MarkAsFinished(){
            GetNode<ShaderTest>("Body").OnMouseExit();
            HasTakenTurn = true;
            IsMyTurn = false;
            IsSmokescreenApplied = false;
            IsMadDog = false;
            cell.MarkAsDeselected();

            if (BlackSheepLife > 0)
            {
                BlackSheepLife--;
                if(BlackSheepLife <= 0)
                {
                    BlackSheepLife=0;
                    IsBlackSheep = false;
                }
            }

            // GetNode<AnimatedSprite>("Body").SelfModulate = new Color(1, 1, 1, 1f);
        }

        public void Reset()
        {
            HasTakenTurn = false;
            IsMyTurn = false;
            // DefenceFactor = 0;
            ConsumeCloak();
            IsHatTrick = false;
            MovementPoints = 2;
            ConsumableMovementPoints = 0;
            UnMark(false);
            ConsumeableShieldLife--;
            if (ConsumeableShieldLife <= 0)
            {
                ConsumableDefenceFactor = 0;
                ConsumeableShieldLife = 0;
                ConsumeableShieldValue = 0;
            }
        }

        public void MarkMyTurn(bool isSwitchingUnit = false){
            if(!isSwitchingUnit)
            {
                if(IsMadDog)
                {
                    GridManager.GM.UiController.ShowMessage("Ally Attack due to Catten");
                    MyPlayer.ConsumeMana(1);
                    MyPlayer.DoAllyAttack();
                    IsMadDog = false;
                    MyPlayer.SkipUnit();
                }
                if(TrappedInplace > 0)
                {
                    TrappedInplace--;
                }
            }

            HasTakenTurn = false;
            GetNode<ShaderTest>("Body").OnMouseEnter(SelectedColor);
            cell.MarkAsSelected();
            IsMyTurn = true;
            GridManager.GM.CM.GreyoutNonUsableCards(data);
        }
        /// <summary>
        /// Method returns the unit to its base appearance
        /// </summary>
        public void UnMark(bool isMoving){
            GridManager.GM.SelectedUnit = null;
            if(!isMoving)
                cell.MarkAsDeselected();
            GetNode<ShaderTest>("Body").OnMouseExit();
            // GetNode<AnimatedSprite>("Body").SelfModulate = new Color(1, 1, 1, 1f);
            // GridManager.GM.OnUnitSelected(Cell);
        }

        public void OnDestroy()
        {
            if (Cell != null)
            {
                Cell.IsTaken = false;
            }
        }

        private void FlipCardEffectOnUnit(Unit currentUnit)
        {

            if (currentUnit.UnitName == "Corgi" || currentUnit.UnitName == "Shiba" || currentUnit.UnitName == "Beagle" || currentUnit.UnitName == "Poodle")
            {

                //Lalito's Laser
                // currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).Position = new Vector2(-580f, -24f);
                currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(1).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(2).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(3).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(4).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(5).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(6).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(7).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(8).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(9).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(10).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(11).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(1).GetChild<Node2D>(12).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
            }
            else if (currentUnit.UnitName == "Golden" || currentUnit.UnitName == "Labrador")
            {
                //Lalito's Laser
                // currentUnit.GetChild(2).GetChild<Node2D>(0).GetChild<Node2D>(0).Position = new Vector2(-650f, 0f);
                currentUnit.GetChild(2).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(1).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(2).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(3).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(4).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(5).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(6).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(7).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(8).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(9).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(10).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(11).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(2).GetChild<Node2D>(12).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
            }
            else if (currentUnit.UnitName == "Malamute" || currentUnit.UnitName == "Mastiff" || currentUnit.UnitName == "Hound")
            {
                //Lalito's Laser
                // currentUnit.GetChild(3).GetChild<Node2D>(0).GetChild<Node2D>(0).Position = new Vector2(-580f, -24f);
                currentUnit.GetChild(3).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(1).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(2).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(3).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(4).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(5).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(6).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(7).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(8).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(9).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(10).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(11).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
                currentUnit.GetChild(3).GetChild<Node2D>(12).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale = new Vector2(-currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.x, currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<Node2D>(0).GetChild<AnimatedSprite>(0).Scale.y);
            }
           
        }
    }

 }


