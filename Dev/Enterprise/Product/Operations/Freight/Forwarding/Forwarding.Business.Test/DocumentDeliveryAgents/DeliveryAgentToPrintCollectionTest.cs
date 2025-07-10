using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business
{
	sealed class DeliveryAgentToPrintCollectionTest : TestCaseWithFactory
	{
		public void TestCollectionDoesNotAllowNew()
		{
			DeliveryAgentToPrintCollection collection = new DeliveryAgentToPrintCollection(Factory);
			collection.AddNew();
			AssertNotNull("Delivery Agent To PrintCollection not null", collection);
			AssertEquals("Delivery Agent To PrintCollection has one member", 1, collection.Count);
			AssertEquals("DeliveryAgentToPrintCollection memebers is of type DeliveryAgentOrgHeader", typeof(DeliveryAgentOrgHeader), collection[0].GetType());
		}
	}
}
