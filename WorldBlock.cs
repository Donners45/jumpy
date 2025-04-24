using Godot;
using System;


[Tool]
public partial class WorldBlock : Node2D
{

	[Export]
	public Polygon2D Polygon;
	
	/// <summary>
	/// todo: bug when setting font in editor then switching OS (mac to win)
	///         seems to remove the font. Perhaps better to fix this with 
	///         some sort of font invariant - if that exists?
	/// 
	/// todo: possible bug when modifying the public polygon many times
	/// 		eventually saw an instance where the debugging text was 
	/// 		completely wrong showing values of 100+ degs on shall inclines
	/// </summary>
	[Export]
	public Font Font;

	[Export]
	public bool ShowDebug;

	public override void _Ready()
	{
		// don't sync during editor
		// will degrade editor performance for little benefit
		if (!Engine.IsEditorHint())
		{
			SyncPolygons();
		}
	}

	/// <summary>
	/// Sets the colision mesh to the same shape as the Polygon2D that holds texture and shape.
	/// We can also set things like Light Occluders etc. 
	/// </summary>
	private void SyncPolygons()
	{
		var col = GetNodeOrNull<CollisionPolygon2D>("CollisionPolygon2D");
		if (col != null) col.Polygon = Polygon.Polygon;        
	}

	/// <summary>
	/// Only for debug/editor purposes
	/// </summary>
	public override void _Draw()
	{
		if (!ShowDebug) return;
		
		if (Polygon == null || Polygon.Polygon.Length < 2 || Font == null)
			return;

		for (int i = 0; i < Polygon.Polygon.Length; i++)
		{            
			Vector2 start = Polygon.Polygon[i];
			Vector2 end = Polygon.Polygon[(i + 1) % Polygon.Polygon.Length];
			Vector2 dir = end - start;
			float slope_deg = Mathf.Abs(Mathf.RadToDeg(Mathf.Atan2(dir.Y, dir.X)));

			// calculate the "outside" angle rather than the inside
			slope_deg = -(slope_deg + 180 - 360);

			var color = slope_deg > 45f ? new Color(1, 0, 0) : new Color(0.5f, 0.5f, 0.5f);

			if (slope_deg > 45f) {
				DrawLine(start, end, color, 2f);
			}
		
			Vector2 mid = (start + end) * 0.5f;
			DrawString(Font, mid + new Vector2(4, -4), $"{Mathf.Round(slope_deg)}°");
		}
	}
}
