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
        if (GameState.getStart() && !GameState.start)
        {
            if(Core.getFlag("blue"))
            {
                Core.setFlag("red", false);
                Core.setFlag("blue", false);
                Core.setFlag("green", false);
            } else
            if (Core.getFlag("red"))
            {
                Core.setFlag("blue");
            }
            else if (Core.getFlag("green"))
            {
                Core.setFlag("red");
            }
            else
            {
                Core.setFlag("green");
            }
        }

        World w = Core.instance.worlds[p.world];
        p.vy += HardwareInterface.deltaTime*GRAVITY;
        if(Core.deadTime<=0)
        {
            if (p.grounded && Input.getA())
            {
                p.grounded = false;
                p.vy -= PLAYER_JUMP_SPEED;
            }
            if (p.grounded && Input.getB() && !b && p.vx != 0 && p.dashing <= 0)
            {
                p.dashing = .5F;
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
        } else
        {
            Core.deadTime -= HardwareInterface.deltaTime;
            
            if(Core.deadTime<=0)
            {
                p.x = p.y = p.vx = p.vy = 0;
                p.lockCamera();
            }
        }
        p.simulate(w);
        foreach(Entity e in w.entities)
        {
            e.simulate(w);
        }
    }

    public override void draw()
    {
        Graphics.clear(Color.White);
        Graphics.setFilter(true);
        p.draw();
        Core.instance.worlds[p.world].draw();
        Graphics.drawStringRight("$" + Mathf.FloorToInt(Core.animationMoney), Graphics.WIDTH-8, 8,2);
        Graphics.setFilter(false);
    }
}
