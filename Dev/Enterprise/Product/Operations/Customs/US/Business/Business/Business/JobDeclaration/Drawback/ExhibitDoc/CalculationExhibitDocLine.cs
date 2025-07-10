using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CalculationExhibitDocLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CalculationExhibitDocLine(ICalculationExhibitsSupporter supporter)
		{
			this.ImportEntryOrCMDNo = supporter.ImportEntryOrCMDNo;
			this.InvoiceNo = supporter.InvoiceNo;
			this.PartNo = supporter.PartNo;
			this.ImportQuantity = supporter.ImportQuantity.Round(2);
			this.ExportQuantity = supporter.ExportQuantity.Round(2);
			this.Description = supporter.Description;
			this.PerUnit = supporter.PerUnit;
			this.AmountPaid = supporter.AmountPaid;
			this.AmountClaimed = supporter.AmountClaimed;
			this.WeightedRatio = supporter.WeightedRatio;
			this.LineAmount = supporter.LineAmount;
			this.LineAmountEligible = supporter.LineAmountEligible;
			this.CalculatedAmountForQtyUsed = supporter.CalculatedAmountForQtyUsed.Round(2);
			this.TotalLineValue = supporter.TotalLineValue;
			this.IndividualValue = supporter.IndividualValue;
			this.LineDutyRateDesc = supporter.LineDutyRateDesc;
		}

		public ZString ImportEntryOrCMDNo { get; private set; }
		public ZString InvoiceNo { get; private set; }
		public ZString PartNo { get; private set; }
		public ZDecimal ImportQuantity { get; private set; }
		public ZDecimal ExportQuantity { get; private set; }
		public ZString Description { get; private set; }
		public ZDecimal PerUnit { get; private set; }
		public ZDecimal AmountPaid { get; private set; }
		public ZDecimal AmountClaimed { get; private set; }
		public ZDecimal WeightedRatio { get; private set; }
		public ZDecimal LineAmount { get; private set; }
		public ZDecimal LineAmountEligible { get; private set; }
		public ZDecimal CalculatedAmountForQtyUsed { get; private set; }
		public ZDecimal TotalLineValue { get; private set; }
		public ZDecimal IndividualValue { get; private set; }
		public ZString LineDutyRateDesc { get; private set; }
	}
}
