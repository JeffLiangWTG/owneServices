using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class UNLOCO_USPortsDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaulting()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "!ZZ";
			var locoMap = Factory.New<RefLocoMap>();
			locoMap.RY_LocalPortCode = "PZZ!";
			locoMap.RY_RL_NKLocoPort = "!ZZ";
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			locoMap.RY_IsSystem = true;

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PZZ!", "Test Name", startDate, endDate);
			Factory.Save();

			var collection = new ZZRefCusCodeListCombinedCollection(Factory);
			collection.Add(port1);

			var bizObj = Factory.New<DummyBusinessObject>();
			var defaulter = new UNLOCO_USPortsDefaulter(Factory, bizObj.Z0_CodeInfo, bizObj.Z0_DescriptionInfo,
				() => { return new List<RefLocoMap>(new[] { locoMap }); }, (n) => { return collection; });
			bizObj.Z0_Code = "PZZ!";
			defaulter.DefaultUNLOCO();
			AssertEquals("!ZZ", bizObj.Z0_Description);
			bizObj.Z0_Code = ZString.Empty;
			defaulter.DefaultPort();
			AssertEquals("PZZ!", bizObj.Z0_Code);

			var locoMap1 = Factory.New<RefLocoMap>();
			locoMap1.RY_LocalPortCode = "PCC!";
			locoMap1.RY_RL_NKLocoPort = "!ZZ";
			locoMap1.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap1.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			locoMap1.RY_IsSystem = true;

			var port2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "PCC!", "Test Name", startDate, endDate);
			Factory.Save();
			collection.Add(port2);

			defaulter = new UNLOCO_USPortsDefaulter(Factory, bizObj.Z0_CodeInfo, bizObj.Z0_DescriptionInfo,
				() => { return new List<RefLocoMap>(new[] { locoMap, locoMap1 }); }, (n) => { return collection; });
			Assert(defaulter.HasMultipleMappingPorts);
			AssertEquals(2, defaulter.MappingPorts.Count);
			bizObj.Z0_Code = ZString.Empty;
			bizObj.Z0_Description = "!ZZ";
			defaulter.DefaultPort();
			AssertEquals(ZString.Empty, bizObj.Z0_Code);
		}
	}
}
