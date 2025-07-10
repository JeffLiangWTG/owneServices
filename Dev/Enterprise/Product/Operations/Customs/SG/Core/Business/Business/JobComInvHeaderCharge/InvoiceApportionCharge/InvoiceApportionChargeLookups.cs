using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceApportionChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceApportionChargeLookups(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent
		{
			get { return (InvoiceApportionCharge)base.Parent; }
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get { return chargeTypeList ?? (chargeTypeList = new ChargeTypeList()); }
		}
		ChargeTypeList chargeTypeList;
	}
}
