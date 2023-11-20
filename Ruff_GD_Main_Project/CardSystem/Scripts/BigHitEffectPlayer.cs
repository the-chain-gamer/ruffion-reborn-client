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
