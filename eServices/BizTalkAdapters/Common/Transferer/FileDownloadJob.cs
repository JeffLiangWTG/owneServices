using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public class FileDownloadJob
	{
		readonly TransferrerProperties.Receive properties;
		readonly ConcurrentTransferrerPool transferrerPool;
		readonly TransferrerFileInfo fileInfo;
		readonly TransferrerProperties.Receive.Location location;
		readonly TaskCompletionSource<bool> processCompletedSource;
		readonly FileDownloadJob previousJob;
		string currentPath;
		string originalFilePath;
		string renamedBeforeFilePath;
		string renamedAfterFilePath;

		public FileDownloadJob(int fileIndex, TransferrerFileInfo fileInfo, TransferrerProperties.Receive.Location location, TransferrerProperties.Receive properties, ConcurrentTransferrerPool transferrerPool, FileDownloadJob previousJob = null)
		{
			FileIndex = fileIndex;
			this.fileInfo = fileInfo;
			this.location = location;
			this.properties = properties;
			this.transferrerPool = transferrerPool;
			this.previousJob = previousJob;
			processCompletedSource = new TaskCompletionSource<bool>();
		}

		Stream Download(CancellationTokenSource cancelTokenSource)
		{
			if (cancelTokenSource.IsCancellationRequested) throw new OperationCanceledException();
			Stream fileStream = null;

			transferrerPool.InvokeActionWithTransferrer(transferrer =>
			{
				TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Processing file: Name = '{0}', Size = {1}, Timestamp = {2:s}", fileInfo.Name, fileInfo.Size, fileInfo.Timestamp);
				currentPath = Path.Combine(location.Folder, fileInfo.Name).Replace('\\', '/');
				originalFilePath = currentPath;

				if (!String.IsNullOrWhiteSpace(properties.MoveBeforeDownload) || !String.IsNullOrWhiteSpace(properties.RenameBeforeDownload))
				{
					renamedBeforeFilePath = GetRenameFilePath(properties.MoveBeforeDownload, properties.RenameBeforeDownload, location.Folder, fileInfo.Name);
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Renaming file from '{0}' to '{1}'", currentPath, renamedBeforeFilePath);
					transferrer.RenameFile(currentPath, renamedBeforeFilePath);
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Renamed file from '{0}' to '{1}'", currentPath, renamedBeforeFilePath);
					currentPath = renamedBeforeFilePath;
				}

				TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Getting file '{0}'", currentPath);
				fileStream = transferrer.GetFile(currentPath);
			});

			return fileStream;
		}

		void Submit(ISyncReceiveSubmitBatchFactory batchFactory, IBTTransportProxy transportProxy, ControlledTermination control, string transportType, IBaseMessageFactory baseMessageFactory, ITransferrerMessageFactory transferrerMessageFactory,
					CancellationTokenSource cancelTokenSource, Stream fileStream)
		{
			if (cancelTokenSource.IsCancellationRequested) throw new OperationCanceledException();

			try
			{
				using (var batch = batchFactory.CreateBatch(transportProxy, control, 1))
				{
					if (!String.IsNullOrWhiteSpace(properties.MoveAfterDownload) || !String.IsNullOrWhiteSpace(properties.RenameAfterDownload))
					{
						renamedAfterFilePath = GetRenameFilePath(properties.MoveAfterDownload, properties.RenameAfterDownload, location.Folder, fileInfo.Name);
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Renaming file from '{0}' to '{1}'", currentPath, renamedAfterFilePath);
						transferrerPool.InvokeActionWithTransferrer(t => t.RenameFile(currentPath, renamedAfterFilePath));
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Renamed file from '{0}' to '{1}'", currentPath, renamedAfterFilePath);
					}

					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Creating BizTalk message.");
					var msg = transferrerMessageFactory.CreateMessage(baseMessageFactory, fileInfo.Name, location.Uri, properties.Uri, transportType, fileStream);
					batch.SubmitMessage(msg);
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Submitting message to BizTalk.");
					batch.Done();
					if (batch.Wait())
					{
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Submitted file '{0}' to BizTalk. Message ID = '{1}'", fileInfo.Name, msg.MessageID);
						msg.BodyPart.GetOriginalDataStream().Close();

						if (String.IsNullOrWhiteSpace(properties.MoveAfterDownload) && String.IsNullOrWhiteSpace(properties.RenameAfterDownload))
						{
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Deleting file '{0}'", currentPath);
							transferrerPool.InvokeActionWithTransferrer(t => t.DeleteFile(currentPath));
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Deleted file '{0}'", currentPath);
						}
						if (!String.IsNullOrWhiteSpace(properties.FlagFile))
						{
							string flagPath = Path.Combine(location.Folder, ReplaceFileNamePlaceholders(properties.FlagFile, fileInfo.Name)).Replace('\\', '/');
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Deleting flag file '{0}'", flagPath);
							transferrerPool.InvokeActionWithTransferrer(t => t.DeleteFile(flagPath));
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Deleted flag file '{0}'", flagPath);
						}

						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, $"Downloaded {FileIndex + 1} files.");
					}
					else
					{
						throw new AdapterException(String.Format("Message failure when submitting to BizTalk. File Name = '{0}'", fileInfo.Name));
					}
				}
			}
			catch
			{
				TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Unable to submit message to BizTalk.");
				throw;
			}
		}

		void Restore()
		{
			if (String.IsNullOrWhiteSpace(renamedBeforeFilePath) && String.IsNullOrWhiteSpace(renamedAfterFilePath))
			{
				return;
			}

			string renamedFilePath = !String.IsNullOrWhiteSpace(renamedAfterFilePath) ? renamedAfterFilePath : renamedBeforeFilePath;
			TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Renaming file '{0}' back to '{1}'", renamedFilePath, originalFilePath);
			transferrerPool.InvokeActionWithTransferrer(t => t.RenameFile(renamedFilePath, originalFilePath));
			TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Renamed file '{0}' back to '{1}'", renamedFilePath, originalFilePath);
		}

		public async Task Process(CancellationTokenSource cancelTokenSource, ISyncReceiveSubmitBatchFactory batchFactory, IBTTransportProxy transportProxy,
									ControlledTermination control, string transportType, IBaseMessageFactory baseMessageFactory, ITransferrerMessageFactory transferrerMessageFactory)
		{
			try
			{
				Stream fileStream = null;

				for (int attempts = properties.DownloadRetries; attempts >= 0; attempts--)
				{
					cancelTokenSource.Token.ThrowIfCancellationRequested();
					try
					{
						fileStream = Download(cancelTokenSource);
						break;
					}
					catch when (attempts > 0)
					{
						await Task.Delay(properties.DownloadRetryDelay, cancelTokenSource.Token);
					}
				}

				if (previousJob != null)
				{
					await previousJob.ProcessCompletedTask;
					await Task.Yield();
					if (!previousJob.IsProcessed) throw new OperationCanceledException();
				}
				Submit(batchFactory, transportProxy, control, transportType, baseMessageFactory, transferrerMessageFactory, cancelTokenSource, fileStream);
				processCompletedSource.SetResult(true);
			}
			catch
			{
				processCompletedSource.SetResult(false);
				Restore();
				throw;
			}
		}

		public Task ProcessInBackground(CancellationTokenSource endPointLocationCancelTokenSource, CancellationTokenSource locationCancelTokenSource, ISyncReceiveSubmitBatchFactory batchFactory, IBTTransportProxy transportProxy,
											ControlledTermination control, string transportType, IBaseMessageFactory baseMessageFactory, ITransferrerMessageFactory transferrerMessageFactory)
		{
			return Task.Run(async () =>
			{
				try
				{
					await Process(endPointLocationCancelTokenSource, batchFactory, transportProxy, control, transportType, baseMessageFactory, transferrerMessageFactory);
				}
				catch
				{
					if (!locationCancelTokenSource.IsCancellationRequested) locationCancelTokenSource.Cancel();
					throw;
				}
			});
		}

		public int FileIndex { get; private set; }
		public Task<bool> ProcessCompletedTask { get => processCompletedSource.Task; }
		public bool IsProcessed { get => processCompletedSource.Task.IsCompleted ? processCompletedSource.Task.Result : false; }

		static string GetRenameFilePath(string moveFolder, string renameFileName, string folder, string fileName)
		{
			renameFileName = ReplaceFileNamePlaceholders(renameFileName, fileName);

			string renamePath = Path.Combine(
				String.IsNullOrWhiteSpace(moveFolder) ? folder : moveFolder,
				String.IsNullOrWhiteSpace(renameFileName) ? fileName : renameFileName);

			return renamePath.Replace('\\', '/');
		}

		internal static string ReplaceFileNamePlaceholders(string targetFileName, string sourceFileName)
		{
			targetFileName = targetFileName.Replace("{f}", sourceFileName)
								.Replace("{n}", Path.GetFileNameWithoutExtension(sourceFileName))
								.Replace("{x}", Path.GetExtension(sourceFileName));
			return targetFileName;
		}
	}
}
