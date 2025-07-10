using System;
using System.Collections.Generic;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class UNDGSubstanceUpdaterTest : DSUpdaterTest<UNDGSubstance, IZZUNDGSubstance>
	{
		public override void AssertDeletePreCondition()
		{
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.ZZUNDGSubstance WHERE DG_UNNO = '9999'"));
			AssertEquals(1, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.UNDGAttribute JOIN dbo.ZZUNDGSubstance ON DA_DG = DG_PK WHERE DG_UNNO = '9999'"));
		}

		public override void AssertDeleteDataSet()
		{
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.ZZUNDGSubstance WHERE DG_UNNO = '9999'"));
			AssertEquals(0, conn.ExecuteScalar($@"SELECT COUNT(*) FROM dbo.UNDGAttribute JOIN dbo.ZZUNDGSubstance ON DA_DG = DG_PK WHERE DG_UNNO = '9999'"));
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
				case nameof(IZZUNDGSubstance):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE DG_UNNO = '9999'"));
					break;
				case nameof(IUNDGAttribute):
					AssertEquals($"Type : {type.Item1.Name}", type.Item2, conn.ExecuteScalar($@"SELECT COUNT(*) FROM {SharedSQLBuilder.GetTableName(type.Item1)} WHERE DA_Descriptor = 'Testing9999'"));
					break;
				default:
					Assert(true);
					break;
			}
		}

		protected override void AssertSysColumnsWhenInsert(Type type)
		{
			var whereCondition = string.Empty;
			switch (type.Name)
			{
				case nameof(IZZUNDGSubstance):
					whereCondition = "WHERE DG_UNNO = '9999'";
					break;
				case nameof(IUNDGAttribute):
					whereCondition = "WHERE DA_Descriptor = 'Testing9999'";
					break;
				default:
					break;
			}
			var tableName = SharedSQLBuilder.GetTableName(type);
			var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(type);
			using (var reader = conn.ExecuteReader($"SELECT TOP 1 {systemColumns} FROM {tableName} {whereCondition}"))
			{
				if (reader.Read())
				{
					AssertNotNullOrEmpty($"{tableName}.SystemCreateTimeUtc", reader[0].ToString());
					AssertNotNullOrEmpty($"{tableName}.SystemLastEditTimeUtc", reader[2].ToString());
					AssertEquals($"{tableName}.SystemCreateUser", "~BP", reader[1].ToString());
					AssertEquals($"{tableName}.SystemLastEditUser", "~BP", reader[3].ToString());
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Windows Compact FrameWorks does not support Enterprise.Core.Globalisation")]
		public override void AssertSysColumnsWhenUpdate(Type type, Tuple<string, string> updatedResult, DateTime lastEditTime)
		{
			var tableName = SharedSQLBuilder.GetTableName(type);
			var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(type);
			using (var reader = conn.ExecuteReader($"SELECT TOP 1 {systemColumns} FROM {tableName} WHERE {updatedResult.Item1} = '{updatedResult.Item2}'"))
			{
				if (reader.Read())
				{
					Assert($"{tableName}.SystemLastEditTimeUtc should be updated.", DateTime.Parse(reader[2].ToString()) >= lastEditTime);
					AssertEquals($"{tableName}.SystemLastEditUser", "~BP", reader[3].ToString());
				}
			}
		}

		protected override Tuple<string, string> UpdateData(object data)
		{
			var substance = data as UNDGSubstance;
			if (substance != null)
			{
				substance.DG_PSN = "A1";
				return Tuple.Create(nameof(UNDGSubstance.DG_PSN), "A1");
			}
			var substanceAttribute = data as UNDGAttribute;
			if (substanceAttribute != null)
			{
				substanceAttribute.DA_Descriptor = "X1";
				return Tuple.Create(nameof(UNDGAttribute.DA_Descriptor), "X1");
			}
			return null;
		}

		protected override UNDGSubstance GetServerData()
		{
			var substance = new UNDGSubstance
			{
				DG_UNNO = "9999",
				DG_Variant = "a",
				DG_Variation = "Air bag inflators.",
				DG_PSN = "AIR BAG INFLATORS",
				DG_Standard = "IMO",
				DG_Class = "9",
				DG_CargoMaxAmt = 0,
				DG_CargoMaxAmtUQ = "",
				DG_CargoPackAmtType = "NLM",
				DG_CargoPackIns = "P902 LP902",
				DG_Code = "9999a",
				DG_CodedStow = "",
				DG_DglPhrase = "",
				DG_EmergencyResponseGuide = "",
				DG_EMS = "F-B,S-X",
				DG_ExceptedQuantityCode = "",
				DG_ExpLim = "",
				DG_EXVector = "",
				DG_FlashPoint = "",
				DG_Hazards = "",
				DG_IBCIns = "",
				DG_IBCProv = "",
				DG_IMOTankIns = "",
				DG_IsActive = false,
				DG_IsNotOtherwiseSpecified = false,
				DG_LQ2OrPaxMaxAmt = 0,
				DG_LQ2OrPaxMaxAmtType = "NLM",
				DG_LQ2OrPaxMaxAmtUQ = "",
				DG_LQMaxAmt = 0,
				DG_LQMaxAmtType = "NLM",
				DG_LQMaxAmtUQ = "",
				DG_LQSpecProvIndex = "",
				DG_Markers = "",
				DG_MP = "",
				DG_PackIns = "",
				DG_PackProv = "",
				DG_PaxPackIns = "",
				DG_PG = "III",
				DG_Pointers = "",
				DG_SpecialHandlingCodes = "",
				DG_State = "S",
				DG_StowCat = "A",
				DG_SubLabel1 = "",
				DG_SubLabel2 = "",
				DG_TankProv = "",
				DG_TechName = "",
				DG_TreatAs = "",
				DG_UlineEMS = "1",
				DG_UniqueRecordId = "",
				DG_UNTankIns = "",
				DG_UsrUSDOTShippingName = ""
			};
			substance.UNDGAttributes = new[] { new UNDGAttribute
			{
				DA_Type = "STS",
				DA_Index = "10",
				DA_Language = "ENG",
				DA_Descriptor = "Testing9999",
			} };
			return substance;
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return Tuple.Create(typeof(IZZUNDGSubstance), 1);
			yield return Tuple.Create(typeof(IUNDGAttribute), 1);
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new UNDGSubstanceUpdater(proxy, dbHelper, versionControlManager);
		}

		protected override void PrepareDatabase()
		{
		}

		public void TestIsActiveCanBeUpdated()
		{
			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			var dataTable = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<IZZUNDGSubstance>(), nameof(serverData.DG_Code), serverData.DG_Code));
			AssertEquals(false, dataTable.Rows[0][nameof(serverData.DG_IsActive)]);

			serverData.DG_IsActive = true;
			proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			dataTable = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<IZZUNDGSubstance>(), nameof(serverData.DG_Code), serverData.DG_Code));
			AssertEquals(true, dataTable.Rows[0][nameof(serverData.DG_IsActive)]);
		}
	}
}
