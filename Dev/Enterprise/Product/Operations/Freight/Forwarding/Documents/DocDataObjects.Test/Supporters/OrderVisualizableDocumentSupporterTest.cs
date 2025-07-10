using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class OrderVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestIsRegisteredWithObjectFactory()
		{
			var order = Factory.New<Order>();
			var supporter = order.GetSupporter();
			Assert("supporter is registered in ObjectFactory", supporter is OrderVisualizableDocumentSupporter);
		}
	}
}
