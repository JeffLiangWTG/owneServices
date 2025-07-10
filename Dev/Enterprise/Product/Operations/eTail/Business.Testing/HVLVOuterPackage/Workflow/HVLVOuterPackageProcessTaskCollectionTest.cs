using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackageProcessTaskCollection))]
	public class HVLVOuterPackageProcessTaskCollectionTest : ProcessTaskCollectionTest<HVLVOuterPackageProcessTaskCollection>
	{
		#region Implementation

		protected override HVLVOuterPackageProcessTaskCollection GetCollectionToTestCore()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			return new HVLVOuterPackageProcessTaskCollection(outerPackage);
		}

		#endregion
	}
}
