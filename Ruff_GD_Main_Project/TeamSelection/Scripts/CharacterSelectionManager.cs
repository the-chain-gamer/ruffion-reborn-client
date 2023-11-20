using Godot;
using RuffGdMainProject.GameScene;
using RuffGdMainProject.DataClasses;
using System;
using System.Collections.Generic;

namespace RuffGdMainProject.UiSystem
{
    public class CharacterSelectionManager : Control
    {
        public static CharacterSelectionManager instance;

        [Signal]
        public delegate void BackToMainFromPlayerSelection();

        int returnValue = -1;
        public int TeamLimit = 3;
        public List<int> TeamIds;

        [Export] public List<Character> CharactersList;

        [Export] public PackedScene TeamItemUI;
        [Export] public PackedScene worldItem;
        [Export] public List<PackedScene> Breeds;

        private Control vsScreenParent;


        public override void _Process(float delta)
        {
            if (TeamIds.Count >= 3)
            {
                GetNode<Control>("ConfirmationButton").Show();
            }
            else
            {
                GetNode<Control>("ConfirmationButton").Hide();
            }
        }

        public override void _Ready()
        {
            instance = this;
            TeamIds = new List<int>();
            Breeds = new List<PackedScene>();
            CharactersList = new List<Character>();
            vsScreenParent = GetNode<Control>("VSScreen");
            InitializeCharactersList();
            InstanceCharacterNode();
        }

        public void InitializeCharactersList()
        {
            CharactersList.Add(new Character(1, "Beagle", "res://CommonAssets/CharacterSprites/Beagle/Beagle_profile.png",
                "res://CommonAssets/CharacterSprites/Beagle/beagle2.png"));
            CharactersList.Add(new Character(2, "Corgi", "res://CommonAssets/CharacterSprites/Corgi/Corgi_profile.png",
                "res://CommonAssets/CharacterSprites/Corgi/corgis2 .png"));
            CharactersList.Add(new Character(3, "Golden", "res://CommonAssets/CharacterSprites/GoldenR/GoldenRetriever_profile.png",
                "res://CommonAssets/CharacterSprites/GoldenR/golden retriver3 .png"));
            CharactersList.Add(new Character(4, "Labrador", "res://CommonAssets/CharacterSprites/Labrador/Labrador_profile.png",
                "res://CommonAssets/CharacterSprites/Labrador/labrador4.png"));
            CharactersList.Add(new Character(5, "Poodle", "res://CommonAssets/CharacterSprites/Poodle/poodle_profile.png",
                "res://CommonAssets/CharacterSprites/Poodle/poodle3.png"));
            CharactersList.Add(new Character(6, "Shiba", "res://CommonAssets/CharacterSprites/Shiba/shiba_profile.png",
                "res://CommonAssets/CharacterSprites/Shiba/shiba idle.png"));
            CharactersList.Add(new Character(7, "Hound", "res://CommonAssets/CharacterSprites/Hound/Hound_profile.png",
                "res://CommonAssets/CharacterSprites/Hound/hound3.png"));
            CharactersList.Add(new Character(8, "Malamute", "res://CommonAssets/CharacterSprites/Malamute/Malamute_profile.png",
                "res://CommonAssets/CharacterSprites/Malamute/malamute3.png"));
            CharactersList.Add(new Character(9, "Mastiff", "res://CommonAssets/CharacterSprites/Mastiff/mastiff-profile-1.png",
                "res://CommonAssets/CharacterSprites/Mastiff/mastiff-1.png"));
        }

        public void InstanceCharacterNode()
        {
            for (int n = 0; n < CharactersList.Count; n++)
            {
                TeamItemUI = (PackedScene)GD.Load("res://TeamSelection/Scenes/TeamItem.tscn");
                TeamItemUI TeamItem = (TeamItemUI)TeamItemUI.Instance();
                TeamItem.Connect("pressed", this, "AddToTheTeam", new Godot.Collections.Array { CharactersList[n].characterID });
                TeamItem.SetValue(CharactersList[n].characterName, CharactersList[n].characterSprite);
                TeamItem.setCharacterId(CharactersList[n].characterID);
                GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").AddChild(TeamItem);

            }
        }

        private int CheckId(int id)
        {
            var item = TeamIds.Find(x => x.Equals(id));
            return item == 0 ? 0 : item;
        }

        public void AddToTheTeam(int id)
        {
            if (TeamIds.Count == 0)
            {
                SoundManager.Instance.PlaySoundByName("ButtonSound2");
                TeamIds.Add(id);

                GlobalData gd = new GlobalData();
                gd.addItem(id);
                PackedScene TeamGridItem = (PackedScene)GD.Load("res://TeamSelection/Scenes/TeamGridItem.tscn");
                TeamGridItemUI TeamItem1 = (TeamGridItemUI)TeamGridItem.Instance();
                TeamItem1.SetValue(CharactersList[id - 1].characterName, CharactersList[id - 1].characterSprite);
                TeamItem1.setCharacterId(CharactersList[id - 1].characterID);
                TeamItem1.SetCharcterStats(CharactersList[id - 1].characterID, CharactersList[id - 1].characterName);
                GetNode<GridContainer>("TeamCharacter").AddChild(TeamItem1);
            }
            else
            {
                int chareacterId = CheckId(id);
                if (chareacterId == 0)
                {
                    if (TeamIds.Count < 3)
                    {
                        SoundManager.Instance.PlaySoundByName("ButtonSound2");
                        TeamIds.Add(id);

                        GlobalData gd = new GlobalData();
                        gd.addItem(id);
                        PackedScene TeamGridItem = (PackedScene)GD.Load("res://TeamSelection/Scenes/TeamGridItem.tscn");
                        TeamGridItemUI TeamItem1 = (TeamGridItemUI)TeamGridItem.Instance();
                        TeamItem1.SetValue(CharactersList[id - 1].characterName, CharactersList[id - 1].characterSprite);
                        TeamItem1.setCharacterId(CharactersList[id - 1].characterID);
                        TeamItem1.SetCharcterStats(CharactersList[id - 1].characterID, CharactersList[id - 1].characterName);
                        GetNode<GridContainer>("TeamCharacter").AddChild(TeamItem1);
                    }
                }
            }
            CharacterSelectedImages(TeamIds.Count, true);
        }

        public void RemoveCharacterFromGrid(int id)
        {
            for (int i = 0; i < TeamIds.Count; i++)
            {
                if (id == TeamIds[i])
                {
                    SoundManager.Instance.PlaySoundByName("ButtonSound2");
                    TeamIds.Remove(id);
                }
            }

            CharacterSelectedImages(TeamIds.Count, false);
        }

        private void CharacterSelectedImages(int teamCount, bool added)
        {
            if (added)
            {
                for (int i = 0; i < teamCount; i++)
                {
                    GetNode<Control>("ButtonsParent").GetChild<TextureRect>(i).Hide();
                }
            }
            else
            {
                var btnsParent = GetNode<Control>("ButtonsParent");
                if (teamCount == 0)
                {
                    btnsParent.GetChild<TextureRect>(0).Show();
                    btnsParent.GetChild<TextureRect>(1).Show();
                    btnsParent.GetChild<TextureRect>(2).Show();
                }
                else if (teamCount == 1)
                {
                    btnsParent.GetChild<TextureRect>(1).Show();
                    btnsParent.GetChild<TextureRect>(2).Show();
                }
                else if (teamCount == 2)
                {
                    btnsParent.GetChild<TextureRect>(2).Show();
                }
            }
        }

        public void _on_ConfirmationButton_pressed()
        {
            SoundManager.Instance.PlaySoundByName("ButtonSound");
            
            List<int> storedCharacterIds = StoreSelectedCharacterIDS();

            vsScreenParent.Show();
            var vsScreen = vsScreenParent.GetNode("AnimationPlayer") as AnimationPlayer;
            vsScreen.Play("PlayerSelectionPanelAnimNEW");
            ShowChosenCharacters(storedCharacterIds);
            StartupScript.Startup.StartGame();
        }

        private List<int> StoreSelectedCharacterIDS()
        {
            GlobalData gd = new GlobalData();
            for (int i = 0; i < GetNode<GridContainer>("TeamCharacter").GetChildCount(); i++)
            {
                TeamGridItemUI teamItem = (TeamGridItemUI)GetNode<GridContainer>("TeamCharacter").GetChild<Control>(i);
                gd.addItem(teamItem.characterId);
            }
            return gd.myNum;
        }

        private void ShowChosenCharacters(List<int> storedCharacterIds)
        {
            for (int n = 0; n < CharactersList.Count; n++)
            {
                for (int i = 0; i < storedCharacterIds.Count; i++)
                {
                    if (CharactersList[n].characterID == storedCharacterIds[i])
                    {
                        TeamItemUI = (PackedScene)GD.Load("res://TeamSelection/Scenes/PlayerTeamItem.tscn");
                        TeamItemUI TeamItem = (TeamItemUI)TeamItemUI.Instance();
                        TeamItem.SetValue(CharactersList[n].characterName, CharactersList[n].characterSprite);
                        TeamItem.setCharacterId(CharactersList[n].characterID);
                        vsScreenParent.GetNode<Control>("VersusScreenBg").GetNode<Control>("VersusBG").GetNode<GridContainer>("Player2Grid").AddChild(TeamItem);
                    }
                }
            }
        }

        public void ShowOpponentCharacters(int id, string nm)
        {
            vsScreenParent.GetNode<Control>("VersusScreenBg").GetNode<Control>("VersusBG").GetNode<Control>("Player1Parent").Hide();
            TeamItemUI = (PackedScene)GD.Load("res://TeamSelection/Scenes/PlayerTeamItem.tscn");
            TeamItemUI TeamItem = (TeamItemUI)TeamItemUI.Instance();
            TeamItem.SetValue(nm, ReturnTeamItemProfilePic(id));
            TeamItem.setCharacterId(id);
            vsScreenParent.GetNode<Control>("VersusScreenBg").GetNode<Control>("VersusBG").GetNode<GridContainer>("Player1Grid").AddChild(TeamItem);
        }

        public Texture ReturnTeamItemProfilePic(int ID)
        {
            var character = CharactersList.Find(x=>x.characterID == ID);
            TeamItemUI = (PackedScene)GD.Load("res://TeamSelection/Scenes/PlayerTeamItem.tscn");
            TeamItemUI TeamItem = (TeamItemUI)TeamItemUI.Instance();
            TeamItem.SetValue(character.characterName, character.characterSprite);
            TeamItem.setCharacterId(character.characterID);
            var TeamCharSprite = TeamItem.GetNode("Frame").GetChild<TextureRect>(0).Texture;
            return TeamCharSprite;
        }


        public async void PlayersJoined()
        {
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout"); 
            vsScreenParent.GetNode<Control>("VersusScreenBg").GetNode<Control>("Select Party2").Hide();
        }
    }
}