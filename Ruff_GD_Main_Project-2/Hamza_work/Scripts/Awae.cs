/*using Godot;
using System;
using System.Collections.Generic;


public class Awae: Control
{
    [Signal]
    public delegate void BackToMainFromPlayerSelection();

    int returnValue = -1;
    public int TeamLimit = 3;
    public List<int> TeamIds;

    [Export] public List<Character> CharactersList;
    // [Export] public Resource CharacterList;

    [Export] public PackedScene TeamItemUI;
    [Export] public PackedScene worldItem;



    public override void _Process(float delta)
    {
        if (TeamIds.Count >= 3)
        {
            //GetTree().Root.GetNode("PlayerSelection").GetNode("PlayerSelectionControlNode").GetNode<Control>("StartBattleBtn").Show();
            // GetTree().Root.GetNode("PlayerSelection").GetNode("PlayerSelectionControlNode").GetNode<TextureButton>("StartBattleBtn").Show();
        }

    }

    public override void _Ready()
    {
        TeamIds = new List<int>();
        //TeamIds.Add(10);
        //TeamIds.Add(11);
        //TeamIds.Add(12);

        CharactersList = new List<Character>();
        InitializeCharactersList();
        //  GetTree().Root.GetNode("PlayerSelection").GetNode("PlayerSelectionControlNode").GetNode<TextureButton>("StartBattleBtn").Hide();
        //TeamItemUI = (PackedScene)GD.Load("res://Main_Rafay/Scenes/TeamItem.tscn");
        //Control TeamItem = (Control)TeamItemUI.Instance();
        //GD.Print("script name is" + TeamItem.GetType());
        GD.Print("List count is" + CharactersList.Count);
        instanceCharacterNode();
    }

    public void InitializeCharactersList()
    {
        CharactersList.Add(new Character(1,"Beagle", "res://Main_Rafay/Art/Characters/beagle_new/Beagle_profile.png", "res://Main_Rafay/Art/Characters/beagle_new/beagle2.png"));
        CharactersList.Add(new Character(2, "Corgi", "res://Main_Rafay/Art/Characters/corgi_new/Corgi_profile.png", "res://Main_Rafay/Art/Characters/corgi_new/corgis2.png"));
        CharactersList.Add(new Character(3, "Golden Retriever", "res://Main_Rafay/Art/Characters/goldenR_new/GoldenRetriever_profile.png", "res://Main_Rafay/Art/Characters/goldenR_new/goldenretriver3.png"));
        CharactersList.Add(new Character(4, "Labrador", "res://Main_Rafay/Art/Characters/labrador_new/Labrador_profile.png", "res://Main_Rafay/Art/Characters/labrador_new/labrador4.png"));

        CharactersList.Add(new Character(5, "Poodle", "res://Main_Rafay/Art/Characters/poodle_new/poodle_profile.png", "res://Main_Rafay/Art/Characters/poodle_new/poodle3.png"));
        CharactersList.Add(new Character(6, "Shiba", "res://Main_Rafay/Art/Characters/shiba_new/shiba_profile.png", "res://Main_Rafay/Art/Characters/shiba_new/shiba5.png"));
      

    }

    public void instanceCharacterNode()
    {
        //worldItem = (PackedScene)GD.Load("res://Hamza_work/My_Scenes/CharacterUIElement.tscn");
        //Control worldItemChild = (Control)worldItem.Instance();
        //GD.Print("Path is" + GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").Name);
        //GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").AddChild(worldItemChild);
        // GetTree().Root.GetNode("BaseCanvasLayer").GetNode("PlayerSelection").GetChild(3).AddChild(worldItemChild);//.GetNode<Control>("PlayerSelectionControlNode").GetNode<ScrollContainer>("allCharacterScroll").GetChild(0).AddChild(worldItemChild);

        for (int n = 0; n < CharactersList.Count; n++)
        {

            // worldItem = (PackedScene)GD.Load("res://Hamza_work/My_Scenes/CharacterUIElement.tscn");
            // TeamItemUI worldItemChild = (TeamItemUI)worldItem.Instance();
            // GD.Print("Path is" + GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").Name);
            //// TeamItemUI sc = (TeamItemUI)TeamItemUI.Instance();
            // worldItemChild.SetValue(CharactersList[n].characterName, CharactersList[n].characterSprite);
            // GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").AddChild(worldItemChild);


            TeamItemUI = (PackedScene)GD.Load("res://Main_Rafay/Scenes/TeamItem.tscn");
            TeamItemUI TeamItem = (TeamItemUI)TeamItemUI.Instance();
            TeamItem.Connect("pressed", this, "AddToTheTeam", new Godot.Collections.Array { CharactersList[n].characterID });
           // TeamItem.Connect("mouse_entered", this, "CheckHoverFunction");
            TeamItem.SetValue(CharactersList[n].characterName, CharactersList[n].characterSprite);
            GD.Print("Path is" + GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").Name);
          //  GD.Print("Button path is"+ GetTree().Root.GetNode("PlayerSelection").GetNode("PlayerSelectionControlNode").GetNode<Control>("StartBattleBtn").Name);
            GetNode<ScrollContainer>("allCharacterScroll").GetNode<Control>("allCharacter").AddChild(TeamItem);
         
        }
    }


    //private int checkIdExistance(int id)
    //{
    //    for (int i = 0; i < CharactersList.Count; i++)
    //    {
    //        if(id == CharactersList[i].characterID)
    //        {
    //            RemoveItemFromGrid();
    //            returnValue = 1;
    //        }
    //        else 
    //        {
    //            returnValue = 0;
    //            for (int j = 0; j < TeamIds.Count; j++)
    //            {
    //                if (id == TeamIds[j])
    //                {
    //                    RemoveItemFromGrid();
    //                    returnValue = 1;
    //                }
    //                else
    //                {
    //                    returnValue = 0;
    //                }
    //            }
    //        }
    //    }
    //    GD.Print("returnValue is"+returnValue);
    //    return returnValue;
     
    //}

    private int CheckId(int id)
    {
        returnValue = -1;

        var item = TeamIds.Find(x => x.Equals(id));

        if (item == null)
            return 0;
        else
            return item;

        //for (int j = 0; j < TeamIds.Count; j++)
        //{
        //    GD.Print("Comes in loop" + TeamIds[j]);
        //    if (id.Equals(TeamIds[j]))
        //    {
        //        returnValue = 1;
        //    }
        //    else
        //    {
        //        returnValue = 0;
        //    }
        //}
        //GD.Print("returnValue " + returnValue);
        //return returnValue;
    }

    public void AddToTheTeam(int id)
    {
        GD.Print("characte id "+ id);
        if (TeamIds.Count == 0)
        {
            TeamIds.Add(id);
            for (int j = 0; j < TeamIds.Count; j++)
            {
                GD.Print("Loop items are:" + TeamIds[j]);
            }
            GlobalData gd = new GlobalData();
            gd.addItem(id);
            PackedScene TeamGridItem = (PackedScene)GD.Load("res://Usman_Work/Scenes/TeamGridItem.tscn");
            TeamGridItemUI TeamItem1 = (TeamGridItemUI)TeamGridItem.Instance();
            TeamItem1.SetValue(CharactersList[id - 1].characterName, CharactersList[id - 1].characterSprite);
            GetNode<GridContainer>("TeamCharacter").AddChild(TeamItem1);
            GD.Print("Node we get"+ TeamItem1.GetNode("TextureButton").Name);
           
            
        }
        else
        {
            int chareacterId = CheckId(id);
            if (chareacterId == 0)
            {
                if (TeamIds.Count < 3)
                {
                    TeamIds.Add(id);
                    for (int j = 0; j < TeamIds.Count; j++)
                    {
                        GD.Print("Loop items are:"+ TeamIds[j]);
                    }
                    GlobalData gd = new GlobalData();
                    gd.addItem(id);
                    PackedScene TeamGridItem = (PackedScene)GD.Load("res://Usman_Work/Scenes/TeamGridItem.tscn");
                    TeamGridItemUI TeamItem1 = (TeamGridItemUI)TeamGridItem.Instance();
                    TeamItem1.SetValue(CharactersList[id - 1].characterName, CharactersList[id - 1].characterSprite);
                    GetNode<GridContainer>("TeamCharacter").AddChild(TeamItem1);
                }
            }
        }

    }

    //public void RemoveItemFromGrid()
    //{
    //    if (GetNode<GridContainer>("TeamCharacter").GetChildCount() > 0)
    //    {
    //        GetNode<GridContainer>("TeamCharacter").QueueFree();
    //    }
       
    //}

    public void CheckHoverFunction()
    {
        //GD.Print("Hower Event Called");
    }

    public void RemoveCharacterFromGrid()
    {
       
    }

}
*/