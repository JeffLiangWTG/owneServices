using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.DataTransfer.Testing
{
	public class SysMergeWarehouseXmlValueObjectSerializerTest : WhsTestCaseWithFactoryEnv
	{
		#region TestCreateOrUpdateFromValueObject

		public void TestCreateOrUpdateFromValueObject()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var existingOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var serializer = new SysMergeWarehouseXmlValueObjectSerializerForTesting(dataAdapter);

			var warehouseWithNewAddress = Helper.CreateWarehouse("WH1", shouldPreGenerateDDL: false); // to avoid exposed bug that will be fixed in WI00151368
			warehouseWithNewAddress.WW_GB_RelatedCompanyBranch = branch.PK;
			var warehouseWithExistingAddress = Helper.CreateWarehouse("WH2", shouldPreGenerateDDL: false); // to avoid exposed bug that will be fixed in WI00151368
			warehouseWithExistingAddress.WW_GB_RelatedCompanyBranch = branch.PK;
			warehouseWithExistingAddress.WW_OA_WarehouseAddress = existingOrg.MainAddress.PK;

			var xsdWarehouseWithExistingAddress = dataAdapter.ExportToValueObject(warehouseWithExistingAddress, new ValueObjectExportContext(new NotificationBuffer()));
			var xsdWarehouseWithNewAddress = dataAdapter.ExportToValueObject(warehouseWithNewAddress, new ValueObjectExportContext(new NotificationBuffer()));

			var otherFactory = new BusinessObjectFactory();
			var importContextInOtherFactory = new ValueObjectImportContext(otherFactory, new NotificationBuffer());

			var importSuccessMessage = string.Format("Warehouse: [({0}) - {1} - {2}] imported successfully\r\n", warehouseWithExistingAddress.PK, warehouseWithExistingAddress.WW_WarehouseCode, warehouseWithExistingAddress.WW_WarehouseName);
			var importErrorMessage = string.Format("Error: Warehouse: [({0}) - {1} - {2}]\r\nCould not find Address with PK = ({3}).\r\nPlease import it first and then retry the import operation.\r\n",
				warehouseWithNewAddress.PK, warehouseWithNewAddress.WW_WarehouseCode, warehouseWithNewAddress.WW_WarehouseName, warehouseWithNewAddress.WW_OA_WarehouseAddress);

			AssertNotNull("If warehouse address exists in the factory it should be imported successfully.", serializer.CreateOrUpdateFromValueObject_Exposed(xsdWarehouseWithExistingAddress, importContextInOtherFactory));
			AssertEquals(importSuccessMessage, importContextInOtherFactory.LastNotificationMessage);
			AssertNotEquals(importErrorMessage, importContextInOtherFactory.LastNotificationMessage);

			AssertNull("If warehouse address doesn't exist in the factory it should not be imported.", serializer.CreateOrUpdateFromValueObject_Exposed(xsdWarehouseWithNewAddress, importContextInOtherFactory));
			AssertNotEquals(importSuccessMessage, importContextInOtherFactory.LastNotificationMessage);
			AssertEquals(importErrorMessage, importContextInOtherFactory.LastNotificationMessage);
		}

		#endregion

		#region TestCreateOrUpdateFromValueObject

		public void TestDefaultInboundDockDoor_Valid()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var whs = Helper.CreateWarehouse("WHS Test");
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_OA_WarehouseAddress = org.MainAddress.PK;

			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			DefaultDockDoor_TestCore(whs, expectError: false, dockDoorType: "inbound");
		}

		public void TestDefaultInboundDockDoor_Invalid()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var whs = Helper.CreateWarehouse("WHS Test");
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_OA_WarehouseAddress = org.MainAddress.PK;

			Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);
			whs.WW_DefaultInboundDockDoor = whs.DefaultLocation.PK;
			Assert("Precondition", whs.WW_DefaultInboundDockDoor.IsValid);
			DefaultDockDoor_TestCore(whs, expectError: true, dockDoorType: "inbound");
		}

		public void TestDefaultInboundDockDoor_Empty()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var whs = Helper.CreateWarehouse("WHS Test", shouldPreGenerateDDL: false);
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_OA_WarehouseAddress = org.MainAddress.PK;

			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			whs.WW_DefaultInboundDockDoor = ZGuid.Empty;
			DefaultDockDoor_TestCore(whs, expectError: false, dockDoorType: "inbound");
		}

		public void TestDefaultOutboundDockDoor_Valid()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var whs = Helper.CreateWarehouse("WHS Test");
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_OA_WarehouseAddress = org.MainAddress.PK;

			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			DefaultDockDoor_TestCore(whs, expectError: false, dockDoorType: "outbound");
		}

		public void TestDefaultOutboundDockDoor_Invalid()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var whs = Helper.CreateWarehouse("WHS Test");
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_OA_WarehouseAddress = org.MainAddress.PK;

			Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);
			whs.WW_DefaultOutboundDockDoor = whs.DefaultLocation.PK;
			Assert("Precondition", whs.WW_DefaultOutboundDockDoor.IsValid);
			DefaultDockDoor_TestCore(whs, expectError: true, dockDoorType: "outbound");
		}

		public void TestDefaultOutboundDockDoor_Empty()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var whs = Helper.CreateWarehouse("WHS Test", shouldPreGenerateDDL: false);
			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			whs.WW_OA_WarehouseAddress = org.MainAddress.PK;

			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			whs.WW_DefaultOutboundDockDoor = ZGuid.Empty;
			DefaultDockDoor_TestCore(whs, expectError: false, dockDoorType: "outbound");
		}

		void DefaultDockDoor_TestCore(WhsWarehouse whs, bool expectError, string dockDoorType)
		{
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var serializer = new SysMergeWarehouseXmlValueObjectSerializerForTesting(dataAdapter);
			var xsdWarehouse = dataAdapter.ExportToValueObject(whs, new ValueObjectExportContext(new NotificationBuffer()));

			var otherFactory = new BusinessObjectFactory();
			var importContextInOtherFactory = new ValueObjectImportContext(otherFactory, new NotificationBuffer());

			var importSuccessMessage = $"Warehouse: [({whs.PK}) - {whs.WW_WarehouseCode} - {whs.WW_WarehouseName}] imported successfully\r\n";
			var importErrorMessage = $"Error: Warehouse: [({whs.PK}) - {whs.WW_WarehouseCode} - {whs.WW_WarehouseName}]\r\nCould not find default {dockDoorType} dock door Location.\r\nPlease fix it first and then retry the import operation.\r\n";

			if (expectError)
			{
				AssertNull("If default outbound dock is not valid, it should not import warehouse", serializer.CreateOrUpdateFromValueObject_Exposed(xsdWarehouse, importContextInOtherFactory));
				AssertEquals(importErrorMessage, importContextInOtherFactory.LastNotificationMessage);
			}
			else
			{
				AssertNotNull("If default outbound dock is valid or empty, it should be imported successfully.", serializer.CreateOrUpdateFromValueObject_Exposed(xsdWarehouse, importContextInOtherFactory));
				AssertEquals(importSuccessMessage, importContextInOtherFactory.LastNotificationMessage);
			}
		}

		#endregion
	}

	#region SysMergeWarehouseXmlValueObjectSerializerForTesting class

	public class SysMergeWarehouseXmlValueObjectSerializerForTesting : SysMergeWarehouseXmlValueObjectSerializer
	{
		public SysMergeWarehouseXmlValueObjectSerializerForTesting(IValueObjectDataAdapter adapter)
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
