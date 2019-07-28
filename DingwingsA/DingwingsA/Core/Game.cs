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
    public static Dictionary<string, bool> flags = new Dictionary<string, bool>();
    public static Dictionary<Coord, GraphicalException> exceptions = new Dictionary<Coord,GraphicalException>();
    public static float deadTime = 0;
    public static int money = 0;
    public static float animationMoney = 0;
    public static float newItemTime = 0;

    public static float[] adStates = new float[3];

    public Core()
    {
        instance = this;

        worlds.Add(new TiledWorld("right", testWorldInit));
        p.world = 0;

        stateStack.Add(new WorldState());
        //stateStack.Add(new ShopState());
        stateStack.Add(new Textbox("Yo this is a test will it work? will it not? will it not? will it not? this is a stupid rap yo"));
        stateStack.Add(new TitleScreen());
    }

    public void testWorldInit(World w)
    {

    }

    public class GraphicalException
    {
        public float time;
        public int newTile;
       
        public GraphicalException(int newTile, float time)
        {
            this.newTile = newTile;
            this.time = time;
        }
    }

    public static void addException(Coord c, int newTile, float time)
    {
        if(!exceptions.ContainsKey(c))
        {
            exceptions.Add(c, new GraphicalException(newTile, time));
        }
    }

    public static bool getFlag(string s)
    {
        if(flags.ContainsKey(s))
        {
            return flags[s];
        }
        return false;
    }

    public static void setFlag(string s, bool b = true)
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

    public static int getGraphicsLevel()
    {
        if (getFlag("graphics2")) return 2;
        if (getFlag("graphics1")) return 1;
        return 0;
    }

    public static int getMusicLevel()
    {
        if (getFlag("music2")) return 2;
        if (getFlag("music1")) return 1;
        return 0;
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

    Stack<Coord> toRemove = new Stack<Coord>();
    public void run()
    {
        if(money<animationMoney)
        {
            int _a = (int)animationMoney;
            animationMoney -= HardwareInterface.deltaTime*100;
            if((int)animationMoney!=_a)
            {
                float x = Graphics.WIDTH-(float)(rand.NextDouble()) * (animationMoney.ToString().Length) * Graphics.CHAR_SIZE - 10;
                float angle = (float)(Core.rand.NextDouble()) * 3.1415F * 2;
                float magnitude = (float)(Core.rand.NextDouble()) * 100;
                Graphics.particles.Add(new Particle(Color.Red, x+p.getCameraX()+p.width/2-Graphics.WIDTH/2, 16 + Graphics.CHAR_SIZE / 2+p.getCameraY()+p.height/2-Graphics.HEIGHT/2, magnitude * Mathf.Cos(angle), magnitude * Mathf.Sin(angle)));
            }
            if (money > animationMoney) animationMoney = money;
        }
        if(money>animationMoney)
        {
            animationMoney += HardwareInterface.deltaTime*100;
            if (money < animationMoney) animationMoney = money;
        }

        foreach(var coord in exceptions.Keys)
        {
            GraphicalException g = exceptions[coord];
            g.time -= HardwareInterface.deltaTime;
            if(g.time<=0)
            {
                toRemove.Push(coord);
            }
        }
        while(toRemove.Count>0)
        {
            exceptions.Remove(toRemove.Pop());
        }

        frames++;
        if (stateStack.Count > 0)
        {
            GameState.startMenu();
            GameState top = stateStack[stateStack.Count - 1];
            if (!(top is ShopState || getFlag("ads")))
            {
                for(int i = 0; i < 3; i++)
                {
                    if(adStates[i]>0)
                    {
                        adStates[i] -= HardwareInterface.deltaTime;
                    } else
                    {
                        if(HardwareInterface.deltaTime>rand.NextDouble()*(top is TitleScreen?10:50))
                        {
                            adStates[i] = 5;
                        }
                    }
                }
            }
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
        if (!(stateStack[stateStack.Count - 1] is ShopState || getFlag("ads")))
        {
            if(adStates[0]>0)
            {
                Graphics.draw(Graphics.ads[0], 16, 16, Graphics.ads[0].Width,Graphics.ads[0].Height,Graphics.spriteDefault);
            }
            if (adStates[1] > 0&&!(stateStack[stateStack.Count-1] is Textbox))
            {
                Graphics.draw(Graphics.ads[1], 16, Graphics.HEIGHT-Graphics.ads[1].Height-16, Graphics.ads[1].Width, Graphics.ads[1].Height, Graphics.spriteDefault);
            }
            if (adStates[2] > 0)
            {
                if(Mathf.FloorToInt(HardwareInterface.timeSinceLevelLoad*2)%2==0)
                    Graphics.draw(Graphics.ads[2], Graphics.WIDTH-Graphics.ads[2].Width-16, 16, Graphics.ads[2].Width, Graphics.ads[2].Height, Graphics.spriteDefault);
                else
                    Graphics.drawRect(Color.Red, Graphics.WIDTH - Graphics.ads[2].Width - 16, 16, Graphics.ads[2].Width, Graphics.ads[2].Height);
            }
        }
    }
}

public class TitleScreen : GameState
{
    public TitleScreen() : base()
    {

    }

    public override void draw()
    {
        Graphics.draw(Graphics.titlescreen, (Graphics.WIDTH - Graphics._WIDTH) / 2, 0, Graphics._WIDTH, Graphics.HEIGHT, Graphics.spriteDefault);
    }

    public override void run()
    {
        //Sound.setMusic(Sound.baseSong);
        if((getA()&&!a))
        {
            Core.instance.stateStack.Remove(this);
            //Core.instance.stateStack.Push(new State());
        }
    }
}