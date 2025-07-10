using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ReconInterestDataProviderReconOriginalEntry : IReconInterestDataProvider
	{
		public ReconInterestDataProviderReconOriginalEntry(ReconOriginalEntryHeader reconOriginalEntry)
		{
			this.reconOriginalEntry = reconOriginalEntry;
		}

		readonly ReconOriginalEntryHeader reconOriginalEntry;

		#region IReconInterestDataProvider Members

		public ZBool HasBeenUnderPaid
		{
			get
			{
				return OriginalPayable - ReconPayable < 0;
			}
		}

		public ZDate OriginalPaymentDate
		{
			get { return reconOriginalEntry.US_PaymentDate.Date; }
		}

		public ZDate ReconPaymentDate
		{
			get { return reconOriginalEntry.ReconDeclaration.ReconPaymentDate; }
		}

		public ZDecimal OriginalPayable
		{
			get { return reconOriginalEntry.OriginalCharges.TotalOriginalAmount; }
		}

		public ZDecimal ReconPayable
		{
			get { return reconOriginalEntry.ReconCharges.TotalAmount; }
		}

		public void UpdateOrAddInterestCharge(ZDecimal amount)
		{
			reconOriginalEntry.ReconCharges.UpdateOrAddCharge(Core.Constants.USCustoms.FeeCodes.ReconciliationInterest, amount);
		}

		#endregion
	}
}
