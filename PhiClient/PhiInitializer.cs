using System.IO;
using Verse;

namespace PhiClient
{
    public class PhiInitializer : ITab
    {
        public PhiInitializer()
        {
            PhiClient client = new PhiClient();
            client.TryConnect();
        }

        protected override void FillTab()
        {
            // Nothing
        }
    }
}
