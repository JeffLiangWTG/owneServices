using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	static class RECR20Populator
	{
		public static RECR20 Populate(IReconciliationImportEntry importEntry, int entryTrailerNumber, bool isAggregate)
		{
			RECR20 r20 = new RECR20();
			r20.TrailerNumber = entryTrailerNumber;
			r20.ImportEntry = importEntry.ImportEntryFilerCodeNumber;
			r20.EntryPort = importEntry.Port;
			r20.OriginalDuty = isAggregate ? ZDecimal.Zero : importEntry.OriginalDuty;
			r20.EstimatedReconciliationDuty = isAggregate ? ZDecimal.Zero : importEntry.EstimatedReconciliationDuty;
			r20.OriginalTax = isAggregate ? ZDecimal.Zero : importEntry.OriginalTax;
			r20.EstimatedReconciliationTax = isAggregate ? ZDecimal.Zero : importEntry.EstimatedReconciliationTax;
			r20.EstimatedReconciliationInterest = isAggregate ? ZDecimal.Zero : importEntry.EstimatedReconciliationInterest;
			return r20;
		}
	}
}
