using System.Linq;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageProcessHandlingInfoTest : PackingTestCaseWithFactory
	{
		#region TestPopulateCascadingTargets

		public void TestPopulateCascadingTargets()
		{
			var handlingInfo = GetNewProcessHandlingInfo();

			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ArrivalCode;
			}

			AssertEquals(Enumerable.Empty<CascadingLink>(), handlingInfo.GetCascadingTargets(eventLog));
		}

		#endregion

		#region TestPropagationTargets

		public void TestPropagationTargets()
		{
			Data.CreatePackingData();

			var dummyWorkflowItem = Data.Dummy.WorkflowItems.AddNew();
			dummyWorkflowItem.P9_Type = Constants.Workflow.WorkflowTriggerType;
			dummyWorkflowItem.TriggerConditions.TriggerEventCode = Events.FreightLoadedCode;

			var outer1 = Data.PackageJob.Packages.AddNew();
			var outer2 = Data.PackageJob.Packages.AddNew();
			var inner1 = outer1.Packages.AddNew();
			var inner2 = outer2.Packages.AddNew();

			// only 1 of 2 *outers* has the event, should not propagate to the dummy
			var package1Log = outer1.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("Only 1 of 2 outers has the event, should not propagate to the parent dummy.", true, dummyWorkflowItem.P9_ActualDate.IsEmpty);

			// 1 of 2 *inners* has the event, however inners should not be considered for propogation
			inner2.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("Only 1 of 2 outers has the event, should not propagate to the parent dummy.", true, dummyWorkflowItem.P9_ActualDate.IsEmpty);

			// all outers have the event, should propagate even though not all inners have the event
			var package2Log = outer2.Logs.AddNew(Events.FreightLoaded);
			AssertEquals("All Outers have the FLB event, should propagate to the parent dummy.", package2Log.SL_EventTime, dummyWorkflowItem.P9_ActualDate.ToZDateTime());
		}

		#endregion

		#region TestPropagatePackingCompleteEventToTargets

		public void TestPropagatePackingCompleteEventToTargets()
		{
			Data.CreatePackingData();

			var dummyWorkflowItem = Data.Dummy.WorkflowItems.AddNew();
			dummyWorkflowItem.P9_Type = Constants.Workflow.WorkflowTriggerType;
			dummyWorkflowItem.TriggerConditions.TriggerEventCode = Events.PackingCompletedCode;

			var outerPackage1 = Data.PackageJob.Packages.AddNew();
			var outerPackage2 = Data.PackageJob.Packages.AddNew();

			// only 1 of 2 *outers* has the event, should not propagate to the dummy
			var package1Log = outerPackage1.Logs.AddNew(Events.PackingCompleted);
			AssertEquals("Only 1 of 2 outers has the event, should not propagate to the parent dummy.", true, dummyWorkflowItem.P9_ActualDate.IsEmpty);

			// all outers have the event, should propagate even though not all inners have the event
			var package2Log = outerPackage2.Logs.AddNew(Events.PackingCompleted);
			AssertEquals("All Outers have the PKC event, should propagate to the parent dummy.", package2Log.SL_EventTime, dummyWorkflowItem.P9_ActualDate.ToZDateTime());
		}

		#endregion

		#region TestPropagationTargets_WithInvalidParent

		public void TestPropagationTargets_WithInvalidParent()
		{
			var packageWithNoPackageJob = Factory.New<PkgPackage>();
			// no packagejob
			AssertEquals("No PackageJob.", 0, new PkgPackageProcessHandlingInfo(packageWithNoPackageJob).GetPropagationTargets(packageWithNoPackageJob.Logs.AddNew(Events.FreightLoaded)).Count());

			// deleted package
			var packageJob = Factory.New<PkgPackageJob>();
			var deletedPackage = packageJob.Packages.AddNew();
			var logFromDeletedPackage = deletedPackage.Logs.AddNew(Events.FreightLoaded);
			deletedPackage.Delete();
			AssertEquals("Package is deleted.", 0, new PkgPackageProcessHandlingInfo(deletedPackage).GetPropagationTargets(logFromDeletedPackage).Count());

			// no parent job (eg. Dummy)
			var package = packageJob.Packages.AddNew();
			AssertEquals("Package has no ParentJob (via PackageJob.ParentJob).", 0, new PkgPackageProcessHandlingInfo(package).GetPropagationTargets(package.Logs.AddNew(Events.FreightLoaded)).Count());
		}

		#endregion

		#region Implementation

		PkgPackageProcessHandlingInfo GetNewProcessHandlingInfo()
		{
			return new PkgPackageProcessHandlingInfo(Factory.NewWithValidTestData<PkgPackage>());
		}

		#endregion
	}
}
