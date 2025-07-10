using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class ReconEntryFeeIReconciliationImportEntryFee : IReconciliationImportEntryFee
	{
		public ReconEntryFeeIReconciliationImportEntryFee()
		{
		}

		public ZDecimal OriginalFee;
		public ZDecimal ReconFee;
		public ZString FeeType;

		#region IReconciliationImportEntryFee Members

		ZString IReconciliationImportEntryFee.FeeClass
		{
			get { return FeeType; }
		}

		ZDecimal IReconciliationImportEntryFee.OriginalFee
		{
			get { return OriginalFee; }
		}

		ZDecimal IReconciliationImportEntryFee.EstimatedReconciliationFee
		{
			get { return ReconFee; }
		}

		#endregion
	}
}
