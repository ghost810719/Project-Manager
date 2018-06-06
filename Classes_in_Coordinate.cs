using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using WinSCP;
using System.Net.NetworkInformation;
using LLP_API;
using System.Windows;

namespace PM
{
    static class Project
    {
        static public string Path;
    }

    class BeaconData
    {
        public int Number { get; set; }
        public class Beacon
        {
            public JArray Coordinates { get; set; }
            public string Level { get; set; }
            public string UUID { get; set; }
            public string Coverage { get; set; }
            public bool Used { get; set; }
        }
        static public List<Beacon> Beacons;

        public BeaconData()
        {
            Beacons = new List<Beacon>();
        }

        public void WhichData(string uuid)
        {
            Number = -1;
            for (int i = 0; i < Beacons.Count; i++)
            {
                if (Beacons[i].UUID == uuid)
                {
                    Number = i;
                }
            }
        }

        public void Used()
        {
            foreach (int index in UsedData.UsedList)
            {
                Beacons[index].Used = true;
            }
        }

        public IEnumerable<string> AvailableList()
        {
            var query = from beacon in Beacons
                        where beacon.Used == false
                        select beacon.UUID;
            return query;
        }
    }

    class Json
    {
        private string _path;
        private string _content;
        private JObject _beaconData;
        public bool Exist { get; set; }

        public Json()
        {
            _path = Project.Path + "_ForLBeacon.json";
            if (File.Exists(_path))
            {
                Exist = true;
                _content = File.ReadAllText(_path);
            }
            else
            {
                Exist = false;
            }
        }

        public void Transfer()
        {
            _beaconData = JObject.Parse(_content);

            foreach (var data in _beaconData["features"])
            {
                BeaconData.Beacon beacon = new BeaconData.Beacon();
                beacon.Coordinates = (JArray)data["geometry"]["coordinates"];
                beacon.Level = (string)data["properties"]["Level"];
                beacon.UUID = (string)data["properties"]["GUID"];
                beacon.Coverage = (string)data["properties"]["Type"];
                beacon.Used = false;
                BeaconData.Beacons.Add(beacon);
            }
        }
    }

    class Config
    {
        private string _path;
        public List<string> ConfText = new List<string>();

        public Config()
        {
            _path = Project.Path + "_Config.txt";
            if (!File.Exists(_path))
            {
                File.Copy("config.conf", _path);
            }

            using (StreamReader sr = new StreamReader(_path))
            {
                string s = "";
                while ((s =sr.ReadLine()) != null)
                {
                    ConfText.Add(s);
                }
            }
        }

        public void Change(int index)
        {
            ConfText[0] = "coordinate_X=" + BeaconData.Beacons[index].Coordinates[0];
            ConfText[1] = "coordinate_Y=" + BeaconData.Beacons[index].Coordinates[1];
            ConfText[2] = "coordinate_Z=" + BeaconData.Beacons[index].Level.Substring(3);
            ConfText[9] = "RSSI_coverage=" + BeaconData.Beacons[index].Coverage.Substring(0, 2);
            ConfText[10] = "uuid=" + BeaconData.Beacons[index].UUID;
            ConfText[11] = "init=1";
        }

        public void Write()
        {
            using (StreamWriter sw = File.CreateText(_path))
            {
                foreach (string s in ConfText)
                {
                    sw.WriteLine(s);
                }
            }
        }
    }

    class UsedData
    {
        private string _path;
        static public List<int> UsedList;

        public UsedData()
        {
            UsedList = new List<int>();
            _path = Project.Path + "_UsedList.txt";
            if (!File.Exists(_path))
            {
                using (File.Create(_path)) { }
            }

            using (StreamReader sr = new StreamReader(_path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    UsedList.Add(int.Parse(s));
                }
            }
        }

        public void Add(int index)
        {
            UsedList.Add(index);
            
            using (StreamWriter sw = File.AppendText(_path))
            {
                sw.WriteLine(index);
            }
        }
    }

    class SCP
    {
        SessionOptions sessionOptions;

        public SCP(string hostName, string userName, string passWord)
        {
            sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Scp,
                HostName = hostName,
                UserName = userName,
                Password = passWord
            };
            sessionOptions.GiveUpSecurityAndAcceptAnySshHostKey = true;
            string file = Project.Path + "_Config.txt";

            using (Session session = new Session())
            {
                session.Open(sessionOptions);

                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Automatic;

                TransferOperationResult transferResult;
                transferResult =
                    session.PutFiles(file, "/home/pi/LBeacon/config/config.conf", false, transferOptions);

                transferResult.Check();

                foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    MessageBox.Show("Upload of " + transfer.FileName + " succeeded");
                }
            }
        }
    }

    class PingTest
    {
        public PingTest()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;

            string hostname = "test01.local";
            string data = "This is a ping test.";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 60;
            PingReply reply = pingSender.Send(hostname, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                MessageBox.Show("Success!");
            }
            else
            {
                MessageBox.Show("Time out!");
            }
        }
    }

    class NetAPI
    {
        public NetAPI()
        {
            ServerAPI serverAPI = new ServerAPI("adasda", "342342", "234234");

            List<BeaconInformation> beacons = new List<BeaconInformation>();
            beacons.Add(new BeaconInformation
            {
                Id = Guid.NewGuid(),
                Longitude = 123.45645,
                Latitude = 4654.12313,
                Name = "",
                Floor = ""
            });

            serverAPI.AddBeaconInformations(beacons);

            List<LaserPointerInformation> laserPointers = new List<LaserPointerInformation>();
            laserPointers.Add(new LaserPointerInformation
            {
                Id = Guid.NewGuid()

            });

            serverAPI.AddLaserPointerInformations(laserPointers);
        }
    }
}
