using Microsoft.Build.Framework;

namespace eHub.DatImplementation.TestObject.Deployment
{
    public class TestBuildStatusEventArgs : BuildStatusEventArgs
    {
        public TestBuildStatusEventArgs(string message, string helpKeyword, string senderName)
            : base(message, helpKeyword, senderName)
        {

        }
    }
}
