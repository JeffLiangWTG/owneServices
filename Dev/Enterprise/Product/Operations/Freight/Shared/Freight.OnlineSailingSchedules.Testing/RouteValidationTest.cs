using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class RouteValidationTest : TestCaseWithFactory
	{
		public void TestValidateCarrierSCAC()
		{
			var carrier = new ServiceModel.Carrier
			{
				Name = "",
				Code = ""
			};

			var serviceLeg = new ServiceModel.Leg
			{
				Voyage = new ServiceModel.Voyage
				{
					Code = "",
					Vessel = new ServiceModel.Vessel { VesselName = "", ImoNumber = "" },
					TradeLane = new ServiceModel.TradeLane { Name = "" },
					Operator = carrier
				},

				LoadPort = new ServiceModel.Port { Unloco = "" },
				DischargePort = new ServiceModel.Port { Unloco = "" },

				Etd = DateTime.Today,
				Eta = DateTime.Today,
			};

			var serviceRoute = new ServiceModel.Route
			{
				Carrier = carrier,
				Legs = new[] { serviceLeg }
			};

			var route = new Route(Factory);
			route.SetValues(serviceRoute);

			var validation = new RouteValidation(route);
			validation.ValidateCarrierSCAC();
			AssertHasNotifications("Carrier SCAC must not be empty.", route.CarrierSCACInfo);

			serviceRoute.Carrier.Code = "ABCDE";
			route = new Route(Factory);
			route.SetValues(serviceRoute);

			validation = new RouteValidation(route);
			validation.ValidateCarrierSCAC();
			AssertHasError(route.CarrierSCACInfo, "No organization found for SCAC ABCDE.");
			AssertHasError(route.CarrierSCACInfo, "SCAC code must consist of 4 symbols.");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OrgCode";

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "ABCD";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCode.OK_OH = org.PK;

			Factory.Save();

			serviceRoute.Carrier.Code = "ABCD";
			route = new Route(Factory);
			route.SetValues(serviceRoute);
			validation = new RouteValidation(route);
			validation.ValidateCarrierSCAC();
			AssertNoErrors(route.CarrierSCACInfo);
		}

		public void TestValidateCarrierSCAC_TwoOrgsWithSameSCACExist()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "OrgCode1";
			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_CustomsRegNo = "ABCD";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "OrgCode2";
			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "ABCD";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier");

			AssertEquals("ABCD", route.CarrierSCAC);
			AssertHasError(route.CarrierSCACInfo, "Multiple organizations with SCAC ABCD found.");
		}

		public void TestValidateCarrierSCAC_FiltersOutInactiveOrganisations_SingleOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsActive = true;
			org.OH_Code = "any";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertNoErrors(route);

			org.OH_IsActive = false;

			Factory.Save();

			route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertHasError(route.CarrierSCACInfo, "No organization found for SCAC ABCD.");
		}

		public void TestValidateCarrierSCAC_FiltersOutInactiveOrganisations_MultipleOrgs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsActive = true;
			org1.OH_Code = "any1";

			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_CustomsRegNo = "ABCD";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsActive = true;
			org2.OH_Code = "any2";

			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "ABCD";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertHasError(route.CarrierSCACInfo, "Multiple organizations with SCAC ABCD found.");

			org2.OH_IsActive = false;

			Factory.Save();

			route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertNoErrors(route);
		}
	}
}
