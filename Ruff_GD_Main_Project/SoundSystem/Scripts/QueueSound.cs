using Godot;
using System;

public class QueueSound : Node2D
{
    AudioStreamPlayer audioClip;
    public override void _Ready()
    {
        audioClip = this.GetChild<AudioStreamPlayer>(0);
    }

   public void PlaySound()
    {
        if (!audioClip.Playing)
        {
            audioClip.Play();
        }
    }
}
