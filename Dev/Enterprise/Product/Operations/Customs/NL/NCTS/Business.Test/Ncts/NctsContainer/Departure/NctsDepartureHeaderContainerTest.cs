using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureHeaderContainer))]
sealed class NctsDepartureHeaderContainerTest : BaseCusInBondContainerTest<NctsDepartureHeaderContainer>
{
	protected override BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return header.DepartureHeaderContainers.AddNew();
	}

	public void TestNotifyChangeOfContainerMode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands);
		var c114 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C114, 0.65, Core.Constants.CountryCodes.Netherlands);
		var c180 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C180, 2.8, Core.Constants.CountryCodes.Netherlands);
		var c100 = helper.CreateTaxOrFee(NLNctsConstants.TaxOrFeeCodes.C100, 0.4, Core.Constants.CountryCodes.Netherlands);

		var container = GetNewBusinessObject() as NctsDepartureHeaderContainer;
		container.Header.CALCalculationMethod = CalculationMethodList.Codes.WGT;
		container.Header.MovementHeader.BM_ExportTransportMode = "1";
		var bill = container.Header.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		goodsItem.BY_NetWeight = 100;
		goodsItem.BY_FormattedHarmonisedTariff = "0101.0102.0000";

		CombineAssertions(() =>
		{
			AssertEquals("Initial no duty or taxes are calculated.", 0, goodsItem.Fees.Count);
			container.BC_Mode = "CNT";
			AssertEquals("Duties are calculated after change of ContainerMode", 1, goodsItem.Fees.Count);
		});
	}
}
