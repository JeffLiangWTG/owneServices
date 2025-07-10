using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class IE531MessageProcessor : IE431And531MessageProcessor
{
	public IE531MessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void SetEntryInfos(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		if (entryHeader.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes._513 || entryHeader.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes.SUP)
		{
			SetEntryNumberExpiryDate(entryHeader, dataProvider);
		}
		else
		{
			SetEntryStatuses(entryHeader, dataProvider, NLConstants.Status.Reminder, CustomsEntryPhaseStatusList.Codes._515);

			SetEntryNumberExpiryDate(entryHeader, dataProvider);
		}
	}
}
