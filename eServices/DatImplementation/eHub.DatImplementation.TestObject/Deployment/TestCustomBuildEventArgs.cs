using Microsoft.Build.Framework;

namespace eHub.DatImplementation.TestObject.Deployment
{
    public class TestCustomBuildEventArgs : CustomBuildEventArgs
    {
        public TestCustomBuildEventArgs(string message, string helpKeyword, string senderName)
            : base(message, helpKeyword, senderName)
        {

        }
    }
}
