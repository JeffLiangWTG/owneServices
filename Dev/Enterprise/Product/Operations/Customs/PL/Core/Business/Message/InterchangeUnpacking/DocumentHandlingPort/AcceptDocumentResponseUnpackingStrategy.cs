using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces.DocumentHandlingPort;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ZArchitecture.Business.AutoEvents;

namespace Enterprise.Customs.PL.Business;

sealed class AcceptDocumentResponseUnpackingStrategy(DataProviderFactory dataProviderFactory)
	: XmlInterchangeUnpackingStrategy<IAcceptDocumentResponse>(dataProviderFactory)
{
	protected override EDIInterchangeUnpackerResult ProcessDataCore(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		IAcceptDocumentResponse acceptDocumentResponse,
		ISimpleLogger logger)
	{
		var sysRef = acceptDocumentResponse.SysRef;
		if (sysRef.IsNullOrEmpty())
		{
			return new EDIInterchangeUnpackerResult(errorReason:
				(NoResString)"Interchange processing failed because AcceptDocumentResponse doesn't contain sysRef value.");
		}

		outgoingMessage.EM_ApplicationReference = sysRef;
		LogHelper.Log(InterchangeAcknowledged, $"AcceptDocumentResponse acknowledgement is processed, a SystemReferenceID [{acceptDocumentResponse.SysRef}] was assigned to the message {outgoingMessage.EM_MessageNum}.",
			serviceLog: logger,
			interchange: interchange,
			transmitMessage: outgoingMessage);
		return new EDIInterchangeUnpackerResult(Array.Empty<EnterpriseEDIMessage>());
	}
}
