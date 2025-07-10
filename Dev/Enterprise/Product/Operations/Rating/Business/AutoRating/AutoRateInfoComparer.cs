using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	using System.Collections.Generic;

	public class AutoRateInfoComparer : IComparer<AutoRateInfo>
	{
		public int Compare(AutoRateInfo autoRateInfo1, AutoRateInfo autoRateInfo2)
		{
#pragma warning disable 0618
			if (autoRateInfo1.Entry == null || autoRateInfo2.Entry == null)
			{
				return 0;
			}
#pragma warning restore 0618

			if (autoRateInfo1.IsInclusiveCalculator != autoRateInfo2.IsInclusiveCalculator)
			{
				return autoRateInfo1.IsInclusiveCalculator.CompareTo(autoRateInfo2.IsInclusiveCalculator);
			}

			if (autoRateInfo1.IsCost != autoRateInfo2.IsCost)
			{
				return autoRateInfo1.IsCost.CompareTo(autoRateInfo2.IsCost);
			}

			if (autoRateInfo1.Bases.Calculate().minimum != autoRateInfo2.Bases.Calculate().minimum)
			{
				return autoRateInfo2.Bases.Calculate().minimum.CompareTo(autoRateInfo1.Bases.Calculate().minimum);
			}

			if (autoRateInfo1.Bases.Calculate().maximum != autoRateInfo2.Bases.Calculate().maximum)
			{
				return autoRateInfo1.Bases.Calculate().maximum.CompareTo(autoRateInfo2.Bases.Calculate().maximum);
			}

			if (autoRateInfo1.InvoiceLineDescription != autoRateInfo2.InvoiceLineDescription)
			{
				return autoRateInfo1.InvoiceLineDescription.CompareTo(autoRateInfo2.InvoiceLineDescription);
			}

			if (autoRateInfo1.CalculationDescription != autoRateInfo2.CalculationDescription)
			{
				return autoRateInfo1.CalculationDescription.CompareTo(autoRateInfo2.CalculationDescription);
			}

			if (autoRateInfo1.Currency != autoRateInfo2.Currency)
			{
				return autoRateInfo1.Currency.CompareTo(autoRateInfo2.Currency);
			}

			return autoRateInfo1.Amount.CompareTo(autoRateInfo2.Amount);
		}
	}
}

