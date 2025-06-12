using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Diagnostics;


namespace XT.REST.Deployment.Tasks
{
    public class WaitForHostedAppToStopTask : Microsoft.Build.Utilities.Task
    {
        public string DockerHostAddress { get; set; }
        public string ContainerId { get; set; }
        public ITaskItem[] Applications { get; set; }
        public int TimeoutSeconds { get; set; } = 30; // Default timeout

        [Output]
        public bool FailedToStop { get; private set; }
        public override bool Execute()
        {
            try
            {
                Log.LogMessage(MessageImportance.High, "Starting wait for hosted applications to stop in container.");

                var client = new DockerClientConfiguration(new Uri(DockerHostAddress)).CreateClient();

                var result = WaitForApplicationsToStopAsync(client, ContainerId, Applications.Select(a => a.ItemSpec).ToArray()).GetAwaiter().GetResult();

                Log.LogMessage(MessageImportance.High, result ? "All applications stopped successfully." : "Timed out waiting for applications to stop.");
                FailedToStop = !result;
                return result;
            }
            catch (Exception ex)
            {
                Log.LogError($"An error occurred: {ex.Message}");
                FailedToStop = true;
                return false;
            }
        }

        private async Task<bool> WaitForApplicationsToStopAsync(IDockerClient client, string containerId, string[] applications)
        {
            int attempts = 0;
            int maxAttempts = TimeoutSeconds / 5; // Poll every 5 seconds
            bool allStopped = false;

            var container = await client.Containers.InspectContainerAsync(containerId);
            if (!container.State.Running)
            {
                await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
            }

            var execCreateParams = new ContainerExecCreateParameters
            {
                AttachStdout = true,
                AttachStderr = true,
                Cmd = new[] { "powershell", "-Command", "'C:\\Program Files\\Xware\\xT\\bin\\xt.exe' application list" }
            };

            do
            {
                var execInstance = await client.Exec.ExecCreateContainerAsync(containerId, execCreateParams);
                using (var execStream = await client.Exec.StartAndAttachContainerExecAsync(execInstance.ID, true))
                {
                    var (stdOut, stdErr) = await execStream.ReadOutputToEndAsync(CancellationToken.None);

                    allStopped = applications.All(app =>
                    {
                        var appId = app.Replace("xt-hostedapp:{", "").Replace("}", "");
                        return !stdOut.Contains(appId);
                    });

                    if (allStopped)
                    {
                        Log.LogMessage(MessageImportance.Normal, "All applications have stopped.");
                        return true;
                    }

                    Log.LogMessage(MessageImportance.Normal, $"Attempt {attempts + 1}: Waiting for applications to stop.");
                }

                attempts++;
                await System.Threading.Tasks.Task.Delay(5000); // Wait for 5 seconds before the next check
            } while (!allStopped && attempts < maxAttempts);

            return allStopped;
        }
    }
}

