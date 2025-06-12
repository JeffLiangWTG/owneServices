using Microsoft.Build.Framework;

namespace eHub.DatImplementation.TestObject.Deployment
{
    public class TestBuildEventArgs : BuildEventArgs
    {
        public TestBuildEventArgs(string message, string helpKeyword, string senderName)
            : base(message, helpKeyword, senderName)
        {

        }
    }
}
