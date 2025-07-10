using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE460And560MessageProcessor : DMSResponseMessageProcessor
{
	public IE460And560MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		var entryStatus = string.Empty;
		var status = string.Empty;
		if (dataProvider.WCOTypeCode.EqualIgnoringOrder(WCoTypeCodes.ExportControlNotification))
		{
			(entryStatus, status) = DMSResponseMessageHelper.GetControlNotificationMessageStatusesForExport(dataProvider);
		}
		else
		{
			(entryStatus, status) = DMSResponseMessageHelper.GetControlNotificationMessageStatuses(dataProvider);
		}
		SetEntryStatuses(entryHeader, entryStatus, status);

		SetEntryHeaderSubmittedDate(entryHeader, dataProvider);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new IE460And560MessageInterpreter().Interpret(dataProvider, message);
}
