using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class AdapterHandler
	{
		public string InterfacesLogDir { get; private set; }
		public string StructuredLogDir { get; private set; }
		public CancellationTokenSource CancelTokenSource { get; }

		readonly Direction direction;
		private readonly string transportType;
		private readonly ILoggerFactory loggerFactory;
		readonly string hostInstance;
		readonly string handlerDetails;
		internal int terminateWaitLimit;
		readonly ConcurrentBag<IDisposable> endpoints = new ConcurrentBag<IDisposable>();
		ILog log;

		public AdapterHandler(Direction direction, string transportType, ILoggerFactory loggerFactory)
		{
			this.direction = direction;
			this.transportType = transportType;
			this.loggerFactory = loggerFactory;
			CancelTokenSource = new CancellationTokenSource();

			hostInstance = Process.GetCurrentProcess().ProcessName.StartsWith("BTSNTSvc") ? Environment.GetCommandLineArgs()[4] : "Unknown";
			handlerDetails = $"Adapter='{transportType}' Direction='{direction}' HostInstance='{hostInstance}'";
		}

		internal void Configure(IPropertyBag handlerPropertyBag)
		{
			var configXml = ConfigProperties.IfExistsExtractConfigDom(handlerPropertyBag) ?? AdapterManagement.DefaultHandlerConfigDom();
			InterfacesLogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/InterfacesLogDir", AdapterManagement.DefaultHandlerInterfacesLogDir);
			StructuredLogDir = ConfigProperties.IfExistsExtract(configXml, "/Config/StructuredLogDir", AdapterManagement.DefaultHandlerStructuredLogDir);
			terminateWaitLimit = ConfigProperties.IfExistsExtractInt(configXml, "/Config/TerminateWaitLimit", AdapterManagement.DefaultHandlerTerminateWaitLimit);

			var logName = $"{transportType}_{direction}_{hostInstance}";
			log = loggerFactory.CreateLogger(logName, configXml);
			TaskScheduler.UnobservedTaskException += UnobservedTaskExceptionHandler;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			log.Info("Handler initialized. " + handlerDetails);
		}

		internal void RegisterEndpoint(IDisposable endpoint)
		{
			endpoints.Add(endpoint);
		}

		public void LogInterfaceError(ILog interfaceLog, string activityId, string format, Exception exception, params object[] args)
		{
			var rootLog = LogManager.GetLogger(typeof(AdapterHandler).FullName);
			rootLog.ErrorFormat(format, exception, args);
			interfaceLog.Log(activityId, LogLevel.Error, format, exception, args);
		}

		public void LogInterfaceError(ILog interfaceLog, string format, Exception exception, params object[] args)
		{
			var rootLog = LogManager.GetLogger(typeof(AdapterHandler).FullName);
			rootLog.ErrorFormat(format, exception, args);
			interfaceLog.ErrorFormat(format, exception, args);
		}

		[ExcludeFromCodeCoverage]
		private void UnobservedTaskExceptionHandler(object sender, UnobservedTaskExceptionEventArgs e)
		{
			log.Error($"Adapter unobserved task exception: {handlerDetails}.", e.Exception);
			e.SetObserved();
		}

		[ExcludeFromCodeCoverage]
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			log.Error($"Adapter unhandled exception: {handlerDetails}.", e.ExceptionObject as Exception);
		}

		internal void Terminate()
		{
			log.Info("Handler terminating.");

			try
			{
				CancelTokenSource.Cancel();

				var disposeTasks = endpoints.Select(e => Task.Factory.StartNew(e.Dispose)).ToArray();
				if (!Task.WhenAll(disposeTasks).Wait(terminateWaitLimit))
					throw new TransferrerException("Timed out waiting for endpoints to terminate.");
			}
			catch (Exception ex)
			{
				log.Error("Error terminating adapter handler. " + handlerDetails, ex);
			}

			log.Info("Handler terminated.");
		}

		public enum Direction
		{
			Send,
			Receive
		}
	}
}
