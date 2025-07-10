using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageHandlingUnitDivot))]
	public class PkgPackageHandlingUnitDivotTest : PackingBusinessObjectTestCase
	{
		#region TestHandlingUnit

		public void TestHandlingUnit()
		{
			var handlingUnitDivot = Factory.New<PkgPackageHandlingUnitDivot>();
			AssertNull(handlingUnitDivot.HandlingUnit);

			var handlingUnit = Factory.New<PkgPackage>();
			handlingUnitDivot.KPD_KP_HandlingUnit = handlingUnit.PK;
			AssertEquals(handlingUnit, handlingUnitDivot.HandlingUnit);
		}

		#endregion

		#region TestPackage

		public void TestPackage()
		{
			var handlingUnitDivot = Factory.New<PkgPackageHandlingUnitDivot>();
			AssertNull(handlingUnitDivot.Package);

			var package = Factory.New<PkgPackage>();
			handlingUnitDivot.KPD_KP_Package = package.PK;
			AssertEquals(package, handlingUnitDivot.Package);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return MakeSaveable((PkgPackageHandlingUnitDivot)base.GetNewBusinessObjectForDeleteTest(factory));
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return MakeSaveable((PkgPackageHandlingUnitDivot)base.GetBusinessObjectForFetchForLoad());
		}

		PkgPackageHandlingUnitDivot MakeSaveable(PkgPackageHandlingUnitDivot divot)
		{
			var packageJob = divot.Package.PackageJob;
			divot.Package.KP_KJ_ParentPackageJob = divot.HandlingUnit.KP_KJ_ParentPackageJob;
			packageJob.Delete();

			return divot;
		}

		#endregion
	}
}
