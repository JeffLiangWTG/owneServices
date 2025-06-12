using Dat.Integration;
using Dat.Integration.Deployment;
using System;
using System.Diagnostics;
using System.IO;

namespace eHub.DatImplementation.NpmDeployment
{
    public class BuildDeployer : IBuildDeployer
    {
        readonly ITaskLogger logger;

        public BuildDeployer(ITaskLogger logger)
        {
            this.logger = logger;
        }

        public void AutoDeployLatestBuild(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
        {
            throw new NotImplementedException();
        }

        public void AutoDeployTestedShelf(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath, TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

        public void DeployOnDemand(string buildConfiguration, string deploymentConfiguration, string sourcePath, string binPath)
        {
            using (logger.RecordTask($"Deploying: deploymentConfiguration={deploymentConfiguration} with sourcePath={sourcePath} and binPath={binPath}"))
            {
                var configuration = new DeploymentConfiguration(deploymentConfiguration);

                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.Combine(binPath, configuration.Directory),
                    FileName = "cmd",
                    Arguments = $"/c {configuration.Command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Environment = { ["DAT_IS_DEPLOYING"] = "true" }
                };

                using (var process = new Process())
                {
                    process.StartInfo = startInfo;

                    process.OutputDataReceived += (_, e) =>
                    {
                        logger.RecordInfo(e.Data);
                    };

                    process.ErrorDataReceived += (_, e) =>
                    {
                        logger.RecordInfo(e.Data);
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"Deployment failed with exit code {process.ExitCode}");
                    }
                }
            }
        }

        public void TeardownTestedShelf(string buildConfiguration, string deploymentConfiguration, TaskInfo taskInfo)
        {
            throw new NotImplementedException();
        }

    }
}
