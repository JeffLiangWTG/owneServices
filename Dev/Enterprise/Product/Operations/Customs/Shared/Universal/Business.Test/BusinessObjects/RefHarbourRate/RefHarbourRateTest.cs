using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing;

[TestedType(typeof(RefHarbourRate))]
sealed class RefHarbourRateTest : EnterpriseBusinessObjectTestCase
{
	public void TestCaptions()
	{
		var harborRate = (RefHarbourRate)GetNewBusinessObject();
		harborRate.ZXF_Port = "121";

		CombineAssertions("HumanReadableName and captions for properties of RefHarbourRate.", () =>
		{
			AssertEquals("HumanReadableName:", "Global Harbor Rate - Port: 121", harborRate.HumanReadableName);
			AssertEquals("ZXF_ZZZ_NKDataGrouping:", "Country/Region or Grouping", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_ZZZ_NKDataGrouping)).Caption);
			AssertEquals("ZXF_Type:", "Type", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_Type)).Caption);
			AssertEquals("ZXF_Port:", "Port", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_Port)).Caption);
			AssertEquals("ZXF_Commodity:", "Commodity", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_Commodity)).Caption);
			AssertEquals("ZXF_PortTaxType:", "Port Tax Type", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_PortTaxType)).Caption);
			AssertEquals("ZXF_RateFormula:", "Rate Formula", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_RateFormula)).Caption);
			AssertEquals("ZXF_StartDate:", "Start Date", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_StartDate)).Caption);
			AssertEquals("ZXF_EndDate:", "End Date", DataBoundResourceStrings.GetDataForProperty(typeof(RefHarbourRate), nameof(RefHarbourRate.ZXF_EndDate)).Caption);
		});

		CombineAssertions("ReadOnly properties.", () =>
		{
			AssertEquals("ZXF_PortTaxType should be ReadOnly.", true, harborRate.ZXF_PortTaxTypeInfo.ReadOnly);
			AssertEquals("ZXF_Commodity should be ReadOnly.", true, harborRate.ZXF_CommodityInfo.ReadOnly);
			AssertEquals("ZXF_CountryOrGrouping should be ReadOnly.", true, harborRate.ZXF_ZZZ_NKDataGroupingInfo.ReadOnly);
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France);
		var harborRate = (RefHarbourRate)base.GetNewBusinessObjectForDeleteTest(factory);
		harborRate.ZXF_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.France;
		harborRate.ZXF_Mode = "CON";
		return harborRate;
	}
}
