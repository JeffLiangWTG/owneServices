using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class UniversalXmlMessageHandler<T> : IHttpXmlMessageHandler where T : TopLevelDataObject, new()
	{
		protected IXmlSessionTracker xmlSessionTracker;

		public IHttpXmlProcessingConfig ProcessingConfig { get; }

		protected UniversalXmlMessageHandler(IXmlSessionTracker xmlSessionTracker, IHttpXmlProcessingConfig processingConfig)
		{
			Argument.NotNull(processingConfig, nameof(processingConfig));

			this.xmlSessionTracker = xmlSessionTracker;
			this.ProcessingConfig = processingConfig;
		}

		public IHttpXmlRequestResponse CreateRequestMessage(DisposableManager manager = null)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "Request Message", RefreshEnabled = false };
			factory.SuspendValidation();
			if (manager != null)
			{
				factory.AddDisposableService(manager);
			}
			var result = factory.New<IHttpXmlEDIMessage>();
			result.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			result.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			result.EM_MessageSubType = RequestMessageSubType;
			result.EM_Status = EDIMessageStatusList.Codes.Recognised; // Even though recognized is not a recognized status in business, we're leaving this as is because if the status is saved it tells us something went fubar and we need to fix it.
			return result;
		}

		public IHttpXmlRequestResponse CreateResponseMessage(DisposableManager manager = null)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = "Response Message", RefreshEnabled = false };
			factory.SuspendValidation();
			if (manager != null)
			{
				factory.AddDisposableService(manager);
			}
			var result = factory.New<IHttpXmlEDIMessage>();
			result.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			result.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			result.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalResponse;
			result.EM_Status = EDIMessageStatusList.Codes.Sent;
			return result;
		}

		protected abstract string RequestMessageSubType { get; }

		public IHttpXmlProcessingResult Process(IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver messageSaver = null)
		{
			return Process(requestMessage, (requestEdiMessage, xmlImportLogger, codemapper) => requestEdiMessage.GetEM_MessageTextReader().Parse<T>(xmlImportLogger, codemapper, this.ProcessingConfig.ThrowOnParsingError), messageSaver);
		}

		public IHttpXmlProcessingResult Process(SubStreamableStream stream, IHttpXmlRequestResponse requestMessage, IHttpXmlMessageSaver messageSaver = null)
		{
			return Process(requestMessage, (_, xmlImportLogger, codemapper) => stream.Parse<T>(xmlImportLogger, codemapper, this.ProcessingConfig.ThrowOnParsingError), messageSaver);
		}

		IHttpXmlProcessingResult Process(IHttpXmlRequestResponse requestMessage,
			Func<IHttpXmlEDIMessage, IXmlImportLogger, CodeMappingManager, T> getTopLevelDataObject,
			IHttpXmlMessageSaver messageSaver = null)
		{
			var requestEdiMessage = (IHttpXmlEDIMessage)requestMessage;

			try
			{
				var factory = requestEdiMessage.Factory;
				var codemapper = new CodeMappingManager(this.xmlSessionTracker);
				var topLevelDataObject = getTopLevelDataObject(requestEdiMessage, this.xmlSessionTracker, codemapper);

				using (topLevelDataObject)
				using (topLevelDataObject is IRequestDataObject ? ProcessTask.Loader.SuppressTemplateApplication() : null)
				{
					return ProcessDataObject(factory, topLevelDataObject, requestEdiMessage, codemapper, messageSaver);
				}
			}
			catch (XmlProcessingException)
			{
				Workflow.UniversalXmlWorkflowProcessor.TryUseNewFactoryToChangeMessageStatus(requestEdiMessage, xmlSessionTracker, EDIMessageStatusList.Codes.Rejected);
				return new HttpXmlProcessingResult() { Status = HttpXmlResultStatusList.Codes.ProcessedOK };
			}
		}

		protected abstract IHttpXmlProcessingResult ProcessDataObject(BusinessObjectFactory factory,
			T topLevelDataObject,
			IHttpXmlEDIMessage requestEdiMessage,
			ICodeMappingManager codeMapper = null,
			IHttpXmlMessageSaver messageSaver = null);

		protected void UpdateStatuses(bool successful, HttpXmlProcessingResult result, IHttpXmlEDIMessage message)
		{
			UpdateResultMessageStatus(successful, result, message);
			UpdateRequestMessageStatus(successful, message);
		}

		protected void UpdateResultMessageStatus(bool successful, HttpXmlProcessingResult result, IHttpXmlEDIMessage message)
		{
			if (successful)
			{
				result.Status = HttpXmlResultStatusList.Codes.ProcessedOK;
				if (message.Status == EDIMessageStatusList.Codes.Failed)
				{
					result.Status = HttpXmlResultStatusList.Codes.Error;
				}
			}
			else
			{
				result.Status = HttpXmlResultStatusList.Codes.Error;
			}
		}

		protected void UpdateRequestMessageStatus(bool successful, IHttpXmlEDIMessage message)
		{
			if (successful)
			{
				if (message.Status == EDIMessageStatusList.Codes.Recognised)
				{
					message.Status = EDIMessageStatusList.Codes.ProcessedOK;
					if (xmlSessionTracker.HasWarnings)
					{
						message.Status = EDIMessageStatusList.Codes.Warning;
					}
				}
			}
			else
			{
				if (message.Status == EDIMessageStatusList.Codes.Recognised)
				{
					message.Status = EDIMessageStatusList.Codes.Rejected;
				}
			}
		}
	}
}
