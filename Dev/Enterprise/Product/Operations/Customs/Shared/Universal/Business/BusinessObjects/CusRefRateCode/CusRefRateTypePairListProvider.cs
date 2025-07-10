using CargoWise.Integration;

namespace Enterprise.Customs.Universal
{
	public class CusRefRateTypePairListProvider : Integration.Customs.ICusRefRateTypePairListProvider
	{
		public ICodeDescriptionPairList GetCusRefRateTypeList() => new RefCusRateTypeCustomizableList();
	}
}
