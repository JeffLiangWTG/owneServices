using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class RefCountryStatesUpdaterTest : DSUpdaterWithLanguageTextTest<RefCountryStates, IRefCountryStates>
	{
		protected override RefCountryStates GetServerData()
		{
			return RefDbRepo.Client.Common.TestHelper.RefDataHelper.GetCountryStates();
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IRefCountryStates), 1);
			yield return Tuple.Create(typeof(IRefLanguageText), 1);
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var refCountryStates = serverData as RefCountryStates;
			if (refCountryStates != null)
			{
				refCountryStates.RW_Description = "XX";
				return Tuple.Create(nameof(RefCountryStates.RW_Description), "XX");
			}
			var refLanguageText = serverData as RefLanguageText;
			if (refLanguageText != null)
			{
				refLanguageText.RLT_ColumnName = "XX";
				return Tuple.Create(nameof(RefLanguageText.RLT_ColumnName), "XX");
			}
			return null;
		}

		public override void AssertInsert(Tuple<Type, int> type)
		{
			switch (type.Item1.Name)
			{
				case nameof(IRefCountryStates):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RW_RegionName = 'TsT'"));
					break;
				case nameof(IRefLanguageText):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RLT_Text = 'LanguageText'"));
					break;
				default:
					Assert(true);
					break;
			}
		}

		public override void AssertUpdatePreCondition()
		{
			return;
		}

		public override void AssertDeletePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCountryStates WHERE RW_RegionName = 'TsT'"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCountryStates WHERE RW_RegionName = 'TsT'"));
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefCountryStatesUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter());
		}

		protected override void PrepareDatabase()
		{
			dbHelper.ExecuteNonQuery("UPDATE dbo.RefUNLOCO SET RL_RW = NULL", null, null);
		}

		protected override void SetServerChild(RefCountryStates serverData, RefLanguageText[] serverChildData)
		{
			serverData.RefLanguageTexts = serverChildData;
		}
	}
}
