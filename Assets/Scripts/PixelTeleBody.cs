using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PixelTeleBody : PixelBody
{
    public List<PixelCollider> teleColliders;
    public GameObject teleSpriteContainer;
    public bool isInPortal;
    public PixelPortal telePortal;

    protected override void Start()
    {
        base.Start();
        teleColliders = new List<PixelCollider>();
        teleSpriteContainer = transform.Find("TeleSpriteContainer").gameObject;
    }

    public override void Move(Vector2Int step)
    {
        base.Move(step);
        TeleMove(step);
    }

    public override void Rotate(Vector2Int o, int a)
    {
        base.Rotate(o, a);
        foreach (PixelCollider col in teleColliders)
        {
            col.Rotate(o, a);
        }
    }

    public override void Reflect(Vector2Int o, int n)
    {
        base.Reflect(o, n);
        foreach (PixelCollider col in teleColliders)
        {
            col.Reflect(o, n);
        }
    }

    public override void Reflect(Vector2Int s, Vector2Int e, int n)
    {
        base.Reflect(s, e, n);
        foreach (PixelCollider col in teleColliders)
        {
            col.Reflect(s, e, n);
        }
    }

    public virtual void TeleMove(Vector2Int step)
    {
        Vector2Int teleStep;
        if (isInPortal)
        {
            if (telePortal.isLinkFlipped)
            {
                teleStep = PixelMath.RotateVectorInt(PixelMath.ReflectVectorInt(step, telePortal.teleNormal), telePortal.linkRotation);
            }
            else
            {
                teleStep = PixelMath.RotateVectorInt(step, telePortal.linkRotation);
            }
        }
        else
        {
            teleStep = step;
        }
        foreach (PixelCollider col in teleColliders)
        {
            col.Move(teleStep);
        }
        teleSpriteContainer.transform.position += new Vector3(teleStep.x - step.x, teleStep.y - step.y);
        List<PixelBody> overlapPortals = new List<PixelBody>();
        foreach (PixelCollider col in colliders)
        {
            overlapPortals.AddRange(pixelPhysics.Scan(col.shape, (int)PixelPhysics.layers.portal));
        }
        if (isInPortal)
        {
            if (!overlapPortals.Contains(telePortal))
            {
                ExitPortal(step);
                Move(step);
            }
        }
        if (!isInPortal)            // No else here in case the player exits one portal and enters another in the same frame.
        {
            if (overlapPortals.Count > 0)
            {
                EnterPortal((PixelPortal)overlapPortals[0], step);
            }
        }
    }

    // Call this in the child class after all colliders are created.
    public void CreateTeleColliders()
    {
        foreach (PixelCollider col in colliders)
        {
            PixelCollider teleCol = (PixelCollider)col.Clone();
            teleColliders.Add(teleCol);
        }
    }
    public override List<PixelCollider> GetActiveColliders()
    {
        List<PixelCollider> ret;
        if (isActive)
        {
            ret = colliders.Where(col => col.isActive).ToList();
            if (isInPortal)
            {
                ret.AddRange(teleColliders.Where(col => col.isActive));
            }
        }
        else
        {
            ret = new List<PixelCollider>();
        }
        return ret;
    }

    public virtual void EnterPortal(PixelPortal portal, Vector2Int step)
    {
        isInPortal = true;
        telePortal = portal;
        Vector2 teleDelta;
        float cos = Mathf.Cos(telePortal.linkRotation * Mathf.Deg2Rad);
        float sin = Mathf.Sin(telePortal.linkRotation * Mathf.Deg2Rad);
        if (telePortal.isLinkFlipped)
        {
            foreach (PixelCollider col in teleColliders)
            {
                col.Reflect(telePortal.start, telePortal.end, telePortal.teleNormal);
                col.Rotate(telePortal.start, telePortal.linkRotation);
                col.Move(telePortal.linkOffset);
            }
            teleSpriteContainer.transform.RotateAround(new Vector3(telePortal.start.x + telePortal.end.x + 1f, telePortal.start.y + telePortal.end.y + 1f) / 2f, new Vector3(Mathf.Cos(telePortal.teleNormal), Mathf.Sin(telePortal.teleNormal), 0f), 180f);
            teleSpriteContainer.transform.RotateAround(new Vector3(telePortal.start.x + cos, telePortal.start.y + sin), Vector3.forward, telePortal.linkRotation);
            teleSpriteContainer.transform.position += new Vector3(telePortal.linkOffset.x, telePortal.linkOffset.y);
            teleSpriteContainer.transform.position = new Vector3(Mathf.RoundToInt(teleSpriteContainer.transform.position.x), Mathf.RoundToInt(teleSpriteContainer.transform.position.y), Mathf.RoundToInt(teleSpriteContainer.transform.position.z));
            teleDelta = PixelMath.RotateVector(Vector2.Reflect(delta, telePortal.teleNormalVec), telePortal.linkRotation);
        }
        else
        {
            foreach (PixelCollider col in teleColliders)
            {
                col.Rotate(telePortal.start, telePortal.linkRotation);
                col.Move(telePortal.linkOffset);
            }
            teleSpriteContainer.transform.RotateAround(new Vector3(telePortal.start.x + .5f, telePortal.start.y + .5f), Vector3.forward, telePortal.linkRotation);
            teleSpriteContainer.transform.position += new Vector3(telePortal.linkOffset.x, telePortal.linkOffset.y);
            teleSpriteContainer.transform.position = new Vector3(Mathf.Round(teleSpriteContainer.transform.position.x), Mathf.Round(teleSpriteContainer.transform.position.y), Mathf.Round(teleSpriteContainer.transform.position.z));
            teleDelta = PixelMath.RotateVector(delta, telePortal.linkRotation);
        }
        int boundDispX = Mathf.FloorToInt(teleDelta.x + (0f < teleDelta.x ? 1f : 0f));
        int boundDispY = Mathf.FloorToInt(teleDelta.y + (0f < teleDelta.y ? 1f : 0f));
        foreach (PixelCollider col in teleColliders)
        {
            col.SetBounds(new Vector2Int(boundDispX, boundDispY));
            col.nearbyColliders.Clear();
            foreach (PixelCollider other in pixelPhysics.GetNearbyColliders(col))
            {
                col.nearbyColliders.Add(other);
                other.nearbyColliders.Add(col);
            }
        }
        teleSpriteContainer.SetActive(true);
    }

    public virtual void ExitPortal(Vector2Int step)
    {
        isInPortal = false;
        if (Math.Abs(step.y) < Math.Abs(step.x))
        {
            if (telePortal.teleNormalVec.x != 0 && Math.Sign(step.x) == -Math.Sign(telePortal.teleNormalVec.x))
            {
                Teleport();
            }
            else
            {
                teleSpriteContainer.transform.localRotation = Quaternion.identity;
                teleSpriteContainer.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            if (telePortal.teleNormalVec.y != 0 && Math.Sign(step.y) == -Math.Sign(telePortal.teleNormalVec.y))
            {
                Teleport();
            }
            else
            {
                teleSpriteContainer.transform.localRotation = Quaternion.identity;
                teleSpriteContainer.transform.localPosition = Vector3.zero;
            }
        }
        telePortal = null;
        teleSpriteContainer.SetActive(false);
    }

    protected virtual void Teleport()
    {
        Vector2 teleDelta;
        Vector2 teleVel;
        if (telePortal.isLinkFlipped)
        {
            foreach (PixelCollider col in colliders)
            {
                col.Reflect(telePortal.start, telePortal.end, telePortal.teleNormal);
                col.Rotate(telePortal.start, telePortal.linkRotation);
                col.Move(telePortal.linkOffset);
            }
            transform.rotation = teleSpriteContainer.transform.rotation;
            transform.position = teleSpriteContainer.transform.position;
            teleSpriteContainer.transform.localRotation = Quaternion.identity;
            teleSpriteContainer.transform.localPosition = Vector3.zero;
            Vector3 reflectAxis = new Vector3(Mathf.Cos(telePortal.teleNormal), Mathf.Sin(telePortal.teleNormal));
            //transform.RotateAround(new Vector3(telePortal.start.x + telePortal.end.x + 1, telePortal.start.y +  telePortal.end.y + 1, transform.position.z), reflectAxis, 180f);
            //transform.RotateAround(new Vector3(telePortal.start.x, telePortal.start.y), Vector3.forward, telePortal.linkRotation);
            //transform.position += new Vector3(telePortal.linkOffset.x, telePortal.linkOffset.y);
            //transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));
            //Reflect(telePortal.start, telePortal.end, telePortal.teleNormal);
            //Rotate(telePortal.start, telePortal.linkRotation);
            //Move(telePortal.linkOffset);
            teleDelta = PixelMath.RotateVector(Vector2.Reflect(delta, telePortal.teleNormalVec), telePortal.linkRotation);
            teleVel = PixelMath.RotateVector(Vector2.Reflect(vel, telePortal.teleNormalVec), telePortal.linkRotation);
        }
        else
        {
            foreach (PixelCollider col in colliders)
            {
                col.Rotate(telePortal.start, telePortal.linkRotation);
                col.Move(telePortal.linkOffset);
            }
            transform.position = teleSpriteContainer.transform.position;
            transform.rotation = teleSpriteContainer.transform.rotation;
            teleSpriteContainer.transform.localRotation = Quaternion.identity;
            teleSpriteContainer.transform.localPosition = Vector3.zero;
            //transform.RotateAround(new Vector3(telePortal.start.x, telePortal.start.y), Vector3.forward, telePortal.linkRotation);
            //transform.position += new Vector3(telePortal.linkOffset.x, telePortal.linkOffset.y);
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));
            teleDelta = PixelMath.RotateVector(delta, telePortal.linkRotation);
            teleVel = PixelMath.RotateVector(vel, telePortal.linkRotation);
        }
        int boundDispX = Mathf.FloorToInt(teleDelta.x + (0f < teleDelta.x ? 1f : 0f));
        int boundDispY = Mathf.FloorToInt(teleDelta.y + (0f < teleDelta.y ? 1f : 0f));
        foreach (PixelCollider col in colliders)
        {
            col.SetBounds(new Vector2Int(boundDispX, boundDispY));
            col.nearbyColliders.Clear();
            foreach (PixelCollider other in pixelPhysics.GetNearbyColliders(col))
            {
                col.nearbyColliders.Add(other);
                other.nearbyColliders.Add(col);
            }
        };
        delta = Vector2.zero;// teleDelta;
        vel = teleVel;
    }

    public override void PrePhysics()
    {
        base.PrePhysics();
        if (isInPortal)
        {
            Vector2 teleDelta;
            if (telePortal.isLinkFlipped)
            {
                teleDelta = PixelMath.RotateVector(Vector2.Reflect(delta, telePortal.teleNormalVec), telePortal.linkRotation);
            }
            else
            {
                teleDelta = PixelMath.RotateVector(delta, telePortal.linkRotation);
            }

            int boundDispX = Mathf.FloorToInt(teleDelta.x + (0f < teleDelta.x ? 1f : 0f));
            int boundDispY = Mathf.FloorToInt(teleDelta.y + (0f < teleDelta.y ? 1f : 0f));
            Vector2Int boundDisp = new Vector2Int(boundDispX, boundDispY);
            foreach (PixelCollider col in teleColliders)
            {
                col.SetBounds(boundDisp);
                col.nearbyColliders = pixelPhysics.GetNearbyColliders(col);
            }
        }
    }
}
