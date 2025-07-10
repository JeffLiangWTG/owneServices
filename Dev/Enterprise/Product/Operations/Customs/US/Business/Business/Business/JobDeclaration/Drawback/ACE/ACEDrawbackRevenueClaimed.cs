using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEDrawbackRevenueClaimed : IACEDrawbackRevenueClaimed
	{
		public ACEDrawbackRevenueClaimed(ZString accountingClassCode, ZDecimal amount, ZDecimal calculatedAmount, ZDecimal adjustedClaimAmount, ZString qualifierIndicator)
		{
			this.accountingClassCode = accountingClassCode;
			this.claimAmount = amount;
			this.calculatedAmount = calculatedAmount;
			this.adjustedClaimAmount = adjustedClaimAmount;
			this.qualifierIndicator = qualifierIndicator;
		}
		readonly ZString accountingClassCode;
		readonly ZDecimal claimAmount;
		readonly ZDecimal calculatedAmount;
		readonly ZDecimal adjustedClaimAmount;
		readonly ZString qualifierIndicator;

		#region IACEDrawbackRevenueClaimed

		ZString IACEDrawbackRevenueClaimed.AccountingClassCode
		{
			get { return accountingClassCode; }
		}

		ZDecimal IACEDrawbackRevenueClaimed.ClaimAmount
		{
			get { return claimAmount; }
		}

		ZDecimal IACEDrawbackRevenueClaimed.CalculatedAmount
		{
			get { return calculatedAmount; }
		}

		ZDecimal IACEDrawbackRevenueClaimed.AdjustedClaimAmount
		{
			get { return adjustedClaimAmount; }
		}

		ZString IACEDrawbackRevenueClaimed.QualifierIndicator
		{
			get { return qualifierIndicator; }
		}

		#endregion
	}
}
