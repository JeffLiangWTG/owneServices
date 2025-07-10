using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

public class PLAddInfoTaxLookups : EUAddInfoTaxLookups
{
	public PLAddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
	{
	}

	public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

	protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new PLTaxLookupsCommon(Parent);
}
