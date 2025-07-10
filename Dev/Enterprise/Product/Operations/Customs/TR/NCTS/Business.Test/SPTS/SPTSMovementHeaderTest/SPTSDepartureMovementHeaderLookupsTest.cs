using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSDepartureMovementHeaderLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestTransportModeList()
		{
			var header = Factory.New<SPTSDepartureMovementHeader>();
			AssertEquals("AIR, SEA", header.Lookups.TransportModeList.CodesAsString);
		}

		public void TestShippingProviders()
		{
			var header = Factory.New<SPTSDepartureMovementHeader>();
			ShippingProviderCollection collection = header.Lookups.ShippingProviders;
			AssertNotNull(collection);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", "ABC Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var cde = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CDE", "CDE Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var xyz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XYZ", "XYZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var zzz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZZ", "ZZZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var xxx = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XXX", "XXX Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var header = Factory.New<SPTSDepartureMovementHeader>();
			var customsOffices = header.Lookups.CustomsOfficeList;
			Assert(customsOffices.ContainsCode("ABC"));
			Assert(customsOffices.ContainsCode("CDE"));
			Assert(customsOffices.ContainsCode("XYZ"));
			Assert(customsOffices.ContainsCode("ZZZ"));
		}

		public void TestTransitStatusList()
		{
			var movementHeader = Factory.New<SPTSDepartureMovementHeader>();
			var transitStatusList = movementHeader.Lookups.TransitStatusList;
			CombineAssertions("SPTSTransitStatusList", () =>
			{
				AssertNotNull(transitStatusList);
				AssertEquals(typeof(SPTSTransitStatusList), transitStatusList.GetType());
				AssertEquals("CodeAsString", "AWA, DAC, DCA, DGN, DMA, DRJ, DNR, DRL, ART, DCC, AWO, , AUP", transitStatusList.CodesAsString);
			});
		}
	}
}
