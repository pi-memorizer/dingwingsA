using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public static class Json {
    private static List<object> jsonArray(string s, ref int i)
    {
        List<object> o = new List<object>();
        i++;
        while (s[i] != ']')
        {
            if (s[i] == ',') i++;
            if ((s[i] >= '0' && s[i] <= '9') || s[i] == '-')
            {
                int j = i + 1;
                while ((s[j] >= '0' && s[j] <= '9') || s[j] == '.') j++;
                try
                {
                    o.Add(int.Parse(s.Substring(i, j - i)));
                }
                catch (Exception)
                {
                    o.Add(float.Parse(s.Substring(i, j - i)));
                }
                i = j;
            }
            else if (s[i] == '[')
            {
                o.Add(jsonArray(s, ref i));
            }
            else if (s[i] == '"')
            {
                int j = i + 1;
                while (s[j] != '"') j++;
                o.Add(s.Substring(i + 1, j - i - 1));
                i = j + 1;
            }
            else if (s[i] == '{')
            {
                o.Add(jsonObject(s, ref i));
            }
            else if (s[i] == 't')
            {
                o.Add(true);
                i += 4;
            }
            else if (s[i] == 'f')
            {
                o.Add(false);
                i += 5;
            }
        }
        i++;
        return o;
    }

    private static Dictionary<string, object> jsonObject(string s, ref int i)
    {
        Dictionary<string, object> o = new Dictionary<string, object>();
        i++;
        while (i < s.Length && s[i] != '}')
        {
            if (s[i] == ',') i++;
            int j = i + 1;
            while (s[j] != '"') j++;
            string l = s.Substring(i + 1, j - i - 1);
            i = j + 2;
            if ((s[i] >= '0' && s[i] <= '9') || s[i] == '-')
            {
                j = i + 1;
                while ((s[j] >= '0' && s[j] <= '9') || s[j] == '.') j++;
                try
                {
                    o.Add(l, int.Parse(s.Substring(i, j - i)));
                }
                catch (Exception)
                {
                    o.Add(l, float.Parse(s.Substring(i, j - i)));
                }
                i = j;
            }
            else if (s[i] == '[')
            {
                o.Add(l, jsonArray(s, ref i));
            }
            else if (s[i] == '"')
            {
                j = i + 1;
                while (s[j] != '"') j++;
                o.Add(l, s.Substring(i + 1, j - i - 1));
                i = j + 1;
            }
            else if (s[i] == '{')
            {
                o.Add(l, jsonObject(s, ref i));
            }
            else if (s[i] == 't')
            {
                o.Add(l, true);
                i += 4;
            }
            else if (s[i] == 'f')
            {
                o.Add(l, false);
                i += 5;
            }
        }
        i++;
        return o;
    }

    public static Dictionary<string, object> jsonObject(string s)
    {
        int i = 0;
        StringBuilder sb = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        bool inQuotes = false;
        int j = 0;
        for (; i < s.Length; i++)
        {
            if (inQuotes)
            {
                if (s[i] == '"')
                {
                    inQuotes = false;
                }
                sb.Append(s[i]);
            }
            else
            {
                if (s[i] == '"')
                {
                    inQuotes = true;
                    sb.Append(s[i]);
                }
                else if (s[i] != ' ' && s[i] != '\n' && s[i] != '\t' && s[i] != 13)
                {
                    sb.Append(s[i]);
                }
            }
        }
        i = 0;
        return jsonObject(sb.ToString(), ref i);
    }

    public static int getInt(object o)
    {
        return o is int ? (int)o : (int)(float)o;
    }

    public static float getFloat(object o)
    {
        return o is float ? (float)o : (int)o;
    }
}
