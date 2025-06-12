using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.TransportProxy.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class TransmitterEndpoint : AsyncTransmitterEndpoint
	{
		readonly ILoggerFactory loggerFactory;
		internal Transmitter Transmitter { get; }
		private IBTTransportProxy TransportProxy { get; }
		protected IBaseMessageFactory MessageFactory { get; private set; }
		private TransmitterEndpointParameters endpointParameters;
		protected string configNamespace;
		protected CancellationToken HandlerCancelToken { get; }
		private readonly CancellationTokenSource sendCancelTokenSource;
		private readonly CancellationTokenRegistration cancelRegistration;
		private static readonly object GenerateBtsDateTimeLock = new object();
		private static string _lastBtsDateTime = string.Empty;

		protected TransmitterEndpoint(AsyncTransmitter transmitter, ILoggerFactory loggerFactory) : base(transmitter)
		{
			Transmitter = (Transmitter)transmitter;
			this.loggerFactory = loggerFactory;

			TransportProxy = Transmitter.TransportProxy;
			MessageFactory = TransportProxy.GetMessageFactory();

			Transmitter.Handler.RegisterEndpoint(this);

			HandlerCancelToken = Transmitter.Handler.CancelTokenSource.Token;
			sendCancelTokenSource = new CancellationTokenSource();
			cancelRegistration = HandlerCancelToken.Register(()
				=> sendCancelTokenSource.CancelAfter(Transmitter.Handler.terminateWaitLimit));
		}

		public override void Open(EndpointParameters endpointParameters, IPropertyBag handlerPropertyBag, string propertyNamespace)
		{
			this.endpointParameters = (TransmitterEndpointParameters)endpointParameters;
			configNamespace = propertyNamespace;
		}

		public override IBaseMessage ProcessMessage(IBaseMessage message)
		{
			var transmitInstanceId = Guid.TryParse((string)message.Context.Read("TransmitInstanceID", "http://schemas.microsoft.com/BizTalk/2003/system-properties"), out var g) ? g : Guid.Empty;
			var configXml = new XmlDocument();
			if (message.Context.Read("AdapterConfig", configNamespace) is string config)
			{
				configXml.LoadXml(config);
			}
			else
			{
				configXml.AppendChild(configXml.CreateElement("Config")).AppendChild(configXml.CreateElement("uri")).InnerText = endpointParameters.Uri;
			}
			ApplyConfigDynamicOverrides(message, "http://cargowise.com/ehub/biztalkadapters/logging-properties", configXml);

			var logSubDir = endpointParameters.PortName.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0];
			if (configXml["Config"] is var element && element != null)
			{
				element.AppendChild(configXml.CreateElement("LogDir")).InnerText =
					Path.Combine(Transmitter.Handler.InterfacesLogDir, logSubDir);
				element.AppendChild(configXml.CreateElement("StructuredLogDir")).InnerText =
					Path.Combine(Transmitter.Handler.StructuredLogDir, logSubDir);
			}

			var activityId = $"{transmitInstanceId.ToString("D").ToUpper()},{message.MessageID.ToString("D").ToUpper()}";
			var log = loggerFactory.CreateLogger(endpointParameters.PortName, "Send", configXml);
			message.BodyPart.GetSize(out var size, out var sizeImplemented);
			log.Log(activityId, LogLevel.Info, "Starting transmit to '{0}'. SessionKey='{1}' TransmitInstanceID='{2}' Size={3} bytes.",
				endpointParameters.Uri, endpointParameters.SessionKey, transmitInstanceId, sizeImplemented ? size.ToString("N0") : "?");
			if (log.IsDebugEnabled)
				LogMessageDetails(log, message, activityId);

			try
			{
				Transmitter.Handler.CancelTokenSource.Token.ThrowIfCancellationRequested();

				return TransmitMessage(message, configXml, log, activityId, sendCancelTokenSource.Token);
			}
			catch (Exception ex)
			{
				Transmitter.Handler.LogInterfaceError(log, activityId, "Error in send port '{0}'", ex, endpointParameters.PortName);
				throw;
			}
		}

		protected virtual IBaseMessage TransmitMessage(IBaseMessage message, XmlDocument configXml, ILog log, string activityId, CancellationToken cancelToken)
			=> TransmitMessageAsync(message, configXml, log, activityId, sendCancelTokenSource.Token).GetAwaiter().GetResult();

		protected virtual Task<IBaseMessage> TransmitMessageAsync(IBaseMessage message, XmlDocument configXml, ILog log, string activityId, CancellationToken cancelToken)
			=> Task.FromResult<IBaseMessage>(null);

		protected static void ApplyConfigDynamicOverrides(IBaseMessage message, string propertyNamespace, XmlDocument configXml)
		{
			var config = configXml["Config"];
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				object value = message.Context.ReadAt(i, out var name, out var ns);
				if (ns == propertyNamespace)
				{
					var node = config[name] ?? config.AppendChild(configXml.CreateElement(name));
					node.InnerText = value.ToString();
				}
			}
		}

		private void LogMessageDetails(ILog log, IBaseMessage message, string activityId)
		{
			var propertiesMessage = new StringBuilder().Append("Message context properties:");
			for (int i = 0; i < message.Context.CountProperties; i++)
			{
				object val = message.Context.ReadAt(i, out var name, out var ns);
				propertiesMessage.AppendLine().AppendFormat("   {0}#{1}={2}", ns, name, val);
			}
			log.Log(activityId, LogLevel.Debug, propertiesMessage.ToString());

			if (log.IsTraceEnabled)
			{
				var messageStream = message.BodyPart.GetOriginalDataStream();
				if (!messageStream.CanSeek)
				{
					message.BodyPart.Data = messageStream = new ReadOnlySeekableStream(messageStream, new VirtualStream());
				}
				var startPosition = messageStream.Position;
				var contentMessage = new StringBuilder().Append("Message content:");
				const int maxContent = 1024000;
				using (var sr = new StreamReader(messageStream, Encoding.UTF8, true, 1024, true))
				{
					string line;
					while (contentMessage.Length < maxContent && (line = sr.ReadLine()) != null)
					{
						var lineToAdd = line.Substring(0, Math.Min(line.Length, maxContent - contentMessage.Length));
						contentMessage.AppendLine().Append(lineToAdd.PadLeft(lineToAdd.Length + 3, ' '));
					}
				}
				log.Log(activityId, LogLevel.Debug, contentMessage.ToString());
				messageStream.Position = startPosition;
			}
		}

		public static string[] ReplaceFileNameMacros(string[] fileNames, IBaseMessage message)
		{
			string overrideFileName = null;
			string messageId = null;
			string destinationPartyQualifier = null;
			string btsDateTime = null;

			return fileNames.Select(fileName =>
				{
					if (string.IsNullOrEmpty(fileName)) { return fileName; }

					fileName = fileName.Replace("%OverrideFilename%", overrideFileName ??= Path.GetFileName((string)message.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06")) ?? "%OverrideFilename%")
						.Replace("%MessageID%", messageId ??= message.MessageID.ToString("B").ToUpper())
						.Replace("%DestinationPartyQualifier%", destinationPartyQualifier ??= (string)message.Context.Read("DestinationPartyQualifier", "http://schemas.microsoft.com/BizTalk/2003/system-properties") ?? "%DestinationPartyQualifier%");

					if (fileName.Contains("%datetime_bts2000%"))
						fileName = fileName.Replace("%datetime_bts2000%", btsDateTime ??= GenerateBtsDateTime());

					return fileName;
				}
			).ToArray();
		}

		internal static Func<string> GenerateBtsDateTime = () =>
		{
			lock (GenerateBtsDateTimeLock)
			{
				string btsDateTime;
				while ((btsDateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmssf", CultureInfo.InvariantCulture)) == _lastBtsDateTime)
				{
					Thread.Sleep(100);
				}

				return _lastBtsDateTime = btsDateTime;
			}
		};
	}
}
