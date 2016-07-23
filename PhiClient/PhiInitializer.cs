using System.IO;
using Verse;

namespace PhiClient
{
    public class PhiInitializer : ITab
    {
        const string FILE_PATH = "server.txt";
        const string DEFAULT_SERVER_ADDRESS = "longwelwind.net";

        public PhiInitializer()
        {
            string address = GetServerAddress();

            PhiClient client = new PhiClient(address);
            client.TryConnect();
        }

        public string GetServerAddress()
        {
            if (!File.Exists(FILE_PATH))
            {
                using (StreamWriter w = File.AppendText(FILE_PATH))
                {
                    w.WriteLine(DEFAULT_SERVER_ADDRESS);
                    return DEFAULT_SERVER_ADDRESS;
                }
            }
            else
            {
                return File.ReadAllLines(FILE_PATH)[0];
            }
        }

        protected override void FillTab()
        {
            // Nothing
        }
    }
}
