using System;
using System.Collections.Generic;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class UNDGCountryReferenceUpdaterTest : DSUpdaterTest<UNDGCountryReference, IUNDGCountryReference>
	{
		protected override UNDGCountryReference GetServerData()
		{
			var reference = new UNDGCountryReference
			{
				DCR_Code = "TEST",
				DCR_Description = "Description",
				DCR_RN_NKCountry = "XX",
				DCR_HasFlashPointLower = true,
				DCR_HasFlashPointUpper = true,
				DCR_FlashPointLowerCentigrade = 1,
				DCR_FlashPointUpperCentigrade = 2,
				DCR_Type = "PSA",
			};
			reference.UNDGCountryReferencePivots = new UNDGCountryReferencePivot[]
			{
				new UNDGCountryReferencePivot
				{
					DCP_UNNO = "TT",
					DCP_Standard = "TST",
					DCP_Variant = "V"
				}
			};
			return reference;
		}

		protected override IEnumerable<Tuple<Type, int>> GetTypesAndNoOfRecords()
		{
			yield return new Tuple<Type, int>(typeof(IUNDGCountryReference), 1);
			yield return new Tuple<Type, int>(typeof(IUNDGCountryReferencePivot), 1);
		}

		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new TwoTableDataSetUpdater<UNDGCountryReference, UNDGCountryReferencePivot, IUNDGCountryReference, IUNDGCountryReferencePivot>(proxy, dbHelper, versionControlManager);
		}

		protected override void PrepareDatabase()
		{
		}

		protected override void AssertSysColumnsWhenInsert(Type type)
		{
			var tableName = SharedSQLBuilder.GetTableName(type);
			var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(type);
			using (var reader = conn.ExecuteReader($"SELECT TOP 1 {systemColumns} FROM {tableName}"))
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

		protected override Tuple<string, string> UpdateData(object serverData)
		{
			var reference = serverData as UNDGCountryReference;
			if (reference != null)
			{
				reference.DCR_Description = "TST";
				return Tuple.Create(nameof(UNDGCountryReference.DCR_Description), "TST");
			}
			var pivot = serverData as UNDGCountryReferencePivot;
			if (pivot != null)
			{
				pivot.DCP_Variant = "SV";
				return Tuple.Create(nameof(UNDGCountryReferencePivot.DCP_Variant), "SV");
			}
			return null;
		}
	}
}
