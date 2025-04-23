using Godot;
using System;

public partial class player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -1000.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	
	private float jumpVelocityMultiplier = 0f;
	private float jumpChargeMultiplier = 0.3f;
	private float angle = 45f;
	private float force = 100f;

	private float frameForce = 0;
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		if (Input.IsActionJustPressed("ui_select") && IsOnFloor()) {
			jumpVelocityMultiplier = 0f;
		}
		if (Input.IsActionPressed("ui_select") && IsOnFloor()) {
			jumpVelocityMultiplier += jumpChargeMultiplier * (float)delta;
			jumpVelocityMultiplier = Mathf.Clamp(jumpVelocityMultiplier, 0f, 1f);
		}

		if (Input.IsActionJustReleased("ui_select") && IsOnFloor()) {
			frameForce = Mathf.Lerp(force, 3000f, jumpVelocityMultiplier);
			
			var rad_angle = Mathf.DegToRad(angle);
			velocity.X = frameForce * Mathf.Cos(rad_angle);
			velocity.Y = -frameForce * Mathf.Sin(rad_angle);

			GD.Print("x: ", velocity.X, " y: ", velocity.Y, " f: ", frameForce, " fm: ", jumpVelocityMultiplier, " cos: ", Mathf.Cos(rad_angle), " sin: ", Mathf.Sin(rad_angle));
		}
		
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_accept","ui_down");
		if (direction != Vector2.Zero && !Input.IsActionPressed("ui_select") && IsOnFloor())
		{
			velocity.X = direction.X * Speed;

		}
		else if(IsOnFloor())
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		QueueRedraw();
		MoveAndSlide();
	}

	public override void _Draw() {
		
		var rad_angle = Mathf.DegToRad(angle);
		// var debug_velocity = Vector2(
		// 	frameForce * Mathf.Cos(rad_angle),
        // 	-frameForce * Mathf.Sin(rad_angle)
    	// );
		var v = new Vector2(
			frameForce * Mathf.Cos(rad_angle),
        	-frameForce * Mathf.Sin(rad_angle)
		);

		var end_point = Vector2.Zero + v * 0.5f;
    	DrawLine(Vector2.Zero, Velocity, new Color(1, 0, 0), 2);
	}
}
