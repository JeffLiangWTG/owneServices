using System.Globalization;
using System.Net;
using Dat.Integration;
using Serilog;
using WTG.DeploymentUtils.FileSystem;
using WTG.DeploymentUtils.NetworkShare;


namespace xHub.DatImplementation.ContainerDeployment
{
	public class NetworkCopy : INetworkCopy
	{
		readonly IFileSystemUtils fileSystemUtils;
		readonly INetworkShareAccessor networkShareAccessor;
		readonly ITaskLogger logger;

		public NetworkCopy(ILogger logger)
		{
			this.logger = new TaskLogger(logger);
			networkShareAccessor = new NetworkShareAccessor();
			this.fileSystemUtils = new FileSystemUtils();
		}

		public void CopyFiles(List<string> zipfiles, Server server, string uniqueFolderId)
		{
			logger.RecordInfo("Copying files...");
			if (server != null && server.ConnectionInfo != null && server.ConnectionInfo.UserName != null && server.ConnectionInfo.DecriptPassword != null)
			{
				logger.RecordInfo($"Network path for copy {server.Name}{server.ConnectionInfo.UserName}{server.ConnectionInfo.DecriptPassword}");

				var uri = new Uri(server.RemotePath);
				var MachineName = string.Format(CultureInfo.InvariantCulture, "\\\\{0}", (uri.IsUnc ? uri.Host : Dns.GetHostName()).ToUpperInvariant());

				using (networkShareAccessor.NetworkShare(MachineName, server.ConnectionInfo.UserName, server.ConnectionInfo.DecriptPassword, false))
				{
					var remotePath = server.RemotePath;
					logger.RecordInfo($"Network share remote path for copy {remotePath}");
					var targetDirectory = Path.Combine(remotePath, uniqueFolderId);
					foreach (var zipfile in zipfiles)
					{
						var filename = Path.GetFileName(zipfile);
						var temp = Path.Combine(targetDirectory, filename);
						logger.RecordInfo(FormattableString.Invariant($"Copying files over from {zipfile} to {temp} - TargetDirectory {targetDirectory} - Filename {temp}"));
						Directory.CreateDirectory(targetDirectory);
						fileSystemUtils.Copy(zipfile, temp, true, logger);
					}
					logger.RecordInfo(FormattableString.Invariant($"finish copy"));
				}
			}
		}
	}

	public class TaskLogger : ITaskLogger, IDisposable
	{
		private readonly ILogger logger;
		private bool disposedValue;

		public TaskLogger(ILogger log)
		{
			logger = log;
		}
		public void RecordInfo(string message)
		{
			logger.Information(message);
		}

		public IDisposable RecordTask(string taskInfo)
		{
			logger.Information($"Task started: {taskInfo}");
			return this;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					logger.Information("Task done.");
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
