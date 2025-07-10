using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IReconciliationImportEntryFee
	{
		ZString FeeClass { get; }
		ZDecimal OriginalFee { get; }
		ZDecimal EstimatedReconciliationFee { get; }
	}
}
