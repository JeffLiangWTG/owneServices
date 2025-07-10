using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Packing.Business.Testing
{
	public abstract class PkgPackageCollectionCommonTest<T> : ActiveBusinessObjectCollectionTestCase<T>
		where T : PkgPackageCollectionCommon
	{
		#region TestRemoveWhenMasterIsPackageJob

		public void TestRemoveWhenMasterIsPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = GetNewCollection(packageJob);

			var package = collection.AddNew();
			AssertContainsExactElementsInAnyOrder("Precondition", package, collection);

			// assign the package to another parent
			package.KP_KJ_ParentPackageJob = Factory.New<PkgPackage>().PK;
			AssertEquals(0, collection.Count);
		}

		#endregion

		#region TestAdd_PackageAlwaysLinksToPackageJob

		public void TestAdd_PackageAlwaysLinksToPackageJob()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = GetNewCollection(packageJob);
			var package = collection.AddNew();
			AssertEquals("Precondition", packageJob.PK, package.KP_KJ_ParentPackageJob);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = "CNT";
			collection.Add(container);
			AssertEquals(packageJob.PK, container.KP_KJ_ParentPackageJob);
		}

		#endregion

		#region TestAdd_SetsPackType

		public void TestAdd_SetsPackType()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = GetNewCollection(packageJob);
			AssertEquals(ExpectedDefaultPackType, collection.AddNew().KP_F3_NKPackType);
		}

		protected abstract ZString ExpectedDefaultPackType { get; }

		#endregion

		#region Implementation

		protected abstract T GetNewCollection(PkgPackageJob packageJob);

		#endregion
	}
}
