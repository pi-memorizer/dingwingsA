using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware;
using Microsoft.Xna.Framework;

class Textbox : GameState
{
    const int WIDTH = Graphics._WIDTH;
    const int HEIGHT = Graphics._HEIGHT / 4;
    const int MAX_LENGTH = (WIDTH-32)/Graphics.CHAR_SIZE;
    string msg;
    List<string> lines = new List<string>();
    float time = 0;
    float closeTimer;

    public Textbox(string msg, float closeTimer = -100)
    {
        this.msg = msg;
        this.closeTimer = closeTimer;
        string line = "";
        string[] s = msg.Split(new char[] { ' ' });
        for(int i = 0; i < s.Length; i++)
        {
            if(line.Length+s[i].Length+1<=MAX_LENGTH)
            {
                line += s[i] + " ";
            } else
            {
                lines.Add(line);
                line = "";
                i--;
            }
        }
        if(line!="") lines.Add(line);
    }

    public override void draw()
    {
        int index = Core.instance.stateStack.FindIndex(a => a==this);
        if(index>0)Core.instance.stateStack[index - 1].draw();
        int maxChars = Mathf.FloorToInt(time*30);
        Graphics.drawRect(new Color(77,66,86), Graphics.WIDTH / 2 - WIDTH / 2, Graphics.HEIGHT - HEIGHT, WIDTH, HEIGHT);
        Graphics.drawRect(Color.White, Graphics.WIDTH / 2 - WIDTH / 2+1, Graphics.HEIGHT - HEIGHT+1, WIDTH-2, HEIGHT-2);
        for (int i = 0; i < lines.Count; i++)
        {
            Graphics.drawString(lines[i], Graphics.WIDTH / 2 - MAX_LENGTH*Graphics.CHAR_SIZE / 2, Graphics.HEIGHT - HEIGHT+i*Graphics.CHAR_SIZE+Graphics.CHAR_SIZE,maxChars);
            maxChars -= lines[i].Length;
        }
    }

    public override void run()
    {
        time += HardwareInterface.deltaTime;
        if(closeTimer>-100)
        {
            closeTimer -= HardwareInterface.deltaTime;
            if(closeTimer<=0)
            {
                Core.instance.stateStack.Remove(this);
            }
        } else
        {
            if (getA() && !a)
            {
                Core.instance.stateStack.Remove(this);
            }
        }
    }
}
