using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public abstract class TransferrerReceiverEndpoint : ReceiverEndpoint
	{
		protected TransferrerReceiverEndpoint(ITransferrerFactory transferrerFactory, ISyncReceiveSubmitBatchFactory batchFactory, ITransferrerMessageFactory transferrerMessageFactory, TransferrerProperties.IReceiveFactory receivePropertiesFactory)
		{
			this.transferrerFactory = transferrerFactory;
			this.batchFactory = batchFactory;
			this.transferrerMessageFactory = transferrerMessageFactory;
			this.receivePropertiesFactory = receivePropertiesFactory;
			this.receiverEndpoints = TransferrerReceiver.ReceiverEndpoints[GetType()];
			this.adapterLogger = TransferrerReceiver.ReceiverLoggers[GetType()];
			this.hostCancelTokenSource = TransferrerReceiver.HostCancelTokenSource[GetType()];

			TransferrerHelpers.Log(this, this.adapterLogger, LogLevel.Debug, "Initialized receiver endpoint of type '{0}'. Hash Code = {1}", GetType().Name, GetHashCode());
		}

		#region ReceiverEndpoint
		public override void Open(
			string uri,
			IPropertyBag config,
			IPropertyBag bizTalkConfig,
			IPropertyBag handlerPropertyBag,
			IBTTransportProxy transportProxy,
			string transportType,
			string propertyNamespace,
			ControlledTermination control)
		{
			this.uri = uri;
			this.transportType = transportType;
			this.propertyNamespace = propertyNamespace;

			string portName;
			if (!TransferrerReceiver.ReceiveLocationNames.TryGetValue(uri, out portName))
			{
				if (TryGetPortName(out portName))
					TransferrerReceiver.ReceiveLocationNames[uri] = portName;
			}

			TransferrerReceiverEndpoint endpoint;
			if (this.receiverEndpoints.TryGetValue(this.uri, out endpoint) && endpoint != null)
			{
				StopEndpoint(endpoint);
				endpoint.Dispose();
			}

			this.endPointCancelTokenSource = new CancellationTokenSource();
			this.endPointHostCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.endPointCancelTokenSource.Token, this.hostCancelTokenSource.Token);

			this.properties = receivePropertiesFactory.Create(uri);
			this.locationConfigDom = ConfigProperties.ExtractConfigDom(config);
			this.properties.ReadLocationConfiguration(this.locationConfigDom, portName, endPointHostCancelTokenSource.Token);

			TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Info, "Opening receiver endpoint. URI = '{0}' Hash Code = {1}", uri, GetHashCode());

			this.transportProxy = transportProxy;
			this.baseMessageFactory = this.transportProxy.GetMessageFactory();
			this.control = control;

			StartEndpoint();
			this.receiverEndpoints[this.uri] = this;
			TransferrerHelpers.Log(this, this.adapterLogger, LogLevel.Debug, "Opened receiver endpoint for port '{0}'. Endpoint Type '{1}'. Hash Code = {2}", this.properties.PortName, GetType().Name, GetHashCode());
		}

		public override void Update(IPropertyBag config, IPropertyBag bizTalkConfig, IPropertyBag handlerPropertyBag)
		{
			TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Info, "Updating receiver endpoint settings. Hash Code = {0}", GetHashCode());

			StopEndpoint(this);

			if (this.hostCancelTokenSource.IsCancellationRequested)
				return;

			this.endPointHostCancelTokenSource.Dispose();
			this.endPointCancelTokenSource.Dispose();

			this.endPointCancelTokenSource = new CancellationTokenSource();
			this.endPointHostCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.endPointCancelTokenSource.Token, this.hostCancelTokenSource.Token);

			if (config != null)
			{
				string oldPortName = this.properties.PortName;
				string portName;
				TryGetPortName(out portName);

				bool portNameChanged = false;
				if (TransferrerReceiver.ReceiveLocationNames.ContainsKey(this.uri))
				{
					TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Checking for port name change.");

					portNameChanged = (portName != oldPortName);
					if (portNameChanged)
					{
						TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Port name changed from '{0}' to '{1}'. Switching to new log file name.", oldPortName, portName);
						TransferrerReceiver.ReceiveLocationNames[this.uri] = portName;
					}
				}

				this.locationConfigDom = ConfigProperties.ExtractConfigDom(config);
				this.properties.ReadLocationConfiguration(this.locationConfigDom, portName, endPointHostCancelTokenSource.Token);

				if (portNameChanged)
				{
					TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Logging started after port name changed from '{0}' to '{1}'. Hash Code = {2}", oldPortName, portName, GetHashCode());
				}
			}

			StartEndpoint();
		}
		#endregion ReceiverEndpoint

		#region TransferrerReceiverEndpoint
		void StartEndpoint()
		{
			TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Starting receiver endpoint. Hash Code = {0}", GetHashCode());

			this.Task = new Task(EndpointTask, TaskCreationOptions.LongRunning);
			this.Task.Start();
		}

		void StopEndpoint(TransferrerReceiverEndpoint endpoint)
		{
			TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Stopping existing receiver endpoint. Hash Code = {0}", endpoint.GetHashCode());
			endpoint.endPointCancelTokenSource.Cancel();
			try
			{
				endpoint.Task.Wait(this.hostCancelTokenSource.Token);
			}
			catch (OperationCanceledException) { }
		}

		internal virtual void EndpointTask()
		{
			try
			{
				TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Starting endpoint task loop.");
				var stopwatch = new Stopwatch();

				while (!this.endPointHostCancelTokenSource.IsCancellationRequested)
				{
					try
					{
						stopwatch.Restart();
						Task.Factory.StartNew(() => DownloadFilesAndSubmit(), TaskCreationOptions.LongRunning).Wait(this.unsafeAbortCancelTokenSource.Token);
					}
					catch (Exception ex)
					{
						while (ex is AggregateException) ex = ex.InnerException;
						if (ex is OperationCanceledException)
							return;
						if (this.properties != null && this.properties.Logger != null)
							TransferrerHelpers.LogException(this, this.properties.Logger, ex);
						throw new AdapterException(String.Format("Error in receive location '{0}' with URI '{1}': {2}", this.properties.PortName, Uri.UnescapeDataString(this.uri).Replace("%", ""), ex.ToString()), ex);
					}
					finally
					{
						if (!this.endPointHostCancelTokenSource.IsCancellationRequested)
						{
							WaitForNextScheduledExecution(stopwatch);
						}
					}
				}

				TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Stopping endpoint task loop.");
			}
			catch (Exception ex)
			{
				this.transportProxy.SetErrorInfo(ex);
			}
		}

		internal virtual void WaitForNextScheduledExecution(Stopwatch executionStopWatch)
		{
			int waitTime = Math.Max(this.properties.PollingInterval - (int)executionStopWatch.ElapsedMilliseconds, 0);
			TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Waiting for next scheduled execution. Wait time = {0:N0} seconds", waitTime / 1000);
			try
			{
				var waitEvent = new ManualResetEventSlim();
				waitEvent.Wait(waitTime, this.endPointHostCancelTokenSource.Token);
			}
			catch (OperationCanceledException) { }
		}

		internal virtual void DownloadFilesAndSubmit()
		{
			try
			{
				TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Starting receive process.");
				if (this.properties.registrationType != null && this.properties.connectionStringName != null)
				{
					TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Retrieving multiple location credentials from client registration.");
					this.properties.ReadLocationConfiguration(this.locationConfigDom, this.properties.PortName, this.endPointHostCancelTokenSource.Token);
				}
				if (this.properties.SingleLocation != null)
				{
					DownloadAndSubmitForLocation(this.properties.SingleLocation);
				}
				else
				{
					foreach (var locn in this.properties.MultipleLocations)
					{
						if (this.endPointHostCancelTokenSource.IsCancellationRequested)
							return;
						DownloadAndSubmitForLocation(locn);
					}
				}
			}
			finally
			{
				this.safeToExit.Set();
				TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Exiting receive process.");
			}
		}

		internal virtual void DownloadAndSubmitForLocation(TransferrerProperties.Receive.Location location)
		{
			TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Info, "Starting receive for location '{0}'.", location.Uri);
			var jobsCreated = new ConcurrentDictionary<int, FileDownloadJob>();

			using (var locationCancelTokenSource = new CancellationTokenSource())
			using (var endPointLocationCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(endPointHostCancelTokenSource.Token, locationCancelTokenSource.Token))
			{
				try
				{
					using (var transferrers = new ConcurrentTransferrerPool(transferrerFactory, location, properties, endPointLocationCancelTokenSource.Token, locationConfigDom))
					{
						int currentFileInfoIndex = 0;
						TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Info, $"Downloading files using {properties.MaximumConcurrentDownloads} maximum number of workers.");

						if (properties.MaximumConcurrentDownloads > 1)
						{
							transferrers.InvokeActionWithTransferrer(getFilesTransferrer =>
							{
								var jobTasks = new ConcurrentBag<Task>();
								var fileInfos = GetServerFiles(getFilesTransferrer, location, endPointLocationCancelTokenSource);
								try
								{
									FileDownloadJob previousJob = null;
									foreach (var fileInfo in fileInfos)
									{
										if (endPointLocationCancelTokenSource.IsCancellationRequested) throw new OperationCanceledException();
										WaitForSubmitsToCatchUp(jobsCreated, endPointLocationCancelTokenSource);
										var job = CreateJob(jobsCreated, ref currentFileInfoIndex, fileInfo, location, transferrers, previousJob);
										jobTasks.Add(job.ProcessInBackground(endPointLocationCancelTokenSource, locationCancelTokenSource, batchFactory, transportProxy, control, transportType, baseMessageFactory, transferrerMessageFactory));
										previousJob = job;
									}
								}
								catch
								{
									if (!locationCancelTokenSource.IsCancellationRequested) locationCancelTokenSource.Cancel();
									throw;
								}
								finally
								{
									Task.WaitAll(jobTasks.ToArray());
								}
							});
						}
						else
						{
							IEnumerable<TransferrerFileInfo> fileInfos = null;
							transferrers.InvokeActionWithTransferrer(transferrer => fileInfos = GetServerFiles(transferrer, location, endPointHostCancelTokenSource));

							foreach (var job in fileInfos.Select(i => CreateJob(jobsCreated, ref currentFileInfoIndex, i, location, transferrers)))
							{
								job.Process(endPointHostCancelTokenSource, batchFactory, transportProxy, control, transportType, baseMessageFactory, transferrerMessageFactory).Wait();
							}
						}
					}
				}
				catch (Exception ex)
				{
					var exceptions = new Stack<Exception>();
					exceptions.Push(ex);

					while (exceptions.Count > 0)
					{
						var e = exceptions.Pop();
						if (e is AggregateException)
						{
							foreach (var innerException in (e as AggregateException).InnerExceptions)
							{
								exceptions.Push(innerException);
							}
						}
						else if (!(endPointLocationCancelTokenSource.IsCancellationRequested && e is OperationCanceledException) &&
								!(endPointHostCancelTokenSource.IsCancellationRequested && e is COMException))
						{
							TransferrerHelpers.LogException(this, this.properties.Logger, ex, LogLevel.Warn, "Error in receive location '{0}' for URI '{1}'.", this.properties.PortName, location.Uri);
						}
					}
				}
				finally
				{
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "The number of downloaded file(s) is {0}.", jobsCreated.Values.Count(j => j.IsProcessed));
					TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Finished receive for location.");
					this.safeToExit.Set();
				}
			}
		}

		FileDownloadJob CreateJob(ConcurrentDictionary<int, FileDownloadJob> jobsCreated, ref int currentFileInfoIndex, TransferrerFileInfo fileInfo, TransferrerProperties.Receive.Location location, ConcurrentTransferrerPool transferrers, FileDownloadJob previousJob = null)
		{
			var job = new FileDownloadJob(currentFileInfoIndex++, fileInfo, location, properties, transferrers, previousJob);
			jobsCreated[job.FileIndex] = job;
			return job;
		}

		void WaitForSubmitsToCatchUp(ConcurrentDictionary<int, FileDownloadJob> jobsCreated, CancellationTokenSource endPointLocationCancelTokenSource)
		{
			if (jobsCreated.Values.Count(j => !j.IsProcessed) >= properties.MaximumConcurrentDownloads &&
				jobsCreated.TryGetValue(jobsCreated.Values.Count(j => j.IsProcessed), out var firstJobNotSubmitted))
			{
				firstJobNotSubmitted.ProcessCompletedTask.Wait(endPointLocationCancelTokenSource.Token);
				if (!firstJobNotSubmitted.IsProcessed) throw new OperationCanceledException();
			}
		}

		IEnumerable<TransferrerFileInfo> GetServerFiles(ITransferrer transferrer, TransferrerProperties.Receive.Location location, CancellationTokenSource cancelTokenSource)
		{
			var serverFilesTask = Task.Run(() =>
			{
				TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Getting file list.");
				return transferrer.ListFiles(location, properties, cancelTokenSource);
			}, cancelTokenSource.Token);
			serverFilesTask.Wait(cancelTokenSource.Token);
			this.safeToExit.Reset();
			return serverFilesTask.Result;
		}

		internal virtual bool TryGetPortName(out string portName)
		{
			portName = null;
			var wmiSearcher = new ManagementObjectSearcher();
			wmiSearcher.Scope = new ManagementScope("root\\MicrosoftBizTalkServer");
			var wmiQuery = new SelectQuery();
			wmiQuery.QueryString = String.Format("SELECT Name FROM MSBTS_ReceiveLocation WHERE AdapterName = '{0}' AND InboundTransportURL = '{1}'", this.transportType, this.uri);
			wmiSearcher.Query = wmiQuery;
			var wmiObjColl = wmiSearcher.Get().GetEnumerator();
			if (wmiObjColl.MoveNext())
			{
				portName = wmiObjColl.Current.Properties["Name"].Value.ToString();
				return true;
			}
			else
			{
				return false;
			}
		}
		#endregion TransferrerReceiverEndpoint

		#region IDisposable
		public sealed override void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Disposing receiver endpoint. Hash Code = {0}", GetHashCode());
					if (this.Task != null && this.endPointCancelTokenSource != null && this.unsafeAbortCancelTokenSource != null)
					{
						TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Stopping endpoint task.");
						this.endPointCancelTokenSource.Cancel();
						if (this.safeToExit != null && !this.safeToExit.IsSet)
						{
							TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Waiting for current transfer to complete.");
							if (!this.safeToExit.Wait(TransferrerProperties.TERMINATE_WAIT_LIMIT))
							{
								TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Debug, "Timed out waiting for current transfer to complete.");
								this.unsafeAbortCancelTokenSource.Cancel();
							}
						}
						if (this.Task.Wait(TransferrerProperties.TERMINATE_WAIT_LIMIT))
							this.Task.Dispose();
					}
					if (this.endPointHostCancelTokenSource != null)
						this.endPointHostCancelTokenSource.Dispose();
					if (this.endPointCancelTokenSource != null)
						this.endPointCancelTokenSource.Dispose();
					if (this.unsafeAbortCancelTokenSource != null)
						this.unsafeAbortCancelTokenSource.Dispose();
				}
				this.Task = null;
				this.endPointHostCancelTokenSource = null;
				this.endPointCancelTokenSource = null;
				this.unsafeAbortCancelTokenSource = null;
				TransferrerReceiverEndpoint endpoint;
				this.receiverEndpoints.TryRemove(this.uri, out endpoint);
				TransferrerReceiver.ReceiveLocationNames.Remove(this.uri);
				this.disposed = true;
				TransferrerHelpers.Log(this, this.properties.Logger, LogLevel.Info, "Receiver endpoint closed. Hash Code = {0}", GetHashCode());
				TransferrerHelpers.Log(this, this.adapterLogger, LogLevel.Debug, "Terminated receiver endpoint for port '{0}'. Endpoint type = '{1}'. Endpoint Hash Code = {2}", this.properties.PortName, GetType().Name, GetHashCode());
			}
		}
		bool disposed = false;
		#endregion IDisposable

		public override int GetHashCode()
		{
			return hash;
		}
		readonly int hash = (int)(TransferrerHelpers.Rng.NextDouble() * int.MaxValue);

		readonly ITransferrerFactory transferrerFactory;
		readonly ISyncReceiveSubmitBatchFactory batchFactory;
		readonly ITransferrerMessageFactory transferrerMessageFactory;
		readonly TransferrerProperties.IReceiveFactory receivePropertiesFactory;
		XmlDocument locationConfigDom;

		internal TransferrerProperties.Receive properties { get; private set; }
		string uri;
		string transportType;
		string propertyNamespace;
		IBTTransportProxy transportProxy;
		IBaseMessageFactory baseMessageFactory;
		ControlledTermination control;
		ManualResetEventSlim safeToExit = new ManualResetEventSlim(true);

		internal Task Task { get; set; }
		CancellationTokenSource unsafeAbortCancelTokenSource = new CancellationTokenSource();
		CancellationTokenSource endPointCancelTokenSource;
		internal CancellationTokenSource endPointHostCancelTokenSource;
		CancellationTokenSource hostCancelTokenSource;
		ConcurrentDictionary<string, TransferrerReceiverEndpoint> receiverEndpoints;
		ILog adapterLogger;
	}
}
