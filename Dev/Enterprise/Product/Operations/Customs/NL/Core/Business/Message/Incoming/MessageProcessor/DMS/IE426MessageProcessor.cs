using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class IE426MessageProcessor : DMSResponseMessageProcessor
{
	public IE426MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Received);

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.AdditionalInformations.FirstOrDefault()?.LimitDate);
		SetEntryNumberIssueDateAndNum(mrnEntryNumber, dataProvider);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(Description,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Description,
			Description,
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);

	string Description => Res.GetString("0CC8147D-D201-4A1C-B7C9-920EF31FD3DC", "Declaration Acceptance");
}
