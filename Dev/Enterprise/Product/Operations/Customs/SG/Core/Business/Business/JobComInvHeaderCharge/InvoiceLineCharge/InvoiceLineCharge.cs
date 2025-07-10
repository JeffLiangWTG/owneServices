using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineCharge : TypeSafeInvoiceLineCharge, Integration.Customs.SG.IInvoiceLineCharge
	{
		public static class ChargeTypes
		{
			public const string OptionalItemCharges = "OPT";
		}

		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J7_IsDutiable = true;
			J7_IsGSTApplicable = true;
		}
	}
}
