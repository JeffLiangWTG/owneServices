using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.Internal.API.tasks
{
	public class ImportConfiguration : Task
	{
        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Importing configuration into xT.");
            try
            {
                var report = new StringBuilder();
                SharedObjects.xtProxy.ImportConfigurationFile(ConfigurationFile, report, out SharedObjects.ExternalRelations);
                SharedObjects.hasConfigurationBeenImported = true;
                Log.LogMessage(MessageImportance.High, report.ToString());
            }
            finally
            {
                if (SharedObjects.hasConfigurationBeenImported)
                {
                    Log.LogMessage(MessageImportance.High, "\tConfiguration was imported.");
                }
                else
                {
                    Log.LogMessage(MessageImportance.High, "\tFailed to import configuration.");
                }
            }
            return SharedObjects.hasConfigurationBeenImported;
        }

        [Required]
        public string ConfigurationFile { get; set; }
    }
}
