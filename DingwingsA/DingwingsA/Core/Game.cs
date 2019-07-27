using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using Hardware;

using unit = System.Single;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

public partial class Core {
    public List<World> worlds = new List<World>();
    public List<GameState> stateStack = new List<GameState>();
    public static int TILE_SIZE = 32;
    public static Core instance = null;
    public static System.Random rand = new System.Random();
    public static Player p = new Player();
    private float touchTime = 0;
    public int frames = 0;
    public const int COLLIDES_X = 1, COLLIDES_Y = 2, COLLIDES_BOTH = 3;
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    public Core()
    {
        instance = this;

        worlds.Add(new TiledWorld("test", testWorldInit));
        //p.world = 0;

        stateStack.Add(new WorldState());
        stateStack.Add(new Textbox("Yo this is a test will it work? will it not? will it not? will it not? this is a stupid rap yo"));
    }

    public void testWorldInit(World w)
    {

    }

    public bool getFlag(string s)
    {
        if(flags.ContainsKey(s))
        {
            return flags[s];
        }
        return false;
    }

    public void setFlag(string s, bool b = true)
    {
        if(flags.ContainsKey(s))
        {
            flags[s] = b;
        }
        else
        {
            flags.Add(s, b);
        }
    }

    public static bool frustrumCull(unit x, unit y)
    {
        return x < -Graphics.WIDTH / 2 || x > 3*Graphics.WIDTH / 2 || y < -Graphics.HEIGHT / 2 || y > 3*Graphics.HEIGHT / 2;
    }
    
    public static bool rectCollides(unit x1, unit y1, float width1, float height1, unit x2, unit y2, float width2, float height2)
    {
        return !(x1 >= x2 + width2 || x1 + width1 <= x2 || y1 >= y2 + height2 || y1 + height1 <= y2);
    }

    public static int safeDiv(int value, int factor)
    {
        if (value >= 0) return value / factor;
        return (value + 1 - factor) / factor;
    }

    public static int safeMod(int value, int factor)
    {
        return (value % factor + factor) % factor;
    }

    public static int safeDiv(float value, int factor)
    {
        return safeDiv(Mathf.FloorToInt(value), factor);
    }

    public static int safeMod(float value, int factor)
    {
        return safeMod(Mathf.FloorToInt(value), factor);
    }

    public static World getWorld(string name)
    {
        return instance.worlds.Find(a => a.name == name);
    }

    public static unit getOnscreenX(unit x)
    {
        return x - p.getCameraX() + Graphics.WIDTH / 2 - TILE_SIZE / 2;
    }

    public static unit getOnscreenY(unit y)
    {
        return y - p.getCameraY() + Graphics.HEIGHT / 2 - TILE_SIZE / 2;
    }

    public static void draw(GameSprite s, int x, int y, int layer = 0)
    {
        Graphics.draw(s, x, y,s.pixelWidth,s.pixelHeight);
    }

    public void run()
    {
        frames++;
        if (stateStack.Count > 0)
        {
            GameState.startMenu();
            GameState top = stateStack[stateStack.Count-1];
            top.run();
            GameState.endMenu();
        }
        if (GameState.getTouch())
        {
            touchTime = 10;
        }
        else
        {
            if (touchTime > 0) touchTime -= HardwareInterface.deltaTime;
        }
    }

    public void draw()
    {
        if (stateStack.Count > 0) stateStack[stateStack.Count-1].draw();
    }
}

public class TitleScreen : GameState
{
    public TitleScreen() : base()
    {

    }

    public override void draw()
    {
        Graphics.draw(Graphics.bitmapFont, (Graphics.WIDTH - 160) / 2, (Graphics.HEIGHT - 144) / 2, 160, 144, Graphics.spriteDefault);
    }

    public override void run()
    {
        //Sound.setMusic(Sound.baseSong);
        if((getTouch()&&!touch)||(getA()&&!a))
        {
            Core.instance.stateStack.Remove(this);
            //Core.instance.stateStack.Push(new State());
        }
    }
}