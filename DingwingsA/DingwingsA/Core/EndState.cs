using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware;
using Microsoft.Xna.Framework;

class EndState : GameState
{
    float animationMoney = 0;
    public override void draw()
    {
        Graphics.clear(Color.White);

    }

    public override void run()
    {
        if (Core.money < animationMoney)
        {
            int _a = (int)animationMoney;
            animationMoney -= HardwareInterface.deltaTime * 100;
            if ((int)animationMoney != _a)
            {
                float x = Graphics.WIDTH - (float)(Core.rand.NextDouble()) * (animationMoney.ToString().Length) * Graphics.CHAR_SIZE - 10;
                float angle = (float)(Core.rand.NextDouble()) * 3.1415F * 2;
                float magnitude = (float)(Core.rand.NextDouble()) * 100;
                Graphics.particles.Add(new Particle(Color.Red, x + p.getCameraX() + p.width / 2 - Graphics.WIDTH / 2, 16 + Graphics.CHAR_SIZE / 2 + p.getCameraY() + p.height / 2 - Graphics.HEIGHT / 2, magnitude * Mathf.Cos(angle), magnitude * Mathf.Sin(angle)));
            }
            if (Core.money > animationMoney) animationMoney = Core.money;
        }
        if (Core.money > animationMoney)
        {
            animationMoney += HardwareInterface.deltaTime * 100;
            if (Core.money < animationMoney) animationMoney = Core.money;
        }
        if (getA()&&!a)
        {
            HardwareInterface.f5();
        }
    }
}