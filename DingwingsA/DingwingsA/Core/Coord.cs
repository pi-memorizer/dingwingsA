using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct Coord : IEquatable<Coord>
{
    public int x, y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Coord other)
    {
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        return x * 31 + y;
    }
}