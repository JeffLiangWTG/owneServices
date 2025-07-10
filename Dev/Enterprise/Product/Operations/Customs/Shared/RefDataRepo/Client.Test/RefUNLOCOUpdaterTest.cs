using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefUNLOCOUpdaterTest : DSUpdaterTest<RefUNLOCO, IRefUNLOCO>
	{
		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefUNLOCOUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter());
		}

		public override void AssertDeletePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefUNLOCO WHERE RL_Code = 'UNTST'"));
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefLocoMap WHERE RY_RL_NKLocoPort = 'UNTST'"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefUNLOCO WHERE RL_Code = 'UNTST'"));
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefLocoMap WHERE RY_RL_NKLocoPort = 'UNTST'"));
		}

		public override void AssertUpdatePreCondition()
		{
			return;
		}

		public override void TestDeleteDependentRecord()
		{
			Assert(true);
		}

		public override void AssertInsert(Tuple<Type, int> type)
		{
			switch (type.Item1.Name)
			{
				case nameof(IRefUNLOCO):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RL_Code = 'UNTST'"));
					break;
				case nameof(IRefLocoMap):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RY_RL_NKLocoPort = 'UNTST'"));
					break;
				default:
					Assert(true);
					break;
			}
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var refUNLOCO = serverData as RefUNLOCO;
			if (refUNLOCO != null)
			{
				refUNLOCO.RL_PortName = "XX";
				return Tuple.Create(nameof(RefUNLOCO.RL_PortName), "XX");
			}
			var refLocoMap = serverData as RefLocoMap;
			if (refLocoMap != null)
			{
				refLocoMap.RY_LocalPortCode = "XX";
				return Tuple.Create(nameof(RefLocoMap.RY_LocalPortCode), "XX");
			}
			return null;
		}

		protected override RefUNLOCO GetServerData()
		{
			return RefDbRepo.Client.Common.TestHelper.RefDataHelper.GetUNLOCO();
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IRefUNLOCO), 1);
			yield return Tuple.Create(typeof(IRefLocoMap), 1);
		}

		protected override void PrepareDatabase()
		{
		}
	}
}
