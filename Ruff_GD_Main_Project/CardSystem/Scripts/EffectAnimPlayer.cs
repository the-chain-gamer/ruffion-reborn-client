using Godot;
using RuffGdMainProject.GridSystem;
using System;
using System.Diagnostics;

public class EffectAnimPlayer : Node2D
{
    [Export]
    public Vector2 Velocity = new Vector2(0f, 0f);
    [Export]
    public float Speed = 400f;
    [Export]
    public float CuttofDistance = 150f;
    public Vector2 TargetPosition;
    protected bool canMove = false;
    protected Vector2 defaultPos;

    protected AnimatedSprite start, loop, stop;
    protected Unit unit, enemy;

    public override void _Ready()
    {
        defaultPos = GlobalPosition;
        start = GetNode<AnimatedSprite>("Start");
        loop = GetNode<AnimatedSprite>("Loop");
        stop = GetNode<AnimatedSprite>("Stop");
    }

    public async void PlayHitAnim(Unit myUnt, Unit target)
    {
        unit = myUnt;
        enemy = target;
        // TargetPosition = target.GetNode<Node2D>("Test").GlobalPosition;
        TargetPosition = target.Cell.GlobalPosition;

        // TargetPosition.y += 10f;
        
        start.Frame = 0;
        start.Show();
        start.Playing = true;
        
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        
        if(loop != null)
        {
            start.Playing = false;
            start.Frame = 0;
            start.Hide();
            
            loop.Frame = 0;
            loop.Show();
            loop.Playing = true;
        }
        
        canMove = true;
    }

    public async virtual void PlayHitAnim()
    {
        if(loop != null)
        {
            loop.Hide();
            loop.Playing = false;
            loop.Frame = 0;
        }
        else
        {
            start.Playing = false;
            start.Frame = 0;
            start.Hide();
        }

        stop.Frame = 0;
        stop.Show();
        stop.Playing = true;

    }

    public async virtual void StopAnim()
    {
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
        // canMove = 
        stop.Playing = false;
        stop.Frame = 0;
        stop.Hide();
    }
    
}
