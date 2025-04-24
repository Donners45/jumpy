using Godot;
using System;

public partial class PointJumper : CharacterBody2D
{
	public const float Speed = 600.0f;
	
	private float[,] JumpForces = {
		{ 15f, 3000f },
		{ 30f, 2000f },
		{ 45f, 1800f },
		{ 55f, 1700f }
	};

	public const float JumpForce = 1800.0f;
	private bool isJumping = false;
	private bool isBouncing = false;
	private bool showAng = false;
	private float HoldTime = 0f;
	private float MaxHold = 2f;
	private float _ropeLength = 500f;
	private bool _isGrappled = false;
	private Vector2 _grapplePoint; 
	private Line2D _ropeLine;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private Vector2 _startPos;
	public override void _Ready()
	{
		_startPos = Transform.Origin;
		_ropeLine = GetNode<Line2D>("RopeLine");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_isGrappled)
		{
			_ropeLine.Visible = true;
		Vector2 playerLocal = _ropeLine.ToLocal(GlobalPosition);
		Vector2 grappleLocal = _ropeLine.ToLocal(_grapplePoint);

		_ropeLine.Points = new Vector2[] { playerLocal, grappleLocal };
		}
		else
		{
			_ropeLine.Visible = false;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
{
	if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
	{
		if (Input.IsKeyLabelPressed(Key.R)) {
			Position = _startPos;
		}
	  
		if (Input.IsKeyLabelPressed(Key.T)) {
			_startPos = Position;
		}
	}
}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;


		if (_isGrappled) {
			velocity = ApplyGrappleForce((float)delta);
		}

		// Add the gravity.
		if (!IsOnFloor() && !isJumping) {
			velocity.Y += (gravity * 2f) * (float)delta;	
			
			// Handle if you bounce
			// todo: bug when bouncing when you are already touching a wall
			if (IsOnWallOnly() && !_isGrappled) {
				if (!isBouncing) {
					for (int i = 0; i < GetSlideCollisionCount(); i++) {
						GD.Print("Checking bounces");
						var collision = GetSlideCollision(i);
						if (Math.Abs(collision.GetNormal().X) > 0.9) {
							isBouncing = true;
							GD.Print("Bounce");
							GD.Print(collision.GetNormal().X);
							velocity.X = 300 * collision.GetNormal().X;
						}
					}
				}
			}
		} else {
			isJumping = false;
			isBouncing = false;
		}
		
		if (Input.IsMouseButtonPressed(MouseButton.Left)) {
			TryShootGrapple();
		}

		if (Input.IsMouseButtonPressed(MouseButton.Right) && _isGrappled) {
			_isGrappled = false;

		}

		// Start Jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor()) {
			HoldTime = 0;
			showAng = true;
		}

		// Charge Jump
		if (Input.IsActionPressed("jump") && IsOnFloor()) {
			HoldTime = Mathf.Min(HoldTime + (float)delta, MaxHold);
		}

		// Release Jump
		if (Input.IsActionJustReleased("jump") && IsOnFloor()) {
			
			var x = Input.GetAxis("ui_left", "ui_right");

			var stageI = (int)Math.Round((HoldTime / MaxHold) * (JumpForces.GetLength(0) - 1));

			stageI = Mathf.Clamp(stageI, 0, JumpForces.GetLength(0) - 1);

			GD.Print("held: ", HoldTime, " max hold: ", MaxHold, " stage: ", stageI);
			var ang = JumpForces[stageI, 0];
			var frc = JumpForces[stageI, 1];

			GD.Print("jumping at: ", ang, " with frc: ", frc, " with x axis: ", x);
			velocity = JumpTowardsAngle(ang, frc ,x);
			showAng = false;
		}

		// Walking left, right.
		// todo: Replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("left", "right", "ui_up", "ui_down");
		if (direction != Vector2.Zero && IsOnFloor() && !Input.IsActionPressed("jump"))
		{
			velocity.X = direction.X * Speed;
		}
		else if(IsOnFloor() && !isJumping)
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
		QueueRedraw();
	}

	private Vector2 JumpTowardsMouse() {
		Vector2 velocity = Velocity;
		var mousePos = GetGlobalMousePosition();
		var direction = (mousePos - GlobalPosition).Normalized();
		velocity = direction * JumpForce;
		velocity.Y -= JumpForce * 0.5f;

		isJumping = true;
		return velocity;
	}

	private Vector2 JumpTowardsAngle(float angle, float force, float x) {
		Vector2 velocity = Velocity;
		var angle_radians = Mathf.DegToRad(angle);

		velocity.Y = -force * Mathf.Sin(angle_radians);
		isJumping = true;
		return velocity;
	}

	private void TryShootGrapple() {
		
		Vector2 mousePos = GetGlobalMousePosition();
		Vector2 direction = (mousePos - GlobalPosition).Normalized();

		// Raycast to detect surface
		var space = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + direction * _ropeLength);
		var result = space.IntersectRay(query);

		if (result.Count > 0)
		{
			GD.Print("Grapple set");
			_grapplePoint = (Vector2)result["position"];
			_isGrappled = true;
			// Optionally reset vertical speed for clean swing
			//Velocity.Y = 0;
		}
	}

	private Vector2 ApplyGrappleForce(float dt)
	{
		Vector2 toGrapple = GlobalPosition - _grapplePoint;
		var velocity = Velocity;
		float distance = toGrapple.Length();

		// Limit to max rope length (pretend the rope is taut)
		if (distance > _ropeLength)
		{
			Vector2 direction = toGrapple.Normalized();

			// Project velocity onto tangent of the swing arc (circular motion)
			Vector2 tangent = new Vector2(-direction.Y, direction.X); // Perpendicular
			float speed = velocity.Dot(tangent);
			velocity = tangent * speed;

			// Move the player back to the rope edge
			GlobalPosition = _grapplePoint + direction * _ropeLength;
		}

		// Optional: Add player swing input (left/right) for control
		float input = Input.GetActionStrength("left") - Input.GetActionStrength("right"); 
		
		Vector2 swingForce = new Vector2(-toGrapple.Y, toGrapple.X).Normalized() * input * 600f;
		velocity += swingForce * dt;
		return velocity;
	}

	public override void _Draw()
	{
		if (isJumping == false && showAng) {
			var angleOfAttack = Mathf.Lerp(1, 75, HoldTime / MaxHold);
			var rad_angle = Mathf.DegToRad(angleOfAttack);

			var v = new Vector2(
				JumpForce * Mathf.Cos(rad_angle),
				-JumpForce * Mathf.Sin(rad_angle)
			);

			DrawLine(Vector2.Zero, v, new Color(1, 0, 0), 2);
		}
	}
}
