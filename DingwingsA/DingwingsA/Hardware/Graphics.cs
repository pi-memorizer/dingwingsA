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
        private const int _WIDTH = 320, _HEIGHT = 180;
        public static int WIDTH = _WIDTH, HEIGHT = _HEIGHT;
        static bool lockedMaterial = false;
        static int currentVertex = 0;
        static Matrix cameraMatrix;
        static RenderTarget2D backbuffer, backbufferGUI;
        static int _width, _height;

        public static float xOffset = 0, yOffset = 0;
        public static Effect solidColor, spriteDefault;
        public static List<Texture2D> tilesets;
        public static Texture2D bitmapFont;
        public static Dictionary<char, GameSprite> charSprites = new Dictionary<char, GameSprite>();
        public static int scale = 1;
        public static int CHAR_SIZE = 8;

        public static GameSprite[] tileset = new GameSprite[256 * 32];

        public static void init()
        {
            graphics.PreferredBackBufferHeight = 4 * HEIGHT;
            graphics.PreferredBackBufferWidth = 4 * WIDTH;
            graphics.ApplyChanges();
        }

        public static void loadContent(ContentManager content)
        {
            bitmapFont = content.Load<Texture2D>("images/charset");
            tilesets = new List<Texture2D>();
            for (int i = 1; i <= 0; i++)
            {
                tilesets.Add(content.Load<Texture2D>("tileset" + i));
            }

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
            backbuffer.Dispose();
            backbufferGUI.Dispose();
        }

        static void adjustCamera()
        {
            if (_width != graphics.PreferredBackBufferWidth || _height != graphics.PreferredBackBufferHeight)
            {
                _width = graphics.PreferredBackBufferWidth;
                _height = graphics.PreferredBackBufferHeight;
                WIDTH = _WIDTH;
                HEIGHT = _HEIGHT;

                scale = graphics.PreferredBackBufferWidth / WIDTH;
                if (graphics.PreferredBackBufferHeight / HEIGHT < scale) scale = graphics.PreferredBackBufferHeight / HEIGHT;
                if (scale < 2) scale = 2;

                xOffset = (graphics.PreferredBackBufferWidth - scale * WIDTH) / 2F;
                yOffset = (graphics.PreferredBackBufferHeight - scale * HEIGHT) / 2F;
                xOffset /= scale;
                yOffset /= scale;
            }
        }

        public static void handleFrame(Core core)
        {
            if (core.stateStack.Count == 0) return;
            if (core.stateStack.Peek().highGraphicsMode)
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
            
            int currentWidth = scale * (Mathf.CeilToInt(xOffset) * 2 + WIDTH);
            int currentHeight = scale * (Mathf.CeilToInt(yOffset) * 2 + HEIGHT);
            if(backbuffer==null||backbuffer.Width!=currentWidth||backbuffer.Height!=currentHeight)
            {
                if (backbuffer != null) backbuffer.Dispose();
                backbuffer = new RenderTarget2D(graphicsDevice, currentWidth, currentHeight,false,SurfaceFormat.Color,DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            }
            graphicsDevice.SetRenderTarget(backbuffer);
            clear(new Color(0));
            try
            {
                core.draw();
            }
            catch (System.Exception e) { Debug.WriteLine(e + "/" + e.StackTrace); }

            cameraMatrix = Matrix.CreateOrthographicOffCenter(0, WIDTH, HEIGHT, 0, -2 * HEIGHT, 2 * HEIGHT);
            if (backbufferGUI == null || backbufferGUI.Width != WIDTH * scale || backbufferGUI.Height != HEIGHT * scale)
            {
                if (backbufferGUI != null) backbufferGUI.Dispose();
                backbufferGUI = new RenderTarget2D(graphicsDevice, WIDTH * scale, HEIGHT * scale,false,SurfaceFormat.Color,DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }
            graphicsDevice.SetRenderTarget(backbufferGUI);
            clear(new Color(0));
            try
            {
                core.gui();
            }
            catch (System.Exception e) { Debug.WriteLine(e + "/" + e.StackTrace); }

            graphicsDevice.SetRenderTarget(null);

            cameraMatrix = Matrix.CreateOrthographicOffCenter(-xOffset, WIDTH + xOffset, HEIGHT + yOffset, -yOffset, -2, 2);
            draw(backbuffer, -xOffset, -yOffset, WIDTH + Mathf.CeilToInt(xOffset) * 2, HEIGHT + Mathf.CeilToInt(yOffset) * 2);
            draw(backbufferGUI, 0, 0, WIDTH, HEIGHT);
            
        }

        public static void clear(Color color)
        {
            graphicsDevice.Clear(color);
        }

        public static void draw(GameSprite s, float x, float y, int r = 255, int g = 255, int b = 255)
        {
            Effect effect = spriteDefault;
            if (!lockedMaterial)
            {
                effect.Parameters["s0"].SetValue(s.texture);
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
            
            verts[currentVertex].Position = new Vector3(x, y + s.pixelHeight, 0);
            verts[currentVertex].TextureCoordinate = new Vector2(s.x, s.y+s.height);
            verts[currentVertex + 1].Position = new Vector3(x, y, 0);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(s.x, s.y);
            verts[currentVertex + 2].Position = new Vector3(x + s.pixelWidth, y, 0);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(s.x+s.width, s.y);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + s.pixelWidth, y + s.pixelHeight, 0);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(s.x+s.width, s.y+s.height);
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

        public static void draw3D(GameSprite s, float x, float y, Effect e, int lowerOffset = 0, int upperOffset = 0, int r = 255, int g = 255, int b = 255, int scale = 1)
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

            verts[currentVertex].Position = new Vector3(x, y + s.pixelHeight*scale, y + s.pixelHeight * scale + lowerOffset);
            verts[currentVertex].TextureCoordinate = new Vector2(s.x, s.y + s.height);
            verts[currentVertex + 1].Position = new Vector3(x, y, y + upperOffset);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(s.x, s.y);
            verts[currentVertex + 2].Position = new Vector3(x + s.pixelWidth*scale, y, y + upperOffset);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(s.x + s.width, s.y);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + s.pixelWidth*scale, y + s.pixelHeight*scale, y + s.pixelHeight * scale + lowerOffset);
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

        public static void drawGrass(GameSprite s, float x, float y, int lowerOffset = 0, int upperOffset = 0)
        {
            if (currentVertex + 6 >= verts.Length)
            {
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, currentVertex / 3);
                currentVertex = 0;
            }

            Vector2 wind = new Vector2(Mathf.Sin(HardwareInterface.timeSinceLevelLoad+x/100+y/100),0);
            wind = wind * .5F + new Vector2(.5F,.5F);

            verts[currentVertex].Position = new Vector3(x-1, y + s.pixelHeight, y + s.pixelHeight + lowerOffset);
            verts[currentVertex].TextureCoordinate = new Vector2(s.x-1/256F, s.y + s.height);
            verts[currentVertex].Color = new Color(wind.X, wind.Y, 0);
            verts[currentVertex + 1].Position = new Vector3(x-1, y, y + upperOffset);
            verts[currentVertex + 1].TextureCoordinate = new Vector2(s.x-1/256F, s.y);
            verts[currentVertex + 1].Color = new Color(wind.X, wind.Y, 1);
            verts[currentVertex + 2].Position = new Vector3(x + s.pixelWidth+1, y, y + upperOffset);
            verts[currentVertex + 2].TextureCoordinate = new Vector2(s.x + s.width + 1 / 256F, s.y);
            verts[currentVertex + 2].Color = new Color(wind.X, wind.Y, 1);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + s.pixelWidth+1, y + s.pixelHeight, y + s.pixelHeight + lowerOffset);
            verts[currentVertex + 4].TextureCoordinate = new Vector2(s.x + s.width+1/256F, s.y + s.height);
            verts[currentVertex + 4].Color = new Color(wind.X, wind.Y, 0);
            verts[currentVertex + 5] = verts[currentVertex];
            
            currentVertex += 6;
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

            //For stuff like drawIntro needing to be called from draw() instead of gui()
            float l = 2 * HEIGHT - .01F;
            verts[currentVertex].Position = new Vector3(x, y + height, l);
            verts[currentVertex + 1].Position = new Vector3(x, y, l);
            verts[currentVertex + 2].Position = new Vector3(x + width, y, l);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width, y + height, l);
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

        public static void drawRect3D(Color c, float x, float y, int width, int height, int lowerOffset = 0, int upperOffset = 0, bool flip = false)
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

            verts[currentVertex].Position = new Vector3(x, y + height * scale, y + height * scale + lowerOffset);
            verts[currentVertex + 1].Position = new Vector3(x, y, y + upperOffset);
            verts[currentVertex + 2].Position = new Vector3(x + width * scale, y, y + upperOffset);

            verts[currentVertex + 3] = verts[currentVertex + 2];
            verts[currentVertex + 4].Position = new Vector3(x + width * scale, y + height * scale, y + height * scale + lowerOffset);
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

        public static void drawCharacter(char c, float x, float y, int r, int g, int b)
        {
            if (c < 256)
            {
                draw(charSprites[c], x, y, r, g, b);
                return;
            }
            //TODO non-ascii characters
        }

        public static void drawCharacterHalf(char c, float x, float y, int r, int g, int b)
        {
            if (c < 256)
            {
                GameSprite s = charSprites[c];
                s.pixelHeight /= 2;
                s.pixelWidth /= 2;

                draw(s, x, y, r, g, b);

                s.pixelHeight *= 2;
                s.pixelWidth *= 2;
                return;
            }
            //TODO non-ascii characters
        }

        public static void drawString(string s, float x, float y, int r = 0, int g = 0, int b = 0)
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
                lockTextMaterial(new Color(r, g, b));
                dx = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    drawCharacter(s[i], x + dx, y, r, g, b);
                    dx += CHAR_SIZE;
                }
                unlockMaterial();
                return;
            }
            //TODO non-ascii characters
        }

        public static void drawStringHalf(string s, float x, float y, int r = 0, int g = 0, int b = 0)
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
                lockTextMaterial(new Color(r, g, b));
                dx = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    drawCharacterHalf(s[i], x + dx, y, r, g, b);
                    dx += CHAR_SIZE / 2;
                }
                unlockMaterial();
                return;
            }
            //TODO non-ascii characters
        }

        public static void drawStringRight(string s, float x, float y, int r = 0, int g = 0, int b = 0)
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
                lockTextMaterial(new Color(r, g, b));
                dx = 0;
                for (int i = s.Length - 1; i >= 0; i--)
                {
                    dx -= CHAR_SIZE;
                    drawCharacter(s[i], x + dx, y, r, g, b);
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
}
