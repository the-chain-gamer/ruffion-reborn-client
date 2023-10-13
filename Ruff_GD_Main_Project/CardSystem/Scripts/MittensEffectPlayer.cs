using Godot;
using RuffGdMainProject.GridSystem;
using System;
using System.Diagnostics;

public class MittensEffectPlayer : EffectAnimPlayer
{
    public override void PlayHitAnim()
    {
        base.PlayHitAnim();
        GridManager.GM.CM.ApplyMittens(unit ,enemy);
        base.StopAnim();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (canMove)
        {
            Vector2 direction = (TargetPosition - GlobalPosition).Normalized();
            Velocity = direction * Speed;

            GlobalPosition += Velocity * delta;

            float distanceToTargetX = GlobalPosition.DistanceTo(TargetPosition);
            // float distanceToTargetX = Math.Abs(TargetPosition.x - GlobalPosition.x);
            GD.Print("distanceToTarget = " + distanceToTargetX);
            if (distanceToTargetX <= CuttofDistance)  // Adjust epsilon value as needed
            {
                Velocity = Vector2.Zero;
                GD.Print("Ending");
                canMove = false;
                PlayHitAnim();
            }
        }
    }
}
