using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	[TestedType(typeof(CommonCartageOrgCollection))]
	sealed class CommonCartageOrgCollectionTest : ActiveBusinessObjectCollectionTestCase<CommonCartageOrgCollection>
	{
		protected override CommonCartageOrgCollection GetCollectionToTest()
		{
			return new CommonCartageOrgCollection(Factory.New<CommonCartageType>());
		}
	}
}
