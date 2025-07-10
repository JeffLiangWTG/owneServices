using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefLocoMapValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDuplicateUSScheduleDUsage()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AU~~2";

			RefLocoMap locoMap = unloco.RefLocoMaps.AddNew();
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			locoMap.RY_LocalPortCode = "9999";

			RefUNLOCO unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "AU~~3";

			RefLocoMap locoMap2 = unloco2.RefLocoMaps.AddNew();
			locoMap2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			locoMap2.RY_LocalPortCode = "9999";
			string errorMessage = string.Format(RefLocoMapValidation.DuplicateCodeMessage, unloco.RL_Code);
			AssertHasWarning(locoMap2.RY_LocalPortCodeInfo, errorMessage);

			locoMap2.RY_LocalPortCode = "9999";
			AssertHasWarning(locoMap2.RY_LocalPortCodeInfo, errorMessage);

			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			locoMap.RY_LocalPortCode = "9999";
			errorMessage = string.Format(RefLocoMapValidation.DuplicateCodeMessage, unloco2.RL_Code);
			AssertHasWarning(locoMap.RY_LocalPortCodeInfo, errorMessage);

			locoMap.RY_LocalPortCode = "9999";
			AssertHasWarning(locoMap.RY_LocalPortCodeInfo, errorMessage);

			RefUNLOCO unloco3 = Factory.New<RefUNLOCO>();
			unloco3.RL_Code = "AU~~1";
			RefLocoMap locoMap3 = unloco3.RefLocoMaps.AddNew();
			locoMap3.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap3.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			locoMap3.RY_LocalPortCode = "9999";
			errorMessage = string.Format(RefLocoMapValidation.DuplicateCodeMessage, unloco.RL_Code + ", " + unloco2.RL_Code);
			AssertHasWarning(locoMap3.RY_LocalPortCodeInfo, errorMessage);

			locoMap2.RY_LocalPortCode = "9999";
			errorMessage = string.Format(RefLocoMapValidation.DuplicateCodeMessage, unloco3.RL_Code + ", " + unloco.RL_Code);
			AssertHasWarning(locoMap2.RY_LocalPortCodeInfo, errorMessage);

			locoMap.RY_LocalPortCode = "9999";
			errorMessage = string.Format(RefLocoMapValidation.DuplicateCodeMessage, unloco3.RL_Code + ", " + unloco2.RL_Code);
			AssertHasWarning(locoMap.RY_LocalPortCodeInfo, errorMessage);
		}

		public void TestDuplicateCACustomsPortUsage()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AU~~2";

			var locoMap = unloco.RefLocoMaps.AddNew();
			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;
			locoMap.RY_LocalPortCode = "9999";

			var unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "AU~~3";

			var locoMap2 = unloco2.RefLocoMaps.AddNew();
			locoMap2.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap2.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Oth;
			locoMap2.RY_LocalPortCode = "9999";
			string errorMessage = string.Format(RefLocoMapValidation.DuplicateCodeMessage, unloco.RL_Code);
			AssertHasWarning(locoMap2.RY_LocalPortCodeInfo, errorMessage);

			locoMap2.RY_LocalPortCode = "9998";
			AssertNoWarnings(locoMap2.RY_LocalPortCodeInfo);
		}

		public void TestCheckRY_SystemUsage()
		{
			RefLocoMap locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			locoMap.RY_SystemUsage = "AAA";
			AssertHasErrors(locoMap.RY_SystemUsageInfo);

			locoMap.RY_SystemUsage = AirSeaMailSystemUsageList.Codes.Air;
			AssertNoErrors(locoMap.RY_SystemUsageInfo);

			locoMap.RY_RN = Core.Constants.CountryGuids.Turkey;
			locoMap.RY_SystemUsage = "BBB";
			AssertHasErrors(locoMap.RY_SystemUsageInfo);

			locoMap.RY_IsSystem = true;
			locoMap.Validation.ValidateRY_SystemUsage();
			AssertNoErrors(locoMap.RY_SystemUsageInfo);
			AssertHasWarning(locoMap.RY_SystemUsageInfo, RefLocoMapValidation.InvalidSystemUsageWarningMessage);
		}

		public void TestCheckRY_LocalPortCodeInfoForIceland()
		{
			RefLocoMap locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RN = Core.Constants.CountryGuids.Iceland;
			locoMap.RY_SystemUsage = "COC";

			locoMap.RY_LocalPortCode = "";
			AssertNoErrors(locoMap.RY_LocalPortCodeInfo);

			locoMap.RY_LocalPortCode = "abc";
			AssertNoErrors(locoMap.RY_LocalPortCodeInfo);

			locoMap.RY_LocalPortCode = "aaaaa";
			AssertNoErrors(locoMap.RY_LocalPortCodeInfo);

			locoMap.RY_LocalPortCode = "1";
			AssertHasErrors(locoMap.RY_LocalPortCodeInfo);

			locoMap.RY_LocalPortCode = "abc34";
			AssertHasErrors(locoMap.RY_LocalPortCodeInfo);

			locoMap.RY_LocalPortCode = "aaabbb";
			AssertHasErrors(locoMap.RY_LocalPortCodeInfo);
		}

		public void TestCheckDuplicateSystemUsageAndCountry()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AU~~~";

			RefLocoMap locoMap = unloco.RefLocoMaps.AddNew();
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_SystemUsage = "A";

			RefLocoMap locoMap2 = unloco.RefLocoMaps.AddNew();
			locoMap2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap2.RY_SystemUsage = "A";
			AssertNoError("US can have multiple of the same schedule type for one port", locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertHasWarning("Give US codes a warning only", locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap2.RY_SystemUsage = "B";
			AssertNoError(locoMap2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			RefLocoMap locoMap3 = unloco.RefLocoMaps.AddNew();
			locoMap3.RY_RN = Core.Constants.CountryGuids.Australia;
			locoMap3.RY_SystemUsage = "A";

			RefLocoMap locoMap4 = unloco.RefLocoMaps.AddNew();
			locoMap4.RY_RN = Core.Constants.CountryGuids.Australia;
			locoMap4.RY_SystemUsage = "A";
			AssertHasError(locoMap4.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			locoMap4.RY_SystemUsage = "B";
			AssertNoError(locoMap4.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);

			RefLocoMap locoMapCN1 = unloco.RefLocoMaps.AddNew();
			locoMapCN1.RY_RN = Core.Constants.CountryGuids.China;
			locoMapCN1.RY_SystemUsage = "CUS";
			RefLocoMap locoMapCN2 = unloco.RefLocoMaps.AddNew();
			locoMapCN2.RY_RN = Core.Constants.CountryGuids.China;
			locoMapCN2.RY_SystemUsage = "CUS";
			AssertHasWarning("Give CN codes a warning only", locoMapCN2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertNoErrors(locoMapCN2.RY_SystemUsageInfo);

			RefLocoMap locoMapTR1 = unloco.RefLocoMaps.AddNew();
			locoMapTR1.RY_RN = Core.Constants.CountryGuids.Turkey;
			locoMapTR1.RY_SystemUsage = "CUS";
			RefLocoMap locoMapTR2 = unloco.RefLocoMaps.AddNew();
			locoMapTR2.RY_RN = Core.Constants.CountryGuids.Turkey;
			locoMapTR2.RY_SystemUsage = "CUS";
			AssertHasWarning("Give TR codes a warning only", locoMapTR2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertNoErrors(locoMapTR2.RY_SystemUsageInfo);

			RefLocoMap locoMapMX1 = unloco.RefLocoMaps.AddNew();
			locoMapMX1.RY_RN = Core.Constants.CountryGuids.Mexico;
			locoMapMX1.RY_SystemUsage = "CUS";
			RefLocoMap locoMapMX2 = unloco.RefLocoMaps.AddNew();
			locoMapMX2.RY_RN = Core.Constants.CountryGuids.Mexico;
			locoMapMX2.RY_SystemUsage = "CUS";
			AssertHasWarning("Give MX codes a warning only", locoMapMX2.RY_SystemUsageInfo, RefLocoMapValidation.DuplicateCountryAndSystemUsageCombinations);
			AssertNoErrors(locoMapMX2.RY_SystemUsageInfo);
		}

		public void TestCountryOfPCSSystemUsage()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var locoMap = unloco.RefLocoMaps.AddNew();
			locoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			locoMap.RY_SystemUsage = "PCS";
			AssertHasError("PCS only can be used when country is France or Overseas France", locoMap.RY_SystemUsageInfo, RefLocoMapValidation.TheCountryOfPCSIsNotFranceOrTerritory);

			locoMap.RY_RN = Core.Constants.CountryGuids.France;
			locoMap.RY_SystemUsage = "PCS";
			AssertHasError("PCS only can be used when country is equal to the country of the port", locoMap.RY_SystemUsageInfo, RefLocoMapValidation.TheCountryOfPCSNotEqualToCountryOfThePort);

			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			locoMap.RY_SystemUsage = "PCS";
			AssertNoErrors(locoMap.RY_SystemUsageInfo);
		}

		public void TestCheckRY_RN()
		{
			RefLocoMap locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RN = ZGuid.Empty;
			AssertHasErrors(locoMap.RY_RNInfo);

			locoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			AssertNoErrors(locoMap.RY_RNInfo);
		}
	}
}
