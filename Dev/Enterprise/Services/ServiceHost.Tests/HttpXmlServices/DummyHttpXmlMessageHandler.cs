using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.Messaging.Integration;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DummyHttpXmlMessageHandler : IHttpXmlMessageHandler
	{
		public IHttpXmlProcessingConfig ProcessingConfig => new DefaultProcessingConfig();

		public IHttpXmlRequestResponse CreateRequestMessage(DisposableManager manager = null)
		{
			var result = new BusinessObjectFactory().New<IHttpXmlEDIMessage>();
			result.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			result.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			result.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalResponse;
			result.EM_Status = EDIMessageStatusList.Codes.Recognised;
			return result;
		}

		public IHttpXmlRequestResponse requestMessage;

		public IHttpXmlRequestResponse CreateResponseMessage(DisposableManager manager = null)
		{
			var result = new BusinessObjectFactory().New<IHttpXmlEDIMessage>();
			result.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			result.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			result.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalResponse;
			result.EM_Status = EDIMessageStatusList.Codes.Sent;
			return result;
		}

		public IHttpXmlRequestResponse responseMessage;

		public IHttpXmlProcessingResult Process(IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver saver = null)
		{
			return processingResult;
		}

		public IHttpXmlProcessingResult Process(SubStreamableStream stream, IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver saver = null)
		{
			return processingResult;
		}

		public HttpXmlProcessingResult processingResult;
	}
}
