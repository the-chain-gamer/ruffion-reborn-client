using Godot;
using System;

namespace RuffGdMainProject.UiSystem
{
    public class FloatingText : Position2D
    {
        private Tween tween;
        private Vector2 velocity = new Vector2(50, -100);
        private Vector2 gravity = new Vector2(0, 1);
        private float mass = -200;
        private Label lblTxt;
        private bool ready = false;
        private Vector2 defaultPos;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Visible = false;
            ready = false;
            // StartEffect();
            defaultPos = Position;
        }
        public void Init(int dmg, TextColorType colorType)
        {
            velocity = Vector2.Zero;
            Position = defaultPos;
            tween = GetNode<Tween>("Tween");
            lblTxt = GetNode<Label>("Label");
            lblTxt.Text = "+"+dmg.ToString();
            if(colorType == TextColorType.Damage)
            {
                lblTxt.SelfModulate = new Color("ff0000");
                if(dmg > 0) lblTxt.Text = "-";
                lblTxt.Text += dmg.ToString();
            }
            else if(colorType == TextColorType.Heal)
            {
                lblTxt.SelfModulate = new Color("68ff00");
            }
            else if(colorType == TextColorType.PowerUp)
            {
                lblTxt.SelfModulate = new Color("00abff");
            }
            ready = true;
            Visible = true;
            StartEffect();
        }
        public override void _Process(float delta)
        {
            if(ready)
            {
                velocity += gravity * mass * delta;
                Position += velocity * delta;
            }
        }

        private async void StartEffect()
        {
            //fade from current color after 0.7 seconds
            tween.InterpolateProperty(this, "Modulate",
                new Color(Modulate.r, Modulate.g, Modulate.b, Modulate.a),
                new Color(Modulate.r, Modulate.g, Modulate.b, 0.0f),
                0.3f, Tween.TransitionType.Linear, Tween.EaseType.Out, 0.2f
            );

            //Increase Size after 0.6 seconds, start to shrink slightly
            tween.InterpolateProperty(this, "Scale",
                new Vector2(0, 0),
                new Vector2(1.0f, 1.0f),
                0.3f, Tween.TransitionType.Quart, Tween.EaseType.Out
            );
            //start the tweens
            tween.Start();

            await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
            Visible = false;
            ready = false;
            velocity = Vector2.Zero;
        }
    }

    public enum TextColorType
    {
        Damage,
        Heal,
        PowerUp
    }
}
