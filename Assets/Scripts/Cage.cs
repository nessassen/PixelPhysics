using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cage : PixelBody
{
    public Vector2Int size;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        PixelShape shape;
        PixelCollider col;
        shape = new PixelLine(new Vector2Int((int)transform.position.x, (int)transform.position.y + 1), new Vector2Int((int)transform.position.x, (int)transform.position.y + size.y - 2));
        col = new PixelCollider(shape, 1, int.MaxValue);
        RegisterCollider(col);
        shape = new PixelLine(new Vector2Int((int)transform.position.x + size.x - 1, (int)transform.position.y + 1), new Vector2Int((int)transform.position.x + size.x - 1, (int)transform.position.y + size.y - 2));
        col = new PixelCollider(shape, 1, int.MaxValue);
        RegisterCollider(col);
        shape = new PixelLine(new Vector2Int((int)transform.position.x + 1, (int)transform.position.y), new Vector2Int((int)transform.position.x + size.x - 2, (int)transform.position.y));
        col = new PixelCollider(shape, 1, int.MaxValue);
        RegisterCollider(col);
        shape = new PixelLine(new Vector2Int((int)transform.position.x + 1, (int)transform.position.y + size.y - 1), new Vector2Int((int)transform.position.x + size.x - 2, (int)transform.position.y + size.y - 1));
        col = new PixelCollider(shape, 1, int.MaxValue);
        RegisterCollider(col);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
