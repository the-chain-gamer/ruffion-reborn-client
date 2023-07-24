using Godot;
using RuffGdMainProject.GridSystem;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GameScene;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.UiSystem
{
    public class CardsManager : Node2D
    {
        public CardData CurrentCard;
        private Card AppliedCard;
        private List<Card> CardsList;
        private List<Card> ShuffleCardList;
        private List<int> RandomNumberList;
        public bool PickNDrop{ get; private set; }
        public bool CanPlaceCones{ get; private set; }

        public void LoadCardDeck(Player player)
        {
            CardsList = new List<Card>();
            //this is my local card deck
            // int totalCards = StartupScript.Startup.DB.MyCardDeck.Count;
            //loading from the deck server sent us
            int totalCards = StartupScript.Startup.DB.MyCardDeck.Count;
            PackedScene cardScene = (PackedScene)GD.Load("res://CardSystem/Scenes/Card.tscn");
            for (int i = 0; i < totalCards; i++) {
                Card card = (Card)cardScene.Instance();
                // GD.Print("StartupScript.Startup.DB.GetDataByID(StartupScript.Startup.DB.MyCardDeck[i] "
                //     +StartupScript.Startup.DB.GetDataByID(StartupScript.Startup.DB.MyCardDeck[i]));
                card.SetCardData(StartupScript.Startup.DB.GetDataByID(StartupScript.Startup.DB.MyCardDeck[i]));
                GridManager.GM.UiController.cardsList.GetChild(0).AddChild(card);
                CardsList.Add(card);
            }
        }

        public void AddCardsInGrid(int id)
        {
            PackedScene cardScene = (PackedScene)GD.Load("res://CardSystem/Scenes/Card.tscn");
            Card card = (Card)cardScene.Instance();
            card.SetCardData(StartupScript.Startup.DB.GetDataByID(id));
            GridManager.GM.UiController.cardsList.GetChild(0).AddChild(card);
        }

        public void AttackOnCone(Cone cone)
        {
            List<TargetDog> targetDogs = new List<TargetDog>();
            List<MoveDogInput> opponantMoves = new List<MoveDogInput>();
            List<MoveDogInput> ourMoves = new List<MoveDogInput>();
            GD.Print("CurrentCard.Name Before = " + CurrentCard.CardName);
            Unit unt = GridManager.GM.TM.GetCurrentUnit();
            CurrentCard.MakeConeAttack();
            AppliedCardInput appliedCardInput;
            GD.Print("CurrentCard.Name After = " + CurrentCard.CardName);

            // GD.Print("DEFAULT CARD APPLIED");
            targetDogs.Add(new TargetDog(cone.MyPlayer.OnlineID, cone.TileNo.ToString()));

            appliedCardInput = new AppliedCardInput(CurrentCard.CardID.ToString(), unt.UnitId.ToString(), targetDogs.ToArray());

            _ = StartupScript.Startup.Multiplayer.Attack(appliedCardInput, opponantMoves.ToArray(), ourMoves.ToArray());
        }

        public async void DropCard(Card card)
        {
            if(CanApply(card.Data))
            {
                GridManager.GM.UiController.ToggleCardsList();
                AppliedCard = card;
                CurrentCard = card.Data;
                AppliedCard.Hide();
                //Show a Message with Card's Name + Applied
                if(CurrentCard.CardName != "Basic Attack")
                {
                    if(GridManager.GM.TM.GetCurrentUnit().IsSmokescreenApplied)
                    {
                        GridManager.GM.UiController.ShowMessage("You have Crosby's Smokescreen");
                        return;
                    }
                    GridManager.GM.UiController.ShowCardMessage(AppliedCard.CardImage.Texture);
                    await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
                }
                if(AppliedCard.Data.CardType == CardType.Manual)
                {
                    GridManager.GM.CanSelectTarget = true;
                    GridManager.GM.UiController.ShowMessage("Select Target");
                    PickNDrop = AppliedCard.Data.CardName.Equals("Bubbles' Pick 'n' Drop");
                    CanPlaceCones = AppliedCard.Data.CardName.Equals("Bethany's Road block");

                    if(CanPlaceCones) GridManager.GM.UiController.ShowMessage("Select 3 Tiles");
                }
                else
                {
                    RequestApplyCard();
                }
            }
        }
        public void ApplyPickNDrop(Unit target, int tileNo)
        {
            GridManager.GM.SelectedUnit = target;
            GridManager.GM.UnblockTile(target.Cell);
            RequestApplyCard(tileNo);
        }
        public void RequestApplyCard(int tileNo = -8)
        {
            if(CurrentCard.CardName.Equals("Bethany's Road block"))
            {
                RequestConePlacement();
            }
            else
            {
                //Pre-Calculate everything
                Precalculate(tileNo);
            }
        }

        private void RequestConePlacement()
        {
            List<TargetDog> targetDogs = new List<TargetDog>();
            List<MoveDogInput> opponantMoves = new List<MoveDogInput>();
            List<MoveDogInput> ourMoves = new List<MoveDogInput>();
            Unit unt = GridManager.GM.TM.GetCurrentUnit();

            CanPlaceCones = false;
            var coneLst = GridManager.GM.GetMyRoadBlocks(unt.MyPlayer.PlayerNumber);
            for (int i=0; i<coneLst.Count; i++)
            {
                targetDogs.Add(new TargetDog(unt.MyPlayer.OnlineID, coneLst[i].TileNo.ToString()));
                coneLst[i].IsSynced = true;
            }

            var appliedCardInput = new AppliedCardInput(AppliedCard.Data.CardID.ToString(), unt.UnitId.ToString(), targetDogs.ToArray());

            _ = StartupScript.Startup.Multiplayer.Attack(appliedCardInput, opponantMoves.ToArray(), ourMoves.ToArray());
        }

        private void Precalculate(int tileNum = -9)
        {
            List<TargetDog> targetDogs = new List<TargetDog>();
            List<MoveDogInput> opponantMoves = new List<MoveDogInput>();
            List<MoveDogInput> ourMoves = new List<MoveDogInput>();

            Unit unt = GridManager.GM.TM.GetCurrentUnit();
            UnitDataModel dogModel = GlobalData.GD.GetUnitDataModel(unt);
            Unit target = GridManager.GM.SelectedUnit;

            AppliedCardInput appliedCardInput;

            if(CurrentCard.CardName.Equals("Basic Attack"))
            {
                // GD.Print("DEFAULT CARD APPLIED");
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                //Apply Damage or whatever
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), target.Cell.TileNo, Convert.ToInt32(target.PreCalcDamage(7.0f))));
            }
            else if(CurrentCard.CardName.Equals("Smolstein's Big Hit"))
            {
                // GD.Print("DEFAULT CARD APPLIED");
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                float dmg = DmgCalculator(unt.data, CurrentCard);
                //Apply Damage or whatever
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), target.Cell.TileNo, Convert.ToInt32(target.PreCalcDamage(dmg))));
            }
            else if (CurrentCard.CardName.Equals("Lalito's Laser"))
            {
                var myCell = unt.Cell;
                var myRow = GridManager.GM.GetMyRow(myCell, unt.MyPlayer.PlayerNumber == 2);
                //search for units on any tile from above list
                var enemyList = myRow.FindAll(
                    x => x.CurrentUnit != null &&
                        x.CurrentUnit.MyPlayer.PlayerNumber != unt.MyPlayer.PlayerNumber
                );

                //check if any unit is enemy and add it to enemy list and then perform attack
                if(enemyList.Count > 0)
                {
                    var successiveUnit = 0;
                    //Calculate Damage
                    var dmg = DmgCalculator(unt.data, CurrentCard);
                    foreach(var e in enemyList)
                    {
                        int d = 0;
                        if(successiveUnit > 0)
                        {
                            //Each successive Pupper hit gets damage reduced by 25%.
                            float tempDmg = dmg/100 * 25.0f;
                            dmg -= tempDmg;
                            GD.Print("Total Dmg after reduction is = " + dmg);
                        }
                        //Apply Damage or whatever
                        if(e.CurrentUnit != null)
                        {
                            d = Convert.ToInt32(e.CurrentUnit.PreCalcDamage(dmg));
                            targetDogs.Add(new TargetDog(e.CurrentUnit.MyPlayer.OnlineID, e.CurrentUnit.UnitId.ToString()));
                            //Apply Damage or whatever
                            opponantMoves.Add(new MoveDogInput(e.CurrentUnit.UnitId.ToString(), e.TileNo, Convert.ToInt32(d)));
                        }
                        successiveUnit++;
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Cyril's Hat Trick"))
            {
                //Calculate Damage
                float dmg = DmgCalculator(unt.data, CurrentCard);
                var TargetsCell = target.Cell;
                //Get neighbours of selected dog tile
                List<Tile> neighbouringTiles = GridManager.GM.GetNeigbourTiles(TargetsCell, target);
                Tile destinantion = neighbouringTiles[0];

                foreach (var T in neighbouringTiles)
                {
                    if(T.IsTaken) continue;
                    int destCol = target.Cell.ColNum;
                    if(unt.IsFacingRight)
                        destCol++;
                    else
                        destCol--;
                    if(T.RowNum == unt.Cell.RowNum && T.ColNum == destCol)
                    {
                        // GridManager.GM.TM.GetCurrentUnit().GlobalPosition = T.GlobalPosition;
                        destinantion = T;
                        break;
                    }
                }
                //Apply Damage or whatever
                int hp = Convert.ToInt32(target.PreCalcDamage(dmg));
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));

                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), target.Cell.TileNo, Convert.ToInt32(hp)));
                ourMoves.Add(new MoveDogInput(unt.UnitId.ToString(), destinantion.TileNo, Convert.ToInt32(unt.HitPoints)));
            }
            else if(CurrentCard.CardName.Equals("Kenzo's Smol Shield"))
            {
                var neigbouringTiles =  GridManager.GM.GetNeigbourTiles(unt.Cell , unt);
                var buff = (10 * unt.data.UnitStats.Smarts) / (unt.data.UnitStats.Strength);
                ourMoves.Add(new MoveDogInput(unt.UnitId.ToString(), unt.Cell.TileNo, Convert.ToInt32(unt.HitPoints+buff)));
                foreach (Tile t in neigbouringTiles)
                {
                    if(t.CurrentUnit != null)
                    {
                        if(t.CurrentUnit.MyPlayer.PlayerNumber == GridManager.GM.TM.GetCurrentUnit().MyPlayer.PlayerNumber)
                        {
                            //Apply Shield / Add HP
                            var extraHp = (10 * t.CurrentUnit.data.UnitStats.Smarts) / (t.CurrentUnit.data.UnitStats.Strength);
                            ourMoves.Add(new MoveDogInput(t.CurrentUnit.UnitId.ToString(), t.CurrentUnit.Cell.TileNo, Convert.ToInt32(t.CurrentUnit.HitPoints+extraHp)));
                        }
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
            {
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
            }
            else if(CurrentCard.CardName.Equals("Bubbles' Pick 'n' Drop"))
            {
                float dmg = DmgCalculator(unt.data, CurrentCard);
                var newHp = target.PreCalcDamage(dmg);
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), tileNum, Convert.ToInt32(target.PreCalcDamage(7.0f))));
            }
            else if(CurrentCard.CardName.Equals("Pinky's Smol Brain"))
            {
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                //Calculate Damage
                float dmg = DmgCalculator(unt.data, CurrentCard);
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), target.Cell.TileNo, Convert.ToInt32(target.PreCalcDamage(dmg))));
            }
            // else if(CurrentCard.CardName.Equals("Hook's Destructor"))
            // {
            //     ApplyHookDestructor();
            // }
            else if(CurrentCard.CardName.Equals("Penelope's Gamble"))
            {
                var random = new System.Random();
                if (random.Next(2) == 0)
                {
                    //Apply Damage or whatever
                    targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                    //Calculate Damage
                    float dmg = DmgCalculator(unt.data, CurrentCard);
                    opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), target.Cell.TileNo, Convert.ToInt32(target.PreCalcDamage(dmg))));
                }
            }
            else if(CurrentCard.CardName.Equals("Crosby's Smokescreen"))
            {
                //Effect
                // SoundManager.Instance.PlaySoundByName("GambleSound");
                var random = new System.Random();
                bool rnd = random.Next(0, 4) == 3;
                if(rnd)
                {
                    foreach (var u in GridManager.GM.TM.GetOpponentPlayer().MyUnits)
                    {
                        targetDogs.Add(new TargetDog(u.MyPlayer.OnlineID, u.UnitId.ToString()));
                        opponantMoves.Add(new MoveDogInput(u.UnitId.ToString(), u.Cell.TileNo, Convert.ToInt32(u.HitPoints)));
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Colonel Saunders"))
            {
                List<Tile> neigbouringTiles =  GridManager.GM.GetNeigbourTiles(unt.Cell , unt);
                //check if any unit is enemy and add it to enemy list and then perform attack
                foreach (Tile t in neigbouringTiles)
                {
                    if(t.CurrentUnit != null)
                    {
                        if(t.CurrentUnit.MyPlayer.PlayerNumber == unt.MyPlayer.PlayerNumber)
                        {
                            ourMoves.Add(new MoveDogInput(t.CurrentUnit.UnitId.ToString(), t.CurrentUnit.Cell.TileNo, Convert.ToInt32(t.CurrentUnit.HitPoints)));
                        }
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Catten"))
            {
                foreach (var e in GridManager.GM.TM.GetOpponentPlayer().MyUnits)
                {
                    var random = new System.Random();
                    if(random.Next(2) == 0)
                    {
                        targetDogs.Add(new TargetDog(e.MyPlayer.OnlineID, e.UnitId.ToString()));
                    }
                }
            }

            appliedCardInput = new AppliedCardInput(AppliedCard.Data.CardID.ToString(), unt.UnitId.ToString(), targetDogs.ToArray());

            _ = StartupScript.Startup.Multiplayer.Attack(appliedCardInput, opponantMoves.ToArray(), ourMoves.ToArray());
        }

        public async void MultiplayerAttack(AppliedCardInput cardInput, PlayerDataModel currentPlayer, List<PlayerDataModel> players = null)
        {
            GD.Print("cardID = " + cardInput.CardId);
            CurrentCard = StartupScript.Startup.DB.GetDataByID(Convert.ToInt32(cardInput.CardId));
            GridManager.GM.UiController.ToggleCardsList();

            GD.Print("CurrentCard.cardID = " + CurrentCard.CardID);
            GD.Print("CurrentCard.CardName = " + CurrentCard.CardName);

            ApplyCard(cardInput, currentPlayer, players);
        }

        public async void ApplyCard(AppliedCardInput cardInput, PlayerDataModel currentPlayer, List<PlayerDataModel> players = null)
        {
            // AppliedCard.Hide();
            //Deduct Mana Cost
            GridManager.GM.TM.ConsumeMana(CurrentCard.ManaCost);
            Unit myUnit = GridManager.GM.TM.GetPlayerByID(currentPlayer.ID).MyUnits.Find(x=>x.UnitId.ToString().Equals(cardInput.DogId));
            Unit targetUnit = null;
            if(cardInput.TargetDogsArray.Length > 0)
            {
                GD.Print("Getting Target Unit");
                targetUnit = GridManager.GM.TM.GetOpponentPlayer().MyUnits.Find(x=>x.UnitId.ToString().Equals(cardInput.TargetDogsArray[0].DogId));
            }

            // GD.Print("CURRENT CARD APPLIED = " + CurrentCard.CardName);
            // GD.Print("Apply Card GridManager.GM.TM.GetCurrentPlayerTotalMana() = " + GridManager.GM.TM.GetCurrentPlayerTotalMana());
            if(CurrentCard.CardName.Equals("Basic Attack"))
            {
                targetUnit = GridManager.GM.TM.GetOpponentPlayer().MyUnits.Find(x=>x.UnitId.ToString().Equals(cardInput.TargetDogsArray[0].DogId));
                // GD.Print("DEFAULT CARD APPLIED");
                //Apply Damage or whatever
                targetUnit.DealDamage(7.0f);
            }
            else if(CurrentCard.CardName.Equals("Cone Attack"))
            {
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                List<Tile> TilesList = GridManager.GM.GetTilesList();
                foreach (var item in cardInput.TargetDogsArray)
                {
                    var tile = TilesList.Find(x=>x.TileNo.Equals(Convert.ToInt32(item.DogId)));
                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                    tile.CurrentCone.DealDamage(7f);
                }
            }
            else if(CurrentCard.CardName.Equals("Smolstein's Big Hit"))
            {
                //Effect
                PackedScene effectScene = (PackedScene)GD.Load("res://Hamza_work/SCENES/ParticleScenes/SmolSteinsBigHit.tscn");
                EffectPlayer bigHit = (EffectPlayer)effectScene.Instance();
                GD.Print("%%%%% Created bigHit Effect..... :)");
                GridManager.GM.AddChild(bigHit);
                if(myUnit.MyPlayer.PlayerNumber == 2)
                {
                    bigHit.Scale = new Vector2(-1, 1);
                }
                bigHit.GlobalPosition = myUnit.GlobalPosition;
                // bigHit.GetChild<AnimationPlayer>(1).Play("bigHitAnim");
                bigHit.PlayAnimationOnce(targetUnit.GlobalPosition);
                //now waiting for callback function
                ApplyBigHit(myUnit ,targetUnit);
            }
            else if (CurrentCard.CardName.Equals("Lalito's Laser"))
            {
                ApplyLalitosLaser(myUnit);
            }
            else if(CurrentCard.CardName.Equals("Cyril's Hat Trick"))
            {
                ApplyCyrilsHatTrick(myUnit, targetUnit);
            }
            else if(CurrentCard.CardName.Equals("Marlon's Smol Smoke Bomb"))
            {
                ApplyMarlonSmokeBomb(myUnit);
            }
            else if(CurrentCard.CardName.Equals("Kenzo's Smol Shield"))
            {
                //Effect
                ApplyCardEffectOnUnit(myUnit, CurrentCard);
                SoundManager.Instance.PlaySoundByName("ShieldSound");
                List<Tile> neigbouringTiles =  GridManager.GM.GetNeigbourTiles(myUnit.Cell , myUnit);
                //check if any unit is enemy and add it to enemy list and then perform attack
                myUnit.AddHP((10 * myUnit.data.UnitStats.Smarts) / (myUnit.data.UnitStats.Strength));
                foreach (Tile t in neigbouringTiles)
                {
                    if(t.CurrentUnit != null)
                    {
                        if(t.CurrentUnit.MyPlayer.PlayerNumber == myUnit.MyPlayer.PlayerNumber)
                        {
                            //Apply Shield / Add HP
                            var extraHp = (10 * t.CurrentUnit.data.UnitStats.Smarts) / (t.CurrentUnit.data.UnitStats.Strength);
                            t.CurrentUnit.AddHP(extraHp);
                        }
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
            {
                ApplyCardEffectOnUnit(myUnit, CurrentCard);
                // SoundManager.Instance.PlaySoundByName("TelescopeSound");
                //View Enemy's Cards
                var lst = GetPupperCards(targetUnit.data);
                if(lst.Count > 0)
                {
                    myUnit.MyPlayer.ApplyRandomShieldDefence(lst.Count);
                    GridManager.GM.UiController.ShowOpponentCards(lst);
                }
            }
            else if(CurrentCard.CardName.Equals("Bubbles' Pick 'n' Drop"))
            {
                PickNDrop = false;
                float dmg = DmgCalculator(myUnit.data, CurrentCard);
                List<Tile> TilesList = GridManager.GM.GetTilesList();

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                var myPlayerData = players.Find(x=>x.ID.Equals(targetUnit.MyPlayer.OnlineID));

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                var myDogData = myPlayerData.Assets.Dogs.Find(x=>x.DogId == targetUnit.UnitId.ToString());

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                var myTileNo = myDogData.Tile;
                var tile = TilesList.Find(x=>x.TileNo.Equals(myTileNo));
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

                GridManager.GM.UnblockTile(targetUnit.Cell);
                GridManager.GM.ResetGridVisuals();

                targetUnit.DealDamage(dmg);
                SoundManager.Instance.PlaySoundByName("BubbleSound");
                ApplyCardEffectOnUnit(targetUnit, CurrentCard);

                targetUnit.Cell = tile;
                tile.IsTaken = true;
                tile.CurrentUnit = targetUnit;
                // tile.MarkAsSelected();
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                targetUnit.Position = tile.Position;
                // GD.Print("targetUnit.Position = " + targetUnit.Position);
            }
            else if(CurrentCard.CardName.Equals("Bethany's Road block"))
            {
                CanPlaceCones = false;
                // var myPlayerData = players.Find(x=>x.ID.Equals(myUnit.MyPlayer.OnlineID));
                // await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                // var cones = myPlayerData.Assets.Dogs;

                if(!StartupScript.Startup.Multiplayer.PlayerId.Equals(cardInput.TargetDogsArray[0].PlayerId))
                {
                    await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                    List<Tile> TilesList = GridManager.GM.GetTilesList();
                    foreach (var item in cardInput.TargetDogsArray)
                    {
                        var tile = TilesList.Find(x=>x.TileNo.Equals(Convert.ToInt32(item.DogId)));
                        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                        GridManager.GM.AddRoadBlock(tile, GridManager.GM.TM.GetPlayerByID(cardInput.TargetDogsArray[0].PlayerId), true);
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Pinky's Smol Brain"))
            {
                //Calculate Damage
                float dmg = DmgCalculator(myUnit.data, CurrentCard);
                //Apply Damage or whatever
                targetUnit.TrapInplace(3);
                targetUnit.DealDamage(dmg);
                //Effect
                SoundManager.Instance.PlaySoundByName("TrapSound");
                ApplyCardEffectOnUnit(targetUnit, CurrentCard);
            }
            // else if(CurrentCard.CardName.Equals("Hook's Destructor"))
            // {
            //     ApplyHookDestructor();
            // }
            else if(CurrentCard.CardName.Equals("Penelope's Gamble"))
            {
                ApplyPenelopsGamble(myUnit, targetUnit);
            }
            else if(CurrentCard.CardName.Equals("Crosby's Smokescreen"))
            {
                if(targetUnit != null)
                {
                    var targetUnits = GridManager.GM.TM.GetPlayerByID(cardInput.TargetDogsArray[0].PlayerId).MyUnits;
                    ApplySmokescreen(myUnit,targetUnits, true);
                }
                else
                {
                    var targetUnits = GridManager.GM.TM.GetOpponentPlayer().MyUnits;
                    ApplySmokescreen(myUnit, targetUnits, false);
                }
            }
            else if(CurrentCard.CardName.Equals("Colonel Saunders"))
            {
                //Effect
                SoundManager.Instance.PlaySoundByName("SaundersSound");
                ApplyCardEffectOnUnit(myUnit, CurrentCard);
                List<Tile> neigbouringTiles =  GridManager.GM.GetNeigbourTiles(myUnit.Cell , myUnit);
                // var teamList = neigbouringTiles.FindAll(
                //     x => x.CurrentUnit != null &&
                //     x.CurrentUnit.MyPlayer.PlayerNumber == GridManager.GM.TM.GetCurrentUnit().MyPlayer.PlayerNumber
                // );
                //check if any unit is enemy and add it to enemy list and then perform attack
                foreach (Tile t in neigbouringTiles)
                {
                    if(t.CurrentUnit != null)
                    {
                        if(t.CurrentUnit.MyPlayer.PlayerNumber == myUnit.MyPlayer.PlayerNumber)
                        {
                            //Apply Shield / Add HP
                            t.CurrentUnit.AddConsumableShield(10, 1);
                            t.CurrentUnit.AddConsumableAttackFactor(2);
                        }
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Catten"))
            {
                myUnit.MakeDeceptiveBlackSheep();
                if(targetUnit != null)
                {
                    foreach(TargetDog t in cardInput.TargetDogsArray)
                    {
                        var u = targetUnit.MyPlayer.MyUnits.Find(x=>x.UnitId.ToString().Equals(t.DogId));
                        u.MakeCattenMadDog();
                    }
                }

                //Effect
                await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
                SoundManager.Instance.PlaySoundByName("CattenSound");
                ApplyCardEffectOnUnit(myUnit, CurrentCard);
            }
            if(GridManager.GM.TM.GetCurrentPlayer().OnlineID.Equals(StartupScript.Startup.Multiplayer.PlayerId))
            {
                GreyoutNonUsableCards(GridManager.GM.TM.GetCurrentUnitData());
                GridManager.GM.UiController.ToggleCardsList(GridManager.GM.TM.GetCurrentPlayerTotalMana() <= 0);
            }
        }
        public async void ApplyBigHit(Unit myUnit, Unit targetunit)
        {
            SoundManager.Instance.PlaySoundByName("HitSound");
            //Calculate Damage
            float dmg = DmgCalculator(myUnit.data, CurrentCard);
            //Apply Damage or whatever
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
            targetunit.DealDamage(dmg);
        }

        public async void ApplyLalitosLaser(Unit myUnit)
        {
            //Effect
            ApplyCardEffectOnUnit(myUnit, CurrentCard);
            await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
            SoundManager.Instance.PlaySoundByName("LalitoLaserSound");
            //Calculate Damage
            float dmg = DmgCalculator(myUnit.data, CurrentCard);
            GD.Print("Total Dmg for Lalito's Laser is = " + dmg);
            var myRow = GridManager.GM.GetMyRow(myUnit.Cell, myUnit.MyPlayer.PlayerNumber == 2);
            //search for units on any tile from above list
            var enemyList = myRow.FindAll(
                x => (x.CurrentUnit != null &&
                    x.CurrentUnit.MyPlayer.PlayerNumber != myUnit.MyPlayer.PlayerNumber) ||
                (x.CurrentCone != null &&
                    x.CurrentCone.MyPlayer.PlayerNumber != myUnit.MyPlayer.PlayerNumber)
            );

            //check if any unit is enemy and add it to enemy list and then perform attack
            if(enemyList.Count > 0)
            {
                var successiveUnit = 0;
                foreach(var e in enemyList)
                {
                    if(successiveUnit > 0)
                    {
                        //Each successive Pupper hit gets damage reduced by 25%.
                        GD.Print("Total Dmg before reduction is = " + dmg);
                        float tempDmg = dmg/100 * 25.0f;
                        GD.Print("tempDmg is = " + tempDmg);
                        dmg -= tempDmg;
                        GD.Print("Total Dmg after reduction is = " + dmg);
                    }
                    //Apply Damage or whatever
                    if(e.CurrentUnit != null)
                    {                       
                        e.CurrentUnit.DealDamage(dmg);
                    }
                    else
                    {
                        e.CurrentCone.DealDamage(dmg);
                    }
                    successiveUnit++;
                }
            }
        }

        public async void ApplySmokescreen(Unit mUnit, List<Unit> targets, bool show)
        {
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            SoundManager.Instance.PlaySoundByName("SmokeScreenSound");
            GD.Print("SMOKESCREEN main ");
            foreach(Unit u in targets)
            {
                GD.Print("SMOKESCREEN foreach LLOOPP !");
                u.IsSmokescreenApplied = show;
                EffectPlayer EP = (EffectPlayer)u.GetChild(4).GetChild<EffectPlayer>(0);
                EP.PlayAnimationOnce();
                GD.Print("SMOKESCREEN applied !");
            }
        }

        public async void ApplyCyrilsHatTrick(Unit mUnit, Unit target)
        {
            ApplyCardEffectOnUnit(mUnit, CurrentCard);
            //Calculate Damage
            float dmg = DmgCalculator(mUnit.data, CurrentCard);
            var TargetsCell = target.Cell;
            //Get neighbours of selected dog tile
            List<Tile> neighbouringTiles = GridManager.GM.GetNeigbourTiles(TargetsCell, GridManager.GM.SelectedUnit);
            // GD.Print("My row",currentTargetUnit.Cell.RowNum);
            // GD.Print("My Col",currentTargetUnit.Cell.ColNum);
            foreach (var T in neighbouringTiles)
            {
                if(T.IsTaken) continue;
                int destCol = target.Cell.ColNum;
                if(mUnit.IsFacingRight)
                    destCol++;
                else
                    destCol--;
                if(T.RowNum == target.Cell.RowNum && T.ColNum == destCol)
                {
                    // GridManager.GM.TM.GetCurrentUnit().GlobalPosition = T.GlobalPosition;
                    mUnit.Move(T, 0);
                    break;
                }
            }

            await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
            SoundManager.Instance.PlaySoundByName("TeleportCardSound");
            //Apply Damage or whatever
            target.DealDamage(dmg);
            mUnit.AddConsumableCloak(1, true); // The Small Pup is not targetable until its next turn
        }
        public async void ApplyMarlonSmokeBomb(Unit mUnit)
        {
            //Effect
            EffectPlayer EP = (EffectPlayer) mUnit.GetChild(5).GetChild<EffectPlayer>(0);
            EP.PlayAnimationOnce();
            //Add Cloack value for next three turns

            mUnit.AddConsumableCloak(1);
            int moveRangeIncrease = (int)Mathf.Round((mUnit.data.UnitStats.Smarts + mUnit.data.UnitStats.Science) / 2);
            // GD.Print("Marlon Smol Smoke Bomb increase = " + moveRangeIncrease);
            mUnit.AddConsumableMovementPoints(moveRangeIncrease);
            await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
            SoundManager.Instance.PlaySoundByName("ExplosionSound");
        }
        public async void ApplyHookDestructor()
        {
            ApplyCardEffectOnUnit(GridManager.GM.TM.GetCurrentUnit(), CurrentCard);
            float dmg = DmgCalculator(GridManager.GM.TM.GetCurrentUnitData(), CurrentCard);
            //Apply Damage or whatever
            GridManager.GM.SelectedUnit.DealDamage(dmg);
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            SoundManager.Instance.PlaySoundByName("DestructorSound");
        }
        public async void ApplyPenelopsGamble(Unit mUnit, Unit target)
        {//Effect
            if (target != null)
            {
                float dmg = DmgCalculator(mUnit.data, CurrentCard);
                //Apply Damage or whatever
                target.DealDamage(dmg);
                await ToSignal(GetTree().CreateTimer(2.2f), "timeout");
                SoundManager.Instance.PlaySoundByName("GambleSound");
                ApplyCardEffectOnUnit(mUnit, CurrentCard);
            }
            else
            {
                GridManager.GM.UiController.ShowMessage("Bad Gamble");
            }
        }
        public bool CanApply(CardData card)
        {
            if(card.CardName.Equals("Basic Attack"))
            {
                return GridManager.GM.TM.GetCurrentPlayerTotalMana() >= card.ManaCost;
            }
            else
            {
                //Check Mana Cost
                // GD.Print("GridManager.GM.TM.GetCurrentPlayerTotalMana() = " + GridManager.GM.TM.GetCurrentPlayerTotalMana());
                // GD.Print("card.ManaCost = " + card.ManaCost);
                return GridManager.GM.TM.GetCurrentPlayerTotalMana() >= card.ManaCost && ReqsOkay(card.CardName, GridManager.GM.TM.GetCurrentUnitData());
            }
        }

        private bool ReqsOkay(string nm, UnitData uData)
        {
            if(nm.Equals("Smolstein's Big Hit"))
            {
                return uData.UnitStats.Size < 3;
            }
            else if(nm.Equals("Lalito's Laser"))
            {
                return uData.UnitStats.Smarts > 4 && uData.UnitStats.Science > 3;
            }
            else if(nm.Equals("Marlon's Smol Smoke Bomb"))
            {
                return uData.UnitStats.Smarts > 5 && uData.UnitStats.Science > 4;
            }
            else if(nm.Equals("Kenzo's Smol Shield"))
            {
                return uData.UnitStats.Strength < 5 && uData.UnitStats.Smarts > 4;
            }
            else if(nm.Equals("Wiz's Smol Telescope"))
            {
                return uData.UnitStats.Smarts > 6 && uData.UnitStats.Science > 5;
            }
            else if(nm.Equals("Bubbles' Pick 'n' Drop"))
            {
                return uData.UnitStats.Smarts > 3;
            }
            else if(nm.Equals("Bethany's Road block"))
            {
                return uData.UnitStats.Smarts > 4;
            }
            else if(nm.Equals("Pinky's Smol Brain"))
            {
                return uData.UnitStats.Smarts > 5;
            }
            else if(nm.Equals("Hook's Destructor"))
            {
                return uData.UnitStats.Smarts > 6;
            }
            else if(nm.Equals("Penelope's Gamble"))
            {
                return uData.UnitStats.Strength > 3;
            }
            else if(nm.Equals("Crosby's Smokescreen"))
            {
                return true;
            }
            else if(nm.Equals("Cyril's Hat Trick"))
            {
                return uData.UnitStats.Strength < 4;
            }
            else if(nm.Equals("Colonel Saunders"))
            {
                return uData.UnitStats.Smarts > 3;
            }
            else if(nm.Equals("Catten"))
            {
                return uData.UnitStats.Smarts > 3;
            }
            return false;
        }

        private float DmgCalculator(UnitData uData, CardData cData)
        {
            if(cData.CardName.Equals("Smolstein's Big Hit"))
            {
                return 10 * (3 - uData.UnitStats.Size);
            }
            else if(cData.CardName.Equals("Lalito's Laser"))
            {
                return (5 * (uData.UnitStats.Smarts + uData.UnitStats.Science)) / uData.UnitStats.Strength;
            }
            else if(cData.CardName.Equals("Marlon's Smoke Bomb"))
            {
                return Mathf.Round((uData.UnitStats.Smarts + uData.UnitStats.Science) / 2);
            }
            else if(cData.CardName.Equals("Kenzo's Smol Shield"))
            {
                //need to recalculate for all neighboring units
                return 10 * uData.UnitStats.Smarts / uData.UnitStats.Strength;
            }
            else if(cData.CardName.Equals("Wiz's Smol Telescope"))
            {
                //need to recalculate for all neighboring units
                return 10 * uData.UnitStats.Smarts * 1; //replace 1 with no. of seected enemy's cartridges revieled
            }
            else if(cData.CardName.Equals("Bubbles' Pick 'n' Drop"))
            {
                return uData.UnitStats.Smarts * 5;
            }
            else if(cData.CardName.Equals("Bethany's Road block"))
            {
                return 10; //10 hp for each cone
            }
            else if(cData.CardName.Equals("Pinky's Smol Brain"))
            {
                return uData.UnitStats.Smarts * 5;
            }
            else if(cData.CardName.Equals("Hook's Destructor"))
            {
                return uData.UnitStats.Smarts * 10; // with a 30% chance to destroy cartridge NFT in opp deck
            }
            else if(cData.CardName.Equals("Penelope's Gamble"))
            {
                return (uData.UnitStats.Smarts/uData.UnitStats.Strength) * 10; // with a 50% chance
            }
            else if(cData.CardName.Equals("Crosby's Smokescreen"))
            {
                return 0; // with a 25% chance of enemy pups missing thier turn
            }
            else if(cData.CardName.Equals("Cyril's Hat Trick"))
            {
                return 24/uData.UnitStats.Strength; // land behind target
            }
            else if(cData.CardName.Equals("Basic Attack"))
            {
                return -7.0f;
            }
            return 0.0f;
        }

        public void GreyoutNonUsableCards(UnitData uData)
        {
            if(CardsList.Count > 0)
            {
                foreach (var c in CardsList)
                {
                    // GD.Print("CanApply(c.Data) = " + CanApply(c.Data));
                    if(!CanApply(c.Data))
                    {
                        c.GreyoutEffect(true);
                        c.MouseFilter = Control.MouseFilterEnum.Ignore;
                        // GD.Print("GreyoutNonUsableCards >>>>>>>>>>>> ");
                    }
                    else
                    {
                        c.GreyoutEffect(false);
                        c.MouseFilter = Control.MouseFilterEnum.Stop;
                        // GD.Print("COLORIZE GreyoutNonUsableCards >>>>>>>>>>>> ");
                    }
                }
            }
        }

        public List<Card> GetPupperCards(UnitData uData)
        {
            List<Card> pupperCardList = new List<Card>();
            if(CardsList.Count > 0)
            {
                foreach (var c in CardsList)
                {
                    // GD.Print("CanApply(c.Data) = " + CanApply(c.Data));
                    if(CanApply(c.Data))
                    {
                        pupperCardList.Add(c);
                    }
                }
            }

            return pupperCardList;
        }

         #region Show Cards WRT TO Selected Dog
        //Newly Added
        public void AddCardInShuffleCardList()
        {
            ShuffleCardList = new List<Card>();
            for (int i = 0; i < CardsList.Count; i++)
            {
                if (ReqsOkay(CardsList[i].Data.CardName, GridManager.GM.TM.GetCurrentUnitData()))
                {
                    ShuffleCardList.Add(CardsList[i]);
                }
            }

            // GD.Print("=========Scroll Card Count is +++++++"+ GridManager.GM.UiController.cardsList.GetChild(0).GetChildCount());

            AddCardsInScroll(ShuffleCardList);
        }

        private void AddCardsInScroll(List<Card> ShuffleCardList)
        {
            if (GridManager.GM.UiController.cardsList.GetChild(0).GetChildCount() > 0)
            {
                for (int i = 0; i < GridManager.GM.UiController.cardsList.GetChild(0).GetChildCount(); i++) //remove all childs from Scroll
                {
                    GridManager.GM.UiController.cardsList.GetChild<GridContainer>(0).GetChild<Control>(i).Hide();
                }
            }

            for (int i = 0; i < CardsList.Count; i++) //remove all childs from Scroll
            {
                for (int j = 0; j < ShuffleCardList.Count; j++)
                {
                    if (CardsList[i].Data.CardName == ShuffleCardList[j].Data.CardName)
                    {
                        CardsList[i].ShowCard();
                    }
                }
            }
        }

        #endregion

        public void ShowRandomCardsInContainer()
        {
            if (CardsList.Count > 5)
            {
                RandomNumberList = new List<int>();
                while (RandomNumberList.Count < 5)
                {
                    var random = new RandomNumberGenerator();
                    random.Randomize();
                    int randNumber = random.RandiRange(0, CardsList.Count - 1);
                    if (CheckUniqueNumber(RandomNumberList, randNumber))
                    {
                        RandomNumberList.Add(randNumber);
                        // GD.Print("+++++++++++++++++++++++=======RandomNumberList Count is=========" + randNumber);
                        if (RandomNumberList.Count == 5)
                        {
                            // GD.Print("+++++++++++++++++++++++======= Inside RandomNumberList Count is=========" + RandomNumberList.Count);
                            break;
                        }
                    }
                }
                // GD.Print("+++++++++++++++++++++++=======RandomNumberList Count is=========" + RandomNumberList.Count);

                ShowCardsinScroll(RandomNumberList);
            }
            else
            {
                for (int j = 0; j < CardsList.Count; j++)
                {
                    CardsList[j].ShowCard();
                }
            }
        }

        private bool CheckUniqueNumber(List<int> randomNumbersList, int num)
        {
            return !Convert.ToBoolean(randomNumbersList.Find(x => x.Equals(num)));
        }
        private void ShowCardsinScroll(List<int> randomNumbersList)
        {
            if (GridManager.GM.UiController.cardsList.GetChild(0).GetChildCount() > 0)
            {
                for (int i = 0; i < GridManager.GM.UiController.cardsList.GetChild(0).GetChildCount(); i++) //remove all childs from Scroll
                {
                    GridManager.GM.UiController.cardsList.GetChild<GridContainer>(0).GetChild<Control>(i).Hide();
                }
            }

            for (int j = 0; j < randomNumbersList.Count; j++)
            {
                CardsList[randomNumbersList[j]].ShowCard();
            }
        }

        private void ApplyCardEffectOnUnit(Unit currentUnit, CardData CurrentCard)
        {
            // GD.Print("&&&&&&&&&&&&& Name  of Card is &&&&&&&&&&&" + CurrentCard.CardName);
            // GD.Print("&&&&&&&&&&&&& Name  of Unit is &&&&&&&&&&&" + currentUnit.UnitName);

            if (currentUnit.UnitName == "Corgi" || currentUnit.UnitName == "Shiba" || currentUnit.UnitName == "Beagle" || currentUnit.UnitName == "Poodle")
            {
                // if (CurrentCard.CardName.Equals("Smolstein's Big Hit"))
                // {
                //     //Effect
                //     currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                //     EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(4).GetChild<EffectPlayer>(0);
                //     EP.PlayAnimationOnce(currentUnit.GlobalPosition);
                // }
                // else
                 if (CurrentCard.CardName.Equals("Lalito's Laser"))
                {
                    //Effect
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(0).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else 
                if (CurrentCard.CardName.Equals("Cyril's Hat Trick"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(1).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Kenzo's Smol Shield"))
                {
                    //Effect
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(2).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(5).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Bubbles' Pick 'n' Drop"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(8).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }

                else if (CurrentCard.CardName.Equals("Pinky's Smol Brain"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(9).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Hook's Destructor"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(7).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Penelope's Gamble"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(3).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Colonel Saunders"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(6).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Catten"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(10).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
            }
            else if (currentUnit.UnitName == "Golden" || currentUnit.UnitName == "Labrador" )
            {

                // if (CurrentCard.CardName.Equals("Smolstein's Big Hit"))
                // {
                //     //Effect
                //     currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                //     EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(4).GetChild<EffectPlayer>(0);
                //     EP.PlayAnimationOnce(GridManager.GM.SelectedUnit.GlobalPosition);
                // }
                // else 
                if (CurrentCard.CardName.Equals("Lalito's Laser"))
                {
                    //Effect
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(0).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else 
                if (CurrentCard.CardName.Equals("Cyril's Hat Trick"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(1).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Kenzo's Smol Shield"))
                {
                    //Effect
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(2).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(5).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Bubbles' Pick 'n' Drop"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(8).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }

                else if (CurrentCard.CardName.Equals("Pinky's Smol Brain"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(9).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Hook's Destructor"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(7).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Penelope's Gamble"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(3).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Colonel Saunders"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(6).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Catten"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(10).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }

            }
            else if (currentUnit.UnitName == "Malamute" || currentUnit.UnitName == "Mastiff" || currentUnit.UnitName == "Hound")
            {
                // if (CurrentCard.CardName.Equals("Smolstein's Big Hit"))
                // {
                //     //Effect
                //     currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                //     EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(4).GetChild<EffectPlayer>(0);
                //     EP.PlayAnimationOnce(currentUnit.GlobalPosition);
                // }
                // else 
                if (CurrentCard.CardName.Equals("Lalito's Laser"))
                {
                    //Effect
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(0).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else
                 if (CurrentCard.CardName.Equals("Cyril's Hat Trick"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(1).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Kenzo's Smol Shield"))
                {
                    //Effect
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(2).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(5).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Bubbles' Pick 'n' Drop"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(8).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
               
                else if (CurrentCard.CardName.Equals("Pinky's Smol Brain"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(9).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Hook's Destructor"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(7).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Penelope's Gamble"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(3).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Colonel Saunders"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(6).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Catten"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(10).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
            }
        }
    }
}
