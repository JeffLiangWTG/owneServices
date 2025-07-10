using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentProcessTaskCollection))]
	class DtbConsignmentProcessTaskCollectionTest : ProcessTaskCollectionTest<DtbConsignmentProcessTaskCollection>
	{
		protected override DtbConsignmentProcessTaskCollection GetCollectionToTestCore()
		{
			return new DtbConsignmentProcessTaskCollection(Helper.CreateConsignment());
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;
	}
}
