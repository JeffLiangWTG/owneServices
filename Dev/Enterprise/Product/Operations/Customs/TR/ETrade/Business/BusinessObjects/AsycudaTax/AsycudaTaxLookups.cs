using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaTaxLookups : ASYCUDA.Business.AsycudaTaxLookups
	{
		public AsycudaTaxLookups(AsycudaTax parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList ChargeTypeList => Factory.GetCachedValue<TaxCodeList>();
		public override CodeDescriptionPairList MethodOfPaymentList => Factory.GetCachedValue<MethodOfPaymentList>();
	}
}

