using System.Threading.Tasks;
using System.IO;
using System;
using Docker.DotNet;
using SharpCompress.Writers;
using SharpCompress.Common;
using Docker.DotNet.Models;
using SharpCompress.Archives;
using System.Diagnostics;
using Microsoft.Build.Framework;
using System.Collections.Generic;
using System.Threading;
using XH.Framework.XT.REST.API;

namespace XT.REST.Deployment.Tasks
{
    public class DockerFileCopyTask : Microsoft.Build.Utilities.Task
    {
        public string DockerHostAddress { get; set; }
        public string ContainerId { get; set; }
        public string LocalPath { get; set; }
        public string TargetPath { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Start to copy");

            var success = TransferFilesToContainerAsync(DockerHostAddress, ContainerId, LocalPath, TargetPath).GetAwaiter().GetResult();
            Log.LogMessage(MessageImportance.High, "EndCopy");

            return success;
        }

        private async Task<bool> TransferFilesToContainerAsync(string dockerHostAddress, string containerId, string directoryPath, string destinationPathInContainer)
        {
            var client = new DockerClientConfiguration(new Uri(dockerHostAddress)).CreateClient();
            string tarFilePath = CreateTar(directoryPath);
            client.DefaultTimeout = TimeSpan.FromMinutes(10);

            try
            {
                var container = await client.Containers.InspectContainerAsync(containerId);
                if (!container.State.Running)
                {
                    await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
                }

                await CreateDirectoryInContainerAsync(client, containerId, destinationPathInContainer);

                await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 30 });

                using (var stream = File.OpenRead(tarFilePath))
                {
                    var parameters = new ContainerPathStatParameters
                    {
                        AllowOverwriteDirWithFile = true,
                        Path = destinationPathInContainer
                    };

                    await client.Containers.ExtractArchiveToContainerAsync(containerId, parameters, stream);
                }

                await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
            finally
            {
                File.Delete(tarFilePath); // Clean up the temporary TAR file
            }
        }

        public async Task CreateDirectoryInContainerAsync(IDockerClient client, string containerId, string directoryPath)
        {
            var command = new List<string> { "powershell", "-Command", $"New-Item -Path '{directoryPath}' -ItemType Directory -Force" };
            var execCreateParams = new ContainerExecCreateParameters
            {
                Cmd = command,
                AttachStdout = true,
                AttachStderr = true
            };

            try
            {
                var execInstance = await client.Exec.ExecCreateContainerAsync(containerId, execCreateParams);

                using (var execStream = await client.Exec.StartAndAttachContainerExecAsync(execInstance.ID, true))
                {
                    var (stdOut, stdErr) = await execStream.ReadOutputToEndAsync(CancellationToken.None);

                    if (!string.IsNullOrEmpty(stdOut))
                    {
                        Console.WriteLine($"Output: {stdOut}");
                    }
                    if (!string.IsNullOrEmpty(stdErr))
                    {
                        Console.WriteLine($"Errors: {stdErr}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while creating directory: {ex.Message}");
            }
        }

        public static string CreateTar(string directoryPath)
        {
            string tarFilePath = Path.GetTempFileName();

            using (var stream = File.OpenWrite(tarFilePath))
            using (var tarArchive = SharpCompress.Archives.Tar.TarArchive.Create())
            {
                AddDirectoryToTar(tarArchive, directoryPath, string.Empty);
                var writerOptions = new WriterOptions(CompressionType.None);
                tarArchive.SaveTo(stream, writerOptions);
            }

            return tarFilePath;
        }

        static void AddDirectoryToTar(IWritableArchive tarArchive, string sourceDirectory, string baseDirectory)
        {
            foreach (var filePath in Directory.GetFiles(sourceDirectory))
            {
                var entryPath = Path.Combine(baseDirectory, Path.GetFileName(filePath));
                tarArchive.AddEntry(entryPath, filePath);
            }

            foreach (var directory in Directory.GetDirectories(sourceDirectory))
            {
                var newBase = Path.Combine(baseDirectory, Path.GetFileName(directory));
                AddDirectoryToTar(tarArchive, directory, newBase);
            }
        }
    }
}
