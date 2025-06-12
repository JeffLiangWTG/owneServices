using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Build.Framework;

namespace XT.REST.Deployment.Tasks
{
    public class DeactivateHostedApplicationsTask : Microsoft.Build.Utilities.Task
    {
        public string ContainerId { get; set; }
        public string DockerHostAddress { get; set; }
        public ITaskItem[] Applications { get; set; }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.High, "Starting hosted application deactivation in container.");

                var client = new DockerClientConfiguration(new Uri(DockerHostAddress)).CreateClient();
                ExecutePowerShellInContainerAsync(client, ContainerId, Applications.Select(a => a.ItemSpec).ToArray()).GetAwaiter().GetResult();

                Log.LogMessage(MessageImportance.High, "Hosted applications deactivation completed.");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"An error occurred: {ex.Message}");
                return false;
            }
        }
        public async Task ExecutePowerShellInContainerAsync(IDockerClient client, string containerId, string[] applications)
        {
            var container = await client.Containers.InspectContainerAsync(containerId);
            if (!container.State.Running)
            {
                await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
            }

            // Construct the PowerShell command to deactivate each application
            var commands = applications.Select(app =>
            //$@"& ""C:\Program Files\Xware\xT\bin\xtcmd.exe"" deactivate --uri {app}");

            $@"& ""C:\Program Files\Xware\xT\bin\xtcmd.exe"" deactivate --uri ""{app}""");

            // Combine commands
            var psScript = string.Join("; ", commands);

            // Create the exec instance
            var execCreateParams = new ContainerExecCreateParameters
            {
                AttachStdin = true,
                AttachStdout = true,
                AttachStderr = true,
                Tty = true,
                Cmd = new List<string>
            {
                "powershell",
                "-Command",
                psScript
            }
            };
            var execInstance = await client.Exec.ExecCreateContainerAsync(containerId, execCreateParams);

            // Start the exec instance and capture the output
            using (var execStream = await client.Exec.StartAndAttachContainerExecAsync(execInstance.ID, true))
            {
                var (stdOut, stdErr) = await execStream.ReadOutputToEndAsync(CancellationToken.None);
                Log.LogMessage(MessageImportance.Normal, $"Output: {stdOut}");
                if (!string.IsNullOrEmpty(stdErr))
                {
                    Log.LogWarning($"Errors: {stdErr}");
                }
            }
        }
    }
}
