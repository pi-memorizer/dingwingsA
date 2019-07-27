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
        public int price = 100;

        public Item(string name, string flag, string description)
        {
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
    static List<string> categories = new List<string>();
    static int categoryIndex = 0, topCategory = 0;
    static List<List<Item>> items = new List<List<Item>>();
    static Dictionary<string, Item> allItems = new Dictionary<string, Item>();
    static int itemIndex = 0, topItem = 0;
    static bool leftSide = true;

    static IEnumerator<Item> unlockables = getUnlockables();
    static ShopState()
    {
        categories.Add("Graphics");
        List<Item> graphicItems = new List<Item>();
        addItem(graphicItems, new Item("Green Channel", "green", "You'll need me to see you!"));
        addItem(graphicItems, new Item("Red Channel", "red", "You'll need me to see you!"));
        addItem(graphicItems, new Item("Blue Channel", "blue", "You'll need me to see you!"));
        items.Add(graphicItems);

        foreach (var item in allItems.Values)
        {
            item.unlocked = true;
        }
    }

    static void addItem(List<Item> list, Item item)
    {
        list.Add(item);
        allItems.Add(item.flag, item);
    }

    public ShopState()
    {

    }

    public static IEnumerator<Item> getUnlockables()
    {
        yield return null;
    }

    public override void draw()
    {
        Core.instance.stateStack[Core.instance.stateStack.IndexOf(this) - 1].draw();
        Graphics.drawRect(Color.White, 0, 0, 150, 300);
        for (int i = topCategory; i < topCategory + 5&&i<categories.Count; i++)
        {
            Graphics.drawString(categories[i], categoryIndex == i ? 10 : 0, 60 * (i - topCategory));
        }
        for (int i = topItem; i < topItem + 3&&i<items[categoryIndex].Count; i++)
        {
            Item item = items[categoryIndex][i];
            int top = 150 * (i - topItem);
            int left = 200 + (i==itemIndex?10:0);
            Graphics.drawRect(Color.White, left, top, 300, 150);
            Graphics.drawString(item.name, left, top);
            for(int j = 0; j < item.description.Count; j++)
            {
                Graphics.drawString(item.description[j], left, top + 20+j*(Graphics.CHAR_SIZE+2));
            }
        }
    }

    public override void run()
    {
        if(leftSide)
        {
            if(getUp()&&!up)
            {
                if (categoryIndex != 0) categoryIndex--;
                if (categoryIndex < topCategory) topCategory--;
            } else if(getDown()&&!down)
            {
                if (categoryIndex != categories.Count - 1) categoryIndex++;
                if (categoryIndex >= topCategory + 5) topCategory++;
            }
            if (itemIndex > items[categoryIndex].Count) itemIndex = items[categoryIndex].Count - 1;
            if(getRight()&&!right)
            {
                leftSide = false;
            }
        } else
        {
            if (getUp() && !up)
            {
                if (itemIndex != 0) itemIndex--;
                if (itemIndex < topItem) topItem--;
            }
            else if (getDown() && !down)
            {
                if (itemIndex != items[categoryIndex].Count - 1) itemIndex++;
                if (itemIndex >= topItem + 2) topItem++;
            }
            if(getLeft()&&!left)
            {
                leftSide = true;
            }
            if(getA()&&!a)
            {
                Item item = items[categoryIndex][itemIndex];
                if(item.unlocked&&!item.bought)
                {
                    item.bought = true;
                    Core.money -= item.price;
                    Core.setFlag(item.flag);
                }
            }
        }
        if(getB()&&!b)
        {
            Core.instance.stateStack.Remove(this);
        }
    }
}