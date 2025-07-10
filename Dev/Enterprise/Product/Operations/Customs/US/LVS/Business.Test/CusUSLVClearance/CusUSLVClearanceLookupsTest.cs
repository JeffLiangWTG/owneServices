using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVClearanceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestULH_TransportModeList()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var list = shipment.Lookups.ULH_TransportModeList;

			AssertEquals(6, list.Count);
			AssertEquals(TransportTypeList.Descriptions.Sea, list[TransportTypeList.Codes.Sea].Description);
			AssertEquals(TransportTypeList.Descriptions.Air, list[TransportTypeList.Codes.Air].Description);
			AssertEquals(TransportTypeList.Descriptions.Road, list[TransportTypeList.Codes.Road].Description);
			AssertEquals(TransportTypeList.Descriptions.Rail, list[TransportTypeList.Codes.Rail].Description);
			AssertEquals(TransportTypeList.Descriptions.Mail, list[TransportTypeList.Codes.Mail].Description);
			AssertEquals(TransportTypeList.Descriptions.Truck, list[TransportTypeList.Codes.Truck].Description);
		}

		public void TestULH_IORTypeList()
		{
			var shipment = Factory.New<CusUSLVClearance>();
			var list = shipment.Lookups.ULH_IORTypeList;
			var codeLists = new OrgCodeLists();
			var cusCodes = codeLists.CustomsCodes_List(Core.Constants.CountryCodes.UnitedStates);

			AssertEquals(3, list.Count);
			AssertEquals(cusCodes.GetDescriptionFromCode(OrgCusCode.USACodeTypes.EmployerIdentificationNumber), list[OrgCusCode.USACodeTypes.EmployerIdentificationNumber].Description);
			AssertEquals(cusCodes.GetDescriptionFromCode(OrgCusCode.USACodeTypes.CBPAssignedNumber), list[OrgCusCode.USACodeTypes.CBPAssignedNumber].Description);
			AssertEquals(cusCodes.GetDescriptionFromCode(OrgCusCode.USACodeTypes.SocialSecurityNumber), list[OrgCusCode.USACodeTypes.SocialSecurityNumber].Description);
		}

		public void TestULH_PortOfLoadingList()
		{
			var unLOCO1 = Factory.New<RefUNLOCO>();
			unLOCO1.RL_Code = "!ZZ11";
			unLOCO1.RL_PortName = "Crystal Lawns 1";
			var unLOCO2 = Factory.New<RefUNLOCO>();
			unLOCO2.RL_Code = "!ZZ22";
			unLOCO2.RL_PortName = "Crystal Lawns 2";
			CreateRefLocoMap("60001", "!ZZ11", USLocoMapSystemUsageList.Codes.SCK);
			CreateRefLocoMap("60002", "!ZZ11", USLocoMapSystemUsageList.Codes.SCK);
			CreateRefLocoMap("60003", "!ZZ22", USLocoMapSystemUsageList.Codes.SCK);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60003", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_RL_NKPortOfLoading = "!ZZ11";
			AssertEquals(2, clearance.Lookups.ULH_PortOfLoadingList.Count);
			AssertEquals(true, clearance.Lookups.ULH_PortOfLoadingList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			AssertEquals(true, clearance.Lookups.ULH_PortOfLoadingList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60002"));

			clearance.ULH_RL_NKPortOfLoading = "!ZZ22";
			AssertEquals(0, clearance.Lookups.ULH_PortOfLoadingList.Count);
		}

		public void TestULH_PortOfDischargeList()
		{
			var unLOCO = Factory.New<RefUNLOCO>();
			unLOCO.RL_Code = "!ZZ22";
			unLOCO.RL_PortName = "Crystal Lawns";
			CreateRefLocoMap("4001", "!ZZ22", USLocoMapSystemUsageList.Codes.Sea);
			CreateRefLocoMap("4002", "!ZZ22", USLocoMapSystemUsageList.Codes.Sea);
			CreateRefLocoMap("4003", "!ZZ22", USLocoMapSystemUsageList.Codes.Air);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4001", "4001 Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4002", "4002 Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "4003", "4003 Test Name", startDate, endDate);
			Factory.Save();

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = "SEA";
			clearance.ULH_RL_NKPortOfDischarge = "!ZZ22";
			AssertEquals(2, clearance.Lookups.ULH_PortOfDischargeList.Count);
			Assert(clearance.Lookups.ULH_PortOfDischargeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4001"));
			Assert(clearance.Lookups.ULH_PortOfDischargeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "4002"));

			clearance.ULH_TransportMode = "AIR";
			AssertEquals(0, clearance.Lookups.ULH_PortOfDischargeList.Count);
		}

		void CreateRefLocoMap(string localPortCode, string locoPort, string usage)
		{
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = localPortCode;
			locoMapping.RY_RL_NKLocoPort = locoPort;
			locoMapping.RY_SystemUsage = usage;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = false;
		}
	}
}
