using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using Hardware;

namespace Hardware
{
    public class GameSprite
    {
        public float x, y, width, height;
        public Texture2D texture;
        public int pixelWidth, pixelHeight;
        public int xOffset = 0, yOffset = 0;
        public GameSprite(Texture2D texture, int x, int y, int width, int height, int xOffset = 0, int yOffset = 0) : this(texture, texture.Width, texture.Height, x, y, width, height, xOffset, yOffset)
        { }

        public GameSprite(Texture2D texture, float textureWidth, float textureHeight, int x, int y, int width, int height, int xOffset = 0, int yOffset = 0)
        {
            this.texture = texture;
            this.x = (x) / textureWidth;
            this.y = y / textureHeight;
            this.width = width / textureWidth;
            this.height = height / textureHeight;
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            pixelHeight = height;
            pixelWidth = width;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + width + "," + height + ")";
        }

        public void draw(int x, int y, int layer = 0)
        {
            Graphics.draw(this, x + xOffset, y + yOffset);
        }
    }
}