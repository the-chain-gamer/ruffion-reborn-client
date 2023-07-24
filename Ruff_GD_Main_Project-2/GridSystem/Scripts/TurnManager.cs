using Godot;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GameScene;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.GridSystem
{
    public class TurnManager : Node2D
    {
        // Signal emitted when the turn changes
        [Signal]
        public delegate void TurnChangeSignal(int currentPlayer, int playerTotalTurns, int matchTotalTurns);

        // Flags to track game state
        public bool GameStarted = false;
        public bool IsGameOver = false;

        // Properties to track turn information
        public int CurrentTurnNumber { get; private set; }
        public int P1TotalTurns { get; private set; }
        public int P2TotalTurns { get; private set; }
        public int MatchTotalTurns { get; private set; }
        public int CurrentPlayerNumber { get; private set; }

        // List of players in the game
        private List<Player> Players = new List<Player>();

        // Add a player to the list
        public void AddPlayers(Player player)
        {
            Players.Add(player);
        }

        // Start the turns
        public void StartTurns(PlayerDataModel currentPlayer)
        {
            CurrentTurnNumber = 0;
            MatchTotalTurns = 0;

            _ = StartupScript.Startup.Multiplayer.ClaimTurnRequest();

            // //For Multiplayer
            // var random = new RandomNumberGenerator();
            // random.Randomize();
            // CurrentPlayerNumber = random.RandiRange(1, 2);

            // // Emit the turn change signal
            // EmitSignal("TurnChangeSignal", CurrentPlayerNumber, Players[CurrentPlayerNumber - 1].TotalTurnsTaken, MatchTotalTurns);
            // // Play the turn change sound
            // SoundManager.Instance.PlaySoundByName("TurnChangeSound");

            // // Mark the current player's turn
            // Players[CurrentPlayerNumber - 1].MarkMyTurn();
            // GridManager.GM.UiController.StartTimer(CurrentPlayerNumber);
        }

        public void CheckIfChangeTurn(Player player)
        {
            if(CurrentPlayerNumber != player.PlayerNumber)
            {
                CurrentPlayerNumber = player.PlayerNumber;
                ChangeTurn(player.OnlineID);
            }
        }

        public void RequestChangeTurn()
        {
            int localPlayer = Players.Find(x => x.IsLocalPlayer).PlayerNumber;
            GD.Print("localPlayer = >>>> " + localPlayer);
            if(localPlayer != CurrentPlayerNumber)
            {
                _ = StartupScript.Startup.Multiplayer.ClaimTurnRequest();
            }
            else if(localPlayer == CurrentPlayerNumber)
            {
                _ = StartupScript.Startup.Multiplayer.SkipTurnRequest();
            }
        }

        // Change the turn to the next player
        public void ChangeTurn(string id)
        {
            GD.Print("PLAYER ID == = = "+id);
            GridManager.GM.ResetGridVisuals();
            MatchTotalTurns++;
            if (MatchTotalTurns > 24)
            {
                // Game ended
                int p1UnitsAlive = Players[0].MyUnits.FindAll(x => !x.IsDead).Count;
                int p2UnitsAlive = Players[1].MyUnits.FindAll(x => !x.IsDead).Count;

                if (p1UnitsAlive > p2UnitsAlive)
                {
                    // Player 1 won
                    GridManager.GM.UiController.GameEnded("Player 1 Won!");
                }
                else if (p2UnitsAlive > p1UnitsAlive)
                {
                    // Player 2 won
                    GridManager.GM.UiController.GameEnded("Player 2 Won!");
                }
                else if (p2UnitsAlive == p1UnitsAlive)
                {
                    // Compare total HP
                    var totalP1Hp = 0.0f;
                    foreach (var unit in Players[0].MyUnits)
                    {
                        totalP1Hp += unit.HitPoints;
                    }

                    var totalP2Hp = 0.0f;
                    foreach (var unit in Players[1].MyUnits)
                    {
                        totalP2Hp += unit.HitPoints;
                    }

                    if (totalP1Hp > totalP2Hp)
                    {
                        GridManager.GM.UiController.GameEnded("Player 1 Won!");
                    }
                    else if (totalP2Hp > totalP1Hp)
                    {
                        GridManager.GM.UiController.GameEnded("Player 2 Won!");
                    }
                    else
                    {
                        GridManager.GM.UiController.GameEnded("It's a Draw...");
                    }
                }
            }
            else
            {
                Players[CurrentPlayerNumber - 1].OnTurnEnded();
                GridManager.GM.CanSelectTarget = false;
                // Switch to the next player's turn
                CurrentPlayerNumber = GetPlayerByID(id).PlayerNumber;
                // Emit the turn change signal
                EmitSignal("TurnChangeSignal", CurrentPlayerNumber, Players[CurrentPlayerNumber - 1].TotalTurnsTaken, MatchTotalTurns);
            }
        }

        // Check if it's a player's turn
        public bool IsMyTurn(int playerID)
        {
            return CurrentPlayerNumber == playerID;
        }

        public bool IsLocalPlayerTurn()
        {
            return CurrentPlayerNumber == GetPlayerByID(StartupScript.Startup.Multiplayer.PlayerId).PlayerNumber;
        }

        // Skip the unit's turn
        public void SkipUnitTurn()
        {
            Players[CurrentPlayerNumber - 1].SkipPlayerTurn();
        }

        // Get the total mana of the current player
        public int GetCurrentPlayerTotalMana()
        {
            return Players[CurrentPlayerNumber - 1].TotalMana;
        }

        // Get the next player
        public Player GetNextPlayer()
        {
            return Players[CurrentPlayerNumber - 1];
        }

        // Get the data of the current unit
        public UnitData GetCurrentUnitData()
        {
            return Players[CurrentPlayerNumber - 1].CurrentUnit.data;
        }

        // Consume mana from the current player
        public void ConsumeMana(int mana)
        {
            Players[CurrentPlayerNumber - 1].ConsumeMana(mana);
        }

        // Get the current unit
        public Unit GetCurrentUnit()
        {
            return Players[CurrentPlayerNumber - 1].CurrentUnit;
        }

        // Get the current player
        public Player GetCurrentPlayer()
        {
            return Players[CurrentPlayerNumber - 1];
        }

        public Player GetPlayerByID(string id)
        {
            return Players.Find(x => x.OnlineID.Equals(id));
        }

        // Get the opponent player
        public Player GetOpponentPlayer()
        {
            if (CurrentPlayerNumber == 1)
                return Players[1];
            else
                return Players[0];
        }

        // public Player GetOpponentPlayer(string id)
        // {
        //     if (CurrentPlayerNumber == 1)
        //         return Players[1];
        //     else
        //         return Players[0];
        // }
    }
}
