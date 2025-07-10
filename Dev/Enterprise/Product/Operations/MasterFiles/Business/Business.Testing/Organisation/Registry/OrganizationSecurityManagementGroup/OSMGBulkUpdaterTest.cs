using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OSMGBulkUpdater))]
	sealed class OSMGBulkUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBulkUpdateOSMGWithNoLinkedOrgMiscServ()
		{
			SetUpData();

			osmgBulkUpdater.BulkOrgSecurityGroup = osmgGroup.PK;
			osmgBulkUpdater.BulkUpdateOSMG();

			var orgMiscServs = new BusinessObjectFactory().Load<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, new ZGuid[] { org1.PK, org2.PK }));
			AssertEquals("OSMG is set to 'osmgGroup1' for organaization 1", "osmgGroup1", orgMiscServs[0].OrgSecurityGroup.GG_Desc);
			AssertEquals("OSMG is set to 'osmgGroup1' for organaization 2", "osmgGroup1", orgMiscServs[1].OrgSecurityGroup.GG_Desc);
			Assert(orgMiscServs[0].Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference == "Bulk updated OSMG: OG1"));
			Assert(orgMiscServs[1].Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference == "Bulk updated OSMG: OG1"));

			var rowsUpdated = osmgBulkUpdater.BulkUpdateOSMG();
			AssertEquals(0, rowsUpdated);
		}

		public void TestBulkUpdateOSMGWithLinkedOrgMiscServ()
		{
			SetUpData();
			LinkOrgMiscServ();

			osmgBulkUpdater.BulkOrgSecurityGroup = osmgGroup.PK;
			osmgBulkUpdater.BulkUpdateOSMG();

			orgMiscServ1.Reload();
			orgMiscServ2.Reload();

			AssertEquals("OSMG is set to 'osmgGroup1' for organaization 1", "osmgGroup1", orgMiscServ1.OrgSecurityGroup.GG_Desc);
			AssertEquals("OSMG is set to 'osmgGroup1' for organaization 2", "osmgGroup1", orgMiscServ2.OrgSecurityGroup.GG_Desc);

			Assert(orgMiscServ1.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference == "Bulk updated OSMG: OG1"));
			Assert(orgMiscServ2.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_Reference == "Bulk updated OSMG: OG1"));

			var rowsUpdated = osmgBulkUpdater.BulkUpdateOSMG();
			AssertEquals(0, rowsUpdated);
		}

		public void TestBulkUpdateOSMG_WithTransactionManager()
		{
			var osmgBulkUpdater = new OSMGBulkUpdater();
			SqlException exCatched = null;
			osmgBulkUpdater.BulkOrgSecurityGroup = Factory.LoadTop1<GlbGroup>(new ZQuery()).PK;

			using (var extraConn = Db.NewExtraConnectionToMainDb())
			using (var tranMgr = extraConn.BeginTransactionWithManager())
			{
				extraConn.ExecuteNonQuery(@"SELECT 1 FROM dbo.StmALog WITH (TABLOCKX)"); //lock

				using (Db.Connection.TemporarySetLockTimeout(1))
				{
					try
					{
						osmgBulkUpdater.BulkUpdateOSMG();
					}
					catch (SqlException ex)
					{
						exCatched = ex;
					}
				}
				tranMgr.RollbackTransaction();
			}

			AssertNotNull(exCatched);
			AssertEquals(new DbErrorMatch(exCatched).ExceptionType, DbErrorType.LockTimeoutExpired);
			AssertNoExceptionThrown(() => osmgBulkUpdater.BulkUpdateOSMG());
		}

		public void TestBulkUpdate_MultipleBatches()
		{
			SetUpData();
			osmgBulkUpdater.BulkOrgSecurityGroup = osmgGroup.PK;
			osmgBulkUpdater.BulkUpdateOSMG();
			var rowsUpdated = osmgBulkUpdater.BulkUpdateOSMG();
			AssertEquals(0, rowsUpdated);

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var logs = new List<int>();
			var updater = new OSMGBulkUpdaterForTest() { MaxRowCount_Exposed = 1, BulkOrgSecurityGroup = osmgGroup.PK };
			updater.BulkUpdateEvent += (_, args) =>
			{
				logs.Add(args.ProcessedCount);
				args.Cancel = (args.ProcessedCount == 2);
			};
			AssertEquals(2, updater.BulkUpdateOSMG());
			AssertEquals("1|2", string.Join("|", logs));

			var orgMiscServs = new BusinessObjectFactory().Load<OrgMiscServ>(new ZQuery(OrgMiscServSchema.OM_OH, new ZGuid[] { org3.PK, org4.PK, org5.PK }));
			AssertEquals(3, orgMiscServs.Length);
			AssertEquals(1, orgMiscServs.Count(x => x.OM_GG_OrgSecurityGroup.IsEmpty));
			AssertEquals(2, orgMiscServs.Count(x => x.OM_GG_OrgSecurityGroup == osmgGroup.PK));
		}

		#region Implementation

		public void SetUpData()
		{
			org1 = Factory.NewWithValidTestData<OrgHeader>();
			org2 = Factory.NewWithValidTestData<OrgHeader>();

			osmgGroup = Factory.NewWithValidTestData<GlbGroup>();
			osmgGroup.GG_Code = "OG1";
			osmgGroup.GG_Desc = "osmgGroup1";

			osmgBulkUpdater = new OSMGBulkUpdater();

			Factory.Save();
		}

		public void LinkOrgMiscServ()
		{
			orgMiscServ1 = Factory.NewWithValidTestData<OrgMiscServ>();
			orgMiscServ2 = Factory.NewWithValidTestData<OrgMiscServ>();

			orgMiscServ1.OM_OH = org1.PK;
			orgMiscServ2.OM_OH = org2.PK;

			Factory.Save();
		}

		OrgHeader org1;
		OrgHeader org2;
		GlbGroup osmgGroup;
		OSMGBulkUpdater osmgBulkUpdater;
		OrgMiscServ orgMiscServ1;
		OrgMiscServ orgMiscServ2;

		public class OSMGBulkUpdaterForTest : OSMGBulkUpdater
		{
			public int MaxRowCount_Exposed { get; set; } = 1000;
			protected override int MaxRowCount => MaxRowCount_Exposed;
		}
		#endregion
	}
}
