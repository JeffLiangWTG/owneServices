using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineApportionChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceLineApportionChargeLookups(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList
		{
			get { return chargeTypeList ?? (chargeTypeList = new ChargeTypeList()); }
		}
		ChargeTypeList chargeTypeList;
	}
}
