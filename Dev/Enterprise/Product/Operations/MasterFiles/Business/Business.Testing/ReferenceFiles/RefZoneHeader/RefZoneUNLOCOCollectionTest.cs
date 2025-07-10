using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefZoneUNLOCOCollection))]
	sealed class RefZoneUNLOCOCollectionTest : BusinessObjectCollectionTestCase
	{
		[ExpectNoExceptions]
		[StressTest]
		public void TestLoad_Performance()
		{
			var header = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USAR"));
			AssertEquals("Precondition: USAR PK", "ca88e9cc-b130-4895-820a-c100c182ab4a", header.PK.ToString());

			using (TestConnection.TrackExecutedCommands())
			{
				BusinessObjectFactory.StartLogging();

				string debugLog;
				try
				{
					header.UNLOCOs.Load();
					debugLog = BusinessObjectFactory.DebugLog;
				}
				finally
				{
					BusinessObjectFactory.StopLogging();
				}

				var query = TestConnection.ExecutedCommands;
				var queryAsString = string.Join("\r\n\r\n", query.Cast<string>().ToArray());

				CombineAssertions("Query should get RL_PK from dbo.RefZonePivot instead of list of PKs that cause 'Error Number 8632 : Internal error: An expression services limit has been reached.'", () =>
				{
					AssertContains("RL_PK should filter from: RefZonePivot", "WHERE RL_PK IN (SELECT F2_ParentID FROM dbo.RefZonePivot WHERE F2_FZ =", queryAsString);
					AssertNotContains
					(
						"RL_PK should not filter from: list of PKs",
						"WHERE (RL_PK in ('",
						queryAsString
					);

					AssertEquals
					(
						"Should not call Factory.Load by filtering RefZonePivot and F2_ParentID i.e. ManyToManyBusinessObjectCollection.LoadRelationshipBusinessObjectFor should use QuickLoadRelationshipBusinessObjectFor",
						0,
						Regex.Matches(debugLog, Regex.Escape("LOAD Top1 Enterprise.MasterFiles.Business.RefZonePivot Filter = F2_FZ = CONVERT('ca88e9cc-b130-4895-820a-c100c182ab4a', 'System.Guid') and F2_ParentID =")).Count
					);
				});
			}
		}

		[ExpectNoExceptions]
		[StressTest]
		public void TestLoadThousandsManyToManyNoStackOverflowException()
		{
			RefZoneHeader header = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "USAR"));
			header.UNLOCOs.Load();
			Assert("Need to have over 3000 children for the problem to be seen", header.UNLOCOs.Count >= 3000);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefZoneUNLOCOCollection(Factory.New<RefZoneHeader>());
		}

		public void TestCollectionReadonly()
		{
			var wrsZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean));
			var collection = wrsZone.UNLOCOs;
			Assert("Collection should be read-only", collection.ReadOnly);

			var zoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneHeader.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			collection = zoneHeader.UNLOCOs;
			AssertEquals("Collection for ALL zone should not be read-only", false, collection.ReadOnly);
		}

		public void TestDataRefreshBus()
		{
			var zoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			zoneHeader.FZ_Code = "VIVI";
			zoneHeader.UNLOCOs.Add(ausyd);
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var zoneHeaderFromAnotherFactory = anotherFactory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "VIVI"));
			AssertContainsExactElementsInAnyOrder("Pre-condition",
				new[] { "AUSYD" },
				zoneHeaderFromAnotherFactory.UNLOCOs.Cast<RefUNLOCO>().Select(un => un.RL_Code));

			zoneHeader.UNLOCOs.RemoveAndDelete(ausyd);
			Factory.Save();
			AssertEquals("UNLOCO is removed from UNLOCOs collection in another factory by data refresh bus", 0, zoneHeaderFromAnotherFactory.UNLOCOs.Count);

			zoneHeader.UNLOCOs.Add(aumel);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("UNLOCO is added to UNLOCOs collection in another factory by data refresh bus",
				new[] { "AUMEL" },
				zoneHeaderFromAnotherFactory.UNLOCOs.Cast<RefUNLOCO>().Select(un => un.RL_Code));
		}
	}
}
