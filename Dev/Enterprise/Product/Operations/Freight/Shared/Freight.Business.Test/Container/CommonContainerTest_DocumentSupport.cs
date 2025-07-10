namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerTest_DocumentSupport : BaseFreightTest
	{
		public void TestLinkedShipment()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			container.LinkedShipment = shipment;
			AssertEquals("Linked CommonShipment is ", "S0001010", container.LinkedShipment.JS_UniqueConsignRef);
		}
	}
}
