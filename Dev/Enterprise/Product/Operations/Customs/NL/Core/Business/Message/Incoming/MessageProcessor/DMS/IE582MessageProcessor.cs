using System;
using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE582MessageProcessor : DMSResponseMessageProcessor
{
	public IE582MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (DMSResponseMessageHelper.IsSubstyleEorForXorY(entryHeader) && entryHeader.CH_Status == NLConstants.StatusNew.Accepted && entryHeader.CH_EntryStatus == NLConstants.EntryStatusNew.ProvisionalRelease)
		{
			SetEntryStatuses(entryHeader, dataProvider, NLConstants.StatusNew.Accepted);
		}
		else
		{
			SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Reminder);
		}

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, ExpiryDate);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(NLEDIMessageInterpreterHelper.ResStrings.CustomsReminder,
			ExpiryDate,
			NLConstants.StatementTypes.Customs,
			Res.GetString("DDBAF627-35DF-4501-823F-771E5DEF54C4", "Declaration awaiting Exit confirmation"),
			Res.GetString("E1A49F3B-0FF3-4A54-90FA-05BDA654E780", "If no Exit information has been received before Expiry the declaration may be canceled by Customs"),
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);

	DateTime ExpiryDate => CachedValueHelper.GetValue(ref expiryDateCached, () => ZDateTime.UtcToday.AddDays(150).ToDateTime());
	CachedValue<DateTime> expiryDateCached;
}
