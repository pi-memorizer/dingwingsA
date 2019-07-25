using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware
{
    static class Mathf
    {
        public static int FloorToInt(float val)
        {
            return (int)Math.Floor(val);
        }

        public static int CeilToInt(float val)
        {
            return (int)Math.Ceiling(val);
        }

        public static int RoundToInt(float val)
        {
            return (int)Math.Round(val);
        }

        public static float Abs(float val)
        {
            return val < 0 ? -val : val;
        }

        public static float Cos(float val)
        {
            return (float)Math.Cos(val);
        }

        public static float Sin(float val)
        {
            return (float)Math.Sin(val);
        }

        public static float Sqrt(float val)
        {
            return (float)Math.Sqrt(val);
        }

        public static int Sign(float val)
        {
            return Math.Sign(val);
        }

        public static float Clamp(float val, float min, float max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public static float Pow(float value, float exponent)
        {
            return (float)Math.Pow(value, exponent);
        }
    }

    public struct Rect
    {
        public float x, y, width, height;
    }
}
