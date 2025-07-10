using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class RefCountryUpdaterTest : DSUpdaterWithLanguageTextTest<RefCountry, IRefCountry>
	{
		protected string DataSetName => SharedSQLBuilder.GetTableName<IRefCountry>();

		public void TestCountryRuleFieldsNotUpdated()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), null, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			PrepareDatabase();
			var storageData = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0}", SharedSQLBuilder.GetTableName<IRefCountry>()));
			var serverData = CreateServerData(storageData);

			typeof(RefCountry).GetProperty(nameof(RefCountry.RN_PostcodeValidationRule)).SetValue(serverData, "X1");
			typeof(RefCountry).GetProperty(nameof(RefCountry.RN_AddressFormattingRule)).SetValue(serverData, "X1");
			typeof(RefCountry).GetProperty(nameof(RefCountry.RN_StateProvinceValidationRule)).SetValue(serverData, "X1");

			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			var dataTable = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<IRefCountry>(), SharedSQLBuilder.GetPKColumn<IRefCountry>(),
				storageData.Rows[0][SharedSQLBuilder.GetPKColumn<IRefCountry>()]));

			AssertNotEquals("X1", dataTable.Rows[0][nameof(RefCountry.RN_PostcodeValidationRule)]);
			AssertNotEquals("X1", dataTable.Rows[0][nameof(RefCountry.RN_AddressFormattingRule)]);
			AssertNotEquals("X1", dataTable.Rows[0][nameof(RefCountry.RN_StateProvinceValidationRule)]);
		}

		protected RefCountry CreateServerData(DataTable storageData)
		{
			return Helper.CreateServerData<RefCountry>(storageData);
		}

		protected override RefCountry GetServerData()
		{
			return RefDbRepo.Client.Common.TestHelper.RefDataHelper.GetCountry();
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IRefCountry), 1);
			yield return Tuple.Create(typeof(IRefLanguageText), 1);
		}

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var refCountry = serverData as RefCountry;
			if (refCountry != null)
			{
				refCountry.RN_Desc = "XX";
				return Tuple.Create(nameof(RefCountry.RN_Desc), "XX");
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
				case nameof(IRefCountry):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RN_CountryDialingCode = 'TsT'"));
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
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCountry WHERE RN_CountryDialingCode = 'TsT'"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCountry WHERE RN_CountryDialingCode = 'TsT'"));
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefCountryUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter());
		}

		protected override void PrepareDatabase()
		{
		}

		protected override void SetServerChild(RefCountry serverData, RefLanguageText[] serverChildData)
		{
			serverData.RefLanguageTexts = serverChildData;
		}
	}
}
