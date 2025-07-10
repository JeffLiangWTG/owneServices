using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE431And531MessageProcessor : DMSResponseMessageProcessor
{
	public IE431And531MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Reminder);

		SetEntryNumberExpiryDate(entryHeader, dataProvider);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(NLEDIMessageInterpreterHelper.ResStrings.CustomsReminder,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Res.GetString("D5BEDE48-4351-4A26-AFDD-522550437BE1", "Declaration awaiting a supplement"),
			Res.GetString("8DEEB94A-65AD-4C39-A243-F8807ABD1157", "If no supplement is sent before Expiry, the declaration may be amended by Customs and (if applicable) preference will be removed"),
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);

	protected void SetEntryNumberExpiryDate(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.Controls.FirstOrDefault()?.LimitDate);
	}
}
