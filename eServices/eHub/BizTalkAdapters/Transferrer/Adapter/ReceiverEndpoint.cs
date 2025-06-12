using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public abstract class ReceiverEndpoint : Microsoft.Samples.BizTalk.Adapter.Common.ReceiverEndpoint
	{
		private readonly ILoggerFactory loggerFactory;
		private int activityIdSource;
		private string endpointActivityId;
		private CancellationToken handlerCancelToken;
		private CancellationTokenSource endpointCancelTokenSource;
		private Task endpointTask;
		private bool disposed;

		protected Receiver TransportReceiver { get; private set; }
		public virtual IIssueManager IssueManager { get; private set; }
		public virtual ILog Logger { get; private set; }
		public virtual string PortName { get; private set; }
		private string TransportUri { get; set; }
		private string TransportType { get; set; }
		private IBTTransportProxy TransportProxy { get; set; }
		private IBaseMessageFactory MessageFactory { get; set; }
		private ControlledTermination Control { get; set; }
		public virtual XmlDocument ConfigXml { get; private set; }

		protected ReceiverEndpoint() : this(LoggerFactory.Instance) { }

		private ReceiverEndpoint(ILoggerFactory loggerFactory)
		{
			this.loggerFactory = loggerFactory;
		}

		public override void Open(string uri, IPropertyBag config, IPropertyBag bizTalkConfig, IPropertyBag handlerPropertyBag, IBTTransportProxy transportProxy, string transportType, string propertyNamespace, ControlledTermination control)
		{
			TransportUri = uri;
			TransportType = transportType;
			TransportProxy = transportProxy;
			MessageFactory = transportProxy.GetMessageFactory();
			Control = control;

			TransportReceiver = Receiver.Receivers[transportType];
			TransportReceiver.Handler.RegisterEndpoint(this);
			PortName = TransportReceiver.ReceiveLocationNames.GetOrAdd(uri, key => TryGetPortName(transportType, uri) ?? $"ERR_{Guid.NewGuid():N}");
			handlerCancelToken = TransportReceiver.Handler.CancelTokenSource.Token;
			endpointCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(handlerCancelToken);
			IssueManager = new IssueManager();

			ConfigureEndpoint(config);
			Logger.Log(endpointActivityId, LogLevel.Info, "Starting receive location: Name='{0}' Uri='{1}'", PortName, uri);
			StartEndpointTask();
		}

		public override void Update(IPropertyBag config, IPropertyBag bizTalkConfig, IPropertyBag handlerPropertyBag)
		{
			Logger.Log(endpointActivityId, LogLevel.Debug, "Updating endpoint.");
			StopEndpointTask();
			ConfigureEndpoint(config);
			StartEndpointTask();
		}

		public override void Dispose()
		{
			if (!disposed)
			{
				Logger.Log(endpointActivityId, LogLevel.Debug, "Stopping endpoint.");
				StopEndpointTask();
				base.Dispose();
				disposed = true;
			}
		}

		private void ConfigureEndpoint(IPropertyBag config)
		{
			endpointActivityId = GetNewActivityId();
			ConfigXml = ConfigProperties.IfExistsExtractConfigDom(config) ?? AdapterManagement.EmptyConfigDom();
			var logSubDir = PortName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0];
			if (ConfigXml["Config"] is var element && element != null)
			{
				element.AppendChild(ConfigXml.CreateElement("LogDir")).InnerText =
					Path.Combine(TransportReceiver.Handler.InterfacesLogDir, logSubDir);
				element.AppendChild(ConfigXml.CreateElement("StructuredLogDir")).InnerText =
					Path.Combine(TransportReceiver.Handler.StructuredLogDir, logSubDir);
			}

			Logger = loggerFactory.CreateLogger(PortName, "Receive", ConfigXml);
		}

		private void StartEndpointTask()
		{
			endpointCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(handlerCancelToken);
			endpointTask = Task.Factory.StartNew(async () =>
			{
				try
				{
					await EndpointTask(endpointCancelTokenSource.Token);
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					LogInterfaceError(ex);
				}
			}, endpointCancelTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		private void StopEndpointTask()
		{
			Logger.Log(endpointActivityId, LogLevel.Debug, "Stopping endpoint task.");
			if (endpointTask != null)
			{
				try
				{
					endpointCancelTokenSource.Cancel();
					Task.WaitAny(
						endpointTask,
						Task.Delay(handlerCancelToken.IsCancellationRequested
							? TransportReceiver.Handler.terminateWaitLimit
							: Timeout.Infinite,
							handlerCancelToken));
					endpointTask.Dispose();
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					LogInterfaceError(ex);
				}
			}
			endpointTask = null;
			endpointCancelTokenSource = null;
		}

		private void LogInterfaceError(Exception ex) => TransportReceiver.Handler.LogInterfaceError(Logger, endpointActivityId, "Error in receive location '{0}'", ex, PortName);

		[ExcludeFromCodeCoverage]
		public virtual bool SubmitMessageToBizTalk(Stream stream, string name, string location, string activityId)
		{
			IBaseMessagePart part = MessageFactory.CreateMessagePart();
			part.Data = stream;
			IBaseMessage message = MessageFactory.CreateMessage();
			message.AddPart("body", part, true);

			var context = new SystemMessageContext(message.Context);
			context.InboundTransportLocation = TransportUri;
			context.InboundTransportType = TransportType;

			message.Context.Write("Uri", "http://cargowise.com/ehub/adapters", location);
			message.Context.Write("FileName", "http://cargowise.com/ehub/adapters", name);
			message.Context.Write("ReceivedFileName", "http://schemas.microsoft.com/BizTalk/2003/file-properties", name);

			Logger.Log(activityId, LogLevel.Debug, "Submitting message to BizTalk for '{0}'.", name);
			using (var batch = new SyncReceiveSubmitBatch(TransportProxy, Control, 1))
			{
				batch.SubmitMessage(message);
				batch.Done();
				if (batch.Wait())
				{
					Logger.Log(activityId, LogLevel.Info, "BizTalk message created with MessageID '{0}' for file '{1}'.", message.MessageID, name);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public virtual Task EndpointTask(CancellationToken cancelToken) => Task.CompletedTask;

		protected string GetNewActivityId() => $"{Interlocked.Increment(ref activityIdSource):X8}";

		public virtual string TryGetPortName(string transportType, string uri)
		{
			using (var wmiSearcher = new ManagementObjectSearcher())
			{
				wmiSearcher.Scope = new ManagementScope("root\\MicrosoftBizTalkServer");
				wmiSearcher.Query = new SelectQuery
				{
					QueryString = $"SELECT Name FROM MSBTS_ReceiveLocation WHERE AdapterName = '{transportType}' AND InboundTransportURL = '{uri}'"
				};
				return wmiSearcher.Get().Cast<ManagementBaseObject>().FirstOrDefault()?.Properties["Name"].Value.ToString();
			}
		}

		public static string ReplaceFileNamePlaceholders(string targetFileName, string sourceFileName)
		{
			targetFileName = targetFileName.Replace("{f}", sourceFileName)
								.Replace("{n}", Path.GetFileNameWithoutExtension(sourceFileName))
								.Replace("{x}", Path.GetExtension(sourceFileName));
			return targetFileName;
		}
	}
}
