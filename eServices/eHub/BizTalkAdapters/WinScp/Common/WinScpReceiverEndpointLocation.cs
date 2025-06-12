using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.BizTalkAdapters.WinScp.Common
{
	internal class WinScpReceiverEndpointLocation : IDisposable
	{
		private readonly WinScpReceiverEndpoint receiverEndpoint;
		private readonly string locnActivityId;
		private readonly WinScpLocation location;
		private readonly CancellationToken cancelToken;
		private readonly ConcurrentBag<IWinScpClient> connectionPool = new();
		private int connectionCounter = 0;
		private List<WinScpFileInfo> listing = new();
		private ConcurrentQueue<WinScpFileInfo> emptyFiles = new();
		private int skippedFileCount = 0;
		internal int SkippedFileCount => skippedFileCount;
		internal int EmptyFileCount => emptyFiles.Count;
		internal TimeSpan DownloadTime { get; private set; }

		public WinScpReceiverEndpointLocation(
			WinScpReceiverEndpoint receiverEndpoint,
			string locnActivityId,
			WinScpLocation location,
			CancellationToken cancelToken)
		{
			this.receiverEndpoint = receiverEndpoint;
			this.locnActivityId = locnActivityId;
			this.location = location;
			this.cancelToken = cancelToken;
		}

		internal async Task DownloadAndSubmitLocation()
		{
			var maximumConcurrentDownloads = receiverEndpoint.Config.Locations.Count == 1 ? receiverEndpoint.Config.MaximumConcurrentDownloads : 1;
			receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Info, "Processing receive location '{0}' with {1} concurrent downloads.", location, maximumConcurrentDownloads);
			var timer = Stopwatch.StartNew();
			IWinScpClient winScpClient = null;

			try
			{
				winScpClient = await CreateLocationClient();
				cancelToken.ThrowIfCancellationRequested();

				receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "Listing files for location {0}:", location);
				foreach (var file in await winScpClient.EnumerateRemoteFilesAsync(location, cancelToken))
				{
					cancelToken.ThrowIfCancellationRequested();
					receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "{0};modified={1:s};size={2:D0}", file.Name, file.LastWriteTime, file.Length);
					if (file.Length > 0)
						listing.Add(file);
					else
						emptyFiles.Enqueue(file);
				}
				timer.Stop();
				DownloadTime = timer.Elapsed;

				if (emptyFiles.Count > 0)
				{
					if (receiverEndpoint.Config.EmptyFileOption == "Discard")
					{
						var emptyFileCount = emptyFiles.Count;
						await DeleteEmptyFiles(winScpClient, locnActivityId, location, cancelToken);
						receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Info, "{0} empty files discarded from location {1}", emptyFileCount, location);
					}
					else
					{
						receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Info, "{0} empty files skipped for location {1}", emptyFiles.Count, location);
					}
				}
				receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Info, "{0} files selected for download from location {1}", listing.Count, location);

				if (listing.Count == 0)
					return;

				if (receiverEndpoint.Config.SortOrder != WinScpSortOrder.None)
				{
					cancelToken.ThrowIfCancellationRequested();
					receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "Sorting by {0}", receiverEndpoint.Config.SortOrder);
					switch (receiverEndpoint.Config.SortOrder)
					{
						case WinScpSortOrder.Name:
							listing.Sort((i1, i2) => string.Compare(i1.Name, i2.Name, StringComparison.Ordinal));
							break;
						case WinScpSortOrder.Timestamp:
							listing.Sort((i1, i2) => i1.LastWriteTime.CompareTo(i2.LastWriteTime));
							break;
					}
				}

				cancelToken.ThrowIfCancellationRequested();
				var maxClients = Math.Min(listing.Count, maximumConcurrentDownloads);
				SaveLocationClient(ref winScpClient);
				await Task.WhenAll(Enumerable.Range(0, maxClients - 1)
					.Select(async _ => connectionPool.Add(await CreateLocationClient())));
				receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "Starting download for {0} with {1} clients.", location, connectionPool.Count);

				cancelToken.ThrowIfCancellationRequested();
				var downloadCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
				var downloadCancelToken = maximumConcurrentDownloads > 1 ? downloadCancelTokenSource.Token : CancellationToken.None;
				var downloadTasks = new Queue<Task<DownloadResult>>();
				var exceptions = new List<Exception>();

				int i = 0;
				for (; i < maxClients; i++)
				{
					downloadTasks.Enqueue(DownloadMessage(listing[i], i, downloadCancelToken));
				}

				while (downloadTasks.Count > 0)
				{
					DownloadResult downloadResult = null;
					try
					{
						try
						{
							downloadResult = await downloadTasks.Dequeue();
							if (downloadResult.messageStream != null)
							{
								await SubmitMessage(downloadResult, cancelToken);
							}
						}
						finally
						{
							if (downloadResult is not null)
							{
								SaveLocationClient(ref downloadResult.downloadClient);
								downloadResult.Dispose();
							}
						}
						
						if (i < listing.Count && !cancelToken.IsCancellationRequested && !downloadCancelToken.IsCancellationRequested)
						{
							downloadTasks.Enqueue(DownloadMessage(listing[i], i, downloadCancelToken));
							i++;
						}
					}
					catch (OperationCanceledException) { }
					catch (Exception ex)
					{
						downloadCancelTokenSource.Cancel();
						exceptions.Add(ex);
					}
				}

				switch (exceptions.Count)
				{
					case 1:
						throw exceptions[0];
					case > 1:
						throw new AggregateException(exceptions);
				}
			}
			finally
			{
				SaveLocationClient(ref winScpClient);
				receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "Finished processing location '{0}'.", location);
				receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "Disposing {0} connection(s).", connectionPool.Count);
				connectionPool.AsParallel().ForAll(x => x.Dispose());
			}
		}

		private async Task<DownloadResult> DownloadMessage(WinScpFileInfo fileInfo, int fileIdx, CancellationToken downloadCancelToken)
		{
			downloadCancelToken.ThrowIfCancellationRequested();
			var downloadActivityId = $"{locnActivityId}+F{fileIdx:D4}";
			receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Processing file [{0}/{1}]: {2}", fileIdx + 1, listing.Count, fileInfo.FullName);
			IWinScpClient downloadClient = null;
			Stream downloadStream = null;
			try
			{
				if (!connectionPool.TryTake(out downloadClient))
					throw new InvalidOperationException("No connections in pool.");
				var itemLocn = location.Clone();
				itemLocn.FileName = fileInfo.Name;
				WinScpLocation flagLocn = null;
				WinScpLocation beforeLocn = null;

				if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.FlagFile))
				{
					downloadCancelToken.ThrowIfCancellationRequested();
					flagLocn = location.Clone();
					flagLocn.FileName = ReceiverEndpoint.ReplaceFileNamePlaceholders(receiverEndpoint.Config.FlagFile, fileInfo.Name);
					receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Checking for flag file: {0}", flagLocn.GetPath());
					if (!await downloadClient.FileExistsAsync(flagLocn, downloadCancelToken))
					{
						receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Skipping file without flag file: {0}", fileInfo.FullName);
						Interlocked.Increment(ref skippedFileCount);
						return new DownloadResult(downloadActivityId, downloadClient, fileInfo, fileIdx, null, location, flagLocn);
					}
				}

				if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.MoveBeforeDownload) || !string.IsNullOrWhiteSpace(receiverEndpoint.Config.RenameBeforeDownload))
				{
					downloadCancelToken.ThrowIfCancellationRequested();
					beforeLocn = itemLocn.Clone();
					if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.MoveBeforeDownload))
						beforeLocn.Folder = receiverEndpoint.Config.MoveBeforeDownload;
					if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.RenameBeforeDownload))
						beforeLocn.FileName = ReceiverEndpoint.ReplaceFileNamePlaceholders(receiverEndpoint.Config.RenameBeforeDownload, fileInfo.Name);
					receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Before download moving {0} to {1}.", itemLocn.GetPath(), beforeLocn.GetPath());
					await downloadClient.MoveFileAsync(itemLocn, beforeLocn, downloadCancelToken);
				}

				downloadCancelToken.ThrowIfCancellationRequested();
				var downloadLocn = beforeLocn ?? itemLocn;
				receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Downloading {0}.", downloadLocn.GetPath());
				var messageStream = new VirtualStream();
				downloadStream = await downloadClient.GetFileAsync(downloadLocn, downloadCancelToken);
				await downloadStream.CopyToAsync(messageStream, 81920, downloadCancelToken);
				messageStream.Position = 0;
				receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Finished downloading message: {0}", fileInfo.FullName);

				if (messageStream.Length <= 0)
				{
					receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Warn, "Retrieved zero byte stream from WinSCP download file: {0}", fileInfo.FullName);
					messageStream.Dispose();
					messageStream = null;
					emptyFiles.Enqueue(fileInfo);
				}

				return new DownloadResult(downloadActivityId, downloadClient, fileInfo, fileIdx, messageStream, downloadLocn, flagLocn);
			}
			catch
			{
				SaveLocationClient(ref downloadClient);
				if (downloadStream != null)
				{
					receiverEndpoint.Logger.Log(downloadActivityId, LogLevel.Debug, "Discarding download stream: {0}", fileInfo.FullName);
					try
					{
						using (var reader = new StreamReader(downloadStream))
							_ = await reader.ReadToEndAsync();
					}
					catch { }
				}
				throw;
			}
			finally
			{
				downloadStream?.Dispose();
			}
		}

		private async Task SubmitMessage(DownloadResult downloadResult, CancellationToken cancelToken)
		{

			cancelToken.ThrowIfCancellationRequested();
			receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Debug,
				"Preparing to submit message [{0}/{1}] to BizTalk: {2}", downloadResult.fileIdx + 1, listing.Count,
				downloadResult.fileInfo.FullName);
			if (receiverEndpoint.SubmitMessageToBizTalk(downloadResult.messageStream, downloadResult.fileInfo.FullName,
				    location.GetUri(), downloadResult.downloadActivityId))
			{
				receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Debug,
					"Successfully submitted message to BizTalk: {0}", downloadResult.fileInfo.FullName);

				var retries = 0;
				while (retries < receiverEndpoint.Config.TransferErrorsRetryCount)
				{
					try
					{
						if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.MoveAfterDownload) ||
						    !string.IsNullOrWhiteSpace(receiverEndpoint.Config.RenameAfterDownload))
						{
							var afterLocn = downloadResult.downloadLocn.Clone();
							if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.MoveAfterDownload))
								afterLocn.Folder = receiverEndpoint.Config.MoveAfterDownload;
							if (!string.IsNullOrWhiteSpace(receiverEndpoint.Config.RenameAfterDownload))
								afterLocn.FileName = ReceiverEndpoint.ReplaceFileNamePlaceholders(
									receiverEndpoint.Config.RenameAfterDownload, downloadResult.downloadLocn.FileName);

							if (await downloadResult.downloadClient.FileExistsAsync(afterLocn, CancellationToken.None))
							{
								receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Debug,
									"Deleting duplicate server file: {0}", afterLocn.GetPath());
								await downloadResult.downloadClient.RemoveFileAsync(afterLocn, CancellationToken.None);
							}

							receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Debug,
								"After download moving {0} to {1}.", downloadResult.downloadLocn, afterLocn);
							await downloadResult.downloadClient.MoveFileAsync(downloadResult.downloadLocn, afterLocn,
								CancellationToken.None);
						}
						else
						{
							receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Debug,
								"Deleting server file: {0}", downloadResult.downloadLocn.GetPath());
							await downloadResult.downloadClient.RemoveFileAsync(downloadResult.downloadLocn,
								CancellationToken.None);
						}

						if (downloadResult.flagLocn != null)
						{
							receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Debug,
								"Deleting flag file: {0}", downloadResult.flagLocn);
							await downloadResult.downloadClient.RemoveFileAsync(downloadResult.flagLocn,
								CancellationToken.None);
						}

						break;
					}
					catch (Exception ex)
					{
						retries++;

						var checkConnectionException = ex;
						bool hasConnectionError = false;
						int connectionRetryCount = 0;
						while (checkConnectionException != null)
						{
							if (checkConnectionException is WinSCP.SessionRemoteException ||
							    checkConnectionException is WinSCP.SessionLocalException ||
							    checkConnectionException is TransferrerException)
							{
								hasConnectionError = true;
								break;
							}

							checkConnectionException = checkConnectionException.InnerException;
						}

						if (hasConnectionError)
						{
							downloadResult.downloadClient.Dispose();
							int connectionRetryLimit = receiverEndpoint.Config.TransferErrorsDisablePort
								? int.MaxValue
								: receiverEndpoint.Config.TransferErrorsRetryCount;
							while (connectionRetryCount < connectionRetryLimit)
							{
								try
								{
									receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Warn,
										$"Exception processing file: {downloadResult.downloadLocn.GetPath()} - Retrying {connectionRetryCount} of {connectionRetryLimit}",
										ex);
									await Task.Delay(receiverEndpoint.Config.TransferErrorsRetryInterval * 1000, cancelToken);

									connectionRetryCount++;
									downloadResult.downloadClient = await CreateLocationClient();
									break;
								}
								catch (OperationCanceledException)
								{
									if (receiverEndpoint.Config.TransferErrorsDisablePort)
									{
										DisableBiztalkLocation(downloadResult);
									}

									throw;
								}
								catch (Exception e)
								{
									if (connectionRetryCount == connectionRetryLimit)
									{
										receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Error,
											$"Exception processing file: {downloadResult.downloadLocn.GetPath()} - Retries Exhausted",
											e);
										throw;
									}
								}
							}
						}

						if (retries == receiverEndpoint.Config.TransferErrorsRetryCount)
						{
							receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Error,
								$"Exception processing file: {downloadResult.downloadLocn.GetPath()} - Retries Exhausted",
								ex);
							if (receiverEndpoint.Config.TransferErrorsDisablePort)
							{
								DisableBiztalkLocation(downloadResult);
							}

							throw;
						}
					}
				}
			}

			receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Info,
				"Successfully received file: {0}", downloadResult.downloadLocn);
		}

		private void DisableBiztalkLocation(DownloadResult downloadResult)
		{
			receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Error,
				$"OperationCanceledException processing file: {downloadResult.downloadLocn.GetPath()} - TransferErrorsDisablePort:{receiverEndpoint.Config.TransferErrorsDisablePort} - Starting to disable Biztalk receive location ");
			try
			{
				string scope = "ROOT\\MicrosoftBizTalkServer";
				string query = $"SELECT * FROM MSBTS_ReceiveLocation WHERE Name = '{receiverEndpoint.PortName}'";

				using (ManagementObjectSearcher search = new ManagementObjectSearcher(scope, query))
				using (ManagementObjectCollection results = search.Get())
				{
					foreach (ManagementObject receiveLocation in results)
					{
						receiveLocation.InvokeMethod("Disable", null);
						receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Info,
							$"Stopped receive location: {receiverEndpoint.PortName}");
					}
				}
			}
			catch (Exception exStopPort)
			{
				receiverEndpoint.Logger.Log(downloadResult.downloadActivityId, LogLevel.Error,
					$"Error stopping BizTalk receive location: {receiverEndpoint.PortName}", exStopPort);
			}
		}

		private async Task<IWinScpClient> CreateLocationClient()
		{
			cancelToken.ThrowIfCancellationRequested();
			var clientActivityId = $"{locnActivityId}+C{Interlocked.Increment(ref connectionCounter):D4}";
			receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Trace, "Creating client with ID ({0}) for location: {1}", clientActivityId, location);
			var newClient = await receiverEndpoint.winScpClientFactory.CreateClientAsync(location, receiverEndpoint.Config, receiverEndpoint.Logger, clientActivityId, cancelToken);
			try
			{
				await newClient.OpenAsync(cancelToken);
				return newClient;
			}
			catch (Exception)
			{
				SaveLocationClient(ref newClient);
				throw;
			}
		}

		private void SaveLocationClient(ref IWinScpClient saveClient)
		{
			if (saveClient != null && !connectionPool.Contains(saveClient))
			{
				connectionPool.Add(saveClient);
			}
			saveClient = null;
		}

		private async Task DeleteEmptyFiles(IWinScpClient winScpClient, string locnActivityId, WinScpLocation location, CancellationToken cancelToken)
		{
			var totalCount = emptyFiles.Count;
			var index = 0;
			while (emptyFiles.TryDequeue(out var emptyFile))
			{
				cancelToken.ThrowIfCancellationRequested();

				index++;
				var item = location.Clone();
				item.FileName = emptyFile.Name;

				receiverEndpoint.Logger.Log(locnActivityId, LogLevel.Debug, "Deleting empty file [{0}/{1}]: {2}", index, totalCount, emptyFile.FullName);
				await winScpClient.RemoveFileAsync(item, cancelToken);
			}
		}

		public void Dispose()
		{
			while (connectionPool.TryTake(out var client))
			{
				client.Dispose();
			}

			while (!emptyFiles.IsEmpty)
			{
				emptyFiles.TryDequeue(out _);
			}

			listing.Clear();
		}

		internal class DownloadResult : IDisposable
		{
			public string downloadActivityId;
			public IWinScpClient downloadClient;
			public WinScpFileInfo fileInfo;
			public int fileIdx;
			public Stream messageStream;
			public WinScpLocation downloadLocn;
			public WinScpLocation flagLocn;

			public DownloadResult(string downloadActivityId, IWinScpClient downloadClient, WinScpFileInfo fileInfo, int fileIdx, Stream messageStream, WinScpLocation downloadLocn, WinScpLocation flagLocn)
			{
				this.downloadActivityId = downloadActivityId;
				this.downloadClient = downloadClient;
				this.fileInfo = fileInfo;
				this.fileIdx = fileIdx;
				this.messageStream = messageStream;
				this.downloadLocn = downloadLocn;
				this.flagLocn = flagLocn;
			}

			public void Dispose()
			{
				messageStream?.Dispose();
				messageStream = null;
				downloadClient = null;
			}
		}
	}
}
