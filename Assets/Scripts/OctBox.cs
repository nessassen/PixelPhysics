using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctBox : PixelBody
{
    public Vector2Int size;
    public int diagonal;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //PixelShape shape = new PixelOct((int)transform.position.x, (int)transform.position.x + size.x - 1, (int)transform.position.y, (int)transform.position.y + size.y - 1, diagonal);
        //PixelCollider col = new PixelCollider(shape, 1, int.MaxValue);
        //RegisterCollider(col);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}