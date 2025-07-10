using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class HarmonisedCodeHelperTest : TestCaseWithFactory
	{
		public void TestGetOuterPackLineHarmonisedCodes_HarmonisedCodes()
		{
			LoadOrCreateNewTariff(Factory, "AU", "11111", Constants.TariffTypes.HarmonizedSystem);
			LoadOrCreateNewTariff(Factory, "AU", "1234567890", Constants.TariffTypes.HarmonizedSystem);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			var auHSCode1 = packLine.HarmonisedCodes.AddNew();
			auHSCode1.JLH_RN_NKCountry = "AU";
			auHSCode1.JLH_Code = "11111";
			var auHSCode2 = packLine.HarmonisedCodes.AddNew();
			auHSCode2.JLH_RN_NKCountry = "AU";
			auHSCode2.JLH_Code = "1234";
			var auHSCode3 = packLine.HarmonisedCodes.AddNew();
			auHSCode3.JLH_RN_NKCountry = "AU";
			auHSCode3.JLH_Code = "1234567890";

			var harmonisedCodes = HarmonisedCodeHelper.GetOuterPackLineHarmonisedCodes(shipment, "AU");
			AssertContainsExactElementsInExactOrder(new[]
			{
				"11111", "1234", "1234567890"
			}, harmonisedCodes.ToArray());
		}

		public void TestGetOuterPackLineHarmonisedCodes_JL_HarmonisedCode()
		{
			LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "1111", Constants.TariffTypes.HarmonizedSystem);
			LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "222210", Constants.TariffTypes.HarmonizedSystem);
			LoadOrCreateNewTariff(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, "222290", Constants.TariffTypes.HarmonizedSystem);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_HarmonisedCode = "1111";
			var harmonisedCodes = HarmonisedCodeHelper.GetOuterPackLineHarmonisedCodes(shipment, "AU");
			AssertContainsExactElementsInExactOrder(new[] { "111100" }, harmonisedCodes.ToArray());
			packLine.JL_HarmonisedCode = "2222";
			harmonisedCodes = HarmonisedCodeHelper.GetOuterPackLineHarmonisedCodes(shipment, "AU");
			AssertContainsExactElementsInExactOrder(new[] { "2222" }, harmonisedCodes.ToArray());
			packLine.JL_HarmonisedCode = "222290";
			harmonisedCodes = HarmonisedCodeHelper.GetOuterPackLineHarmonisedCodes(shipment, "AU");
			AssertContainsExactElementsInExactOrder(new[] { "222290" }, harmonisedCodes.ToArray());
		}

		internal static void LoadOrCreateNewTariff(BusinessObjectFactory factory, string dataGrouping, string tariffCode, string nkTariffType)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, nkTariffType);
			var tariffView = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}
}
