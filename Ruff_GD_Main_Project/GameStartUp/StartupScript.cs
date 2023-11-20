using Godot;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.GridSystem;
using RuffGdMainProject.UiSystem;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.GameScene
{
    public class StartupScript : Node2D
    {
        // Singleton instance
        public static StartupScript Startup;
        public MultiplayerManager Multiplayer;
        // Cards Data DB
        public CardsDB DB;
        public UserDataModel PlayerData;
        // Scenes and Managers
        private PackedScene MainMenuScene;
        private MainSceneHandler main;
        private GridManager game;
        private SoundManager soundManager;

        public override void _Ready()
        {
            // Create PlayerData
            PlayerData = new UserDataModel("My Player");
            // Set the Singleton instance
            Startup = this;
            // Initialize CardsData DB
            DB = new CardsDB();
            Multiplayer = GetChild<MultiplayerManager>(0);
            // Load the MainMenuScene
            MainMenuScene = GD.Load<PackedScene>("res://Cards_UI/SCENES/MainMenuScene.tscn");
            main = (MainSceneHandler)MainMenuScene.Instance();
            AddChild(main);
            main.Init();
            // Load SoundManager
            LoadSoundManager();
        }

        // Start the game scene
        public void StartGame()
        {
            // Get card and unit data
            PlayerData.CardsDeck = DB.GetCardDataModel();
            PlayerData.MyUnits = GlobalData.GD.GetUnitDataModel();
            _ = Multiplayer.SearchForRooms(PlayerData);
        }

        public async void HideWithDelay(SubscriptionData data)
        {
            await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
            // Hide the MainMenuScene
            main.HideALL();
            // Load and initialize the GameScene
            var GameScene = GD.Load<PackedScene>("res://GridSystem/Scenes/Grid.tscn");
            game = (GridManager)GameScene.Instance();
            Startup.AddChild(game);
            game.Init();
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            game.SpawnPlayers(data.GameUpdates.Player1, data.GameUpdates.Player2);
        }

        public void LoadOpponentImgs(PlayerDataModel player1, PlayerDataModel player2)
        {
            var playerSelection = main.GetNode<CustomButtonHandler>("PlayerSelection").GetNode<CharacterSelectionManager>("PlayerSelectionControlNode");
            var lst = new List<UnitDataModel>();
            if(player1.ID.Equals(Multiplayer.PlayerId))
            {
                lst = player2.Assets.Dogs;
            }
            else if(player2.ID.Equals(Multiplayer.PlayerId))
            {
                lst = player1.Assets.Dogs;
            }
            foreach (var item in lst)
            {
                playerSelection.ShowOpponentCharacters(Convert.ToInt32(item.DogId), item.DogName);
            }
            CharacterSelectionManager.instance.PlayersJoined();
        }

        public async void HideWithDelay(GameResponse data)
        {
            await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
            // Hide the MainMenuScene
            main.HideALL();
            // Load and initialize the GameScene
            var GameScene = GD.Load<PackedScene>("res://GridSystem/Scenes/Grid.tscn");
            game = (GridManager)GameScene.Instance();
            Startup.AddChild(game);
            game.Init();
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            game.SpawnPlayers(data.Game.Player1, data.Game.Player2);
        }

        public void LoadSoundManager()
        {
            // Load and add the SoundManager
            var SoundManagerScene = GD.Load<PackedScene>("res://SoundSystem/Scenes/SoundManager.tscn");
            soundManager = (SoundManager)SoundManagerScene.Instance();
            AddChild(soundManager);
        }

        public async void ReturnToMenu()
        {
            if(!GridManager.GM.TM.IsGameOver)
            {
                GridManager.GM.UiController.GameEnded("You Won!", false);
            }
            await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
            GridManager.GM.GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }
    }
}
