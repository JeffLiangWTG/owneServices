using CargoWise.Integration;

namespace Enterprise.Customs.Universal
{
	public class CusRefRateCodeLookups : AutoCusRefRateCodeLookups
	{
		public CusRefRateCodeLookups(AutoCusRefRateCode parent) : base(parent) { }

		public ICodeDescriptionPairList RateTypeList => Factory.GetCachedValue<RefCusRateTypeCustomizableList>();
	}
}
