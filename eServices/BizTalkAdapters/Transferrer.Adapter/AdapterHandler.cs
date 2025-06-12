using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	class AdapterHandler
	{
		public string InterfacesLogDir { get; private set; }
		public CancellationTokenSource CancelTokenSource { get; private set; }

		Direction direction;
		internal string transportType;
		private readonly Func<string, XmlDocument, ILog> loggerFactory;
		string hostInstance;
		string handlerDetails;
		internal int terminateWaitLimit;
		ConcurrentBag<IDisposable> endpoints = new ConcurrentBag<IDisposable>();
		ILog log;

		public AdapterHandler(Direction direction, string transportType, Func<string, XmlDocument, ILog> loggerFactory)
		{
			this.direction = direction;
			this.transportType = transportType;
			this.loggerFactory = loggerFactory;
			CancelTokenSource = new CancellationTokenSource();

			hostInstance = Process.GetCurrentProcess().ProcessName.StartsWith("BTSNTSvc") ? Environment.GetCommandLineArgs()[4] : "Unknown";
			handlerDetails = String.Format("Adapter='{0}' Direction='{1}' HostInstance='{2}'", transportType, direction, hostInstance);
		}

		internal void Configure(IPropertyBag handlerPropertyBag)
		{
			var configXml = ConfigProperties.ExtractConfigDom(handlerPropertyBag);
			InterfacesLogDir = ConfigProperties.Extract(configXml, "/Config/InterfacesLogDir", null);
			terminateWaitLimit = ConfigProperties.ExtractInt(configXml, "/Config/TerminateWaitLimit");

			string logName = String.Format("{0}_{1}_{2}", transportType, direction, hostInstance);
			log = loggerFactory(logName, configXml);
			log.Info("Handler initialized. " + handlerDetails);
		}

		internal void RegisterEndpoint(IDisposable endpoint)
		{
			endpoints.Add(endpoint);
		}

		internal void LogInterfaceError(ILog interfaceLog, string format, Exception exception, params object[] args)
		{
			var rootLog = LogManager.GetLogger(typeof(AdapterHandler).FullName);
			rootLog.ErrorFormat(format, exception, args);
			interfaceLog.ErrorFormat(format, exception, args);
		}

		internal void Terminate()
		{
			log.Info("Handler terminating.");

			try
			{
				CancelTokenSource.Cancel();

				var disposeTasks = endpoints.Select(e => Task.Factory.StartNew(e.Dispose)).ToArray();
				if (!Task.WhenAll(disposeTasks).Wait(terminateWaitLimit * 1000))
					throw new TransferrerException("Timed out waiting for endpoints to terminate.");
			}
			catch (Exception ex)
			{
				log.Error("Error terminating adapter handler. " + handlerDetails, ex);
			}

			log.Info("Transmitter terminated.");
		}

		public enum Direction
		{
			Send,
			Receive
		}
	}
}
