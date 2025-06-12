using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Core;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter
{
	public class PollingEndpoint : TransmitterEndpoint
	{
		readonly Func<XmlDocument, IReceiveHandler> receiveHandlerFactory;
		private readonly string pollingRequestSchemaName;

		[ExcludeFromCodeCoverage]
		public PollingEndpoint(AsyncTransmitter asyncTransmitter, string pollingRequestSchemaName, Func<ITransferrer> transferrerFactory)
			: this(asyncTransmitter, pollingRequestSchemaName, (configXml) => new ReceiveHandler(transferrerFactory, configXml), Logging.CreateLogger)
		{ }

		internal PollingEndpoint(AsyncTransmitter asyncTransmitter, string pollingRequestSchemaName, Func<XmlDocument, IReceiveHandler> receiveHandlerFactory, Func<string, XmlDocument, ILog> loggerFactory)
			: base(asyncTransmitter, loggerFactory)
		{
			this.receiveHandlerFactory = receiveHandlerFactory;
			this.pollingRequestSchemaName = pollingRequestSchemaName;
		}

		protected override async Task<IBaseMessage> TransmitMessageAsync(IBaseMessage message, XmlDocument configXml, ILog log, CancellationToken cancelToken)
		{
			string uri = "unknown";
			try
			{
				var messageType = (string)message.Context.Read("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties");
				if (messageType != pollingRequestSchemaName)
					throw new TransferrerException("Invalid request message received for polling adapter. Should be '" + pollingRequestSchemaName + "' but was '" + messageType + "'.");
				cancelToken.ThrowIfCancellationRequested();

				uri = ApplyConfigRequestMessageValues(message, configXml);
				cancelToken.ThrowIfCancellationRequested();

				log.InfoFormat("Starting polling from '{0}'.", uri);

				Stream responseStream;
				using (var receiveHandler = receiveHandlerFactory(configXml))
					responseStream = await CreateResponseMessage(configXml, log, cancelToken, receiveHandler, uri);

				responseStream.Position = 0;
				var responseMessage = MessageFactory.CreateMessage();
				responseMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
				responseMessage.BodyPart.Data = responseStream;
				return responseMessage;
			}
			catch (OperationCanceledException) { throw; }
			catch (Exception ex)
			{
				throw new TransferrerException(String.Format("Error polling from '{0}'. See inner exception for details.", uri), ex);
			}
			finally
			{
				log.InfoFormat("Finished polling from '{0}'.", uri);
			}
		}

		static async Task<Stream> CreateResponseMessage(XmlDocument configXml, ILog log, CancellationToken cancelToken, IReceiveHandler receiveHandler,
			string uri)
		{
			Stream responseStream;
			await receiveHandler.OpenAsync(configXml, log, cancelToken);
			cancelToken.ThrowIfCancellationRequested();

			var downloadedFiles = new List<TransferrerFileInfo>();
			var serverFiles = await receiveHandler.ListServerFilesAsync(log, cancelToken);

			using (var xWrtr = XmlWriter.Create(responseStream = new VirtualStream(),
				new XmlWriterSettings {Async = true, Encoding = new UTF8Encoding(false)}))
			{
				await xWrtr.WriteStartElementAsync("ns0", "PollingResponse", "http://cargowise.com/ehub/biztalkadapters/polling");
				await xWrtr.WriteAttributeStringAsync("", "Uri", "", uri);

				foreach (var file in serverFiles)
				{
					if (cancelToken.IsCancellationRequested) break;
					try
					{
						using (var fileStream = await receiveHandler.DownloadAsync(file, log, cancelToken))
						{
							if (cancelToken.IsCancellationRequested) break;

							await xWrtr.WriteStartElementAsync("", "File", "");
							await xWrtr.WriteAttributeStringAsync("", "Name", "", file.Name);

							var buffer = new byte[3072];
							int count;
							while ((count = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
								await xWrtr.WriteBase64Async(buffer, 0, count);

							await xWrtr.WriteEndElementAsync();
							downloadedFiles.Add(file);
						}
					}
					catch (OperationCanceledException)
					{
						break;
					}
				}

				await xWrtr.WriteEndElementAsync();
			}

			try
			{
				foreach (var file in downloadedFiles)
					await receiveHandler.PostDownloadProcessingAsync(file, log);
			}
			catch (Exception)
			{
			}

			await receiveHandler.CloseAsync(log, cancelToken);
			return responseStream;
		}

		static string ApplyConfigRequestMessageValues(IBaseMessage message, XmlDocument configXml)
		{
			var requestMessage = new XmlDocument();
			requestMessage.Load(message.BodyPart.GetOriginalDataStream());
			var uriNode = requestMessage.SelectSingleNode("/*/Uri");
			if (uriNode == null) throw new TransferrerException("Uri missing from PollingRequest message.");
			var location = new Location(uriNode.InnerText);
			var configNode = configXml["Config"];
			Func<string, XmlNode> getOrAddConfigNode =
				name => configNode[name] ?? configNode.AppendChild(configXml.CreateElement(name));
			getOrAddConfigNode("Server").InnerText = location.Server;
			if (location.Port.HasValue) getOrAddConfigNode("Port").InnerText = location.Port.ToString();
			getOrAddConfigNode("User").InnerText = location.UserName;
			getOrAddConfigNode("Folder").InnerText = location.Folder;
			getOrAddConfigNode("FileMask").InnerText = location.FileMask;
			foreach (XmlElement node in requestMessage.SelectNodes("/*/*[local-name()!='Uri']"))
				getOrAddConfigNode(node.Name).InnerText = node.InnerText;
			return uriNode.InnerText;
		}
	}
}
