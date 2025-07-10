using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class CusClassPartPivotLookups(CusClassPartPivot parent) : Customs.Business.CusClassPartPivotLookups(parent)
{
	public new CusClassPartPivot Parent => (CusClassPartPivot)base.Parent;

	public CodeDescriptionPairList VATCodeList => Factory.GetCachedValue<CodeDescriptionPairList>();

	public CodeDescriptionPairList ReducedCustomsFlagList => Factory.GetCachedValue<CodeDescriptionPairList>();

	public CodeDescriptionPairList CountyOfOriginList => Factory.GetCachedValue<CodeDescriptionPairList>();
}
