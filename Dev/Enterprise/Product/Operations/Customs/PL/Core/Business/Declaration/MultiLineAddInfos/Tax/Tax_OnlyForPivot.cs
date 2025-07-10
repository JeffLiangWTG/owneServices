using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBTax)]
public class Tax_OnlyForPivot : Tax_CusAddInfoOnlyForPIVOT
{
	public Tax_OnlyForPivot(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	public new CusClassPartPivot Pivot => (CusClassPartPivot)base.Pivot;

	protected override EUAddInfoTaxLookups GetNewLookups() => new PLAddInfoTaxLookups(this);

	public new PLAddInfoTaxLookups Lookups => (PLAddInfoTaxLookups)base.Lookups;
}
