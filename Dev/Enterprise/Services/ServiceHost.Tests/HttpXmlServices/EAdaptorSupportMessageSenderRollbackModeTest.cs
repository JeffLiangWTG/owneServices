using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class EAdaptorSupportMessageSenderRollbackModeTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSendInRollbackMode()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001010";
			shipment.JS_TransportMode = "SEA";
			factory.Save();

			const string universalXmlRequest =
@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S0001010</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			var sender = new EAdaptorSupportMessageSender();
			var res = sender.SendInRollbackMode(universalXmlRequest);
			var allCountSql = "SELECT COUNT(*) FROM dbo.EDIMessage;";
			AssertEquals(0, Db.Connection.ExecuteScalar(allCountSql));
			AssertContains("Expected to generate UniversalShipment for ForwardingShipment S0001010", "UniversalResponse", res);
			AssertContains("Expected to generate Processing Time in response", "Processing Time", res);
		}
	}
}
