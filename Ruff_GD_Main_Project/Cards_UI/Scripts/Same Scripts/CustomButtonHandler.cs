using Godot;
using System;
using RuffGdMainProject.UiSystem;

public class CustomButtonHandler : CanvasLayer
{
    [Signal]
    public delegate void DeckBtnPressed();
    [Signal]
    public delegate void PlayBtnPressed();
    [Signal]
    public delegate void DeckBackPressed();
    [Signal]
    public delegate void BackToMainFromPlayerSelection();
    [Signal]
    public delegate void StartBattleBtnPressed();
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public void _on_QuitBtn_pressed()
    {
        GetTree().Quit();
    }


    public void _on_DeckBtn_pressed()
    {
        this.EmitSignal("DeckBtnPressed");
        GD.Print("DECKKKKKK");
        MainSceneHandler.ins._on_MainPanel_DeckBtnPressed();
    }



    public void _on_PlayBtn_pressed()
    {
        //GD.Print("play Pressed");
        this.EmitSignal("PlayBtnPressed");
        MainSceneHandler.ins._on_MainPanel_PlayBtnPressed();

    }

    public void _on_DeckBack_pressed()
    {
        //GD.Print("deck Pressed");
        this.EmitSignal("DeckBackPressed");

        MainSceneHandler.ins._on_DeckPanel_DeckBackPressed();
    }

     public void _on_DoneButton_pressed()
    {
        //GD.Print("deck Pressed");
        this.EmitSignal("DeckBackPressed");

        MainSceneHandler.ins._on_DoneButton_DeckBackPressed();
    }

    public void _BackToMainFromPlayerSelection()
    {
        this.EmitSignal("BackToMainFromPlayerSelection");
        MainSceneHandler.ins._on_PlayerSelection_BackToMainFromPlayerSelection();

    }

    public void _on_StartBattleBtn_pressed()
    {
        this.EmitSignal("StartBattleBtnPressed");
    }

    public void _on_Button_pressed()
    {
        //Globals.camera.shake(25);
        //Transitioner._in();
    }

    public void _on_Button2_pressed()
    {
        //print("iiinnnnnn func");
        //Globals.camera.shake(100);
    }

    public void _on_Button3_pressed()
    {
        //Globals.camera.shake(300, 2, 300);
    }


    public void AllCards()
    {
        GD.Print("All");

        for(int i = 0; i< CardSelectionManager.instance.CardsList.Count; i++)
        {
            CardSelectionManager.instance.CardsList[i].Show();
        }
    }
    public void _on_Smarts_pressed()
    {
        HideAllCards();

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            if (CardSelectionManager.instance.CardsList[i].CardCategory == CardSubType.SMARTS)
            {
                CardSelectionManager.instance.CardsList[i].Show();
            }
        }
    }

    public void _on_Science_pressed()
    {
        HideAllCards();

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            if (CardSelectionManager.instance.CardsList[i].CardCategory == CardSubType.SCIENCE)
            {
                CardSelectionManager.instance.CardsList[i].Show();
            }
        }
    }

    public void _on_Sorcery_pressed()
    {
        HideAllCards();

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            if (CardSelectionManager.instance.CardsList[i].CardCategory == CardSubType.SORCERY)
            {
                CardSelectionManager.instance.CardsList[i].Show();
            }
        }
    }

    public void _on_Speed_pressed()
    {
        HideAllCards();

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            if (CardSelectionManager.instance.CardsList[i].CardCategory == CardSubType.SPEED)
            {
                CardSelectionManager.instance.CardsList[i].Show();
            }
        }
    }

    public void _on_Defend_pressed()
    {
        HideAllCards();

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            if (CardSelectionManager.instance.CardsList[i].CardCategory == CardSubType.DEFEND)
            {
                CardSelectionManager.instance.CardsList[i].Show();
            }
        }
    }

    public void _on_Strength_pressed()
    {
        HideAllCards();

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            if (CardSelectionManager.instance.CardsList[i].CardCategory == CardSubType.STRENGTH)
            {
                CardSelectionManager.instance.CardsList[i].Show();
            }
        }
    }

    public void HideAllCards()
    {
        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
        {
            CardSelectionManager.instance.CardsList[i].Hide();
        }
    }
}
