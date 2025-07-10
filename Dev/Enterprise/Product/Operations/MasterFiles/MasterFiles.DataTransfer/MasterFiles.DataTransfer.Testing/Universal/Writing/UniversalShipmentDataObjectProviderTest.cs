using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class UniversalShipmentDataObjectProviderTest : TestCaseWithFactory
	{
		[SnailTest]
		public void TestGetDataObject()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			Factory.Save();

			var provider = new UniversalShipmentDataObjectProvider();
			var dataObject = provider.GetDataObject(shipmentBO, null);

			AssertNotNull("DataObject has been created", dataObject);
		}
	}
}