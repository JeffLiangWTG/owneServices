using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CountryNameCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateCountryNameFrom()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "00001", "Some Foreign Port, COUNTRY", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "00003", "Some Foreign Port", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0002", "Test Domestic Port", startDate, endDate);
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKPortOfLoading = "";
			declaration.US_SchDLoading = "24575";

			//test if mapping exists
			RefLocoMap testMap = Factory.New<RefLocoMap>();
			testMap.RY_LocalPortCode = "24575";
			testMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			testMap.RY_RL_NKLocoPort = "USAAA";

			AssertEquals("If mapping between UNLOCO and schedule code exists, valid country name should be returned", "United States", declaration.CountryPortOfLoading);

			declaration.US_SchDLoading = "0002";
			AssertEquals("If UNLoco code is empty, valid country name should be returned by Domestic Port Code", ZString.Empty, declaration.CountryPortOfLoading);

			//test if mapping doesn't exists and we have foreign port code
			declaration.US_SchDLoading = "00001";
			AssertEquals("If UNLoco code is empty, valid country name should be returned by Foreign Port Code", "COUNTRY", declaration.CountryPortOfLoading);

			declaration.US_SchDLoading = "00003";
			AssertEquals("If UNLoco code is empty, valid country name should be returned by Foreign Port Code", ZString.Empty, declaration.CountryPortOfLoading);

			// test if UNLoco code not empty
			foreignPort.ZZD_Code = "TSTSE";
			foreignPort.ZZD_Description = "Some Foreign Port";

			RefCountry testCountry = Factory.New<RefCountry>();
			testCountry.RN_Code = "TS";
			testCountry.RN_Desc = "TEST-COUNTRY";

			declaration.JE_RL_NKPortOfLoading = "TSTSE";
			AssertEquals("If UNLoco code not empty, valid country name should be returned", "TEST-COUNTRY", declaration.CountryPortOfLoading);
		}

		public void TestGetForeignPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var port1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "0013", "Common", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			var port2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "0014", "AES", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port2.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.AES);
			var port3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "0015", "InBond", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port3.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();

			AssertForeignPort(port1, "0013", USCForeignPortWrapper.Type.AES);
			AssertForeignPort(port1, "0013", USCForeignPortWrapper.Type.Common);
			AssertForeignPort(port1, "0013", USCForeignPortWrapper.Type.InBond);

			AssertForeignPort(port2, "0014", USCForeignPortWrapper.Type.AES);
			AssertForeignPort(port2, "0014", USCForeignPortWrapper.Type.Common, false);
			AssertForeignPort(port2, "0014", USCForeignPortWrapper.Type.InBond, false);

			AssertForeignPort(port3, "0015", USCForeignPortWrapper.Type.AES, false);
			AssertForeignPort(port3, "0015", USCForeignPortWrapper.Type.Common, false);
			AssertForeignPort(port3, "0015", USCForeignPortWrapper.Type.InBond);
		}

		void AssertForeignPort(RefCusCodeList port, ZString code, USCForeignPortWrapper.Type type, bool isEqual = true)
		{
			var foreignPort = CountryNameCalculator.GetForeignPort(code, type, Factory);
			if (isEqual)
			{
				AssertEquals(port.ZZD_Code, foreignPort.PortCode);
				AssertEquals(port.ZZD_Description, foreignPort.PortName);
				AssertSame("Cached", Factory.GetCachedValue<USCForeignPortWrapper>("CountryNameCalculator|" + code + "|" + type, () => null), foreignPort);
			}
			else
			{
				AssertEquals(ZString.Empty, foreignPort.PortCode);
				AssertEquals(ZString.Empty, foreignPort.PortName);
			}
		}
	}
}
