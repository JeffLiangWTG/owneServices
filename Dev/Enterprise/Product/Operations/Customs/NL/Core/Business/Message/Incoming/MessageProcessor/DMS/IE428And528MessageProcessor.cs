using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE428And528MessageProcessor : DMSResponseMessageProcessor
{
	public IE428And528MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (dataProvider.WCOTypeCode.EqualIgnoringOrder(WCoTypeCodes.ExportAcceptance))
		{
			if (entryHeader.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes._511)
			{
				SetEntryStatuses(entryHeader, EntryStatusNew.MRNAllocated, StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
			}
			else
			{
				SetEntryStatuses(entryHeader, dataProvider, StatusNew.Accepted);
			}
		}
		else
		{
			SetEntryStatuses(entryHeader, dataProvider, Status.MRN);
		}

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberIssueDateAndNum(mrnEntryNumber, dataProvider);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(Description,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Description,
			Description,
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);

	string Description => Res.GetString("47853803-616A-4348-9292-1004486105DD", "Acceptance");
}
