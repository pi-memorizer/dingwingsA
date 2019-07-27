using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using jobject = System.Collections.Generic.Dictionary<string, object>;
using jarray = System.Collections.Generic.List<object>;
using Microsoft.Xna.Framework;
using Hardware;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

public class TiledWorld : World
{
    private class Tileset
    {
        public int firstgid, index;
    }
    class Chunk
    {
        public struct Tile
        {
            public byte id, x, y;
            public short lower, upper;
        }
        public byte[] collision = new byte[CHUNK_SIZE*CHUNK_SIZE];
        //the int is which of the 256 tilemaps we're using
        public Dictionary<int, List<Tile>> tiles = new Dictionary<int, List<Tile>>();
    }
    private Dictionary<Coord, Chunk> chunks = new Dictionary<Coord, Chunk>();
    private jobject objects = new jobject();
    private int xOffset = 100000, yOffset = 100000;
    private int width, height;
    private int defaultBlock;
    private const int CHUNK_SIZE = 16;
    public TiledWorld(string name, InitLevel initLevel, int defaultBlock = 0) : base(name,initLevel)
    {
        this.defaultBlock = defaultBlock;
        string pathToJson = HardwareInterface.instance.Content.RootDirectory + "/maps/" + name + ".json";
        loadFromJson(pathToJson);
    }
    
    private object findProperty(jobject o, string name, object defaultValue)
    {
        if (!o.ContainsKey("properties")) return defaultValue;
        foreach(jobject j in (jarray)o["properties"])
        {
            if((string)j["name"]==name)
            {
                return j["value"];
            }
        }
        return defaultValue;
    }

    private bool loadFromJson(string path)
    {
        try
        {
            jobject json = Json.jsonObject(File.ReadAllText(path));
            List<Tileset> tilesets = new List<Tileset>();
            Tileset collision = null;
            foreach (object _ in (jarray)json["tilesets"])
            {
                jobject tileset = _ as jobject;
                Tileset t = new Tileset();
                t.firstgid = (int)tileset["firstgid"];
                int index = 0;
                string path2 = (string)tileset["source"];
                if (path2.Contains("tileset"))
                {
                    path2 = path2.Substring(path2.IndexOf("tileset") + 7);
                    path2 = path2.Substring(0, path2.Length - 4);
                    index = int.Parse(path2) - 1;
                    t.index = index;

                    //Console.WriteLine("Found tileset " + index + " starting at gid " + t.firstgid);
                }
                else
                {
                    t.index = 0;
                    collision = t;
                }
                tilesets.Add(t);
            }
            //find width and height
            foreach (jobject layer in (jarray)json["layers"])
            {
                if ((string)layer["type"] != "tilelayer") continue;
                if (xOffset > (int)layer["startx"])
                    xOffset = (int)layer["startx"];
                if (yOffset > (int)layer["starty"])
                    yOffset = (int)layer["starty"];
                if (width < (int)layer["width"])
                    width = (int)layer["width"];
                if (height < (int)layer["height"])
                    height = (int)layer["height"];
            }

            foreach(jobject layer in (jarray)json["layers"])
            {
                if ((string)layer["type"] != "tilelayer")
                {
                    if((string)layer["type"]=="objectgroup")
                    {
                        foreach(jobject o in (jarray)layer["objects"])
                        {
                            objects.Add((string)o["name"], o);
                        }
                    }
                    continue;
                }

                int lower = (int)findProperty(layer, "lower", 0);
                int upper = (int)findProperty(layer, "upper", 0);

                foreach(jobject chunk in (jarray)layer["chunks"])
                {
                    int startx = (int)chunk["x"];
                    int starty = (int)chunk["y"];
                    jarray data = (jarray)chunk["data"];
                    bool isCollision = (string)layer["name"] == "Collision";
                    Chunk c;
                    Coord coord = new Coord(Core.safeDiv(startx, CHUNK_SIZE), Core.safeDiv(starty, CHUNK_SIZE));
                    if (!chunks.TryGetValue(coord,out c))
                    {
                        c = new Chunk();
                        chunks.Add(coord, c);
                    }
                    for(int y = 0; y < CHUNK_SIZE; y++)
                    {
                        for (int x = 0; x < CHUNK_SIZE; x++)
                        {
                            ushort value = (ushort)(int)data[x + CHUNK_SIZE * y];
                            if(!isCollision)
                            {
                                foreach (Tileset t in tilesets)
                                {
                                    if (value >= t.firstgid && value < t.firstgid + 256)
                                    {
                                        value = (ushort)(value - t.firstgid + t.index * 256);
                                        Chunk.Tile tile = new Chunk.Tile();
                                        tile.id = (byte)(value & 0xFF);
                                        tile.x = (byte)x;
                                        tile.y = (byte)y;
                                        tile.lower = (short)lower;
                                        tile.upper = (short)upper;
                                        List<Chunk.Tile> list;
                                        if(!c.tiles.TryGetValue(t.index,out list))
                                        {
                                            list = new List<Chunk.Tile>();
                                            c.tiles.Add(t.index, list);
                                        }
                                        list.Add(tile);
                                        break;
                                    }
                                }
                            } else
                            {
                                if(value!=0)
                                {
                                    c.collision[x + CHUNK_SIZE * y] = (byte)(value - collision.firstgid);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine("Trouble loading Tiled map " + name);
            Debug.WriteLine(e.Message + "\n" + e.StackTrace);
            return false;
        }
    }

    public override object get(string id, string property, object defaultValue = null)
    {
        if (!objects.ContainsKey(id)) return defaultValue;
        jobject o = (jobject)objects[id];
        if (o.ContainsKey(property)) return o[property];
        jobject p = (jobject)((jarray)o["properties"]).Find(a => (string)((jobject)a)["name"] == property);
        if (p == null) return defaultValue;
        return p["value"];
    }

    public override int getCoord(string id, string axis)
    {
        object o = get(id, axis);
        if (o == null) return 0;
        float f = o is float ? (float)o : (int)o;
        return Core.safeDiv(f, Core.TILE_SIZE);
    }

    public override int getCollision(int x, int y)
    {
        if (x >= xOffset && y >= yOffset && x < xOffset + width && y < yOffset + height)
        {
            Coord c = new Coord(Core.safeDiv(x, CHUNK_SIZE), Core.safeDiv(y, CHUNK_SIZE));
            Chunk chunk;
            if (chunks.TryGetValue(c, out chunk))
            {
                return chunk.collision[Core.safeMod(x, CHUNK_SIZE) + CHUNK_SIZE * Core.safeMod(y, CHUNK_SIZE)];
            }
            else return 0;
        }
        else return 0;
    }

    public override void draw()
    {
        int w = (int)(HardwareInterface.timeSinceLevelLoad) % 4;
        if (w == 3) w = 1;
        int x1 = Core.safeDiv(Core.p.getCameraX() - Graphics.WIDTH / 2 - Graphics.xOffset-Core.TILE_SIZE, CHUNK_SIZE*Core.TILE_SIZE)-1;
        int x2 = Core.safeDiv(Core.p.getCameraX() + Graphics.WIDTH / 2 + Graphics.xOffset+Core.TILE_SIZE, CHUNK_SIZE*Core.TILE_SIZE)+1;
        int y1 = Core.safeDiv(Core.p.getCameraY() - Graphics.HEIGHT / 2 - Graphics.yOffset-Core.TILE_SIZE, CHUNK_SIZE*Core.TILE_SIZE)-1;
        int y2 = Core.safeDiv(Core.p.getCameraY() + Graphics.HEIGHT / 2 + Graphics.yOffset+Core.TILE_SIZE, CHUNK_SIZE*Core.TILE_SIZE)+1;
        for(int x = x1; x <= x2; x++)
        {
            for(int y = y1; y <= y2; y++)
            {
                Chunk c;
                if(!chunks.TryGetValue(new Coord(x,y),out c))
                {
                    continue;
                }
                foreach(var pair in c.tiles)
                {
                    var list = pair.Value;
                    
                    Graphics.lockMaterial(Graphics.tilesets[pair.Key]);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Chunk.Tile tile = list[i];
                        Graphics.draw(Graphics.tileset[pair.Key * 256 + tile.id], Core.getOnscreenX(Core.TILE_SIZE * (tile.x + x * CHUNK_SIZE)), Core.getOnscreenY(Core.TILE_SIZE * (tile.y + y * CHUNK_SIZE)), null);
                    }
                    Graphics.unlockMaterial();
                }
            }
        }
    }
}