using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using XH.Framework.Deployment.XT.Controllers;

namespace XT.REST.Deployment.Tasks
{
  public class ListHostedApplications : Task
  {
    public override bool Execute()
    {
      Shared.ConfigurationManager.Profile.WorkingDirectory = WorkingDirectory;
      var fileSystem = new FileSystem(Shared.ConfigurationManager);
      var applications = fileSystem.ExtractHostedApplicationsFromConfiguration();
      HostedApplications = applications.Select(x =>
      {
        Log.LogMessage(MessageImportance.High, $"Hosted Application Found: {x}");
        return new TaskItem(x);
      }).ToArray();
      return true;
    }

    public string WorkingDirectory { get; set; }

    [Output]
    public ITaskItem[] HostedApplications { get; private set; } = new TaskItem[0];
  }
}
