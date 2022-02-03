using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_Udp_Multicast_Winform
{
    public static class ProfanityExtension
    {
        public static string[] profanity { get; set; }
        public static StringBuilder Sharps { get; set; }

        static ProfanityExtension()
        {
            profanity = "shit,gey".Split(',');
            Sharps = new StringBuilder(10);
        }

        public static string IsProfanity(this string text, out bool res)
        {
            res = false;
            foreach (string item in profanity)
            {
                if (text.Contains(item))
                {
                    Sharps.Clear();
                    for (int i = 0; i < item.Length; i++)
                    {
                        Sharps.Append('#');
                    }
                    text = text.Replace(item, Sharps.ToString());
                    res = true;
                }
            }
            return text;
        }
    }
}
