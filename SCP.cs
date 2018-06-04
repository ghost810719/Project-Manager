using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.NetworkInformation;
using WinSCP;
using LLP_API;

namespace PM
{
    class SCP
    {
        SessionOptions sessionOptions;

        public SCP()
        {
            sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Scp,
                HostName = "test01.local",
                UserName = "pi",
                Password = "raspberry"
            };
            sessionOptions.GiveUpSecurityAndAcceptAnySshHostKey = true;
        }

        public void Transmit(string file)
        {
            using (Session session = new Session())
            {
                session.Open(sessionOptions);

                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Automatic;

                TransferOperationResult transferResult;
                transferResult =
                    session.PutFiles(file, "/home/pi/LBeacon/config/", false, transferOptions);

                transferResult.Check();

                foreach(TransferEventArgs transfer in transferResult.Transfers)
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
            ServerAPI serverAPI = new ServerAPI("adasda","342342","234234");
            
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
            laserPointers.Add(new LaserPointerInformation {
                Id = Guid.NewGuid()
                
            });

            serverAPI.AddLaserPointerInformations(laserPointers);
        }
    }
}
