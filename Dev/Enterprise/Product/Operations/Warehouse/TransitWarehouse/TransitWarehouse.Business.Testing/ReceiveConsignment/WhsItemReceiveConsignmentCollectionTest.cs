using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveConsignmentCollection))]
	public class WhsItemReceiveConsignmentCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemReceiveConsignmentCollection>
	{
		#region Implementation

		protected override WhsItemReceiveConsignmentCollection GetCollectionToTest()
		{
			return new WhsItemReceiveConsignmentCollection(Factory);
		}

		#endregion
	}
}
