using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccDraftInvoiceExRate : AutoAccDraftInvoiceExRate
	{
		public AccDraftInvoiceExRate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AIE_AIH_Header), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AIE_ExchangeRate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AIE_IsReciprocal), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AIE_RX_NKRateCurrency), ConcurrencyPolicy.Strict);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AIE_ExchangeRate = (decimal)1;
			AIE_IsReciprocal = false;
		}
	}
}
