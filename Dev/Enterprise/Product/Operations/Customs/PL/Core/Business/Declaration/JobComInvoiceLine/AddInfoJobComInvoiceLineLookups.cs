using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
{
	public AddInfoJobComInvoiceLineLookups(EU.Business.Declaration.AddInfoJobComInvoiceLine parent) : base(parent)
	{
	}

	public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	public CodeDescriptionPairList FuelTypeList => Factory.GetCachedValue<FuelTypeList>();

	protected override ICollection GetCountriesOfDestinationCore() => CountryOfOriginListForCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.EXP34);

	protected override ICollection CountryOfSupplyListCore()
	{
		var result = new RefCusTradeGroupCollection(Factory);
		result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefCusTradeGroupCollection.FilterName.EconomicGroup, "Property", (ZString)Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, false));
		result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(RefCusTradeGroupCollection.FilterName.EconomicGroup, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, false));
		return result;
	}

	ZZRefCusCodeListCombinedCollection CountryOfOriginListForCodeType(string codeType)
	{
		var invoiceLine = Parent.Parent;
		return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, invoiceLine.GetDefaultDataGroupingCode(), codeType, ZDateTime.Today);
	}
}
