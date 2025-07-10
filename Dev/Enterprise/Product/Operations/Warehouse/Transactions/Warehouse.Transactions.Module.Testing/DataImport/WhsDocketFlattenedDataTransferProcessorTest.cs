using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class WhsDocketFlattenedDataTransferProcessorTest<T> : TestCaseWithFactory
		where T : WhsDocket
	{
		public void TestImport()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "LOLA";
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "DELIVERY ADDRESS";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PRT1";
			var partRelation = part.RelatedOrganisations.AddNew();
			partRelation.OU_OH = org.PK;
			partRelation.OU_Relationship = "OWN";

			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseCode = "ZUB";

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.WD_TransportReference = "ALEE";
			flatRecord.WD_ExternalReference = "WD1";
			flatRecord.Warehouse_WW_WarehouseCode = "ZUB";

			flatRecord.PickupAddress_E2_Address1 = "PICKUP ADDRESS";
			flatRecord.DropOffAddress_OH_Code = "LOLA";
			flatRecord.DropOffAddress_E2_Address1 = "DELIVERY ADDRESS";

			flatRecord.Forwarder_OH_Code = "MILO";
			flatRecord.Client_OH_Code = "LOLA";
			flatRecord.ConsigneeAddress_OH_Code = "LOLA";
			flatRecord.ConsigneeAddress_E2_Address1 = "DELIVERY ADDRESS";

			// DateTimeOffSet Records
			flatRecord.WD_BookingDate = ZDateTimeOffset.Today;
			flatRecord.WD_ETA = ZDateTimeOffset.Today;
			flatRecord.WD_ETD = ZDateTimeOffset.Today;
			flatRecord.WD_ArrivalDate = ZDateTimeOffset.Today;
			flatRecord.WD_RequiredDate = ZDateTimeOffset.Today;
			flatRecord.Line_WE_RequiredByDate = ZDateTimeOffset.Today;

			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = "PRT1";

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\n", processor.Log);

			var docket = Factory.LoadTop1<T>(new ZQuery(WhsDocketSchema.WD_TransportReference, "ALEE"));
			AssertEquals(true, docket.IsInDatabase);

			AssertEquals(whs.PK, docket.WD_WW_Whs);

			AssertEquals(true, docket.PickUpDocAddress.E2_AddressOverride);
			AssertEquals("PICKUP ADDRESS", docket.PickUpDocAddress.E2_Address1);

			AssertEquals(false, docket.DropOffDocAddress.E2_AddressOverride);
			AssertEquals(address.PK, docket.DropOffDocAddress.E2_OA_Address);

			AssertEquals(ZGuid.Empty, docket.WD_OH_Forwarder);
			AssertEquals(org.PK, docket.WD_OH_Client);

			AssertEquals(1, docket.Lines.Count);
			AssertEquals(43m, docket.Lines[0].WE_TransactionQuantity);
			AssertEquals(part.PK, docket.Lines[0].WE_OP);
		}

		public void TestImport_MultipleLinesSameHeader()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CNR";
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "PICKUP ADDRESS";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "CNE";
			var address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = "DELIVERY ADDRESS";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PRT1";
			var partRelation = part.RelatedOrganisations.AddNew();
			partRelation.OU_OH = org.PK;
			partRelation.OU_Relationship = "OWN";

			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseCode = "ZUB";

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.WD_TransportReference = "ALEE";
			flatRecord.Client_OH_Code = "CNR";
			flatRecord.Warehouse_WW_WarehouseCode = "ZUB";

			flatRecord.SupplierAddress_E2_Address1 = "PICKUP ADDRESS";
			flatRecord.ConsigneeAddress_OH_Code = "CNE";
			flatRecord.ConsigneeAddress_E2_Address1 = "DELIVERY ADDRESS";

			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = "PRT1";

			var flatRecord2 = collection.AddNew();
			flatRecord2.WD_TransportReference = "ALEE";
			flatRecord2.Client_OH_Code = "CNR";
			flatRecord2.Line_WE_TransactionQuantity = 16m;
			flatRecord2.Warehouse_WW_WarehouseCode = "ZUB";
			flatRecord2.LinePart_OP_PartNum = "PRT1";

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));

			processor.Import();
			AssertEquals("Creating Job [Ref: ALEE]\r\nUpdating Job [Ref: ALEE]\r\n", processor.Log);

			var docket = Factory.LoadTop1<T>(new ZQuery(WhsDocketSchema.WD_TransportReference, "ALEE"));
			AssertEquals(2, docket.Lines.Count);
			AssertEquals(43m, docket.Lines[0].WE_TransactionQuantity);
			AssertEquals(part.PK, docket.Lines[0].WE_OP);
			AssertEquals(16m, docket.Lines[1].WE_TransactionQuantity);
			AssertEquals(part.PK, docket.Lines[1].WE_OP);
		}

		public void TestImport_DuplicateProductMatched()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "LOLA";
			var address = client.Addresses.AddNew();
			address.OA_Address1 = "CLIENT ADDRESS";

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PRT1";
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PRT1";

			var partRelation1 = part1.RelatedOrganisations.AddNew();
			partRelation1.OU_OH = client.PK;
			partRelation1.OU_Relationship = "OWN";
			var partRelation2 = part2.RelatedOrganisations.AddNew();
			partRelation2.OU_OH = client.PK;
			partRelation2.OU_Relationship = "OWN";

			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseCode = "ZUB";

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.Client_OH_Code = "LOLA";
			flatRecord.WD_TransportReference = "ALEE";
			flatRecord.WD_ExternalReference = "WD1";
			flatRecord.Warehouse_WW_WarehouseCode = "ZUB";
			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = "PRT1";

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));
			processor.Import();
			AssertEquals("A duplicate product was matched. Nothing was saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, processor.IsCanceled);
		}

		public void TestImport_EnsureInactiveProductNotMatched_SinglePart()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "LOLA";
			var address = client.Addresses.AddNew();
			address.OA_Address1 = "CLIENT ADDRESS";

			var inactivePart = Factory.New<OrgSupplierPart>();
			inactivePart.OP_PartNum = "PRT1";
			inactivePart.OP_IsActive = false;

			var partInactiveRelation = inactivePart.RelatedOrganisations.AddNew();
			partInactiveRelation.OU_OH = client.PK;
			partInactiveRelation.OU_Relationship = "OWN";

			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseCode = "ZUB";

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.Client_OH_Code = "LOLA";
			flatRecord.WD_TransportReference = "ALEE";
			flatRecord.WD_ExternalReference = "WD1";
			flatRecord.Warehouse_WW_WarehouseCode = "ZUB";
			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = "PRT1";

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));
			processor.Import();
			var docket = Factory.LoadTop1<T>(new ZQuery(WhsDocketSchema.WD_TransportReference, "ALEE"));
			AssertEquals(ZGuid.Empty, docket.Lines.Single().WE_OP);
		}

		public void TestImport_EnsureInactiveProductNotMatched_MultipleParts()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "LOLA";
			var address = client.Addresses.AddNew();
			address.OA_Address1 = "CLIENT ADDRESS";

			var inactivePart = Factory.New<OrgSupplierPart>();
			inactivePart.OP_PartNum = "PRT1";
			inactivePart.OP_IsActive = false;
			var activePart = Factory.New<OrgSupplierPart>();
			activePart.OP_PartNum = "PRT1";

			var partInactiveRelation = inactivePart.RelatedOrganisations.AddNew();
			partInactiveRelation.OU_OH = client.PK;
			partInactiveRelation.OU_Relationship = "OWN";
			var partActiveRelation = activePart.RelatedOrganisations.AddNew();
			partActiveRelation.OU_OH = client.PK;
			partActiveRelation.OU_Relationship = "OWN";

			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseCode = "ZUB";

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.WD_TransportReference = "ALEE";
			flatRecord.WD_ExternalReference = "WD1";
			flatRecord.Warehouse_WW_WarehouseCode = "ZUB";
			flatRecord.Client_OH_Code = "LOLA";
			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = "PRT1";

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));
			processor.Import();
			var docket = Factory.LoadTop1<T>(new ZQuery(WhsDocketSchema.WD_TransportReference, "ALEE"));
			AssertEquals(activePart.PK, docket.Lines.Single().WE_OP);
		}

		public void TestImport_ProductWithExpiryDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.WD_ExternalReference = "WD1";
			flatRecord.Warehouse_WW_WarehouseCode = data.Whs1.WW_WarehouseCode;
			flatRecord.WD_BookingDate = ZDateTimeOffset.Today;
			flatRecord.Client_OH_Code = data.Org1.OH_Code;
			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = data.Part1.OP_PartNum;
			var expiryDate = ZDate.Today;
			flatRecord.Line_WE_ExpiryDate = expiryDate;

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));

			AssertNoExceptionThrown(processor.Import);
			var docket = Factory.LoadTop1<T>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "WD1"));
			AssertEquals(true, docket.IsInDatabase);

			var docketLine = docket.Lines.Single();
			AssertEquals(true, docketLine.IsInDatabase);
			AssertEquals(expiryDate, docketLine.WE_ExpiryDate);
		}

		public void TestImport_ProductWithPackingDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);

			var collection = new WhsDocketFlattenedCollection();
			var flatRecord = collection.AddNew();
			flatRecord.WD_ExternalReference = "WD1";
			flatRecord.Warehouse_WW_WarehouseCode = data.Whs1.WW_WarehouseCode;
			flatRecord.WD_BookingDate = ZDateTimeOffset.Today;
			flatRecord.Client_OH_Code = data.Org1.OH_Code;
			flatRecord.Line_WE_TransactionQuantity = 43m;
			flatRecord.LinePart_OP_PartNum = data.Part1.OP_PartNum;
			var packingDate = ZDate.Today;
			flatRecord.Line_WE_PackingDate = packingDate;

			var info = new ImportCollectionInfoImplForWhsDocketFlattened(collection);
			var processor = GetProcessor(info, GetDocketCollection(Factory));

			AssertNoExceptionThrown(processor.Import);
			var docket = Factory.LoadTop1<T>(new ZQuery(WhsDocketSchema.WD_ExternalReference, "WD1"));
			AssertEquals(true, docket.IsInDatabase);

			var docketLine = docket.Lines.Single();
			AssertEquals(true, docketLine.IsInDatabase);
			AssertEquals(packingDate, docketLine.WE_PackingDate);
		}

		#region Implementation

		protected abstract WhsDocketFlattenedDataTransferProcessor<T> GetProcessor(ImportCollectionInfoImplForWhsDocketFlattened info, WhsDocketCollection docketCollection);

		protected WhsDocketCollection GetDocketCollection(BusinessObjectFactory factory) => GetDocketCollection(factory, new AdhocCollectionRelationship(typeof(T)));

		protected abstract WhsDocketCollection GetDocketCollection(BusinessObjectFactory factory, AdhocCollectionRelationship relationship);

		#endregion
	}
}
