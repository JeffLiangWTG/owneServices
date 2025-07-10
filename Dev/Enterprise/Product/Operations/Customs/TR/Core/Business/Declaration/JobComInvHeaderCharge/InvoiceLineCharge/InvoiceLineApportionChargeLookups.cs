using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceLineApportionChargeLookups : EU.Business.Declaration.InvoiceLineApportionChargeLookups
	{
		public InvoiceLineApportionChargeLookups(InvoiceLineApportionCharge invoiceLineApportionCharge) : base(invoiceLineApportionCharge)
		{
		}

		public new InvoiceLineApportionCharge Parent => (InvoiceLineApportionCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<TRIncotermChargeCodeList>();
	}
}
