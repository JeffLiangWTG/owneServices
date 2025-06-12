using Docker.DotNet;
using Microsoft.Build.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XH.Framework.Deployment.XT.Controllers;
using XH.Framework.Deployment.XT.Interfaces;

namespace XT.REST.Deployment.Tasks
{
    public class DeployOnContainer : Microsoft.Build.Utilities.Task
    {
        public string ContainerId { get; set; }
        public string DockerHostAddress { get; set; }
        public string WorkingDirectory { get; set; }
        public string InterfaceVersion { get; set; }

        public override bool Execute()
        {
            var success = false;
            Log.LogMessage(MessageImportance.High, "Importing xT configuration");
            try
            {
                var containerIpAddress = GetContainerIPAddressAsync(ContainerId, DockerHostAddress).GetAwaiter().GetResult();
                Shared.ConfigurationManager.Profile.ApiUrl = $"http://{containerIpAddress}:5000";
                Shared.ConfigurationManager.Profile.ServerName = containerIpAddress;

                var fileSystem = new FileSystem(Shared.ConfigurationManager);
                GetDeployManager(fileSystem, Shared.ConfigurationManager).Deploy().GetAwaiter().GetResult();

                success = true;
            }
            finally
            {
                Shared.LogResult(success, Log);
            }
            return true;
        }
        public virtual IDeployManager GetDeployManager(IFileSystem fileSystem, ConfigurationManager configurationManager)
            => new DeployManager(() => fileSystem, null, configurationManager);

        public async Task<string> GetContainerIPAddressAsync(string containerId, string dockerHostAddress)
        {
            var client = new DockerClientConfiguration(new Uri(dockerHostAddress)).CreateClient();
            var containerInspectResponse = await client.Containers.InspectContainerAsync(containerId);
            var ipAddress = containerInspectResponse.NetworkSettings.Networks
                .FirstOrDefault().Value?.IPAddress;

            return ipAddress;
        }
    }
}
