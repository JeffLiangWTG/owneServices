using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusGoodsLocationLookups : AutoCusGoodsLocationLookups
	{
		public CusGoodsLocationLookups(AutoCusGoodsLocation parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList QualifierList => Factory.GetCachedValue<CusGoodsLocationQualifierList>();

		public virtual CodeDescriptionPairList TypeList => Factory.GetCachedValue<CusGoodsLocationTypeList>();

		public virtual CodeDescriptionPairList LocationUseList => Factory.GetCachedValue<CusGoodsLocationUseList>();
	}
}
