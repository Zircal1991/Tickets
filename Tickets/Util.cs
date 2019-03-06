using System;
using System.Collections.Generic;
using System.Text;

namespace Tickets
{
    public class Util
    {
        private static readonly int[,] offline_sample = new int[,] {
                { 82,0,136},
                {61,5,108 },
                {128,2,7 },
                {130,4,99 },
                {189,3,65 },
                {108,5,285 },
                {136,0,36 },
                {41,0,263},
                {124,3,185 }
        };

        private static float Random()
        {
            Random rd = new Random();
            int a = rd.Next(10000);
            float f = (float)(a * 0.0001);
            return f;
        }


        private static string CaculateChallenge(int a,string challenge)
        {
            char[] challengeChars = challenge.ToCharArray();
            List<int> challengeInts = new List<int>();

            for(int i = 32;i<challengeChars.Length;i++)
            {
                if(challengeChars[i]>57)
                {
                    challengeInts.Add(challengeChars[i] - 87);
                }
                else
                {
                    challengeInts.Add(challengeChars[i] - 48);
                }
            }

            int sum = 36 * challengeInts[0] + challengeInts[1];
            sum = sum + a;

            List<char> h = new List<char>();
            List<List<char>> list = new List<List<char>>();

            for(int i=0;i<5;i++)
            {
                list.Add(new List<char>());
            }

            Dictionary<char, string> dict = new Dictionary<char, string>();

            int k = 0;

            for(int i=0;i<32;i++)
            {
                if(!dict.ContainsKey(challengeChars[i]))
                {
                    dict.Add(challengeChars[i], "1");
                    list[k].Add(challengeChars[i]);
                    k++;
                    k = 5 == k ? 0 : k;
                }
            }

            List<int> q = new List<int>() {1,2,5,10,50 };

            int m;

            StringBuilder sb = new StringBuilder();

            for(int o=4;sum>0;)
            {
                if(sum-q[o]>=0)
                {
                    m = (int)(Random() * list[o].Count);
                    sb.Append(list[o][m]);
                    sum -= q[o];
                }else
                {
                    list.RemoveAt(o);
                    q.RemoveAt(o);
                    o--;
                }
            }
            return sb.ToString();
        }


        public static string CaculateValidate(string challenge)
        {
            int r = (int)(Random() * offline_sample.Rank);

            int distance = offline_sample[r,0];
            int rand0 = offline_sample[r, 1];
            int rand1 = offline_sample[r, 2];

            string distance_r = CaculateChallenge(distance,challenge);
            string rand0_r = CaculateChallenge(rand0,challenge);
            string rand1_r = CaculateChallenge(rand1,challenge);

            string validate = distance_r + "_" + rand0_r + "_" + rand1_r;

            return validate;

        }



    }
}
