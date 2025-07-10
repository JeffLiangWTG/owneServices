using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	[TestedType(typeof(PkgPackageDocManagerInfo))]
	class PkgPackageDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestGetRelatedObjects

		public void TestGetRelatedObjects()
		{
			var packageWithNoRelatedObjects = Factory.New<PkgPackage>();
			AssertNull(packageWithNoRelatedObjects.GetRelatedBusinessObjects);

			var packageDocManagerWithNoRelatedBizOs = new PkgPackageDocManagerInfo(packageWithNoRelatedObjects);
			AssertEquals(0, packageDocManagerWithNoRelatedBizOs.RelatedObjects.Length);

			var dummyBizO = Factory.New<DummyDocManagerTestBizO>();
			var packageWithRelatedObjects = Factory.New<PkgPackage>();
			packageWithRelatedObjects.GetRelatedBusinessObjects = () => new BusinessObject[] { dummyBizO };
			var packageDocManagerWithRelatedBizOs = new PkgPackageDocManagerInfo(packageWithRelatedObjects);
			AssertContainsExactElementsInAnyOrder(new[] { dummyBizO }, packageDocManagerWithRelatedBizOs.RelatedObjects);
		}

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<PkgPackage>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return Factory.NewWithValidTestData<PkgPackage>();
		}

		#endregion
	}
}
