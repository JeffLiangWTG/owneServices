using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class WhsOrderFlattenedDataTransferProcessorTest : WhsDocketFlattenedDataTransferProcessorTest<WhsOrder>
	{
		#region TestImport_SameOrgButMismatchedAddress

		public void TestImport_SameOrgButMismatchedAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "LOLA";
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "DELIVERY ADDRESS";

			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseCode = "ZUB";

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.WD_TransportReference = "ALEE";
			flatRecord.Client_OH_Code = "LOLA";
			flatRecord.Warehouse_WW_WarehouseCode = "ZUB";

			flatRecord.SupplierAddress_E2_Address1 = "PICKUP ADDRESS";
			flatRecord.ConsigneeAddress_OH_Code = "LOLA";
			flatRecord.ConsigneeAddress_E2_Address1 = "DELIVERY ADDRESS_X";

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));

			processor.Import();

			var order = Factory.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.WD_TransportReference, "ALEE"));

			AssertEquals(false, order.PickUpDocAddress.E2_AddressOverride);
			AssertEquals("PICKUP ADDRESS", order.SupplierDocAddress.E2_Address1);

			AssertEquals(false, order.DropOffDocAddress.E2_AddressOverride);
			AssertEquals("DELIVERY ADDRESS_X", order.ConsigneeDocAddress.E2_Address1);
		}

		#endregion

		#region Implementation

		protected override WhsDocketCollection GetDocketCollection(BusinessObjectFactory factory, AdhocCollectionRelationship relationship)
		{
			return new WhsOrderCollection(Factory, relationship);
		}

		protected override WhsDocketFlattenedDataTransferProcessor<WhsOrder> GetProcessor(ImportCollectionInfoImplForWhsDocketFlattened info, WhsDocketCollection docketCollection)
		{
			return new WhsOrderFlattenedDataTransferProcessor(info, docketCollection);
		}

		#endregion
	}
}
