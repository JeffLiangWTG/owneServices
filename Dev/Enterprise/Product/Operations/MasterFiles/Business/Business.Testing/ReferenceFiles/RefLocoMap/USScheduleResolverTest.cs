using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class USScheduleResolverTest : TestCaseWithFactory
	{
		public void TestMatchingUserDefinedFirstThenSystemDefinedForScheduleD()
		{
			#region Prepare Test Data

			CreateUNLoco("ABAAA");

			CreateLocoMap("0002", "ABAAA", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("0003", "ABDDD", USLocoMapSystemUsageList.Codes.Sea, false);

			CreateUNLoco("ABBBB");
			CreateLocoMap("0004", "ABBBB", USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMap("0005", "ABDDD", USLocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMap("0006", "ABBBB", USLocoMapSystemUsageList.Codes.All, true);

			CreateUNLoco("ABCCC");
			CreateLocoMap("0007", "ABCCC", USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMap("0008", "ABCCC", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("0009", "ABCCC", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("0010", "ABCCC", USLocoMapSystemUsageList.Codes.All, true);

			CreateUNLoco("ABDDD");
			CreateLocoMap("0011", "ABDDD", USLocoMapSystemUsageList.Codes.Sea, false);
			CreateLocoMap("0012", "ABDDD", USLocoMapSystemUsageList.Codes.All, false);
			CreateLocoMap("0013", "ABDDD", USLocoMapSystemUsageList.Codes.SCD, true);

			CreateUNLoco("ABEEE");
			CreateLocoMap("0014", "ABEEE", USLocoMapSystemUsageList.Codes.Sea, true);
			CreateLocoMap("0015", "ABEEE", USLocoMapSystemUsageList.Codes.SCD, true);

			Factory.Save();

			#endregion

			AssertEquals("AIR user definied code should be matched", "0002", USScheduleResolver.GetScheduleCode(Schedule.D, "ABAAA", "AIR", Factory));
			AssertEquals("AIR system definied code should be matched", "0004", USScheduleResolver.GetScheduleCode(Schedule.D, "ABBBB", "AIR", Factory));
			AssertEquals("No code matched as multiple AIR user definied results found", ZString.Empty, USScheduleResolver.GetScheduleCode(Schedule.D, "ABCCC", "AIR", Factory));
			AssertEquals("ALL system definied code should be matched", "0012", USScheduleResolver.GetScheduleCode(Schedule.D, "ABDDD", "AIR", Factory));
			AssertEquals("SCD system definied code should be matched", "0015", USScheduleResolver.GetScheduleCode(Schedule.D, "ABEEE", "AIR", Factory));
		}

		public void TestMatchingArrivalPortForUserDefinedPortWhenSCDAndTransportMode()
		{
			#region Prepare Test Data

			CreateUNLoco("KARNJ");
			CreateLocoMap("2720", "KARNJ", USLocoMapSystemUsageList.Codes.Air, true);
			CreateLocoMap("2775", "KARNJ", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("2776", "KARNJ", USLocoMapSystemUsageList.Codes.Air, false);
			CreateLocoMap("2704", "KARNJ", USLocoMapSystemUsageList.Codes.All, false);
			CreateLocoMap("2704", "KARNJ", USLocoMapSystemUsageList.Codes.Sea, true);
			Factory.Save();

			#endregion

			var list = USScheduleResolver.GetMatchesForSchedule(Schedule.D, "KARNJ", USLocoMapSystemUsageList.Codes.Air, Factory);
			AssertEquals("AIR user added codes should be matched", 3, list.Count);
			var items = list.Select(x => x.RY_LocalPortCode).ToList();
			AssertEquals(true, items.Contains("2775"));
			AssertEquals(true, items.Contains("2776"));
			AssertEquals(true, items.Contains("2720"));
		}

		public void TestMatchingUserDefinedFirstThenSystemDefinedForScheduleK()
		{
			#region Prepare Test Data

			CreateUNLoco("ABAAA");
			CreateLocoMap("0001", "ABAAA", USLocoMapSystemUsageList.Codes.SCK, true);
			CreateLocoMap("0002", "ABAAA", USLocoMapSystemUsageList.Codes.SCK, false);

			CreateUNLoco("ABBBB");
			CreateLocoMap("0003", "ABBBB", USLocoMapSystemUsageList.Codes.SCK, true);

			CreateUNLoco("ABCCC");
			CreateLocoMap("0004", "ABCCC", USLocoMapSystemUsageList.Codes.SCK, true);
			CreateLocoMap("0005", "ABCCC", USLocoMapSystemUsageList.Codes.SCK, false);
			CreateLocoMap("0006", "ABCCC", USLocoMapSystemUsageList.Codes.SCK, false);

			Factory.Save();

			#endregion

			AssertEquals("SCK user definied code should be matched", "0002", USScheduleResolver.GetScheduleCode(Schedule.K, "ABAAA", "SCK", Factory));
			AssertEquals("SCK system definied code should be matched", "0003", USScheduleResolver.GetScheduleCode(Schedule.K, "ABBBB", "SCK", Factory));
			AssertEquals("No code matched as multiple SCK user definied results found", ZString.Empty, USScheduleResolver.GetScheduleCode(Schedule.K, "ABCCC", "SCK", Factory));
		}

		public void TestMatchingUNLOCO()
		{
			var loco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCD;

			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			noMatch.RY_SystemUsage = "OTH";

			AssertEquals(loco.RL_Code, USScheduleResolver.MatchingUNLOCO("LOCAL", Factory));

			var anotherMatch = loco.RefLocoMaps.AddNew();
			anotherMatch.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;
			AssertEquals(loco.RL_Code, USScheduleResolver.MatchingUNLOCO("LOCAL", Factory));
		}

		public void TestNoMatch()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "BRETT";
			AssertEquals(0, USScheduleResolver.GetMatchesForSchedule(Schedule.D, "BRETT", ZString.Empty, Factory).Count);
			AssertEquals(ZString.Empty, USScheduleResolver.GetScheduleCode(Schedule.D, "BRETT", ZString.Empty, Factory));
		}

		public void TestSingleMatch()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "BRETT";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match.RY_LocalPortCode = "LOCAL";
			match.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Sea;
			var noMatch = loco.RefLocoMaps.AddNew();
			noMatch.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			noMatch.RY_SystemUsage = USLocoMapSystemUsageList.Codes.Air;

			var matches = USScheduleResolver.GetMatchesForSchedule(Schedule.D, "BRETT", "SEA", Factory);
			AssertEquals(1, matches.Count);
			AssertEquals(match, matches[0]);
			AssertEquals("LOCAL", USScheduleResolver.GetScheduleCode(Schedule.D, "BRETT", "SEA", Factory));
		}

		public void TestMultipleMatches()
		{
			var loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "BRETT";
			var match = loco.RefLocoMaps.AddNew();
			match.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			match.RY_LocalPortCode = "1111";
			var match2 = loco.RefLocoMaps.AddNew();
			match2.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			match2.RY_SystemUsage = USLocoMapSystemUsageList.Codes.All;
			match2.RY_LocalPortCode = "2222";

			var matches = USScheduleResolver.GetMatchesForSchedule(Schedule.D, "BRETT", ZString.Empty, Factory);
			AssertEquals(2, matches.Count);
			AssertEquals(ZString.Empty, USScheduleResolver.GetScheduleCode(Schedule.D, "BRETT", ZString.Empty, Factory));
		}

		public void TestGetAllScheduleMatches()
		{
			var matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "JPTYO", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("58886", matches[0].RY_LocalPortCode);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "SGSIN", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("55976", matches[0].RY_LocalPortCode);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "AUSYD", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("60267", matches[0].RY_LocalPortCode);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "DOSDQ", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("24737", matches[0].RY_LocalPortCode);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "CASEI", Core.Constants.TransportModes.Sea, Factory);
			AssertEquals(1, matches.Count);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "PHBAU", Core.Constants.TransportModes.Sea, Factory);
			AssertEquals(2, matches.Count);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "PHLIM", Core.Constants.TransportModes.Sea, Factory);
			AssertEquals(2, matches.Count);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "AIAXA", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("24821", matches[0].RY_LocalPortCode);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "BRSNZ", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("35199", matches[0].RY_LocalPortCode);

			matches = USScheduleResolver.GetMatchesForSchedule(Schedule.K, "BESHR", Core.Constants.TransportModes.Air, Factory);
			AssertEquals(1, matches.Count);
			AssertEquals("42306", matches[0].RY_LocalPortCode);
		}

		void CreateUNLoco(string locoCode)
		{
			var testUSLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			testUSLoco.RL_Code = locoCode;
			testUSLoco.RL_PortName = "TEST Port - " + locoCode;
			testUSLoco.RL_IsSystem = true;
			testUSLoco.RL_HasAirport = true;
			testUSLoco.RL_HasSeaport = true;
			testUSLoco.RL_RN_NKCountryCode = "US";
		}

		void CreateLocoMap(string localPort, string unLoco, string usage, bool isSystem)
		{
			var locoMap = Factory.NewWithValidTestData<RefLocoMap>();
			locoMap.RY_LocalPortCode = localPort;
			locoMap.RY_RL_NKLocoPort = unLoco;
			locoMap.RY_SystemUsage = usage;
			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			locoMap.RY_IsSystem = isSystem;
		}
	}
}
