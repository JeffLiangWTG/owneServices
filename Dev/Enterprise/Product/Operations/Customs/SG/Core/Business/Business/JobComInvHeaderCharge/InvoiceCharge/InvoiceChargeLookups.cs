using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get { return chargeTypeList ?? (chargeTypeList = new ChargeTypeList()); }
		}
		ChargeTypeList chargeTypeList;
	}
}
