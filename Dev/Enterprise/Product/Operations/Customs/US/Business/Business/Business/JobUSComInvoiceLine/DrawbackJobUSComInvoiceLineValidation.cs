using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackJobUSComInvoiceLineValidation : JobUSComInvoiceLineValidation
	{
		public DrawbackJobUSComInvoiceLineValidation(AutoJobUSComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckUSI_DRW99ClaimedDuty()
		{
			base.CheckUSI_DRW99ClaimedDuty();
			var drw99ClaimedAmount = Parent._99ClaimedDuty;
			if (drw99ClaimedAmount > ZDecimal.Zero && drw99ClaimedAmount > Parent.CalculatedDuty)
			{
				Parent.USI_DRW99ClaimedDutyInfo.AddMessageError(_99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			}
		}

		protected override void CheckUSI_DRW99ClaimedHMF()
		{
			base.CheckUSI_DRW99ClaimedHMF();
			var drw99ClaimedAmount = Parent._99ClaimedHMF;
			if (drw99ClaimedAmount > ZDecimal.Zero && drw99ClaimedAmount > Parent.CalculatedHMF)
			{
				Parent.USI_DRW99ClaimedHMFInfo.AddMessageError(_99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			}
		}

		protected override void CheckUSI_DRW99ClaimedMPF()
		{
			base.CheckUSI_DRW99ClaimedMPF();
			var drw99ClaimedAmount = Parent._99ClaimedMPF;
			if (drw99ClaimedAmount > ZDecimal.Zero && drw99ClaimedAmount > Parent.CalculatedMPF)
			{
					Parent.USI_DRW99ClaimedMPFInfo.AddMessageError(_99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			}
		}

		protected override void CheckUSI_DRW99ClaimedTax()
		{
			base.CheckUSI_DRW99ClaimedTax();
			var drw99ClaimedAmount = Parent._99ClaimedTax;
			if (drw99ClaimedAmount > ZDecimal.Zero && drw99ClaimedAmount > Parent.CalculatedTax)
			{
				Parent.USI_DRW99ClaimedTaxInfo.AddMessageError(_99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			}
		}

		public const string _99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount = "The Claimed Amount 99% should not be greater than Calculated Amount. ";
	}
}
