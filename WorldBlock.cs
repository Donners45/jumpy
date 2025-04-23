using Godot;
using System;


[Tool]
public partial class WorldBlock : Node2D
{

    [Export]
    public Polygon2D Polygon;
	
    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            SyncPolygons(); // sync once at game start
        }
    }

    // public override void _Process(double delta)
    // {
    //     if (Engine.IsEditorHint())
    //     {
    //         SyncPolygons(); // sync continuously in editor
    //     }
    // }

    private void SyncPolygons()
    {
        var col = GetNodeOrNull<CollisionPolygon2D>("CollisionPolygon2D");
        var poly = GetNodeOrNull<Polygon2D>("Polygon2D");

        if (col != null && poly != null)
        {
          //  poly.Polygon = col.Polygon;
			if (col != null) col.Polygon = Polygon.Polygon;
        	if (poly != null) poly.Polygon = Polygon.Polygon;
        }
    }
}
