using Godot;
using System;


[Tool]
public partial class WorldBlock : Node2D
{

    [Export]
    public Polygon2D Polygon;
	
    [Export]
    public Font Font;

    [Export]
    public bool ShowDebug;

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            SyncPolygons(); // sync once at game start
        }
    }

    private void SyncPolygons()
    {
        var col = GetNodeOrNull<CollisionPolygon2D>("CollisionPolygon2D");
        //var poly = GetNodeOrNull<Polygon2D>("Polygon2D");

        if (col != null)
        {
          //  poly.Polygon = col.Polygon;
			if (col != null) col.Polygon = Polygon.Polygon;
        	//if (poly != null) poly.Polygon = Polygon.Polygon;
        }
    }

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

            slope_deg = -(slope_deg + 180 - 360);
            if (slope_deg > 40f)
                DrawLine(start, end, new Color(1, 0, 0), 2f); // Red = steep
            else
                DrawLine(start, end, new Color(0.5f, 0.5f, 0.5f), 1f);

        // Label slope
            Vector2 mid = (start + end) * 0.5f;
            DrawString(Font, mid + new Vector2(4, -4), $"{Mathf.Round(slope_deg)}°");
        }
    }

}
