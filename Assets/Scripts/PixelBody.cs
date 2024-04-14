using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PixelBody : MonoBehaviour
{
    protected PixelPhysics pixelPhysics;
    public List<PixelCollider> colliders;
    public GameObject spriteContainer;
    public bool isDynamic;
    public int rotation;
    public bool reflectX;
    public bool reflectY;
    public Vector2 delta;                               // Change in position for current frame.
    public Vector2 spentDelta;                          // Amount position has already changed this frame.
    public Vector2 lateDelta;                           // Delta deffered to the late cycle due to clipping issues etc.
    public Vector2 vel;
    public Vector2 grav;
    public Vector2 netForce;
    public float netForceMag;
    public float mass;
    public float drag;
    public float elastic;
    public float friction;
    public bool xMovementDone;
    public bool yMovementDone;
    public bool isActive = true;
    public bool isDead;

    protected virtual void Start()
    {
        colliders = new List<PixelCollider>();
        CreateColliders();
        spriteContainer = transform.Find("SpriteContainer").gameObject;
        spriteContainer.transform.parent = transform;
        pixelPhysics = PixelPhysics.instance;
        pixelPhysics.RegisterObject(this);
    }

    protected virtual void CreateColliders()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    public virtual void Move(Vector2Int disp)
    {
        transform.position += new Vector3(disp.x, disp.y);
        foreach (PixelCollider col in colliders)
        {
            col.Move(disp);
        }
    }

    public virtual void Rotate(Vector2Int o, int a)
    {
        rotation = (rotation + a + 360) % 360;
        transform.RotateAround(new Vector3(o.x + 0.5f, o.y + 0.5f), Vector3.forward, a);
        foreach (PixelCollider col in colliders)
        {
            col.Rotate(o, a);
        }
    }

    public virtual void Reflect(Vector2Int o, int n)
    {
        Vector3 reflectAxis = new Vector3(Mathf.Cos(n), Mathf.Sin(n));
        reflectX = reflectX ^ (reflectAxis.y > .1f);
        reflectY = reflectY ^ (reflectAxis.x > .1f);
        transform.RotateAround(new Vector3(o.x + 0.5f, o.y + 0.5f), reflectAxis, 180f);
        foreach (PixelCollider col in colliders)
        {
            col.Reflect(o, n);
        }
    }
    public virtual void Reflect(Vector2Int s, Vector2Int e, int n)
    {
        Vector3 o = new Vector3(s.x + e.x, s.y + e.y) / 2;
        Vector3 reflectAxis = new Vector3(Mathf.Cos(n), Mathf.Sin(n));
        reflectX = reflectX ^ (reflectAxis.y > .1f);
        reflectY = reflectY ^ (reflectAxis.x > .1f);
        transform.RotateAround(new Vector3(o.x, o.y), reflectAxis, 180f);
        foreach (PixelCollider col in colliders)
        {
            col.Reflect(s, e, n);
        }
    }

    public void RegisterCollider(PixelCollider col)
    {
        col.body = this;
        colliders.Add(col);
    }

    public void RemoveCollider(int i)
    {
        PixelCollider col = colliders[i];
        col.body = null;
        colliders.RemoveAt(i);
    }

    public virtual List<PixelCollider> GetActiveColliders()
    {
        if (isActive)
        {
            return colliders.Where(col => col.isActive).ToList();
        }
        else
        {
            return new List<PixelCollider>();
        }
    }

    public void ApplyForce(Vector2Int force)
    {
        netForce += force;
        netForceMag += force.magnitude;
    }

    public virtual void PrePhysics()
    {
        if (isDynamic)
        {
            vel += grav * Time.deltaTime;
            vel += (netForce / mass) * Time.deltaTime;
            netForce = Vector2.zero;
            netForceMag = 0f;
            vel *= Mathf.Clamp01(1f - drag * Time.deltaTime);
            delta += vel * Time.deltaTime;
            spentDelta = Vector2.zero;
            lateDelta = Vector2.zero;
            xMovementDone = false;
            yMovementDone = false;

            int boundDispX = Mathf.FloorToInt(delta.x + (0f < delta.x ? 1f : 0f));
            int boundDispY = Mathf.FloorToInt(delta.y + (0f < delta.y ? 1f : 0f));
            Vector2Int boundDisp = new Vector2Int(boundDispX, boundDispY);
            foreach (PixelCollider col in colliders)
            {
                col.SetBounds(boundDisp);
                col.nearbyColliders.Clear();
            }
        }
    }

    public virtual void PostPhysics()
    {
    }

    // Returns true if physical collision should occur.
    public virtual bool PreCollision(PixelBody other, int normal)
    {
        return true;
    }

    // Returns true if collision resolved as expected.
    public virtual bool PostCollision(PixelBody other, int normal)
    {
        return true;
    }
}