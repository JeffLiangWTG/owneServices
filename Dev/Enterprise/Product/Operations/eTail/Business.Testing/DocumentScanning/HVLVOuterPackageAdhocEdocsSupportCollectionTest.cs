using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackageAdhocEdocsSupportCollection))]
	class HVLVOuterPackageAdhocEdocsSupportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVOuterPackageAdhocEdocsSupportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVOuterPackage>();
		}
	}
}
