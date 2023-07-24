using Godot;
using System;

namespace RuffGdMainProject.GridSystem
{
    public class EffectPlayer : Node2D
    {
        public Vector2 Velocity = new Vector2(0f, 0f);
        public float Speed = 400f;
        public Vector2 TargetPosition;
        private bool canMove = false;
        private Vector2 defaultPos;

        public override void _Ready()
        {
            defaultPos = GlobalPosition;
            // GD.Print("%%%%% I'm ALive..... :)");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            if(canMove)
            {
                float angle = GetAngleTo(TargetPosition);

                Velocity.x = Mathf.Cos(angle);
                Velocity.y = Mathf.Sin(angle);

                GlobalPosition += Velocity * Speed * delta;

                if(GlobalPosition.Equals(TargetPosition))
                {
                    canMove = false;
                }
            }
        }

        public void PlayAnimationOnce()
        {
            GetChild<AnimatedSprite>(0).Show();
            GetChild<AnimatedSprite>(0).Play("default", false);
            waitchk();
        }

        public void PlayAnimationOnce(Vector2 targetPos, bool noMovement =false)
        {
            if(noMovement)
            {
                GlobalPosition = new Vector2(200f, 200f);
            }
            else
            {
                // Position = new Vector2(0f, 0f);
                TargetPosition = targetPos;
                GetChild<AnimationPlayer>(1).Play("bigHitAnim");
                canMove = true;
            }
            GetChild<AnimationPlayer>(1).Play();
        }

        public async void waitchk()
        {
            await ToSignal(GetTree().CreateTimer(4.0f), "timeout");
            GetChild<AnimatedSprite>(0).Stop();
            GetChild<AnimatedSprite>(0).Frame = 0;

        }

        // public void PlayAnimationOnce(Unit attacker, Unit target, Vector2 targetPos, bool noMovement =false)
        // {
        //     if(noMovement)
        //     {
        //         GlobalPosition = new Vector2(200f, 200f);
        //     }
        //     else
        //     {
        //         // Position = new Vector2(0f, 0f);
        //         TargetPosition = targetPos;
        //         GetChild<AnimationPlayer>(1).Play("bigHitAnim");
        //         canMove = true;
        //     }
        //     GetChild<AnimationPlayer>(1).Play();
        // }

        public void BigHit()
        {
            // GridManager.GM.CM.ApplyBigHit();
        }
    }
}
