using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE509MessageProcessor : DMSResponseMessageProcessor
{
	public IE509MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		switch (entryHeader.CH_PhaseStatus)
		{
			case CustomsEntryPhaseStatusList.Codes._514:
			case CustomsEntryPhaseStatusList.Codes._515:
			case CustomsEntryPhaseStatusList.Codes._583:
				SetEntryStatuses(entryHeader, EntryStatusNew.Cancelled, StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
				break;
			default:
				SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Cancelled);
				break;
		}
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new IE509MessageInterpreter().Interpret(dataProvider, message);
}
