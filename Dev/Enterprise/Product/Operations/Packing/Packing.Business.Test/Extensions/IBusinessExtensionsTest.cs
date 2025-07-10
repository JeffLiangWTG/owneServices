using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.Packing.Business.Testing
{
	public class IBusinessExtensionsTest : PackingTestCaseWithFactory
	{
		public void TestHasChangesOnChildrenNotValidIfFinalised()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = ZGuid.NewZGuid();
			packageJob.KJ_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;
			packageJob.Logs.AddNew();
			var note = packageJob.Notes.AddNew();
			AssertEquals("Added Logs and Notes are *not* Packing changes, should be false.", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			note.ST_NoteText = "xXx";
			AssertEquals("Changing a Note is *not* a Packing change, should be false.", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			var package = packageJob.Packages.AddNew();
			AssertEquals("Adding an IPackingHasChanges object (PkgPackage) is a Packing change, should be true.", true, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			Factory.Save();
			AssertEquals("Precondition", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());
			package.Logs.AddNew();
			package.Notes.AddNew();
			AssertEquals("Adding Logs and Notes to a Package is *not* a Packing change, should be false.", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			var divot = package.PackedItemDivots.AddNew();
			divot.KI_PackedQty = 1m; // cannot have 0 qty divot
			divot.KI_ParentTableCode = "ZD1";
			AssertEquals("Adding an IPackingHasChanges object (PkgPackageItemDivot) is a Packing change, should be true.", true, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			Factory.Save();
			AssertEquals("Precondition", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());
			divot.Logs.AddNew();
			divot.Notes.AddNew();
			AssertEquals("Adding Logs and Notes to a Divot is *not* a Packing change, should be false.", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			divot.KI_PackedQty = 10m;
			AssertEquals("Changing an IPackingHasChanges object (PkgPackageItemDivot) is a Packing change, should be true.", true, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			package.KP_F3_NKPackType = "CNT";
			package.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();
			AssertEquals("Precondition", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());
			package.Container.Logs.AddNew();
			package.Container.Notes.AddNew();
			AssertEquals("Adding Logs and Notes to a Container is *not* a Packing change, should be false.", false, packageJob.HasChangesOnChildrenNotValidIfFinalised());

			package.Container.K0_HumidityPercent = 5;
			AssertEquals("Changing an IPackingHasChanges object (PkgPackageContainer) is a Packing change, should be true.", true, packageJob.HasChangesOnChildrenNotValidIfFinalised());
		}
	}
}
