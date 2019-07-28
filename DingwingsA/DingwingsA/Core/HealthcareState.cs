using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware;
using Microsoft.Xna.Framework;

class HealthcareState : GameState
{
    float time = 0;
    string []option;
    string cost = "$100";

    public HealthcareState()
    {
        if (p.deathBySpikes)
        {
            option = new string[]
            {
            "Putting",
            "you back",
            "together"
            };
        }
        else
        {
            option = new string[]
            {
            "Scraping",
            "you off",
            "the floor"
            };
        }
    }

    public override void draw()
    {
        Core.instance.stateStack[Core.instance.stateStack.IndexOf(this) - 1].draw();
        int width = 150;
        int height = 200;
        float x = Graphics.WIDTH / 2 - width / 2;
        float y = -Mathf.Pow(time-1,3)*Graphics.HEIGHT+height/2;
        Graphics.draw(Graphics.medbill, Graphics.WIDTH / 2 - width / 2, y, width, height, Graphics.spriteDefault);
        for(int i = 0; i < option.Length; i++)
        {
            Graphics.drawString(option[i], x + 4, y + 50 + 20 * i);
        }
        Graphics.drawStringRight(cost, Graphics.WIDTH / 2 + width / 2, y + height - 24);
    }

    public override void run()
    {
        time += HardwareInterface.deltaTime;
        if (time > 2) Core.instance.stateStack.Remove(this);
    }
}