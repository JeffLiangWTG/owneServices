using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOriginLoadListAdhocEdocsSupportCollection))]
	class HVLVOriginLoadListAdhocEdocsSupportCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVOriginLoadListAdhocEdocsSupportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVOriginLoadList>();
		}
	}
}
