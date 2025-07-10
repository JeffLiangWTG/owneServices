using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class VoucherOfCorrectionValueLookups : CusCodeDataLookups
	{
		public VoucherOfCorrectionValueLookups(AutoCusCodeData parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return Factory.GetCachedValue<VOCValueTypeList>(); }
		}
	}
}
