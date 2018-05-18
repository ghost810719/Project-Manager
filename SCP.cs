using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

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
                    Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                }
            }
        }
    }
}
