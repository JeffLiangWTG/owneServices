using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentActionPackageDivotCollection))]
	class DtbConsignmentActionPackageDivotCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentActionPackageDivotCollection>
	{
		protected override DtbConsignmentActionPackageDivotCollection GetCollectionToTest()
		{
			return new DtbConsignmentActionPackageDivotCollection(Helper.CreateConsignmentAction());
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;
	}
}
