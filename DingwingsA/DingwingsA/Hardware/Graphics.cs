using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace Hardware
{
    class Graphics
    {
        internal static GraphicsDeviceManager graphics;
        internal static GraphicsDevice graphicsDevice;
        static VertexPositionColorTexture[] verts = new VertexPositionColorTexture[2000];
        public const int _WIDTH = 640, _HEIGHT = 360;
        public static int WIDTH = _WIDTH, HEIGHT = _HEIGHT;
        static bool lockedMaterial = false;
        static int currentVertex = 0;
        static Matrix cameraMatrix;
        static RenderTarget2D backbufferGUI;

        public static float xOffset = 0, yOffset = 0;
        public static Effect solidColor, spriteDefault;
        public static List<Texture2D> tilesets;
        public static Texture2D bitmapFont;
        public static Dictionary<char, GameSprite> charSprites = new Dictionary<char, GameSprite>();
        public static float scale = 1;
        public const int CHAR_SIZE = 16;

        public static GameSprite[] tileset = new GameSprite[256 * 32];
        public static Texture2D[] slime32sheet;
        public static GameSprite[] slime32;
        public static Texture2D[] backgrounds = new Texture2D[3];
        public static Texture2D[] ads = new Texture2D[7];
        public static Texture2D shop, newItem;
        public static Texture2D buttonHighlight, buttonDisabled, buttonActive;
        public static Texture2D titlescreen, shopCarrot, medbill;
        public static List<Particle> particles = new List<Particle>();
        

        public static void init()
        {
            graphics.PreferredBackBufferHeight = HEIGHT;
            graphics.PreferredBackBufferWidth = WIDTH;
            //graphics.ApplyChanges();
        }

        public static void loadContent(ContentManager content)
        {
            bitmapFont = content.Load<Texture2D>("images/charset");
            tilesets = new List<Texture2D>();
            for (int i = 1; i <= 1; i++)
            {
                tilesets.Add(content.Load<Texture2D>("images/tileset" + i+"-tiny"));
                tilesets.Add(content.Load<Texture2D>("images/tileset" + i+"-small"));
                tilesets.Add(content.Load<Texture2D>("images/tileset" + i));
            }
            slime32sheet = new Texture2D[3];
            slime32sheet[0] = content.Load<Texture2D>("images/slime-sheet-tiny");
            slime32sheet[1] = content.Load<Texture2D>("images/slime-sheet-small");
            slime32sheet[2] = content.Load<Texture2D>("images/slime-sheet");
            slime32 = new GameSprite[24];
            for(int i = 0; i < 24; i++)
            {
                slime32[i] = new GameSprite(slime32sheet[i/8], 32 * (i % 4), 32 * ((i%8) / 4), 32, 32);
            }
            for(int i = 0; i < 3; i++)
            {
                backgrounds[i] = content.Load<Texture2D>("images/background" + (i + 1));
            }
            ads[0] = content.Load<Texture2D>("images/single-slimes");
            ads[1] = content.Load<Texture2D>("images/dr hate him");
            ads[2] = content.Load<Texture2D>("images/1000player");
            ads[3] = content.Load<Texture2D>("images/1000player-blink");
            ads[4] = content.Load<Texture2D>("images/click");
            ads[5] = content.Load<Texture2D>("images/fire");
            ads[6] = content.Load<Texture2D>("images/shopping");
            shop = content.Load<Texture2D>("images/shop");
            buttonActive = content.Load<Texture2D>("images/active-button");
            buttonDisabled = content.Load<Texture2D>("images/disabled-button");
            buttonHighlight = content.Load<Texture2D>("images/button-highlight");
            newItem = content.Load<Texture2D>("images/new-item");
            titlescreen = content.Load<Texture2D>("images/titlescreen");
            medbill = content.Load<Texture2D>("images/medbill");
            shopCarrot = content.Load<Texture2D>("images/shop-carrot");

            //shaders
            spriteDefault = content.Load<Effect>("shaders/spriteDefault");
            solidColor = content.Load<Effect>("shaders/solidShader");
            
            for (int i = 0; i < tilesets.Count; i++)
            {
                fillTileset(i, 0, 0);
            }
            for(int i = 0; i < 256; i++)
            {
                charSprites.Add((char)i, new GameSprite(bitmapFont, (i % 16)*8, (i / 16) * 8, 8, 8));
            }

        }

        private static void fillTileset(int set, int xOffset, int yOffset)
        {
            Texture2D t = tilesets[set];
            int width = t.Width / 16;
            int height = t.Height / 16;
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    tileset[x + y * 16 + 256 * set] = new GameSprite(t, width * x, height * y, width, height, xOffset, yOffset);
                }
            }
        }

        public static void unloadContent()
        {
            backbufferGUI.Dispose();
        }

        static void adjustCamera()
        {
            //if (_width != graphics.PreferredBackBufferWidth || _height != graphics.PreferredBackBufferHeight)
            {
                HEIGHT = _HEIGHT;

                scale = (float)graphics.PreferredBackBufferHeight / _HEIGHT;
                
                WIDTH = Mathf.RoundToInt(graphics.PreferredBackBufferWidth / scale);

                /*xOffset = (graphics.PreferredBackBufferWidth - scale * WIDTH) / 2F;
                yOffset = (graphics.PreferredBackBufferHeight - scale * HEIGHT) / 2F;
                xOffset /= scale;
                yOffset /= scale;*/
            }
        }

        public static void handleFrame(Core core)
        {
            if (core.stateStack.Count == 0) return;
            if (core.stateStack[core.stateStack.Count-1].highGraphicsMode)
            {
                try
                {
                    core.draw();
                }
                catch (System.Exception e) { Debug.WriteLine(e+"/"+e.StackTrace); }
                return;
            }
            adjustCamera();
            if (scale == 0)
            {
                return;
            }

            cameraMatrix = Matrix.CreateOrthographicOffCenter(-Mathf.CeilToInt(xOffset), WIDTH + Mathf.CeilToInt(xOffset), HEIGHT + Mathf.CeilToInt(yOffset), -Mathf.CeilToInt(yOffset), -2 * HEIGHT, 2 * HEIGHT);
            
            int currentWidth = graphics.PreferredBackBufferWidth;
            int currentHeight = graphics.PreferredBackBufferHeight;
            if (backbufferGUI == null || backbufferGUI.Width != currentWidth || backbufferGUI.Height != currentHeight)
            {
                if (backbufferGUI != null) backbufferGUI.Dispose();
                backbufferGUI = new RenderTarget2D(graphicsDevice, currentWidth, currentHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }
            graphicsDevice.SetRenderTarget(backbufferGUI);
            graphicsDevice.Clear(Color.Black);
            setFilter(false);
            try
            {
                core.draw();
            }
            catch (System.Exception e) { Debug.WriteLine(e + "/" + e.StackTrace); }

            graphicsDevice.SetRenderTarget(null);
            
            cameraMatrix = Matrix.CreateOrthographicOffCenter(-xOffset, WIDTH + xOffset, HEIGHT + yOffset, -yOffset, -2, 2);
            draw(backbufferGUI, -xOffset, -yOffset, WIDTH + Mathf.CeilToInt(xOffset) * 2, HEIGHT + Mathf.CeilToInt(yOffset) * 2);
            
        }

        public static void setFilter(bool affected)
        {
            Color c = Color.White;
            if (affected)
                c = new Color(Core.getFlag("red")?1F:0F,Core.getFlag("green")?1F:0F,Core.getFlag("blue")?1F:0F);
            spriteDefault.Parameters["Filter"].SetValue(c.ToVector4());
            solidColor.Parameters["Filter"].SetValue(c.ToVector4());
        }

        public static void clear(Color color)
        {
            graphicsDevice.Clear(color);
        }

        public static void draw(GameSprite s, float x, float y, int width, int height, bool flipped = false)
        {
            Effect effect = spriteDefault;
            if (!lockedMaterial)
            {
                effect.Parameters["s0"].SetValue(s.texture);
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(Color.White.ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            x = Mathf.FloorToInt(x);
            y = Mathf.FloorToInt(y);

            verts[currentVertex].Position = new Vector3(x, y + height, 0);
            verts[currentVertex].TextureCoordinate = new Vector2(s.x+(flipped?s.width:0), s.y+s.height);
            verts[currentVertex + 1].Position = new Vector3(x, y, 0);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(s.x + (flipped ? s.width : 0), s.y);
            verts[currentVertex + 2].Position = new Vector3(x + width, y, 0);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(s.x + (flipped ? 0 : s.width), s.y);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width, y + height, 0);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(s.x + (flipped ? 0 : s.width), s.y+s.height);
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }

        public static void draw(RenderTarget2D rt, float x, float y, int width, int height)
        {
            Effect effect = spriteDefault;
            if (!lockedMaterial)
            {
                effect.Parameters["s0"].SetValue(rt);
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(Color.White.ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            bool flipX = false, flipY = false;
            verts[currentVertex].Position = new Vector3(x, y + height, 0);
            verts[currentVertex].TextureCoordinate = new Vector2(flipX ? 1 : 0, flipY ? 0 : 1);
            verts[currentVertex + 1].Position = new Vector3(x, y, 0);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(flipX ? 1 : 0, flipY ? 1 : 0);
            verts[currentVertex + 2].Position = new Vector3(x + width, y, 0);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(flipX ? 0 : 1, flipY ? 1 : 0);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width, y + height, 0);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(flipX ? 0 : 1, flipY ? 0 : 1);
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }

        public static void lockMaterial(Texture2D t)
        {
            Effect effect = spriteDefault;
            effect.Parameters["s0"].SetValue(t);
            effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
            effect.Parameters["Color"].SetValue(Color.White.ToVector4());
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            effect.CurrentTechnique.Passes[0].Apply();
            lockedMaterial = true;
        }

        public static void lockMaterial(Texture2D t, Color c)
        {
            Effect effect = spriteDefault;
            effect.Parameters["s0"].SetValue(t);
            effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
            effect.Parameters["Color"].SetValue(c.ToVector4());
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            effect.CurrentTechnique.Passes[0].Apply();
            lockedMaterial = true;
        }

        public static void lockMaterial(Color c)
        {
            Effect effect = solidColor;
            effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
            effect.Parameters["Color"].SetValue(Color.White.ToVector4());
            effect.Parameters["Filter"].SetValue(Color.White.ToVector4());
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            effect.CurrentTechnique.Passes[0].Apply();
            lockedMaterial = true;
        }

        public static void lockTextMaterial(Color color)
        {
            Effect effect = spriteDefault;
            effect.Parameters["s0"].SetValue(bitmapFont);
            effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
            effect.Parameters["Color"].SetValue(color.ToVector4());
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            effect.CurrentTechnique.Passes[0].Apply();
            lockedMaterial = true;
        }

        public static void lockMaterial(Effect e)
        {
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
            e.CurrentTechnique.Passes[0].Apply();
            lockedMaterial = true;
        }

        public static void unlockMaterial()
        {
            if(currentVertex!=0) graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
            currentVertex = 0;
            lockedMaterial = false;
        }

        public static void draw(GameSprite s, float x, float y, Effect e, int r = 255, int g = 255, int b = 255, int scale = 1)
        {
            Effect effect = e;
            if (!lockedMaterial)
            {
                effect.Parameters["s0"].SetValue(s.texture);
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(new Color(r, g, b).ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }
            
            x = Mathf.FloorToInt(x);
            y = Mathf.FloorToInt(y);

            verts[currentVertex].Position = new Vector3(x, y + s.pixelHeight*scale, 0);
            verts[currentVertex].TextureCoordinate = new Vector2(s.x, s.y + s.height);
            verts[currentVertex + 1].Position = new Vector3(x, y, 0);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(s.x, s.y);
            verts[currentVertex + 2].Position = new Vector3(x + s.pixelWidth*scale, y, 0);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(s.x + s.width, s.y);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + s.pixelWidth*scale, y + s.pixelHeight*scale, 0);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(s.x + s.width, s.y + s.height);
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }
        
        public static void draw(Texture t, float x, float y, int width, int height, Effect e, int r = 255, int g = 255, int b = 255)
        {
            Effect effect = e;
            if (!lockedMaterial)
            {
                effect.Parameters["s0"].SetValue(t);
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(new Color(r,g,b).ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            x = Mathf.FloorToInt(x);
            y = Mathf.FloorToInt(y);

            //for drawIntro?
            float l = 2 * HEIGHT - 1;
            verts[currentVertex].Position = new Vector3(x, y + height, l);
            verts[currentVertex].TextureCoordinate = new Vector2(0, 1);
            verts[currentVertex + 1].Position = new Vector3(x, y, l);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(0, 0);
            verts[currentVertex + 2].Position = new Vector3(x + width, y, l);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(1, 0);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width, y + height, l);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(1, 1);
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }

        public static void drawParallax(Texture t, float x, float amount)
        {
            Effect effect = spriteDefault;
            if (!lockedMaterial)
            {
                effect.Parameters["s0"].SetValue(t);
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(Color.White.ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            float left = x * amount;
            float right = left + (float)WIDTH / _WIDTH / 2;
            
            float l = 2 * HEIGHT - 1;
            verts[currentVertex].Position = new Vector3(0, HEIGHT, l);
            verts[currentVertex].TextureCoordinate = new Vector2(left, 1);
            verts[currentVertex + 1].Position = new Vector3(0, 0, l);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(left, 0);
            verts[currentVertex + 2].Position = new Vector3(WIDTH, 0, l);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(right, 0);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(WIDTH, HEIGHT, l);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(right, 1);
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }

        public static void drawRect(Color c, float x, float y, int width, int height)
        {
            Effect effect = solidColor;
            if (!lockedMaterial)
            {
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(c.ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            x = Mathf.FloorToInt(x);
            y = Mathf.FloorToInt(y);

            //For stuff like drawIntro needing to be called from draw() instead of gui()
            float l = 2 * HEIGHT - .01F;
            verts[currentVertex].Position = new Vector3(x, y + height, l);
            verts[currentVertex + 1].Position = new Vector3(x, y, l);
            verts[currentVertex + 2].Position = new Vector3(x + width, y, l);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width, y + height, l);
            verts[currentVertex + 5] = verts[currentVertex];

            verts[currentVertex].Color = Color.White;
            verts[currentVertex + 1].Color = Color.White;
            verts[currentVertex + 2].Color = Color.White;

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Color = Color.White;
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }

        public static void drawParticle(Color c, float x, float y, int width, int height)
        {
            Effect effect = solidColor;
            if (!lockedMaterial)
            {
                effect.Parameters["WorldViewProjection"].SetValue(cameraMatrix);
                effect.Parameters["Color"].SetValue(Color.White.ToVector4());
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
                effect.CurrentTechnique.Passes[0].Apply();
            }
            else if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            x = Mathf.FloorToInt(x);
            y = Mathf.FloorToInt(y);

            //For stuff like drawIntro needing to be called from draw() instead of gui()
            float l = 2 * HEIGHT - .01F;
            verts[currentVertex].Position = new Vector3(x, y + height, l);
            verts[currentVertex + 1].Position = new Vector3(x, y, l);
            verts[currentVertex + 2].Position = new Vector3(x + width, y, l);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width, y + height, l);
            verts[currentVertex + 5] = verts[currentVertex];

            verts[currentVertex].Color = c;
            verts[currentVertex + 1].Color = c;
            verts[currentVertex + 2].Color = c;

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Color = c;
            verts[currentVertex + 5] = verts[currentVertex];

            if (!lockedMaterial)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
                currentVertex = 0;
            }
            else
            {
                currentVertex += 6;
            }
        }

        public static void drawCharacter(char c, float x, float y, int scale = 1)
        {
            if (c < 256)
            {
                draw(charSprites[c], x, y,CHAR_SIZE*scale,CHAR_SIZE*scale);
                return;
            }
            //TODO non-ascii characters
        }

        public static void drawString(string s, float x, float y, int maxLength = 1000000)
        {
            int dx = 0;

            bool allAscii = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] >= 256)
                {
                    allAscii = false;
                    break;
                }
            }

            if (allAscii)
            {
                //lockTextMaterial(new Color(r, g, b));
                lockTextMaterial(Color.Black);
                dx = 0;
                for (int i = 0; i < s.Length && i < maxLength; i++)
                {
                    drawCharacter(s[i], x + dx, y);
                    dx += CHAR_SIZE;
                }
                unlockMaterial();
                return;
            }
            //TODO non-ascii characters
        }

        public static void drawStringRight(string s, float x, float y, int scale = 1)
        {
            int dx = 0;
            bool allAscii = true;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] >= 256)
                {
                    allAscii = false;
                    break;
                }
            }

            if (allAscii)
            {
                lockTextMaterial(Color.Black);
                dx = 0;
                for (int i = s.Length - 1; i >= 0; i--)
                {
                    dx -= CHAR_SIZE*scale;
                    drawCharacter(s[i], x + dx, y, scale);
                }
                unlockMaterial();
                return;
            }
            //TODO non-ascii characters
        }

        public static int getCharWidth(char c)
        {
            return CHAR_SIZE;
            //TODO non-ascii characters
        }

        public static int getStringWidth(string s)
        {
            int x = 0;
            for (int i = 0; i < s.Length; i++) x += getCharWidth(s[i]);
            return x;
        }

        internal static void renderChars(List<char> toRender)
        {
            throw new NotImplementedException();
        }
    }

    class Particle
    {
        public float x, y, vx, vy;
        public Color color;
        public bool isMoney;

        public Particle(Color color, float x, float y, float vx, float vy, bool isMoney = false)
        {
            this.isMoney = isMoney;
            this.color = color;
            this.x = x;
            this.y = y;
            this.vx = vx;
            this.vy = vy;
        }
    }

}
