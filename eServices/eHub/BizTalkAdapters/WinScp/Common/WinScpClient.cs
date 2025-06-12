using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using WinSCP;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	[ExcludeFromCodeCoverage]
	public class WinScpClient : IWinScpClient
	{
		private string winScpLogFile;
		private CancellationTokenSource logCancelTokenSource;
		private Task logMonitorTask;
		private TaskCompletionSource<bool> logMonitorTrigger;
		private readonly Session session;
		private readonly ILog logger;
		private readonly string activityId;
		private Stream downloadStream;
		private bool disposed;

		public WinScpClient(WinScpLocation location, IWinScpConfiguration config, ILog logger, string activityId)
		{
			Location = location ?? throw new ArgumentNullException(nameof(location));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.activityId = activityId ?? throw new ArgumentNullException(nameof(activityId));
			SessionOptions.HostName = Location.Server;
			if (Location.Port.HasValue)
				SessionOptions.PortNumber = Location.Port.Value;
			SessionOptions.UserName = Location.UserName;
			SessionOptions.Password = Location.Password;
			SessionOptions.TimeoutInMilliseconds = config.Timeout;

			session = new Session();

			if (logger.IsTraceEnabled)
			{
				winScpLogFile = Path.GetTempFileName();
				session.SessionLogPath = winScpLogFile;
				logCancelTokenSource = new CancellationTokenSource();
				logger.Log(activityId, LogLevel.Trace, "Writing WinSCP log to {0}", winScpLogFile);
				logMonitorTask = MonitorWinScpLog(logCancelTokenSource.Token);
			}
		}

		public WinScpLocation Location { get; }
		public SessionOptions SessionOptions { get; } = new SessionOptions();

		public async Task OpenAsync(CancellationToken cancelToken)
			=> await ExecuteCommandAsync(Open, cancelToken);

		public void Open()
			=> session.Open(SessionOptions);

		public async Task CloseAsync(CancellationToken cancelToken)
			=> await ExecuteCommandAsync(Close, cancelToken);

		public void Close()
		{
			if (session.Opened) session.Close();
		}

		public async Task PutFileAsync(Stream stream, WinScpLocation location, CancellationToken cancelToken)
			=> await ExecuteCommandAsync(() => PutFile(stream, location), cancelToken);

		public void PutFile(Stream stream, WinScpLocation location)
			=> session.PutFile(stream, location.GetPath());

		public Task<Stream> GetFileAsync(WinScpLocation location, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();
			return Task.FromResult(GetFile(location));
		}

		public Stream GetFile(WinScpLocation location)
			=> (downloadStream = session.GetFile(location.GetPath()));

		public async Task MoveFileAsync(WinScpLocation fromPath, WinScpLocation toPath, CancellationToken cancelToken)
			=> await ExecuteCommandAsync(() => MoveFile(fromPath, toPath), cancelToken);

		public void MoveFile(WinScpLocation fromPath, WinScpLocation toPath)
			=> session.MoveFile(fromPath.GetPath(), toPath.GetPath());

		public async Task RemoveFileAsync(WinScpLocation downloadLocn, CancellationToken cancelToken)
			=> await ExecuteCommandAsync(() => RemoveFile(downloadLocn), cancelToken);

		public void RemoveFile(WinScpLocation downloadLocn)
		{
			var path = downloadLocn.GetPath();
			if (session.FileExists(path))
				session.RemoveFile(path);
		}

		public async Task<IEnumerable<WinScpFileInfo>> EnumerateRemoteFilesAsync(WinScpLocation location, CancellationToken cancelToken)
			=> await ExecuteCommandAsync(() => EnumerateRemoteFiles(location), cancelToken);

		public IEnumerable<WinScpFileInfo> EnumerateRemoteFiles(WinScpLocation location)
			=> session.EnumerateRemoteFiles(string.IsNullOrWhiteSpace(location.Folder) ? "." : location.Folder, location.FileName ?? string.Empty, EnumerationOptions.None)
				.Select(f => new WinScpFileInfo(f.Name, f.FullName, f.Length, f.LastWriteTime));

		public async Task<bool> FileExistsAsync(WinScpLocation location, CancellationToken cancelToken)
			=> await ExecuteCommandAsync(() => FileExists(location), cancelToken);

		public bool FileExists(WinScpLocation location)
			=> session.FileExists(location.GetPath());

		private async Task ExecuteCommandAsync(Action command, CancellationToken cancelToken)
		{
			cancelToken.ThrowIfCancellationRequested();
			try
			{
				var task = Task.Run(command, cancelToken);
				logMonitorTrigger?.TrySetResult(true);
				await await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancelToken));
			}
			catch (Exception ex) when (ex is (SessionException or TimeoutException))
			{
				throw new TransferrerException(ex);
			}
		}

		private async Task<T> ExecuteCommandAsync<T>(Func<T> command, CancellationToken cancelToken)
		{
			T result = default;
			await ExecuteCommandAsync(() => { result = command(); }, cancelToken);
			return result;
		}

		private async Task MonitorWinScpLog(CancellationToken cancelToken)
		{
			logMonitorTrigger = new TaskCompletionSource<bool>();
			using (var logStream = new FileStream(winScpLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var logReader = new StreamReader(logStream))
			{
				try
				{
					while (!cancelToken.IsCancellationRequested)
					{
						var line = await Task.Run(() => logReader.ReadLine(), cancelToken);
						if (line != null)
						{
							logger.Log(activityId, LogLevel.Trace, line);
						}
						else
						{
							if (await Task.WhenAny(logMonitorTrigger.Task, Task.Delay(100, cancelToken)) == logMonitorTrigger.Task)
								logMonitorTrigger = new TaskCompletionSource<bool>();
						}
					}
				}
				catch (OperationCanceledException) { }
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			try
			{
				if (downloadStream is not null and { CanRead: true })
					using (var reader = new StreamReader(downloadStream))
						_ = reader.ReadToEnd();
			}
			catch { }

			try
			{
				logMonitorTrigger?.TrySetResult(true);
				session?.Dispose();
				logCancelTokenSource?.Cancel();
				logCancelTokenSource = null;
				logMonitorTask?.Wait();
				logMonitorTask = null;
				if (winScpLogFile != null)
				{
					File.Delete(winScpLogFile);
					winScpLogFile = null;
				}
			}
			catch (Exception ex)
			{
				logger?.Log(activityId, LogLevel.Warn, "Exception caught during disposal of " + nameof(WinScpClient) + " instance.", ex);
			}

			disposed = true;
		}

		~WinScpClient()
		{
			Dispose(disposing: false);
		}
	}
}
