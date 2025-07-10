using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.Test
{
	[TestFixture]
	class RefCusPreferenceUpdaterInfoFixture
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void GetMergeSqlText_SaveDeleteRecord(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				connection.Open();
				var schemaInfo = new SchemaInfo(connection);
				var sqlBuilder = new SqlServerSQLBuilder();
				var foreignKeyRelationships = schemaInfo.GetReferencedForeignKeysFromDb(null).Where(x => x.ReferencedTable == typeof(IRefCusPreference)).ToArray();
				var fkDirectory = new Dictionary<Type, ForeignKeyRelationship[]>()
				{
					{ typeof(IRefCusPreference), foreignKeyRelationships}
				};
				var preferenceUpdaterInfo = new RefCusPreferenceUpdaterInfo<IRefCusPreference>();
				var mergeSql = preferenceUpdaterInfo.GetMergeSqlText(sqlBuilder, fkDirectory, schemaInfo);
				Assert.True(mergeSql.Contains(@"SELECT t.ZZS_PK, Deleted into #TempRefCusPreference_DELETE
FROM RefCusPreference AS t
JOIN #TempRefCusPreference AS s ON ((t.ZZS_ZZZ_NKDataGrouping = s.ZZS_ZZZ_NKDataGrouping) AND (t.ZZS_Preference = s.ZZS_Preference)) AND Deleted = 1;"));
				Assert.True(mergeSql.Contains(@"DELETE t
FROM RefCusPreference AS t
JOIN #TempRefCusPreference_DELETE AS s ON t.ZZS_PK = s.ZZS_PK;"));

				Assert.True(mergeSql.Contains(@"UPDATE t SET ZZ2_ZZS_Preference = NULL FROM RefCusRate t
JOIN #TempRefCusPreference_DELETE ON ZZS_PK = ZZ2_ZZS_Preference;"));
				Assert.True(mergeSql.Contains(@"UPDATE t SET ZX1_ZZS_Preference = NULL FROM RefCusCondition t 
JOIN #TempRefCusPreference_DELETE ON ZZS_PK = ZX1_ZZS_Preference
WHERE t.ZX1_ZZ1_Tariff IS NOT NULL OR t.ZX1_ZZ5_Nomenclature IS NOT NULL;"));
			}
		}
	}
}
