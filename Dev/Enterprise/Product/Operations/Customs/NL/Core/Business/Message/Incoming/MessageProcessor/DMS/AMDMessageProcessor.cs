using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Declaration;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class AMDMessageProcessor : DMSResponseMessageProcessor
{
	public AMDMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		SetEntryStatuses(entryHeader, EntryStatusNew.Amended, StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._515);
	}
}
