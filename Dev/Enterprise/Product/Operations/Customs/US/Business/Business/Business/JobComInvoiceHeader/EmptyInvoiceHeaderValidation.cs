using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class EmptyInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public EmptyInvoiceHeaderValidation(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckJZ_JE()
		{
			//
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
			//
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			//
		}

		protected override void CheckJZ_OH_Buyer()
		{
			//
		}

		protected override void CheckJZ_GB()
		{
			//
		}

		protected override void CheckJZ_OH_Supplier()
		{
			//
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			//
		}

		protected override void CheckJZ_InvoiceCurrExRate()
		{
			//
		}

		protected override void CheckInvoice(ZPropertyInfo amountInfo, ZPropertyInfo currencyInfo)
		{
			//
		}

		protected override void CheckJZ_IncoTerm()
		{
			//
		}

		protected override void CheckJZ_MessageType()
		{
			//
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			//
		}

		protected override void CheckJZ_Calc_FOBAmount()
		{
			//
		}

		protected override void CheckJZ_Calc_CIFAmount()
		{
			//
		}

		protected override void CheckJZ_Calc_TNI()
		{
			//
		}

		protected override void CheckJZ_WeightUQ()
		{
			//
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			//
		}

		protected override void CheckJZ_InvoiceDate()
		{
			//
		}
	}
}
