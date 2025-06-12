using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public abstract class TransferrerTransmitter : AsyncTransmitter
	{
		protected TransferrerTransmitter(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType, int maxBatchSize)
			: base(name, version, description, transportType, clsid, propertyNamespace, endpointType, maxBatchSize)
		{
			this.endpointType = endpointType;
			this.transportType = transportType;
            this.hostInstance = Process.GetCurrentProcess().ProcessName.StartsWith("BTSNTSvc") ? Environment.GetCommandLineArgs()[4] : "[Unknown]";
			this.logger = TransferrerHelpers.CreateAdapterLogger(this, this.transportType, this.hostInstance);
			TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Creating transmitter of type '{0}' for adapter '{1}' in host instance '{2}'. Hash Code = {3}", GetType().Name, this.transportType, this.hostInstance, GetHashCode());
		}

		public override void Terminate()
		{
			TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Terminating transmitter of type '{0}' for adapter '{1}' in host instance '{2}'. Hash Code = {3}", GetType().Name, this.transportType, this.hostInstance, GetHashCode());

			HostCancelTokenSource.Cancel();

			List<Task> tasks = new List<Task>();
			foreach (var key in PooledTransferrers.Keys)
			{
				ConcurrentBag<ITransferrer> pool;
				if (PooledTransferrers.TryRemove(key, out pool) && pool != null)
				{
					ITransferrer transferrer = null;
					while (pool.TryTake(out transferrer))
					{
						TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Terminating transferrer of type '{0}' for send port '{1}'. Transferrer Hash Code = {2}.", transferrer.GetType().Name, key, transferrer.GetHashCode());
						tasks.Add(Task.Factory.StartNew(t => (t as IDisposable).Dispose(), transferrer, TaskCreationOptions.LongRunning));
					}
				}
			}
			foreach (var key in TransmitterEndpoints.Keys)
			{
				TransferrerTransmitterEndpoint endpoint;
				if (TransmitterEndpoints.TryRemove(key, out endpoint) && endpoint != null)
				{
					TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Terminating transmitter endpoint of type '{0}'. Endpoint Hash Code = {1}.", endpoint.GetType().Name, endpoint.GetHashCode());
					tasks.Add(Task.Factory.StartNew(e => (e as IDisposable).Dispose(), endpoint, TaskCreationOptions.LongRunning));
				}
			}

			try
			{
				Task.WaitAll(tasks.ToArray(), TransferrerProperties.TERMINATE_WAIT_LIMIT);

				TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Transmitter endpoints terminated successfully.");
			}
			catch (Exception ex)
			{
				TransferrerHelpers.Log(this, this.logger, LogLevel.Error, "Error terminating transmitter endpoints.");
				TransferrerHelpers.LogException(this, this.logger, ex, LogLevel.Error, "Error terminating transmitter endpoints for adapter '{0}'.", this.transportType);
			}

			base.Terminate();

			TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Transmitter terminated. Hash Code = {0}", GetHashCode());
		}

		public override int GetHashCode()
		{
			return hash;
		}
		readonly int hash = (int)(TransferrerHelpers.Rng.NextDouble() * int.MaxValue);

		Type endpointType;
		string transportType;
		string hostInstance;

		internal readonly ILog logger;

		internal readonly ConcurrentDictionary<int, TransferrerTransmitterEndpoint> TransmitterEndpoints = new ConcurrentDictionary<int, TransferrerTransmitterEndpoint>();
		internal readonly ConcurrentDictionary<string, ConcurrentBag<ITransferrer>> PooledTransferrers = new ConcurrentDictionary<string, ConcurrentBag<ITransferrer>>();
		internal readonly CancellationTokenSource HostCancelTokenSource = new CancellationTokenSource();
	}
}
