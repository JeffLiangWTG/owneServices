using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class TransmitterEndpoint : AsyncTransmitterEndpoint
	{
		readonly Func<string, XmlDocument, ILog> loggerFactory;
		protected Transmitter Transmitter { get; private set; }
		protected IBTTransportProxy TransportProxy { get; private set; }
		protected IBaseMessageFactory MessageFactory { get; private set; }
		protected string uri;
		string configNamespace;
		IPropertyBag handlerProperties;
		internal readonly CancellationTokenSource HandlerCancelTokenSource;

		[ExcludeFromCodeCoverage]
		protected TransmitterEndpoint(AsyncTransmitter transmitter)
			: this(transmitter, Logging.CreateLogger)
		{
		}

		protected TransmitterEndpoint(AsyncTransmitter transmitter, Func<string, XmlDocument, ILog> loggerFactory)
			: base(transmitter)
		{
			Transmitter = (Transmitter)transmitter;
			this.loggerFactory = loggerFactory;

			TransportProxy = Transmitter.TransportProxy;
			MessageFactory = TransportProxy.GetMessageFactory();

			Transmitter.Handler.RegisterEndpoint(this);
			HandlerCancelTokenSource = Transmitter.Handler.CancelTokenSource;
		}

		public override void Open(EndpointParameters endpointParameters, IPropertyBag handlerPropertyBag, string propertyNamespace)
		{
			uri = endpointParameters.OutboundLocation;
			handlerProperties = handlerPropertyBag;
			configNamespace = propertyNamespace;
		}

		public override IBaseMessage ProcessMessage(IBaseMessage message)
		{
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				string name, ns;
				object val = message.Context.ReadAt(i, out name, out ns);
				Trace.WriteLine(ns + "#" + name + " = " + val);
			}
			var serviceInstanceId = message.Context.Read("TransmitInstanceID", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			var portName = (string)message.Context.Read("SPName", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
			var configXml = new XmlDocument();
			var config = (string)message.Context.Read("AdapterConfig", configNamespace);
			if (config != null)
			{
				configXml.LoadXml(config);
			}
			else
			{
				string location = (string)message.Context.Read("OutboundTransportLocation", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
				configXml.AppendChild(configXml.CreateElement("Config")).AppendChild(configXml.CreateElement("uri")).InnerText = location;
			}
			ApplyConfigDynamicOverrides(message, "http://cargowise.com/ehub/biztalkadapters/logging-properties", configXml);

			var logSubDir = portName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0];
			configXml["Config"].AppendChild(configXml.CreateElement("LogDir")).InnerText = Path.Combine(Transmitter.Handler.InterfacesLogDir, logSubDir);
			var log = loggerFactory(portName + ".Send", configXml);
			log.DebugFormat("Starting transmit to '{0}'. Message ID = '{1}'. Service Instance ID = '{2}'", uri, message.MessageID, serviceInstanceId);

			if (HandlerCancelTokenSource.IsCancellationRequested) return null;

			try
			{
				return TransmitMessageAsync(message, configXml, log, HandlerCancelTokenSource.Token).Result;
			}
			catch (Exception ex)
			{
				if (ex is AggregateException && ex.InnerException is TaskCanceledException) return null;
				Transmitter.Handler.LogInterfaceError(log, "Error in send port '{0}'", ex, portName);
				throw;
			}
		}

		protected virtual Task<IBaseMessage> TransmitMessageAsync(IBaseMessage message, XmlDocument configXml, ILog log,
			CancellationToken cancelToken)
		{
			ApplyConfigDynamicOverrides(message, "http://cargowise.com/ehub/biztalkadapters/transferrer-properties", configXml);
			return null;
		}

		protected static void ApplyConfigDynamicOverrides(IBaseMessage message, string propertyNamespace, XmlDocument configXml)
		{
			var config = configXml["Config"];
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				string name, ns;
				object value = message.Context.ReadAt(i, out name, out ns);
				if (ns == propertyNamespace)
				{
					var node = config[name] ?? config.AppendChild(configXml.CreateElement(name));
					node.InnerText = value.ToString();
				}
			}
		}
	}
}
