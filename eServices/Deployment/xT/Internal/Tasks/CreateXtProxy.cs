using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
    public class CreateXtProxy : Task
	{
        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Creating proxy for xT instance.");
            try
            {
                SharedObjects.xtProxy = new XtProxy(ConnectionString, xTUserName, xTPassword, ServerName);
                SharedObjects.hasProxyBeenCreated = true;
            }
            finally
            {
                if (SharedObjects.hasProxyBeenCreated)
                {
                    Log.LogMessage(MessageImportance.High, "\tProxy was created for xT instance.");
                    Log.LogMessage(MessageImportance.High, $"\t\tServer: {ServerName}");
                }
                else
                {
                    Log.LogMessage(MessageImportance.High, "\tFailed to create proxy for xT instance.");
                    Log.LogMessage(MessageImportance.High, $"\t\tConnection String: {ConnectionString}");
                    Log.LogMessage(MessageImportance.High, $"\t\txT UserName: {xTUserName}");
                    Log.LogMessage(MessageImportance.High, $"\t\tServer Name: {ServerName}");
                }
            }
            return SharedObjects.hasProxyBeenCreated;
        }

        [Required]
        public string ConnectionString { get; set; }

        [Required]
        public string xTUserName { get; set; }

        [Required]
        public string xTPassword { get; set; }

        [Required]
        public string ServerName { get; set; }
    }
}
