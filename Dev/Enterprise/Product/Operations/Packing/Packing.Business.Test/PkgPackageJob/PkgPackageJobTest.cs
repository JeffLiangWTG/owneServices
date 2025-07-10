using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageJob))]
	public class PkgPackageJobTest : PackingBusinessObjectTestCase
	{
		#region TestFetchForLoad_MultiplePackagesInPackageJob

		public void TestFetchForLoad_MultiplePackagesInPackageJob()
		{
			var dummyBizOWithPackageJob1 = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob1);//var packageJob1 = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob1.Packages.AddNew();
			packageJob1.Packages.AddNew();
			packageJob1.Packages.AddNew();
			packageJob1.Packages.AddNew().Packages.AddNew();
			packageJob1.Packages.AddNew().Packages.AddNew();
			packageJob1.Packages.AddNew().Packages.AddNew().Packages.AddNew();

			var dummyBizOWithPackageJob2 = Factory.New<DummyWithPacking>();
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyBizOWithPackageJob2); //var packageJob2 = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob2.Packages.AddNew();
			packageJob2.Packages.AddNew();
			packageJob2.Packages.AddNew();
			packageJob2.Packages.AddNew().Packages.AddNew();
			packageJob2.Packages.AddNew().Packages.AddNew();
			packageJob2.Packages.AddNew().Packages.AddNew().Packages.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var packageJob1InNewFactory = PkgPackageJob.LoadPackageJob(newFactory, packageJob1.PK);
			var packageJob2InNewFactory = PkgPackageJob.LoadPackageJob(newFactory, packageJob2.PK);
			PokeAllPackages(packageJob1InNewFactory.Packages);
			PokeAllPackages(packageJob2InNewFactory.Packages);
			var expetedDbHits = new Dictionary<string, int>
			{
				{ PkgPackageSchema.Constants.TableName, 2 },
				{ PkgPackageJobSchema.Constants.TableName, 2 },
				{ DummyBizoSchema.Constants.TableName,  2 }
			};
			AssertDbHits(expetedDbHits, newFactory);
		}

		void PokeAllPackages(PkgPackageCollection packages)
		{
			foreach (var package in packages)
			{
				var poke = package.PK;
				PokeAllPackages(package.Packages);
			}
		}

		#endregion

		#region Static Loader

		#region TestLoadPackageJob

		public void TestLoadPackageJob_ViaParent()
		{
			var dummy = Factory.New<DummyWithPacking>();

			// create a new job
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			packageJob.KJ_IsFinalized = true;
			packageJob.ReadOnly = false;
			AssertEquals("Precondition", false, packageJob.ReadOnly);
			AssertEquals(1, dummy.OnPackageJobCreatedOrLoadedCount);

			// load the existing (newly created) job
			dummy.UnRegisterEditableChildObject(packageJob);
			AssertEquals("The existing PackageJob was not loaded.", packageJob, PkgPackageJob.LoadPackageJob(dummy));
			AssertEquals("The PackageJob was not registered as editable on the parent (Dummy).", true, dummy.IsRegisteredEditableChildObject(packageJob));
			AssertEquals("ReadOnly should be updated on load.", true, packageJob.ReadOnly);
		}

		public void TestLoadPackageJob_ViaPK()
		{
			var dummy = Factory.New<DummyWithPacking>();

			// create a new job
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			packageJob.KJ_IsFinalized = true;
			packageJob.ReadOnly = false;
			AssertEquals("Precondition", false, packageJob.ReadOnly);

			// load the existing (newly created) job
			AssertEquals("The existing PackageJob was not loaded.", packageJob, PkgPackageJob.LoadPackageJob(Factory, packageJob.PK));
			AssertEquals("ReadOnly should be updated on load.", true, packageJob.ReadOnly);
		}

		#endregion

		#region TestLoadOrCreatePackageJob

		public void TestLoadOrCreatePackageJob()
		{
			AssertLoadOrCreatePackageJob(d => PkgPackageJob.LoadOrCreatePackageJob(d), expectHasChangesOnCreate: true);
		}

		public void TestLoadOrCreatePackageJobWithNoChanges()
		{
			AssertLoadOrCreatePackageJob(d => PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(d), expectHasChangesOnCreate: false);
		}

		void AssertLoadOrCreatePackageJob(Func<IPackingParent, PkgPackageJob> loader, bool expectHasChangesOnCreate)
		{
			var dummy = Factory.New<DummyWithPacking>();
			dummy.IsParentJobFinalised = true;

			// create a new job
			var packageJob = loader(dummy);
			AssertEquals("OnPackageJobCreatedOrLoadedCount must be called once when creating package job.", 1, dummy.OnPackageJobCreatedOrLoadedCount);
			AssertEquals("The PackageJob was not linked to the parent (Dummy).", dummy, packageJob.ParentJob);
			AssertEquals("The PackageJob was not registered as editable on the parent (Dummy).", true, dummy.IsRegisteredEditableChildObject(packageJob));
			AssertEquals("PackageJob.HasChanges on create is incorrect.", expectHasChangesOnCreate, packageJob.HasChanges);
			AssertEquals("KJ_IsFinalized should be set from the parent job.", true, packageJob.KJ_IsFinalized);
			AssertEquals("KJ_IsFinalized should be set from the parent job and update ReadOnly.", true, packageJob.ReadOnly);

			// load the existing (newly created) job
			packageJob.ReadOnly = false;
			dummy.UnRegisterEditableChildObject(packageJob);
			AssertEquals("The existing PackageJob was not loaded.", packageJob, loader(dummy));
			AssertEquals("OnPackageJobCreatedOrLoadedCount must be called once more when loading package job.", 2, dummy.OnPackageJobCreatedOrLoadedCount);
			AssertEquals("The PackageJob was not registered as editable on the parent (Dummy).", true, dummy.IsRegisteredEditableChildObject(packageJob));
			AssertEquals("ReadOnly should be updated on load.", true, packageJob.ReadOnly);

			// save then reload in another factory
			packageJob.Packages.AddNew(); // empty job would be deleted on save
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var dummyInOtherFactory = otherFactory.Load<DummyWithPacking>(dummy.PK);
			var packageJobInOtherFactory = loader(dummyInOtherFactory);
			AssertEquals(packageJob.PK, packageJobInOtherFactory.PK);
		}

		#endregion

		#region TestLoadParent

		public void TestLoadParent()
		{
			var dummy = Factory.New<DummyWithPacking>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			var dummyFromLoader = PkgPackageJob.LoadParent<IPackingParent>(packageJob);
			AssertEquals("Should have loaded the PackageJob's parent.", dummy, dummyFromLoader);

			// ensure subsequent loads do not hit the DB
			int dbHitCount = Factory.DatabaseLoadCount;
			PkgPackageJob.LoadParent<IPackingParent>(packageJob);
			PkgPackageJob.LoadParent<IPackingParent>(packageJob);
			AssertEquals("Subsequent access to LoadParent(packageJob) should not hit the DB.", dbHitCount, Factory.DatabaseLoadCount);

			var packageJobWithNoParent = Factory.New<PkgPackageJob>();
			AssertNull("Should not die in the arse when no parent exists.", PkgPackageJob.LoadParent<IPackingParent>(packageJobWithNoParent));
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestParentJob

		public void TestParentJob()
		{
			Data.CreatePackingData();

			var orphanPackageJob = Factory.New<PkgPackageJob>();
			AssertNull(orphanPackageJob.ParentJob);

			var packageJob = Data.PackageJob;
			AssertEquals(Data.Dummy, packageJob.ParentJob);

			// ensure the parent is cached
			int dbHitCount = Factory.DatabaseLoadCount;
			var hit1 = packageJob.ParentJob;
			var hit2 = packageJob.ParentJob;
			AssertEquals("Subsequent access to PkgPackageJob.ParentJob should not hit the DB.", dbHitCount, Factory.DatabaseLoadCount);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			AssertEquals(packageJob.Packages.Count, 0);
			AssertEquals(true, packageJob.IsRegisteredEditableChildObject(packageJob.Packages));

			var package1 = packageJob.Packages.AddNew();
			var package2 = packageJob.Packages.AddNew();
			var childPackage1 = package1.Packages.AddNew(); // this should not be found in the collection
			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { package1, package2 }, packageJob.Packages);
		}

		#endregion

		#region TestShouldReloadPackagesCollection

		public void TestShouldReloadPackagesCollection()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var oldPackages = packageJob.Packages;

			packageJob.ShouldReloadPackagesCollection = true;

			AssertNotEquals("Packages should be a new collection object.", packageJob.Packages, oldPackages);
			AssertEquals("ShouldReloadPackagesCollection should have been reset to false.", false, packageJob.ShouldReloadPackagesCollection);
		}

		#endregion

		#region TestAllPackages

		public void TestAllPackages()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1a = container1.Packages.AddNew("PLT");
			var pallet1b = container1.Packages.AddNew("PLT");
			var box1a = pallet1a.Packages.AddNew("BOX");
			var ctn1a = box1a.Packages.AddNew("CTN");

			var container2 = Data.PackageJob.Packages.AddNew("CNT");

			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { container1, container2, pallet1a, pallet1b, box1a, ctn1a }, Data.PackageJob.GetAllPackagesOnJob());
		}

		#endregion

		#region TestAllHeldPackages

		public void TestAllHeldPackages()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1a = container1.Packages.AddNew("PLT");
			var pallet1b = container1.Packages.AddNew("PLT");
			pallet1b.KP_IsHeld = true;
			var box1a = pallet1a.Packages.AddNew("BOX");
			var ctn1a = box1a.Packages.AddNew("CTN");
			ctn1a.KP_IsHeld = true;

			var container2 = Data.PackageJob.Packages.AddNew("CNT");

			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { pallet1b, ctn1a }, Data.PackageJob.AllHeldPackages);
		}

		#endregion

		#region TestLoosePackageIDs

		public void TestLoosePackageIDs()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			AssertEquals(0, packageJob.LoosePackageIDs.Count);

			Data.PackageJob.Packages.AddNew("CNT");
			AssertEquals(0, packageJob.LoosePackageIDs.Count);

			var cnt = Data.PackageJob.Packages.AddNew("CNT", "XYZ");
			AssertEquals(0, packageJob.LoosePackageIDs.Count);

			var newPackageID = Data.PackageJob.LoosePackageIDs.AddNew();
			AssertEquals(1, packageJob.LoosePackageIDs.Count);

			newPackageID.KPH_PackageID = "ABC";
			AssertEquals("ABC", packageJob.LoosePackageIDs.First().KPH_PackageID);

			Assert(packageJob.IsRegisteredEditableChildObject(packageJob.LoosePackageIDs));
		}

		#endregion

		#region TestLoosePackagePivots

		public void TestLoosePackagePivots()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			AssertEquals(0, packageJob.LoosePackagePivots.Count);

			Data.PackageJob.Packages.AddNew("CNT");
			AssertEquals(0, packageJob.LoosePackagePivots.Count);

			var cnt = Data.PackageJob.Packages.AddNew("CNT", "XYZ");
			AssertEquals(0, packageJob.LoosePackagePivots.Count);

			var newPackageID = Data.PackageJob.LoosePackagePivots.AddNew();
			AssertEquals(1, packageJob.LoosePackagePivots.Count);

			newPackageID.PackageHeader.KPH_PackageID = "ABC";
			AssertEquals("ABC", packageJob.LoosePackagePivots.First().PackageHeader.KPH_PackageID);

			Assert(packageJob.IsRegisteredEditableChildObject(packageJob.LoosePackagePivots));
		}

		#endregion

		#region TestGetAllNonContainerOutersAndFirstLevelPackagesOnContainers

		public void TestGetAllNonContainerOutersAndFirstLevelPackagesOnContainers()
		{
			Data.CreatePackingData();

			// top-level
			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var container2 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet2 = Data.PackageJob.Packages.AddNew("PLT");

			var pallet1a = container1.Packages.AddNew("PLT");
			var pallet1b = container1.Packages.AddNew("PLT");
			var box1a = pallet1a.Packages.AddNew("BOX");

			AssertContainsExactElementsInAnyOrder(new PkgPackage[] { pallet2, pallet1a, pallet1b, }, Data.PackageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers());
		}

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			AssertEquals(typeof(PkgPackageCollectionContainersOnly), Factory.New<PkgPackageJob>().Containers.GetType());
		}

		#endregion

		#region TestPackableItemParents

		public void TestPackableItemParents()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			AssertEquals("Collection should be cached.", Data.PackageJob.PackableItemParents, Data.PackageJob.PackableItemParents);

			var itemsFromWrappers = Data.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>().Select(w => w.PackableItemParent);
			AssertContainsExactElementsInAnyOrder(itemsFromWrappers, new IPackableItemParent[] { Data.DummyLine1, Data.DummyLine2, Data.DummyLine3 });
		}

		public void TestPackableItemParents_WhenNoParent()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			AssertEquals(0, packageJob.PackableItemParents.Count);
		}

		#endregion

		#region TestIsParentJobPackingParentWithPackableItems

		public void TestIsParentJobPackingParentWithPackableItems()
		{
			var dummyPackingParentWithPackableItems = Factory.New<DummyWithPacking>();
			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParentWithPackableItems);
			AssertEquals("Precondition", dummyPackingParentWithPackableItems, packageJob1.ParentJob);
			AssertEquals("Precondition: dummyPackingParentWithPackableItems implements IPackingParentWithPackableItems.", true, dummyPackingParentWithPackableItems is IPackingParentWithPackableItems);
			AssertEquals(true, packageJob1.IsParentJobPackingParentWithPackableItems);

			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParent); // to load DummyPackingParent object during factory load for packing job's parent job
			var dummyPackingParent = Factory.New<DummyPackingParent>();
			var packageJob2 = PkgPackageJob.LoadOrCreatePackageJob(dummyPackingParent);
			AssertEquals("Precondition", dummyPackingParent, packageJob2.ParentJob);
			AssertEquals("Precondition: dummyPackingParent does not implement IPackingParentWithPackableItems.", false, dummyPackingParent is IPackingParentWithPackableItems);
			AssertEquals(false, packageJob2.IsParentJobPackingParentWithPackableItems);
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region KJ_IsFinalized

		public void TestKJ_IsFinalized()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var isPackingReadOnlyChangedCount = 0;
			packageJob.IsFinalisedChanged += delegate
			{ isPackingReadOnlyChangedCount++; };
			AssertEquals("Precondition", 0, isPackingReadOnlyChangedCount);
			AssertEquals("Precondition", false, packageJob.KJ_IsFinalized);
			AssertEquals("Precondition", false, packageJob.ReadOnly);
			AssertEquals("Precondition", false, packageJob.Packages.ReadOnly);

			packageJob.KJ_IsFinalized = true;
			AssertEquals(1, isPackingReadOnlyChangedCount);
			AssertEquals(true, packageJob.KJ_IsFinalized);
			AssertEquals(true, packageJob.ReadOnly);
			AssertEquals(true, packageJob.Packages.ReadOnly);

			packageJob.KJ_IsFinalized = false;
			AssertEquals(2, isPackingReadOnlyChangedCount);
			AssertEquals(false, packageJob.KJ_IsFinalized);
			AssertEquals(false, packageJob.ReadOnly);
			AssertEquals(false, packageJob.Packages.ReadOnly);

			packageJob.KJ_IsFinalized = false;
			AssertEquals("Setting IsFinalised to it's current value should not fire change event.", 2, isPackingReadOnlyChangedCount);
		}

		public void TestPackagesClosedOnFinalize()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var outer1 = packageJob.Packages.AddNew();
			var outer2 = packageJob.Packages.AddNew();
			var inner1 = outer1.Packages.AddNew();

			AssertEquals("Precondition", false, packageJob.KJ_IsFinalized);
			AssertEquals("Precondition", false, outer1.IsClosed);
			AssertEquals("Precondition", false, outer2.IsClosed);
			AssertEquals("Precondition", false, inner1.IsClosed);

			packageJob.KJ_IsFinalized = true;
			AssertEquals(true, packageJob.KJ_IsFinalized);
			AssertEquals(true, outer1.IsClosed);
			AssertEquals(true, outer2.IsClosed);
			AssertEquals("Only outer packages are to be forced to close", false, inner1.IsClosed);
		}

		public void TestSuspendClosingPackagesOnFinalize_PreventsPackagesClosedOnFinalize()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			var outer1 = packageJob.Packages.AddNew();
			var outer2 = packageJob.Packages.AddNew();
			var inner1 = outer1.Packages.AddNew();

			AssertEquals("Precondition", false, packageJob.KJ_IsFinalized);
			AssertEquals("Precondition", false, outer1.IsClosed);
			AssertEquals("Precondition", false, outer2.IsClosed);
			AssertEquals("Precondition", false, inner1.IsClosed);

			using (packageJob.SuspendClosingPackagesOnFinalize())
			{
				packageJob.KJ_IsFinalized = true;
			}

			AssertEquals(true, packageJob.KJ_IsFinalized);
			AssertEquals(false, outer1.IsClosed);
			AssertEquals(false, outer2.IsClosed);
			AssertEquals(false, inner1.IsClosed);
		}

		public void TestPackagesClosedOnFinalize_ScalesLinearly()
		{
			var propertiesHit10 = RunFinalize(10);
			var propertiesHit1000 = RunFinalize(1000);

			var ratio = (propertiesHit1000 / propertiesHit10);
			AssertLessThanOrEqualTo("Expected ratio of calling with 100x elements to be roughly 100.", ratio, 125);

			int RunFinalize(int n)
			{
				return GetPersistentPropertiesHitCount(() =>
				{
					var parent = Factory.New<DummyWithPacking>();
					var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parent);

					for (int i = 0; i < n; i++)
					{
						packageJob.Packages.AddNew();
					}

					packageJob.KJ_IsFinalized = true;
				});
			}
		}

		public void TestKJ_IsFinalizedInfo_ConcurrencyPolicy()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			AssertEquals(ConcurrencyPolicy.Strict, job.KJ_IsFinalizedInfo.ConcurrencyPolicy);
		}

		public void TestKJ_IsFinalized_KJ_SetCriticalChangesVersionIDInfoConcurrencyPolicyToStrict()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			AssertEquals(ConcurrencyPolicy.Ignore, job.KJ_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			// Finalize the job
			job.KJ_IsFinalized = true;

			// Verify the ConcurrencyPolicy is set to Strict
			AssertEquals(ConcurrencyPolicy.Strict, job.KJ_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		public void TestKJ_IsFinalized_KJ_CriticalChangesVersionIDSetToEmptyAfterFinalized()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			job.KJ_CriticalChangesVersionID = ZGuid.NewZGuid();

			job.KJ_IsFinalized = true;

			AssertEquals(ZGuid.Empty, job.KJ_CriticalChangesVersionID);
		}

		#endregion

		#region TestKJ_ReleasedTimeUtc

		#region TestKJ_ReleasedTimeUtc

		public void TestKJ_ReleasedTimeUtc()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var pallet1 = packageJob.Packages.AddNew();
			var pallet2 = packageJob.Packages.AddNew();
			var pallet3 = packageJob.Packages.AddNew();
			var box1 = pallet1.Packages.AddNew();

			pallet1.KP_ReleasedTimeUtc = ZDateTime.UtcNow; // directly release pallet1 (this is akin to a user scanning the package for rls)
			pallet2.KP_ClosedTimeUtc = ZDateTime.UtcNow;   // close pallet 2

			AssertPackageFlags("Precondition", pallet1, isClosed: false, isReleasedViaJob: false, isReleased: true);
			AssertPackageFlags("Precondition", pallet2, isClosed: true, isReleasedViaJob: false, isReleased: false);
			AssertPackageFlags("Precondition", pallet3, isClosed: false, isReleasedViaJob: false, isReleased: false);
			AssertPackageFlags("Precondition", box1, isClosed: false, isReleasedViaJob: false, isReleased: false);

			// release the job (this is akin to a user scanning the job for rls)
			packageJob.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertPackageFlags("Releasing a job should release all outer packages.", pallet1, isClosed: false, isReleasedViaJob: true, isReleased: true);
			AssertPackageFlags("Releasing a job should release all outer packages.", pallet2, isClosed: true, isReleasedViaJob: true, isReleased: false);
			AssertPackageFlags("Releasing a job should release all outer packages.", pallet3, isClosed: false, isReleasedViaJob: true, isReleased: false);
			AssertPackageFlags("Releasing a job should not release inner packages.", box1, isClosed: false, isReleasedViaJob: false, isReleased: false);
			AssertEquals("KJ_GS_NKReleasedBy is not empty after setting the released time.", false, packageJob.KJ_GS_NKReleasedBy.IsEmpty);
			AssertEquals("KJ_GS_NKReleasedBy is not empty after setting the released time.", true, packageJob.KJ_IsReleased);

			// cancel the release
			packageJob.KJ_ReleasedTimeUtc = ZDateTime.Empty;
			AssertPackageFlags("Cancelling a release should not modify other status flags.", pallet1, isClosed: false, isReleasedViaJob: false, isReleased: false);
			AssertPackageFlags("Cancelling a release should not modify other status flags.", pallet2, isClosed: true, isReleasedViaJob: false, isReleased: false);
			AssertPackageFlags("Cancelling a release should not modify other status flags.", pallet3, isClosed: false, isReleasedViaJob: false, isReleased: false);
			AssertPackageFlags("Cancelling a release should not modify other status flags.", box1, isClosed: false, isReleasedViaJob: false, isReleased: false);
			AssertEquals("KJ_GS_NKReleasedBy is empty after setting the released time to nothing.", true, packageJob.KJ_GS_NKReleasedBy.IsEmpty);
			AssertEquals("KJ_GS_NKReleasedBy is empty after setting the released time to nothing.", false, packageJob.KJ_IsReleased);
		}

		void AssertPackageFlags(string message, PkgPackage package, bool isClosed, bool isReleasedViaJob, bool isReleased)
		{
			AssertEquals(message, isClosed, package.IsClosed);
			AssertEquals(message, isReleasedViaJob, package.KP_IsReleasedViaJob);
			AssertEquals(message, isReleased, package.IsReleased);
		}

		public void TestKJ_ReleasedTimeAuditColumnsReadOnly()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			Assert("Released Audit Columns should be readonly.", packageJob.KJ_ReleasedTimeUtcInfo.ReadOnly);
			Assert("Released Audit Columns should be readonly.", packageJob.KJ_GS_NKReleasedByInfo.ReadOnly);
			Assert("Released Audit Columns should be readonly.", packageJob.KJ_IsReleasedInfo.ReadOnly);
		}

		#endregion

		#region TestKJ_ReleasedTimeUtc_FiresReleaseEvent

		public void TestKJ_ReleasedTimeUtc_FiresReleaseEvent()
		{
			var eventQuery = new ZQuery();
			eventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.Released.Code);
			eventQuery.AddToFilter(StmALogSchema.SL_Reference, "Released");

			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			AssertEquals("Precondition", false, packageJob.Logs.Find(eventQuery).Any());
			AssertEquals("Precondition", 0, Data.Dummy.OnPackageJobReleasedFiredCount);

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals(1, packageJob.Logs.Find(eventQuery).Length);
			AssertEquals(1, Data.Dummy.OnPackageJobReleasedFiredCount);

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Releasing the packages a second time should create another Release Event.", 2, packageJob.Logs.Find(eventQuery).Length);
			AssertEquals(2, Data.Dummy.OnPackageJobReleasedFiredCount);

			packageJob.Packages.AddNew().KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Precondition - Adding and releasing a new package will create another Release Event.", 3, Data.Dummy.OnPackageJobReleasedFiredCount);
			AssertEquals("Precondition", 3, Data.Dummy.OnPackageJobReleasedFiredCount);

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.Empty;
			AssertEquals("All packages were released directly, 'un-releasing' the job should *not* create a Release Event.", 3, packageJob.Logs.Find(eventQuery).Length);
			AssertEquals(3, Data.Dummy.OnPackageJobReleasedFiredCount);
		}

		#endregion

		#region TestFirePackageJobReleasedIfNecessary

		public void TestFirePackageJobReleasedIfNecessary()
		{
			var eventQuery = new ZQuery();
			eventQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.Released.Code);
			eventQuery.AddToFilter(StmALogSchema.SL_Reference, "Released");

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.FirePackageJobReleasedIfNecessary();
			AssertEquals("No packages exist - should *not* fire RLS event.", false, packageJob.Logs.Find(eventQuery).Any());

			// attempt to release an inner (outers are *not* all released)
			var pallet = packageJob.Packages.AddNew("PLT");
			var box = pallet.Packages.AddNew();
			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Releasing an inner - should *not* fire RLS event.", false, packageJob.Logs.Find(eventQuery).Any());
			box.KP_ReleasedTimeUtc = ZDateTime.Empty; // cleanup

			// release the outers
			pallet.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("All packages are Released - should fire RLS event.", 1, packageJob.Logs.Find(eventQuery).Length);

			// re-release the outers
			pallet.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("No change to Outers - should *not* fire RLS event.", 1, packageJob.Logs.Find(eventQuery).Length);

			// again attempt to release an inner (outers *are* released)
			box.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Releasing an inner - should *not* re-fire RLS event.", 1, packageJob.Logs.Find(eventQuery).Length);

			// add another outer then release it
			var pallet2 = packageJob.Packages.AddNew();
			pallet2.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("All packages are Released - should fire RLS event.", 2, packageJob.Logs.Find(eventQuery).Length);
		}

		#endregion

		#endregion

		#region TestWeight

		#region TestWeight_InvalidTargetWeightUQ

		public void TestWeight_InvalidTargetWeightUQ()
		{
			Data.CreatePackingData();
			var package_ValidWeightUQ = Data.PackageJob.Packages.AddNew();
			package_ValidWeightUQ.KP_Weight = 1000m;
			package_ValidWeightUQ.KP_WeightUQ = Constants.Weight.Grams;

			using (PackingRegistry.Instance.WeightUnit.DataType.SuspendValidation())
			{
				PackingRegistry.Instance.SetWeightUnitForTest("6D031A14-24A0-4084-BD61-00863B");
			}

			AssertNoExceptionThrown("Invalid PackageJob Target WeightUQ should have 0 as weight ", () => { AssertEquals(0m, Data.PackageJob.Weight); });
		}

		#endregion

		public void TestWeight()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew();
			var box = Data.PackageJob.Packages.AddNew();
			var bag = box.Packages.AddNew();

			container.KP_Weight = 500m;
			container.KP_WeightUQ = Constants.Weight.Kilograms;

			box.KP_Weight = 1000m;
			box.KP_WeightUQ = Constants.Weight.Grams;

			bag.KP_Weight = 1m;
			bag.KP_WeightUQ = Constants.Weight.Kilograms;

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);
			AssertEquals(502m, Data.PackageJob.Weight);

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Pounds);
			AssertEquals(1106.720556m, Data.PackageJob.Weight);

			// try setting bad values, ensure they are not added to the total
			box.KP_WeightUQ = "XX";
			AssertEquals(1102.311311m, Data.PackageJob.Weight);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			Data.CreatePackingData();
			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Kilograms);
			AssertEquals(Constants.Weight.Kilograms, Data.PackageJob.WeightUQ);

			PackingRegistry.Instance.SetWeightUnitForTest(Constants.Weight.Pounds);
			AssertEquals(Constants.Weight.Pounds, Data.PackageJob.WeightUQ);
		}

		#endregion

		#region TestVolume

		public void TestVolumn_InvalidTargetVolumnUQ()
		{
			Data.CreatePackingData();
			var package_InvalidVolumnUQ = Data.PackageJob.Packages.AddNew();
			package_InvalidVolumnUQ.KP_Volume = 1000m;
			package_InvalidVolumnUQ.KP_VolumeUQ = Constants.Volume.CubicFeet;

			using (PackingRegistry.Instance.VolumeUnit.DataType.SuspendValidation())
			{
				PackingRegistry.Instance.SetVolumeUnitForTest("6D031A14-24A0-4084-BD61-00863B");
			}

			AssertNoExceptionThrown("Invalid PackageJob Target VolumnUQ should have 0 as volumn ", () => { AssertEquals(0m, Data.PackageJob.Volume); });
		}

		public void TestVolume()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew();
			var box = Data.PackageJob.Packages.AddNew();
			var bag = box.Packages.AddNew();

			container.KP_Volume = 500m;
			container.KP_VolumeUQ = Constants.Volume.CubicMetres;

			box.KP_Volume = 1000m;
			box.KP_VolumeUQ = Constants.Volume.CubicDecimetres;

			bag.KP_Volume = 1m;
			bag.KP_VolumeUQ = Constants.Volume.CubicMetres;

			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicMetres);
			AssertEquals(501m, Data.PackageJob.Volume);

			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicFeet);
			AssertEquals(17692.648023m, Data.PackageJob.Volume);

			// try setting bad values, ensure they are not added to the total
			box.KP_VolumeUQ = "XX";
			AssertEquals(17657.333356m, Data.PackageJob.Volume);
		}

		#endregion

		#region TestVolumeUQ

		public void TestVolumeUQ()
		{
			Data.CreatePackingData();
			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicMetres);
			AssertEquals(Constants.Volume.CubicMetres, Data.PackageJob.VolumeUQ);

			PackingRegistry.Instance.SetVolumeUnitForTest(Constants.Volume.CubicFeet);
			AssertEquals(Constants.Volume.CubicFeet, Data.PackageJob.VolumeUQ);
		}

		#endregion

		#region TestContents

		public void TestContents()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew();
			var box = Data.PackageJob.Packages.AddNew();
			var bag = box.Packages.AddNew();

			AssertEquals("2x PLT", Data.PackageJob.Contents);
		}

		#endregion

		// calculated

		#region TestLastUsedOuterPackType

		public void TestLastUsedOuterPackType()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			PackingRegistry.Instance.SetOuterPackageUnitForTest(Constants.PkgUnit.Pallet);
			AssertEquals(Constants.PkgUnit.Pallet, packageJob.LastUsedOuterPackType);

			packageJob.LastUsedOuterPackType = Constants.PkgUnit.Box;
			AssertEquals(Constants.PkgUnit.Box, packageJob.LastUsedOuterPackType);
		}

		#endregion

		#region TestLastUsedInnerPackType

		public void TestLastUsedInnerPackType()
		{
			var packageJob = Factory.New<PkgPackageJob>();

			PackingRegistry.Instance.SetInnerPackageUnitForTest(Constants.PkgUnit.Carton);
			AssertEquals(Constants.PkgUnit.Carton, packageJob.LastUsedInnerPackType);

			packageJob.LastUsedInnerPackType = Constants.PkgUnit.Keg;
			AssertEquals(Constants.PkgUnit.Keg, packageJob.LastUsedInnerPackType);
		}

		#endregion

		#region TestPackageIDSequenceCalculator

		public void TestPackageIDSequenceCalculator()
		{
			Data.CreatePackingData();

			AssertNotNull(Data.PackageJob.PackageSequenceCalculator);
		}

		#endregion

		#endregion

		#region Flags

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			AssertEquals(true, Factory.New<PkgPackageJob>().IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestIsPackageIdValid

		public void TestIsPackageIdValid()
		{
			Data.CreatePackingData();

			var packageWithUniqueID = Data.PackageJob.Packages.AddNew();
			var packageWithNoID = Data.PackageJob.Packages.AddNew();
			var packageWithDuplicateID1 = packageWithUniqueID.Packages.AddNew();
			var packageWithDuplicateID2 = packageWithNoID.Packages.AddNew();

			packageWithUniqueID.KP_PackageID = "CNT-1";
			packageWithNoID.KP_PackageID = "";
			packageWithDuplicateID1.KP_PackageID = "PLT-1";
			packageWithDuplicateID2.KP_PackageID = "plt-1";

			AssertEquals("Unique Package ID should be valid.", true, Data.PackageJob.IsPackageIdValid(packageWithUniqueID));
			AssertEquals("Empty Package ID should be valid.", true, Data.PackageJob.IsPackageIdValid(packageWithNoID));
			AssertEquals("Duplicate Package ID should be invalid.", false, Data.PackageJob.IsPackageIdValid(packageWithDuplicateID1));
			AssertEquals("Duplicate Package ID should be invalid.", false, Data.PackageJob.IsPackageIdValid(packageWithDuplicateID2));
		}

		#endregion

		#region TestShowBasicLabelOnly

		public void TestShowBasicLabelOnly()
		{
			Data.CreatePackingData();
			AssertEquals("Must be a ZBool so that Document Customisation can see the property.", typeof(ZBool), Data.PackageJob.ShowBasicLabelOnly.GetType());

			Data.Dummy.DocumentOptions = DocumentOptions.None;
			AssertEquals("With the default DocumentOptions, all Documents should be shown.", false, Data.PackageJob.ShowBasicLabelOnly);

			Data.Dummy.DocumentOptions = DocumentOptions.ShowBasicLabelOnly;
			AssertEquals("With DocumentOptions of ShowBasicLabelOnly, only the Basic Label should be shown.", true, Data.PackageJob.ShowBasicLabelOnly);

			Data.PackageJob.KJ_ParentID = ZGuid.Empty;
			Data.PackageJob.KJ_ParentTableCode = "";
			AssertEquals("With no Parent, all Documents should be shown.", false, Data.PackageJob.ShowBasicLabelOnly);
		}

		#endregion

		#endregion

		#region Events

		#region TestOnParentJobNumberChanged

		public void TestOnParentJobNumberChanged()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.OnParentJobNumberChanged();

			var raised = false;
			packageJob.ParentJobNumberChanged += delegate
			{ raised = true; };
			packageJob.OnParentJobNumberChanged();
			AssertEquals("Was raised.", true, raised);
		}

		#endregion

		#region TestPackageDataChanged

		public void TestPackageDataChanged()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var fired = false;
			packageJob.PackageDataChanged += (s, e) => fired = true;

			var outerPackage = packageJob.Packages.AddNew();
			packageJob.FirePackageDataChanged(outerPackage, PackageDataChangeType.Weight);
			Assert("PackageDataChanged should be fired.", fired);

			var innerPackage = packageJob.Packages.AddNew();
			innerPackage.KP_KP_ParentPackage = outerPackage.PK;
			fired = false;
			packageJob.FirePackageDataChanged(innerPackage, PackageDataChangeType.Weight);
			Assert("PackageDataChanged should not be fired for inner package.", !fired);
		}

		#endregion

		#region TestOuterPackageAdded

		public void TestOuterPackageAdded()
		{
			int outerPackageAddedHitCount = 0;
			PackageEventArgs args = null;
			Data.CreatePackingData();
			Data.PackageJob.OuterPackageAdded += (sender, e) =>
			{
				outerPackageAddedHitCount++;
				args = e;
			};

			var package = Factory.New<PkgPackage>();
			Data.PackageJob.FireOuterPackageAdded(package);
			AssertEquals(1, outerPackageAddedHitCount);
			AssertEquals(package, args.Package);

			var otherPackageJob = Factory.New<PkgPackageJob>();
			var inner = otherPackageJob.Packages.AddNew().Packages.AddNew();

			Data.PackageJob.FireOuterPackageAdded(inner);
			AssertEquals(1, outerPackageAddedHitCount);
		}

		#endregion

		#region TestOuterParentPackageChanged

		public void TestOuterParentPackageChanged()
		{
			int outerParentPackageChangedHitCount = 0;
			PackageEventArgs args = null;
			Data.CreatePackingData();
			Data.PackageJob.OuterParentPackageChanged += (sender, e) =>
			{
				outerParentPackageChangedHitCount++;
				args = e;
			};

			var outer = Data.PackageJob.Packages.AddNew();
			Data.PackageJob.FireParentPackageChangedOnOuter(outer);
			AssertEquals(0, outerParentPackageChangedHitCount);

			var inner = outer.Packages.AddNew();
			Data.PackageJob.FireParentPackageChangedOnOuter(inner);
			AssertEquals(1, outerParentPackageChangedHitCount);
		}

		#endregion

		#endregion

		#region TestInitiateMassPackageProcess

		public void TestInitiateMassPackageProcess()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var hitCount = 0;
			packageJob.MassPackageProcessFinished += (sender, e) => hitCount++;
			AssertEquals("Precondition: Mass Package Process has not started.", false, packageJob.IsMassPackageProcessRunning);

			using (packageJob.InitiateMassPackageProcess())
			{
				AssertEquals("Mass Package Process has started.", true, packageJob.IsMassPackageProcessRunning);
				AssertEquals("Mass Package Process has not yet finished.", 0, hitCount);

				using (packageJob.InitiateMassPackageProcess())
				{
					AssertEquals("Mass Package Process is still running.", true, packageJob.IsMassPackageProcessRunning);
					AssertEquals("Mass Package Process has not yet finished.", 0, hitCount);
				}

				AssertEquals("Mass Package Process is still running.", true, packageJob.IsMassPackageProcessRunning);
				AssertEquals("Mass Package Process has not yet finished.", 0, hitCount);
			}

			AssertEquals("Mass Package Process is finished.", false, packageJob.IsMassPackageProcessRunning);
			AssertEquals("Mass Package Process is finished.", 1, hitCount);
		}

		#endregion

		#region TestInitiateMassPackageProcess_WithNoDefer

		public void TestInitiateMassPackageProcess_WithNoDefer()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var hitCount = 0;
			packageJob.MassPackageProcessFinished += (sender, e) => hitCount++;
			AssertEquals("Precondition: Mass Package Process has not started.", false, packageJob.IsMassPackageProcessRunning);

			using (packageJob.InitiateMassPackageProcess_WithNoDefer())
			{
				AssertEquals("Mass Package Process has started.", true, packageJob.IsMassPackageProcessRunning);
				AssertEquals("Mass Package Process has not yet finished.", 0, hitCount);

				using (packageJob.InitiateMassPackageProcess_WithNoDefer())
				{
					AssertEquals("Mass Package Process is still running.", true, packageJob.IsMassPackageProcessRunning);
					AssertEquals("Mass Package Process has not yet finished.", 0, hitCount);
				}

				AssertEquals("Mass Package Process is still running.", true, packageJob.IsMassPackageProcessRunning);
				AssertEquals("Mass Package Process has not yet finished.", 0, hitCount);
			}

			AssertEquals("Mass Package Process is finished.", false, packageJob.IsMassPackageProcessRunning);
			AssertEquals("Mass Package Process is finished but we did not defer, so event should not be fired.", 0, hitCount);
		}

		#endregion

		#region TestCloseAndGenerateIDs

		public void TestCloseAndGenerateIDs()
		{
			var dummy = Factory.New<DummyWithPacking>();
			dummy.JobNoForPackingParent = "abc";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			var package1 = packageJob.Packages.AddNew();
			var package2 = packageJob.Packages.AddNew();
			AssertEquals("Precondition", false, package1.IsClosed);
			AssertEquals("Precondition", true, package1.KP_PackageID.IsEmpty);
			AssertEquals("Precondition", false, package2.IsClosed);
			AssertEquals("Precondition", true, package2.KP_PackageID.IsEmpty);
			AssertEquals("Precondition", null, dummy.LastSSCCGenerationContext);

			packageJob.CloseAndGenerateIDs(Notify);
			AssertEquals("Package should be closed.", true, package1.IsClosed);
			AssertEquals("Package should have an ID.", false, package1.KP_PackageID.IsEmpty);
			AssertEquals("Package should be closed.", true, package2.IsClosed);
			AssertEquals("Package should have an ID.", false, package2.KP_PackageID.IsEmpty);
			AssertEquals(SSCCGenerationContext.AutoClosingPackage, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestOnFactorySaving_GeneratePackageIDsOfChildPackages

		public void TestGeneratePackageIDsOfInnerPackages()
		{
			var dummy = Factory.New<DummyWithPacking>();
			dummy.JobNoForPackingParent = "abc";

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			var outerPackage1 = packageJob.Packages.AddNew("PLT");
			var outerPackage2 = packageJob.Packages.AddNew("ENV", "ID-001");
			var innerPackage1 = outerPackage1.Packages.AddNew("CTN");
			var innerPackage2 = outerPackage2.Packages.AddNew("SHT");

			AssertEquals("Precondition", true, outerPackage1.KP_PackageID.IsEmpty);
			AssertEquals("Precondition", false, outerPackage2.KP_PackageID.IsEmpty);
			AssertEquals("Precondition", true, innerPackage1.KP_PackageID.IsEmpty);
			AssertEquals("Precondition", true, innerPackage2.KP_PackageID.IsEmpty);
			AssertEquals("Precondition", null, dummy.LastSSCCGenerationContext);

			packageJob.CloseAndGenerateIDs(Notify);

			AssertEquals("Package should have an ID", false, outerPackage1.KP_PackageID.IsEmpty);
			AssertEquals("Package should have an ID", false, outerPackage2.KP_PackageID.IsEmpty);
			AssertEquals("Package should have an ID", false, innerPackage1.KP_PackageID.IsEmpty);
			AssertEquals("Package should have an ID", false, innerPackage2.KP_PackageID.IsEmpty);
			AssertEquals(SSCCGenerationContext.AutoClosingPackage, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestFetchStrategy

		public void TestFetchStrategy()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			AssertEquals(typeof(PkgPackageJobFetchStrategy), packageJob.FetchStrategy.GetType());
		}

		#endregion

		#region TestGetAllPackageIDs

		public void TestGetAllPackageIDs()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var container2 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");

			container1.KP_PackageID = "CNT-1";
			container2.KP_PackageID = "CNT-2";
			pallet1.KP_PackageID = "PLT-1";
			pallet2.KP_PackageID = "PLT-2";

			AssertContainsExactElementsInAnyOrder(new ZString[] { "CNT-1", "CNT-2", "PLT-1", "PLT-2" }, Data.PackageJob.GetAllPackageIDs());
		}

		#endregion

		#region TestRemoveHoldOfAllPackages

		public void TestRemoveHoldOfAllPackages()
		{
			Data.CreatePackingData();

			Data.PackageJob.Packages.AddNew().KP_IsHeld = true;
			Data.PackageJob.Packages.AddNew().KP_IsHeld = false;
			Data.PackageJob.Packages.AddNew().KP_IsHeld = true;

			AssertEquals("The package job has three packages", 3, Data.PackageJob.GetAllPackagesOnJob().Length);
			Data.PackageJob.RemoveHoldOfAllPackages();
			AssertEquals("There should be no packages with hold", true, Data.PackageJob.GetAllPackagesOnJob().All(p => !p.KP_IsHeld));
		}

		#endregion

		#region TestGetDefaultOuterPackType

		public void TestGetDefaultOuterPackType()
		{
			Data.CreatePackingData();

			var packageJob = Data.PackageJob;
			AssertEquals("Should get the Default Pack Type from the Packing Registry if the Parent doesn't specify a Pack Type to Default to.", "PLT", packageJob.GetDefaultOuterPackType());

			Data.Dummy.DefaultOuterPackType = "BOX";
			AssertEquals("Should get the Default Pack Type from the the Parent if Default Pack Type is specified.", "BOX", packageJob.GetDefaultOuterPackType());

			var package = packageJob.Packages.AddNew();
			AssertEquals("New Packages should get the correct Default Pack Type.", "BOX", package.KP_F3_NKPackType);

			var inner = package.Packages.AddNew();
			AssertEquals("New Inner Packages should not get the Default Outer Pack Type.", "CTN", inner.KP_F3_NKPackType);
		}

		#endregion

		#region TestAssignPackageIDs

		public void TestAssignPackageIDs__NumberOfHeadersLessThanPackageQty()
		{
			Data.CreatePackingData();

			var packagePLT = Data.PackageJob.Packages.AddNew("PLT", 5);
			AssertEquals("Pre-condition: PackageID", "", packagePLT.KP_PackageID);
			AssertEquals("Pre-condition: PackageQty", 5, packagePLT.KP_PackageQty);
			var packageHeader = Helper.CreatePackageHeader(Data.PackageJob, "abc");

			Data.PackageJob.AssignPackageIDs(packagePLT, new[] { packageHeader });

			AssertEquals("Should split into 2 packages.", 2, Data.PackageJob.Packages.Count);
			AssertEquals("Existing package id should be empty.", "", packagePLT.KP_PackageID);
			AssertEquals("Existing package Qty after split should reduce by 1.", 4, packagePLT.KP_PackageQty);

			var packageSplited = Data.PackageJob.Packages.Single(p => p.KP_PackageID == "abc");
			AssertEquals("Package Qty should be 1 after assigned package id.", 1, packageSplited.KP_PackageQty);
			AssertEquals("Should have no unassigned package ids.", 0, Data.PackageJob.LoosePackageIDs.Count);
		}

		public void TestAssignPackageIDs_NumberOfHeadersEqualToPackageQty()
		{
			Data.CreatePackingData();

			var packagePLT = Data.PackageJob.Packages.AddNew("PLT", 2);
			AssertEquals("Pre-condition: PackageID", "", packagePLT.KP_PackageID);
			AssertEquals("Pre-condition: PackageQty", 2, packagePLT.KP_PackageQty);
			var packageHeader1 = Helper.CreatePackageHeader(Data.PackageJob, "abc");
			var packageHeader2 = Helper.CreatePackageHeader(Data.PackageJob, "def");

			Data.PackageJob.AssignPackageIDs(packagePLT, new[] { packageHeader1, packageHeader2 });

			AssertEquals("Should split into 2 packages.", 2, Data.PackageJob.Packages.Count);
			AssertEquals(1, Data.PackageJob.Packages.Single(p => p.KP_PackageID == "abc").KP_PackageQty);
			AssertEquals(1, Data.PackageJob.Packages.Single(p => p.KP_PackageID == "def").KP_PackageQty);
			AssertEquals("Should have no unassigned package ids.", 0, Data.PackageJob.LoosePackageIDs.Count);
		}

		public void TestAssignPackageIDs_NumberOfHeadersMoreThanQty()
		{
			Data.CreatePackingData();

			var packagePLT = Data.PackageJob.Packages.AddNew("PLT", 3);
			AssertEquals("Pre-condition: PackageID", "", packagePLT.KP_PackageID);
			AssertEquals("Pre-condition: PackageQty", 3, packagePLT.KP_PackageQty);

			var packageHeader1 = Helper.CreatePackageHeader(Data.PackageJob, "01");
			var packageHeader2 = Helper.CreatePackageHeader(Data.PackageJob, "02");
			var packageHeader3 = Helper.CreatePackageHeader(Data.PackageJob, "03");
			var packageHeader4 = Helper.CreatePackageHeader(Data.PackageJob, "04");
			var packageHeader5 = Helper.CreatePackageHeader(Data.PackageJob, "05");

			Data.PackageJob.AssignPackageIDs(packagePLT, new[] { packageHeader1, packageHeader2, packageHeader3, packageHeader4, packageHeader5 });

			AssertEquals("Should have 3 packages splitted.", 3, Data.PackageJob.Packages.Count);
			AssertEquals(1, Data.PackageJob.Packages.Single(p => p.KP_PackageID == "01").KP_PackageQty);
			AssertEquals(1, Data.PackageJob.Packages.Single(p => p.KP_PackageID == "02").KP_PackageQty);
			AssertEquals(1, Data.PackageJob.Packages.Single(p => p.KP_PackageID == "03").KP_PackageQty);

			AssertEquals("Should have 2 package IDs not assigned.", 2, Data.PackageJob.LoosePackageIDs.Count);
			AssertNotNull(Data.PackageJob.LoosePackageIDs.Single(h => h.KPH_PackageID == "04"));
			AssertNotNull(Data.PackageJob.LoosePackageIDs.Single(h => h.KPH_PackageID == "05"));
		}

		public void TestAssignPackageIDs_LinkRemainsAfterSave()
		{
			Data.CreatePackingData();

			var packagePLT = Data.PackageJob.Packages.AddNew("PLT", 5);
			AssertEquals("Pre-condition: PackageID", "", packagePLT.KP_PackageID);
			AssertEquals("Pre-condition: PackageQty", 5, packagePLT.KP_PackageQty);
			var packageHeader = Helper.CreatePackageHeader(Data.PackageJob, "abc");

			Data.PackageJob.AssignPackageIDs(packagePLT, new[] { packageHeader });

			var packageSplited = Data.PackageJob.Packages.Single(p => p.KP_PackageID == "abc");
			AssertEquals("Pre-condition: package id assigned.", 1, packageSplited.KP_PackageQty);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var packageSplitInOtherFactory = otherFactory.Load<PkgPackage>(packageSplited.PK);
			AssertEquals("Package ID should remain as before.", "abc", packageSplitInOtherFactory.KP_PackageID);
		}

		#endregion

		#region TestIsPackageIDAlreadyAssigned

		public void TestIsPackageIDAlreadyAssigned()
		{
			Data.CreatePackingData();

			var packagePLT = Data.PackageJob.Packages.AddNew("PLT", 5);
			AssertEquals("Pre-condition: PackageQty", 5, packagePLT.KP_PackageQty);
			var packageHeader = Data.PackageJob.LoosePackageIDs.AddNew();
			packageHeader.KPH_PackageID = "abc";
			AssertEquals(false, Data.PackageJob.IsPackageIDAlreadyAssigned(packageHeader));

			Data.PackageJob.AssignPackageIDs(packagePLT, new[] { packageHeader });
			AssertEquals(true, Data.PackageJob.IsPackageIDAlreadyAssigned(packageHeader));
		}

		#endregion

		#region TestUnassignPackageIDs

		public void TestUnassignPackageIDs()
		{
			Data.CreatePackingData();

			var packagePLT1 = Data.PackageJob.Packages.AddNew("PLT", 1);
			var packageHeader1 = Helper.CreatePackageHeader(Data.PackageJob, "123");
			Data.PackageJob.AssignPackageIDs(packagePLT1, new[] { packageHeader1 });

			var packagePLT2 = Data.PackageJob.Packages.AddNew("PLT", 1);
			var packageHeader2 = Helper.CreatePackageHeader(Data.PackageJob, "456");
			Data.PackageJob.AssignPackageIDs(packagePLT2, new[] { packageHeader2 });

			var packageBOX = Data.PackageJob.Packages.AddNew("BOX", 1);
			var packageHeaderBOX = Helper.CreatePackageHeader(Data.PackageJob, "B001");
			Data.PackageJob.AssignPackageIDs(packageBOX, new[] { packageHeaderBOX });

			AssertEquals("Pre-condition: PackageID", "123", packagePLT1.KP_PackageID);
			AssertEquals("Pre-condition: PackageID", "456", packagePLT2.KP_PackageID);
			AssertEquals("Pre-condition: PackageID", "B001", packageBOX.KP_PackageID);
			AssertEquals("Should have 0 package IDs not assigned.", 0, Data.PackageJob.LoosePackageIDs.Count);

			Data.PackageJob.UnassignPackageIDs(new[] { packagePLT1, packagePLT2 });

			AssertNull(packagePLT1.PackageID);
			AssertNull(packagePLT2.PackageID);
			AssertEquals("PackageID", "", packagePLT1.KP_PackageID);
			AssertEquals("PackageID", "", packagePLT2.KP_PackageID);
			AssertEquals("PackageID", "B001", packageBOX.KP_PackageID);

			AssertEquals("Should have 2 package IDs not assigned.", 2, Data.PackageJob.LoosePackageIDs.Count);
			AssertNotNull(Data.PackageJob.LoosePackageIDs.Single(h => h.KPH_PackageID == "123"));
			AssertNotNull(Data.PackageJob.LoosePackageIDs.Single(h => h.KPH_PackageID == "456"));
		}

		#endregion

		#region TestFindPackageByRawBarcodeOrAddNewIfSSCC

		public void TestFindPackageByRawBarcodeOrAddNewIfSSCC()
		{
			Data.CreatePackingData();
			Data.Dummy.SSCCPrefix = "1234567";

			var packageWithNoID = Data.PackageJob.Packages.AddNew("BOX");
			var packageWithID1 = Data.PackageJob.Packages.AddNew("BOX");
			var packageWithID2 = packageWithID1.Packages.AddNew("BOX");
			var packageWithSSCC = Data.PackageJob.Packages.AddNew("BOX");

			packageWithID1.KP_PackageID = "outer";
			packageWithID2.KP_PackageID = "inner";
			packageWithSSCC.KP_PackageID = "012345670000000015";
			AssertEquals("Precondition - SSCC must be valid.", true, SSCCBarCodeChecker.IsSSCCBarCode("012345670000000015"));
			AssertEquals("Precondition", null, Data.Dummy.LastSSCCGenerationContext);

			// select the package with no ID
			Data.PackageJob.Selected.UpdateSelectedPackages(new PkgPackage[] { packageWithNoID });

			// try to find an existing ID
			AssertEquals(packageWithID1, Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("outer"));
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);

			// try to find an existing ID that is not an outer
			AssertEquals(packageWithID2, Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("inner"));
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);

			// try to find a non-existant ID
			AssertNull(Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("fluff"));
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);

			// try to find a non-existant SSCC that has no matching prefix
			AssertEquals("Precondition - SSCC must be valid.", true, SSCCBarCodeChecker.IsSSCCBarCode("011111110000000014"));
			AssertNull(Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("00" + "011111110000000014"));
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);

			// try to find an existing SSCC
			AssertEquals(packageWithSSCC, Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("00" + "012345670000000015"));
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);

			// try to find a non-existant SSCC (should plug the ID into the current selection as it has no ID)
			AssertEquals("Precondition - SSCC must be valid.", true, SSCCBarCodeChecker.IsSSCCBarCode("012345670000000022"));
			AssertEquals(packageWithNoID, Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("00" + "012345670000000022"));
			AssertEquals("012345670000000022", packageWithNoID.KP_PackageID);
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);

			// try to find a non-existant SSCC (should create a new package as the current selection has an ID)
			var newPackage = Data.PackageJob.FindPackageByRawBarcodeOrAddNewIfSSCC("00" + "012345670000000039");
			AssertCollectionNotContains(newPackage, new PkgPackage[] { packageWithNoID, packageWithID1, packageWithID2, packageWithSSCC });
			AssertEquals("012345670000000039", newPackage.KP_PackageID);
			AssertEquals(SSCCGenerationContext.CheckIfBarcodeIsSSCC, Data.Dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region TestPrintAllLabels

		public void TestPrintAllLabels()
		{
			// setup the job
			var dummy = Factory.New<DummyWithPacking>();
			dummy.JobNoForPackingParent = "abc";
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			packageJob.Packages.AddNew();
			packageJob.Packages.AddNew();
			packageJob.Packages.AddNew();

			packageJob.CloseAndGenerateIDs(Notify);

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocument, Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			// test
			AssertEquals("Precondtion", 0, Factory.Load<IStmPrintJob>(new ZQuery()).Length);

			packageJob.PrintAllLabels();
			var printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
			AssertEquals("There should be a print job for all 3 packages.", 3, printJobs.Length);
		}

		public void TestPrintAllLabels_SSCCPrefix()
		{
			var dummy = Factory.New<DummyWithPacking>();
			dummy.JobNoForPackingParent = "abc";
			dummy.SSCCPrefix = "123456789";
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(dummy);
			var package = packageJob.Packages.AddNew();
			packageJob.CloseAndGenerateIDs(Notify);
			AssertEquals("012345678900000012", package.KP_PackageID);
			AssertEquals(SSCCGenerationContext.AutoClosingPackage, dummy.LastSSCCGenerationContext);
		}

		#endregion

		#region Packing

		#region TestGetPackedQty

		public void TestGetPackedQty()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var container2 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet2.Packages.AddNew("BOX");

			pallet1.Pack(Data.DummyLine1, 10m);
			pallet2.Pack(Data.DummyLine1, 3m);
			box1.Pack(Data.DummyLine1, 1.5m);
			box2.Pack(Data.DummyLine2, 7m); // different line

			AssertEquals(14.5m, Data.PackageJob.GetPackedQty(Data.DummyLine1));
			AssertEquals(7m, Data.PackageJob.GetPackedQty(Data.DummyLine2));
			AssertEquals(0m, Data.PackageJob.GetPackedQty(Data.DummyLine3));
		}

		#endregion

		#region TestIsPacked

		public void TestIsPacked()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var container2 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet2.Packages.AddNew("BOX");

			pallet1.Pack(Data.DummyLine1, 10m);
			pallet2.Pack(Data.DummyLine1, 3m);
			box1.Pack(Data.DummyLine1, 1.5m);
			box2.Pack(Data.DummyLine2, 7m); // different line

			AssertEquals(true, Data.PackageJob.IsPacked(Data.DummyLine1));
			AssertEquals(true, Data.PackageJob.IsPacked(Data.DummyLine2));
			AssertEquals(false, Data.PackageJob.IsPacked(Data.DummyLine3));
		}

		#endregion

		#region TestIsPacked_PackableItem

		public void TestIsPacked_PackableItem()
		{
			Data.CreatePackingData();

			var container1 = Data.PackageJob.Packages.AddNew("CNT");
			var container2 = Data.PackageJob.Packages.AddNew("CNT");
			var pallet1 = container1.Packages.AddNew("PLT");
			var pallet2 = container2.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet2.Packages.AddNew("BOX");

			pallet1.Pack(Data.DummyLine1, 10m);
			pallet2.Pack(Data.DummyLine1, 88.5m);
			box1.Pack(Data.DummyLine1, 1.5m);
			box2.Pack(Data.DummyLine2, 7m); // different line

			var dummyLine1PackedItem1 = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 10m);
			var dummyLine1PackedItem2 = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 88.5m);
			var dummyLine1PackedItem3 = Data.DummyLine1.PackableItems.Single(i => i.Quantity == 1.5m);
			var dummyLine2PackedItem1 = Data.DummyLine2.PackableItems.Single(i => i.Quantity == 7m);
			var dummyLinePackingItem = Data.DummyLine3.PackableItems.Single();
			AssertEquals(true, Data.PackageJob.IsPacked(dummyLine1PackedItem1));
			AssertEquals(true, Data.PackageJob.IsPacked(dummyLine1PackedItem2));
			AssertEquals(true, Data.PackageJob.IsPacked(dummyLine1PackedItem3));
			AssertEquals(true, Data.PackageJob.IsPacked(dummyLine2PackedItem1));
			AssertEquals(false, Data.PackageJob.IsPacked(dummyLinePackingItem));
		}

		#endregion

		#region TestAutoPack

		#region TestAutoPack

		[TestDate(2014, 1, 1)]
		public void TestAutoPack()
		{
			Data.CreatePackingData();
			Data.PackageJob.AutoPack(Notify);

			AssertAutoPack(Data.PackageJob, Data.DummyLine1, 25, "PLT", 4m); // should have 25 PLTs with 4 TVs in each.
			AssertAutoPack(Data.PackageJob, Data.DummyLine2, 10, "CTN", 10m); // should have 10 CTNs with 10 Amps in each.
			AssertAutoPack(Data.PackageJob, Data.DummyLine3, 8, "BOX", 12m, 4m); // should have 8 BOXs with 12 Speakers in each, and 1 BOX with the remaining 4 Speakers.

			var log = Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Single();
			AssertEquals("Auto-Pack Event should have been added with correct reference.", "|TYP=AutoPack", log.SL_Reference);
			AssertEquals("Auto-Pack Event should have been added with correct event time.", new ZDateTime(2014, 1, 1), log.SL_EventTime);
		}

		public void TestAutoPack_InitiatesMassPackageProcess()
		{
			Data.CreatePackingData();

			var packingProcessInvoked = false;
			var wasMassPackageProcessStartedDuringPacking = false;
			Data.PackageJob.Packages.CollectionCountChange += (sender, e) =>
			{
				wasMassPackageProcessStartedDuringPacking = Data.PackageJob.IsMassPackageProcessRunning;
				((PkgPackage)e.BizObject).PackedItemDivots.CountChanged += (x, y) =>
				{
					packingProcessInvoked = true;
					wasMassPackageProcessStartedDuringPacking &= Data.PackageJob.IsMassPackageProcessRunning;
				};
			};

			Data.PackageJob.AutoPack(Notify);
			AssertEquals("Packages were packed.", true, packingProcessInvoked);
			AssertEquals("Mass Package Process was started during Auto Pack.", true, wasMassPackageProcessStartedDuringPacking);
		}

		void AssertAutoPack(PkgPackageJob packageJob, IPackableItemParent itemParent, int numberOfPackages, ZString packageType, ZDecimal qtyPerPackage, decimal remainder = 0)
		{
			var packages = packageJob.Packages.Where(p => p.KP_F3_NKPackType == packageType && p.PackedItemDivots.First().KI_PackedQty == qtyPerPackage);
			AssertEquals(numberOfPackages, packages.Count());

			foreach (var package in packages)
			{
				var divot = package.PackedItemDivots.Single(); // Single() ensures 1 divot
				AssertEquals(itemParent, divot.GetPackableItemParent());
				AssertEquals(qtyPerPackage, divot.KI_PackedQty);
			}

			if (remainder > 0)
			{
				var lastPackage = packageJob.Packages.Single(p => p.KP_F3_NKPackType == packageType && p.PackedItemDivots.First().KI_PackedQty == remainder);
				AssertEquals(itemParent, lastPackage.PackedItemDivots.Single().GetPackableItemParent());
			}
		}

		#endregion

		#region TestAutoPack_AfterPackingStarted

		public void TestAutoPack_AfterPackingStarted()
		{
			Data.CreatePackingData();
			Data.PackageJob.Packages.AddNew();
			Data.PackageJob.AutoPack(Notify);

			string expectedMsg =
				"The Auto-Pack function requires that no packages exist in order to run.\r\n" +
				"Please remove your packages before running Auto-Pack.";
			AssertEquals(expectedMsg, Notify.LastEvent.Message);
			AssertEquals("Auto-Pack failed, should not add Auto-Pack Event.", false, Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Any());
		}

		#endregion

		#region TestAutoPack_FiresEventForEachSavedAutoPack

		[TestDate(2014, 1, 1)]
		public void TestAutoPack_FiresEventForEachSavedAutoPack()
		{
			Data.CreatePackingData();
			Data.DummyLine1.AutoPackQtyPerPackage = 0;
			Data.DummyLine3.Delete();
			Data.PackageJob.AutoPack(Notify);

			// 1 of 2 items were packed, auto-pack event should fire.
			var log1 = Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Single();
			AssertEquals("Auto-Pack Event should have been added with correct reference.", "|TYP=AutoPack", log1.SL_Reference);
			AssertEquals("Auto-Pack Event should have been added with correct event time.", new ZDateTime(2014, 1, 1), log1.SL_EventTime);

			// auto-pack a second time, but do not save. the first auto-pack event should be replaced.
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			try
			{
				Data.PackageJob.Packages.DeleteAll();
				Data.PackageJob.AutoPack(Notify);
			}
			finally
			{
				TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-1);
			}
			var log2 = Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Single(); // should replace existing log, as no save has occured since the last auto-pack
			AssertEquals("Auto-Pack Event should have been added with correct reference.", "|TYP=AutoPack", log2.SL_Reference);
			AssertEquals("Auto-Pack Event should have been updated to the last Auto-Pack event time.", new ZDateTime(2014, 1, 2), log2.SL_EventTime);

			// save changes and auto-pack a third time. a new log should be created for the third auto-pack.
			Factory.Save(); // commit log to DB
			Data.PackageJob.Packages.DeleteAll();
			Data.DummyLine1.AutoPackQtyPerPackage = 4; // makes sure partial auto-pack works
			Data.PackageJob.AutoPack(Notify);
			AssertEquals("Since first Auto-Pack event was committed, second Auto-Pack event should fire.",
				2, Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Count());
		}

		#endregion

		#region TestAutoPack_WithNoParent

		public void TestAutoPack_WithNoParent()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.AutoPack(Notify);

			AssertEquals("There is nothing to Auto-Pack.", Notify.LastEvent.Message);
		}

		#endregion

		#region TestAutoPack_WithInvalidAutoPackValues

		public void TestAutoPack_WithInvalidAutoPackValues()
		{
			Data.CreatePackingData();
			Data.Dummy.IsAutoPackAllowed = YesNoWithReasonForNo.No("Dummy does not Auto-Pack.");
			Data.PackageJob.AutoPack(Notify);
			AssertEquals("Dummy does not Auto-Pack.", Notify.LastEvent.Message);
			AssertEquals(0, Data.PackageJob.Packages.Count);
			AssertEquals("If Auto-Pack fails, should not add Auto-Pack Event.", false, Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Any());

			Data.PackageJob.PackableItemParents.Sort(nameof(PackableItemParentWrapper.Description)); // To make deterministic, in production it makes sense to follow the order from GUI
			Data.Dummy.IsAutoPackAllowed = YesNoWithReasonForNo.Yes;
			Data.DummyLine1.AutoPackQtyPerPackage = 0;
			Data.DummyLine2.AutoPackPackageType = "";
			Data.DummyLine3.Delete();
			Data.PackageJob.AutoPack(Notify);

			string expectedMsg =
				"\r\n" +
				"The following items could not be Auto-Packed because on the Dummy, the Qty per Package is 0, or no Package Unit is defined:" +
				"\r\n" +
				"\r\n	100 x Amp" +
				"\r\n	100 x TV";
			AssertEquals(expectedMsg, Notify.LastEvent.Message);
			AssertEquals(0, Data.PackageJob.Packages.Count);
			AssertEquals("If Auto-Pack fails, should not add Auto-Pack Event.", false, Data.Dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode).Any());
		}

		#endregion

		#region TestNothingToAutoPackMessage

		public void TestNothingToAutoPackMessage()
		{
			AssertEquals("There is nothing to Auto-Pack.", PkgPackageJob.NothingToAutoPackMessage);
		}

		#endregion

		#endregion

		#region TestMultiPack

		#region TestMulitPack_NoExceptionWithMaxQuantity

		public void TestMulitPack_NoExceptionWithMaxQuantity()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var wrappers = Data.PackageJob.PackableItemParents;

			var box1 = packageJob.Packages.AddNew();
			var box2 = packageJob.Packages.AddNew();
			var box3 = packageJob.Packages.AddNew();

			Data.DummyLine1Wrapper.ProposedPackQty = 999999999999999;

			AssertNoExceptionThrown(() => packageJob.MultiPack(new[] { box1, box2, box3 }, wrappers));
		}

		#endregion

		#region TestMultiPack_WithNewPackages

		public void TestMultiPack_WithNewPackages()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var wrappers = Data.PackageJob.PackableItemParents;

			Data.DummyLine1Wrapper.ProposedPackQty = 75;  // 25 per box
			Data.DummyLine2Wrapper.ProposedPackQty = 100; // 33 per box with remainder

			var boxes = packageJob.MultiPack(3, "BOX", wrappers);
			AssertEquals(3, boxes.Length);

			// find boxes 1 + 2
			var countOfBox1And2 =
			(
				from b in boxes
				where b.KP_PackageQty == 1
				where b.KP_F3_NKPackType == "BOX"
				where b.PackedItemDivots.Count == 2
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 25m
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 33m
				select b
			)
			.Count();
			AssertEquals(2, countOfBox1And2);

			// find box 3
			var countOfBox3 =
			(
				from b in boxes
				where b.KP_PackageQty == 1
				where b.KP_F3_NKPackType == "BOX"
				where b.PackedItemDivots.Count == 2
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 25m
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 34m
				select b
			)
			.Count();
			AssertEquals(1, countOfBox3);
		}

		#endregion

		#region TestMultiPack_WithNewPackages_UsingParentPackage

		public void TestMultiPack_WithNewPackages_UsingParentPackage()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var pallet = packageJob.Packages.AddNew("PLT");
			var wrappers = Data.PackageJob.PackableItemParents;

			Data.DummyLine1Wrapper.ProposedPackQty = 75;  // 25 per box
			Data.DummyLine2Wrapper.ProposedPackQty = 100; // 33 per box with remainder

			var boxes = packageJob.MultiPack(3, "BOX", wrappers, pallet);
			AssertEquals(3, boxes.Length);

			// find boxes 1 + 2 on the pallet
			var countOfBox1And2 =
			(
				from b in boxes
				where b.KP_KP_ParentPackage == pallet.PK
				where b.KP_PackageQty == 1
				where b.KP_F3_NKPackType == "BOX"
				where b.PackedItemDivots.Count == 2
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 25m
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 33m
				select b
			)
			.Count();
			AssertEquals(2, countOfBox1And2);

			// find box 3 on the pallet
			var countOfBox3 =
			(
				from b in boxes
				where b.KP_KP_ParentPackage == pallet.PK
				where b.KP_PackageQty == 1
				where b.KP_F3_NKPackType == "BOX"
				where b.PackedItemDivots.Count == 2
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 25m
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 34m
				select b
			)
			.Count();
			AssertEquals(1, countOfBox3);
		}

		#endregion

		#region TestMultiPack_WithExistingPackages

		public void TestMultiPack_WithExistingPackages()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var wrappers = Data.PackageJob.PackableItemParents;

			var box1 = packageJob.Packages.AddNew();
			var box2 = packageJob.Packages.AddNew();
			var box3 = packageJob.Packages.AddNew();

			Data.DummyLine1Wrapper.ProposedPackQty = 50; // 25 per box
			Data.DummyLine2Wrapper.ProposedPackQty = 67; // 33 per box with remainder

			packageJob.MultiPack(new[] { box1, box2 }, wrappers);

			// find box 1
			var box1Found =
			(
				from b in packageJob.Packages
				where b.PackedItemDivots.Count == 2
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 25m
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 33m
				select b
			)
			.Count() == 1;
			AssertEquals(true, box1Found);

			// find box 2
			var box2Found =
			(
				from b in packageJob.Packages
				where b.PackedItemDivots.Count == 2
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 25m
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine2).KI_PackedQty == 34m
				select b
			)
			.Count() == 1;
			AssertEquals(true, box2Found);

			// the third box was not included, should not have anything packed
			AssertEquals(0, box3.PackedItemDivots.Count);
		}

		#endregion

		#region TestMultiPack_WithNewPackages_MorePackagesThanItems

		public void TestMultiPack_WithNewPackages_MorePackagesThanItems()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var wrappers = Data.PackageJob.PackableItemParents;

			Data.DummyLine1Wrapper.ProposedPackQty = 2;

			var boxes = packageJob.MultiPack(3, "BOX", wrappers);
			AssertEquals(3, boxes.Length);

			// find boxes 1 + 2
			var countOfBox1And2 =
			(
				from b in boxes
				where b.KP_PackageQty == 1
				where b.KP_F3_NKPackType == "BOX"
				where b.PackedItemDivots.Count == 1
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 1m
				select b
			)
			.Count();
			AssertEquals(2, countOfBox1And2);

			// find box 3 (empty)
			var countOfBox3 =
			(
				from b in boxes
				where b.KP_PackageQty == 1
				where b.KP_F3_NKPackType == "BOX"
				where b.PackedItemDivots.Count == 0
				select b
			)
			.Count();
			AssertEquals(1, countOfBox3);
		}

		#endregion

		#region TestMultiPack_DecimalRemainder

		public void TestMultiPack_DecimalRemainder()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			// attempt to pack 0.75 items into a box
			var box = packageJob.Packages.AddNew();
			Data.DummyLine1Wrapper.ProposedPackQty = 0.75;
			packageJob.MultiPack(new[] { box }, Data.PackageJob.PackableItemParents);

			var box1Found =
			(
				from b in packageJob.Packages
				where b.PackedItemDivots.Count == 1
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 0.75m
				select b
			)
			.Count() == 1;
			AssertEquals(true, box1Found);
			box.PackedItemDivots[0].Delete(); // cleanup

			// attempt to pack 2.5 items into 3 boxes
			var box2 = packageJob.Packages.AddNew();
			var box3 = packageJob.Packages.AddNew();
			Data.DummyLine1Wrapper.ProposedPackQty = 2.5;
			packageJob.MultiPack(new[] { box, box2, box3 }, Data.PackageJob.PackableItemParents);

			var box1And2Found =
			(
				from b in packageJob.Packages
				where b.PackedItemDivots.Count == 1
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 1m
				select b
			)
			.Count() == 2;
			AssertEquals(true, box1And2Found);

			var box3Found =
			(
				from b in packageJob.Packages
				where b.PackedItemDivots.Count == 1
				where b.PackedItemDivots.FindByPackableItemParent_ForTesting(Data.DummyLine1).KI_PackedQty == 0.5m
				select b
			)
			.Count() == 1;
			AssertEquals(true, box3Found);
		}

		#endregion

		#endregion

		#region TestContainsPackage

		public void TestContainsPackage()
		{
			Data.CreatePackingData();

			var container = Data.PackageJob.Packages.AddNew("CNT");

			AssertEquals(true, Data.PackageJob.ContainsPackage(container));
		}

		#endregion

		#endregion

		#region Save

		#region TestKJ_JobIDIsSetOnSave

		public virtual void TestKJ_JobIDIsSetOnSave()
		{
			ZString nextNumber = Env.NumberFountains.PackingID.PeekPreliminaryFormatted(Factory);
			AssertEquals("Precondition", true, !nextNumber.IsEmpty);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob.Packages.AddNew(); // empty package jobs are deleted on save
			AssertEquals("Precondition", true, packageJob.KJ_JobID.IsEmpty);

			Factory.Save();
			AssertEquals(nextNumber, packageJob.KJ_JobID);

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("The Packing ID should not be set twice.", nextNumber, packageJob.KJ_JobID);

			// ensure not cleared on save failure if already in DB
			var packageJobToCreateSaveFailure = Factory.New<PkgPackageJob>();
			packageJobToCreateSaveFailure.Packages.AddNew(); // empty package jobs are deleted on save
			packageJobToCreateSaveFailure.KJ_JobID = packageJob.KJ_JobID;

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.Empty; // ensure OnSaved is invoked
			AssertExceptionThrown("Precondition - Should have thrown a save exception as the JobIDs are duplicated.", typeof(ZSaveException), () => Factory.Save());
			AssertEquals("The Packing ID should not be cleared on save failure as the job is already in the DB.", nextNumber, packageJob.KJ_JobID);
		}

		#endregion

		#region TestKJ_JobIDIsClearedOnSaveFailure

		public void TestKJ_JobIDIsClearedOnSaveFailure()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			var packageJob2 = Factory.New<PkgPackageJob>();
			packageJob1.Packages.AddNew(); //
			packageJob2.Packages.AddNew(); // empty package jobs are deleted on save
			packageJob1.KJ_JobID = "abc";
			packageJob2.KJ_JobID = "abc";
			AssertEquals("Precondition - Package Job 1's JobID must not be empty.", false, packageJob1.KJ_JobID.IsEmpty);
			AssertEquals("Precondition - Package Job 2's JobID must not be empty.", false, packageJob2.KJ_JobID.IsEmpty);

			AssertExceptionThrown("Precondition - Should have thrown a save exception as the JobIDs are duplicated.", typeof(ZSaveException), () => Factory.Save());
			AssertEquals(true, packageJob1.KJ_JobID.IsEmpty);
			AssertEquals(true, packageJob2.KJ_JobID.IsEmpty);
		}

		#endregion

		#region TestEmptyUnsavedPackageJobIsNotSaved

		public void TestEmptyUnsavedPackageJobIsNotSaved()
		{
			var packageJob_NoChanges = Factory.New<PkgPackageJob>();
			var packageJob_WithChanges = Factory.New<PkgPackageJob>();
			packageJob_WithChanges.Packages.AddNew();
			packageJob_WithChanges.KJ_ParentID = ZGuid.NewZGuid();
			packageJob_WithChanges.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			AssertEquals(false, packageJob_NoChanges.HasChanges);

			Factory.Save();
			AssertEquals("PackageJob with no changes should not be saved to the DB.", false, packageJob_NoChanges.IsInDatabase);
			AssertEquals("PackageJob with no changes should not be deleted (required for display in bound controls ie the tree).", false, packageJob_NoChanges.IsDeleted);
			AssertEquals("PackageJob with changes should be saved to the DB.", true, packageJob_WithChanges.IsInDatabase);
			AssertEquals("PackageJob with changes should not be deleted.", false, packageJob_WithChanges.IsDeleted);
		}

		#endregion

		#region TestEmptyFinalisedPackageJobIsDeletedOnSave

		public void TestEmptyFinalisedPackageJobIsDeletedOnSave()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var packageJobWithPackages = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJobWithPackages.Packages.AddNew();

			packageJob.KJ_IsFinalized = true;
			packageJobWithPackages.KJ_IsFinalized = true;
			Factory.Save();
			AssertEquals(true, packageJob.IsDeleted);
			AssertEquals(false, packageJobWithPackages.IsDeleted);
		}

		#endregion

		#region TestPreventSaveIfFinalised

		public void TestPreventSaveIfFinalised()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.Packages.AddNew(); // to prevent deletion of empty finalised job
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var packageJobInOtherFactory = otherFactory.Load<PkgPackageJob>(packageJob.PK);

			Factory.RefreshEnabled = false;
			otherFactory.RefreshEnabled = false;

			packageJobInOtherFactory.KJ_IsFinalized = true;
			otherFactory.Save();

			packageJob.Logs.AddNew();
			Factory.Save();

			packageJob.Packages.AddNew();
			AssertExceptionThrown("Should have thrown a save exception because the job is finalised.", typeof(ZSaveConcurrencyException), () => Factory.Save());
			AssertExceptionThrown("Should have thrown a save exception on subsequent saves because the job is finalised.", typeof(ZSaveConcurrencyException), () => Factory.Save());

			// test the packagejob cannot be changed

			var factory2 = new BusinessObjectFactory();
			var packageJob2 = factory2.New<PkgPackageJob>();
			packageJob2.KJ_ParentID = ZGuid.NewZGuid();
			packageJob2.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			packageJob2.Packages.AddNew(); // to prevent deletion of empty finalised job
			packageJob2.KJ_IsFinalized = true;
			factory2.Save();

			packageJob2.KJ_JobID = "abc";
			AssertExceptionThrown("Should have thrown a save exception because the job is finalised.", typeof(ZCannotSaveException), () => factory2.Save());
		}

		#endregion

		#region TestIsReleasedCanBeChangedWhenFinalised

		public void TestIsReleasedCanBeChangedWhenFinalised()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			var package = packageJob.Packages.AddNew();
			packageJob.KJ_IsFinalized = true;
			Factory.Save();
			AssertEquals("Precondition", true, packageJob.KJ_IsFinalized);
			AssertEquals("Precondition", false, packageJob.IsReleased);

			// test packagejob can still be released (incl releasing packages via job)

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.UtcNow; // also tests that HasChangesThatAreInvalidIfFinalised is reset on successful save
			AssertNoExceptionThrown("Should not prevent save when changing IsReleased on a Finalised PackageJob.", () => Factory.Save());
			AssertEquals("Should not prevent save when changing IsReleased on a Finalised PackageJob.", true, packageJob.IsReleased);

			// test packages can still be released

			AssertEquals("Precondition", false, package.IsReleased);
			package.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertNoExceptionThrown("Should not prevent save when changing package.IsReleased on a Finalised PackageJob.", () => Factory.Save());
			AssertEquals("Should not prevent save when changing package.IsReleased on a Finalised PackageJob.", true, package.IsReleased);
		}

		#endregion

		#region TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackage

		[TestDate(2014, 2, 1)]
		public void TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackage_RegistryOn()
		{
			TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackageCore(enableAnonymousPackageRegistryItem: true);
		}

		[TestDate(2014, 2, 1)]
		public void TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackage_RegistryOff()
		{
			TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackageCore(enableAnonymousPackageRegistryItem: false);
		}

		void TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackageCore(bool enableAnonymousPackageRegistryItem)
		{
			using (PackingRegistry.Instance.AnonymousPackageCreationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableAnonymousPackageRegistryItem))
			{
				var year = ZDateTime.Now.Year;

				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "SSS";

				var orphanScan1 = CreateNewOrphanScan("123", new ZDateTime(year, 1, 1), "SSS", "DUM", "REF1", Events.PickedUp, 10, 20, 30, Constants.Length.Metres, 20, Constants.Weight.Kilograms);
				var orphanScan2 = CreateNewOrphanScan("123", new ZDateTime(year, 1, 2), "SSS", "XXX", "REF2", Events.PickedUp, 15, 25, 35, Constants.Length.Metres, 25, Constants.Weight.Kilograms);
				var orphanScan3 = CreateNewOrphanScan("abc", new ZDateTime(year, 1, 3), "SSS", "DUM", "REF3", Events.PickedUp, 20, 40, 60, Constants.Length.Feet, 80, Constants.Weight.Pounds);
				Factory.Save();

				// package job that has no parent job (should not match any orphans)
				var packageJob1 = Factory.New<PkgPackageJob>();
				packageJob1.KJ_ParentID = ZGuid.NewZGuid();
				packageJob1.KJ_ParentTableCode = WhsDocketSchema.Constants.Prefix;
				var packageWithNoParent = packageJob1.Packages.AddNew("BOX", "123");
				Factory.Save();
				AssertEquals(0, packageWithNoParent.Logs.GetAllLogs().Count);

				// package job with parent
				Data.CreatePackingData();
				var packageJob2 = Data.PackageJob;
				var package1 = packageJob2.Packages.AddNew("BOX", " 123");
				var package2 = packageJob2.Packages.AddNew("BOX");
				var package3 = packageJob2.Packages.AddNew("BOX", "789");
				AssertEquals("Precondition", 0, package1.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 0, package2.Logs.GetAllLogs().Count);

				Factory.Save();
				AssertPackage(package1, new ZDateTime(year, 1, 1, 11, 0, 0), "SSS", new ZDateTime(year, 2, 1), Events.PickedUp, "REF1", 10, 20, 30, Constants.Length.Metres, 20, Constants.Weight.Kilograms, enableAnonymousPackageRegistryItem);

				var assertionMessage = enableAnonymousPackageRegistryItem
					? "Should delete an Orphan Scan after it is matched."
					: "Matching is disabled via Registry, should *not* delete the Orphan Scan.";

				AssertEquals(assertionMessage, enableAnonymousPackageRegistryItem, orphanScan1.IsDeleted);
				AssertEquals("No logs should have been added to 2nd package as no Scans match the Job type & Package ID.", 0, package2.Logs.GetAllLogs().Count);
				AssertDimsAndWeight(package2, 0, 0, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
				AssertEquals("No logs should have been added to 3rd package as no Scans match the Job type & Package ID.", 0, package3.Logs.GetAllLogs().Count);
				AssertDimsAndWeight(package3, 0, 0, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
				AssertEquals("Orphan Scan 2 was not matched, should *not* have been deleted.", false, orphanScan2.IsDeleted);
				AssertEquals("Orphan Scan 3 was not matched, should *not* have been deleted.", false, orphanScan3.IsDeleted);

				// change the ID for a package that is already in the db
				package2.KP_PackageID = "aBC";
				Assert("Precondition", package2.KP_KPH_PackageHeaderInfo.HasChanges);
				AssertEquals("Precondition.", 0, package2.Logs.GetAllLogs().Count);

				Factory.Save();
				AssertPackage(package2, new ZDateTime(year, 1, 3, 11, 0, 0), "SSS", new ZDateTime(year, 2, 1), Events.PickedUp, "REF3", 20, 40, 60, Constants.Length.Feet, 80, Constants.Weight.Pounds, enableAnonymousPackageRegistryItem);
				AssertEquals(enableAnonymousPackageRegistryItem, orphanScan3.IsDeleted);

				// create an orphan *after* the package job
				// (we don't want this to match -- if the job existed before scanning, the scanner would
				// have assigned the event to the job directly, instead of creating an orphan scan)
				var orphanScan4 = CreateNewOrphanScan("789", new ZDateTime(year, 1, 4), "SSS", "DUM", "REF4", Events.PickedUp, 10, 20, 30, Constants.Length.Metres, 20, Constants.Weight.Kilograms);
				Factory.Save();
				AssertEquals("No logs should have been added to 3rd package as the package has no changes to its ID.", 0, package3.Logs.GetAllLogs().Count);
				AssertDimsAndWeight(package3, 0, 0, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
				AssertEquals(false, orphanScan4.IsDeleted);
			}
		}

		[TestDate(2014, 2, 1)]
		public void TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackage_DefaultsWeightandDIMs()
		{
			using (PackingRegistry.Instance.AnonymousPackageCreationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "SSS";

				// IMPORTANT:
				// When enumerating Orphan Scans, the Factory uses the creation order. This means that packs will always be sorted correctly
				// even if the code does not have the OrderBy EventTime clause. Clearly this means it is not tested correctly, so to force the
				// necessity of the OrderBy clause in this test the packs are created in a random order.
				var orphanScanWithDimsForPackage1_2 = CreateNewOrphanScan("123", new ZDateTime(2014, 1, 2), "SSS", "DUM", "REF2", Events.PickedUp, 15, 25, 35, Constants.Length.Metres, 25, Constants.Weight.Kilograms);
				var orphanScanWithDimsForPackage1_3 = CreateNewOrphanScan("123", new ZDateTime(2014, 1, 3), "SSS", "DUM", "REF3", Events.PickedUp, 20, 40, 60, Constants.Length.Feet, 80, Constants.Weight.Pounds);
				var orphanScanWithDimsForPackage1_1 = CreateNewOrphanScan("123", new ZDateTime(2014, 1, 1), "SSS", "DUM", "REF1", Events.PickedUp, 10, 20, 30, Constants.Length.Metres, 20, Constants.Weight.Kilograms);
				var orphanScanWithoutDimsForPackage1 = CreateNewOrphanScan("123", new ZDateTime(2014, 1, 5), "SSS", "DUM", "REF4", Events.PickedUp, 0, 0, 0, ZString.Empty, 0, ZString.Empty);
				var orphanScanWithoutDimsForPackage1_ButHasUQ = CreateNewOrphanScan("123", new ZDateTime(2014, 1, 5), "SSS", "DUM", "REF5", Events.PickedUp, 0, 0, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);

				var orphanScanWithDimsForPackage2_2 = CreateNewOrphanScan("789", new ZDateTime(2014, 1, 2), "SSS", "DUM", "REF7", Events.PickedUp, 0, 0, 0, ZString.Empty, 20, Constants.Weight.Kilograms);
				var orphanScanWithDimsForPackage2_1 = CreateNewOrphanScan("789", new ZDateTime(2014, 1, 1), "SSS", "DUM", "REF6", Events.PickedUp, 10, 20, 30, Constants.Length.Metres, 0, ZString.Empty);

				var orphanScanWithDimsForPackage4 = CreateNewOrphanScan("1010", new ZDateTime(2014, 1, 2), "SSS", "DUM", "REF9", Events.PickedUp, 0, 10, 0, Constants.Length.Metres, 0, ZString.Empty);
				var orphanScanWithDimsForPackage3 = CreateNewOrphanScan("1000", new ZDateTime(2014, 1, 1), "SSS", "DUM", "REF8", Events.PickedUp, 10, 0, 0, Constants.Length.Metres, 0, ZString.Empty);
				var orphanScanWithDimsForPackage5 = CreateNewOrphanScan("1020", new ZDateTime(2014, 1, 3), "SSS", "DUM", "REF10", Events.PickedUp, 0, 0, 10, Constants.Length.Metres, 0, ZString.Empty);

				var orphanScanWithDimsForPackage6_2 = CreateNewOrphanScan("1030", new ZDateTime(2014, 1, 2), "SSS", "DUM", "REF12", Events.PickedUp, 0, 10, 0, ZString.Empty, 2, ZString.Empty);
				var orphanScanWithDimsForPackage6_3 = CreateNewOrphanScan("1030", new ZDateTime(2014, 1, 3), "SSS", "DUM", "REF13", Events.PickedUp, -5, 0, 0, ZString.Empty, 3, ZString.Empty);
				var orphanScanWithDimsForPackage6_1 = CreateNewOrphanScan("1030", new ZDateTime(2014, 1, 1), "SSS", "DUM", "REF11", Events.PickedUp, 0, 0, 10, ZString.Empty, 1, ZString.Empty);

				var orphanScanWithDimsForPackage7 = CreateNewOrphanScan("1040", new ZDateTime(2014, 1, 2), "SSS", "DUM", "REF14", Events.PickedUp, 0, 0, 0, ZString.Empty, 20, Constants.Weight.Pounds);

				var orphanScanForPackage8_InitialisedWithDimsAndWeights = CreateNewOrphanScan("456", new ZDateTime(2014, 1, 3), "SSS", "DUM", "REF15", Events.PickedUp, 20, 40, 60, Constants.Length.Feet, 80, Constants.Weight.Pounds);

				Factory.Save();

				// package job with parent
				Data.CreatePackingData();
				var packageJob = Data.PackageJob;
				var package1_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "123");
				var package2_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "789");
				var package3_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "1000");
				var package4_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "1010");
				var package5_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "1020");
				var package6_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "1030");
				var package7_NoWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "1040");
				var package8_WithWeightOrDIMsInitialised = packageJob.Packages.AddNew("BOX", "456");
				package8_WithWeightOrDIMsInitialised.KP_Length = 1;
				package8_WithWeightOrDIMsInitialised.KP_Width = 3;
				package8_WithWeightOrDIMsInitialised.KP_Height = 2;
				package8_WithWeightOrDIMsInitialised.KP_DimensionUQ = Constants.Length.Metres;
				package8_WithWeightOrDIMsInitialised.KP_Weight = 4;
				package8_WithWeightOrDIMsInitialised.KP_WeightUQ = Constants.Weight.Kilograms;

				Factory.Save();
				AssertEquals("Precondition", 5, package1_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 2, package2_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 1, package3_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 1, package4_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 1, package5_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 3, package6_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 1, package7_NoWeightOrDIMsInitialised.Logs.GetAllLogs().Count);
				AssertEquals("Precondition", 1, package8_WithWeightOrDIMsInitialised.Logs.GetAllLogs().Count);

				AssertDimsAndWeight(package1_NoWeightOrDIMsInitialised, 20, 40, 60, Constants.Length.Feet, 80, Constants.Weight.Pounds);
				AssertDimsAndWeight(package2_NoWeightOrDIMsInitialised, 10, 20, 30, Constants.Length.Metres, 20, Constants.Weight.Kilograms); // Consider Weight and DIMs seperately when populating
				AssertDimsAndWeight(package3_NoWeightOrDIMsInitialised, 10, 0, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
				AssertDimsAndWeight(package4_NoWeightOrDIMsInitialised, 0, 10, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
				AssertDimsAndWeight(package5_NoWeightOrDIMsInitialised, 0, 0, 10, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
				AssertDimsAndWeight(package6_NoWeightOrDIMsInitialised, 0, 10, 0, Constants.Length.Metres, 3, Constants.Weight.Kilograms); // Take latest > 0 Weight and DIMs, don't override UQ if empty on OrphanScan
				AssertDimsAndWeight(package7_NoWeightOrDIMsInitialised, 0, 0, 0, Constants.Length.Metres, 20, Constants.Weight.Pounds); // Take latest Weights
				AssertDimsAndWeight(package8_WithWeightOrDIMsInitialised, 20, 40, 60, Constants.Length.Feet, 80, Constants.Weight.Pounds); // Should override existing Weight or DIMs
			}
		}

		[TestDate(2014, 2, 1)]
		public void TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackage_GracefullyHandlesDuplicatedID()
		{
			using (PackingRegistry.Instance.AnonymousPackageCreationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "SSS";

				CreateNewOrphanScan("ABC", new ZDateTime(2014, 1, 1), "SSS", "DUM", "REF", Events.Authorised, 0m, 0m, 0m, "", 0m, "");
				Factory.Save();

				Data.CreatePackingData();
				Data.Dummy.JobNoForPackingParent = "DUM123";
				var packageJob = Data.PackageJob;
				packageJob.Packages.AddNew("PLT", "ABC");
				packageJob.Packages.AddNew("PLT", "ABC");

				AssertNoExceptionThrown(() => Factory.Save());
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC|", "DUM123-001|ABC" }, packageJob.Packages.Select(p => p.KP_PackageID + "|" + p.KP_PreviousPackageID));

				var packageWithOrphanScan = packageJob.Packages.Single(p => p.KP_PackageID == "ABC");
				AssertPackage(packageWithOrphanScan, new ZDateTime(2014, 1, 1, DateTimeKind.Utc).ToDateTime().ToLocalTime(), "SSS", new ZDateTime(2014, 2, 1), Events.Authorised,
					"REF", 0m, 0m, 0m, Constants.Length.Metres, 0m, Constants.Weight.Kilograms, anonymousPacksRegistryEnabled: true);
			}
		}

		[TestDate(2014, 2, 1)]
		public void TestOnFactorySave_AddsEventsFromOrphanScanToMatchingPackage_GracefullyHandlesCaseInsensitiveDuplicatedID()
		{
			using (PackingRegistry.Instance.AnonymousPackageCreationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "SSS";

				CreateNewOrphanScan("ABC", new ZDateTime(2014, 1, 1), "SSS", "DUM", "REF", Events.Authorised, 0m, 0m, 0m, "", 0m, "");
				Factory.Save();

				Data.CreatePackingData();
				Data.Dummy.JobNoForPackingParent = "DUM123";
				var packageJob = Data.PackageJob;
				packageJob.Packages.AddNew("PLT", "ABC");
				packageJob.Packages.AddNew("PLT", "abc");

				AssertNoExceptionThrown(() => Factory.Save());
				AssertContainsExactElementsInAnyOrder(new ZString[] { "ABC|", "DUM123-001|abc" }, packageJob.Packages.Select(p => p.KP_PackageID + "|" + p.KP_PreviousPackageID));

				var packageWithOrphanScan = packageJob.Packages.Single(p => p.KP_PackageID == "ABC");
				AssertPackage(packageWithOrphanScan, new ZDateTime(2014, 1, 1, DateTimeKind.Utc).ToDateTime().ToLocalTime(), "SSS", new ZDateTime(2014, 2, 1), Events.Authorised,
					"REF", 0m, 0m, 0m, Constants.Length.Metres, 0m, Constants.Weight.Kilograms, anonymousPacksRegistryEnabled: true);
			}
		}

		public void TestOnFactorySave_RemoveDuplicatePackageIDs_DoesntHitDbIfNoChanges()
		{
			Data.CreatePackingData();
			Data.Dummy.JobNoForPackingParent = "DUM123";
			var packageJob = Data.PackageJob;
			packageJob.Packages.AddNew("PLT", "ABC1");
			Factory.Save();

			Factory.ResetDatabaseLoadCount();
			packageJob.Packages[0].KP_GoodsDescription = "TEST";
			Factory.Save();
			AssertTableHitCount("Shouldn't have loaded packages.", 0, PkgPackageSchema.Constants.TableName);

			Factory.ResetDatabaseLoadCount();
			packageJob.Packages[0].KP_PackageID = "ABC2";
			Factory.Save();
			AssertTableHitCount("Should have loaded packages.", 1, PkgPackageSchema.Constants.TableName);

			Factory.ResetDatabaseLoadCount();
			packageJob.Packages.AddNew().KP_PackageID = "ABC3";
			packageJob.Packages.AddNew().KP_PackageID = "ABC4";
			Factory.Save();
			AssertTableHitCount("Should have loaded packages. Is cached. Should be db only query?", 0, PkgPackageSchema.Constants.TableName);
		}

		JobOrphanScan CreateNewOrphanScan(ZString packageID, ZDateTime eventTime, ZString staffCode, ZString moduleCode, ZString reference, Event eventType, ZDecimal length, ZDecimal width, ZDecimal height, ZString dimensionsUQ, ZDecimal weight, ZString weightUQ)
		{
			var orphanScan = Factory.New<JobOrphanScan>();
			orphanScan.JOS_Barcode = packageID;
			orphanScan.JOS_EventTimeUtc = eventTime;
			orphanScan.JOS_GS_NKUser = staffCode;
			orphanScan.JOS_JobType = moduleCode;
			orphanScan.JOS_Reference = reference;
			orphanScan.JOS_SE_NKEvent = eventType.Code;
			orphanScan.JOS_Length = length;
			orphanScan.JOS_Width = width;
			orphanScan.JOS_Height = height;
			orphanScan.JOS_DimensionUQ = dimensionsUQ;
			orphanScan.JOS_Weight = weight;
			orphanScan.JOS_WeightUQ = weightUQ;

			return orphanScan;
		}

		void AssertPackage(PkgPackage package, ZDateTime expectedEventTime, ZString expectedStaff, ZDateTime expectedUtcTime, Event expectedEvent, ZString expectedReference, ZDecimal expectedLength, ZDecimal expectedWidth, ZDecimal expectedHeight, ZString expectedDimensionsUQ, ZDecimal expectedWeight, ZString expectedWeightUQ, bool anonymousPacksRegistryEnabled)
		{
			var packageLog = package.Logs.GetAllLogs().Cast<StmALog>().SingleOrDefault();

			if (anonymousPacksRegistryEnabled)
			{
				AssertEquals(expectedEventTime, packageLog.SL_EventTime);
				AssertEquals(expectedStaff, packageLog.SL_GS_NKUser);
				AssertEquals(expectedUtcTime, packageLog.SL_PostedTimeUtc);
				AssertEquals(expectedEvent.Code, packageLog.SL_SE_NKEvent);
				AssertEquals(expectedReference, packageLog.SL_Reference);
				AssertDimsAndWeight(package, expectedLength, expectedWidth, expectedHeight, expectedDimensionsUQ, expectedWeight, expectedWeightUQ);
			}
			else
			{
				AssertNull(packageLog);
				AssertDimsAndWeight(package, 0, 0, 0, Constants.Length.Metres, 0, Constants.Weight.Kilograms);
			}
		}

		void AssertDimsAndWeight(PkgPackage package, ZDecimal expectedLength, ZDecimal expectedWidth, ZDecimal expectedHeight, ZString expectedDimensionsUQ, ZDecimal expectedWeight, ZString expectedWeightUQ)
		{
			CombineAssertions(() =>
			{
				AssertEquals("KP_Length", expectedLength, package.KP_Length);
				AssertEquals("KP_Width", expectedWidth, package.KP_Width);
				AssertEquals("KP_Height", expectedHeight, package.KP_Height);
				AssertEquals("KP_DimensionUQ", expectedDimensionsUQ, package.KP_DimensionUQ);
				AssertEquals("KP_Weight", expectedWeight, package.KP_Weight);
				AssertEquals("KP_WeightUQ", expectedWeightUQ, package.KP_WeightUQ);
			});
		}

		#endregion

		#endregion

		#region Delete

		public void TestDeleteFiresDeleted()
		{
			bool deletedFired = false;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.Deleted += delegate
			{ deletedFired = true; };
			AssertEquals("Precondition", false, deletedFired);

			packageJob.Delete();
			AssertEquals(true, deletedFired);
		}

		public void TestDeleteViaDataRefreshFiresDeleted()
		{
			bool deletedFiredViaDRB = false;

			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			Factory.Save();

			var packageJobInOtherFactory = new BusinessObjectFactory().Load<PkgPackageJob>(packageJob.PK);
			packageJobInOtherFactory.Deleted += delegate
			{ deletedFiredViaDRB = true; };
			AssertEquals("Precondition", false, deletedFiredViaDRB);

			packageJob.Delete();
			Factory.Save();
			AssertEquals("Deleting the PackageJob in Factory1 should fire packageJob.Deleted in Factory2.", true, deletedFiredViaDRB);
		}

		[UseSnapshotProtection]
		public void TestDelete_DeletesNumberFountain()
		{
			var packingParent = Helper.CreatePackingParent();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(packingParent);

			packingParent.JobNoForPackingParent = "D01";
			var packableItemParent = Helper.CreatePackableItemParent();
			Helper.CreatePackableItem(packableItemParent, 5m);
			Factory.Save();

			var package = Helper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box);
			package.Pack(packableItemParent, 5m);

			var customisation = PackingRegistry.Instance.PackageIDCustomisation.Value;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.JobNo].Fountain = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.CompanyCode].Fountain = true;

			using (PackingRegistry.Instance.PackageIDCustomisation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				AssertEquals(true, PackageIDGenerator.GenerateIDsForAllPackages(packageJob.Packages, Notify, SSCCGenerationContext.GeneratingIDsViaUser));
				AssertEquals("Number generation should use the Job No and company code.", "D01-EDI001", package.KP_PackageID);
				Factory.Save();
			}

			CreateNonPackingGeneratorNumberFountain();
			AssertEquals("Package job has number fountains.", 2, GetFountainsWithOwner(packageJob.PK.ToGuid()));

			var newFactory = new BusinessObjectFactory();
			var packageJobInNewFactory = newFactory.Load<PkgPackageJob>(packageJob.PK);
			packageJobInNewFactory.Delete();
			newFactory.Save();

			AssertEquals("Package job has all generator number fountains deleted.", 0, GetFountainsWithOwner(packageJob.PK.ToGuid()));

			int GetFountainsWithOwner(Guid packingJobPK)
			{
				var sql = "SELECT COUNT(*) FROM dbo.StmNums WHERE SN_Owner = @owner";
				var command = ((IDbConnected)Factory).Connection.Command(sql);
				command.AddParameter("@owner", SqlDbType.UniqueIdentifier, packingJobPK);

				return (int)command.ExecuteScalar();
			}

			void CreateNonPackingGeneratorNumberFountain()
			{
				var sql = @"-- InsertStmNums
INSERT dbo.StmNums (SN_Name, SN_Owner, SN_MinimumValue, SN_Value, SN_MaximumValue) VALUES
	(@Name, @Owner, 1, 1, 9220000000000000000);";

				using (var cmd = TestConnection.Command(sql))
				{
					cmd.AddParameter("@Name", SqlDbType.VarChar, "SomeName");
					cmd.AddParameter("@Owner", SqlDbType.UniqueIdentifier, packageJob.PK.ToGuid());
					cmd.ExecuteNonQuery();
				}
			}
		}

		#endregion

		#region DataRefresh

		public void TestDataRefresh_FinalisedPackageJobUpdatesReadOnlyInOtherFactories()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob.Packages.AddNew(); // to prevent deletion of empty finalised job
			Factory.Save();
			var packageJobInOtherFactory = new BusinessObjectFactory().Load<PkgPackageJob>(packageJob.PK);

			packageJob.KJ_IsFinalized = true;
			AssertEquals("Precondition", true, packageJob.HasChanges);
			AssertEquals("Precondition", true, packageJob.ReadOnly);
			AssertEquals("Precondition", false, packageJobInOtherFactory.ReadOnly);
			AssertEquals("Precondition", false, packageJobInOtherFactory.HasChanges);

			Factory.Save();
			AssertEquals(true, packageJob.ReadOnly);
			AssertEquals("Since Package job changes are saved, HasChanges must be false.", false, packageJob.HasChanges);
			AssertEquals("Package was Finalised in another Factory, should be ReadOnly across all factories.", true, packageJobInOtherFactory.ReadOnly);
			AssertEquals("Package Job in the other factory must not have any changes.", false, packageJobInOtherFactory.HasChanges);
		}

		public void TestDataRefresh_FinalisePackageJobAfterPackagesLoadedInAnotherFactory()
		{
			var isFinalisedChangedCount = 0;
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			var package = packageJob.Packages.AddNew();
			Factory.Save();
			var packageJobInOtherFactory = new BusinessObjectFactory().Load<PkgPackageJob>(packageJob.PK);
			var packageInOtherFactory = packageJobInOtherFactory.Packages.Single();
			packageJobInOtherFactory.IsFinalisedChanged += delegate
			{ isFinalisedChangedCount++; };

			packageJob.KJ_IsFinalized = true;
			AssertEquals("Precondition", true, package.IsClosed);
			AssertEquals("Precondition", true, package.HasChanges);
			AssertEquals("Precondition", false, packageInOtherFactory.IsClosed);
			AssertEquals("Precondition", false, packageInOtherFactory.HasChanges);
			AssertEquals("Precondition", 0, isFinalisedChangedCount);

			Factory.Save();
			AssertEquals("Package must still remain closed in original factory.", true, package.IsClosed);
			AssertEquals("Package in original factory must not have any changes.", false, package.HasChanges);
			AssertEquals("Package in other factory must be changed now.", true, packageInOtherFactory.IsClosed);
			AssertEquals("Package in other factory must not have any changes.", false, packageInOtherFactory.HasChanges);
			AssertEquals("IsFinalisedChanged event must be fired once.", 1, isFinalisedChangedCount);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var pkgPackageJob = Factory.New<PkgPackageJob>();
			AssertEquals("Package Job", pkgPackageJob.HumanReadableName);

			pkgPackageJob.KJ_JobID = "2";
			AssertEquals("Package Job 2", pkgPackageJob.HumanReadableName);
		}

		#endregion

		#region TestIPackageSummary

		public void TestIPackageSummary()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packageSummary = (IPackageSummary)packageJob;

			// these fields are ignored
			AssertEquals("", packageSummary.DimensionsCaption);
			AssertEquals("", packageSummary.PackageIDCaption);
			AssertEquals("", packageSummary.TemperatureCaption);
			AssertEquals("", packageSummary.CommodityCaption);
			AssertEquals("", packageSummary.Dimensions);
			AssertEquals("", packageSummary.PackageID);
			AssertEquals("", packageSummary.IsHeldCaption);
			AssertEquals("", packageSummary.IsHeld);
			AssertEquals("", packageSummary.HandlingUnit);

			AssertEquals("Wgt", packageSummary.WeightCaption);
			AssertEquals("Vol", packageSummary.VolumeCaption);
			AssertEquals("0 KG", packageSummary.Weight);
			AssertEquals("0 M3", packageSummary.Volume);
			AssertEquals("", packageSummary.Contents);
			AssertEquals("", packageSummary.Commodity);
			AssertEquals("", packageSummary.HandlingUnitCaption);

			AssertEquals(false, packageSummary.IsTemperatureControlled);
			AssertEquals("", packageSummary.Temperature);

			// add a pallet to the job
			var pallet = packageJob.Packages.AddNew();
			pallet.KP_F3_NKPackType = "PLT";
			pallet.KP_WeightUQ = Constants.Weight.Kilograms;
			pallet.KP_VolumeUQ = Constants.Volume.CubicMetres;
			pallet.KP_Weight = 1021;
			pallet.KP_Volume = 2010.5126;

			// add a box to the pallet
			var box = pallet.Packages.AddNew();
			box.KP_F3_NKPackType = "BOX";
			box.KP_WeightUQ = Constants.Weight.Kilograms;
			box.KP_VolumeUQ = Constants.Volume.CubicMetres;
			box.KP_Weight = 10;
			box.KP_Volume = 10;

			// only top-level packages are relevant (the box should be ignored)
			AssertEquals("1,031 KG", packageSummary.Weight);
			AssertEquals("2,010.513 M3", packageSummary.Volume);
			AssertEquals("1x PLT", packageSummary.Contents);
		}

		public void TestIPackageSummary_Status()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;
			var packageSummary = (IPackageSummary)packageJob;
			AssertEquals("No packages, should have no status significance.", false, packageSummary.IsStatusSignificant);

			var pallet1 = packageJob.Packages.AddNew("PLT");
			var box = pallet1.Packages.AddNew("BOX"); // inners should not be considered for job release status
			AssertEquals("Not all packages are released, Job should *not* be RELEASED.", "", packageSummary.Status);
			AssertEquals(false, packageSummary.IsStatusSignificant);

			pallet1.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Not all packages are released, Job should *not* be RELEASED.", "", packageSummary.Status);
			AssertEquals(false, packageSummary.IsStatusSignificant);

			packageJob.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Job was directly released, Job should be RELEASED VIA JOB.", PackageStatuses.ReleasedViaJob, packageSummary.Status);
			AssertEquals(false, packageSummary.IsStatusSignificant);

			pallet1.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("All packages individually released, Job should be RELEASED.", PackageStatuses.Released, packageSummary.Status);
			AssertEquals("All packages individually released, status should be significant.", true, packageSummary.IsStatusSignificant);

			var pallet2 = packageJob.Packages.AddNew("PLT");
			AssertEquals("One package is unreleased, Job should revert to be RELEASED VIA JOB.", PackageStatuses.ReleasedViaJob, packageSummary.Status);
			AssertEquals(false, packageSummary.IsStatusSignificant);

			pallet2.KP_ReleasedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("All packages individually released, Job should be RELEASED.", PackageStatuses.Released, packageSummary.Status);
			AssertEquals(true, packageSummary.IsStatusSignificant);
		}

		#endregion

		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var documentSupportable = (IDocumentSupportable)packageJob;

			AssertEquals(packageJob, documentSupportable.DocumentSupporter.BusinessObject);
			AssertEquals(typeof(PkgPackageJobDocumentSupporter), documentSupportable.DocumentSupporter.GetType());
		}

		public void TestDocumentSupporterBusinessContext()
		{
			Data.CreatePackingData();
			AssertEquals("BusinessContext", BusinessContext.Packing, ((IDocumentSupportable)Data.PackageJob).DocumentSupporter.BusinessContext);
		}

		public void TestDocumentSupporterSupportedDataContext()
		{
			Data.CreatePackingData();
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, ((IDocumentSupportable)Data.PackageJob).DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestPackageSupportedDataContexts()
		{
			Data.CreatePackingData();
			AssertEquals("The dummy package job should contain it's dummy DataContexts comma separated so they can be filtered by a Document Menu.", "GenericAuditVarianceLabel,GenericAuditVarianceLabelAll", Data.PackageJob.PackageSupportedDataContexts.ToString());
		}

		#endregion

		#region TestUniversalCopyAttributes

		public void TestUniversalCopyAttributes()
		{
			Data.CreatePackingData();
			var packageJob = Data.PackageJob;

			var componentType = packageJob.GetType();
			AssertEquals("PkgPackageJob should have UniversalCopyWithExtendedEntitiesAttribute.", true, componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), true).Length > 0);

			var parentTableCodeInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == PkgPackageJobSchema.Constants.KJ_ParentTableCode);
			AssertEquals("KJ_ParentTableCode should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, parentTableCodeInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));

			var packagesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "Packages");
			AssertEquals("Packages collection should have UniversalCopyCollectionEntityAttribute.", true, packagesInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);
		}

		#endregion

		#region TestCancelPackageCarrierLabel

		public void TestCancelPackageCarrierLabel()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });

			AssertEquals("CancelPackageCarrierLabel is successful.", true, result.Success);
			AssertEquals("CancelPackageCarrierLabel is successful.", true, package.ShouldCancelCarrierLabelOnSaving);
		}

		public void TestCancelPackageCarrierLabel_NotAllOnTheSamePackageJob()
		{
			Data.CreatePackingData();
			var package1 = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");

			var package2 = Factory.New<PkgPackage>();
			package2.KP_F3_NKPackType = "PLT";
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			package2.KP_KJ_ParentPackageJob = packageJob.PK;
			Factory.Save();

			var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package1, package2 });

			AssertEquals("CancelPackageCarrierLabel is not successful.", false, result.Success);
			AssertEquals("CancelPackageCarrierLabel is not successful.", false, package1.ShouldCancelCarrierLabelOnSaving);
			AssertEquals("CancelPackageCarrierLabel is not successful.", false, package2.ShouldCancelCarrierLabelOnSaving);
			AssertEquals("One or more packages does not belong to the same package job.", result.Message);
		}

		public void TestCancelPackageCarrierLabel_NoCancellationOccursWhenParentHasNoParentJobType()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Precondition: Package cannot cancel carrier label.", false, package.CanCancelPackageLabel);

			var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });

			AssertEquals(false, result.Success);
			AssertEquals("One or more packages cannot cancel package label.", result.Message);
		}

		public void TestCancelPackageCarrierLabel_NoCancellationOccursWhenParentHasNoCarrierBookingAgent()
		{
			Data.CreatePackingData();
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = true;
			Factory.Save();

			AssertEquals("Precondition: Package cannot cancel carrier label.", false, package.CanCancelPackageLabel);

			var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });

			AssertEquals(false, result.Success);
			AssertEquals("One or more packages cannot cancel package label.", result.Message);
		}

		public void TestCancelPackageCarrierLabel_NoCancellationOccursWhenPackageIsNotSentToRTUS()
		{
			Data.CreatePackingData();
			var smartFreight = Factory.NewWithValidTestData<OrgHeader>();
			Data.Dummy.CarrierBookingAgent = smartFreight;
			Data.Dummy.ParentJobType = ParentJobType.Dummy;

			var package = Data.PackageJob.Packages.AddNew("PLT", "ABC-1");
			package.IsSentToRTUS = false;
			Factory.Save();

			AssertEquals("Precondition: Package cannot cancel carrier label.", false, package.CanCancelPackageLabel);

			var result = Data.PackageJob.CancelPackageCarrierLabel(new[] { package });

			AssertEquals(false, result.Success);
			AssertEquals("One or more packages cannot cancel package label.", result.Message);
		}

		public void TestCancelPackageCarrierLabel_ArgumentCheck()
		{
			Data.CreatePackingData();
			AssertExceptionThrown<ArgumentNullException>("Null collection is not accepted.", () => Data.PackageJob.CancelPackageCarrierLabel(null));
		}

		public void TestCancelPackageCarrierLabel_ArgumentCheck_NullPackageInCollection()
		{
			Data.CreatePackingData();
			AssertExceptionThrown<ArgumentException>("Cannot have null Package elements in Collection.", () => Data.PackageJob.CancelPackageCarrierLabel(new PkgPackage[] { null }));
		}

		public void TestCancelPackageCarrierLabel_EmptyCollection()
		{
			Data.CreatePackingData();
			var result = Data.PackageJob.CancelPackageCarrierLabel(new List<PkgPackage>());

			AssertEquals(false, result.Success);
			AssertEquals("No packages to cancel package label.", result.Message);
		}

		#endregion

		#region TestCheckAndFixSequence

		public void TestFixSequence_SameJobID()
		{
			Data.CreatePackingData();
			var newDummy = Factory.New<DummyWithPacking>();
			Factory.Save();

			var jobID = Data.PackageJob.KJ_JobID;
			var newPackageJob = PkgPackageJob.LoadOrCreatePackageJob(newDummy);
			newPackageJob.KJ_JobID = jobID;

			newPackageJob.CheckAndFixSequence();

			AssertCollectionContains(ErrorReporter.ExceptionsThrown, (p) => p.Contains("Number Fountain adjust failed after an unique index violation"));

			ErrorReporter.Clear();
		}

		public void TestFixSequence()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob = Data.PackageJob;
			CreatePackagesWithSequence(packageJob, 1, 3, 5, 7);

			packageJob.CheckAndFixSequence();

			AssertPackagesWithSequence(packageJob, 1, 2, 3, 4);
		}

		public void TestFixSequence_WithOuterPackages()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob = Data.PackageJob;
			CreatePackagesWithSequence(packageJob, 1, 3, 5, 7);
			var outerPackages = packageJob.Packages.Where(p => p.KP_Sequence != 7);

			PkgPackageJob.CheckAndFixSequence(packageJob.ParentJob, outerPackages);

			AssertPackagesWithSequence(packageJob, 1, 2, 3, 7);
		}

		public void TestFixSequence_WithNonHeaderOuterPackages()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob = Data.PackageJob;
			CreatePackagesWithSequence(packageJob, 1, 3, 5, 7);
			var outerPackages = packageJob.Packages.Where(p => p.KP_Sequence != 7);

			var package = packageJob.Packages.AddNew();
			package.KP_Sequence = 4;
			outerPackages.Append(package);

			PkgPackageJob.CheckAndFixSequence(packageJob.ParentJob, outerPackages);

			AssertPackagesWithSequence(packageJob, 1, 2, 3, 7);
		}

		public void TestFixSequence_LoosePackageAndPackageCombined()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob = Data.PackageJob;
			CreatePackagesWithSequence(packageJob, 1, 3, 5, 7);
			CreateLoosePackagesWithSequence(packageJob, 7, 7, 7, 7);

			packageJob.CheckAndFixSequence();

			AssertPackagesWithSequence(packageJob, 1, 2, 3, 4, 5, 6, 7, 8);
		}

		public void TestFixSequence_DuplicatedSequence()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob = Data.PackageJob;
			CreatePackagesWithSequence(packageJob, 1, 7, 7, 7);

			packageJob.CheckAndFixSequence();

			AssertPackagesWithSequence(packageJob, 1, 2, 3, 4);
		}

		public void TestFixSequence_IgnoreInnerPackages()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;
			var job = Data.PackageJob;
			CreatePackagesWithSequence(job, 1, 3, 5, 7, 9, 11);
			CreateLoosePackagesWithSequence(job, 7, 7);
			job.Packages[0].KP_KP_ParentPackage = job.Packages[2].PK;
			job.Packages[1].KP_KP_ParentPackage = job.Packages[2].PK;

			job.CheckAndFixSequence();

			AssertPackagesWithSequence(job, 1, 2, 3, 4, 5, 6);
			var innerPackages = job.GetAllPackagesOnJob().Where(p => !p.KP_KP_ParentPackage.IsEmpty);
			AssertEquals(true, innerPackages.All(p => p.KP_Sequence == 0));
		}

		public void TestFixSequence_IgnorePackageWithoutHeader()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;
			var job = Data.PackageJob;
			CreatePackagesWithSequence(job, 1, 2, 3, 4);
			var packageWithoutHeader = job.Packages.FirstOrDefault(p => p.KP_Sequence == 1);
			packageWithoutHeader.KP_KPH_PackageHeader = ZGuid.Empty;

			job.CheckAndFixSequence();

			AssertPackagesWithSequence(job, 1, 2, 3);

			var packageWithoutHeaderAfter = job.Packages.First(p => p.KP_KPH_PackageHeader.IsEmpty);
			AssertEquals(ZShort.Zero, packageWithoutHeaderAfter.KP_Sequence);
		}

		public void TestFixSequence_Oversized()
		{
			Data.CreatePackingData();
			Data.Dummy.PackageSequenceType = PackageSequenceType.OuterWithLooseID;

			var packageJob = Data.PackageJob;
			var oversizedSequences = new ZShort[short.MaxValue];
			var expectedSequences = new ZShort[short.MaxValue + 2];

			for (int i = 0; i < short.MaxValue; i++)
			{
				oversizedSequences[i] = 3;
				expectedSequences[i] = (short)(i + 1);
			}
			expectedSequences[short.MaxValue] = short.MaxValue;
			expectedSequences[short.MaxValue + 1] = short.MaxValue;

			CreatePackagesWithSequence(packageJob, oversizedSequences);
			CreateLoosePackagesWithSequence(packageJob, 3, 3);

			packageJob.CheckAndFixSequence();
			AssertEquals($"Current Package Job {packageJob.PK} has too many outer packages, cannot fix the sequence correctly.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertPackagesWithSequence(packageJob, expectedSequences);
		}

		void CreatePackagesWithSequence(PkgPackageJob job, params ZShort[] sequence)
		{
			for (int i = 0; i < sequence.Length; i++)
			{
				var packageHeader = Factory.New<PkgPackageHeader>();
				packageHeader.KPH_PackageID = $"P{i}";
				var package = job.Packages.AddNew();
				package.KP_Sequence = sequence[i];
				package.KP_KPH_PackageHeader = packageHeader.PK;
			}
		}

		void CreateLoosePackagesWithSequence(PkgPackageJob job, params ZShort[] sequence)
		{
			for (int i = 0; i < sequence.Length; i++)
			{
				var packageHeader = Factory.New<PkgPackageHeader>();
				packageHeader.KPH_PackageID = $"LP{i}";
				var loosePackage = job.LoosePackagePivots.AddNew();
				loosePackage.KPJ_Sequence = sequence[i];
				loosePackage.KPJ_KPH_PackageHeader = packageHeader.PK;
			}
		}

		void AssertPackagesWithSequence(PkgPackageJob job, params ZShort[] expected)
		{
			var actual = job.LoosePackagePivots.Select(p => p.KPJ_Sequence)
				.Concat(job.Packages.Where(p => !p.KP_KPH_PackageHeader.IsEmpty).Select(c => c.KP_Sequence).ToArray());
			AssertContainsExactElementsInAnyOrder(expected, actual);
		}

		#endregion

		#region TestCriticalChangesVersionIDAndConcurrency

		[UseSnapshotProtection(true)]
		public void TestConcurrencyOnFinalization_EndTo_End_PackageAdded()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			job.Packages.AddNew("PLT");
			Factory.Save();
			job.KJ_IsFinalized = true;

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(secondConnection);
				factory.RefreshEnabled = false;
				var jobInFactory = factory.Load<PkgPackageJob>(job.PK);
				jobInFactory.Packages.AddNew("BOX");
				factory.Save();
			}

			AssertExceptionThrown(
				"Should have thrown a save exception as job has been modified while finalizing.",
				typeof(ZSaveConcurrencyException), () => Factory.Save());
		}

		[UseSnapshotProtection(true)]
		public void TestConcurrencyAfterFinalization_EndTo_End_PackageAdded()
		{
			Data.CreatePackingData();
			var job = Data.PackageJob;
			job.Packages.AddNew("PLT");
			Factory.Save();

			using (var secondConnection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(secondConnection);
				factory.RefreshEnabled = false;
				var jobInFactory = factory.Load<PkgPackageJob>(job.PK);
				jobInFactory.KJ_IsFinalized = true;
				factory.Save();
			}

			job.Packages.AddNew("BOX");
			AssertExceptionThrown("Should have thrown a save exception as job has been finalised.",
				typeof(ZSaveConcurrencyException), () => Factory.Save());
		}

		#endregion

		#region TestParentTableCodeConstraint

		public void TestParentTableCodeConstraint_Allow_CUL() => AssertTestParentTableCodeConstraint("CUL", true);
		public void TestParentTableCodeConstraint_Allow_JS() => AssertTestParentTableCodeConstraint("JS", true);
		public void TestParentTableCodeConstraint_Allow_KB() => AssertTestParentTableCodeConstraint("KB", true);
		public void TestParentTableCodeConstraint_Allow_KM() => AssertTestParentTableCodeConstraint("KM", true);
		public void TestParentTableCodeConstraint_Allow_KPU() => AssertTestParentTableCodeConstraint("KPU", true);
		public void TestParentTableCodeConstraint_Allow_LTC() => AssertTestParentTableCodeConstraint("LTC", true);
		public void TestParentTableCodeConstraint_Allow_WD() => AssertTestParentTableCodeConstraint("WD", true);
		public void TestParentTableCodeConstraint_Allow_WDC() => AssertTestParentTableCodeConstraint("WDC", true);
		public void TestParentTableCodeConstraint_Allow_WDH() => AssertTestParentTableCodeConstraint("WDH", true);
		public void TestParentTableCodeConstraint_Allow_WRC() => AssertTestParentTableCodeConstraint("WRC", true);
		public void TestParentTableCodeConstraint_Allow_WRH() => AssertTestParentTableCodeConstraint("WRH", true);
		public void TestParentTableCodeConstraint_OtherNotAllowed_WA() => AssertTestParentTableCodeConstraint("WA", false);
		public void TestParentTableCodeConstraint_OtherNotAllowed_WW() => AssertTestParentTableCodeConstraint("WW", false);

		public void AssertTestParentTableCodeConstraint(string parentTableCode, bool expectedAllowed)
		{
			var job = Factory.New<PkgPackageJob>();
			job.KJ_ParentTableCode = parentTableCode;
			job.KJ_ParentID = ZGuid.NewZGuid();

			if (expectedAllowed)
			{
				AssertNoExceptionThrown(Factory.Save);
			}
			else
			{
				var expectedErrorMsg = "The INSERT statement conflicted with the CHECK constraint \"Constraint_KJ_ParentTableCode_NoCheck\".";
				AssertInnermostException("SqlException should throw", typeof(SqlException), expectedErrorMsg, Factory.Save, true);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			Data.CreatePackingData();
			return Data.PackageJob;
		}

		#endregion
	}
}
