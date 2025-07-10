
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageCollectionContainersOnly))]
	class PkgPackageCollectionContainersOnlyTest : PkgPackageCollectionCommonTest<PkgPackageCollectionContainersOnly>
	{
		#region TestAdd_SetsPackType

		protected override ZString ExpectedDefaultPackType
		{
			get { return Constants.PkgUnit.Container; }
		}

		#endregion

		#region TestRelationshipFilter

		public void TestRelationshipFilter()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var collection = new PkgPackageCollectionContainersOnly(packageJob);
			var newContainerPackage = collection.AddNew();
			AssertEquals(true, newContainerPackage.IsContainer);

			newContainerPackage.KP_F3_NKPackType = Constants.PkgUnit.Box;
			AssertEquals(0, collection.Count);

			var package = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			AssertContainsExactElementsInAnyOrder(new[] { package }, collection);
		}

		#endregion

		#region Implementation

		protected override PkgPackageCollectionContainersOnly GetCollectionToTest()
		{
			return new PkgPackageCollectionContainersOnly(Factory.New<PkgPackageJob>());
		}

		protected override PkgPackageCollectionContainersOnly GetNewCollection(PkgPackageJob packageJob)
		{
			return new PkgPackageCollectionContainersOnly(packageJob);
		}

		#endregion
	}
}
