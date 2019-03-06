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

        private static  Dictionary<string,string> BuildingName = new Dictionary<string, string>()
            {
                {"0011449816806945psc","1#"},
                {"0011449816830250MuI","2#"},
                {"0011449816876736sfx","综合楼东"},
                {"0011449816949458BXk","综合楼西"}
            };

        static void Main(string[] args)
        {
            var userConfig = InitConfig();

            var backgroundTasks = new []
            {
                Task.Run(()=>CheckHouse("0011449816806945psc",userConfig.Account,userConfig.PWD)),
                Task.Run(()=>CheckHouse("0011449816830250MuI",userConfig.Account,userConfig.PWD)),
                Task.Run(()=>CheckHouse("0011449816876736sfx",userConfig.Account,userConfig.PWD)),
                Task.Run(()=>CheckHouse("0011449816949458BXk",userConfig.Account,userConfig.PWD)),
            };

            Task.WaitAll(backgroundTasks);

            

        }


        private static void CheckHouse(string buildingCode,string user,string pwd)
        {

            if(!BuildingName.ContainsKey(buildingCode))
            {
                Console.WriteLine("楼层code不存在：{0}",buildingCode);
                return;
            }

            string url = @"/online/roomResource.xp?action=register&t=" + DateTime.Now.CurrentTimeMillis().ToString();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(@"http://117.71.57.99:9080");
                Console.WriteLine("正在查找{0}楼层房源信息....", BuildingName[buildingCode]);
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
                        param.Add(new KeyValuePair<string, string>("buildingCode", buildingCode));
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
                                    where room.Status.Equals("02") || room.Status.Equals("01")
                                    select room;
                        var availabeRooms = query.ToList();
                        if (availabeRooms.Count != 0)
                        {
                            Console.WriteLine("找到房源啦!!");
                            foreach (var r in availabeRooms)
                            {
                                Console.WriteLine("开始尝试预定房间{0},房间大小{1},房间朝向{2},房间类型{3},房间价格{4},楼号{5},楼层{6}",r.RoomCode,r.RoomArea,r.RoomDirection,r.RoomType,r.Price,BuildingName[buildingCode],r.RoomFloor);
                                HttpResponseMessage message;
                                var loginUrl = "/online/gzflogin.jtml?action=login";
                                List<KeyValuePair<string, string>> loginParams = new List<KeyValuePair<string, string>>();
                                loginParams.Add(new KeyValuePair<string, string>("accountCode", user));
                                loginParams.Add(new KeyValuePair<string, string>("accountPass", pwd));
                                loginParams.Add(new KeyValuePair<string, string>("wrong", ""));
                                message = client.PostAsync(loginUrl, new FormUrlEncodedContent(loginParams)).Result;
                                // HttpResponseMessage message = client.GetAsync("/online/roomConfig.xp?action=getRoomConfig&roomID="+id).Result;
                                List<KeyValuePair<string, string>> p = new List<KeyValuePair<string, string>>();
                                p.Add(new KeyValuePair<string, string>("roomCode", r.RoomCode));
                                message = client.PostAsync("/online/apply.do?action=roomConfirm", new FormUrlEncodedContent(p)).Result;
                                string s = message.Content.ReadAsStringAsync().Result;
                                if (s.Equals("error"))
                                {
                                    Console.WriteLine("房间号：{0}预定失败", r.Id);
                                }
                                else
                                {
                                    SubmitMode mode = new SubmitMode();
                                    mode.Code = "01";
                                    mode.Name="皖水公寓";
                                    mode.BuildingCode = buildingCode;
                                    mode.BuildingName = BuildingName[buildingCode];
                                    mode.BuildingFloor = r.RoomFloor;
                                    List<string> ss = new List<string>();
                                    ss.Add(r.RoomCode);
                                    mode.RoomList = ss;
                                    string rooms = JsonConvert.SerializeObject(mode);
                                    Console.WriteLine("预定房间信息:{0}",rooms);

                                    List<KeyValuePair<string, string>> submitPara = new List<KeyValuePair<string, string>>();
                                    submitPara.Add(new KeyValuePair<string, string>("jsonStr",rooms));
                                    submitPara.Add(new KeyValuePair<string, string>("geetest_challenge",challenge));
                                    submitPara.Add(new KeyValuePair<string, string>("geetest_validate",validate));
                                    submitPara.Add(new KeyValuePair<string, string>("geetest_seccode",seccode));

                                    message = client.PostAsync(@"/online/apply.do?action=roomSchedule",new FormUrlEncodedContent(submitPara)).Result;
                                    string data = message.Content.ReadAsStringAsync().Result;
                                        
                                    if(data.Equals("ok"))
                                    {
                                        Console.WriteLine("房间：{0}预定成功,{1}", r.Id,s);
                                    }else
                                    {
                                        Console.WriteLine("房间：{0}失败,{1}", r.Id,s);
                                    }

                                }
                            }
                        }
                        Thread.Sleep(500);
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
