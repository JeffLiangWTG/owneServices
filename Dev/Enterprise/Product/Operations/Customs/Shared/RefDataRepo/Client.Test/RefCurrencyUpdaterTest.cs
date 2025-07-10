using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefCurrencyUpdaterTest : DSUpdaterWithLanguageTextTest<RefCurrency, IRefCurrency>
	{
		protected override Tuple<string, string> UpdateData(object data)
		{
			var currency = data as RefCurrency;
			if (currency != null)
			{
				currency.RX_Desc = "AUD";
				return Tuple.Create(nameof(RefCurrency.RX_Desc), "AUD");
			}
			var languageText = data as RefLanguageText;
			if (languageText != null)
			{
				languageText.RLT_Text = "X1";
				return Tuple.Create(nameof(RefLanguageText.RLT_Text), "X1");
			}
			return null;
		}

		protected override RefCurrency GetServerData()
		{
			var currency = new RefCurrency
			{
				RX_Code = "AUX",
				RX_Desc = "Australian dollar",
				RX_IsActive = true,
				RX_ISOSubUnitRatio = 1,
				RX_SubUnitName = "X",
				RX_SubUnitRatio = 1,
				RX_Symbol = "$",
				RX_UnitName = "C"
			};
			currency.RefLanguageTexts = new[] { new RefLanguageText
			{
				RLT_ColumnName = "Desc",
				RLT_ParentTableCode = "RX",
				RLT_Text = "XXX",
				RLT_Language = "AU"
			} };
			return currency;
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IRefCurrency), 1);
			yield return Tuple.Create(typeof(IRefLanguageText), 1);
		}

		public override void AssertInsert(Tuple<Type, int> type)
		{
			switch (type.Item1.Name)
			{
				case nameof(IRefCurrency):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RX_Code = 'AUX'"));
					break;
				case nameof(IRefLanguageText):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE RLT_Text = 'XXX'"));
					break;
				default:
					Assert(true);
					break;
			}
		}

		public override void AssertUpdatePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCurrency WHERE RX_Code = 'AUX'"));
		}

		public override void AssertDeletePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCurrency WHERE RX_Code = 'AUX'"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.RefCurrency WHERE RX_Code = 'AUX'"));
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefCurrencyUpdater(proxy, dbHelper, versionControlManager);
		}

		protected override void PrepareDatabase()
		{
		}

		protected override void SetServerChild(RefCurrency serverData, RefLanguageText[] serverChildData)
		{
			serverData.RefLanguageTexts = serverChildData;
		}
	}
}
