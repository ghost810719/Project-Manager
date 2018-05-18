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
            lsLast.ItemsSource = processJson._confText;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            processJson.ModifyData();
            lsThis.ItemsSource = processJson._confText;
            SCP scp = new SCP();
            bool flag = true;
            do
            {
                flag = true;
                try
                {
                    scp.Transmit(processJson.configPath);
                }
                catch
                {
                    flag = false;
                    MessageBox.Show("Connection of remote failed");
                }
            } while (flag == false);
        }
    }

    public class ProcessJson
    {
        private struct Beacon
        {
            public JArray coordinate;
            public string level;
            public string uuid;
        };
        private List<Beacon> _beacons = new List<Beacon>();
        public List<string> _confText = new List<string>();
        public string configPath;
        private int _dataCount;

        public ProcessJson(string jsonPath)
        {
            string _json = "";
            JObject _beaconData;
            if (File.Exists(jsonPath))
            {
                _json = File.ReadAllText(jsonPath);
            }
            else
            {
                Console.WriteLine("No such file.");
            }

            _beaconData = JObject.Parse(_json);

            foreach (var data in _beaconData["features"])
            {
                Beacon beacon;
                beacon.coordinate = (JArray)data["geometry"]["coordinates"];
                beacon.level = (string)data["properties"]["Level"];
                beacon.uuid = (string)data["properties"]["GUID"];
                _beacons.Add(beacon);
            }

            configPath = System.IO.Path.GetDirectoryName(jsonPath) + "\\config.conf";
            InitConfig(configPath);
            _dataCount = WhichData();
        }

        private void InitConfig(string configPath)
        {
            if (!File.Exists(configPath))
            {
                File.Copy("config.conf", configPath);
            }

            using (StreamReader sr = new StreamReader(configPath))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    _confText.Add(s);
                }
            }
        }

        private int WhichData()
        {
            string uuid = "";
            int startIndex;
            int iterator = 0;

            foreach (var data in _confText)
            {
                if (data == "init=0")
                {
                    return 0;
                }
                if (data.Contains("uuid="))
                {
                    startIndex = data.IndexOf('=') + 1;
                    uuid = data.Substring(startIndex);
                }
            }

            foreach (var beacon in _beacons)
            {
                iterator++;
                if (uuid == beacon.uuid)
                {
                    return iterator;
                }
            }

            return -1;
        }

        public void ModifyData()
        {
            try
            {
                _confText[0] = "coordinate_X=" + _beacons[_dataCount].coordinate[0];
                _confText[1] = "coordinate_Y=" + _beacons[_dataCount].coordinate[1];
                _confText[2] = "coordinate_Z=" + _beacons[_dataCount].level.Substring(3);
                _confText[10] = "uuid=" + _beacons[_dataCount].uuid;
                _confText[11] = "init=1";
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("There are no more LBeacon's can be read!");
                return;
            }

            using (StreamWriter sw = new StreamWriter(configPath))
            {
                foreach (var str in _confText)
                {
                    sw.WriteLine(str);
                }
            }
        }
    }
}
