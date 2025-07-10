using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(PkgPackageJob))]
	sealed class CusPackageJobTest : PkgPackageJobTest
	{
		#region Static Loader

		#region TestLoadPackageJob

		public new void TestLoadPackageJob_ViaParent()
		{
			var dummy = Factory.New<DummyWithPacking>();

			var packageJob = CusPackageJob.LoadOrCreatePackageJob(dummy);
			packageJob.KJ_IsFinalized = true;
			packageJob.ReadOnly = false;
			AssertEquals("Precondition", false, packageJob.ReadOnly);
			AssertEquals(1, dummy.OnPackageJobCreatedOrLoadedCount);

			dummy.UnRegisterEditableChildObject(packageJob);
			AssertEquals("The existing PackageJob was not loaded.", packageJob, CusPackageJob.LoadPackageJob(dummy));
			AssertEquals("The PackageJob was not registered as editable on the parent (Dummy).", true, dummy.IsRegisteredEditableChildObject(packageJob));
			AssertEquals("ReadOnly should be updated on load.", true, packageJob.ReadOnly);
		}

		public new void TestLoadPackageJob_ViaPK()
		{
			var dummy = Factory.New<DummyWithPacking>();

			var packageJob = CusPackageJob.LoadOrCreatePackageJob(dummy);
			packageJob.KJ_IsFinalized = true;
			packageJob.ReadOnly = false;
			AssertEquals("Precondition", false, packageJob.ReadOnly);

			AssertEquals("The existing PackageJob was not loaded.", packageJob, CusPackageJob.LoadPackageJob(Factory, packageJob.PK));
			AssertEquals("ReadOnly should be updated on load.", true, packageJob.ReadOnly);
		}

		#endregion

		#endregion

		public void TestSupportsClone()
		{
			AssertEquals(true, Factory.New<CusPackageJob>().SupportsClone());
		}

		public void TestIsSavedByFactory()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackageJob = cusPackingList.PackageJob;

			Assert("HasChanges is false", !cusPackageJob.HasChanges);
			Assert("IsSavedByFactory should be false when no changes ", !cusPackageJob.IsSavedByFactory);

			cusPackageJob.Packages.AddNew().HasChanges = false;
			cusPackageJob.HasChanges = false;

			Assert("HasChanges is false", !cusPackageJob.HasChanges);
			Assert("IsSavedByFactory should be true when have any Packages ", cusPackageJob.IsSavedByFactory);
		}

		public void TestPackagesType()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackageJob = cusPackingList.PackageJob;

			AssertType(typeof(CusPackageCollection), cusPackageJob.Packages);
		}

		public void TestPackingList()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			AssertEquals(cusPackingList.PK, cusPackingList.PackageJob.PackingList.PK);
		}

		public void TestNoExceptionOnDeletePackage()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var pack1 = cusPackingList.PackageJob.Packages.AddNew();
			var pack2 = cusPackingList.PackageJob.Packages.AddNew();
			pack2.KP_KP_ParentPackage = pack1.PK;
			Factory.Save();

			cusPackingList = new BusinessObjectFactory().Load<CusPackingList>(cusPackingList.PK);
			AssertNoExceptionThrown(() => cusPackingList.PackageJob.Packages[0].Delete());
		}

		public override void TestKJ_JobIDIsSetOnSave()
		{
			ZString nextNumber = Env.NumberFountains.PackingListID.PeekPreliminaryFormatted(Factory);
			AssertEquals("Precondition", true, !nextNumber.IsEmpty);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var packageJob = cusPackingList.PackageJob;
			var package = packageJob.Packages.AddNew();
			AssertEquals("Precondition", true, packageJob.KJ_JobID.IsEmpty);
			AssertEquals(true, packageJob.HasChanges);

			Factory.Save();
			AssertEquals(nextNumber, packageJob.KJ_JobID);

			packageJob.KJ_ParentID = ZGuid.BrettsGuid;
			Factory.Save();
			AssertEquals("The Packing ID should not be set twice.", nextNumber, packageJob.KJ_JobID);

			var packageJobToCreateSaveFailure = Factory.New<CusPackageJob>();
			packageJobToCreateSaveFailure.Packages.AddNew();
			packageJobToCreateSaveFailure.KJ_JobID = packageJob.KJ_JobID;

			AssertExceptionThrown("Precondition - Should have thrown a save exception as the JobIDs are duplicated.", typeof(ZSaveException), () => Factory.Save());
			AssertEquals("The Packing ID should not be cleared on save failure as the job is already in the DB.", nextNumber, packageJob.KJ_JobID);
		}
	}
}
