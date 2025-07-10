using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Common;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.ServiceHost.Common;
using Enterprise.ZArchitecture.Core;
using SimpleLogger = Enterprise.UniversalDataBuss.Management.SimpleLogger;
namespace Enterprise.Services.ServiceHost
{
	class eAdaptorAsyncRequestProcessor
	{
		eAdaptorAsyncRequestProcessor() { }

		static string UniversalInterchangeSchemaName => EDIInterchangeTypeList.Descriptions.XDC;

		internal static HttpResponseMessage Post(HttpRequestMessage request, IeAdaptorConfig eAdaptorConfig) => eAdaptorRequestProcessorCore.ProcessRequestWithExceptionHandling(request, Post, eAdaptorConfig);

		SubStreamableStream incomingStream;
		string rootElement;
		XmlReader headerReader;
		IMessageHandler messageHandler;
		bool interchangeIsTemporary;
		EDIMessage ediMessage;
		IDisposable suppressReportRowDeletedError;

		static IeAdaptorRequestProcessorResult Post(SubStreamableStream incomingStream, IeAdaptorConfig eAdaptorConfig, string handlerName)
		{
			return new eAdaptorAsyncRequestProcessor().PostAndProcess(incomingStream, eAdaptorConfig, handlerName);
		}

		IeAdaptorRequestProcessorResult PostAndProcess(SubStreamableStream incomingStream, IeAdaptorConfig eAdaptorConfig, string handlerName)
		{
			this.incomingStream = incomingStream;
			try
			{
				try
				{
					(headerReader, rootElement) = ReadRootElement(incomingStream);
					if (string.Equals(rootElement, "UniversalInterchange", StringComparison.Ordinal))
					{
						messageHandler = HandlerFactory.GetHandler(UniversalInterchangeSchemaName);
						messageHandler.InterchangeCreated += OnInterchangeCreated;
					}
					else
					{
						SetStreamAndHandlerForSingleMessage();
					}
					messageHandler.MessageCreated += OnMessageCreated;
				}
				catch (XmlException)
				{
					return new eAdaptorRequestProcessorResult(HttpStatusCode.BadRequest, EDIMessageStatusList.Codes.Error, CreateErrorResponse((NoResString)"Invalid XML."));
				}

				if (!TryDeserializeHeader(headerReader, rootElement, out Header header, out string headerErrorMessage))
				{
					return new eAdaptorRequestProcessorResult(HttpStatusCode.BadRequest, EDIMessageStatusList.Codes.Error, CreateErrorResponse(headerErrorMessage));
				}

				if (!IsHeaderValid(header, out var errorMessage))
				{
					return new eAdaptorRequestProcessorResult(HttpStatusCode.BadRequest, EDIMessageStatusList.Codes.Error, CreateErrorResponse(errorMessage));
				}

				return ProcessMessage(header);
			}
			finally
			{
				suppressReportRowDeletedError?.Dispose();
			}
		}

		void SetStreamAndHandlerForSingleMessage()
		{
			incomingStream.Position = FindStreamStartReadingPosition();
			incomingStream = WrapIncomingStreamWithInterchange(incomingStream);
			headerReader = XmlReader.Create(new StreamReader(incomingStream));
			headerReader.MoveToContent();
			rootElement = "UniversalInterchange";
			messageHandler = HandlerFactory.GetHandler(UniversalInterchangeSchemaName);
			messageHandler.BeforeSavingInterchange += OnBeforeSavingInterchange;
			interchangeIsTemporary = true;
		}

		long FindStreamStartReadingPosition()
		{
			//We need to skip the XML Identifier tag (<?xml ...) to wrap it in UnversalInterchange
			//The headerReader is already at the start of the XML content (MoveToContent() has been called).
			var lineNumber = ((IXmlLineInfo)headerReader).LineNumber - 1; //Indexes start at 1
			var linePosition = ((IXmlLineInfo)headerReader).LinePosition - 2; //Apparently, skips the '<' character
			incomingStream.Position = 0;
			using var streamReader = new StreamReader(incomingStream, Encoding.UTF8, true, 1024, true);
			using var sr = new PositioningReader(streamReader);
			while (lineNumber > 0)
			{
				sr.ReadLine();
				lineNumber--;
			}
			while (linePosition > 0)
			{
				sr.Read();
				linePosition--;
			}
			return sr.BytePosition;
		}

		void OnMessageCreated(EDIMessage message)
		{
			message.EM_ECC_CommunicationPartyConfig = ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig?.PK ?? ZGuid.Empty;
			message.EM_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
			ediMessage = message;
		}

		static void OnInterchangeCreated(EDIInterchange interchange)
		{
			var inboundConfig = ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig?.PK ?? ZGuid.Empty;
			interchange.EI_ECC_CommunicationPartyConfig = inboundConfig;

			if (interchange.Factory.Load<EDICommunicationPartyConfig>(inboundConfig)?.Party?.Configs?.Cast<EDICommunicationPartyConfig>().FirstOrDefault(c => c.ECC_IsActive && c.ECC_Direction == EDICommunicationPartyConfigDirectionsList.Codes.Outbound) == null)
			{
				interchange.AddMessageProcessWarning(Res.GetString("1419F7CF-A811-4419-88E8-13A76D768E3C", "Acknowledgement message requested, but acknowledgement will not be generated as outbound configuration is missing."));
			}
		}

		System.Collections.Generic.IReadOnlyList<IEDIMessage> savedMessages;
		void OnBeforeSavingInterchange(EDIInterchange interchange)
		{
			savedMessages = interchange.ContainedMessages.OfType<IEDIMessage>().ToList();
			interchange.ContainedMessages.RemoveAll();
			interchange.Delete();
			suppressReportRowDeletedError = ((IBusinessObjectInternals)interchange).SuppressReportRowDeletedError();
		}

		static ZString? GetErrorOrWarningMessage(IMessageHandlerResult result)
		{
			if (result.FailureReason != ZString.Empty)
			{
				return result.FailureReason;
			}
			else if (result.Warnings.Any())
			{
				return string.Join(System.Environment.NewLine, result.Warnings);
			}
			return null;
		}

		static SubStreamableStream WrapIncomingStreamWithInterchange(SubStreamableStream incomingStream)
		{
			var start = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(interchangeWraperStart, GlbCompany.CurrentCompany.LicenceKeyIdentifier)));
			var end = new MemoryStream(Encoding.UTF8.GetBytes(interchangeWraperEnd));
			var multiStream = new MultiStream(start, incomingStream, end);
			multiStream.Position = 0;
			return new SubStreamableStream(multiStream, EmptyDisposableLeakListener.Instance);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML")]
		const string interchangeWraperStart = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>{0}</SenderID>
    <RecipientID>{0}</RecipientID>
  </Header>
  <Body>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML")]
		const string interchangeWraperEnd = "</Body></UniversalInterchange>";

		static (XmlReader, string rootElement) ReadRootElement(SubStreamableStream stream)
		{
			stream.Position = 0;
			var reader = XmlReader.Create(new StreamReader(stream));
			reader.MoveToContent();
			var rootElement = string.IsNullOrEmpty(reader.Prefix) ? reader.Name : reader.LocalName;
			return (reader, rootElement);
		}

		static bool TryDeserializeHeader(XmlReader reader, string rootElement, out Header header, out string errorMessage)
		{
			header = null;
			errorMessage = "";
			try
			{
				if (reader.ReadToFollowing((NoResString)"Header")) // Not a code smell.
				{
					var @namespace = reader.NamespaceURI;
					var xml = reader.ReadOuterXml();
					var xmlSerializer = new XmlSerializer(typeof(Header), @namespace); // ZXmlSerializer not applicable in this instance.
					header = xmlSerializer.Deserialize(new StringReader(xml)) as Header;
					if (header != null)
					{
						header.ParentElement = rootElement;
						return true;
					}
				}
			}
			catch (XmlException)
			{
				errorMessage = (NoResString)"Invalid XML."; // Error message, currently supported in English only.
				return false;
			}

			errorMessage = (NoResString)"XML does not contain a valid Header element."; // Error message, currently supported in English only.
			return false;
		}

		static bool IsHeaderValid(Header header, out string errorMessage)
		{
			errorMessage = null;

			if (string.IsNullOrEmpty(header.SenderID))
			{
				errorMessage = $"Header does not contain a valid {nameof(header.SenderID)}."; // Error message, currently supported in English only.
			}
			else if (string.IsNullOrEmpty(header.RecipientID))
			{
				errorMessage = $"Header does not contain a valid {nameof(header.RecipientID)}."; // Error message, currently supported in English only.
			}

			return errorMessage == null;
		}

		IeAdaptorRequestProcessorResult ProcessMessage(Header header)
		{
			var trackingID = Guid.NewGuid();
			using (var requestEdiMessageStream = incomingStream.Copy())
			{
				IeHubMessage wrappedMessage = new eHubMessage(
					trackingID: trackingID,
					senderID: header.SenderID,
					recipientID: header.RecipientID,
					schemaType: MessageSchemaType.Xml,
					applicationCode: ApplicationCodeList.Codes.XMS,
					schemaName: UniversalInterchangeSchemaName,
				messageStream: requestEdiMessageStream);

				try
				{
					var logger = new SimpleLogger();
					var result = messageHandler.SaveMessageFromAdapter(wrappedMessage);
					if (interchangeIsTemporary)
					{
						if (!(ediMessage?.IsInDatabase ?? false))
						{
							return new eAdaptorRequestProcessorResult(HttpStatusCode.BadRequest, EDIMessageStatusList.Codes.Error, CreateErrorResponse(GetErrorOrWarningMessage(result) ?? (NoResString)"This message is not supported"));
						}
						result = new MessageNumberResult
						{
							MessageNumbers = savedMessages.Select(m => m.EM_MessageNum),
							ExternalReferenceNumbers = savedMessages.Select(m => m.EM_ExternalReferenceNumber),
						};
					}

					var statusCode = HttpStatusCode.OK;
					var universalStatusCode = UniversalResponseStatus.ProcessedOK;

					if (!result.FailureReason.IsEmpty)
					{
						statusCode = HttpStatusCode.BadRequest;
						universalStatusCode = UniversalResponseStatus.Error;
						logger.Log(LogType.Error, result.FailureReason);
					}
					else if (!result.Warnings.IsNullOrEmpty())
					{
						result.Warnings.ToList().ForEach(x => logger.Log(LogType.Warning, x));
					}

					return new eAdaptorRequestProcessorResult(statusCode, EDIMessageStatusList.Codes.ProcessedOK, UniversalResponseWriter.CreateResponse(universalStatusCode, null, new StringReader(logger.ToString()), result, emitMessageNumberCollections: true), CustomHeadersHelper.CreateCustomHeadersForAsyncRequest(result));
				}
				catch (MessageHandlerException ex)
				{
					return new eAdaptorRequestProcessorResult(HttpStatusCode.BadRequest, EDIMessageStatusList.Codes.Error, CreateErrorResponse(ex.Message));
				}
			}
		}

		static SubStreamableStream CreateErrorResponse(string errorMessage)
		{
			var logger = new SimpleLogger();
			logger.Log(LogType.Error, errorMessage);
			return UniversalResponseWriter.CreateResponse(UniversalResponseStatus.Error, null, new StringReader(logger.ToString()));
		}
	}

	public class Header
	{
		public string SenderID { get; set; }
		public string RecipientID { get; set; }
		internal string ParentElement { get; set; }
	}
}
