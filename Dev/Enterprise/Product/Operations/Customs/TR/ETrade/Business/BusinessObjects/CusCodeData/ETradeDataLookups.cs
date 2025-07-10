using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class ETradeDataLookups : CusCodeDataLookups
	{
		public ETradeDataLookups(ETradeData parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetCachedValue<CusCodeDataTypeList>();
	}
}
