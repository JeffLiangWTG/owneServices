using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceLineCharge : BaseInvoiceLineCharge, Integration.Customs.ZA.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool GetJ7_RX_NKCurrency_ReadOnly() => true;

		protected override bool GetJ7_IsDutiable_ReadOnly() => true;

		protected override bool GetJ7_IsGSTApplicable_ReadOnly() => true;

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly() => true;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		public override bool ShouldSetCurrencyFromParent => true;
	}
}
