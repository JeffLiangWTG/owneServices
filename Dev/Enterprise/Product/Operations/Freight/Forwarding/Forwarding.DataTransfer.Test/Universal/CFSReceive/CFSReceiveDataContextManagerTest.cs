using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(CFSReceiveDataContextManager))]
	class CFSReceiveDataContextManagerTest : ShipmentDataContextManagerTestCase<CFSReceiveDataContextManager, CFSReceive>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => messageText;

		JobSupplierBooking booking;
		JobSupplierBookingLine bookingLine1, bookingLine2;
		Receive[] expectReceives1, expectReceives2, expectReceives3;

		readonly string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>10</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			prepareTestData();
			Factory.SaveForTesting();
		}

		void prepareTestData()
		{
			var order = Factory.NewWithValidTestData<Order>();
			booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = "SB00000001";

			var orderLine = order.OrderLines.AddNew();
			bookingLine1 = booking.SupplierBookingLines.AddNew();
			bookingLine2 = booking.SupplierBookingLines.AddNew();
			CFSReceiveTestHelper.SetBookingCfsAddress(Factory, booking);
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation;
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Approved;

			var receiptDate1 = new ZDateTime(2023, 10, 20);
			var receiptDate2 = new ZDateTime(2023, 10, 30);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 5, 0, 0, 0, 0, receiptDate1, receiptDate2);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 10);
			bookingLine1.JSL_JO_OrderLine = orderLine.PK;
			bookingLine2.JSL_JO_OrderLine = orderLine.PK;

			var expectDate = new ZDateTime(2023, 10, 9, 13, 0, 0);
			expectReceives1 = new Receive[] {
				new Receive
			{
				ReceivedQuantity = 5m,
				ReceivedPacks = 10,
				ReceivedWeight = 10m,
				ReceivedVolume = 10m,
				FirstReceiptTime = expectDate,
				LastReceiptTime = receiptDate2,
			},
				new Receive
			{
				ReceivedQuantity = 10m,
				ReceivedPacks = 10,
				ReceivedWeight = 10m,
				ReceivedVolume = 10m,
				FirstReceiptTime = expectDate,
				LastReceiptTime = expectDate,
			},
			};
			expectReceives2 = new Receive[] {
				new Receive
			{
				ReceivedQuantity = 0m,
				ReceivedPacks = 0,
				ReceivedWeight = 0m,
				ReceivedVolume = 0m
			},
				new Receive
			{
				ReceivedQuantity = 0m,
				ReceivedPacks = 0,
				ReceivedWeight = 0m,
				ReceivedVolume = 0m
			},
			};
			expectReceives3 = new Receive[] {
				new Receive
			{
				ReceivedQuantity = 8m,
				ReceivedPacks = 7,
				ReceivedWeight = 6m,
				ReceivedVolume = 6m,
				FirstReceiptTime = expectDate,
				LastReceiptTime = expectDate,
			},
				new Receive
			{
				ReceivedQuantity = 7m,
				ReceivedPacks = 8,
				ReceivedWeight = 9m,
				ReceivedVolume = 9m,
				FirstReceiptTime = expectDate,
				LastReceiptTime = expectDate,
			},
			};
		}

		void assertResults(string messageText, string log, ZString status, Receive[] values, bool receiptDatesUpdated = false)
		{
			var message = GetQueuedUniversalShipmentMessage(messageText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_Status", status, message.EM_Status);
				var text = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Import Log", log, message.GetLogNoteText());

				var query = new ZQuery();
				query.OrderBy = JobSupplierBookingLineSchema.Constants.JSL_BookingLineId;
				var factory = new BusinessObjectFactory();
				var bookingLines = factory.Load<JobSupplierBookingLine>(query);
				AssertEquals(values.Length, bookingLines.Length);
				for (var i = 0; i < bookingLines.Length; i++)
				{
					AssertEquals(values[i].ReceivedQuantity, bookingLines[i].JSL_ReceivedQuantity);
					AssertEquals((int)values[i].ReceivedPacks, bookingLines[i].JSL_ReceivedPackages);
					AssertEquals(values[i].ReceivedWeight, bookingLines[i].JSL_ReceivedWeight);
					AssertEquals(values[i].ReceivedVolume, bookingLines[i].JSL_ReceivedVolume);
					if (receiptDatesUpdated)
					{
						AssertEquals(values[i].FirstReceiptTime, bookingLines[i].JSL_FirstReceiptDateUtc);
						AssertEquals(values[i].LastReceiptTime, bookingLines[i].JSL_LastReceiptDateUtc);
					}
				}
			});
		}

		public void TestImportXML_FieldsUpdated_MultipleLines()
		{
			prepareTestData();
			Factory.SaveForTesting();

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Updated Supplier Booking SB00000001 from UniversalShipment.
Successfully saved Supplier Booking SB00000001 with 2 x JobSupplierBookingLine.
", EDIMessageStatusList.Codes.ProcessedOK, expectReceives1, true);
		}

		public void TestImportXML_FieldsUpdated_MultipleLinesWithAdjustment()
		{
			prepareTestData();
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 10, 10, 10, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 10, 10, 10, 10, 10);
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>-2</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>-3</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>-4</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>-4</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>-3</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>-2</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>-1</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>-1</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Warning - Booked Quantity of 10.00000 is not fully received.
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Warning - Booked Quantity of 10.00000 is not fully received.
Updated Supplier Booking SB00000001 from UniversalShipment.
Successfully saved Supplier Booking SB00000001 with 2 x JobSupplierBookingLine.
", EDIMessageStatusList.Codes.Warning, expectReceives3, true);
		}

		public void TestImportXML_FieldsUpdated_OneLine()
		{
			prepareTestData();
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Updated Supplier Booking SB00000001 from UniversalShipment.
Successfully saved Supplier Booking SB00000001 with 1 x JobSupplierBookingLine.
", EDIMessageStatusList.Codes.ProcessedOK, new Receive[] { expectReceives1[0], expectReceives2[0] });
		}

		public void TestImportXML_LoadModeNotMatch()
		{
			prepareTestData();
			booking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;

			Factory.SaveForTesting();

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Error - Supplier Booking must have load mode = CFS.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives2);
		}

		public void TestImportXML_StatusNotMatch()
		{
			prepareTestData();
			booking.JSB_Status = Core.Constants.SupplierBookingStatus.Placed;

			Factory.SaveForTesting();

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Error - CFS receipt can only be performed on Approved Supplier Booking.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives2);
		}

		public void TestImportXML_EmptyCFSAddress()
		{
			prepareTestData();
			booking.JSB_OA_CFSAddress = Guid.Empty;

			Factory.SaveForTesting();

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Error - Supplier Booking must have CFS Address.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives2);
		}

		public void TestImportXML_LineNotFound()
		{
			prepareTestData();
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000003</PackingLineID>
        <InnerQty>10</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Error - Supplier Booking Line JSL00000000000000003 cannot be found in the system.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives2);
		}

		public void TestImportXML_QuantityAndPackDifferentSign()
		{
			prepareTestData();
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 10, 5, 10, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 10, 10, 10, 10, 10);
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>-5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>3</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>10</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Error - Both Received Quantity and Received Packs should either be positive or negative value.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives1);
		}

		public void TestImportXML_CalculatedFieldsContainNegative()
		{
			prepareTestData();
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine1, "JSL00000000000000001", 10, 5, 10, 10, 10);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 10, 10, 10, 10, 10);
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>-11</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>-3</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>-4</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>-4</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>-3</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>-2</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>-1</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>-1</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Error - Total Received Qty, Total Received Package, Total Received Volume, or Total Received Weight would calculate to a negative value. Please adjust Received Qty, Received Package, Received Volume or Received Weight.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives1);
		}

		public void TestImportXML_TotalReceivedQuantityGreaterThanBookedQuantity()
		{
			prepareTestData();
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>11</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Error - Total Received Quantity cannot be greater than the Booked Quantity.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives2);
		}

		public void TestImportXML_TotalReceivedQuantityLessThanBookedQuantity()
		{
			prepareTestData();
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-10-10T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>3</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			expectReceives1[1].ReceivedQuantity = 3;
			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Warning - Booked Quantity of 10.00000 is not fully received.
Updated Supplier Booking SB00000001 from UniversalShipment.
Successfully saved Supplier Booking SB00000001 with 2 x JobSupplierBookingLine.
", EDIMessageStatusList.Codes.Warning, expectReceives1);
		}

		public void TestImportXML_InvalidReceiptDate()
		{
			prepareTestData();
			var receiptDate = new ZDateTime(2023, 10, 01);
			CFSReceiveTestHelper.SetBookingLineValue(bookingLine2, "JSL00000000000000002", 10, 0, 0, 0, 0, receiptDate, receiptDate);
			Factory.SaveForTesting();

			const string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CFSReceive</Type>
          <Key>SB00000001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <PackingLineCollection>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate></LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000001</PackingLineID>
        <InnerQty>5</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <Commodity>
          <Code></Code>
        </Commodity>
        <FirstCFSReceiptDate></FirstCFSReceiptDate>
        <LastCFSReceiptDate>2023-09-25T00:00:00</LastCFSReceiptDate>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>JSL00000000000000002</PackingLineID>
        <InnerQty>10</InnerQty>
        <InnerPackType>
          <Code>PCE</Code>
          <Description>Piece</Description>
        </InnerPackType>
        <PackQty>10</PackQty>
        <PackType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </PackType>
        <Volume>10</Volume>
        <VolumeUnit>
          <Code></Code>
        </VolumeUnit>
        <Weight>10</Weight>
        <WeightUnit>
          <Code>G</Code>
          <Description>Grams</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

			assertResults(messageText, @"
Successfully loaded matching CFSReceive.
Populating CFSReceive...
Successfully loaded matching JobSupplierBookingLine.
Populating JobSupplierBookingLine...
Error - Last CFS Receipt Date cannot be empty.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.
", EDIMessageStatusList.Codes.Discarded, expectReceives2);
		}
	}

	class Receive
	{
		public decimal? ReceivedQuantity { get; set; }
		public long? ReceivedPacks { get; set; }
		public decimal? ReceivedWeight { get; set; }
		public decimal? ReceivedVolume { get; set; }
		public ZDateTime? FirstReceiptTime { get; set; }
		public ZDateTime? LastReceiptTime { get; set; }
	}
}
