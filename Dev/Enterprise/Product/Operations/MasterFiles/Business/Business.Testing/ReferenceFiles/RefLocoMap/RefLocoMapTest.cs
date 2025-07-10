using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefLocoMap))]
	sealed class RefLocoMapTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsScheduleDUsage()
		{
			RefLocoMap locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			AssertEquals("IsScheduleDUsage", true, locoMap.IsUSScheduleDUsage);

			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			AssertEquals("IsScheduleDUsage", true, locoMap.IsUSScheduleDUsage);

			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			AssertEquals("IsScheduleDUsage", true, locoMap.IsUSScheduleDUsage);

			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			AssertEquals("IsScheduleDUsage", false, locoMap.IsUSScheduleDUsage);

			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			locoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			AssertEquals("IsScheduleDUsage", false, locoMap.IsUSScheduleDUsage);

			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;
			AssertEquals("IsScheduleDUsage", true, locoMap.IsUSScheduleDUsage);
		}

		void CreateUNLoco(string locoCode)
		{
			RefUNLOCO testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			testUSLoco.RL_Code = locoCode;
			testUSLoco.RL_PortName = "TEST Port - " + locoCode;
			testUSLoco.RL_IsSystem = true;
			testUSLoco.RL_HasAirport = true;
			testUSLoco.RL_HasSeaport = true;
			testUSLoco.RL_RN_NKCountryCode = "US";
		}

		RefLocoMap CreateLocoMap(string localPort, string unLoco, string usage, bool isSystem)
		{
			RefLocoMap locoMap = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap.RY_LocalPortCode = localPort;
			locoMap.RY_RL_NKLocoPort = unLoco;
			locoMap.RY_SystemUsage = usage;
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_IsSystem = isSystem;
			return locoMap;
		}

		public void TestReadOnlyFields()
		{
			string locoCode = "US555";
			CreateUNLoco(locoCode);
			var refLocoMap = CreateLocoMap("1155", locoCode, USLocoMapSystemUsageList.Codes.SCD, false);
			AssertEquals(false, refLocoMap.RY_IsSystem);
			AssertEquals(false, refLocoMap.RY_LocalPortCodeInfo.ReadOnly);
			AssertEquals(false, refLocoMap.RY_RL_NKLocoPortInfo.ReadOnly);
			AssertEquals(false, refLocoMap.RY_RNInfo.ReadOnly);
			AssertEquals(false, refLocoMap.RY_SystemUsageInfo.ReadOnly);

			var refLocoMap2 = CreateLocoMap("1156", locoCode, USLocoMapSystemUsageList.Codes.SCD, true);
			AssertEquals(true, refLocoMap2.RY_IsSystem);
			AssertEquals(true, refLocoMap2.RY_LocalPortCodeInfo.ReadOnly);
			AssertEquals(true, refLocoMap2.RY_RL_NKLocoPortInfo.ReadOnly);
			AssertEquals(true, refLocoMap2.RY_RNInfo.ReadOnly);
			AssertEquals(true, refLocoMap2.RY_SystemUsageInfo.ReadOnly);
		}

		public void TestLoadingRefLocoMapWithDuplicateCode()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AUXXX";
			unloco.RefLocoMaps.Factory.New<RefLocoMap>();
			RefUNLOCO unloco2 = Factory.New<RefUNLOCO>();
			unloco2.RL_Code = "AUXXX";
			AssertNoExceptionThrown(delegate()
			{
				unloco2.RefLocoMaps.AddNew();
			}
			);
		}

		public void TestStaticLoad()
		{
			RefUNLOCO unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "USAA1";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			RefLocoMap locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RL_NKLocoPort = "USAA1";
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;

			AssertEquals(locoMap, RefLocoMap.Load(Factory, "USAA1", Core.Constants.CountryGuids.UnitedStates, ""));
			AssertEquals(locoMap, RefLocoMap.Load(Factory, "USAA1", Core.Constants.CountryGuids.UnitedStates, USLocoMapSystemUsageList.Codes.All));
			AssertNull(RefLocoMap.Load(Factory, "USAA1", ZGuid.Empty, USLocoMapSystemUsageList.Codes.All));
			AssertNull(RefLocoMap.Load(Factory, "", Core.Constants.CountryGuids.UnitedStates, USLocoMapSystemUsageList.Codes.All));
		}

		public void TestIsCACustomsPortUsage()
		{
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.All;
			Assert("IsCACustomsPortUsage", locoMap.IsCACustomsPortUsage);

			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Air;
			Assert("IsCACustomsPortUsage", locoMap.IsCACustomsPortUsage);

			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			Assert("IsCACustomsPortUsage", locoMap.IsCACustomsPortUsage);

			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sub;
			Assert("IsCACustomsPortUsage", !locoMap.IsCACustomsPortUsage);

			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Sea;
			locoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			Assert("IsCACustomsPortUsage", !locoMap.IsCACustomsPortUsage);

			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			locoMap.RY_SystemUsage = CALocoMapSystemUsageList.Codes.Oth;
			Assert("IsCACustomsPortUsage", locoMap.IsCACustomsPortUsage);
		}

		public void TestLocalPortCodeFiledType()
		{
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_SystemUsage = LocoMapSystemUsageList.Codes.CustomsPortCodeList;
			AssertEquals(FieldType.Text, locoMap.LocalPortCodeFiledType);

			locoMap.RY_RN = Core.Constants.CountryGuids.France;
			AssertEquals(FieldType.Text, locoMap.LocalPortCodeFiledType);
			locoMap.RY_SystemUsage = LocoMapSystemUsageList.Codes.PCS;
			AssertEquals(FieldType.TextDropEdit, locoMap.LocalPortCodeFiledType);
		}
	}
}
