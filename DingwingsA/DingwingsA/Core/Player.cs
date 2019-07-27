﻿using System;
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
    public int world = 0;
    public Player()
    {
        x = 0;
        y = 0;
        width = 32;
        height = 32;

    }

    public override void draw()
    {
        //Graphics.drawRect(Color.White, Core.getOnscreenX(x), Core.getOnscreenY(y), width, height);
        Graphics.draw(Graphics.slime32[(((int)HardwareInterface.timeSinceLevelLoad)%2)*4], Core.getOnscreenX(x), Core.getOnscreenY(y),flipped);
    }

    public override void run()
    {
        vy += HardwareInterface.deltaTime;
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