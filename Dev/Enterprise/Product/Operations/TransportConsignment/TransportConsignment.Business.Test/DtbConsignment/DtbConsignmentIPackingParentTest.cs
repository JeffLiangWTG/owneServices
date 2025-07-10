using Enterprise.Packing.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignment))]
	class DtbConsignmentIPackingParentTest : PackingParentTestCase<DtbConsignment>
	{
		protected override DtbConsignment GetNewParent()
		{
			return Helper.CreateConsignment();
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;
	}
}
