using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class TransportLegCarrierAddressMatcherTest : TestCaseWithFactory
	{
		public void TestAddressMatchingWithCarrierCodeInRegistration()
		{
			var factory = new BusinessObjectFactory();
			var org1 = factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			org1.MainAddress.OA_Address1 = "ADD 1";
			var org2 = factory.New<OrgHeader>();
			org2.OH_Code = "AB2";
			org2.MainAddress.OA_Address1 = "ADD 1";
			var org3 = factory.New<OrgHeader>();
			org3.OH_Code = "AB1";
			org3.MainAddress.OA_Address1 = "ADD 1";

			var code1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			var code2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			var code3 = org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODE2", Core.Constants.CountryCodes.UnitedStates);

			var orgMatchingData = new OrgHeaderForMatching(factory);
			var orgCusCodeMatch = new OrgCusCodeForMatching();

			orgMatchingData.CustomsCodes.Add(orgCusCodeMatch);
			orgCusCodeMatch.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCodeMatch.OK_CustomsRegNo = "CODE1";
			orgCusCodeMatch.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var logger = new TestErrorLogger();
			var matcher = new TransportLegCarrierAddressMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);

			AssertNull("Duplicate address match for CODE1", matchedAddress);
			AssertContains("Warning - Unable to determine a match as multiple organizations were matched with registration detail (Country/Region='US', Type='CCC', Number='CODE1').", logger.Logs);

			orgCusCodeMatch.OK_CustomsRegNo = "CODE2";
			orgCusCodeMatch.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			logger.ClearLogs();
			matcher = new TransportLegCarrierAddressMatcher(factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);

			AssertEquals(org3.MainAddress.PK, matchedAddress.PK);
		}

		public void TestAddressMatchingWithCarrierCodeInRegistration_ActiveInactiveOrganizations()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AB3";
			org1.MainAddress.OA_Address1 = "ADD 1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AB2";
			org2.MainAddress.OA_Address1 = "ADD 1";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "AB1";
			org3.MainAddress.OA_Address1 = "ADD 1";

			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODE1", Core.Constants.CountryCodes.UnitedStates);
			org3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CODE1", Core.Constants.CountryCodes.UnitedStates);

			var orgMatchingData = new OrgHeaderForMatching(Factory);
			var orgCusCodeMatch = new OrgCusCodeForMatching();

			orgMatchingData.CustomsCodes.Add(orgCusCodeMatch);
			orgCusCodeMatch.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCodeMatch.OK_CustomsRegNo = "CODE1";
			orgCusCodeMatch.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var logger = new TestErrorLogger();
			var matcher = new TransportLegCarrierAddressMatcher(Factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			var matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);

			AssertNull("Duplicate address match for CODE1 and organizations all active", matchedAddress);
			AssertContains("Warning - Unable to determine a match as multiple organizations were matched with registration detail (Country/Region='US', Type='CCC', Number='CODE1').", logger.Logs);

			org1.OH_IsActive = false;
			org3.OH_IsActive = false;

			logger = new TestErrorLogger();
			matcher = new TransportLegCarrierAddressMatcher(Factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);

			AssertNotNull("Only one organization is active, return matched address", matchedAddress);
			AssertContains("Information - Matched to address 'ADD 1' on 'AB2' by registration detail (Country/Region='US', Type='CCC', Number='CODE1').", logger.Logs);
			AssertEquals(org2.MainAddress.PK, matchedAddress.PK);

			org2.OH_IsActive = false;

			logger = new TestErrorLogger();
			matcher = new TransportLegCarrierAddressMatcher(Factory, IfUnmatched.TakeBehaviourFromOverallSetting, logger);
			matchedAddress = matcher.GetMatchingAddress(orgMatchingData, null, false);

			AssertNull("All organizations are inactive", matchedAddress);
			AssertContains("Warning - No match found", logger.Logs);
		}
	}
}
