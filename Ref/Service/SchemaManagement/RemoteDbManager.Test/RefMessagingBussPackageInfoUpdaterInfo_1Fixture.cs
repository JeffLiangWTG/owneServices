using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	[TestFixture]
	class RefMessagingBussPackageInfoUpdaterInfo_1Fixtures
	{
		[TransactionedTestCase]
		[TestCaseSource(typeof(TransactionedTestCaseAttribute), nameof(TransactionedTestCaseAttribute.RemoteDbTestCases))]
		public void RefCusProcedureUpdaterInfo(DbSchema dbSchema)
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(dbSchema);
			using (var conn  = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				using (var trans = conn.BeginTransaction())
				{
					TestDBHelper.ExecuteNonQuery(conn, @"
INSERT INTO RefMessagingBussPackageInfo (ZMP_PK,ZMP_PackageName)
VALUES ('1799FE44-08BE-42CF-B130-E89921BC3E85','A'),
('D272AC2C-C3CB-4981-851E-764899C55D73', 'B')

INSERT INTO RefMessagingBussCarrierInfo (ZMC_PK,ZMC_ZMP_PackageInfo,ZMC_CarrierCode,ZMC_CarrierName,ZMC_CountryCode)
VALUES (NEWID(),'1799FE44-08BE-42CF-B130-E89921BC3E85','AAAAA','AA','AA'),
(NEWID(),'D272AC2C-C3CB-4981-851E-764899C55D73','BBBBB','BB','AA')

INSERT INTO RefMessagingBussPackageVersion (ZMV_PK,ZMV_ZMP_PackageInfo,ZMV_Version)
VALUES (NEWID(), '1799FE44-08BE-42CF-B130-E89921BC3E85', 'AAA'),
(NEWID(), 'D272AC2C-C3CB-4981-851E-764899C55D73', 'BBB')
", trans);
					var info = new RefMessagingBussPackageInfoUpdaterInfo_1();
					foreach (var script in info.PrepareTemporaryTablesScripts)
					{
						TestDBHelper.ExecuteNonQuery(conn, script.Value, trans);
					}

					TestDBHelper.ExecuteNonQuery(conn, $@"
INSERT INTO #TempRefMessagingBussPackageInfo (ZMP_PK,ZMP_PackageName,Deleted)
VALUES (NEWID(),'A',1),
('B870FB04-63EB-4B7A-84F6-D332B2992E3F','B',0),
('6F717A71-A9E0-411B-A405-FEAC61086A64','C',0)

INSERT INTO #TempRefMessagingBussCarrierInfo (ZMC_PK,ZMC_ZMP_PackageInfo,ZMC_CarrierCode,ZMC_CarrierName,ZMC_CountryCode)
VALUES (NEWID(),'6F717A71-A9E0-411B-A405-FEAC61086A64','CCCCC','CC','CC'),
(NEWID(),'B870FB04-63EB-4B7A-84F6-D332B2992E3F','BBBBB','XB','XB')

INSERT INTO #TempRefMessagingBussPackageVersion (ZMV_PK,ZMV_ZMP_PackageInfo,ZMV_Version)
VALUES (NEWID(), '6F717A71-A9E0-411B-A405-FEAC61086A64', 'CCC'),
(NEWID(), 'B870FB04-63EB-4B7A-84F6-D332B2992E3F', 'XBB')
", trans);

					TestDBHelper.ExecuteNonQuery(conn, info.MergeScript, trans);
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussPackageInfo WHERE ZMP_PackageName = 'A'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussCarrierInfo WHERE ZMC_CountryCode = 'AA'", trans));
					Assert.AreEqual(0, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussPackageVersion WHERE ZMV_Version = 'AAA'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussPackageInfo WHERE ZMP_PackageName = 'B'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussCarrierInfo WHERE ZMC_CountryCode = 'XB'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussPackageVersion WHERE ZMV_Version = 'XBB'", trans));

					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussPackageInfo WHERE ZMP_PackageName = 'C'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussCarrierInfo WHERE ZMC_CountryCode = 'CC'", trans));
					Assert.AreEqual(1, TestDBHelper.ExecuteScalar(conn, @"SELECT COUNT(*) FROM RefMessagingBussPackageVersion WHERE ZMV_Version = 'CCC'", trans));
				}
			}
		}
	}
}
