using System.IO;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test;

[TestedType(typeof(CYDPickupDataContextManager))]
class CYDPickupDataContextManagerTest : DataContextManagerTestCase<CYDPickupDataContextManager, CYDPickup>
{
	public void TestExportBookingConfirmationEventWithDataContextKey()
	{
		var pickup = Factory.NewWithValidTestData<CYDPickup>();
		pickup.YPL_PickupID = "YPL000000000001";

		var logger = new DummyLogger();
		var dataContext = DataContextFactory.New();
		dataContext.AddDataSource(DataContextType.GateMovementBooking, "GBM00000001");
		var linkCreator = new UniversalJobLinkCreator(pickup.Factory, pickup, null, dataContext, logger, true);
		linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);

		Factory.SaveForTesting();

		var log = pickup.Logs.AddNew(AutoEvents.BookingConfirmed);
		using (log.LockForUpdatingKeyFieldsForTesting())
		{
			log.SL_EventTime = ZDateTime.BrettsBirthday;
		}

		const string expectedXml = """
<?xml version="1.0" encoding="utf-8"?>
<UniversalEvent xmlns="http://www.cargowise.com/Schemas/Universal/2011/11" version="1.1">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>CYDPickup</Type>
          <Key>YPL000000000001</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
      <DataTargetCollection>
        <DataTarget>
          <Type>GateMovementBooking</Type>
          <Key>GBM00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>1971-09-18T00:00:00.000+10:00</EventTime>
    <EventType>BKC</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
    </ContextCollection>
  </Event>
</UniversalEvent>
""";

		var manager = (IEventDataContextManager)pickup.GetUniversalDataContextManager();
		var writer = manager.GetEventDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.GDM, pickup)));
		var universalEvent = writer.GetDataObject(log);

		using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
		{
			using var stream = (SubStreamableStream)new MemoryStream();
			var xmlWriter = ObjectFactory.Get<IXmlWriter>();
			xmlWriter.WriteXML(universalEvent, stream);

			using var reader = new StreamReader(stream);
			var result = reader.ReadToEnd();
			AssertMultilineASCIIEquals("Expected Universal Shipment Message", expectedXml, result);
		}
	}

	protected override void TestBusinessObjectImplementsIJobNumberCore()
	{
		Assert("CYDYardUnitState does not implement IJobNumber", true);
	}
}
