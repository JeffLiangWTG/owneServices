using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusFiscalReferenceLookups : EU.Business.Declaration.CusFiscalReferenceLookups
{
	public CusFiscalReferenceLookups(EU.Business.Declaration.CusFiscalReference parent) : base(parent)
	{
	}

	new CusFiscalReference Parent => (CusFiscalReference)base.Parent;

	public override CodeDescriptionPairList CodeList => Parent.Declaration?.IsImport ?? false
		? Factory.GetCachedValue<FiscalReferenceCodeList>()
		: base.CodeList;
}
