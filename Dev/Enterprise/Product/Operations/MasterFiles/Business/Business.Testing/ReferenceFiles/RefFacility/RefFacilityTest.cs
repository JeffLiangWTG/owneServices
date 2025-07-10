using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefFacility))]
	public class RefFacilityTest : EnterpriseBusinessObjectTestCase
	{
		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			factory.Save();

			return facility;
		}

		public void TestHumanReadableNameCore()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			AssertEquals($"Facility ({facility.RFT_Code})", facility.HumanReadableName);
		}

		public void TestTerminalType()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			facility.RFT_IsAir = true;
			facility.RFT_IsSea = false;
			facility.RFT_IsRail = false;
			facility.RFT_IsRoad = false;
			facility.RFT_IsInlandWaterway = false;

			AssertEquals(Core.Constants.TransportModes.Air, facility.RFT_TerminalType);

			facility.RFT_IsAir = false;
			facility.RFT_IsSea = true;

			AssertEquals(Core.Constants.TransportModes.Sea, facility.RFT_TerminalType);

			facility.RFT_IsSea = false;
			facility.RFT_IsRail = true;

			AssertEquals(Core.Constants.TransportModes.Rail, facility.RFT_TerminalType);

			facility.RFT_IsRail = false;
			facility.RFT_IsRoad = true;

			AssertEquals(Core.Constants.TransportModes.Road, facility.RFT_TerminalType);

			facility.RFT_IsRoad = false;
			facility.RFT_IsInlandWaterway = true;

			AssertEquals(Core.Constants.TransportModes.InlandWaterwayTransport, facility.RFT_TerminalType);

			facility.RFT_IsInlandWaterway = false;

			AssertEquals(ZString.Empty, facility.RFT_TerminalType);
		}

		public void TestRTF_CountryCodeDesc()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			facility.RFT_RN_NKCountryCode = "US";
			AssertEquals(facility.Country.Code + " - " + facility.Country.Description, facility.RFT_CountryCodeDesc);
		}

		public void TestRFT_FacilityTypeFullName()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			facility.RFT_FacilityType = Constants.FacilityType.Code.Terminal;
			AssertEquals(Constants.FacilityType.Code.Terminal + " - " + Constants.FacilityType.Description.Terminal, facility.RFT_FacilityTypeFullName);

			facility.RFT_FacilityType = Constants.FacilityType.Code.ContainerYard;
			AssertEquals(Constants.FacilityType.Code.ContainerYard + " - " + Constants.FacilityType.Description.ContainerYard, facility.RFT_FacilityTypeFullName);

			facility.RFT_FacilityType = Constants.FacilityType.Code.TransitWarehouse;
			AssertEquals(Constants.FacilityType.Code.TransitWarehouse + " - " + Constants.FacilityType.Description.TransitWarehouse, facility.RFT_FacilityTypeFullName);

			facility.RFT_FacilityType = "CCC";
			AssertEquals("CCC", facility.RFT_FacilityTypeFullName);
		}

		public void TestRTF_UNLOCODesc()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			var unloco = factory.NewWithValidTestData<RefUNLOCO>();
			facility.RFT_RL_NKLocationCode = unloco.Code;

			AssertEquals(facility.LocationCode.Code + " - " + facility.LocationCode.Description, facility.RFT_UNLOCODesc);
		}

		public void TestRFT_StateCodeDesc()
		{
			var factory = new BusinessObjectFactory();
			var facility = factory.NewWithValidTestData<RefFacility>();
			var refCountryState = factory.NewWithValidTestData<RefCountryStates>();
			facility.RFT_State = refCountryState.RW_Code;
			facility.RFT_RN_NKCountryCode = refCountryState.RW_RN_NKCountryCode;

			AssertEquals(facility.RFT_State + " - " + facility.State.RW_Description, facility.RFT_StateCodeDesc);
		}

		public void TestState()
		{
			var factory = new BusinessObjectFactory();
			var refCountryStates = factory.NewWithValidTestData<RefCountryStates>();
			var refFacility = factory.NewWithValidTestData<RefFacility>();

			refFacility.RFT_State = ZString.Empty;

			AssertNull(refFacility.State);

			refFacility.RFT_State = refCountryStates.RW_Code;
			refFacility.RFT_RN_NKCountryCode = refCountryStates.RW_RN_NKCountryCode;

			AssertNotNull(refFacility.State);
			AssertType(typeof(RefCountryStates), refFacility.State);
			AssertEquals(refFacility.State.RW_Description, refCountryStates.RW_Description);
		}
	}
}
