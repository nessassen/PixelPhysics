using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : PixelTeleBody
{
    private PixelCollider bodyBox;
    private PixelCollider legBox;
    public int legLength;
    public int legLengthDiag;
    public bool isOnSurface;
    public int surfaceNormal;
    private Vector2 defaultGrav;

    public float runForce;
    public float runDecel;
    public float airForce;
    public float airDecel;
    public float climbForce;
    public float climbDecel;
    public float grabSpeed;
    public float jumpForceInitial = 60f;            // Amount of force added when the player jumps.
    public float jumpForceAdded = 1f;               // Amount of force added each frame while the player holds jump.

    public float dInputThreshold;                   // Only register input if it's magnitude meets the threshold
    private bool dInput;                            // Whether directional input this frame meets threshold
    private float hInput;                           // Horizontal input component
    private float vInput;                           // Vertical input component
    private float dInputAngle;                      // Rounded to the nearest 45

    private bool jumpPress = false;
    private bool jumpHold = false;

    protected override void Start()
    {
        defaultGrav = grav;
        base.Start();
    }

    protected override void CreateColliders()
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        bodyBox = new PixelCollider(new PixelOct(pos.x + 5, pos.x + 26, pos.y + 5, pos.y + 26, 6, 6, 6, 6), (int)PixelPhysics.layers.player, (int)PixelPhysics.layers.all);
        //bodyBox = new PixelCollider(new PixelRect(pos.x, pos.x + 32, pos.y, pos.y + 32), (int)PixelPhysics.layers.player, (int)PixelPhysics.layers.all);
        bodyBox.body = this;
        colliders.Add(bodyBox);
        legBox = new PixelCollider(new PixelOct(pos.x + 7, pos.x + 24, pos.y, pos.y + 4, 4, 0, 4, 0), (int)PixelPhysics.layers.player, (int)PixelPhysics.layers.structure | (int)PixelPhysics.layers.interactable);
        legBox.body = this;
        legBox.isActive = false;
        colliders.Add(legBox);
    }

    public override void PrePhysics()
    {
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
        jumpPress = Input.GetButtonDown("Jump");
        jumpHold = Input.GetButton("Jump");
        base.PrePhysics();
    }

    public override bool PreCollision(PixelBody other, int normal)
    {
        GrabSurface(normal);
        return base.PreCollision(other, normal);
    }

    public void GrabSurface(int normal)
    {
        isOnSurface = true;
        surfaceNormal = normal;
        grav = Vector2.zero;
        int length = (normal % 90 == 0 ? legLength : legLengthDiag);
        int cosNorm = PixelMath.CosInt(normal);
        int sinNorm = PixelMath.SinInt(normal);
        Vector2Int standDisp = new Vector2Int(cosNorm, sinNorm) * length;
        Debug.Log(standDisp);
        PixelShape bodyScanShape = (PixelShape)bodyBox.shape.Clone();
        bodyScanShape.Move(standDisp);
        bool canStand = true;// pixelPhysics.Scan(bodyScanShape, bodyBox.mask).Count == 0;
        if (canStand)
        {
            Move(standDisp);
        }
        legBox.isActive = true;
    }
}
