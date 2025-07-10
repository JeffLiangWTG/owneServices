using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business;

public class UPOMessageProcessor(LoggingInformation logger) : ImpExpMessageProcessorBase<IUpo>(logger)
{
	protected override string MessageFriendlyNameCore => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.UPO}";

	protected override string EmailSubject(BaseEDIMessage message, IUpo dataProvider)
		=> $"{MessageTitles.UPO}{UPOInterpreterBase<CusEntryHeader>.GetTransmitDocumentTypeForTitle(dataProvider)} {dataProvider.CorrelationIdentifier}.";

	protected override Type MessageInterpreterType => typeof(UPOMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IUpo messageDataProvider)
	{
		var cusEntryHeader = (CusEntryHeader)message.EM_LinkedObject;
		if (cusEntryHeader.CH_EntryStatus == LogicalStatusList.Codes.Sent)
		{
			cusEntryHeader.CH_EntryStatus = LogicalStatusList.Codes.Accepted;
		}

		return ProcessingResult.Succeed;
	}
}
