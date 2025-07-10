using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefCusConfigurationUpdaterInfoFixture
	{
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		[TransactionedTestCase]
		public void RefCusConfigurationUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(NEWID(), 'ZA', 'South Africa')

INSERT RefCusConfiguration (ZZJ_PK,ZZJ_RN_NKCustomsCountry,ZZJ_TariffDataSource,ZZJ_IsGenericCountry,ZZJ_AllowRiskManagement,ZZJ_IsTransitDeclarationCounty,ZZJ_TurnOnASYDCUDAManifest,ZZJ_TurnOnASYCUDACustoms,ZZJ_ZZZ_NKDefaultDataGrouping,ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping,ZZJ_StartDate,ZZJ_EndDate)
VALUES (NEWID(), 'ZA', 'WTG', 1, 1, 1, 1, 1, 'ZA', 'ZA', '1900-01-01', '2020-12-31'),
(NEWID(), 'ZA', 'WTG', 1, 1, 1, 1, 1, 'ZA', 'ZA', '2021-01-01', '2079-06-06')", trans);
					var info = new RefCusConfigurationUpdaterInfo_1();
					TestDBHelper.ExecuteNonQuery(conn, info.PrepareTemporaryTablesScripts.FirstOrDefault().Value, trans);
					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT {info.PrepareTemporaryTablesScripts.FirstOrDefault().Key} (ZZJ_PK,ZZJ_RN_NKCustomsCountry,ZZJ_TariffDataSource,ZZJ_IsGenericCountry,ZZJ_AllowRiskManagement,ZZJ_IsTransitDeclarationCounty,ZZJ_TurnOnASYDCUDAManifest,ZZJ_TurnOnASYCUDACustoms,ZZJ_ZZZ_NKDefaultDataGrouping,ZZJ_ZZZ_NKAlternateTariffOnlyDataGrouping,ZZJ_StartDate,ZZJ_EndDate, Deleted)
VALUES (NEWID(), 'ZA', 'WTG', 1, 1, 1, 1, 1, 'ZA', 'ZA', '2010-01-01', '2020-12-31', 0),
(NEWID(), 'ZA', 'WTG', 1, 1, 1, 1, 1, 'ZA', 'ZA', '1900-01-01', '2020-12-31', 1),
(NEWID(), 'ZA', 'WTG', 1, 1, 1, 1, 1, 'ZA', 'ZA', '2021-01-01', '2079-06-06', 0)", trans);
					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConfiguration WHERE ZZJ_StartDate = '2021-01-01'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConfiguration WHERE ZZJ_StartDate = '1900-01-01'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefCusConfiguration WHERE ZZJ_StartDate = '2010-01-01'", trans));
				}
			}
		}
	}
}
