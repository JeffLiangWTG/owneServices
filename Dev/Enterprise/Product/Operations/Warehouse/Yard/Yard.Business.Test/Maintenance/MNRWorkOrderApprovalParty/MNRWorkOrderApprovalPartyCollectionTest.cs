using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderApprovalPartyCollection))]
	public class MNRWorkOrderApprovalPartyCollectionTest : ActiveBusinessObjectCollectionTestCase<MNRWorkOrderApprovalPartyCollection>
	{
		#region Implementation

		protected override MNRWorkOrderApprovalPartyCollection GetCollectionToTest()
		{
			return new MNRWorkOrderApprovalPartyCollection(Factory);
		}

		#endregion
	}
}
