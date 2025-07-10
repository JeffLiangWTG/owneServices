using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentVariationCollection))]
	sealed class DtbConsignmentVariationCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentVariationCollection>
	{
		protected override DtbConsignmentVariationCollection GetCollectionToTest()
		{
			return new DtbConsignmentVariationCollection(Helper.CreateConsignment());
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;
	}
}
