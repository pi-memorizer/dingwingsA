using System.Collections;
using System.Collections.Generic;
using System.IO;
using unit = System.Single;
using System;
using Hardware;

public abstract class GameState
{
    public Player p;
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
    public abstract void draw();

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
    public List<Entity> entities = new List<Entity>();
    public delegate void InitLevel(World w);
    public InitLevel initLevel;


    public World(string name, InitLevel initLevel)
    {
        this.name = name;
        this.initLevel = initLevel;
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
    public float vx, vy;
    public int width, height;
    public bool alive = true, grounded;
    public string id = "";
    public int size = 1;
    public float dashing = 0, vdash;
    public bool flipped = false;

    public virtual bool collides(Entity e)
    {
        return Core.rectCollides(e.x,e.y,e.width,e.height,x,y,width,height);
    }
    

    public abstract void draw();
    public abstract void run();

    public virtual bool passable(int collision)
    {
        return collision == 0;
    }

    public bool collidesTile(World w, int tilex, int tiley, ref bool jump, ref bool dead)
    {
        int c = w.getCollision(tilex, tiley);

        if(c==1)
        {
            return Core.rectCollides(tilex * Core.TILE_SIZE, tiley * Core.TILE_SIZE, Core.TILE_SIZE, Core.TILE_SIZE, x, y, width, height);
        }
        if(c==2&&grounded)
        {
            jump = true;
        }
        if(c==3&&grounded&&vx!=0)
        {
            dashing = 1;
            vdash = Mathf.Sign(vx) * 3 * WorldState.PLAYER_MOVE_SPEED;
        }
        if(c==4||c==5)
        {
            Player p = this as Player;
            if(p!=null&&!p.coins.Contains(new Coord(tilex,tiley)))
            {
                //get the coins
                p.coins.Add(new Coord(tilex, tiley));
            }
        }
        if(c==6)
        {
            //thin platform
            return Core.rectCollides(tilex * Core.TILE_SIZE, tiley * Core.TILE_SIZE, Core.TILE_SIZE, 4, x, y, width, height);
        }
        if(c==7)
        {
            dead = true;
        }

        return false;
    }

    public void simulate(World w)
    {
        if (vy > WorldState.PLAYER_JUMP_SPEED * .2F) grounded = false;
        float __vx = vx;
        float __vy = vy;
        if (dashing > 0)
        {
            __vx = vdash;
            __vy = 0;
            vy = 0;
            grounded = false;
        }
        float mag = Mathf.CeilToInt(Mathf.Sqrt(__vx * __vx + __vy * __vy));
        float _vx = __vx / mag * HardwareInterface.deltaTime;
        float _vy = __vy / mag * HardwareInterface.deltaTime;
        if (dashing > 0)
        {
            dashing -= HardwareInterface.deltaTime;
        }
        
        bool jump = false, dead = false;
        for (int i = 0; i < mag; i++)
        {
            //apply subvelocity
            x += _vx;
            bool collision = false;
            foreach(Entity e in w.entities)
            {
                if (e == this) continue;
                collision = collision || e.collides(this);
            }
            if (this != Core.p) collision = collision || Core.p.collides(this);
            for(int _x = Core.safeDiv(x,Core.TILE_SIZE); _x <= Core.safeDiv(x+width,Core.TILE_SIZE); _x++)
            {
                for (int _y = Core.safeDiv(y, Core.TILE_SIZE); _y <= Core.safeDiv(y + height, Core.TILE_SIZE); _y++)
                {
                    collision = collision || collidesTile(w, _x, _y, ref jump, ref dead);
                }
            }
            //if collided, undo that subvelocity and cancel velocity
            if(collision)
            {
                x -= _vx;
                vx = 0;
                dashing = 0;
            }

            y += _vy;
            collision = false;
            foreach (Entity e in w.entities)
            {
                if (e == this) continue;
                collision = collision || e.collides(this);
            }
            for (int _x = Core.safeDiv(x, Core.TILE_SIZE); _x <= Core.safeDiv(x + width, Core.TILE_SIZE); _x++)
            {
                for (int _y = Core.safeDiv(y, Core.TILE_SIZE); _y <= Core.safeDiv(y + height, Core.TILE_SIZE); _y++)
                {
                    collision = collision || collidesTile(w, _x, _y, ref jump, ref dead);
                }
            }
            if (collision)
            {
                y -= _vy;
                if(vy>0)
                {
                    //probably hitting the ground
                    grounded = true;
                }
                vy = 0;
            }
        }
        if(jump)
        {
            grounded = false;
            vy -= WorldState.PLAYER_JUMP_SPEED;
        }
        if(dead&&this is Player)
        {
            x = 0;
            y = 0;
        }
    }
}