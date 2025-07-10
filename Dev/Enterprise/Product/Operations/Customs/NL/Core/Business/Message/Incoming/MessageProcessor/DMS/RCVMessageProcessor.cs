using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class RCVMessageProcessor : DMSResponseMessageProcessor
{
	public RCVMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		switch (entryHeader.CH_PhaseStatus)
		{
			case CustomsEntryPhaseStatusList.Codes._513:
			case CustomsEntryPhaseStatusList.Codes._514:
				SetEntryStatuses(entryHeader, EntryStatusNew.Received, StatusNew.Accepted);
				break;
			case CustomsEntryPhaseStatusList.Codes._583:
			case CustomsEntryPhaseStatusList.Codes.CRE when entryHeader.CH_Status == NLConstants.StatusNew.SentToCustoms:
			case CustomsEntryPhaseStatusList.Codes.SUP:
				SetEntryStatuses(entryHeader, EntryStatusNew.Received, StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
				break;
			default:
				SetEntryStatuses(entryHeader, dataProvider, Status.Received);
				break;
		}
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(Description,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Description,
			Description,
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);

	string Description => Res.GetString("D2669918-B195-42CD-8084-AC32865372A2", "Receive Message");
}
