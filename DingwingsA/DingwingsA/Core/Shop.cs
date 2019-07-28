using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware;
using Microsoft.Xna.Framework;

class ShopState : GameState
{
    public class Item
    {
        public string name, flag;
        public List<string> description = new List<string>();
        const int MAX_LENGTH = 20;
        public bool bought, unlocked;
        public int price = 1000;
        public string prereq;

        public Item(string name, string flag, string description, string prereq = "")
        {
            this.prereq = prereq;
            this.name = name;
            this.flag = flag;
            this.description = new List<string>();
            string line = "";
            string[] s = description.Split(new char[] { ' ' });
            for (int i = 0; i < s.Length; i++)
            {
                if (line.Length + s[i].Length + 1 <= MAX_LENGTH)
                {
                    line += s[i] + " ";
                }
                else
                {
                    this.description.Add(line);
                    line = "";
                    i--;
                }
            }
            if (line != "") this.description.Add(line);
        }
    }
    public static List<string> categories = new List<string>();
    public static int categoryIndex = 0, topCategory = 0;
    public static List<List<Item>> items = new List<List<Item>>();
    public static Dictionary<string, Item> allItems = new Dictionary<string, Item>();
    public static int itemIndex = 0, topItem = 0;
    public static bool leftSide = true;
    public bool purchase;

    public static IEnumerator<Item[]> unlockables = getUnlockables();
    static ShopState()
    {
        categories.Add("Graphics");
        List<Item> graphicItems = new List<Item>();
        addItem(graphicItems, new Item("Green Channel", "green", "You'll need me to see you!"));
        addItem(graphicItems, new Item("Red Channel", "red", "You'll need me to see you!"));
        addItem(graphicItems, new Item("Blue Channel", "blue", "You'll need me to see you!"));
        addItem(graphicItems, new Item("Background", "background", "Learn to see past "));
        addItem(graphicItems, new Item("Graphics", "graphics1", "Learn to see past blocky shapes and unclear items! Upgrade your sight today!"));
        addItem(graphicItems, new Item("Graphics 2", "graphics2", "If you ever wanted to see the world in HD, this is your chance!", "graphics1"));
        items.Add(graphicItems);

        categories.Add("Sound");
        List<Item> soundItems = new List<Item>();
        addItem(soundItems, new Item("Sound", "music1", "Tired of eternal bongos? Get the latest sick beats to soothe your frazzled soul!"));
        addItem(soundItems, new Item("Better Sound", "music2", "For the consumers in need of a true melody.", "music1"));
        items.Add(soundItems);

        categories.Add("Abilities");
        List<Item> abilityItems = new List<Item>();
        addItem(abilityItems, new Item("Move Left", "left", "If you've been stuck making three rights for a left your whole life, then do we have a product for you!"));
        addItem(abilityItems, new Item("Move Right", "right", "You'll always be wrong without the ability to go right! Invest in this lifechanging ability today!"));
        addItem(abilityItems, new Item("Jump", "jump", "Ever wondered what life is like off the ground? Grow that leg and jump!"));
        addItem(abilityItems, new Item("Dash", "dash", "Drink BlueCow (TM) to get your wings today!"));
        items.Add(abilityItems);

        categories.Add("Misc");
        List<Item> miscItems = new List<Item>();
        addItem(miscItems, new Item("No Ads", "ads", "Remove those pesky ads cluttering up your life!"));
        addItem(miscItems, new Item("Win Game", "win", "Tired of life being just one big game? Purchase the end of your trials now"));
        items.Add(miscItems);
    }

    static void addItem(List<Item> list, Item item)
    {
        list.Add(item);
        allItems.Add(item.flag, item);
    }

    public ShopState()
    {

    }

    public static IEnumerator<Item[]> getUnlockables()
    {
        yield return new Item[] { allItems["left"], allItems["right"] };
        yield return new Item[] { allItems["red"], allItems["green"], allItems["blue"] };
        yield return new Item[] { allItems["graphics1"], allItems["music1"] };
        yield return new Item[] { allItems["jump"], allItems["dash"] };
        yield return new Item[] { allItems["background"] };
        yield return new Item[] { allItems["ads"] };
        yield return new Item[] { allItems["music2"] };
        yield return new Item[] { allItems["graphics2"] };
        yield return new Item[] { allItems["win"] };
        while (true) yield return null;
    }

    public override void draw()
    {
        float dx = (Graphics.WIDTH-Graphics._WIDTH)/ 2;
        /*Graphics.drawRect(Color.White, 0, 0, 150, 300);
        for (int i = topCategory; i < topCategory + 5&&i<categories.Count; i++)
        {
            Graphics.drawString(categories[i], categoryIndex == i ? 10 : 0, 60 * (i - topCategory));
        }*/
        Graphics.draw(Graphics.shop, dx, 0, Graphics.WIDTH, Graphics.HEIGHT,Graphics.spriteDefault);
        if(leftSide) Graphics.draw(Graphics.shopCarrot, dx + 124, 75 + (153 - 75) * categoryIndex, Graphics.shopCarrot.Width, Graphics.shopCarrot.Height, Graphics.spriteDefault);
        for (int i = topItem; i < topItem + 3&&i<items[categoryIndex].Count; i++)
        {
            Item item = items[categoryIndex][i];
            int top = 150 * (i - topItem)+48;
            int left = 150;
            int width = 492;
            int height = 140;
            Graphics.draw((item.unlocked&&!item.bought && (item.prereq == "" || Core.getFlag(item.prereq)) ? Graphics.buttonActive:Graphics.buttonDisabled), left+dx, top, width, height, Graphics.spriteDefault);
            
            Graphics.drawString(item.name, left+dx, top);
            for(int j = 0; j < item.description.Count; j++)
            {
                Graphics.drawString(item.description[j], left+dx, top + 20+j*(Graphics.CHAR_SIZE+2));
            }
            if(!leftSide&&i==itemIndex) Graphics.draw(Graphics.buttonHighlight, left+width-Graphics.buttonHighlight.Width-5+dx, top+height-Graphics.buttonHighlight.Height-4, 122, 47,Graphics.spriteDefault);
        }
        WorldState.drawMoney(false);
    }

    public override void run()
    {
        Sound.setMusic(Sound.shopSong);
        if(leftSide)
        {
            if(getUp()&&!up)
            {
                if (categoryIndex != 0) categoryIndex--;
                if (categoryIndex < topCategory) topCategory--;
                Sound.menu.Play();
            } else if(getDown()&&!down)
            {
                if (categoryIndex != categories.Count - 1) categoryIndex++;
                if (categoryIndex >= topCategory + 5) topCategory++;
                Sound.menu.Play();
            }
            if (itemIndex >= items[categoryIndex].Count)
            {
                itemIndex = items[categoryIndex].Count - 1;
                topItem = 0;
            }
            if(getRight()&&!right)
            {
                leftSide = false;
                Sound.menu.Play();
            }
        } else
        {
            if (getUp() && !up)
            {
                if (itemIndex != 0) itemIndex--;
                if (itemIndex < topItem) topItem--;
                Sound.menu.Play();
            }
            else if (getDown() && !down)
            {
                if (itemIndex != items[categoryIndex].Count - 1) itemIndex++;
                if (itemIndex >= topItem + 2) topItem++;
                Sound.menu.Play();
            }
            if(getLeft()&&!left)
            {
                leftSide = true;
                Sound.menu.Play();
            }
            if(getA()&&!a)
            {
                Item item = items[categoryIndex][itemIndex];
                if(item.unlocked&&!item.bought&&(item.prereq==""||Core.getFlag(item.prereq)))
                {
                    item.bought = true;
                    Core.money -= item.price;
                    Core.setFlag(item.flag);
                    purchase = true;
                }
            }
        }
        if(getB()&&!b)
        {
            Core.instance.stateStack.Remove(this);
            if(Core.getFlag("win"))
            {
                Core.instance.stateStack.Add(new EndState());
                Sound.victory.Play();
            } else if(purchase)
            {
                Sound.clap.Play();
                for(int i = 0; i < 200; i++)
                {
                    float x = (float)(Core.rand.NextDouble()) * Graphics.WIDTH;
                    float vx = ((float)(Core.rand.NextDouble())*2-1) * 100;
                    float angle = (float)(Core.rand.NextDouble()) * 3.1415F * 2;
                    float magnitude = (float)(Core.rand.NextDouble()) * 100;
                    Color color = Color.Yellow;
                    int r = Core.rand.Next(3);
                    if (r == 1) color = Color.Magenta;
                    if (r == 2) color = Color.Cyan;
                    Graphics.particles.Add(new Particle(color, x+Core.p.x-Graphics.WIDTH/2, Core.p.y-Graphics.HEIGHT/2, vx+magnitude*Mathf.Cos(angle), magnitude*Mathf.Sin(angle)));
                }
            }
        }
    }
}