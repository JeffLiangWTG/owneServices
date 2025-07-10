using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefTimeZoneSetUpdaterTest : DSUpdaterTest<RefTimeZoneSet, IRefTimeZoneSet>
	{
		public override Tuple<string, string> ExtraUpdateTestAction(object serverData, string name)
		{
			Tuple<string, string> result = null;
			foreach (var property in serverData.GetType().GetProperties().Where(x => x.PropertyType == typeof(RefTimeZone) && !x.PropertyType.IsArray))
			{
				var children = property.GetValue(serverData);
				result = UpdateType(children, name) ?? result;
			}

			return result;
		}

		public override IEnumerable<Tuple<Type, int>> GetTypesForUpdate()
		{
			return GetTypesAndNoOfRecords().Skip(1);
		}

		public override void AssertUpdatePreCondition()
		{
			return;
		}

		public override void AssertDeletePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefTimeZoneSet WHERE R3_TimeZoneSetName = 'TMZS1'"));
			AssertEquals(2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefTimeZone WHERE R2_CivilianTimeZoneCode IN ('CD1', 'DL1')"));
			AssertEquals(2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefTimeZoneRule WHERE R4_DaylightSavingDayName IN ('D1', 'D2')"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefTimeZoneSet WHERE R3_TimeZoneSetName = 'TMZS1'"));
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefTimeZone WHERE R2_CivilianTimeZoneCode IN ('CD1', 'DL1')"));
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefTimeZoneRule WHERE R4_DaylightSavingDayName IN ('D1', 'D2')"));
		}

		public override void TestDeleteDependentRecord()
		{
			Assert(true);
		}

		public override void AssertInsert(Tuple<Type, int> type)
		{
			switch (type.Item1.Name)
			{
				case nameof(IRefTimeZoneSet):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE R3_TimeZoneSetName = 'TMZS1'"));
					break;
				case nameof(IRefTimeZone):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE R2_CivilianTimeZoneCode IN ('CD1', 'DL1')"));
					break;
				case nameof(IRefTimeZoneRule):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE R4_DaylightSavingDayName IN ('D1', 'D2')"));
					break;
				default:
					Assert(true);
					break;
			}
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefTimeZoneSetUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter());
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var refTimeZone = serverData as RefTimeZone;
			if (refTimeZone != null)
			{
				refTimeZone.R2_MilitaryTimeZoneCode = "XX";
				return Tuple.Create(nameof(RefTimeZone.R2_MilitaryTimeZoneCode), "XX");
			}
			var refTimeZoneRule = serverData as RefTimeZoneRule;
			if (refTimeZoneRule != null)
			{
				refTimeZoneRule.R4_DaylightSavingDayName = "XX";
				return Tuple.Create(nameof(RefTimeZoneRule.R4_DaylightSavingDayName), "XX");
			}
			return null;
		}

		protected override RefTimeZoneSet GetServerData()
		{
			return RefDbRepo.Client.Common.TestHelper.RefDataHelper.GetTimeZoneSet();
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IRefTimeZoneSet), 1);
			yield return Tuple.Create(typeof(IRefTimeZone), 2);
			yield return Tuple.Create(typeof(IRefTimeZoneRule), 2);
		}

		protected override void PrepareDatabase()
		{
		}
	}
}
