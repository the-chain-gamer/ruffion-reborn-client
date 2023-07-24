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
            MainMenuScene = GD.Load<PackedScene>("res://Hamza_work/SCENES/MainMenuScene.tscn");
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
            // Hide the MainMenuScene with delay
            // HideWithDelay();
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
            game.SpawnPlayers(data.GameUpdates.Player1, data.GameUpdates.Player2, data.GameUpdates.CurrentPlayer);
        }

        public void LoadOppnantImgs(PlayerDataModel player1, PlayerDataModel player2)
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
                GD.Print("Other Player Dog Id = " + item.DogId);
                playerSelection.ShowOpponantCharacters(Convert.ToInt32(item.DogId), item.DogName);
            }
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
            game.SpawnPlayers(data.Game.Player1, data.Game.Player2, data.Game.CurrentPlayer);
        }

        public void LoadSoundManager()
        {
            // Load and add the SoundManager
            var SoundManagerScene = GD.Load<PackedScene>("res://SoundSystem/Scenes/SoundManager.tscn");
            soundManager = (SoundManager)SoundManagerScene.Instance();
            AddChild(soundManager);
        }
    }
}
