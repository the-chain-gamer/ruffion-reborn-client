using Godot;
using RuffGdMainProject.GridSystem;
using System;
using System.Diagnostics;

public class BigHitEffectPlayer : EffectAnimPlayer
{
    public override void PlayHitAnim()
    {
        base.PlayHitAnim();
        GridManager.GM.CM.ApplyBigHit(unit ,enemy);
        base.StopAnim();
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(float delta)
    // {
    //     if (canMove)
    //     {
    //         Vector2 direction = (TargetPosition - GlobalPosition).Normalized();
    //         Velocity = direction * Speed;

    //         GlobalPosition += Velocity * delta;

    //         float distanceToTargetX = GlobalPosition.DistanceTo(TargetPosition);
    //         // float distanceToTargetX = Math.Abs(TargetPosition.x - GlobalPosition.x);
    //         GD.Print("distanceToTarget = " + distanceToTargetX);
    //         if (distanceToTargetX <= CuttofDistance)  // Adjust epsilon value as needed
    //         {
    //             Velocity = Vector2.Zero;
    //             GD.Print("Ending");
    //             canMove = false;
    //             PlayHitAnim();
    //         }
    //     }
    // }

    // public override void _Process(float delta)
    // {
    //     if (canMove)
    //     {
    //         Vector2 direction = (TargetPosition - GlobalPosition).Normalized();
    //         Velocity = direction * Speed;

    //         GlobalPosition += Velocity * delta;

    //         float distanceToTargetX = Math.Abs(TargetPosition.x - GlobalPosition.x);
    //         float distanceToTargetY = Math.Abs(TargetPosition.y - GlobalPosition.y);
    //         GD.Print("distanceToTargetX = " + distanceToTargetX + ", distanceToTargetY = " + distanceToTargetY);
            
    //         if (distanceToTargetX <= CuttofDistance && distanceToTargetY <= CuttofDistance)
    //         {
    //             Velocity = Vector2.Zero;
    //             GD.Print("Ending");
    //             canMove = false;
    //             PlayHitAnim();
    //         }
    //     }
    // }

    public override void _Process(float delta)
    {
        if (canMove)
        {
            Vector2 direction = (TargetPosition - GlobalPosition).Normalized();
            Velocity = direction * Speed;

            GlobalPosition += Velocity * delta;

            float sqrDistanceToTarget = (TargetPosition - GlobalPosition).LengthSquared();
            GD.Print("Squared Distance To Target = " + sqrDistanceToTarget);
            
            float cutoffDistanceSquared = CuttofDistance * CuttofDistance;
            
            if (sqrDistanceToTarget <= cutoffDistanceSquared)
            {
                Velocity = Vector2.Zero;
                GD.Print("Ending");
                canMove = false;
                PlayHitAnim();
            }
        }
    }
}
