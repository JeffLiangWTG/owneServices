using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Packing.ServiceTasks.Testing
{
	class ProcessingManagerTest : PackingTestCaseWithFactory
	{
		#region TestPurgeOldJobOrphanScans

		[TestDate(2014, 1, 10)]
		public void TestPurgeOldJobOrphanScans()
		{
			using (EnvProxy.Instance.TemporaryServiceTaskContext("APP", canRunInAnyBranch: true))
			{
				// delete any orphans scans > 5 days old
				using (PackingRegistry.Instance.AnonymousPackagePurgeTime.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
				{
					var jobOrphanScan1 = CreateNewOrphanScan(new ZDateTime(2014, 1, 10));
					var jobOrphanScan2 = CreateNewOrphanScan(new ZDateTime(2014, 1, 6));
					var jobOrphanScan3 = CreateNewOrphanScan(new ZDateTime(2014, 1, 5));
					var jobOrphanScan4 = CreateNewOrphanScan(new ZDateTime(2014, 1, 4, 23, 59, 59));
					var jobOrphanScan5 = CreateNewOrphanScan(new ZDateTime(2014, 1, 4));
					Factory.Save();

					var logger = new TestServiceLogger();
					var processingManager = new ProcessingManager(logger);
					processingManager.PurgeOldJobOrphanScans();
					AssertOrphanScanWasDeleted(jobOrphanScan1, shouldBeDeleted: false);
					AssertOrphanScanWasDeleted(jobOrphanScan2, shouldBeDeleted: false);
					AssertOrphanScanWasDeleted(jobOrphanScan3, shouldBeDeleted: false);
					AssertOrphanScanWasDeleted(jobOrphanScan4, shouldBeDeleted: true);
					AssertOrphanScanWasDeleted(jobOrphanScan5, shouldBeDeleted: true);
					AssertEquals("Information|Successfully purged 2 Anonymous Packages.", logger.ToString().Trim());
				}
			}
		}

		void AssertOrphanScanWasDeleted(JobOrphanScan orphanScan, bool shouldBeDeleted)
		{
			var reloadedOrphanScan = new BusinessObjectFactory().Load<JobOrphanScan>(orphanScan.PK);

			if (shouldBeDeleted)
			{
				AssertNull(string.Format("Should delete an Orphan Scan that is more than 5 days old. Today is 10th Jan 2014, scan time was {0}.",
					orphanScan.JOS_EventTimeUtc.ToLongTimeString()), reloadedOrphanScan);
			}
			else
			{
				AssertNotNull(string.Format("Should not delete an Orphan Scan that is more than 5 days old. Today is 10th Jan 2014, scan time was {0}.",
					orphanScan.JOS_EventTimeUtc.ToLongTimeString()), reloadedOrphanScan);
			}
		}

		JobOrphanScan CreateNewOrphanScan(ZDateTime eventTime)
		{
			var orphanScan = Factory.New<JobOrphanScan>();
			orphanScan.JOS_Barcode = "123";
			orphanScan.JOS_EventTimeUtc = eventTime;
			orphanScan.JOS_GS_NKUser = "XYX";
			orphanScan.JOS_JobType = "DUM";
			orphanScan.JOS_SE_NKEvent = Events.PickedUpCode;

			return orphanScan;
		}

		#endregion

		#region TestPurgeOldJobOrphanScans_LogsSqlExceptions

		public void TestPurgeOldJobOrphanScans_LogsSqlExceptions()
		{
			TestConnection.ExecuteNonQuery("DROP TABLE JobOrphanScan");

			var logger = new TestServiceLogger();
			new ProcessingManager(logger).PurgeOldJobOrphanScans();
			AssertEquals("Should log SQL Exception errors.", @"
Error|Invalid object name 'dbo.JobOrphanScan'.
Information|Purging of Anonymous Packages failed.
			".Trim(), logger.ToString().Trim());
		}

		#endregion
	}
}
