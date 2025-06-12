using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public abstract class TransferrerTransmitterEndpoint : AsyncTransmitterEndpoint
	{
		public TransferrerTransmitterEndpoint(AsyncTransmitter asyncTransmitter, ITransferrerFactory factory)
			: base(asyncTransmitter)
		{
			this.transmitter = asyncTransmitter as TransferrerTransmitter;
			this.transferrerFactory = factory;
			this.linkedCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.transmitter.HostCancelTokenSource.Token, this.localCancelTokenSource.Token);
			this.linkedCancelToken = this.linkedCancelTokenSource.Token;
			while (!this.transmitter.TransmitterEndpoints.TryAdd(GetHashCode(), this))
				this.hash = (int)(TransferrerHelpers.Rng.NextDouble() * int.MaxValue);
			TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Initialized transmitter endpoint of type '{0}'. Endpoint Hash Code = {1}.", GetType().Name, GetHashCode());
		}

		public override void Open(EndpointParameters endpointParameters, IPropertyBag handlerPropertyBag, string propertyNamespace)
		{
			this.propertyNamespace = propertyNamespace;
			this.uri = endpointParameters.OutboundLocation;
			TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Opened transmitter endpoint of type '{0}' for URI '{1}'. Endpoint Hash Code = {2}.", GetType().Name, this.uri, GetHashCode());
		}

		public override IBaseMessage ProcessMessage(IBaseMessage message)
		{
			SemaphoreSlim connLimiter = null;
			TransferrerProperties.Transmit properties = null;
			ITransferrer transferrer = null;

			try
			{
				object serviceInstanceID = null;

				try
				{
					serviceInstanceID = message.Context.Read("TransmitInstanceID", "http://schemas.microsoft.com/BizTalk/2003/system-properties");

					properties = new TransferrerProperties.Transmit(message, this.propertyNamespace, this.uri);

					this.linkedCancelToken.ThrowIfCancellationRequested();

					string fileName = ReplaceFileNameMacros(properties.TargetFileName, message);
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Starting send for '{0}' to '{1}'. Message ID = '{2}'. Service Instance ID = '{3}'", fileName, this.uri, message.MessageID, serviceInstanceID);

					this.linkedCancelToken.ThrowIfCancellationRequested();

					if (properties.ConnectionLimit > 0)
					{
						connLimiter = connectionLimiters.GetOrAdd(properties.PortName, u => new SemaphoreSlim(properties.ConnectionLimit, properties.ConnectionLimit));
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Acquiring connection semaphore. Current available slots = {0}/{1}", connLimiter.CurrentCount, properties.ConnectionLimit);
						connLimiter.Wait(this.linkedCancelToken);
					}

					this.linkedCancelToken.ThrowIfCancellationRequested();

					var transferrerPool = this.transmitter.PooledTransferrers.GetOrAdd(properties.PortName, key => new ConcurrentBag<ITransferrer>());
					transferrer = GetTransferrer(properties, transferrerPool);

					this.linkedCancelToken.ThrowIfCancellationRequested();

					Stream source = message.BodyPart.GetOriginalDataStream();
					string targetPath = Path.Combine(properties.Folder, fileName).Replace('\\', '/');

					this.safeToExitEvent.AddCount();
					try
					{
						if (String.IsNullOrWhiteSpace(properties.TemporaryFolder) && String.IsNullOrWhiteSpace(properties.TemporaryFileName))
						{
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Putting file '{0}'", targetPath);
							transferrer.PutFile(targetPath, source);
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Put file '{0}'", targetPath);
						}
						else
						{
							string temporaryPath = Path.Combine(
								String.IsNullOrWhiteSpace(properties.TemporaryFolder) ? properties.Folder : properties.TemporaryFolder,
								String.IsNullOrWhiteSpace(properties.TemporaryFileName) ? fileName : properties.TemporaryFileName);
							temporaryPath = ReplaceFileNameMacros(temporaryPath, message).Replace('\\', '/');
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Putting temporary file '{0}'", temporaryPath);
							transferrer.PutFile(temporaryPath, source);
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Put temporary file '{0}'", temporaryPath);
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Renaming temporary file from '{0}' to '{1}'", temporaryPath, targetPath);
							transferrer.RenameFile(temporaryPath, targetPath);
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Renamed file from '{0}' to '{1}'", temporaryPath, targetPath);
						}
						if (!String.IsNullOrWhiteSpace(properties.FlagFile))
						{
							string flagPath = Path.Combine(properties.Folder, ReplaceFileNameMacros(properties.FlagFile, message)).Replace('\\', '/');
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Creating flag file '{0}'", flagPath);
							using (var ms = new MemoryStream())
								transferrer.PutFile(flagPath, ms);
							TransferrerHelpers.Log(this, properties.Logger, LogLevel.Info, "Created flag file '{0}'", flagPath);
						}
					}
					finally
					{
						this.safeToExitEvent.Signal();
					}

					if (properties.KeepAlive && !this.linkedCancelToken.IsCancellationRequested)
					{
						transferrerPool.Add(transferrer);
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Saved transferrer to pool. Hash Code = {0}. Current instances = {1}", transferrer.GetHashCode(), transferrerPool.Count);
					}
					else
					{
						transferrer.Dispose();
					}

					return null;
				}
				catch (Exception ex)
				{
					while (ex != null && ex is AggregateException) ex = ex.InnerException;

					TransferrerHelpers.LogException(this, properties.Logger, ex, LogLevel.Warn, "Error in send port '{0}' for message ID '{1}'.", properties.PortName, message.MessageID);

					if (transferrer != null)
						transferrer.Dispose();

					if (ex is OperationCanceledException)
						throw new AdapterException(String.Format("Canceled send in send port '{0}'. It will be retried when host instance and/or send port is restarted.", properties.PortName));
					else
						throw new AdapterException(String.Format("Error in send port '{0}': {1}", properties.PortName, ex.ToString()), ex);
				}
				finally
				{
					if (connLimiter != null && !this.linkedCancelToken.IsCancellationRequested)
					{
						connLimiter.Release();
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Released connection semaphore. Current available slots = {0}/{1}", connLimiter.CurrentCount, properties.ConnectionLimit);
					}
				}

			}
			catch (Exception ex)
			{
				if (ex is AdapterException)
					throw;
				else
					throw new AdapterException(String.Format("Error in send port: '{0}'.", ex.ToString()), ex);
			}
		}

		ITransferrer GetTransferrer(TransferrerProperties.Transmit properties, ConcurrentBag<ITransferrer> transferrerPool)
		{
			ITransferrer transferrer = null;

			if (properties.KeepAlive)
			{
				TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Attempting to retrieve pooled transferrer.");

				while (transferrer == null && transferrerPool.TryTake(out transferrer))
				{
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Existing transferrer retrieved from pool. Hash Code = {0}", transferrer.GetHashCode());
					if (transferrer.ConfigDom != properties.ConfigDom.InnerXml)
					{
						TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Pooled transferrer configuration is different to current transferrer configuration.");
						transferrer.Dispose();
						transferrer = null;
					}
					this.linkedCancelToken.ThrowIfCancellationRequested();
				}
				if (transferrer == null)
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "No pooled transferrers available.");
				else
					return transferrer;
			}

			try
			{
				transferrer = transferrerFactory.CreateTransferrer();
				TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Created new transferrer. Hash Code = {0}", transferrer.GetHashCode());

				transferrer.Server = properties.Server;
				transferrer.Port = properties.Port;
				transferrer.UserName = properties.User;
				transferrer.Password = properties.Password;
				transferrer.Timeout = properties.Timeout;
				transferrer.Logger = properties.Logger;
				transferrer.ConfigDom = properties.ConfigDom.InnerXml;
				transferrer.ReadLocationConfiguration(properties.ConfigDom);
				transferrer.CancelToken = this.linkedCancelToken;
				transferrer.HighPriority = true;

				TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Opening transferrer.");
				if (Task.Factory.StartNew(transferrer.Open, this.linkedCancelToken, TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning, TaskScheduler.Current).Wait(properties.Timeout, this.linkedCancelToken))
				{
					TransferrerHelpers.Log(this, properties.Logger, LogLevel.Debug, "Opened transferrer.");
					return transferrer;
				}
				else
				{
					throw new TransferrerException("Timed out opening transferrer.");
				}
			}
			catch (Exception)
			{
				if (transferrer != null)
					transferrer.Dispose();
				throw;
			}
		}

		static string ReplaceFileNameMacros(string fileName, IBaseMessage message)
		{
			string overrideFileName = Path.GetFileName((string)message.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")) ?? "%OverrideFilename%";
			string destinationPartyQualifier = (string)message.Context.Read("DestinationPartyQualifier", "http://schemas.microsoft.com/BizTalk/2003/system-properties") ?? "%DestinationPartyQualifier%";

			fileName = fileName.Replace("%OverrideFilename%", overrideFileName)
						   .Replace("%MessageID%", message.MessageID.ToString("B").ToUpper())
						   .Replace("%DestinationPartyQualifier%", destinationPartyQualifier)
						   .Replace("%datetime_bts2000%", DateTime.UtcNow.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture));

			if (fileName.Contains("%SourceFileName%"))
				fileName = fileName.Replace("%SourceFileName%",
								(string)message.Context.Read("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/ftp-properties")
							 ?? (string)message.Context.Read("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties")
							 ?? "%SourceFileName%");

			return fileName;
		}

		public sealed override void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.localCancelTokenSource != null)
						this.localCancelTokenSource.Cancel();
					if (this.safeToExitEvent != null && !this.safeToExitEvent.Signal())
					{
						TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Waiting for {0} active transfer(s) to complete. Endpoint Hash Code = {1}.", this.safeToExitEvent.CurrentCount, this.GetHashCode());
						if (this.safeToExitEvent.Wait(TransferrerProperties.TERMINATE_WAIT_LIMIT))
							TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Last transfer completed successfully.");
						else
							TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Timed out waiting for transfer to complete.");
					}
					if (this.safeToExitEvent != null)
						this.safeToExitEvent.Dispose();
					if (this.linkedCancelTokenSource != null)
						this.linkedCancelTokenSource.Dispose();
					if (this.localCancelTokenSource != null)
						this.localCancelTokenSource.Dispose();
				}
				this.safeToExitEvent = null;
				this.localCancelTokenSource = null;
				this.linkedCancelTokenSource = null;
				TransferrerHelpers.Log(this, this.transmitter.logger, LogLevel.Debug, "Terminated transmitter endpoint of type '{0}'. Endpoint Hash Code = {1}.", GetType().Name, GetHashCode());
				TransferrerTransmitterEndpoint endpoint;
				this.transmitter.TransmitterEndpoints.TryRemove(this.GetHashCode(), out endpoint);
				this.transmitter = null;
				this.disposed = true;
			}
		}

		public override int GetHashCode()
		{
			return hash;
		}
		readonly int hash = (int)(TransferrerHelpers.Rng.NextDouble() * int.MaxValue);

		bool disposed = false;
		TransferrerTransmitter transmitter;
		ITransferrerFactory transferrerFactory;
		string propertyNamespace;
		string uri;
		CancellationTokenSource localCancelTokenSource = new CancellationTokenSource();
		CancellationTokenSource linkedCancelTokenSource;
		CancellationToken linkedCancelToken;
		CountdownEvent safeToExitEvent = new CountdownEvent(1);

		static internal ILog adapterLogger = new NoOpLogger();
		static ConcurrentDictionary<string, SemaphoreSlim> connectionLimiters = new ConcurrentDictionary<string, SemaphoreSlim>();
	}
}
