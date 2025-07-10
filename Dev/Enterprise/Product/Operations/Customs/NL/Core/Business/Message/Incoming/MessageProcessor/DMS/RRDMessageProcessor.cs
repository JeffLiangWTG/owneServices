using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.Business;

public class RRDMessageProcessor : DMSResponseMessageProcessor
{
	public RRDMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (entryHeader.CH_PhaseStatus != CustomsEntryPhaseStatusList.Codes._513 
			&& entryHeader.CH_PhaseStatus != CustomsEntryPhaseStatusList.Codes.CRE 
			&& entryHeader.CH_PhaseStatus != CustomsEntryPhaseStatusList.Codes.SUP)
		{
			SetEntryStatuses(entryHeader, NLConstants.EntryStatusNew.RequestForInformation, NLConstants.StatusNew.ReminderReceived);
		}

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.Declaration?.ExpirationDate);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider)
		=> NLEDIMessageInterpreterHelper.FormatMessageContent(NLEDIMessageInterpreterHelper.ResStrings.CustomsReminder,
			dataProvider.Declaration?.ExpirationDate,
			NLConstants.StatementTypes.Customs,
			Res.GetString("4879298C-8DFD-4A48-AC7F-AC3B5E994A05", "RFI awaiting CRE reply to Customs"),
			Res.GetString("DACDEED5-52A9-4890-BA52-58F10BBECA4A", "If no CRE is sent before Expiry the declaration may be canceled by Customs"),
			dataProvider.Statuses.FirstOrDefault()?.EffectiveDateTime);
}
