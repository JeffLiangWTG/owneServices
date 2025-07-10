using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNatures()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertSame(Factory.GetCachedValue<ZaShipmentTypeList>(), header.Lookups.Natures);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "EXP", "IMP", "TSS", "TRN", "ZZZ" }, header.Lookups.Natures.GetAllCodesZString());
		}

		public void TestContainerModes()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Break Bulk(BB)", header.Lookups.ContainerModes.GetDescriptionFromCode(Core.Constants.ContainerModes.BreakBulk));
			AssertEquals("Bulk(DB)", header.Lookups.ContainerModes.GetDescriptionFromCode(Core.Constants.ContainerModes.Bulk));
			AssertEquals("Containerized(CN)", header.Lookups.ContainerModes.GetDescriptionFromCode(Core.Constants.ContainerModes.Containerised));
			AssertEquals("Liquid(LB)", header.Lookups.ContainerModes.GetDescriptionFromCode(Core.Constants.ContainerModes.Liquid));
			AssertEquals("Other(MX)", header.Lookups.ContainerModes.GetDescriptionFromCode(Core.Constants.ContainerModes.Other));
		}

		public void TestMessageStatusList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertSame(Factory.GetCachedValue<ZAMessageStatusList>(), header.Lookups.MessageStatusList);
		}

		public void TestAgentTypeList()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				AssertEquals("BASE", "DRT, CLD, AGT, CHT, COU, OTH", header.Lookups.AgentTypeList.CodesAsString);
				header.AMA_TransportMode = "AIR";
				AssertEquals("BASE_AIR", "DRT, CLD, AGT, CHT, COU, OTH, CLA, CLM, FWB", header.Lookups.AgentTypeList.CodesAsString);
				header.AMA_ApplicationCode = "VOC";
				AssertEquals("BASE_AIR_ZA_VOC", "DRT, CLD, AGT, CHT, COU, OTH, CLA, CLM", header.Lookups.AgentTypeList.CodesAsString);
				header.AMA_TransportMode = "";
				AssertEquals("BASE_VOC", "DRT, CLD, AGT, CHT, COU, OTH", header.Lookups.AgentTypeList.CodesAsString);
			});
		}

		public void TestRegistrationLookups()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsStatus");
			var za8 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border (SACU clearances)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IAllowCancel", "true");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "ISendEntryDocs", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateCustomsStatus", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za8.PK, "IUpdateEntryNumber", "");
			var za9 = helper.CreateNewOrGetExistingCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "INotify", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(za9.PK, "IUpdateEntryNumber", "");
			Factory.Save();
			var headerVU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			var listVU1 = headerVU.Lookups.RegistrationStatusList;
			var listVU2 = headerVU.Lookups.RegistrationStatusList;
			Assert(ReferenceEquals(listVU1, listVU2));
			Assert(listVU1.ContainsCode("NOT"));
			Assert(!listVU1.ContainsCode("AWA"));
			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM);
			var listZA1 = headerZA.Lookups.RegistrationStatusList;
			var listZA2 = headerZA.Lookups.RegistrationStatusList;
			Assert(!ReferenceEquals(listVU1, listZA1));
			Assert(ReferenceEquals(listZA1, listZA2));
			Assert(listZA1.ContainsCode("8"));
			Assert(listZA1.ContainsCode("9"));
		}

		public void TestTSS_CarrierLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertType(typeof(ShippingProviderCollection), header.Lookups.CarrierList);
			header.AMA_TransportMode = "AIR";
			AssertType(typeof(AirShippingProviderCollection), header.Lookups.CarrierList);
			header.AMA_TransportMode = "SEA";
			AssertType(typeof(SeaShippingProviderCollection), header.Lookups.CarrierList);
		}

		public void TestTSS_VesselLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertType(typeof(RefVesselCollection), header.Lookups.TSSRefVessels);
		}

		public void TestVesselRadioCallSignLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertType(typeof(RadioCallSignCodeFindBoxCollection), header.Lookups.VesselRadioCallSigns);
		}
	}
}
