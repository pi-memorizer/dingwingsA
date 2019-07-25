using System.Collections;
using System.Collections.Generic;
using System.IO;
using unit = System.Single;
using System;
using Hardware;

public abstract class GameState
{
    Player p;
    public static bool a = true, b = true, up = true, down = true, left = true, right = true, start = true;
    public static bool touch = true;
    protected static float weight = .15F;
    public bool blocking = true;
    public bool highGraphicsMode = false;
    public GameState()
    {
        p = Core.p;
    }
    public abstract void run();
    public virtual void draw()
    {

    }

    public virtual void gui()
    {

    }

    public static void startMenu()
    {

    }

    public static void endMenu()
    {
        a = getA();
        b = getB();
        up = getUp();
        down = getDown();
        left = getLeft();
        right = getRight();
        start = getStart();
        touch = getTouch();
    }

    public static bool getTouch()
    {
        if (Input.touchCount() > 0) return true;
        if (Input.getLeftMouse()) return true;
        return false;
    }

    public static int getTouchX()
    {
        float x = 0;
        if (Input.touchCount() > 0) x =  Input.getTouch(0).Position.X;
        else x = Input.mouseState.Position.X;
        return (int)(x / Graphics.scale - Graphics.xOffset);
    }

    public static int getTouchY()
    {
        float y = 0;
        if (Input.touchCount() > 0) y = Input.getTouch(0).Position.Y;
        else y = Input.mouseState.Position.Y;
        return (int)(Graphics.HEIGHT - (y / Graphics.scale - Graphics.yOffset));
    }

    public static bool getA()
    {
        return Input.getA();
    }

    public static bool getB()
    {
        return Input.getB();
    }

    public static bool getStart()
    {
        return Input.getStart();
    }

    public static bool getUp()
    {
        return Input.getUp();
    }

    public static bool getDown()
    {
        return Input.getDown();
    }

    public static bool getRight()
    {
        return Input.getRight();
    }

    public static bool getLeft()
    {
        return Input.getLeft();
    }

    public virtual void reset()
    {
    }
}

public abstract class World
{
    public string name;

    public World(string name)
    {
        this.name = name;
    }

    public abstract int getCollision(int x, int y);
    public abstract void draw();


    public virtual object get(string id, string property, object defaultValue = null)
    {
        return defaultValue;
    }

    public virtual int getCoord(string id, string axis)
    {
        return 0;
    }
}

public abstract class Entity
{
    public unit x, y;
    public unit desiredX, desiredY;
    public bool alive = true;
    public string id = "";
    public int size = 1;

    public int tileX
    {
        get
        {
            return Mathf.FloorToInt(Core.safeDiv(x, Core.TILE_SIZE));
        }
        set
        {
            x = value * Core.TILE_SIZE;
        }
    }

    public int tileY
    {
        get
        {
            return Mathf.FloorToInt(Core.safeDiv(y, Core.TILE_SIZE));
        }
        set
        {
            y = value * Core.TILE_SIZE;
        }
    }

    public int desiredTileX
    {
        get
        {
            return Mathf.FloorToInt(Core.safeDiv(desiredX, Core.TILE_SIZE));
        }
        set
        {
            desiredX = value * Core.TILE_SIZE;
        }
    }

    public int desiredTileY
    {
        get
        {
            return Mathf.FloorToInt(Core.safeDiv(desiredY, Core.TILE_SIZE));
        }
        set
        {
            desiredY = value * Core.TILE_SIZE;
        }
    }

    public virtual bool collides(Entity e, unit x, unit y)
    {
        return false;
    }
    

    public abstract void draw();
    public abstract void run();

    public Entity setID(string s)
    {
        id = s;
        return this;
    }

    public bool destinationCollision(Entity e, unit x, unit y)
    {
        return Core.rectCollides(desiredX, desiredY, size * Core.TILE_SIZE, Core.TILE_SIZE, x, y, e.size * Core.TILE_SIZE, Core.TILE_SIZE);
    }

    public virtual bool passable(int collision)
    {
        return collision == 0;
    }

    public int getDir(unit x, unit y)
    {
        //two dot products lets do this
        float cx = x - this.x;
        float cy = y - this.y;
        float m1 = Mathf.Sqrt(cx * cx + cy * cy);
        cx /= m1;
        cy /= m1;
        if (cx + cy > 0)
        {
            if (cx - cy > 0)
            {
                return 0;
            }
            else
            {
                return 3;
            }
        }
        else
        {
            if (cx - cy > 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
    }

    public bool canMove(World w, int dir)
    {
        bool passable = true;
        int x = Core.safeDiv(this.x, Core.TILE_SIZE);
        int y = Core.safeDiv(this.y, Core.TILE_SIZE);
        if (dir == 0)
        {
            if (!this.passable(w.getCollision(x + size, y))) passable = false;
        }
        if (dir == 1)
        {
            for (int i = 0; i < size; i++) if (!this.passable(w.getCollision(x + i, y - 1))) passable = false;
        }
        if (dir == 2)
        {
            if (!this.passable(w.getCollision(x-1, y))) passable = false;
        }
        if (dir == 3)
        {
            for (int i = 0; i < size; i++) if (!this.passable(w.getCollision(x + i, y + 1))) passable = false;
        }
        return passable;
    }

    public bool isTouching(Entity other)
    {
        return Core.rectCollides(desiredX - Core.TILE_SIZE, desiredY, Core.TILE_SIZE * (2 + size), Core.TILE_SIZE, other.desiredX, other.desiredY, other.size * Core.TILE_SIZE,Core.TILE_SIZE) ||
            Core.rectCollides(desiredX, desiredY - Core.TILE_SIZE, Core.TILE_SIZE * size, Core.TILE_SIZE * 3, other.desiredX, other.desiredY, other.size * Core.TILE_SIZE, Core.TILE_SIZE);
    }

    public static void getDirOffsets(int dir, out int dx, out int dy)
    {
        dx = dy = 0;
        if (dir == 0) dx = 1;
        if (dir == 1) dy = -1;
        if (dir == 2) dx = -1;
        if (dir == 3) dy = 1;
    }

    public int distanceFrom(Entity other)
    {
        int minDistance = 999999;
        for(int i = 0; i < size; i++)
        {
            int distance = Math.Abs(desiredTileX+i - other.desiredTileX) + Math.Abs(desiredTileY - other.desiredTileY);
            if (distance < minDistance) minDistance = distance;
        }
        return minDistance;
    }
}