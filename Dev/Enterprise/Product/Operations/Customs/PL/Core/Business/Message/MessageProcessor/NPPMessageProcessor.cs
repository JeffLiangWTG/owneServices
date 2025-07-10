using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business;

public class NPPMessageProcessor(LoggingInformation logger) : ImpExpMessageProcessorBase<IConfirmation>(logger)
{
	protected override string MessageFriendlyNameCore => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.NPP}";

	protected override bool IsFailureNotification => true;

	protected override Type MessageInterpreterType => typeof(NPPMessageInterpreter<CusEntryHeader>);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IConfirmation messageDataProvider)
	{
		var cusEntryHeader = (CusEntryHeader)message.EM_LinkedObject;

		cusEntryHeader.CH_EntryStatus = LogicalStatusList.Codes.Error;
		return ProcessingResult.Succeed;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportMessageErrors;
}
