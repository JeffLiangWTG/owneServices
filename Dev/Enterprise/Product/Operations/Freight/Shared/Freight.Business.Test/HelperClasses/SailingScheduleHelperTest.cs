using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class SailingScheduleHelperTest : TestCaseWithFactory
	{
		public void TestGetLineOperatorPKFromExternalCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var oneStopCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OneStopCode, "LO1", Core.Constants.CountryCodes.Australia);
			var dakosyCode = org.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "LO2", Core.Constants.CountryCodes.Germany);

			Factory.Save();

			AssertEquals(org.PK, SailingScheduleHelper.GetLineOperatorPKFromExternalCode("LO1", FreightConstants.VesselDataProviders.OneStop, Factory));
			AssertEquals(org.PK, SailingScheduleHelper.GetLineOperatorPKFromExternalCode("LO2", FreightConstants.VesselDataProviders.DAKOSY, Factory));
		}

		public void TestGetLineOperatorPKFromExternalCode_DAKOSY()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var dpcCode = org1.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode, "XYZ", Core.Constants.CountryCodes.Germany);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var scacCode = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "XYZ", Core.Constants.CountryCodes.Germany);

			Factory.Save();

			AssertEquals("Org2 found using SCAC code", org2.PK, SailingScheduleHelper.GetLineOperatorPKFromExternalCode("XYZ", FreightConstants.VesselDataProviders.DAKOSY, Factory));

			org2.CustomsCodes.RemoveAndDeleteAll();
			Factory.Save();

			AssertEquals("Org1 found using DPC code", org1.PK, SailingScheduleHelper.GetLineOperatorPKFromExternalCode("XYZ", FreightConstants.VesselDataProviders.DAKOSY, Factory));
		}

		public void TestLineOperatorPKFromExternalCodeMatchesActiveOrganizations()
		{
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsActive = false;
			var oneStopCode1 = carrier1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OneStopCode, "RCL", Core.Constants.CountryCodes.Australia);

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsActive = false;
			var oneStopCode2 = carrier2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OneStopCode, "RCL", Core.Constants.CountryCodes.Australia);

			var carrier3 = Factory.NewWithValidTestData<OrgHeader>();
			carrier3.OH_IsActive = true;
			var oneStopCode3 = carrier3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OneStopCode, "RCL", Core.Constants.CountryCodes.Australia);

			var carrier4 = Factory.NewWithValidTestData<OrgHeader>();
			carrier4.OH_IsActive = false;
			var oneStopCode4 = carrier4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.OneStopCode, "RCL", Core.Constants.CountryCodes.Australia);

			Factory.Save();

			AssertEquals("The code matches the active organization", carrier3.PK, SailingScheduleHelper.GetLineOperatorPKFromExternalCode("RCL", FreightConstants.VesselDataProviders.OneStop, Factory));
		}
	}
}
