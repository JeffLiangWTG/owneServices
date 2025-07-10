using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInbondQPMessageStatusList()
		{
			CombineAssertions(() =>
				{
					var list = lookups.InbondQPMessageStatusList;
					AssertEquals("Codes", "ADA, ADO, ADW, CDA, CDO, CPA, CPO, CPW, CDW, EDA, EDO, EDW, ", list.CodesAsString);
					AssertSame("Cached", list, new CusInBondMoveHeaderLookups(moveHeader).InbondQPMessageStatusList);
				});
		}

		public void TestInbondWPMessageStatusList()
		{
			CombineAssertions(() =>
				{
					var list = lookups.InbondWPMessageStatusList;
					AssertEquals("Codes", "AAV, AEX, ATL, CAV, CEX, CTL, EAV, EEX, ETL, ", list.CodesAsString);
					AssertSame("Cached", list, new CusInBondMoveHeaderLookups(moveHeader).InbondWPMessageStatusList);
				});
		}

		public void TestExportTransportModeCodes()
		{
			CombineAssertions(() =>
			{
				var list = lookups.TransportModeCodes;
				AssertEquals("Codes", "10, 11", list.CodesAsString);
				AssertSame("Cached", list, new CusInBondMoveHeaderLookups(moveHeader).TransportModeCodes);
			});
		}

		public void TestWarehouseTransactionStatusList()
		{
			AssertEquals(Factory.GetCachedValue<WarehouseTransactionStatusList>(), lookups.WarehouseTransactionStatusList);
		}

		public void TestSplitCarrierCollection()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirContainer;
			var collection = header.Lookups.CarrierCollection;
			collection = header.Lookups.CarrierCollection;
			AssertNotNull(collection);
		}

		public void TestAirlineCollection()
		{
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";
			var refAirline2 = Factory.New<RefAirline>();
			refAirline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "";
			refAirline2.RM_TwoCharacterCode = "A2";
			refAirline2.RM_ThreeLetterCode = "BBB";
			var refAirline3 = Factory.New<RefAirline>();
			refAirline3.RM_EagleAddedAirlinePrefixOrAccountingCode = "789";
			refAirline3.RM_TwoCharacterCode = "A3";
			refAirline3.RM_ThreeLetterCode = "";
			var collection = moveHeader.Lookups.AirlineCollection;
			AssertNotNull(collection);
			var airline1 = collection.FirstOrDefault(airline => airline.RM_ThreeLetterCode == "AAA");
			AssertNotNull("Airlines with a Numeric Code and Three Letter Code should be returned", airline1);
			Assert("Airline should be of the derived type 'InbondRefAirline' to support the Three Letter Code FindBox lookup", airline1 is ThreeLetterRefAirline);
			var airline2 = collection.FirstOrDefault(airline => airline.RM_ThreeLetterCode == "BBB");
			AssertNull("Airlines without a Numeric Code should not be returned", airline2);
			var airline3 = collection.FirstOrDefault(airline => airline.RM_TwoCharacterCode == "A3");
			AssertNull("Airlines without a Three Letter Code should not be returned", airline2);
		}

		public void TestFIRMSCollectionType()
		{
			AssertType<ZZRefCusCodeListCombinedCollection>(moveHeader.Lookups.FIRMSCollection);
		}

		public void TestForeignDestPortKCodeList()
		{
			var foreignDest = Factory.New<RefUNLOCO>();
			foreignDest.RL_Code = "!ZZ22";
			foreignDest.RL_PortName = "Crystal Lawns";
			var locoMapping = Factory.New<RefLocoMap>();
			locoMapping.RY_LocalPortCode = "60001";
			locoMapping.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping.RY_IsSystem = false;
			var locoMapping2 = Factory.New<RefLocoMap>();
			locoMapping2.RY_LocalPortCode = "60002";
			locoMapping2.RY_RL_NKLocoPort = "!ZZ22";
			locoMapping2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;
			locoMapping2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMapping2.RY_IsSystem = false;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60001", "60001 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60002", "60002 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			AssertEquals(0, moveHeader.Lookups.ForeignDestPortKCodeList.Count);

			moveHeader.BM_RL_NKForeignDestPort = "!ZZ22";
			AssertEquals(2, moveHeader.Lookups.ForeignDestPortKCodeList.Count);
			Assert(moveHeader.Lookups.ForeignDestPortKCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60001"));
			Assert(moveHeader.Lookups.ForeignDestPortKCodeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "60002"));
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		CusInBondMoveHeaderLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			lookups = new CusInBondMoveHeaderLookups(moveHeader);
		}
	}
}
