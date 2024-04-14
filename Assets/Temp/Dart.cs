using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dart : PixelTeleBody
{
    public enum coatTypes
    {
        none = 0,
        portal = 1
    }
    public Vector2Int pos;
    public Vector2Int size;
    public uint coat;
    public PixelPortal coatPortal;

    public Dart(Vector2Int p, Vector2Int s)
    {
        pos = p;
        size = s;
    }

    protected override void Start()
    {
        base.Start();
        elastic = 1f;
        friction = 0f;
        grav = Vector2.zero;
        PixelCollider mainCol = new PixelCollider(new PixelRect(pos.x, pos.x + size.x, pos.y, pos.y + size.y), (int)PixelPhysics.layers.projectile, (int)PixelPhysics.layers.all);
        // Used to check for continuous surface in front of collider (so dart can't stick to the edge of objects)
        PixelCollider checkA = new PixelCollider(new PixelRect(pos.x + size.x, pos.x + size.x + 1, pos.y, pos.y + 1), (int)PixelPhysics.layers.projectile, (int)PixelPhysics.layers.none);
        PixelCollider checkB = new PixelCollider(new PixelRect(pos.x + size.x, pos.x + size.x + 1, pos.y + 1, pos.y + size.y), (int)PixelPhysics.layers.projectile, (int)PixelPhysics.layers.none);
        CreateTeleColliders();
    }

    public override bool PreCollision(PixelBody other, int normal)
    {
        if (normal == rotation)
        {
            switch(coat)
            {
                case 0:
                    StickTo(other);
                    break;
                case 1:
                    MoveCoatPortal();
                    break;
            }
        }
        return base.PreCollision(other, normal);
    }

    private void StickTo(PixelBody other)
    {
        elastic = 0f;
    }

    private void MoveCoatPortal()
    {
        if (rotation % 90 == 0)
        {
            int cos = PixelMath.CosInt(rotation);
            int sin = PixelMath.SinInt(rotation);
            int length = Math.Abs(coatPortal.end.x - coatPortal.end.x) + Math.Abs(coatPortal.end.y - coatPortal.end.y);
            int halfLength = PixelMath.IntDiv(length, 2);
            Vector2Int tmp = new Vector2Int(pos.x + (cos < 0 ? -1 : 0) + (cos > 0 ? size.x : 0) + (sin != 0 ? PixelMath.IntDiv(size.x, 2) : 0), pos.y + (sin < 0 ? -1 : 0) + (sin > 0 ? size.y : 0) + (cos != 0 ? PixelMath.IntDiv(size.y, 2) : 0));
            coatPortal.Reposition(new Vector2Int(tmp.x + length * sin, tmp.y + length * cos) * (coatPortal.isClockwise ? 1 : -1), new Vector2Int(tmp.x - length * sin, tmp.y - length * cos) * (coatPortal.isClockwise ? 1 : -1), (rotation + 180) % 360);
        }
    }
}
