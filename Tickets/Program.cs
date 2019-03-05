using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tickets.Mode;
using Newtonsoft.Json.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Tickets
{
    class Program
    {
        private const string V = "buildingCode";

        static void Main(string[] args)
        {

            var userConfig = InitConfig();


            string url = @"/online/roomResource.xp?action=register&t=" + DateTime.Now.CurrentTimeMillis().ToString();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(@"http://117.71.57.99:9080");

                while (true)
                {
                    try
                    {
                        List<Room> roomList = new List<Room>();
                        HttpResponseMessage responseMessage = client.GetAsync(url).Result;
                        GeetestMode result = JsonConvert.DeserializeObject<GeetestMode>(responseMessage.Content.ReadAsStringAsync().Result);

                        var challenge = result.Challenge;
                        var validate = Util.CaculateValidate(challenge);
                        var seccode = validate + "|jordan";

                        List<KeyValuePair<string, string>> param = new List<KeyValuePair<string, string>>();
                        param.Add(new KeyValuePair<string, string>("geetest_challenge", challenge));
                        param.Add(new KeyValuePair<string, string>("geetest_validate", validate));
                        param.Add(new KeyValuePair<string, string>("geetest_seccode", seccode));
                        param.Add(new KeyValuePair<string, string>("code", "01"));
                        param.Add(new KeyValuePair<string, string>("buildingCode", userConfig.BuildCode));
                        Task<string> reString = client.PostAsync(@"/online/roomResource.xp?action=vaildate", new FormUrlEncodedContent(param)).Result.Content.ReadAsStringAsync();
                        reString.Wait();
                        JObject jObject = JObject.Parse(reString.Result);
                        string status = jObject.GetValue("status").ToString();
                        if (status.Equals("success"))
                        {
                            JToken jToken = jObject.GetValue("list");
                            foreach (JToken token in jToken)
                            {
                                var property = token as JProperty;
                                List<Room> value = property.Value.ToObject<List<Room>>();
                                roomList.AddRange(value);
                            }
                        }

                        var query = from room in roomList
                                    where room.Status.Equals("02")
                                    select room;
                        var availabeRooms = query.ToList();
                        if (availabeRooms.Count != 0)
                        {

                            foreach (var r in availabeRooms)
                            {
                                HttpResponseMessage message;
                                var loginUrl = "/online/gzflogin.jtml?action=login";
                                List<KeyValuePair<string, string>> loginParams = new List<KeyValuePair<string, string>>();
                                loginParams.Add(new KeyValuePair<string, string>("accountCode", userConfig.Account));
                                loginParams.Add(new KeyValuePair<string, string>("accountPass", userConfig.PWD));
                                loginParams.Add(new KeyValuePair<string, string>("wrong", ""));
                                message = client.PostAsync(loginUrl, new FormUrlEncodedContent(loginParams)).Result;
                                // HttpResponseMessage message = client.GetAsync("/online/roomConfig.xp?action=getRoomConfig&roomID="+id).Result;
                                List<KeyValuePair<string, string>> p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("roomCode", r.Id));
                                message = client.PostAsync("/online/apply.do?action=roomConfirm", new FormUrlEncodedContent(p)).Result;
                                string s = message.Content.ReadAsStringAsync().Result;
                                if (s.Equals("error"))
                                {
                                    Console.WriteLine("房间号：{0}预定失败", r.Id);
                                }
                                else
                                {
                                    Console.WriteLine("房间：{0}预定成功", r.Id);
                                }
                            }
                        }

                        Thread.Sleep(1000);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("出现异常：{0}", ex.Message);
                    }


                }


            }
        }

        private static User InitConfig()
        {
            var build = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            var config = build.Build();
            var buildingCode = config[V];
            var account = config["user"];
            var pwd = config["password"];

            return new User() { BuildCode = buildingCode, Account = account, PWD = pwd };
        }

    }
}
