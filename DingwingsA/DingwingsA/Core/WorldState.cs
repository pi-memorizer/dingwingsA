using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware;
using Microsoft.Xna.Framework;
using System.Diagnostics;

class WorldState : GameState
{
    public const float GRAVITY = 500;
    public const float PLAYER_MOVE_SPEED = 150;
    public const float PLAYER_JUMP_SPEED = 300;
    public override void run()
    {
        World w = Core.instance.worlds[p.world];
        p.vy += HardwareInterface.deltaTime*GRAVITY;
        if(p.grounded&&Input.getA())
        {
            p.grounded = false;
            p.vy -= PLAYER_JUMP_SPEED;
        }
        if(p.grounded&&Input.getB()&&!b&&p.vx!=0&&p.dashing<=0)
        {
            p.dashing = 1;
            p.vdash = Mathf.Sign(p.vx) * 3 * PLAYER_MOVE_SPEED;
        }
        if (Input.getRight())
        {
            p.vx = PLAYER_MOVE_SPEED;
            p.flipped = false;
        }
        else if (Input.getLeft())
        {
            p.vx = -PLAYER_MOVE_SPEED;
            p.flipped = true;
        }
        else p.vx = 0;
        p.simulate(w);
        foreach(Entity e in w.entities)
        {
            e.simulate(w);
        }
    }

    public override void draw()
    {
        Graphics.clear(Color.Green);
        p.draw();
        Core.instance.worlds[p.world].draw();
    }
}
