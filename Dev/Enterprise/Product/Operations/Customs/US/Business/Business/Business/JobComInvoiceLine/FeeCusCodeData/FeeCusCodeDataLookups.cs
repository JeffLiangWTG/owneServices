//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class FeeCusCodeDataLookups : Customs.Business.CusCodeDataLookups
	{
		public FeeCusCodeDataLookups(FeeCusCodeData parent)
			: base(parent)
		{
		}

		public RateTypeList CY_SelectedRateTypeList => Factory.GetCachedValue<RateTypeList>();

		public override CodeDescriptionPairList CY_CodeList => CusFeeCodeConstants.GetAccountingClassFeeCodeList(Factory);
	}
}
