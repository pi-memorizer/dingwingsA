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
    public float newItemTime = 3;
    public override void run()
    {
        if (Core.newItemTime <= 0)
        {
            Core.newItemTime = 10;
            ShopState.unlockables.MoveNext();
            var item = ShopState.unlockables.Current;
            if (item != null)
            {
                bool valid = false;
                for (int i = 0; i < item.Length; i++)
                {
                    item[i].unlocked = true;
                    if (item[i].prereq == "" || Core.getFlag(item[i].prereq)) valid = true;
                }
                if (valid)
                {
                    newItemTime = 0;
                    Sound.owenWilson.Play();
                }
            }
        }
        else Core.newItemTime -= HardwareInterface.deltaTime;
        if(newItemTime<3)
        {
            newItemTime += HardwareInterface.deltaTime;
        }
        Sound.setMusic(Sound.baseSong);

        World w = Core.instance.worlds[p.world];
        p.vy += HardwareInterface.deltaTime*GRAVITY;
        if(Core.deadTime<=0)
        {
            if (Core.getFlag("jump")&&p.grounded && Input.getA())
            {
                p.grounded = false;
                p.vy -= PLAYER_JUMP_SPEED;
                Sound.jump.Play();
            }
            if (Core.getFlag("dash")&&p.grounded && Input.getB() && !b && p.dashing <= 0)
            {
                p.dashing = .5F;
                if(p.vx!=0)
                    p.vdash = Mathf.Sign(p.vx) * 3 * PLAYER_MOVE_SPEED;
                else
                    p.vdash = (p.flipped?-1:1) * 3 * PLAYER_MOVE_SPEED;
                Sound.dash.Play();
            }
            if (Core.getFlag("right")&&Input.getRight())
            {
                p.vx = PLAYER_MOVE_SPEED;
                p.flipped = false;
            }
            else if (Core.getFlag("left")&&Input.getLeft())
            {
                p.vx = -PLAYER_MOVE_SPEED;
                p.flipped = true;
            }
            else p.vx = 0;
            if(getStart()&&!start)
            {
                Core.instance.stateStack.Add(new ShopState());
            }
            if(p.x>85*32&&p.y<18*32&&!Core.getFlag("success1"))
            {
                Core.setFlag("success1");
                Sound.success.Play();
            }
            if (p.x < -75*32 && p.y<18*32&&!Core.getFlag("success2"))
            {
                Core.setFlag("success2");
                Sound.success.Play();
            }
        } else
        {
            Core.deadTime -= HardwareInterface.deltaTime;
            
            if(Core.deadTime<=0)
            {
                p.x = p.y = p.vx = p.vy = 0;
                p.alive = true;
                p.lockCamera();
                Core.instance.stateStack.Add(new HealthcareState());
            }
        }
        p.simulate(w);
        foreach(Entity e in w.entities)
        {
            e.simulate(w);
        }
        for(int i = 0; i < Graphics.particles.Count; i++)
        {
            var particle = Graphics.particles[i];
            particle.vy += GRAVITY * HardwareInterface.deltaTime;
            particle.x += HardwareInterface.deltaTime * particle.vx;
            particle.y += HardwareInterface.deltaTime * particle.vy;
            if(Core.getOnscreenY(particle.y)>Graphics.HEIGHT)
            {
                Graphics.particles.RemoveAt(i);
                i--;
            }
        }
    }

    public override void draw()
    {
        Graphics.clear(Color.White);
        Graphics.setFilter(true);
        if(Core.getFlag("background")) for(int i = 0; i < 3; i++)
        {
            if (i == 0 && !(Core.getFlag("green")||Core.getFlag("red")||Core.getFlag("blue"))) continue;
            Graphics.drawParallax(Graphics.backgrounds[i], p.getCameraX(), .0001F * i);
        }
        Core.instance.worlds[p.world].draw();
        p.draw();
        Graphics.setFilter(false);
        Graphics.lockMaterial(Color.White);
        foreach(var particle in Graphics.particles)
        {
            if(!particle.isMoney)Graphics.drawParticle(particle.color, Core.getOnscreenX(particle.x), Core.getOnscreenY(particle.y), (int)Graphics.scale, (int)Graphics.scale);
        }
        Graphics.unlockMaterial();
        Graphics.lockMaterial(Graphics.bitmapFont,Color.Red);
        foreach (var particle in Graphics.particles)
        {
            if (particle.isMoney) Graphics.draw(Graphics.charSprites['$'], Core.getOnscreenX(particle.x), Core.getOnscreenY(particle.y), (int)Graphics.scale*8, (int)Graphics.scale*8);
        }
        Graphics.unlockMaterial();
        drawMoney(true);
        Graphics.draw(Graphics.newItem, Graphics.WIDTH - 5 - Graphics.newItem.Width, Graphics.HEIGHT-Graphics.newItem.Height+Mathf.Pow(newItemTime - 1.5F, 2) * 50-5, Graphics.newItem.Width, Graphics.newItem.Height,Graphics.spriteDefault);
    }

    public static void drawMoney(bool drawBackground)
    {
        int width = 32 * (1 + Mathf.FloorToInt(Core.animationMoney).ToString().Length);
        if(drawBackground)Graphics.drawRect(Color.White, Graphics.WIDTH - 8 - width, 8 + 2, width, 32);
        Graphics.drawStringRight("$" + Mathf.FloorToInt(Core.animationMoney), Graphics.WIDTH - 8, 8, 2);
    }
}
