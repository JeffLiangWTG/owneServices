using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : TestCaseWithFactory
	{
		public void TestManifestTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = ZString.Empty;
			AssertEquals("", header.Lookups.ManifestTypeList.CodesAsString);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AOR, EOR, ALD", header.Lookups.ManifestTypeList.CodesAsString);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("BBB, DOR, VOR", header.Lookups.ManifestTypeList.CodesAsString);
		}

		public void TestNatures()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("EXP, IMP, TSS, TRN, FRB", header.Lookups.Natures.CodesAsString);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", "ABC Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var cde = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CDE", "CDE Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var xyz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "XYZ", "XYZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var zzz = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZZ", "ZZZ Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var customsOffices = header.Lookups.CustomsOfficeList;
			Assert(customsOffices.ContainsCode("ABC"));
			Assert(customsOffices.ContainsCode("CDE"));
			Assert(customsOffices.ContainsCode("XYZ"));
			Assert(customsOffices.ContainsCode("ZZZ"));
		}

		public void TestTransportModeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("AIR, SEA", header.Lookups.TransportModeList.CodesAsString);
		}

		public void TestContainerModeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("BBK, BLK, CNT, LQD, OTH", header.Lookups.ContainerModeList.CodesAsString);
		}

		public void TestAgentTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("DRT, CLD, AGT, CHT, COU, OTH", header.Lookups.AgentTypeList.CodesAsString);
		}

		public void TestCarrierList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<ShippingProviderCollection>(header.Lookups.CarrierList);
			header.AMA_TransportMode = "AIR";
			AssertType<AirShippingProviderCollection>(header.Lookups.CarrierList);
			header.AMA_TransportMode = "SEA";
			AssertType<SeaShippingProviderCollection>(header.Lookups.CarrierList);
		}

		public void TestOutturnProviderList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities,
				"VWG", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			var outturnProviderList = header.Lookups.OutturnProviderList;
			outturnProviderList.Load();
			Assert(outturnProviderList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "VWG"));
		}

		public void TestGateInOutMessageTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("", header.Lookups.GateInOutMessageTypeList.CodesAsString);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ADI, ATI", header.Lookups.GateInOutMessageTypeList.CodesAsString);
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("DGI, DGO, DCI, BGI, TGI, TGO", header.Lookups.GateInOutMessageTypeList.CodesAsString);
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("", header.Lookups.GateInOutMessageTypeList.CodesAsString);
			header.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			AssertEquals("", header.Lookups.GateInOutMessageTypeList.CodesAsString);
		}

		public void TestCustomsStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "Customs Manifest Status");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("6, 8", header.Lookups.CustomsStatusList.CodesAsString);
			AssertEquals(AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus), header.Lookups.CustomsStatusList);
		}

		public void TestOrganizationsFindBoxList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(typeof(OrganisationsFindBoxCollection), header.Lookups.OrganizationsFindBoxList.GetType());
		}

		public void TestParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeader>(header.Lookups.Parent);
		}

		public void TestExcessIndicatorList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("1, 2", header.Lookups.ExcessIndicatorList.CodesAsString);
		}

		public void TestRegistrationLookups()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			// TODO: Once we switched to 'CMAN' statuses from 'CSTA' change to 'Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus'
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var za8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "8", "Proceed to Border (SACU clearances)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var allCodes = header.Lookups.RegistrationStatusList.CodesAsString;

			AssertContains("NOT,", allCodes);
			AssertContains("ACK,", allCodes);
			AssertContains("8", allCodes);
		}
	}
}
