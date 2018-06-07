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
        private Json _json;
        private Xml _xml;
        private BeaconData _beaconData;
        private Config _config;
        private UsedData _usedData;

        public Coordinate(ProjectsList.ProjectFiles p)
        {
            InitializeComponent();

            string projectPath = p.FullPath;
            int length = projectPath.IndexOf(".rvt");
            projectPath = projectPath.Substring(0, length);
            Project.Path = projectPath;
            _json = new Json();
            _xml = new Xml();
            _beaconData = new BeaconData();
            _config = new Config();
            _usedData = new UsedData();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (beaconSelect.SelectedItem == null)
            {
                MessageBox.Show("Please select data first.");
                return;
            }
            _config.Write();
            SCP scp = new SCP(txtHost.Text, txtUser.Text, txtPass.Text);
            _usedData.Add(_beaconData.Number);
            _beaconData.Used();
            beaconSelect.ItemsSource = _beaconData.AvailableList();
            lsLast.ItemsSource = _config.ConfText;
        }

        private void btnPing_Click(object sender, RoutedEventArgs e)
        {
            PingTest ping = new PingTest();
        }

        private void beaconSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (beaconSelect.SelectedItem == null)
            {
                return;
            }
            string uuid = beaconSelect.SelectedItem.ToString(); 
            _beaconData.WhichData(uuid);
            _config.Change(_beaconData.Number);
            lsLast.ItemsSource = null;
            lsLast.ItemsSource = _config.ConfText;
        }

        private void btnJson_Click(object sender, RoutedEventArgs e)
        {
            if (_json.Exist == false)
            {
                MessageBox.Show("No json file.");
                return;
            }
            _json.Transfer();
            _beaconData.Used();
            beaconSelect.ItemsSource = _beaconData.AvailableList();
            lsLast.ItemsSource = _config.ConfText;
            btnJson.Visibility = Visibility.Hidden;
            btnXml.Visibility = Visibility.Hidden;
        }

        private void btnXml_Click(object sender, RoutedEventArgs e)
        {
            if (_xml.Exist == false)
            {
                MessageBox.Show("No xml file.");
                return;
            }
            _xml.Transfer();
            _beaconData.Used();
            beaconSelect.ItemsSource = _beaconData.AvailableList();
            lsLast.ItemsSource = _config.ConfText;
            btnJson.Visibility = Visibility.Hidden;
            btnXml.Visibility = Visibility.Hidden;
        }
    }

}
