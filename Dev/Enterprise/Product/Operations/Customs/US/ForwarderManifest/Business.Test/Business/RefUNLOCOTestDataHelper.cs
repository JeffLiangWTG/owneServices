using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public static class RefUNLOCOTestDataHelper
	{
		public static RefLocoMap CreateLocoMapIfNotExists(BusinessObjectFactory factory,string localPort, string unLoco, string usage, bool isSystem = false)
		{
			var codeFilter = new ZQuery(RefLocoMapSchema.RY_LocalPortCode, localPort);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_RL_NKLocoPort, unLoco);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_SystemUsage, usage);
			codeFilter.AddToFilter(RefLocoMapSchema.RY_IsSystem, isSystem);

			var locoMap = factory.LoadTop1<RefLocoMap>(codeFilter);
			if (locoMap == null)
			{
				locoMap = factory.NewWithValidTestData<RefLocoMap>();
				locoMap.RY_LocalPortCode = localPort;
				locoMap.RY_RL_NKLocoPort = unLoco;
				locoMap.RY_SystemUsage = usage;
				locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
				locoMap.RY_IsSystem = isSystem;
			}

			return locoMap;
		}

		public static void CreateLocoIfNotExists(BusinessObjectFactory factory, string locoCode, string country = "US")
		{
			var codeFilter = new ZQuery(RefUNLOCOSchema.RL_Code, locoCode);
			var unLoco = factory.LoadTop1<RefUNLOCO>(codeFilter);
			if (unLoco == null)
			{
				var testUSLoco = factory.NewWithValidTestData<RefUNLOCO>();
				testUSLoco.RL_Code = locoCode;
				testUSLoco.RL_PortName = "TEST Port - " + locoCode;
				testUSLoco.RL_IsSystem = true;
				testUSLoco.RL_HasAirport = true;
				testUSLoco.RL_HasSeaport = true;
				testUSLoco.RL_RN_NKCountryCode = country;
				factory.Save();
			}
		}

		public static void CreateScheduleDPort(BusinessObjectFactory factory, bool isCreateDependentData = false)
		{
			CreateLocoIfNotExists(factory, "USTES", "US");
			CreateLocoMapIfNotExists(factory, "4001", "USTES", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists(factory, "4002", "USTES", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists(factory, "4003", "USTES", USLocoMapSystemUsageList.Codes.Sea);
			CreateLocoMapIfNotExists(factory, "4004", "USTES", USLocoMapSystemUsageList.Codes.Air);

			if (isCreateDependentData)
			{
				var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
				var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4001", "Test Name", startDate, endDate);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4002", "Test Name", startDate, endDate);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4004", "Test Name", startDate, endDate);
			}
			factory.Save();
		}

		public static void CreateScheduleKPort(BusinessObjectFactory factory, bool isCreateDependentData = false)
		{
			CreateLocoIfNotExists(factory, "TEST1", "US");
			CreateLocoIfNotExists(factory, "TEST2", "US");
			CreateLocoMapIfNotExists(factory, "60001", "TEST1", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists(factory, "60002", "TEST1", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists(factory, "60003", "TEST1", USLocoMapSystemUsageList.Codes.SCK);
			CreateLocoMapIfNotExists(factory, "60004", "TEST2", USLocoMapSystemUsageList.Codes.SCK);

			if (isCreateDependentData)
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60004", "60004 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			}
			factory.Save();
		}
	}
}
