using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tickets.Mode;
using Newtonsoft.Json.Linq;
namespace Tickets
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string url = @"http://117.71.57.99:9080/online/roomResource.xp?action=register&t=" + DateTime.Now.CurrentTimeMillis().ToString();

            using (var client = new HttpClient())
            {
                List<Room> roomList = new List<Room>();

                HttpResponseMessage responseMessage = client.GetAsync(url).Result;
                GeetestMode result = JsonConvert.DeserializeObject<GeetestMode>(responseMessage.Content.ReadAsStringAsync().Result);

                var challenge = result.Challenge;
                var validate = Util.CaculateValidate(challenge);
                var seccode = validate + "|jordan";

                List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                param.Add(new KeyValuePair<string, string>("geetest_challenge",challenge));
                param.Add(new KeyValuePair<string, string>("geetest_validate",validate));
                param.Add(new KeyValuePair<string, string>("geetest_seccode", seccode));
                param.Add(new KeyValuePair<string, string>("code","01"));
                param.Add(new KeyValuePair<string, string>("buildingCode", "0011449816830250MuI"));
                Task<string> reString = client.PostAsync(@"http://117.71.57.99:9080/online/roomResource.xp?action=vaildate", new FormUrlEncodedContent(param)).Result.Content.ReadAsStringAsync();

                reString.Wait();

                JObject jObject = JObject.Parse(reString.Result);

                string status = jObject.GetValue("status").ToString();

                if(status.Equals("success"))
                {
                    JToken jToken = jObject.GetValue("list");
                    foreach(JToken token in jToken)
                    {
                        var property = token as JProperty;
                        List<Room> value = property.Value.ToObject<List<Room>>();
                        roomList.AddRange(value);
                    }
                }

                var test = roomList.Select(x => x.Status.Equals("02")).ToList();

                Console.WriteLine(test.Count);

            }
                Console.ReadLine();
        }
    }
}
