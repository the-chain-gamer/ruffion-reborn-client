using Godot;
using System;
using Godot.Collections;
using RuffGdMainProject.DataClasses;
using RuffGdMainProject.UiSystem;

public class MainSceneHandler : CanvasLayer
{
    // int[] TeamIds;

    public static MainSceneHandler ins;

    public void Init()
    {
        ins = this;
        HideALL();
        GetNode("MainPanel").GetNode<Control>("MainPanelControlNode").Show();
    }
    public void HideALL()
    {
        GetNode("MainPanel").GetNode<Control>("MainPanelControlNode").Hide();
        GetNode("DeckPanel").GetNode<Control>("DeckControlNode").Hide();
        GetNode("PlayerSelection").GetNode<Control>("PlayerSelectionControlNode").Hide();
        //GetNode("GamePlay").GetNode<Node2D>("GamePlayParent").Hide();
    }
    public void _on_MainPanel_PlayBtnPressed()
    {
        SoundManager.Instance.PlaySoundByName("ButtonSound");
        HideALL();
        GetNode("PlayerSelection").GetNode<Control>("PlayerSelectionControlNode").Show();
    }

    public void _on_DeckPanel_DeckBackPressed()
    {
        SoundManager.Instance.PlaySoundByName("ButtonSound");
        CardSelectionManager.instance.Save();
        HideALL();
        GetNode("MainPanel").GetNode<Control>("MainPanelControlNode").Show();
        GetNode("DeckPanel").GetNode<Control>("DeckControlNode").Hide();
    }

     public void _on_DoneButton_DeckBackPressed()
    {
        SoundManager.Instance.PlaySoundByName("ButtonSound");
        CardSelectionManager.instance.Save();
        HideALL();
        GetNode("MainPanel").GetNode<Control>("MainPanelControlNode").Show();
        GetNode("DeckPanel").GetNode<Control>("DeckControlNode").Hide();
    }

    public void _on_MainPanel_DeckBtnPressed()
    {
        SoundManager.Instance.PlaySoundByName("ButtonSound");
        GD.Print("DECK PRESSED");
        HideALL();
        GetNode("DeckPanel").GetNode<Control>("DeckControlNode").Show();
    }

    public void _on_PlayerSelection_BackToMainFromPlayerSelection()
    {
        SoundManager.Instance.PlaySoundByName("ButtonSound");
        HideALL();
        GetNode("MainPanel").GetNode<Control>("MainPanelControlNode").Show();
    }

    public void _on_PlayerSelection_StartBattleBtnPressed()
    {
        SoundManager.Instance.PlaySoundByName("ButtonSound");
        HideALL();
        GetNode("GamePlay").GetNode<Node2D>("GamePlayParent").Hide();
        //$GamePlay/GameplayManager.SpawnPlayers()
    }
}
