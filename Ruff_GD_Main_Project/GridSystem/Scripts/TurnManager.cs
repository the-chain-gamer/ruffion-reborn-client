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
        public void StartTurns()
        {
            IsGameOver = false;
            CurrentTurnNumber = 0;
            MatchTotalTurns = 0;
            //sending claim turn request for the fist time
            _ = StartupScript.Startup.Multiplayer.ClaimTurnRequest();
        }

        public void RequestChangeTurn()
        {
            if(IsLocalPlayerTurn())
            {
                _ = StartupScript.Startup.Multiplayer.SkipTurnRequest();
            }
        }

        // Change the turn to the next player
        public void ChangeTurn(Player player, int currentTurn, int totalTurns)
        {
            if(IsGameOver)
                return;
            if(totalTurns == 1)
                CurrentPlayerNumber = player.PlayerNumber;
            
            GridManager.GM.CM.ClearEverything();

            //Reset Grid Visuals and Highlights
            GridManager.GM.ResetGridVisuals();
            //Reset All Units
            Players[CurrentPlayerNumber - 1].OnTurnEnded();
            
            //check if it is my turn?
            CurrentPlayerNumber = player.PlayerNumber;
            MatchTotalTurns = totalTurns;

            if (MatchTotalTurns > 10 || AllUnitsDead())
            {
                // Game ended
                ProcessGameEndResult();
            }
            else
            {
                GridManager.GM.CanSelectTarget = false;
                // Emit the turn change signal
                EmitSignal("TurnChangeSignal", CurrentPlayerNumber, currentTurn, MatchTotalTurns);
            }
        }

        private void ProcessGameEndResult()
        {
            IsGameOver = true;
            int p1UnitsAlive = Players[0].MyUnits.FindAll(x => !x.IsDead).Count;
            int p2UnitsAlive = Players[1].MyUnits.FindAll(x => !x.IsDead).Count;
            int winner = -1;
            if (p1UnitsAlive > p2UnitsAlive)
            {
                // Player 1 won
                winner = Players[0].PlayerNumber;
            }
            else if (p2UnitsAlive > p1UnitsAlive)
            {
                // Player 2 won
                winner = Players[1].PlayerNumber;
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
                    winner = Players[0].PlayerNumber;
                }
                else if (totalP2Hp > totalP1Hp)
                {
                    winner = Players[1].PlayerNumber;
                }
                else
                {
                    GridManager.GM.UiController.GameEnded("It's a Draw...", false);
                    return;
                }
            }
            ShowWinMsg(winner);
        }

        private bool AllUnitsDead()
        {
            return Players[0].MyUnits.Count == 0 || Players[1].MyUnits.Count == 0;
        }

        private void ShowWinMsg(int winner)
        {
            if(winner == GetPlayerByID(StartupScript.Startup.Multiplayer.PlayerId).PlayerNumber)
            {
                GridManager.GM.UiController.GameEnded("You Win!", false);
            }
            else
            {
                GridManager.GM.UiController.GameEnded("You Lose...", true);
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
    }
}
