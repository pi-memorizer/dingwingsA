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

    public EndState()
    {
        Sound.stopMusic();
        Core.setFlag("ads", true);
        Core.setFlag("premium", false);
    }

    public override void draw()
    {
        Graphics.clear(Color.White);
        int width = 32 * (1 + Mathf.FloorToInt(animationMoney).ToString().Length);
        Graphics.drawStringRight("$" + Mathf.FloorToInt(animationMoney), Graphics.WIDTH / 2 + width / 2, 100,2);
        string s = "Things bought " + ShopState.thingsBought + "/15";
        Graphics.drawStringRight(s, Graphics.WIDTH / 2 + (s.Length) * 16, 130,2);
        s = "YOU WIN!";
        Graphics.drawStringRight(s, Graphics.WIDTH / 2 + (s.Length) * 16, 50, 2);
        s = "A Game By Studio Dingwing";
        Graphics.drawString(s, 10, Graphics.HEIGHT - 10-16);
    }

    public override void run()
    {
        if (Core.money < animationMoney)
        {
            animationMoney -= HardwareInterface.deltaTime * 1000;
            if (Core.money > animationMoney) animationMoney = Core.money;
        }
        if (Core.money > animationMoney)
        {
            animationMoney += HardwareInterface.deltaTime * 1000;
            if (Core.money < animationMoney) animationMoney = Core.money;
        }
        if (getA()&&!a)
        {
            HardwareInterface.f5();
        }
    }
}