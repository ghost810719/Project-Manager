using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PM
{
    /// <summary>
    /// Interaction logic for Coordinate.xaml
    /// </summary>
    public partial class Coordinate : UserControl
    {
        private ProcessJson processJson;

        public Coordinate(ProjectsList.ProjectFiles p)
        {
            InitializeComponent();

            string jsonPath = p.FullPath;
            int endIndex = jsonPath.IndexOf(".rvt");
            jsonPath = jsonPath.Substring(0, endIndex);
            jsonPath += "_ForLBeacon.json";
            processJson = new ProcessJson(jsonPath);
            lsLast.ItemsSource = processJson.ConfText;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            processJson.ModifyData();
            SCP scp = new SCP();
            bool flag = true;
            do
            {
                flag = true;
                try
                {
                    scp.Transmit(processJson.ConfigPath);
                }
                catch
                {
                    flag = false;
                    MessageBox.Show("Connection of remote failed");
                }
            } while (flag == false);
        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            PingTest ping = new PingTest();
        }

        private void beaconSelect_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = processJson._beacons.Select(C => C.uuid);
        }

        private void beaconSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool used = false;
            var comboBox = sender as ComboBox;
            string uuid = comboBox.SelectedItem.ToString();

            processJson.DataCount = processJson.WhichData(ref used, uuid);
            processJson.ChangeListBox();
            lsLast.ItemsSource = null;
            lsLast.ItemsSource = processJson.ConfText;
        }
    }

    public class ProcessJson
    {
        public struct Beacon
        {
            public JArray coordinate;
            public string level;
            public string uuid;
            public string coverage;
        };
        public List<Beacon> _beacons = new List<Beacon>();
        public List<string> ConfText = new List<string>();
        public string ConfigPath;
        public int DataCount;

        public ProcessJson(string jsonPath)
        {
            string json = "";
            JObject _beaconData;
            if (File.Exists(jsonPath))
            {
                json = File.ReadAllText(jsonPath);
            }
            else
            {
                Console.WriteLine("No such file.");
            }

            _beaconData = JObject.Parse(json);

            foreach (var data in _beaconData["features"])
            {
                Beacon beacon;
                beacon.coordinate = (JArray)data["geometry"]["coordinates"];
                beacon.level = (string)data["properties"]["Level"];
                beacon.uuid = (string)data["properties"]["GUID"];
                beacon.coverage = (string)data["properties"]["Type"];
                _beacons.Add(beacon);
            }

            ConfigPath = System.IO.Path.GetDirectoryName(jsonPath) + "\\config.conf";
            InitConfig(ConfigPath);
            ChangeListBox();
        }

        private void InitConfig(string ConfigPath)
        {
            if (!File.Exists(ConfigPath))
            {
                File.Copy("config.conf", ConfigPath);
            }

            using (StreamReader sr = new StreamReader(ConfigPath))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    ConfText.Add(s);
                }
            }
        }

        public int WhichData(ref bool used, string uuid)
        {
            int iterator = 0;

            foreach (var beacon in _beacons)
            {
                if (uuid == beacon.uuid)
                {
                    return iterator;
                }
                iterator++;
            }
            return -1;
        }

        public void ChangeListBox()
        {
                ConfText[0] = "coordinate_X=" + _beacons[DataCount].coordinate[0];
                ConfText[1] = "coordinate_Y=" + _beacons[DataCount].coordinate[1];
                ConfText[2] = "coordinate_Z=" + _beacons[DataCount].level.Substring(3);
                ConfText[9] = "RSSI_coverage=" + _beacons[DataCount].coverage.Substring(0, 2);
                ConfText[10] = "uuid=" + _beacons[DataCount].uuid;
                ConfText[11] = "init=1";
        }

        public void ModifyData()
        {
            using (StreamWriter sw = new StreamWriter(ConfigPath))
            {
                foreach (var str in ConfText)
                {
                    sw.WriteLine(str);
                }
            }
        }
    }
}
