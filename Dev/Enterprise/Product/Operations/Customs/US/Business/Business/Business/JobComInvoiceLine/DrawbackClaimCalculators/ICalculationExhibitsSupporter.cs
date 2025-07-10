using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface ICalculationExhibitsSupporter
	{
		ZString ImportEntryOrCMDNo { get; }
		ZString InvoiceNo { get; }
		ZString PartNo { get; }
		ZDecimal ImportQuantity { get; }
		ZDecimal ExportQuantity { get; }
		ZString Description { get; }
		ZDecimal PerUnit { get; }
		ZDecimal AmountPaid { get; }
		ZDecimal AmountClaimed { get; }
		ZDecimal WeightedRatio { get; }
		ZDecimal LineAmount { get; }
		ZDecimal LineAmountEligible { get; }
		ZDecimal CalculatedAmountForQtyUsed { get; }
		ZDecimal TotalLineValue { get; }
		ZDecimal IndividualValue { get; }
		ZString LineDutyRateDesc { get; }
	}
}
