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
        public string name, description, flag;
        public bool bought, unlocked;

        public Item(string name, string flag, string description)
        {
            this.name = name;
            this.flag = flag;
            this.description = description;
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
        for (int i = 0; i < 10; i++)
        {
            categories.Add("category" + i);
            items.Add(new List<Item>());
            for(int j = 0; j < 10; j++)
            {
                items[i].Add(new Item("item" + j, "item" + j, "This does something snarky"));
            }
        }
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
        Graphics.drawRect(Color.White, 0, 0, 150, 300);
        for (int i = topCategory; i < topCategory + 5; i++)
        {
            Graphics.drawString(categories[i], categoryIndex == i ? 10 : 0, 60 * (i - topCategory));
        }
        for (int i = topItem; i < topItem + 3; i++)
        {
            Item item = items[categoryIndex][i];
            int top = 150 * (i - topItem);
            int left = 200 + (i==itemIndex?10:0);
            Graphics.drawRect(Color.White, left, top, 300, 150);
            Graphics.drawString(item.name, left, top);
            Graphics.drawString(item.description, left, top + 20);
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
                if (itemIndex >= topItem + 3) topItem++;
            }
            if(getLeft()&&!left)
            {
                leftSide = true;
            }
        }
    }
}