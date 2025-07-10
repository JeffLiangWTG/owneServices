using CargoWise.EntityFramework;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class SysMergeWarehouseInventoryXmlValueObjectSerializerTest : WhsTestCaseWithFactory
	{
		public void TestCreateOrUpdateFromValueObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save(); // so we don't need to import it into another factory.

			var existingClient = data.Org1;
			var newClient = Helper.CreateClient("CLIENT2"); // this client is not in DB.
			Helper.CreateProductClientRelationShip(newClient, data.Part2);

			var dataAdapter = new SysMergeWarehouseInventoryValueObjectDataAdapter();
			var serializer = new SysMergeWarehouseInventoryXmlValueObjectSerializerForTesting(dataAdapter);

			var receiveWithExistingClient = Helper.CreateWhsReceiveWithInventory(existingClient, data.Whs1, "R1", data.Part1, 10m);
			var receiveWithNewClient = Helper.CreateWhsReceiveWithInventory(newClient, data.Whs1, "R2", data.Part2, 10m);
			receiveWithExistingClient.WD_DocketID = "W00000123";
			receiveWithNewClient.WD_DocketID = "W00000456";

			var xsdReceiveWithExistingClient = dataAdapter.ExportToValueObject(receiveWithExistingClient, new ValueObjectExportContext(new NotificationBuffer()));
			var xsdReceiveWithNewClient = dataAdapter.ExportToValueObject(receiveWithNewClient, new ValueObjectExportContext(new NotificationBuffer()));

			var otherFactory = new BusinessObjectFactory();
			var importContextInOtherFactory = new ValueObjectImportContext(otherFactory, new NotificationBuffer());

			var importSuccessMessage = string.Format("Receive [({0}) - System Merge {1}] imported successfully\r\n", receiveWithExistingClient.PK, receiveWithExistingClient.WD_DocketID);
			var importErrorMessage = string.Format("Error: Receive [({0}) - {1}]\r\nCould not find Organization with PK = ({2}).\r\nPlease import it first and then retry the import operation.\r\n",
				receiveWithNewClient.PK, receiveWithNewClient.WD_ExternalReference, newClient.PK);

			AssertNotNull("If warehouse client exists in the factory, Receive should be imported successfully.", serializer.CreateOrUpdateFromValueObject_Exposed(xsdReceiveWithExistingClient, importContextInOtherFactory));
			AssertEquals(importSuccessMessage, importContextInOtherFactory.LastNotificationMessage);
			AssertNotEquals(importErrorMessage, importContextInOtherFactory.LastNotificationMessage);

			AssertNull("If warehouse client doesn't exist in the factory, Refeive should NOT be imported.", serializer.CreateOrUpdateFromValueObject_Exposed(xsdReceiveWithNewClient, importContextInOtherFactory));
			AssertNotEquals(importSuccessMessage, importContextInOtherFactory.LastNotificationMessage);
			AssertEquals(importErrorMessage, importContextInOtherFactory.LastNotificationMessage);
		}
	}

	#region SysMergeWarehouseInventoryXmlValueObjectSerializerForTesting class

	public class SysMergeWarehouseInventoryXmlValueObjectSerializerForTesting : SysMergeWarehouseInventoryXmlValueObjectSerializer
	{
		public SysMergeWarehouseInventoryXmlValueObjectSerializerForTesting(IValueObjectDataAdapter adapter)
			: base(adapter.ValueObjectType)
		{
			Adapter = adapter;
		}

		readonly IValueObjectDataAdapter Adapter;

		public BusinessObject CreateOrUpdateFromValueObject_Exposed(IValueObject valueObject, IValueObjectImportContext context)
		{
			return base.CreateOrUpdateFromValueObject(Adapter, null, valueObject, context);
		}
	}

	#endregion
}