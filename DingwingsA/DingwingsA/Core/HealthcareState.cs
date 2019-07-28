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
    public override void draw()
    {
        Core.instance.stateStack[Core.instance.stateStack.IndexOf(this) - 1].draw();
        int width = 100;
        int height = 200;
        float y = -Mathf.Pow(time-1,3)*Graphics.HEIGHT+height/2;
        Graphics.drawRect(Color.Red, Graphics.WIDTH / 2 - width / 2, y, width, height);
    }

    public override void run()
    {
        time += HardwareInterface.deltaTime;
        if (time > 2) Core.instance.stateStack.Remove(this);
    }
}