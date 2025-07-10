using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class IE404And504MessageProcessor : DMSResponseMessageProcessor
{
	public IE404And504MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (dataProvider.WCOTypeCode.EqualIgnoringOrder(WCoTypeCodes.ExportAmendmentAccepted))
		{
			switch (entryHeader.CH_PhaseStatus)
			{
				case CustomsEntryPhaseStatusList.Codes._513 when entryHeader.CH_Status == StatusNew.Accepted && entryHeader.CH_EntryStatus == EntryStatusNew.Received:
					SetEntryStatuses(entryHeader, phaseStatus: CustomsEntryPhaseStatusList.Codes._515);
					break;
				default:
					SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Admentment);
					break;
			}
		}
		else
		{
			SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Admentment);
		}
	}

	protected override ZString InterpretMessage(EDIMessage message, IDMSIncomingDataProvider dataProvider) => new IE404And504MessageInterpreter().Interpret(dataProvider, message);
}
