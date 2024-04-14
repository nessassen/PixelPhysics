using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagBox : PixelBody
{
    public int downDiag;
    public int upDiag;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        PixelShape shape = new PixelDiag(new Vector2Int((int)transform.position.x, (int)transform.position.y + downDiag), downDiag, upDiag);
        PixelCollider col = new PixelCollider(shape, 1, int.MaxValue);
        RegisterCollider(col);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
