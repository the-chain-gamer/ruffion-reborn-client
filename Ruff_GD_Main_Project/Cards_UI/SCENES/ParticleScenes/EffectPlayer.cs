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

        private int targetFrame = 5; // The frame number at which you want to stop

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

        public async void PlayPinkyEffect()
        {
            var anim = GetNode<AnimatedSprite>("Start");
            GetNode<AnimatedSprite>("End").Hide();
            anim.Show();
            anim.Frame = 0;
            anim.Play();

            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            GetNode<AnimatedSprite>("End").Frame = 0;
            GetNode<AnimatedSprite>("End").Show();
            anim.Hide();
            anim.Stop();
        }

        public async void StopPinkyEffect()
        {
            var anim = GetNode<AnimatedSprite>("End");
            anim.Frame = 0;
            anim.Play();
            
            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            anim.Frame = 0;
            anim.Hide();
            anim.Stop();
        }

        public void PlayAnimationOnce(bool isCatten = false)
        {
            GetChild<AnimatedSprite>(0).Show();
            GetChild<AnimatedSprite>(0).Play("default", false);
            if(!isCatten)
            {
                waitchk();
            }
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

            // GetChild<AnimatedSprite>(0).Hide();
            // if(isCatten)
            // {
            //     GetChild<AnimatedSprite>(1).Stop();
            //     GetChild<AnimatedSprite>(1).Frame = 0;
            //     GetChild<AnimatedSprite>(1).Hide();
            // }
        }

        public void DsiableCattenEffect()
        {
            GetChild<AnimatedSprite>(0).Stop();
            GetChild<AnimatedSprite>(0).Frame = 0;
            GetChild<AnimatedSprite>(0).Hide();
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
