using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceChargeLookups : EU.Business.Declaration.InvoiceChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<TRIncotermChargeCodeList>();
	}
}
