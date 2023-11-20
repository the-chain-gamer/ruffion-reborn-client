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
        private bool isAllyAttack = false;
        public bool PickNDrop{ get; private set; }
        public bool CanPlaceCones{ get; private set; }

        public void LoadCardDeck(Player player)
        {
            CardsList = new List<Card>();
            //loading from the deck server sent us
            int totalCards = StartupScript.Startup.DB.MyCardDeck.Count;
            PackedScene cardScene = (PackedScene)GD.Load("res://CardSystem/Scenes/Card.tscn");

            for (int i = 0; i < totalCards; i++) {
                Card card = (Card)cardScene.Instance();
                card.SetCardData(StartupScript.Startup.DB.GetDataByID(StartupScript.Startup.DB.MyCardDeck[i]));
                GridManager.GM.UiController.cardsList.GetChild(0).AddChild(card);
                CardsList.Add(card);
            }
        }

        public List<Card> GetCardsInDeck()
        {
            return CardsList;
        }

        public void AddCardsInGrid(int id)
        {
            PackedScene cardScene = (PackedScene)GD.Load("res://CardSystem/Scenes/Card.tscn");
            Card card = (Card)cardScene.Instance();
            card.SetCardData(StartupScript.Startup.DB.GetDataByID(id));
            GridManager.GM.UiController.cardsList.GetChild(0).AddChild(card);
        }

        public void ClearEverything()
        {
            GridManager.GM.CanSelectCell = false;
            CurrentCard = null;
            AppliedCard = null;
            isAllyAttack = false;
            PickNDrop = false;
            CanPlaceCones = false;
            GridManager.GM.UiController.ToggleCardsList(false);
            GridManager.GM.ClearUnsavedRoadblocks();
        }

        public void AttackOnCone(Cone cone)
        {
            List<TargetDog> targetDogs = new List<TargetDog>();
            List<MoveDogInput> opponantMoves = new List<MoveDogInput>();
            List<MoveDogInput> ourMoves = new List<MoveDogInput>();
            Unit unt = GridManager.GM.TM.GetCurrentUnit();
            CurrentCard.MakeConeAttack();
            AppliedCardInput appliedCardInput;
            targetDogs.Add(new TargetDog(cone.MyPlayer.OnlineID, cone.TileNo.ToString()));

            appliedCardInput = new AppliedCardInput(CurrentCard.CardID.ToString(), unt.UnitId.ToString(), targetDogs.ToArray());

            _ = StartupScript.Startup.Multiplayer.Attack(appliedCardInput, opponantMoves.ToArray(), ourMoves.ToArray());
        }

        public async void DropCard(Card card)
        {
            if(card != null && CanApply(card.Data))
            {
                GridManager.GM.UiController.ToggleCardsList();
                AppliedCard = card;
                CurrentCard = card.Data;
                AppliedCard.Hide();
                if(GridManager.GM.TM.GetCurrentUnit().IsSmokescreenApplied)
                {
                    GridManager.GM.UiController.ShowMessage("You have Crosby's Smokescreen");
                    AppliedCard.Show();
                    return;
                }
                else
                {
                    //Show a Message with Card's Name + Applied
                    if(CurrentCard.CardName != "Basic Attack")
                    {
                        GridManager.GM.UiController.ShowCardMessage(AppliedCard.CardImage.Texture);
                        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
                    }
                    if(CurrentCard.CardType == CardType.Manual)
                    {
                        // await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
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
        }
        public void ApplyPickNDrop(Unit target, int tileNo)
        {
            GridManager.GM.SelectedUnit = target;
            RequestApplyCard(tileNo);
        }
        public void RequestApplyCard(int tileNo = -8)
        {
            if(CurrentCard.CardName.Equals("Bethany's Road block"))
            {
                CanPlaceCones = false;
                GridManager.GM.CanSelectCell = false;
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

            var coneLst = GridManager.GM.GetMyRoadBlocks(unt.MyPlayer.PlayerNumber);
            if(coneLst.Count > 0)
            {
                for (int i=0; i<coneLst.Count; i++)
                {
                    targetDogs.Add(new TargetDog(unt.MyPlayer.OnlineID, coneLst[i].TileNo.ToString()));
                    coneLst[i].IsSynced = true;
                }

                GridManager.GM.ClearConesAddedTemp();

                var appliedCardInput = new AppliedCardInput(AppliedCard.Data.CardID.ToString(), unt.UnitId.ToString(), targetDogs.ToArray());

                _ = StartupScript.Startup.Multiplayer.Attack(appliedCardInput, opponantMoves.ToArray(), ourMoves.ToArray());
            }
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
            if(unt.IsMadDog > 0)
            {
                var random = new System.Random();
                // if(random.Next(2) == 0)
                // {
                    GridManager.GM.UiController.ShowMessage("Ally Attack");
                    // unt.MyPlayer.ConsumeMana(1);
                    // unt.IsMadDog = false;
                    //select an ally for attack
                    target = unt.MyPlayer.MyUnits.Find(x=> x != unt && !x.IsAttackedByMadDog);
                    if(target == null) target = GridManager.GM.SelectedUnit;
                // }
            }

            if (CurrentCard.CardName.Equals("Basic Attack") ||
                CurrentCard.CardName.Equals("Smolstein's Big Hit") ||
                CurrentCard.CardName.Equals("Pinky's Smol Brain") ||
                CurrentCard.CardName.Equals("HBL's Takedown") ||
                CurrentCard.CardName.Equals("Blondie Breaks A Heart Of Glass") ||
                CurrentCard.CardName.Equals("Mr. Stache's Gloves of Doom") ||
                CurrentCard.CardName.Equals("SwolMask's SWOL Mask of EEEE!") ||
                CurrentCard.CardName.Equals("Mittens - The Gloves Are Off") ||
                CurrentCard.CardName.Equals("Champ's Surprise") ||
                CurrentCard.CardName.Equals("Babeeee's Chair Toss") ||
                CurrentCard.CardName.Equals("The Wolf's Lair") ||
                CurrentCard.CardName.Equals("Jock Smash"))
            {
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                if(CurrentCard.CardName.Equals("HBL's Takedown"))
                {
                    targetDogs.Add(new TargetDog(unt.MyPlayer.OnlineID, unt.UnitId.ToString()));
                }
                float dmg = DmgCalculator(unt.data, CurrentCard);
                int hp = Convert.ToInt32(target.PreCalcDamage(CurrentCard.CardName.Equals("Basic Attack") ? 7.0f : dmg));
                
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), target.Cell.TileNo, hp));
            }
            else if(CurrentCard.CardName.Equals("Bubbles' Pick 'n' Drop"))
            {
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                float dmg = DmgCalculator(unt.data, CurrentCard);
                int hp = Convert.ToInt32(target.PreCalcDamage(CurrentCard.CardName.Equals("Basic Attack") ? 7.0f : dmg));
                
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), tileNum, hp));
            }
            else if(CurrentCard.CardName.Equals("Miss Cake By The Ocean"))
            {
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
                targetDogs.Add(new TargetDog(unt.MyPlayer.OnlineID, unt.UnitId.ToString()));
                
                float dmg = DmgCalculator(unt.data, CurrentCard);
                int hp = Convert.ToInt32(target.PreCalcDamage(CurrentCard.CardName.Equals("Basic Attack") ? 7.0f : dmg));
                
                opponantMoves.Add(new MoveDogInput(target.UnitId.ToString(), unt.Cell.TileNo, hp));
                ourMoves.Add(new MoveDogInput(unt.UnitId.ToString(), target.Cell.TileNo, hp));
            }
            else if (CurrentCard.CardName.Equals("Lalito's Laser"))
            {
                var myCell = unt.Cell;
                var myRow = GridManager.GM.GetMyRow(myCell, unt.MyPlayer.PlayerNumber == 2);
                //search for units on any tile from above list
                var enemyList = myRow.FindAll(x => 
                    x.CurrentUnit != null &&
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
                        if (successiveUnit > 0)
                        {
                            float tempDmg = dmg * 0.25f; // Reduce damage by 25% for successive hits
                            dmg -= tempDmg;
                        }
                        
                        int d = 0;
                        if (e.CurrentUnit != null)
                        {
                            d = Convert.ToInt32(e.CurrentUnit.PreCalcDamage(dmg));
                            targetDogs.Add(new TargetDog(e.CurrentUnit.MyPlayer.OnlineID, e.CurrentUnit.UnitId.ToString()));
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
                    if (T.IsTaken) continue;
                    int destCol = target.Cell.ColNum + (unt.IsFacingRight ? 1 : -1);
                    if (T.RowNum == unt.Cell.RowNum && T.ColNum == destCol)
                    {
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
                    if (t.CurrentUnit != null && t.CurrentUnit.MyPlayer.PlayerNumber == unt.MyPlayer.PlayerNumber)
                    {
                        var extraHp = (10 * t.CurrentUnit.data.UnitStats.Smarts) / t.CurrentUnit.data.UnitStats.Strength;
                        ourMoves.Add(new MoveDogInput(t.CurrentUnit.UnitId.ToString(), t.CurrentUnit.Cell.TileNo, Convert.ToInt32(t.CurrentUnit.HitPoints + extraHp)));
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
            {
                targetDogs.Add(new TargetDog(target.MyPlayer.OnlineID, target.UnitId.ToString()));
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
                    if (t.CurrentUnit != null && t.CurrentUnit.MyPlayer.PlayerNumber == unt.MyPlayer.PlayerNumber)
                    {
                        ourMoves.Add(new MoveDogInput(t.CurrentUnit.UnitId.ToString(), t.CurrentUnit.Cell.TileNo, Convert.ToInt32(t.CurrentUnit.HitPoints)));
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Catten"))
            {
                foreach (var e in GridManager.GM.TM.GetOpponentPlayer().MyUnits)
                {
                    targetDogs.Add(new TargetDog(e.MyPlayer.OnlineID, e.UnitId.ToString()));
                }
            }
            else if(CurrentCard.CardName.Equals("Viv's Weights"))
            {
                ourMoves.Add(new MoveDogInput(unt.UnitId.ToString(), unt.Cell.TileNo, Convert.ToInt32(unt.HitPoints)));
            }
            else if(CurrentCard.CardName.Equals("Mustachio's Throwdown"))
            {
                ourMoves.Add(new MoveDogInput(unt.UnitId.ToString(), unt.Cell.TileNo, Convert.ToInt32(unt.HitPoints)));
            }

            appliedCardInput = new AppliedCardInput(AppliedCard.Data.CardID.ToString(), unt.UnitId.ToString(), targetDogs.ToArray());

            _ = StartupScript.Startup.Multiplayer.Attack(appliedCardInput, opponantMoves.ToArray(), ourMoves.ToArray());
        }

        public async void MultiplayerAttack(AppliedCardInput cardInput, PlayerDataModel currentPlayer, List<PlayerDataModel> players = null)
        {
            var p = GridManager.GM.TM.GetPlayerByID(currentPlayer.ID.ToString());
            if(!p.IsLocalPlayer)
            {
                //get all unts for this player
                var units = p.MyUnits;

                //find dog who attacked
                var dog = units.Find(x=>x.UnitId.ToString().Equals(cardInput.DogId));
                await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
                //update profile picture position for other dogs
                 GridManager.GM.UiController.UpdateUnitsOnPlayerId(p, dog);
                //update profile pic for the dog who attacked
                GridManager.GM.UiController.UpdateProfilePic(p.PlayerNumber, dog);
            }
            CurrentCard = StartupScript.Startup.DB.GetDataByID(Convert.ToInt32(cardInput.CardId));

            ApplyCard(cardInput, currentPlayer, players);
        }

        public async void ApplyCard(AppliedCardInput cardInput, PlayerDataModel currentPlayer, List<PlayerDataModel> players = null)
        {
            //Deduct Mana Cost
            GridManager.GM.TM.ConsumeMana(CurrentCard.ManaCost);
            Unit myUnit = GridManager.GM.TM.GetPlayerByID(currentPlayer.ID).MyUnits.Find(x=>x.UnitId.ToString().Equals(cardInput.DogId));
            Unit targetUnit = null;
            Player player = null;
            
            if(cardInput.TargetDogsArray.Length > 0)
            {
                player = GridManager.GM.TM.GetPlayerByID(cardInput.TargetDogsArray[0].PlayerId);
                targetUnit = player.MyUnits.Find(x=>x.UnitId.ToString().Equals(cardInput.TargetDogsArray[0].DogId));
                //Check if we are attacking or applyng card on allies
                isAllyAttack = targetUnit != null && targetUnit.MyPlayer.OnlineID.Equals(myUnit.MyPlayer.OnlineID);
            }

            if(CurrentCard.CardName.Equals("Basic Attack"))
            {
                //Apply Damage or whatever
                targetUnit.DealDamage(7.0f, isAllyAttack);
            }
            else if(CurrentCard.CardName.Equals("Cone Attack"))
            {
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                List<Tile> TilesList = GridManager.GM.GetTilesList();
                foreach (var item in cardInput.TargetDogsArray)
                {
                    var tile = TilesList.Find(x=>x.TileNo.Equals(Convert.ToInt32(item.DogId)));
                    await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                    tile.CurrentCone.DealDamage(7.0f);
                }
            }
            else if(CurrentCard.CardName.Equals("Smolstein's Big Hit"))
            {
                //Effect
                PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/SCENES/ParticleScenes/SmolSteinsBigHit.tscn");
                BigHitEffectPlayer bigHit = (BigHitEffectPlayer)effectScene.Instance();
                GridManager.GM.AddChild(bigHit);
                if(myUnit.Cell.ColNum > targetUnit.Cell.ColNum)
                {
                    bigHit.Scale = new Vector2(-1, 1);
                }
                else if(myUnit.Cell.ColNum == targetUnit.Cell.ColNum)
                {
                    if(myUnit.Cell.Position.x > targetUnit.Cell.Position.x) bigHit.Scale = new Vector2(-1, 1);
                }
                bigHit.GlobalPosition = myUnit.GlobalPosition;
                bigHit.PlayHitAnim(myUnit ,targetUnit);
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
                            var extraHp = 10 * t.CurrentUnit.data.UnitStats.Smarts / t.CurrentUnit.data.UnitStats.Strength;
                            t.CurrentUnit.AddHP(extraHp);
                        }
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Wiz's Smol Telescope"))
            {
                ApplyCardEffectOnUnit(myUnit, CurrentCard);
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
                
                SoundManager.Instance.PlaySoundByName("BubbleSound");
                
                ApplyCardEffectOnUnit(targetUnit, CurrentCard);

                var myPlayerData = players.Find(x=>x.ID.Equals(targetUnit.MyPlayer.OnlineID));

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                var myDogData = myPlayerData.Assets.Dogs.Find(x=>x.DogId == targetUnit.UnitId.ToString());

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                var myTileNo = myDogData.Tile;
                var tile = TilesList.Find(x=>x.TileNo.Equals(myTileNo));
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

                GridManager.GM.UnblockTile(targetUnit.Cell);
                GridManager.GM.ResetGridVisuals();

                targetUnit.Cell = tile;
                tile.IsTaken = true;
                tile.CurrentUnit = targetUnit;
                // tile.MarkAsSelected();
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
                targetUnit.Position = tile.Position;
                targetUnit.DealDamage(dmg);                
            }
            else if (CurrentCard.CardName == "Bethany's Road block")
            {
                CanPlaceCones = false;
                var myId = StartupScript.Startup.Multiplayer.PlayerId;
                var targetId = cardInput.TargetDogsArray[0].PlayerId;
                if(myId != targetId)
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
                targetUnit.TrapInplace(2);
                targetUnit.DealDamage(dmg, isAllyAttack);
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
                
                List<Tile> neigbouringTiles =  GridManager.GM.GetNeigbourTiles(myUnit.Cell , myUnit);
                //check if any unit is on tile and add it to enemy list and then perform buffs+
                int tempCount = 0;
                foreach (Tile t in neigbouringTiles)
                {
                    if(t.CurrentUnit != null)
                    {
                        if(t.CurrentUnit.MyPlayer.PlayerNumber == myUnit.MyPlayer.PlayerNumber)
                        {
                            //Apply Shield / Add HP
                            t.CurrentUnit.AddConsumableShield(10, 1);
                            t.CurrentUnit.AddConsumableAttackFactor(2);
                            ApplyCardEffectOnUnit(t.CurrentUnit, CurrentCard);
                            tempCount++;
                        }
                    }
                }

                if(tempCount == 0)
                {
                    GridManager.GM.UiController.ShowMessage("No Neigbours");
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
            else if(CurrentCard.CardName.Equals("HBL's Takedown"))
            {
                if(cardInput.TargetDogsArray.Length > 0)
                {
                    foreach(TargetDog t in cardInput.TargetDogsArray)
                    {
                        player = GridManager.GM.TM.GetPlayerByID(t.PlayerId);
                        targetUnit = player.MyUnits.Find(x=>x.UnitId.ToString().Equals(t.DogId));
                        var u = targetUnit.MyPlayer.MyUnits.Find(x=>x.UnitId.ToString().Equals(t.DogId));
                        u.KnockOut();
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Blondie Breaks A Heart Of Glass"))
            {
                if(targetUnit != null)
                {
                    if(targetUnit.data.UnitStats.Strength < 3)
                    {
                        targetUnit.InstantKnockOut();
                    }
                    else
                    {
                        var dmg = 5 * (myUnit.data.UnitStats.Strength - 4);
                        targetUnit.DealDamage(dmg, isAllyAttack);
                    }
                }
            }
            else if(CurrentCard.CardName.Equals("Mr. Stache's Gloves of Doom"))
            {
                if(targetUnit != null)
                {
                    var dmg = 5 * (myUnit.data.UnitStats.Strength + 3);
                    targetUnit.DealDamage(dmg, isAllyAttack);
                    targetUnit.ApplyGlovesOfDoom();
                }
            }
            else if(CurrentCard.CardName.Equals("SwolMask's SWOL Mask of EEEE!"))
            {
                if(targetUnit != null)
                {
                    targetUnit.DealDamage(10.0f, isAllyAttack);
                    myUnit.ApplyMaskofEEEEEE();
                }
            }
            else if(CurrentCard.CardName.Equals("Viv's Weights"))
            {
                if(myUnit != null)
                {
                    ApplyVisWeight(myUnit);
                }
            }
            else if(CurrentCard.CardName.Equals("Mustachio's Throwdown"))
            {
                if(myUnit != null)
                {
                    ApplyThrowdown(myUnit);
                }
            }
            else if(CurrentCard.CardName.Equals("Jock Smash"))
            {
                if(targetUnit != null)
                {
                    targetUnit.ApplyJockSmash();
                }
            }
            else if (CurrentCard.CardName.Equals("Mittens - The Gloves Are Off"))
            {
                //Effect
                PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/Mittens.tscn");
                MittensEffectPlayer mittens = (MittensEffectPlayer)effectScene.Instance();
                GridManager.GM.AddChild(mittens);
                if(myUnit.Cell.ColNum > targetUnit.Cell.ColNum)
                {
                    mittens.Scale = new Vector2(-1, 1);
                }
                else if(myUnit.Cell.ColNum == targetUnit.Cell.ColNum)
                {
                    if(myUnit.Cell.Position.x > targetUnit.Cell.Position.x) mittens.Scale = new Vector2(-1, 1);
                }
                mittens.GlobalPosition = myUnit.GlobalPosition;
                mittens.PlayHitAnim(myUnit ,targetUnit);
            }

            else if (CurrentCard.CardName.Equals("Champ's Surprise"))
            {
                //Effect
                PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/ChampSurprise.tscn");
                ChampEffectPlayer champ = (ChampEffectPlayer)effectScene.Instance();
                GridManager.GM.AddChild(champ);
                if(myUnit.Cell.ColNum > targetUnit.Cell.ColNum)
                {
                    champ.Scale = new Vector2(-1, 1);
                }
                else if(myUnit.Cell.ColNum == targetUnit.Cell.ColNum)
                {
                    if(myUnit.Cell.Position.x > targetUnit.Cell.Position.x) champ.Scale = new Vector2(-1, 1);
                }
                champ.GlobalPosition = myUnit.GlobalPosition;
                champ.PlayHitAnim(myUnit ,targetUnit);
            }
            else if (CurrentCard.CardName.Equals("Babeeee's Chair Toss"))
            {
                if (targetUnit != null)
                {
                    var target = cardInput.TargetDogsArray[0];
                    var targetUnt = targetUnit.MyPlayer.MyUnits.Find(x => x.UnitId.ToString().Equals(target.DogId));
                    if (targetUnt != null)
                    {
                        PlayChairTossAnimation(myUnit, targetUnit);
                    }
                }
            }
            else if (CurrentCard.CardName.Equals("The Wolf's Lair"))
            {
                if (targetUnit != null)
                {
                    var target = cardInput.TargetDogsArray[0];
                    var targetUnt = targetUnit.MyPlayer.MyUnits.Find(x => x.UnitId.ToString().Equals(target.DogId));
                    if (targetUnt != null)
                    {
                        ApplyWolfsLair(myUnit, targetUnit);
                    }
                }
            }
            else if (CurrentCard.CardName.Equals("Miss Cake By The Ocean"))
            {
                PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/CakeOcean.tscn");
                Node2D missCakes = (Node2D)effectScene.Instance();
                Node2D missCakes2 = (Node2D)effectScene.Instance();
                myUnit.AddChild(missCakes);
                targetUnit.AddChild(missCakes2);
                missCakes.GlobalPosition = myUnit.GlobalPosition;
                missCakes2.GlobalPosition = targetUnit.GlobalPosition;
                missCakes.GetNode<AnimatedSprite>("AnimatedSprite").Play();
                missCakes2.GetNode<AnimatedSprite>("AnimatedSprite").Play();

                await ToSignal(GetTree().CreateTimer(2f), "timeout");

                float dmg = DmgCalculator(myUnit.data, CurrentCard);

                List<Tile> TilesList = GridManager.GM.GetTilesList();

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

                var myPlayerData = players.Find(x=>x.ID.Equals(myUnit.MyPlayer.OnlineID));
                var otherPlayerData = players.Find(x=>x.ID.Equals(targetUnit.MyPlayer.OnlineID));

                await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
                var myDogData = myPlayerData.Assets.Dogs.Find(x=>x.DogId == myUnit.UnitId.ToString());
                var otherDogData = otherPlayerData.Assets.Dogs.Find(x=>x.DogId == targetUnit.UnitId.ToString());

                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

                var myTileNo = myDogData.Tile;
                var otherTileNo = otherDogData.Tile;

                var tile1 = TilesList.Find(x=>x.TileNo.Equals(myTileNo));
                var tile2 = TilesList.Find(x=>x.TileNo.Equals(otherTileNo));

                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

                GridManager.GM.UnblockTile(myUnit.Cell);
                GridManager.GM.UnblockTile(targetUnit.Cell);
                GridManager.GM.ResetGridVisuals();

                targetUnit.Cell = tile2;
                myUnit.Cell = tile1;

                tile1.IsTaken = true;
                tile2.IsTaken = true;

                tile2.CurrentUnit = targetUnit;
                tile1.CurrentUnit = myUnit;

                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

                targetUnit.Position = tile2.Position;
                myUnit.Position = tile1.Position;
                targetUnit.DealDamage(dmg);

                await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
                missCakes.QueueFree();
                missCakes2.QueueFree();
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
            targetunit.DealDamage(dmg, isAllyAttack);
        }

        public async void ApplyLalitosLaser(Unit myUnit)
        {
            //Effect
            ApplyCardEffectOnUnit(myUnit, CurrentCard);
            await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
            SoundManager.Instance.PlaySoundByName("LalitoLaserSound");
            //Calculate Damage
            float dmg = DmgCalculator(myUnit.data, CurrentCard);
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
                        float tempDmg = dmg/100 * 25.0f;
                        dmg -= tempDmg;
                    }
                    //Apply Damage or whatever
                    if(e.CurrentUnit != null)
                    {
                        e.CurrentUnit.DealDamage(dmg, isAllyAttack);
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
            foreach(Unit u in targets)
            {
                u.IsSmokescreenApplied = show;
                ApplyCardEffectOnUnit(u, CurrentCard);
                EffectPlayer EP = (EffectPlayer)u.GetChild(4).GetChild<EffectPlayer>(0);
                EP.PlayAnimationOnce();
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
                    mUnit.Move(T, 0);
                    break;
                }
            }

            await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
            SoundManager.Instance.PlaySoundByName("TeleportCardSound");
            //Apply Damage or whatever
            target.DealDamage(dmg, isAllyAttack);
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
            mUnit.AddConsumableMovementPoints(moveRangeIncrease);
            await ToSignal(GetTree().CreateTimer(1.5f), "timeout");
            SoundManager.Instance.PlaySoundByName("ExplosionSound");

            mUnit.GetNode<Node2D>("MarlonSmokeBombLoop").GetChild<Node2D>(0).Show();
        }
        public async void ApplyHookDestructor()
        {
            ApplyCardEffectOnUnit(GridManager.GM.TM.GetCurrentUnit(), CurrentCard);
            float dmg = DmgCalculator(GridManager.GM.TM.GetCurrentUnitData(), CurrentCard);
            //Apply Damage or whatever
            GridManager.GM.SelectedUnit.DealDamage(dmg, isAllyAttack);
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            SoundManager.Instance.PlaySoundByName("DestructorSound");
        }
        public async void ApplyPenelopsGamble(Unit mUnit, Unit target)
        {//Effect
            if (target != null)
            {
                float dmg = DmgCalculator(mUnit.data, CurrentCard);
                //Apply Damage or whatever
                target.DealDamage(dmg, isAllyAttack);
                await ToSignal(GetTree().CreateTimer(2.2f), "timeout");
                SoundManager.Instance.PlaySoundByName("GambleSound");
                ApplyCardEffectOnUnit(mUnit, CurrentCard);
            }
            else
            {
                GridManager.GM.UiController.ShowMessage("Bad Gamble");
            }
        }
        
        public async void ApplyVisWeight(Unit unt)
        {
            //Effect
            PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/VivWeight.tscn");
            Node2D viv = (Node2D)effectScene.Instance();
            GridManager.GM.AddChild(viv);
            viv.GlobalPosition = unt.GlobalPosition;
            viv.GetNode<AnimatedSprite>("AnimatedSprite").Play();
            await ToSignal(GetTree().CreateTimer(4f), "timeout");
            unt.AddHP((unt.data.UnitStats.Strength * 2) + unt.MyPlayer.TotalMana/2);
            viv.QueueFree();
        }

        public async void ApplyThrowdown(Unit unt)
        {
            PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/ThrowDown.tscn");
            Node2D throwdown = (Node2D)effectScene.Instance();
            GridManager.GM.AddChild(throwdown);
            throwdown.GlobalPosition = unt.GlobalPosition;
            throwdown.GetNode<AnimatedSprite>("AnimatedSprite").Play();
            await ToSignal(GetTree().CreateTimer(4f), "timeout");
            unt.AddHP(10.0f);
            throwdown.QueueFree();
        }

        public async void ApplyMittens(Unit myUnit, Unit targetUnit)
        {
            SoundManager.Instance.PlaySoundByName("HitSound");
            //Calculate Damage
            int myMana = GridManager.GM.TM.MatchTotalTurns / 2;
            float dmg = myMana * myUnit.data.UnitStats.Strength + (myUnit.data.UnitStats.Strength - myMana);
            //Apply Damage or whatever
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
            targetUnit.DealDamage(dmg, isAllyAttack);
        }
        public async void ApplyChampSurprise(Unit myUnit, Unit targetunit)
        {
            SoundManager.Instance.PlaySoundByName("HitSound");
            //Calculate Damage

            float dmg = 12 / myUnit.data.UnitStats.Strength;
            //Apply Damage or whatever
            targetunit.DealDamage(dmg, isAllyAttack);
        }

        public async void PlayChairTossAnimation(Unit myUnit, Unit targetUnit)
        {
            //
            PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/Chair.tscn");
            ChairToss chair = (ChairToss)effectScene.Instance();
            GridManager.GM.AddChild(chair);
            chair.GlobalPosition = myUnit.GlobalPosition;
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            if(myUnit.Cell.ColNum >= targetUnit.Cell.ColNum)
            {
                chair.Scale = new Vector2(-1, 1);
            }
            chair.GlobalPosition = myUnit.GlobalPosition;
            chair.PlayHitAnim(myUnit ,targetUnit);
        }

        public async void ApplyChairToss(Unit mUnit, Unit target)
        {
            float dmg = mUnit.data.UnitStats.Strength + GridManager.GM.TM.GetCurrentPlayerTotalMana();
            int destCol;
            
            if (target.IsFacingRight)
                destCol = target.Cell.ColNum - 1;
            else
                destCol = target.Cell.ColNum + 1;

            foreach (var T in GridManager.GM.GetNeigbourTiles(target.Cell, GridManager.GM.SelectedUnit))
            {
                if (T.RowNum == target.Cell.RowNum && T.ColNum == destCol)
                {
                    target.DealDamage(3f);
                    if (!T.IsTaken)
                    {
                        target.Move(T, 0);
                    }
                    else
                    {
                        if(T.CurrentCone != null)
                        {
                            T.CurrentCone.DealDamage(3.0f);
                        }
                        else if(T.CurrentUnit != null)
                        {
                            T.CurrentUnit.DealDamage(3.0f);
                        }
                    }
                }
            }
        }

        public async void ApplyWolfsLair(Unit mUnit, Unit target)
        {
            //Calculate Damage
            float dmg = (6-mUnit.data.UnitStats.Strength) * 5;

            int enemyUnitCounts = target.MyPlayer.MyUnits.Count;
            int MUnitCounts = mUnit.MyPlayer.MyUnits.Count;
            int diff = enemyUnitCounts - MUnitCounts;
            if (diff >= 1)
            {
                target.ApplyWolfLairEffect(diff * dmg);
            }
            else
            {
                target.ApplyWolfLairEffect(dmg);
            }
        }

        public async void ApplyMissCake(Unit mUnit, Unit target, List<PlayerDataModel> players)
        {
            //Calculate Damage
            float dmg = mUnit.data.UnitStats.Strength * 2;
            //Effect
            PackedScene effectScene = (PackedScene)GD.Load("res://Cards_UI/NewDiscordEffects/SwolNewEffects/SwolEffectScenes/CakeOcean.tscn");
            Node2D missCakes = (Node2D)effectScene.Instance();
            Node2D missCakes2 = (Node2D)effectScene.Instance();
            mUnit.AddChild(missCakes);
            target.AddChild(missCakes2);
            missCakes.GlobalPosition = mUnit.GlobalPosition;
            missCakes2.GlobalPosition = target.GlobalPosition;
            missCakes.GetNode<AnimatedSprite>("AnimatedSprite").Play();
            missCakes2.GetNode<AnimatedSprite>("AnimatedSprite").Play();
            await ToSignal(GetTree().CreateTimer(4f), "timeout");
            
            Tile cell1 = mUnit.Cell;
            Tile cell2 = target.Cell;
            GridManager.GM.UnblockTile(cell1);
            GridManager.GM.UnblockTile(cell2);
            GridManager.GM.ResetGridVisuals();
            target.Cell = cell1;
            cell1.IsTaken = true;
            cell1.CurrentUnit = target;
            
            mUnit.Cell = cell2;
            cell2.IsTaken = true;
            cell2.CurrentUnit = mUnit;

            target.Position = cell1.Position;
            mUnit.Position = cell2.Position;

            target.DealDamage(dmg);


            await ToSignal(GetTree().CreateTimer(5f), "timeout");
            missCakes.QueueFree();
            missCakes2.QueueFree();
        }

        
        public bool CanApply(CardData card)
        {
            if(GridManager.GM.UiController.GetTime() <= 2)
                return false;
            if(card.CardName.Equals("Basic Attack"))
            {
                return GridManager.GM.TM.GetCurrentPlayerTotalMana() >= card.ManaCost;
            }
            else
            {
                //Check Mana Cost
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
            else if(nm.Equals("HBL's Takedown"))
            {
                return uData.UnitStats.Strength > 5;
            }
            else if(nm.Equals("Blondie Breaks A Heart Of Glass"))
            {
                return uData.UnitStats.Strength > 4;
            }
            else if(nm.Equals("Mr. Stache's Gloves of Doom"))
            {
                return uData.UnitStats.Strength > 3;
            }
            else if(nm.Equals("SwolMask's SWOL Mask of EEEE!"))
            {
                return uData.UnitStats.Strength > 4;
            }
            else if(nm.Equals("Viv's Weights"))
            {
                return uData.UnitStats.Strength > 3;
            }
            else if (nm.Equals("Mittens - The Gloves Are Off"))
            {
                return uData.UnitStats.Strength > 3;
            }
            else if (nm.Equals("Champ's Surprise"))
            {
                return uData.UnitStats.Strength < 4;
            }
            else if (nm.Equals("Babeeee's Chair Toss"))
            {
                return uData.UnitStats.Strength > 3;
            }
            else if (nm.Equals("The Wolf's Lair"))
            {
                return uData.UnitStats.Strength > 4;
            }
            else if (nm.Equals("Miss Cake By The Ocean"))
            {
                return uData.UnitStats.Strength > 4;
            }
            else if (nm.Equals("Mustachio's Throwdown"))
            {
                return uData.UnitStats.Strength > 4;
            }
            else if (nm.Equals("Jock Smash"))
            {
                return uData.UnitStats.Strength > 4;
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
                return 10 * uData.UnitStats.Smarts * 1; //replace 1 with no. of selected enemy's cartridges revieled
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
                float val = (float)uData.UnitStats.Smarts / (float)uData.UnitStats.Strength;
                return val * 10f; // with a 50% chance
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
                    if(!CanApply(c.Data))
                    {
                        c.GreyoutEffect(true);
                        c.MouseFilter = Control.MouseFilterEnum.Ignore;
                    }
                    else
                    {
                        c.GreyoutEffect(false);
                        c.MouseFilter = Control.MouseFilterEnum.Stop;
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
                    if(CanApply(c.Data))
                    {
                        pupperCardList.Add(c);
                    }
                }
            }

            return pupperCardList;
        }

         #region Show Cards WRT TO Selected Dog
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
                        if (RandomNumberList.Count == 5)
                        {
                            break;
                        }
                    }
                }
                ShowCardsInScroll(RandomNumberList);
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
        private async void ShowCardsInScroll(List<int> randomNumbersList)
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
                if (j == 0)
                {
                    await ToSignal(GetTree().CreateTimer(4.0f), "timeout");
                }
                else
                {
                    await ToSignal(GetTree().CreateTimer(0.4f), "timeout");
                }
                CardsList[randomNumbersList[j]].ShowCard();
            }
        }

        private void ApplyCardEffectOnUnit(Unit currentUnit, CardData CurrentCard)
        {
            if (currentUnit.UnitName == "Corgi" || currentUnit.UnitName == "Shiba" || currentUnit.UnitName == "Beagle" || currentUnit.UnitName == "Poodle")
            {
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
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetNode("SmallDogParticles").GetNode<Node2D>("PinkysSmolBrain").GetChild<EffectPlayer>(0);
                    EP.PlayPinkyEffect();
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
                    currentUnit.GetNode<Node2D>("SmallDogParticles").GetNode<Node2D>("SaundersLoop").GetChild<Node2D>(0).Show();
                }
                else if (CurrentCard.CardName.Equals("Catten"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(1).GetChild<Node2D>(10).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Crosby's Smokescreen"))
                {
                    currentUnit.GetNode<Node2D>("SmallDogParticles").GetNode<Node2D>("CrosbyBubbles").GetChild<Node2D>(0).Show();
                }
            }
            else if (currentUnit.UnitName == "Golden" || currentUnit.UnitName == "Labrador" )
            {
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
                    EP.PlayPinkyEffect();
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
                    currentUnit.GetNode<Node2D>("MediumDogParticles").GetNode<Node2D>("SaundersLoop").GetChild<Node2D>(0).Show();
                }
                else if (CurrentCard.CardName.Equals("Catten"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(2).GetChild<Node2D>(10).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Crosby's Smokescreen"))
                {
                    currentUnit.GetNode<Node2D>("MediumDogParticles").GetNode<Node2D>("CrosbyBubbles").GetChild<Node2D>(0).Show();
                }

            }
            else if (currentUnit.UnitName == "Malamute" || currentUnit.UnitName == "Mastiff" || currentUnit.UnitName == "Hound")
            {
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
                    EP.PlayPinkyEffect();
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
                    currentUnit.GetNode<Node2D>("LargeDogParticles").GetNode<Node2D>("SaundersLoop").GetChild<Node2D>(0).Show();
                }
                else if (CurrentCard.CardName.Equals("Catten"))
                {
                    currentUnit.GetChild(1).GetChild<Node2D>(0).Show();
                    EffectPlayer EP = (EffectPlayer)currentUnit.GetChild(3).GetChild<Node2D>(10).GetChild<EffectPlayer>(0);
                    EP.PlayAnimationOnce();
                }
                else if (CurrentCard.CardName.Equals("Crosby's Smokescreen"))
                {
                    currentUnit.GetNode<Node2D>("LargeDogParticles").GetNode<Node2D>("CrosbyBubbles").GetChild<Node2D>(0).Show();
                }
            }
        }
    }
}
