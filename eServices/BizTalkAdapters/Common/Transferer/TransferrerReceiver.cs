using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Common
{
	public abstract class TransferrerReceiver : Receiver
	{
		protected TransferrerReceiver(string name, string version, string description, string transportType, Guid clsid, string propertyNamespace, Type endpointType)
			: base(name, version, description, transportType, clsid, propertyNamespace, endpointType)
		{
			this.endpointType = endpointType;
			this.transportType = transportType;
            this.hostInstance = Process.GetCurrentProcess().ProcessName.StartsWith("BTSNTSvc") ? Environment.GetCommandLineArgs()[4] : "[Unknown]";

			this.logger = TransferrerHelpers.CreateAdapterLogger(this, this.transportType, this.hostInstance);
			TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Creating receiver for adapter '{0}' in host instance '{1}'. Receiver Type = '{2}'. Endpoint Type = '{3}'. Hash Code = {4}", this.transportType, this.hostInstance, GetType().Name, endpointType.Name, GetHashCode());

			TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(UnobservedTaskExceptionHandler);
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			ReceiverEndpoints[this.endpointType] = new ConcurrentDictionary<string, TransferrerReceiverEndpoint>();
			HostCancelTokenSource[this.endpointType] = new CancellationTokenSource();
			ReceiverLoggers[this.endpointType] = this.logger;

			TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Loading receive location names from WMI.", this.transportType, this.hostInstance, GetType().Name, endpointType.Name, GetHashCode());
			LoadReceiveLocationNames();

			TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Finished creating receiver.", this.transportType, this.hostInstance, GetType().Name, endpointType.Name, GetHashCode());
		}

		public override void Terminate()
		{
			try
			{
				TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Terminating receiver for adapter '{0}' in host instance '{1}'. Hash Code = {2}", this.transportType, this.hostInstance, GetHashCode());

				HostCancelTokenSource[this.endpointType].Cancel();

				Dictionary<string, Task> tasks = new Dictionary<string, Task>();
				foreach (var key in ReceiverEndpoints[this.endpointType].Keys)
				{
					TransferrerReceiverEndpoint endpoint;
					if (ReceiverEndpoints[this.endpointType].TryRemove(key, out endpoint) && endpoint != null)
					{
						TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Terminating receiver endpoint for port '{0}'. Endpoint Type = '{1}'. Endpoint Hash Code = {2}", endpoint.properties.PortName, endpoint.GetType().Name, endpoint.GetHashCode());
						tasks.Add(key, Task.Factory.StartNew(e => (e as IDisposable).Dispose(), endpoint, TaskCreationOptions.LongRunning));
					}
				}

				try
				{
					if (!Task.WaitAll(tasks.Values.ToArray(), TransferrerProperties.TERMINATE_WAIT_LIMIT))
					{
						string stillActive = String.Join(", ", tasks.Where(t => !t.Value.IsCompleted).Select(t => t.Key));
						TransferrerHelpers.Log(this, this.logger, LogLevel.Error, "Timed out waiting for transferrer processes to complete. Still active transferrers are: " + stillActive);
					}
				}
				catch (Exception ex)
				{
					TransferrerHelpers.LogException(this, this.logger, ex, LogLevel.Error, "Exception terminating endpoints for adapter '{0}' in host instance '{1}'.", this.transportType, this.hostInstance);
				}

				base.Terminate();
			}
			catch (Exception ex)
			{
				TransferrerHelpers.LogException(this, this.logger, ex, LogLevel.Error, "Exception terminating adapter '{0}' in host instance '{1}'.", this.transportType, this.hostInstance);
			}
			finally
			{
				TransferrerHelpers.Log(this, this.logger, LogLevel.Debug, "Receiver terminated. Hash Code = {0}", GetHashCode());
			}
		}

		void UnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
		{
			if (!HostCancelTokenSource[this.endpointType].IsCancellationRequested)
				TransferrerHelpers.LogException(this, this.logger, (Exception)e.Exception, LogLevel.Error, "Unobserved task exception from adapter '{0}' in host instance '{1}'.", this.transportType, this.hostInstance);
			e.SetObserved();
		}

		void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			TransferrerHelpers.LogException(this, this.logger, (Exception)e.ExceptionObject, LogLevel.Error, "Unhandled exception from adapter '{0}' in host instance '{1}'.", this.transportType, this.hostInstance);
		}

		internal virtual void LoadReceiveLocationNames()
		{
			var wmiSearcher = new ManagementObjectSearcher();
			wmiSearcher.Scope = new ManagementScope("root\\MicrosoftBizTalkServer");
			var wmiQuery = new SelectQuery();
			wmiQuery.QueryString = String.Format("SELECT InboundTransportURL, Name FROM MSBTS_ReceiveLocation WHERE AdapterName = '{0}'", this.transportType);
			wmiSearcher.Query = wmiQuery;
			var wmiObjColl = wmiSearcher.Get().GetEnumerator();
			while (wmiObjColl.MoveNext())
				ReceiveLocationNames[wmiObjColl.Current.Properties["InboundTransportURL"].Value.ToString()] = wmiObjColl.Current.Properties["Name"].Value.ToString();
		}

		public override int GetHashCode()
		{
			return hash;
		}
		readonly int hash = (int)(TransferrerHelpers.Rng.NextDouble() * int.MaxValue);

		internal readonly ILog logger;
		Type endpointType;
		string transportType;
		string hostInstance;

		internal static IDictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>> ReceiverEndpoints = new Dictionary<Type, ConcurrentDictionary<string, TransferrerReceiverEndpoint>>();
		internal static IDictionary<Type, ILog> ReceiverLoggers = new Dictionary<Type, ILog>();
		internal static IDictionary<string, string> ReceiveLocationNames = new Dictionary<string, string>();
		internal static IDictionary<Type, CancellationTokenSource> HostCancelTokenSource = new Dictionary<Type, CancellationTokenSource>();
	}
}