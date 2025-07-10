using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Packing.DataTransfer.Testing
{
	class PkgPackageHandlingUnitDivotHelperTest : PackingTestCaseWithFactory
	{
		public void TestCreatePackageHandlingUnitDivot_DivotsCreated()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackingParent);
			var parentJob = Factory.New<DummyPackingParent>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(parentJob);

			var hu = packageJob.Packages.AddNew(PackType.Freight.PLT, "HU1");
			var huInner1 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU1Inner1");
			var huInner2 = packageJob.Packages.AddNew(PackType.Freight.PKG, "HU1Inner2");
			hu.Packages.Add(huInner1);
			hu.Packages.Add(huInner2);

			Assert("Precondition - Top HU must not have top handling unit package set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Top HU must not have parent package set.", hu.KP_KP_ParentPackage.IsEmpty);
			AssertEquals("Precondition - Parent package has been set.", hu.PK, huInner1.KP_KP_ParentPackage);
			AssertEquals("Precondition - Parent package has been set.", hu.PK, huInner2.KP_KP_ParentPackage);
			Assert("Precondition - Top handling unit package has not been set.", huInner1.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Precondition - Top handling unit package has not been set.", huInner2.KP_KP_TopHandlingUnitPackage.IsEmpty);
			AssertEquals("Precondition - No Divots have been created yet.", 0, Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery()).Length);

			PkgPackageHandlingUnitDivotHelper.ReplaceParentPackageWithDivotForLabelledPackages(Factory, huInner1);
			PkgPackageHandlingUnitDivotHelper.ReplaceParentPackageWithDivotForLabelledPackages(Factory, huInner2);

			Assert("Top handling unit package has not been set.", hu.KP_KP_TopHandlingUnitPackage.IsEmpty);
			Assert("Parent package has been cleared.", hu.KP_KP_ParentPackage.IsEmpty);
			AssertEquals("Parent package has not been cleared.", hu.PK, huInner1.KP_KP_ParentPackage);
			AssertEquals("Parent package has not been cleared.", hu.PK, huInner2.KP_KP_ParentPackage);
			AssertEquals("Top handling unit package has been set.", hu.PK, huInner1.KP_KP_TopHandlingUnitPackage);
			AssertEquals("Top handling unit package has been set.", hu.PK, huInner2.KP_KP_TopHandlingUnitPackage);

			var divots = Factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery());

			AssertEquals("Should have created 2 divots", 2, divots.Length);

			var huInner1Divot = divots.Single(d => d.KPD_KP_Package == huInner1.PK);
			AssertEquals(huInner1Divot.KPD_KP_HandlingUnit, hu.PK);
			AssertEquals(huInner1Divot.KPD_GS_NKPackedUser, Env.CurrentUser.Initials);
			AssertEquals(false, huInner1Divot.KPD_PackedTime.IsEmpty);

			var huInner2Divot = divots.Single(d => d.KPD_KP_Package == huInner2.PK);
			AssertEquals(huInner2Divot.KPD_KP_HandlingUnit, hu.PK);
			AssertEquals(huInner2Divot.KPD_GS_NKPackedUser, Env.CurrentUser.Initials);
			AssertEquals(false, huInner2Divot.KPD_PackedTime.IsEmpty);
		}
	}
}
