using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnCargoPackLineDataObjectReaderForTransitWarehouseTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void TestImportingData_MissingAllDetails()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, null, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Containers, Master Bill Numbers and House Bill Number",
@"UXML received could not be used to match with any Cargo lines because of below errors. Correct them and try again.
Container details are missing.
Master Bill Number is not found.
House Bill Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingMasterAndHouseBillNumberDetails()
		{
			var container = new Container() { ContainerNumber = "CNT1" };
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, container, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Master Bill Numbers and House Bill Number",
@"UXML received could not be used to match with any Cargo lines because of below errors. Correct them and try again.
Master Bill Number is not found.
House Bill Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingContainerAndMasterBillNumberDetails()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.WayBillNumber = "HSB1";

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, null, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Containers and Master Bill Numbers.",
@"UXML received could not be used to match with any Cargo lines because of below errors. Correct them and try again.
Container details are missing.
Master Bill Number is not found.
", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingContainerAndHouseBillNumberDetails()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetAdditionalReferenceCollection(() =>
			new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.MasterBill }, ReferenceNumber = "MAB1" }
			});

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, null, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Containers and House Bill Number",
@"UXML received could not be used to match with any Cargo lines because of below errors. Correct them and try again.
Container details are missing.
House Bill Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_NotMatchingCargoLine()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.WayBillNumber = "HSB1";
			shipment.SetAdditionalReferenceCollection(() =>
			new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.MasterBill }, ReferenceNumber = "MAB1" }
			});

			var container = new Container() { ContainerNumber = "CNT1" };

			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, container, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Containers",
@"No matching Cargo Lines found for Master Bill: MAB1, House Bill: HSB1 and Container Number: CNT1.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MatchingMultipleCargoLines()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			CreateOuttrun("LCL", "CNT1", "MAB1", "HSB1", outturnHeader);
			CreateOuttrun("FCL", "CNT1", "MAB1", "HSB1", outturnHeader);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.WayBillNumber = "HSB1";

			shipment.SetAdditionalReferenceCollection(() =>
			new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.MasterBill }, ReferenceNumber = "MAB1" }
			});

			var container = new Container() { ContainerNumber = "CNT1" };

			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, container, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Containers, Master Bill Numbers and House Bill Number",
@"Multiple matching Cargo Lines found for Master Bill: MAB1, House Bill: HSB1 and Container Number: CNT1. Cargo Types were FCL, LCL.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MatchingCargoLine()
		{
			var now = ZDateTime.Now;
			var outturnHeader = Factory.New<CusOutturnHeader>();
			CreateOuttrun("LCL", "CNT1", "MAB1", "HSB1", outturnHeader);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.WayBillNumber = "HSB1";
			shipment.SetAdditionalReferenceCollection(() =>
			new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.MasterBill }, ReferenceNumber = "MAB1" }
			});
			shipment.SetPackingLineCollection(() =>
			new DataObjectList<PackingLine>()
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 1, OutturnedVolume = 2.5m, OutturnedWeight = 1.5m },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 2, OutturnedVolume = 1.5m, OutturnedWeight = 1m },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 0, OutturnedVolume = 0m, OutturnedWeight = 0m },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 3, OutturnedVolume = 1m, OutturnedWeight = 1m, OutturnDamagedQty = 1, OutturnPillagedQty = 1 }
			});

			var container = new Container() { ContainerNumber = "CNT1", LCLUnpack = now };

			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, outturnHeader, container, logger, Factory);
			var matchingOutturn = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				Assert(logger.Logs.Contains("Information - Matching Cargo Line found for Container Number: CNT1, Master Bill: MAB1 and House Bill: HSB1."));
				AssertEquals(6, matchingOutturn.C5_PackagesOutturned);
				AssertEquals(5m, matchingOutturn.C5_VolumeOutturned);
				AssertEquals(3.5m, matchingOutturn.C5_WeightOutturned);
				AssertEquals(true, matchingOutturn.C5_DamageIndicator);
				AssertEquals(true, matchingOutturn.C5_PillageIndicator);
				AssertEquals(now, matchingOutturn.C5_CargoReceiptDate);
				AssertEquals(now, matchingOutturn.C5_CargoUnpackDate);
			});
		}

		public void TestImportingData_NotMatchingCargoLine_IncorrectHeader()
		{
			var matchingOutturnHeader = Factory.New<CusOutturnHeader>();
			var nonMatchingOutturnHeader = Factory.New<CusOutturnHeader>();
			CreateOuttrun("LCL", "CNT1", "MAB1", "HSB1", matchingOutturnHeader);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.WayBillNumber = "HSB1";
			shipment.SetAdditionalReferenceCollection(() =>
			new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.MasterBill }, ReferenceNumber = "MAB1" }
			});
			shipment.SetPackingLineCollection(() =>
			new DataObjectList<PackingLine>()
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 1, OutturnedVolume = 2.5m, OutturnedWeight = 1.5m },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 2, OutturnedVolume = 1.5m, OutturnedWeight = 1m },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 0, OutturnedVolume = 0m, OutturnedWeight = 0m },
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { OutturnQty = 3, OutturnedVolume = 1m, OutturnedWeight = 1m, OutturnDamagedQty = 1, OutturnPillagedQty = 1 }
			});

			var container = new Container() { ContainerNumber = "CNT1" };

			var reader = new CusOutturnCargoPackLineDataObjectReaderForTransitWarehouse(shipment, nonMatchingOutturnHeader, container, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Containers, Master Bill Numbers and House Bill Number",
@"No matching Cargo Lines found for Master Bill: MAB1, House Bill: HSB1 and Container Number: CNT1.", () => reader.ReadIntoBusinessObject());
		}

		CusOutturn CreateOuttrun(ZString cargoType, ZString containerNumber, ZString masterBill, ZString houseBill, CusOutturnHeader outturnHeader)
		{
			var existOutturn = Factory.BOFactory.New<CusOutturn>();
			existOutturn.C5_C6 = outturnHeader.PK;
			existOutturn.C5_CargoType = cargoType;
			existOutturn.C5_ContainerNumber = containerNumber;
			existOutturn.C5_MasterBill = masterBill;
			existOutturn.C5_HouseBill = houseBill;
			return existOutturn;
		}
	}
}
