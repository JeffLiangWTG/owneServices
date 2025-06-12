using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XT.REST.Deployment.Tasks
{
  public class PrepareDeployment : Task
  {
    public override bool Execute()
    {
      var success = false;
      Log.LogMessage(MessageImportance.High, "Preparing interface directory.");
      try
      {
        var deployManager = Shared.CreateDeployManager(WorkingDirectory, InterfaceVersion);
        deployManager.PrepareDeployment();
        InstalledInterfaceName = Shared.ConfigurationManager.Interface.InterfaceName;
        InstalledInterfaceVersion = Shared.ConfigurationManager.Interface.InterfaceVersion;
        success = true;
      }
      finally
      {
        Shared.LogResult(success, Log);
      }
      return true;
    }
    public string WorkingDirectory { get; set; }
    public string InterfaceVersion { get; set; }
    [Output]
    public string InstalledInterfaceName { get; private set; }
    [Output]
    public string InstalledInterfaceVersion { get; private set; }
  }
}
