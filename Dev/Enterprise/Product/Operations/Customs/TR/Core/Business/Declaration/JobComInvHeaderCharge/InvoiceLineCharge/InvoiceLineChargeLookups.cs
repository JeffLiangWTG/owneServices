using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceLineChargeLookups : EU.Business.Declaration.InvoiceLineChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge parent) : base(parent)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<TRIncotermChargeCodeList>();
	}
}
