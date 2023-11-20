using Godot;
using System;

public class SoundSettings : Control
{
    public static SoundSettings Instance;

    private ConfigFile CF = new ConfigFile();
    [Export]
    public string CFName = "MuteSaveIDS.cfg";
    public override void _Ready()
    {
        Instance = this;

        if (CF.Load("user://" + CFName) != Error.Ok)
        {
            Save(1);
        }
        else
        {
            LoadMuteSettings();
        }
    }

     int x;
    public async void LoadMuteSettings()
    {
        await ToSignal(GetTree().CreateTimer(0.0f), "timeout");
        int MuteValue = (int)CF.GetValue("MuteSettings", "Mute", x);
        var slider = GetNode<Control>("SliderParent");
        if (MuteValue == 0)
        {
            slider.GetChild<Control>(1).Hide();
            slider.GetChild<Control>(2).Show();
            SoundManager.Instance.ToggleSound(false);
        }
        else if (MuteValue == 1)
        {
            slider.GetChild<Control>(1).Show();
            slider.GetChild<Control>(2).Hide();
            SoundManager.Instance.ToggleSound(true);
        }
    }

    public void Save(int val)
    {
        CF.SetValue("MuteSettings", "Mute", val);
        CF.Save("user://" + CFName);
    }
}
