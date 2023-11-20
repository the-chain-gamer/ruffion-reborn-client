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

    public TextureButton MuteButton;

    public void _on_QuitBtn_pressed()
    {
        GetTree().Quit();
    }


    public void _on_DeckBtn_pressed()
    {
        this.EmitSignal("DeckBtnPressed");
        MainSceneHandler.ins._on_MainPanel_DeckBtnPressed();
    }



    public void _on_PlayBtn_pressed()
    {
        this.EmitSignal("PlayBtnPressed");
        MainSceneHandler.ins._on_MainPanel_PlayBtnPressed();
    }

    public void _on_DeckBack_pressed()
    {
        this.EmitSignal("DeckBackPressed");
        MainSceneHandler.ins._on_DeckPanel_DeckBackPressed();
    }

    public void _on_DoneButton_pressed()
    {
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

    public void AllCards()
    {
        GD.Print("All");

        for (int i = 0; i < CardSelectionManager.instance.CardsList.Count; i++)
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

    public void _on_soundButton_pressed()
    {
        var slider  = GetNode<Control>("MainPanelControlNode").GetNode<Control>("SoundControl").GetNode<Control>("SliderParent");
        if (slider.Visible == true)
        {
            slider.Hide();
        }
        else
        {
            slider.Show();
        }
    }
    public void _on_HSlider_value_changed(float val)
    {
        SoundManager.Instance.SetSoundVolume(val);
    }

    public void _on_MuteButton_pressed()
    {
        var slider = GetNode<Control>("MainPanelControlNode").GetNode<Control>("SoundControl").GetNode<Control>("SliderParent");
        slider.GetChild<Control>(1).Hide();
        slider.GetChild<Control>(2).Show();
        SoundManager.Instance.ToggleSound(false);
        SoundSettings.Instance.Save(0);
    }
    public void _on_UnMuteButton_pressed()
    {
        var slider = GetNode<Control>("MainPanelControlNode").GetNode<Control>("SoundControl").GetNode<Control>("SliderParent");
        slider.GetChild<Control>(1).Show();
        slider.GetChild<Control>(2).Hide();
        SoundManager.Instance.ToggleSound(true);
        SoundSettings.Instance.Save(1);
    }
}
