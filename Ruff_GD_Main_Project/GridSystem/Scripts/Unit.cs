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
        public bool MaskOfEEEEEE { get; private set; }
        public bool IsHatTrick { get; private set; }
        public int TrappedInplace { get; private set; } = 0;
        public int GlovesOfDoom { get; private set; } = 0;
        public int ActionPoints { get; protected set; } = 1;
        public bool IsSmokescreenApplied = false;
        public bool IsBlackSheep { get; private set; }
        public int BlackSheepLife { get; private set; }
        public int IsMadDog { get; private set; } //will attack it's allies
        public bool IsAttackedByMadDog { get; private set; } //if it is attacked by an ally or not
        public bool JockSmash { get; private set; } //if it is attacked by an ally or not
        public int JockSmashValue { get; private set; } //if it is attacked by an ally or not

        /// <summary>
        /// Indicates if movement animation is playing.
        /// </summary>
        public bool IsMoving { get; set; }

        // A Reference to the Player
        public Player MyPlayer { get; private set; }

        public UnitData data { get; private set; }
        private Color HighlightColor = new Color("f5b50a");
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
            MaskOfEEEEEE = false;
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
                GetNode<FloatingText>("FloatingText").Scale = new Vector2(-1, 1);
            }
            Cell = spawnTile;
            Position = spawnTile.Position;
            IsSmokescreenApplied = false;
            this.GetNode<AnimatedSprite>("Body").SetAnimation(animations[UnitId - 1]);
            this.GetNode<AnimatedSprite>("Body").GetNode<Area2D>(UnitName).Show();
            Cell.ActivatePlayerhighlight(MyPlayer.IsLocalPlayer);
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
            GD.Print("Consumeable SHield from UNIT = " + ConsumeableShieldValue);
            GridManager.GM.UiController.ShowDmgAtUnit(this, ConsumeableShieldLife, TextColorType.PowerUp);
        }

        public void AddConsumableCloak(int val, bool isHatTrick = false)
        {
            IsHatTrick = isHatTrick;
            ConsumableCloak += val;
            if(isHatTrick)
            {
                GetNode<Node2D>("CyrilsLoop").GetChild<Node2D>(0).Show();
            }
        }

        public void ConsumeCloak()
        {
            ConsumableCloak = 0;
            if(IsHatTrick)
            {
                IsHatTrick = false;
                GetNode<Node2D>("CyrilsLoop").GetChild<Node2D>(0).Hide();
            }
            else
            {
                GetNode<Node2D>("MarlonSmokeBombLoop").GetChild<Node2D>(0).Hide();
            }
        }

        public void ConsumeShield()
        {
            ConsumeableShieldLife--;
            if (ConsumeableShieldLife <= 0)
            {
                ConsumableDefenceFactor = 0;
                ConsumeableShieldLife = 0;
                ConsumeableShieldValue = 0;
                IsConsumeableShieldApplied = false;
                GetNode<Node2D>("SmallDogParticles").GetNode<Node2D>("SaundersLoop").GetChild<Node2D>(0).Hide();
                GetNode<Node2D>("MediumDogParticles").GetNode<Node2D>("SaundersLoop").GetChild<Node2D>(0).Hide();
                GetNode<Node2D>("LargeDogParticles").GetNode<Node2D>("SaundersLoop").GetChild<Node2D>(0).Hide();
            }
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
            IsMadDog = 1;
            Node2D node;
            if (UnitName == "Corgi" || UnitName == "Shiba" || UnitName == "Beagle" || UnitName == "Poodle")
            {
                //Effect
                node = this.GetNode<Node2D>("SmallDogParticles").GetNode<Node2D>("cattenHearts");
            }
            else if (UnitName == "Golden" || UnitName == "Labrador" )
            {
                //Effect
                node = this.GetNode<Node2D>("MediumDogParticles").GetNode<Node2D>("cattenHearts");
            }
            else
            {
                //Effect
                node = this.GetNode<Node2D>("LargeDogParticles").GetNode<Node2D>("cattenHearts");
            }

            EffectPlayer EP = (EffectPlayer) node.GetChild<EffectPlayer>(0);
            EP.PlayAnimationOnce(true);
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "Ismad Dog = " + IsMadDog);
        }

        public void DisableCattenMadDog()
        {
            IsAttackedByMadDog = false;
            IsMadDog--;
            if(IsMadDog<=0)
            {
                IsMadDog = 0;
                IsAttackedByMadDog = false;
                Node2D node;
                if (UnitName == "Corgi" || UnitName == "Shiba" || UnitName == "Beagle" || UnitName == "Poodle")
                {
                    //Effect
                    node = this.GetNode<Node2D>("SmallDogParticles").GetNode<Node2D>("cattenHearts");
                }
                else if (UnitName == "Golden" || UnitName == "Labrador" )
                {
                    //Effect
                    node = this.GetNode<Node2D>("MediumDogParticles").GetNode<Node2D>("cattenHearts");
                }
                else
                {
                    //Effect
                    node = this.GetNode<Node2D>("LargeDogParticles").GetNode<Node2D>("cattenHearts");
                }

                EffectPlayer EP = (EffectPlayer) node.GetChild<EffectPlayer>(0);
                EP.DsiableCattenEffect();
            }
        }

        public void ApplyGlovesOfDoom()
        {
            GlovesOfDoom = 2;
            //enable gloves of doom effect
            GetNode<Node2D>("GlovesOfDoom").Show();
        }

        public void ApplyJockSmash()
        {
            JockSmash = true;
            JockSmashValue = 0;
            //play effect
            GetNode<Node2D>("JockSmash").Show();
        }
        public void ApplyMaskofEEEEEE()
        {
            MaskOfEEEEEE = true;
            GetNode<Node2D>("MaskOfEEEEE").Show();
            //play effect
        }

        public async void Move(Tile target, int cost = 1)
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
            // if(cost == 0)
            // {
            //     UnMark(false);
            // }
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
                else if(!MyPlayer.IsMyTurn && GridManager.GM.CanSelectTarget && ConsumableCloak == 0 && !MaskOfEEEEEE)
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
            this.GetNode<ShaderTest>("Body").OnMouseEnter(HighlightColor);
            GridManager.GM.UiController.ShowUnitVitals(this);
        }
        public void OnMouseExit()
        {
            if(!IsMyTurn)
            {
                this.GetNode<ShaderTest>("Body").OnMouseExit();
            }
            else
            {
                this.GetNode<ShaderTest>("Body").OnMouseEnter(SelectedColor);
            }
            GridManager.GM.UiController.HideUnitVitals();
        }
        public void ConsumeMovementPoints(int mp)
        {
            MovementPoints += ConsumableMovementPoints;
            MovementPoints -= mp;
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
                if(GlovesOfDoom > 0)
                {
                    dmg *= 1.25f;
                }
                if(JockSmash)
                {
                    JockSmashValue+=1;
                    dmg *= JockSmashValue + 1;
                }

                HitPoints -= dmg;
            }
            else
            {
                HitPoints = 0.0f;
                //Dead
                IsDead = true;
            }
            this.GetNode<AnimatedSprite>("Body").SelfModulate = new Color("ff0000");
            Logger.UiLogger.Log(Logger.LogLevel.INFO, "Damage  =   =  " + dmg.ToString());
            GridManager.GM.UiController.ShowDmgAtUnit(this, dmg, TextColorType.Damage);
            GridManager.GM.camShake.Shake(0.35f);
            await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            this.GetNode<AnimatedSprite>("Body").SelfModulate = new Color("ffffff");

            if (HitPoints <= 0)
            {
                IsDead = true;
                OnDestroy();
                // await ToSignal(GetTree().CreateTimer(1.75f), "timeout");
                // this.GetNode<AnimatedSprite>("Body").Visible = false;
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
            // this.GetNode<ShaderTest>("Body").OnMouseEnter(SelectedColor);
            if(isForAttack)
            {
                GridManager.GM.OnUnitSelected(Cell, this);
            }
            else if(isForMove)
            {
                GD.Print("Can't MOVE, Movement points  = " + MovementPoints);
                if(TrappedInplace > 0 || MovementPoints <= 0) return;
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
            this.GetNode<ShaderTest>("Body").OnMouseExit();
            HasTakenTurn = true;
            IsMyTurn = false;
            IsSmokescreenApplied = false;
            DisableSmokeScreen();
            DisableCattenMadDog();
            if(TrappedInplace > 0)
            {
                ConsumeTrap();
            }
            GlovesOfDoom--;
            if(GlovesOfDoom <= 0 )
            {
                //disable gloves of doom effect
                GetNode<Node2D>("GlovesOfDoom").Hide();
            }
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

            // this.GetNode<AnimatedSprite>("Body").SelfModulate = new Color(1, 1, 1, 1f);
        }

        public void Reset()
        {
            IsAttackedByMadDog = false;
            JockSmash = false;
            JockSmashValue = 0;
            GetNode<Node2D>("JockSmash").Hide();
            HasTakenTurn = false;
            IsMyTurn = false;
            // DefenceFactor = 0;
            ConsumeCloak();
            MaskOfEEEEEE = false;
            GetNode<Node2D>("MaskOfEEEEE").Hide();
            IsHatTrick = false;
            MovementPoints = 2;
            GD.Print("INCREASING MOVEMENT POINTS");
            ConsumableMovementPoints = 0;
            UnMark(false);
            ConsumeShield();
            
        }

        public void MarkMyTurn(bool isSwitchingUnit = false){
            // if(!isSwitchingUnit)
            // {
            //     if(TrappedInplace > 0)
            //     {
            //         ConsumeTrap();
            //     }
            // }

            HasTakenTurn = false;
            this.GetNode<ShaderTest>("Body").OnMouseEnter(SelectedColor);
            cell.MarkAsSelected();
            GridManager.GM.SelectedUnit = this;
            IsMyTurn = true;
            GridManager.GM.CM.GreyoutNonUsableCards(data);
        }

        private void DisableSmokeScreen()
        {
            GetNode<Node2D>("SmallDogParticles").GetNode<Node2D>("CrosbyBubbles").GetChild<Node2D>(0).Hide();
            GetNode<Node2D>("MediumDogParticles").GetNode<Node2D>("CrosbyBubbles").GetChild<Node2D>(0).Hide();
            GetNode<Node2D>("LargeDogParticles").GetNode<Node2D>("CrosbyBubbles").GetChild<Node2D>(0).Hide();
        }

        private void ConsumeTrap()
        {
            TrappedInplace--;
            if(TrappedInplace <= 0)
            {
                TrappedInplace = 0;
                GD.Print("Pinky's effect vanishing soon");
                GetNode("SmallDogParticles").GetNode<Node2D>("PinkysSmolBrain").GetChild<EffectPlayer>(0).StopPinkyEffect();
                GetNode("MediumDogParticles").GetNode<Node2D>("PinkysSmolBrain").GetChild<EffectPlayer>(0).StopPinkyEffect();
                GetNode("LargeDogParticles").GetNode<Node2D>("PinkysSmolBrain").GetChild<EffectPlayer>(0).StopPinkyEffect();
            }
        }
        /// <summary>
        /// Method returns the unit to its base appearance
        /// </summary>
        public void UnMark(bool isMoving){
            GridManager.GM.SelectedUnit = null;
            if(!isMoving)
                cell.MarkAsDeselected();
            this.GetNode<ShaderTest>("Body").OnMouseExit();
            // this.GetNode<AnimatedSprite>("Body").SelfModulate = new Color(1, 1, 1, 1f);
            // GridManager.GM.OnUnitSelected(Cell);
        }

        public async void KnockOut()
        {
            //Play HBL Takedown Effect
            // GD.Print("GetNode = " + GetNode<Node2D>("HblTakedown").Name);
            // GD.Print("GetNode = " + GetNode<Node2D>("HblTakedown").GetNode<AnimatedSprite>("AnimatedSprite").Name);
            GetNode<Node2D>("HblTakedown").GetNode<AnimatedSprite>("AnimatedSprite").Play();
            await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
            DealDamage(HitPoints+1);
        }

        public async void InstantKnockOut()
        {
            //Play HBL Takedown Effect
            // GD.Print("GetNode = " + GetNode<Node2D>("HblTakedown").Name);
            // GD.Print("GetNode = " + GetNode<Node2D>("HblTakedown").GetNode<AnimatedSprite>("AnimatedSprite").Name);
            GetNode<Node2D>("HeartOfGlass").GetNode<AnimatedSprite>("AnimatedSprite").Show();
            GetNode<Node2D>("HeartOfGlass").GetNode<AnimatedSprite>("AnimatedSprite").Play();
            await ToSignal(GetTree().CreateTimer(4.0f), "timeout");
            DealDamage(HitPoints+1);
        }

         public async void ApplyWolfLairEffect(float dmg)
        {
            GetNode<Node2D>("WolfLair").GetNode<AnimatedSprite>("AnimatedSprite").Frame = 0;
            GetNode<Node2D>("WolfLair").GetNode<AnimatedSprite>("AnimatedSprite").Show();
            GetNode<Node2D>("WolfLair").GetNode<AnimatedSprite>("AnimatedSprite").Play();
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            DealDamage(dmg);
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            GetNode<Node2D>("WolfLair").GetNode<AnimatedSprite>("AnimatedSprite").Hide();
            GetNode<Node2D>("WolfLair").GetNode<AnimatedSprite>("AnimatedSprite").Stop();
        }

        public void OnDestroy()
        {
            MyPlayer.MyUnits.Remove(this);
            if(MyPlayer.MyUnits.Count == 0)
                GridManager.GM.UiController._on_SkipUnitTurn_mouse_entered();
            Cell.UnitDestroyed();
            GridManager.GM.UiController.HideUnitVitals();
            QueueFree();
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


