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

    public Textbox(string msg)
    {
        this.msg = msg;
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
        int maxChars = Mathf.FloorToInt(time*30);
        Graphics.drawRect(Color.White, Graphics.WIDTH / 2 - WIDTH / 2, Graphics.HEIGHT - HEIGHT, WIDTH, HEIGHT);
        for(int i = 0; i < lines.Count; i++)
        {
            Graphics.drawString(lines[i], Graphics.WIDTH / 2 - MAX_LENGTH*Graphics.CHAR_SIZE / 2, Graphics.HEIGHT - HEIGHT+i*Graphics.CHAR_SIZE+Graphics.CHAR_SIZE,maxChars);
            maxChars -= lines[i].Length;
        }
    }

    public override void run()
    {
        time += HardwareInterface.deltaTime;
    }
}
