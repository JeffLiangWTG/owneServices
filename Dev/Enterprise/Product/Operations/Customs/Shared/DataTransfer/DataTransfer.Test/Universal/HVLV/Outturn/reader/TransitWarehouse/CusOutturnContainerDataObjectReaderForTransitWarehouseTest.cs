using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnContainerDataObjectReaderForTransitWarehouseTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void TestImportingData_ContainerCollectionIsNull()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var outturnHeader = Factory.New<CusOutturnHeader>();
			AssertExceptionThrown<ArgumentNullException>("Shipment doesn't have Containers", () => new CusOutturnContainerDataObjectReaderForTransitWarehouse(shipment, outturnHeader, logger, Factory));
		}

		public void TestImportingData_NoContainerNumber()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			CreateOutturn("FCL", "CNT1", "MAB1", "", outturnHeader);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var container = new Container() { };

			AssertExceptionThrown<ArgumentNullException>("Shipment doesn't have Container Number", () => new CusOutturnContainerDataObjectReaderForTransitWarehouse(shipment, outturnHeader, logger, Factory));
		}

		public void TestImportingData_MatchingMultipleCargoLines()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			CreateOutturn("FCL", "CNT1", "MAB1", "", outturnHeader);
			CreateOutturn("FCL", "CNT1", "MAB2", "", outturnHeader);

			var container = new Container() { ContainerNumber = "CNT1" };
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { container }));

			var reader = new CusOutturnContainerDataObjectReaderForTransitWarehouse(shipment, outturnHeader, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment has multiple matching FCL cargo packlines",
@"Multiple matching FCL Cargo Lines found for Container Number: CNT1.", () => reader.ReadIntoBusinessObject());
		}

		[TestDate(2021, 10, 2)]
		public void TestImportingData_MatchingCargoLine()
		{
			var now = ZDateTime.Now;
			var outturnHeader = Factory.New<CusOutturnHeader>();
			var outturnWithoutReceiptDate = CreateOutturn("FCL", "CNT1", "MAB1", "", outturnHeader);
			var outturnWithReceiptDate = CreateOutturn("FCL", "CNT2", "MAB2", "", outturnHeader, now.AddDays(-3));

			var shipmentForCNT1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentForCNT1.TotalNoOfPiecesLanded = 2;
			shipmentForCNT1.SetDateCollection(() => new List<Date>(new Date[]
			{
				Date.New(DateType.Unpack, ZBool.False, now),
				Date.New(DateType.Received, ZBool.False, now.AddDays(-1))
			}));

			var containerForCNT1 = new Container() { ContainerNumber = "CNT1", IsSealOk = true };
			containerForCNT1.LCLUnpack = now;
			shipmentForCNT1.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerForCNT1 }));

			var readerForCNT1 = new CusOutturnContainerDataObjectReaderForTransitWarehouse(shipmentForCNT1, outturnHeader, logger, Factory);
			var matchingOutturnWithoutReceiptDate = readerForCNT1.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				Assert(logger.Logs.Contains("Information - Matching FCL Cargo Line found for Container Number: CNT1."));
				AssertEquals(2, matchingOutturnWithoutReceiptDate.C5_PackagesOutturned);
				AssertEquals(true, matchingOutturnWithoutReceiptDate.C5_SealIntactIndicator);
				AssertEquals(now, matchingOutturnWithoutReceiptDate.C5_CargoReceiptDate);
				AssertEquals(ZDateTime.Empty, matchingOutturnWithoutReceiptDate.C5_CargoUnpackDate);
			});

			var shipmentForCNT2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var containerForCNT2 = new Container() { ContainerNumber = "CNT2", IsSealOk = true };
			containerForCNT2.LCLUnpack = now.AddDays(1);
			shipmentForCNT2.SetContainerCollection(() => new DataObjectList<Container>(new[] { containerForCNT2 }));

			var readerForCNT2 = new CusOutturnContainerDataObjectReaderForTransitWarehouse(shipmentForCNT2, outturnHeader, logger, Factory);
			var matchingOutturnWithReceiptDate = readerForCNT2.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals(now.AddDays(-3), matchingOutturnWithReceiptDate.C5_CargoReceiptDate);
				AssertEquals(ZDateTime.Empty, matchingOutturnWithReceiptDate.C5_CargoUnpackDate);
			});
		}

		public void TestImportingData_NotMatchingCargoLine_IncorrectHeader()
		{
			var matchingOutturnHeader = Factory.New<CusOutturnHeader>();
			var nonMatchingOutturnHeader = Factory.New<CusOutturnHeader>();
			CreateOutturn("FCL", "CNT1", "MAB1", "", matchingOutturnHeader);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var container = new Container() { ContainerNumber = "CNT1" };
			shipment.SetContainerCollection(() => new DataObjectList<Container>(new[] { container }));

			var reader = new CusOutturnContainerDataObjectReaderForTransitWarehouse(shipment, nonMatchingOutturnHeader, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Cargo Header doesn't have Matching Container",
@"No matching FCL Cargo Line found for Container Number: CNT1.", () => reader.ReadIntoBusinessObject());
		}
	}
}
