using Godot;
using System.Collections.Generic;

public class SoundManager : Node2D
{
    // Singleton instance
    public static SoundManager Instance;

    // Dictionary to store sounds
    private Dictionary<string, QueueSound> soundsDictionary = new Dictionary<string, QueueSound>();

    public override void _Ready()
    {
        // Set the singleton instance
        Instance = this;

        // Start playing the first audio stream player
        this.GetChild<AudioStreamPlayer>(0).Playing = true;

        // Initialize the sounds dictionary
        InitializeSounds();
    }

    // Play a sound by its name
    public void PlaySoundByName(string soundName)
    {
        GD.Print("SoundName: " + soundName);
        soundsDictionary[soundName].PlaySound();
    }

    // Initialize the sounds dictionary with sound nodes
    private void InitializeSounds()
    {
        // Add sound nodes to the dictionary
        soundsDictionary.Add("TurnChangeSound", GetNode<QueueSound>("TurnChangeSound"));
        soundsDictionary.Add("VsSound", GetNode<QueueSound>("VsSound"));
        soundsDictionary.Add("TeleportSimpleSound", GetNode<QueueSound>("TeleportSimpleSound"));
        soundsDictionary.Add("TeleportCardSound", GetNode<QueueSound>("TeleportCardSound")); // Cyril's Hat Trick
        soundsDictionary.Add("ButtonSound", GetNode<QueueSound>("ButtonSound"));
        soundsDictionary.Add("ButtonSound2", GetNode<QueueSound>("ButtonSound2"));
        soundsDictionary.Add("AttackSound", GetNode<QueueSound>("AttackSound"));
        soundsDictionary.Add("LalitoLaserSound", GetNode<QueueSound>("LalitoLaserSound"));
        soundsDictionary.Add("ExplosionSound", GetNode<QueueSound>("ExplosionSound")); // Marlon's Smol Smoke Bomb
        soundsDictionary.Add("ShieldSound", GetNode<QueueSound>("ShieldSound")); // Kenzo's Smol Shield
        soundsDictionary.Add("TelescopeSound", GetNode<QueueSound>("TelescopeSound")); // Wiz's Smol Telescope
        soundsDictionary.Add("BubbleSound", GetNode<QueueSound>("BubbleSound")); // Bubbles Pick n Drop / Bethany's Road block
        soundsDictionary.Add("TrapSound", GetNode<QueueSound>("TrapSound")); // Pinky's Smol Brain
        soundsDictionary.Add("DestructorSound", GetNode<QueueSound>("DestructorSound")); // Hook's Destructor
        soundsDictionary.Add("GambleSound", GetNode<QueueSound>("GambleSound")); // Penelope's Gamble
        soundsDictionary.Add("HitSound", GetNode<QueueSound>("HitSound")); // Smolstein's Big Hit
        soundsDictionary.Add("SmokeScreenSound", GetNode<QueueSound>("SmokeScreenSound")); // Smoke Screen Sound
        soundsDictionary.Add("SaundersSound", GetNode<QueueSound>("SaundersSound")); // Colonel Saunders Sound
        soundsDictionary.Add("CattenSound", GetNode<QueueSound>("CattenSound")); // Catten Sound
    }
}
