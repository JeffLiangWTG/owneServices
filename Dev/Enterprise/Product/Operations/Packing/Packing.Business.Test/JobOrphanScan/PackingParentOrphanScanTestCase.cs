using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestsSubclassesOf(typeof(IPackingParentOrphanScan))]
	public abstract class PackingParentOrphanScanTestCase<T> : PackingParentTestCase<T>
		where T : IPackingParentOrphanScan, IPackingParent
	{
		/// <summary>
		/// This is a cursory test that ensures we match and save the log.
		/// Thorough testing (log content etc.) is in the PkgPackage test.
		/// </summary>
		public void TestOnSave_AddsEventsFromOrphanScanToMatchingPackage()
		{
			using (PackingRegistry.Instance.AnonymousPackageCreationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var year = ZDateTime.Now.Year;

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "SSS";

				// create orphan scan with ID "123"
				var parent = GetNewParent();
				var orphanScan = Factory.New<JobOrphanScan>();
				orphanScan.JOS_Barcode = "123";
				orphanScan.JOS_EventTimeUtc = new ZDateTime(year, 1, 1);
				orphanScan.JOS_GS_NKUser = "SSS";
				orphanScan.JOS_JobType = parent.JobType.ToString();
				orphanScan.JOS_SE_NKEvent = Events.PickedUpCode;
				Factory.Save();

				// create package job with package "123"
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parent);
				var package = packageJob.Packages.AddNew("BOX", "123");
				AssertEquals("Precondition", 0, package.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", false, orphanScan.IsDeleted);

				// save the package job -- should match the orphan scan and pull in logs
				Factory.Save();
				var packageLog = package.Logs.GetAllLogs().Cast<StmALog>().Single();
				AssertEquals("SSS", packageLog.SL_GS_NKUser);
				AssertEquals(Events.PickedUpCode, packageLog.SL_SE_NKEvent);
				AssertEquals(true, orphanScan.IsDeleted);
			}
		}
	}
}
