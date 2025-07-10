using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchConsignmentCollection))]
	public class WhsItemDispatchConsignmentCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemDispatchConsignmentCollection>
	{
		#region Implementation

		protected override WhsItemDispatchConsignmentCollection GetCollectionToTest()
		{
			return new WhsItemDispatchConsignmentCollection(Factory);
		}

		#endregion
	}
}
