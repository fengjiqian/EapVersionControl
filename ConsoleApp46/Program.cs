using log4net.Config;
using Newtonsoft.Json;
using System.Linq;

namespace ConsoleApp46
{
    internal class EquipmentState
    {
        public string EquipmentName = "";
        public string ModelName = "";
        public DateTime BeginTime;
        public DateTime EndTime;
        public string EqpState;
        public long Duration;
        public string ClientVersion;
        public string IpAddress;
        public DateTime LastContactTime;
        public string EAPClientState = "OnLine";
        public string EapClientPath;
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));




            new MessageService();


            //Dictionary<string, string>? eqpName_version = new Dictionary<string, string>();

            //Timer timer = new Timer(doUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            while (true) Console.ReadLine();
        }

        private static void doUpdate(object? state)
        {
            Dictionary<string, string>? eqpName_version = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(@"resources\eqpName_version.json"));

            update(eqpName_version);

            return;
        }

        private static void update(Dictionary<string, string> eqpName_version)
        {
            using HttpClient httpClient = new HttpClient();
            List<string> ipList = new List<string>();
            List<string> ret = new List<string>();

            string equipmentStatesList = httpClient.GetStringAsync("http://172.16.204.220/ls").GetAwaiter().GetResult();
            List<EquipmentState>? equipmentStates = JsonConvert.DeserializeObject<List<EquipmentState>>(equipmentStatesList);
            if (equipmentStates == null) return;


            Console.WriteLine(JsonConvert.SerializeObject(ipList, Formatting.Indented));
            Console.WriteLine($"COUNT[{ipList.Count}]");

            Console.WriteLine("START");
            eqpName_version.Keys.ToList().ForEach(eqpName =>
            {
                string ip = equipmentStates.FindLast(e => e.EquipmentName.Equals(eqpName)).IpAddress;
                string version = eqpName_version.GetValueOrDefault(eqpName);

                httpClient.GetAsync($"http://{ip}:5001/eapupdate/eapclient-gui-{version}-jar-with-dependencies.jar").ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        Console.WriteLine(t.Exception.Message);
                        File.AppendAllText("temp.log", $"{ip}  {t.Exception.Message}\n");
                        return;
                    }

                    t.Result.Content.ReadAsStringAsync().ContinueWith(e =>
                    {
                        Console.WriteLine($"{ip}  {e.Result}");
                        ret.Add($"{ip}  {e.Result}");
                        File.AppendAllText("temp.log", $"{ip}  {e.Result}\n");
                    });
                });

                Thread.Sleep(3000);
            });

            Console.WriteLine("FINISH");

        }
    }
}