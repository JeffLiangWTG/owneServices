using System.Linq;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE551MessageProcessor : DMSResponseMessageProcessor
{
	public IE551MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		var jobDeclaration = entryHeader.Declaration;
		switch (entryHeader.CH_PhaseStatus)
		{
			case CustomsEntryPhaseStatusList.Codes._513:
			case CustomsEntryPhaseStatusList.Codes._514:
			case CustomsEntryPhaseStatusList.Codes._515:
			case CustomsEntryPhaseStatusList.Codes.CRE:
			case CustomsEntryPhaseStatusList.Codes.SUP:
				SetEntryStatuses(entryHeader, EntryStatusNew.NoRelease, StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
				break;
			default:
				SetEntryStatuses(entryHeader, dataProvider, Status.Cancelled);
				break;
		}

		var mrnEntryNumber = GetMRNCusEntryNumber(entryHeader);
		SetEntryNumberExpiryDate(mrnEntryNumber, dataProvider.AdditionalInformations.FirstOrDefault()?.LimitDate);
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new IE551MessageInterpreter().Interpret(dataProvider, message);
}
