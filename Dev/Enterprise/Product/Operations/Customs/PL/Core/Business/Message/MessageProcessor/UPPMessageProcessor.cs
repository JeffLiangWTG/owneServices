using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business;

public class UPPMessageProcessor(LoggingInformation logger) : ImpExpMessageProcessorBase<IConfirmation>(logger)
{
	protected override string MessageFriendlyNameCore => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.UPP}";

	protected override Type MessageInterpreterType => typeof(ConfirmationMessageInterpreter<CusEntryHeader>);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IConfirmation messageDataProvider)
	{
		var cusEntryHeader = (CusEntryHeader)message.EM_LinkedObject;

		cusEntryHeader.CH_EntryStatus = LogicalStatusList.Codes.Accepted;
		return ProcessingResult.Succeed;
	}
}
