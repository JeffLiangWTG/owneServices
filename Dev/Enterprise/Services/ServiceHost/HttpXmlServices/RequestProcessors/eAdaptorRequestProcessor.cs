using System;
using System.IO;
using System.Net;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Services.ServiceHost.Common;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	static class eAdaptorRequestProcessor
	{
		const int MaxRetryCount = 3;

		internal static HttpResponseMessage Post(HttpRequestMessage request, IeAdaptorConfig eAdaptorConfig, string handlerName = "") => eAdaptorRequestProcessorCore.ProcessRequestWithExceptionHandling(request, Post, eAdaptorConfig, handlerName);

		static IeAdaptorRequestProcessorResult Post(SubStreamableStream incomingStream, IeAdaptorConfig eAdaptorConfig, string handlerName)
		{
			var currentAttempt = 0;
			var shouldRetry = false;
			string processStatus = null;
			IHttpXmlRequestResponse requestEdiMessage = null;
			var statusCode = HttpStatusCode.OK;
			var emStatusCode = EDIMessageStatusList.Codes.ProcessedOK;
			IXmlSessionTracker xmlSessionTracker = null;
			var context = ObjectFactory.Get<IMessagingContext>().CurrentInboundConfig;
			var clientName = context?.Party.ECP_Name;

			do
			{
				try
				{
					xmlSessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());

					var handler = eAdaptorHandlerFactory.GetHandler(xmlSessionTracker, eAdaptorConfig, handlerName, incomingStream);
					using (var disposableManager = new DisposableManager())
					{
						if (handler != null)
						{
							if (requestEdiMessage == null)
							{
								// On first attempt write the message to the DB
								requestEdiMessage = handler.CreateRequestMessage(disposableManager);
								{
									if (context != null)
									{
										((IHttpXmlEDIMessage)requestEdiMessage).EM_ECC_CommunicationPartyConfig = context.PK;
									}
									var requestEdiMessageStream = disposableManager.Subscribe(incomingStream.Copy());
									incomingStream.Position = 0;
									requestEdiMessage.SetMessageTextSource(requestEdiMessageStream);
									var number = eAdaptorExternalReferenceNumberHelper.GetExternalReferenceNumber(incomingStream);
									incomingStream.Position = 0;
									((IHttpXmlEDIMessage)requestEdiMessage).EM_ExternalReferenceNumber = number;
									((IHttpXmlEDIMessage)requestEdiMessage).EM_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
									ZExceptionReporting.ProcessWithSaveExceptionHandling(requestEdiMessage.Save, () => incomingStream.Position = 0);
								}
							}
							else
							{
								// On all other attempts reload this message
								var messageFactory = new BusinessObjectFactory() { NameForDebugging = "Retry Process Stream", RefreshEnabled = false };
								messageFactory.AddDisposableService(disposableManager);
								requestEdiMessage = messageFactory.Load<IHttpXmlEDIMessage>(((BusinessObject)requestEdiMessage).PK);
							}

							var responseMessage = handler.CreateResponseMessage(disposableManager);
							((IEDIMessage)responseMessage).EM_EM_RequestMessage = ((IHttpXmlEDIMessage)requestEdiMessage).PK;
							((IEDIMessage)responseMessage).EM_ExternalReferenceNumber = ((IHttpXmlEDIMessage)requestEdiMessage).EM_ExternalReferenceNumber;
							((IEDIMessage)responseMessage).EM_TransportType = EDIInterchangeTransportTypeList.Codes.eAdaptor;
							if (context != null)
							{
								((IHttpXmlEDIMessage)responseMessage).EM_ECC_CommunicationPartyConfig = context.PK;
							}
							var messageSaver = new UniversalResponseSaver(requestEdiMessage, responseMessage, xmlSessionTracker);
							if (currentAttempt >= MaxRetryCount)
							{
								messageSaver.IsFinalProcessingAttempt = true;
								xmlSessionTracker.Log(Integration.LogType.Information, $"Processing stream after {MaxRetryCount} retries.");
							}
							else
							{
								messageSaver.IsFinalProcessingAttempt = false;
							}
							incomingStream.Position = 0;
							using (var processStream = incomingStream.Copy())
							using (var processingResult = handler.Process(processStream, requestEdiMessage, messageSaver))
							{
								if (processingResult != null)
								{
									processStatus = processingResult.Status;
									emStatusCode = ((IHttpXmlEDIMessage)requestEdiMessage).EM_Status;
									shouldRetry = processingResult.ShouldRetry;
									if (processingResult.ResponseMessageText != null && (!shouldRetry || messageSaver.IsFinalProcessingAttempt))
									{
										if (processingResult.FullResponseMessageText != null)
										{
											return new eAdaptorRequestProcessorResult(statusCode, emStatusCode, processingResult.FullResponseMessageText.Copy(), CustomHeadersHelper.CreateCustomHeadersForSyncRequest((IEDIMessage)requestEdiMessage, clientName, (IEDIMessage)responseMessage));
										}
										else
										{
											return new eAdaptorRequestProcessorResult(statusCode, emStatusCode, UniversalResponseWriter.CreateResponse(processStatus, processingResult.ResponseMessageText, new StringReader(xmlSessionTracker.ToString()), new SingleMessageResult(((IEDIMessage)requestEdiMessage).EM_MessageNum, ((IEDIMessage)requestEdiMessage).EM_ExternalReferenceNumber), xmlSessionTracker.ValidationRuleCollection), CustomHeadersHelper.CreateCustomHeadersForSyncRequest((IEDIMessage)requestEdiMessage, clientName, (IEDIMessage)responseMessage));
										}
									}
								}
								else
								{
									break;
								}
							}
						}
						else
						{
							return new eAdaptorRequestProcessorResult(statusCode, emStatusCode, CreateErrorResponse((EZC.NoResString)"Unrecognized root element or malformed XML. Examples of supported root elements are: UniversalShipmentRequest, Native, etc.", ZString.Empty, ZString.Empty), CustomHeadersHelper.CreateCustomHeadersForSyncRequest((IEDIMessage)requestEdiMessage, clientName));
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var innerEx = ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex);
					if (innerEx != null)
					{
						var messageNumber = ((IEDIMessage)requestEdiMessage)?.EM_MessageNum;
						var externalReferenceNumber = ((IEDIMessage)requestEdiMessage)?.EM_ExternalReferenceNumber;
						return new eAdaptorRequestProcessorResult(statusCode, EDIMessage.Status.Error, CreateErrorResponse(innerEx.Message, messageNumber, externalReferenceNumber), CustomHeadersHelper.CreateCustomHeadersForSyncRequest((IEDIMessage)requestEdiMessage, clientName));
					}
					else
					{
						throw;
					}
				}
			} while (shouldRetry && currentAttempt++ < MaxRetryCount);

			return new eAdaptorRequestProcessorResult(statusCode, emStatusCode, UniversalResponseWriter.CreateResponse(processStatus ?? EDIMessageStatusList.Codes.Error, null, new StringReader(xmlSessionTracker.ToString()), new SingleMessageResult(((IEDIMessage)requestEdiMessage).EM_MessageNum, ((IEDIMessage)requestEdiMessage).EM_ExternalReferenceNumber)), CustomHeadersHelper.CreateCustomHeadersForSyncRequest((IEDIMessage)requestEdiMessage, clientName));
		}

		static SubStreamableStream CreateErrorResponse(string errorMessage, string requestMessageNumber, string externalReferenceNumber)
		{
			var messageResult = !string.IsNullOrEmpty(requestMessageNumber) ? new SingleMessageResult(requestMessageNumber, externalReferenceNumber) : null;
			return UniversalResponseWriter.CreateResponse(UniversalResponseStatus.Error, null, new StringReader(errorMessage), messageResult);
		}
	}
}
