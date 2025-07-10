using System;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefComplianceListUpdaterTest : OneTableDataSetUpdaterTest<RefComplianceList, IRefComplianceList>
	{
		protected override IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefComplianceListUpdater(proxy, dbHelper, versionControlManager);
		}

		protected override string ColumnForUpdate => nameof(IRefComplianceList.RCL_ListDescription);

		public override void TestInsert()
		{
			var serverData = new RefComplianceList
			{
				RCL_ListName = "Local District Offices (US)",
				RCL_ListCode = "USMARSHSERV",
				RCL_IsActive = true,
				RCL_ListDescription = "The geographical structure of the U.S. Marshal....",
				RCL_ListPublisher = "U.S. Department Service | Marshals Service",
				RCL_ListType = "Sanctions List",
				RCL_PublisherJurisdiction = "",
				RCL_PublisherDescription = "",
				RCL_IntegrationDate = new DateTime(2020, 7, 22),
				RCL_LastUpdatedDate = new DateTime(2021, 6, 30),
				RCL_MainSourceURL = "MainSourceURL",
				RCL_SecondarySourceURL = "SecondarySourceURL",
				RCL_PublishDate = new DateTime(2021, 1, 1)
			};

			dbHelper.Delete<IRefComplianceList>(null, 300);

			AssertEquals(0, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<IRefComplianceList>())));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefComplianceList WHERE RCL_ListType = 'Sanctions List' AND RCL_IsExcluded = 0"));

			var complianceListPK = conn.ExecuteScalar("SELECT RCL_PK FROM dbo.RefComplianceList WHERE RCL_ListType = 'Sanctions List' AND RCL_IsExcluded = 0").ToString();
			AssertEquals(0, conn.ExecuteScalar($"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Table = 'RefComplianceList' AND SL_Parent = '{complianceListPK}' AND SL_SE_NKEvent = 'ADD' AND SL_FireWorkflow = 1"));
		}

		public override void TestUpdate()
		{
			base.TestUpdate();
			AssertEquals(1, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefComplianceList WHERE RCL_ListType = 'Sanctions List' AND RCL_IsExcluded = 1"));
			AssertEquals(0, conn.ExecuteScalar("SELECT COUNT(*) FROM dbo.RefComplianceList WHERE RCL_ListType = 'Sanctions List' AND RCL_IsExcluded = 0"));

			var complianceListPK = conn.ExecuteScalar("SELECT RCL_PK FROM dbo.RefComplianceList WHERE RCL_ListType = 'Sanctions List' AND RCL_IsExcluded = 1").ToString();
			AssertEquals(0, conn.ExecuteScalar($"SELECT COUNT(*) FROM dbo.StmALog WHERE SL_Table = 'RefComplianceList' AND SL_Parent = '{complianceListPK}' AND SL_SE_NKEvent = 'EDT' AND SL_FireWorkflow = 1"));
		}

		protected override void PrepareData()
		{
			base.PrepareData();
			conn.ExecuteNonQuery(@"
DELETE FROM dbo.RefComplianceList;
INSERT INTO dbo.RefComplianceList([RCL_PK],[RCL_ListName],[RCL_ListCode],[RCL_IsActive],[RCL_IsSystem],[RCL_IsExcluded],[RCL_ListPublisher],[RCL_ListType],[RCL_IntegrationDate],[RCL_PublisherJurisdiction],[RCL_ListDescription],[RCL_PublisherDescription],[RCL_LastUpdatedDate], [RCL_MainSourceURL], [RCL_SecondarySourceURL])
VALUES
('70C0FAE4-2F82-461D-899F-00B74010A208', 'Local District Offices (US)', 'USMARSHSERV', 1, 1, 1, 'U.S. Department Service | Marshals Service', 'Sanctions List', '2020-07-22', '', 'The geographical structure of the U.S. Marshals Service mirrorsthe structure of United States district courts. There are 94federal judicial districts, including at least one district ineach state, the District of Columbia, the Commonwealths ofPuerto Rico and the Northern Mariana Islands and the two territoriesof the United States -- the Virgin Islands and Guam.', '', '2021-06-30', 'MainSourceURL', 'SecondarySourceURL');
");
		}
	}
}
