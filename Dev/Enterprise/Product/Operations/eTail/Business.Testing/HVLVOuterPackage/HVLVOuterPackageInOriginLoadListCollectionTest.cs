using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackagesInOriginLoadListCollection))]
	public class HVLVOuterPackageInOriginLoadListCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVOuterPackagesInOriginLoadListCollection(Factory.NewWithValidTestData<HVLVOriginLoadList>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVOuterPackage>();
		}

		#endregion
	}
}
