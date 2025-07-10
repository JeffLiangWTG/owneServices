using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	[TestDate(2022, 07, 01)]
	public void TestCountryOfDestinations()
	{
		var list = (ZZRefCusCodeListCombinedCollection)lookups.CountriesOfDestination;
		list.Load();
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("Country of Destination", new[] { "GI" }, list.Select(x => x.ZZD_Code));
			AssertSame("Cached", list, lookups.CountriesOfDestination);
		});
	}

	public void TestCountryOfSupplyList()
	{
		var refCusTradeGroup1 = Factory.NewWithValidTestData<RefCusTradeGroup>();
		refCusTradeGroup1.ZZA_TradeGroup = "TG1";
		refCusTradeGroup1.ZZA_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		var refCusTradeGroup2 = Factory.NewWithValidTestData<RefCusTradeGroup>();
		refCusTradeGroup2.ZZA_TradeGroup = "TG3";
		refCusTradeGroup2.ZZA_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		Factory.Save();

		AssertContainsExactElementsInAnyOrder(new[] { refCusTradeGroup1, refCusTradeGroup2 }, lookups.CountryOfSupplyList);
	}

	public void TestCountryOfSupplyList_FilterDefaults()
	{
		var filterProvider = (IFilterBusinessObjectDefaultsProvider)lookups.CountryOfSupplyList;
		var filterDefaults = filterProvider.FilterBusinessObjectDefaults;

		CombineAssertions(() =>
		{
			Assert($"Contains default for {RefCusTradeGroupCollection.FilterName.EconomicGroup}:Property", filterDefaults.ContainsDefaultFor($"{RefCusTradeGroupCollection.FilterName.EconomicGroup}:Property"));
			Assert($"Contains default for {RefCusTradeGroupCollection.FilterName.EconomicGroup}:ComparisonOperator", filterDefaults.ContainsDefaultFor($"{RefCusTradeGroupCollection.FilterName.EconomicGroup}:ComparisonOperator"));

			AssertEquals($"{RefCusTradeGroupCollection.FilterName.EconomicGroup}:Property value", (ZString)Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, filterDefaults[$"{RefCusTradeGroupCollection.FilterName.EconomicGroup}:Property"].Value);
			AssertEquals($"{RefCusTradeGroupCollection.FilterName.EconomicGroup}:ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.Exact, filterDefaults[$"{RefCusTradeGroupCollection.FilterName.EconomicGroup}:ComparisonOperator"].Value);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		var polandCountryCode = Core.Constants.CountryCodes.Poland;
		var grouping = helper.CreateNewOrGetExistingDataGrouping(polandCountryCode);
		helper.CreateNewOrGetExistingDataGrouping(polandCountryCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.IMP34, "Country of Origin (Import)");
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.EXP34, "Country of Origin (Export)");
		helper.CreateCusCodeList(polandCountryCode, UniversalReferenceConstants.RefCusCodeListType.Codes.CustomsProcedures, "0018137-1", "DESC1", new ZDateTime(2021, 01, 01), new ZDateTime(2021, 12, 31));
		helper.CreateCusCodeList(polandCountryCode, UniversalReferenceConstants.RefCusCodeListType.Codes.IMP34, "AL", "Albania", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		helper.CreateCusCodeList(polandCountryCode, UniversalReferenceConstants.RefCusCodeListType.Codes.IMP34, "DK", "Denmark", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 05, 31));
		helper.CreateCusCodeList(polandCountryCode, UniversalReferenceConstants.RefCusCodeListType.Codes.EXP34, "GI", "Gibraltar", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
		helper.CreateCusCodeList(polandCountryCode, UniversalReferenceConstants.RefCusCodeListType.Codes.EXP34, "NL", "Netherlands", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 05, 31));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		lookups = declaration.Invoices.AddNew().InvoiceLines.AddNew().AddInfoLookups;
	}
	AddInfoJobComInvoiceLineLookups lookups;
}
