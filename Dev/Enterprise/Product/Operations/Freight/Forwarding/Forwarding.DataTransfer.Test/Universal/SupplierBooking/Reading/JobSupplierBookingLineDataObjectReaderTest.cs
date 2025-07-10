using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalDate = Enterprise.UniversalDataBuss.DataObjects.Universal.Date;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class JobSupplierBookingLineDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappingsForNoExisted()
		{
			var oldSupplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();

			var universalShipment = BuildUniversalShipment();
			universalShipment.SubShipmentCollection[0].PackingLineCollection[0].PackingLineID = ZString.Empty;

			var supplierBooking = new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertNotNull(supplierBooking);
			AssertNotEquals(oldSupplierBooking.PK, supplierBooking.PK);
			AssertEquals(1, supplierBooking.SupplierBookingLines.Count);

			var supplierBookingLine = supplierBooking.SupplierBookingLines[0];
			AssertEquals(8.2m, supplierBookingLine.JSL_BookedQuantity);
			AssertEquals(2m, supplierBookingLine.JSL_BookedPackages);
			AssertEquals("Desc", supplierBookingLine.JSL_Description);
			AssertEquals("BAG", supplierBookingLine.JSL_F3_NKBookedPackagesUnit);
			AssertEquals(3m, supplierBookingLine.JSL_GrossWeight);
			AssertEquals("KT", supplierBookingLine.JSL_GrossWeightUnit);
			AssertEquals(5m, supplierBookingLine.JSL_Volume);
			AssertEquals("CF", supplierBookingLine.JSL_VolumeUnit);
			AssertEquals("PLT", supplierBookingLine.OrderLine.JO_F3_NKPackType);
			AssertEquals("GEN", supplierBookingLine.JSL_RH_NKCommodityCode);
			AssertEquals("HC001", supplierBookingLine.JSL_HarmonisedCode);
			AssertEquals(new ZDate(2023, 9, 1), supplierBookingLine.JSL_ShipmentWindowStart);
			AssertEquals(new ZDate(2023, 9, 3), supplierBookingLine.JSL_ShipmentWindowEnd);
			Assert(!supplierBookingLine.JSL_BookingLineId.IsEmpty);

			var manufacturerAddress = supplierBookingLine.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
			AssertNotNull(manufacturerAddress);
			AssertEquals("MFAKL", manufacturerAddress.Organisation.OH_Code);
		}

		public void TestBasicFieldMappingsForExisted()
		{
			var oldSupplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();

			var universalShipment = BuildUniversalShipment();
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");

			var supplierBooking = new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();

			AssertNotNull(supplierBooking);
			AssertEquals(oldSupplierBooking.PK, supplierBooking.PK);
			AssertEquals("SBK001", supplierBooking.JSB_BookingId);
			AssertEquals(1, supplierBooking.SupplierBookingLines.Count);

			var supplierBookingLine = supplierBooking.SupplierBookingLines[0];
			AssertEquals(8.2m, supplierBookingLine.JSL_BookedQuantity);
			AssertEquals(2m, supplierBookingLine.JSL_BookedPackages);
			AssertEquals("Desc", supplierBookingLine.JSL_Description);
			AssertEquals("BAG", supplierBookingLine.JSL_F3_NKBookedPackagesUnit);
			AssertEquals(3m, supplierBookingLine.JSL_GrossWeight);
			AssertEquals("KT", supplierBookingLine.JSL_GrossWeightUnit);
			AssertEquals(5m, supplierBookingLine.JSL_Volume);
			AssertEquals("CF", supplierBookingLine.JSL_VolumeUnit);
			AssertEquals("Line Marks&Nos", supplierBookingLine.JSL_MarksAndNumbers);
			AssertEquals("PLT", supplierBookingLine.OrderLine.JO_F3_NKPackType);
			AssertEquals("GEN", supplierBookingLine.JSL_RH_NKCommodityCode);
			AssertEquals("HC001", supplierBookingLine.JSL_HarmonisedCode);
			AssertEquals(new ZDate(2023, 9, 1), supplierBookingLine.JSL_ShipmentWindowStart);
			AssertEquals(new ZDate(2023, 9, 3), supplierBookingLine.JSL_ShipmentWindowEnd);

			var manufacturerAddress = supplierBookingLine.DocAddresses.FindByDocAddressType(DocAddressType.Manufacturer);
			AssertNotNull(manufacturerAddress);
			AssertEquals("MFAKL", manufacturerAddress.Organisation.OH_Code);
		}

		public void TestDifferentNKPackType()
		{
			var oldSupplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();

			var universalShipment = BuildUniversalShipment();
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			universalShipment.SubShipmentCollection[0].TotalNoOfPacksPackageType = new PackageType() { Code = "KEG", Description = "Keg" };

			Logger.ClearLogs();
			var supplierBooking = new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();
			var supplierBookingLine = supplierBooking.SupplierBookingLines[0];
			AssertNotNull(supplierBooking);
			AssertEquals(oldSupplierBooking.PK, supplierBooking.PK);
			AssertEquals("PLT", supplierBookingLine.OrderLine.JO_F3_NKPackType);
			AssertContains("Unit of Qty of ORD0001~1~1~2 cannot be updated when adding or editing a supplier booking line.", Logger.GetWarnings());
		}

		public void TestCheck()
		{
			JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();

			Logger.ClearLogs();
			var universalShipment = BuildUniversalShipment();
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			universalShipment.SubShipmentCollection[0].Order = null;
			new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("There is no related order line", Logger.GetErrors());

			Logger.ClearLogs();
			universalShipment = BuildUniversalShipment();
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			universalShipment.SubShipmentCollection[0].PackingLineCollection.Clear();
			new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("There is no packline", Logger.GetErrors());

			Logger.ClearLogs();
			universalShipment = BuildUniversalShipment();
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			universalShipment.SubShipmentCollection[0].Order.OrderNumber = "XXX";
			new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("There is no matched order line", Logger.GetErrors());

			Logger.ClearLogs();
			universalShipment = BuildUniversalShipment();
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			universalShipment.SubShipmentCollection[0].PackingLineCollection[0].PackingLineID = "XXX";
			new JobSupplierBookingDataObjectReader(universalShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Supplier Booking Line XXX is not matched", Logger.GetErrors());
		}

		static UniversalShipment BuildUniversalShipment()
		{
			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.PortOfDischarge = new UniversalUNLOCO { Code = "SGSIN" };
			universalShipment.PortOfLoading = new UniversalUNLOCO { Code = "AUSYD" };
			universalShipment.ShipmentIncoTerm = new IncoTerm { Code = "EXW" };
			universalShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "PLC" };
			universalShipment.TransportMode = new UniversalCodeDescriptionPair { Code = "SEA" };
			universalShipment.LoadMode = new LoadMode { Code = "CY" };

			universalShipment.SetDateCollection(() => new List<UniversalDate>());
			universalShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.BookedOnDate, Value = new ZDateTime(2022, 4, 3) });
			universalShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.CargoAvailableDate, Value = new ZDateTime(2022, 4, 6) });

			universalShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			universalShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.BookingPartyDocumentaryAddress), "BK", "SGSIN"));
			universalShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Supplier), "SP", "SGSIN"));
			universalShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ControllingCustomer), "CC", "CNSZX"));

			universalShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			universalShipment.SubShipmentCollection.Add(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));

			var universalSubShipment = universalShipment.SubShipmentCollection[0];
			universalSubShipment.TotalNoOfPacksDecimal = 8.2;
			universalSubShipment.TotalNoOfPacksPackageType = new PackageType() { Code = "PLT", Description = "Pallet" };
			universalSubShipment.Order = new Order(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0001", OrderNumberSplit = 1 };
			universalSubShipment.Order.SetOrderLineCollection(() => new DataObjectList<OrderLine>());
			universalSubShipment.Order.OrderLineCollection.Add(new OrderLine { LineNumber = 1, SubLineNumber = 2 });
			universalSubShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			universalSubShipment.PackingLineCollection.Add(new PackingLine
			{
				PackQty = 2,
				GoodsDescription = "Desc",
				MarksAndNos = "Line Marks&Nos",
				PackType = new PackageType { Code = "BAG" },
				Weight = 3,
				WeightUnit = new UnitOfWeight { Code = "KT" },
				Volume = 5,
				VolumeUnit = new UnitOfVolume { Code = "CF" },
				PackingLineID = "JSL001",
				Commodity = new Commodity { Code = "GEN" },
				HarmonisedCode = "HC001"
			});
			universalSubShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			universalSubShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Manufacturer), "MF", "NKAKL"));
			universalSubShipment.SetDateCollection(() => new List<UniversalDate>
			{
				new UniversalDate { Type = UniversalDateType.ShipmentWindowStart, Value = new ZDateTime(2023,9,1) },
				new UniversalDate { Type = UniversalDateType.ShipmentWindowEnd, Value = new ZDateTime(2023,9,3) }
			});
			return universalShipment;
		}
	}
}
