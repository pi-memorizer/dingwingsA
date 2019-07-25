using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hardware;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using unit = System.Single;
using System.Diagnostics;

public class Player : Entity
{
    public int dir = 3;
    public float pTime = 0;
    public unit cameraX, cameraY;
    private bool cameraUnlocked = false;
    public bool test = false;
    public bool test2 = false;
    public Player()
    {
        x = 16 * Core.TILE_SIZE;
        y = 62 * Core.TILE_SIZE;
        desiredX = x;
        desiredY = y;
    }

    public override void draw()
    {
    }

    public override void run()
    {
        
    }

    public override bool collides(Entity e, unit x, unit y)
    { 
        return destinationCollision(e,x, y);
    }
    
    public void unlockCamera(unit x, unit y)
    {
        cameraUnlocked = true;
        cameraX = x;
        cameraY = y;
    }

    public void lockCamera()
    {
        cameraUnlocked = false;
    }

    public unit getCameraX()
    {
        if (cameraUnlocked)
            return cameraX;
        else
            return x;
    }

    public unit getCameraY()
    {
        if (cameraUnlocked)
            return cameraY;
        else
            return y;
    }

    public override bool passable(int collision)
    {
        return collision == 0||collision==2;
    }
}