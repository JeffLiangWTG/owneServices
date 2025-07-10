using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Common
{
	class OrderManagerExtensionsTest : TestCaseWithFactory
	{
		public void TestClonePlanningShipment()
		{
			var source = Factory.New<ForwardingShipment>();
			var target = Factory.New<ForwardingShipment>();
			source.JS_TransportMode = TransportModes.Sea;
			source.JS_PackingMode = ContainerModes.FCL;
			source.JS_ShipmentType = ShipmentTypes.BuyersConsolLead;
			source.JS_INCO = IncoTerms.ExWorks;
			source.JS_GoodsDescription = "test goods desc.";
			source.JS_E_DEP = new ZDateTime(2024, 10, 1);
			source.JS_E_ARV = new ZDateTime(2024, 10, 3);
			source.DetailedGoodsDescriptionNoteText = "test good desc note.";
			source.JS_MarksAndNumbers = "test mark & num";
			source.JS_RL_NKOrigin = "THBKK";
			source.JS_RL_NKDestination = "SGSIN";

			var consignorDocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var controllingCustomerAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var notifyPartyDocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var notifyParty2DocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var notifyParty3DocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var consigneeDocumentaryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var manufacturerDocAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var consignorPickupAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var consigneeDeliveryAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

			source.ConsignorDocumentaryAddress.E2_OA_Address = consignorDocumentaryAddress.PK;
			source.ControllingCustomerAddress.E2_OA_Address = controllingCustomerAddress.PK;
			source.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyPartyDocumentaryAddress.PK;
			source.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2DocumentaryAddress.PK;
			source.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3DocumentaryAddress.PK;
			source.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeDocumentaryAddress.PK;
			source.ManufacturerDocAddress.E2_OA_Address = manufacturerDocAddress.PK;
			source.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.PK;
			source.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.PK;

			var result = source.ClonePlanningShipment(target);
			CombineAssertions("should clone properties", () =>
			{
				AssertEquals(TransportModes.Sea, result.JS_TransportMode);
				AssertEquals(ContainerModes.FCL, result.JS_PackingMode);
				AssertEquals(ShipmentTypes.BuyersConsolLead, result.JS_ShipmentType);
				AssertEquals(IncoTerms.ExWorks, result.JS_INCO);
				AssertEquals("test goods desc.", result.JS_GoodsDescription);
				AssertEquals(new ZDateTime(2024, 10, 1), result.JS_E_DEP);
				AssertEquals(new ZDateTime(2024, 10, 3), result.JS_E_ARV);
				AssertEquals("test good desc note.", result.DetailedGoodsDescriptionNoteText);
				AssertEquals("test mark & num", result.JS_MarksAndNumbers);
				AssertEquals("THBKK", result.JS_RL_NKOrigin);
				AssertEquals("SGSIN", result.JS_RL_NKDestination);

				AssertEquals(consignorDocumentaryAddress.PK, result.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertEquals(controllingCustomerAddress.PK, result.ControllingCustomerAddress.E2_OA_Address);
				AssertEquals(notifyPartyDocumentaryAddress.PK, result.NotifyPartyDocumentaryAddress.E2_OA_Address);
				AssertEquals(notifyParty2DocumentaryAddress.PK, result.NotifyParty2DocumentaryAddress.E2_OA_Address);
				AssertEquals(notifyParty3DocumentaryAddress.PK, result.NotifyParty3DocumentaryAddress.E2_OA_Address);
				AssertEquals(consigneeDocumentaryAddress.PK, result.ConsigneeDocumentaryAddress.E2_OA_Address);
				AssertEquals(manufacturerDocAddress.PK, result.ManufacturerDocAddress.E2_OA_Address);
				AssertEquals(consignorPickupAddress.PK, result.ConsignorPickupAddress.E2_OA_Address);
				AssertEquals(consigneeDeliveryAddress.PK, result.ConsigneeDeliveryAddress.E2_OA_Address);
			});
		}

		public void TestMatchReferenceParameter()
		{
			var log1 = Factory.New<IQueuedLog>();
			var log2 = Factory.New<IQueuedLog>();
			(log1 as AutoStmJobQueue).SJ_Reference = "|NEW=SHP";
			(log2 as AutoStmJobQueue).SJ_Reference = "|NEW=XXX";

			AssertEquals(true, log1.MatchReferenceParameter("NEW", SupplierBookingStatus.Shipped));
			AssertEquals(false, log2.MatchReferenceParameter("NEW", SupplierBookingStatus.Shipped));
		}

		public void TestCopyDocAddressFrom_NotOverriden()
		{
			var toDocAddress = Factory.New<JobSupplierBooking>().NotifyPartyDocAddress;
			var fromDocAddress = Factory.New<JobSupplierBooking>().NotifyPartyDocAddress;

			CombineAssertions(() =>
			{
				AssertEquals(false, toDocAddress.E2_AddressOverride);
				AssertEquals(true, toDocAddress.IsEmpty);
				AssertEquals(true, toDocAddress.E2_OA_Address.IsEmpty);
			});

			toDocAddress.CopyDocAddressFrom(fromDocAddress);
			CombineAssertions(() =>
			{
				AssertEquals(false, toDocAddress.E2_AddressOverride);
				AssertEquals(true, toDocAddress.IsEmpty);
				AssertEquals(true, toDocAddress.E2_OA_Address.IsEmpty);
			});

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.AddNew();
			fromDocAddress.E2_OA_Address = org.MainAddress.PK;
			fromDocAddress.ContactPK = org.Contacts[0].PK;

			toDocAddress.CopyDocAddressFrom(fromDocAddress);
			CombineAssertions(() =>
			{
				AssertEquals(false, toDocAddress.E2_AddressOverride);
				AssertEquals(false, toDocAddress.IsEmpty);
				AssertEquals(org.MainAddress.PK, toDocAddress.E2_OA_Address);
				AssertEquals(org.Contacts[0].PK, toDocAddress.ContactPK);
			});
		}

		public void TestCopyDocAddressFrom_Overriden()
		{
			var toDocAddress = Factory.New<JobSupplierBooking>().NotifyPartyDocAddress;
			toDocAddress.E2_AddressOverride = true;
			var fromDocAddress = Factory.New<JobSupplierBooking>().NotifyPartyDocAddress;
			fromDocAddress.E2_AddressOverride = true;

			CombineAssertions(() =>
			{
				AssertEquals(true, toDocAddress.E2_AddressOverride);
				AssertEquals(true, toDocAddress.IsOverridenButEmpty);
			});

			toDocAddress.CopyDocAddressFrom(fromDocAddress);
			CombineAssertions(() =>
			{
				AssertEquals(true, toDocAddress.E2_AddressOverride);
				AssertEquals(true, toDocAddress.IsOverridenButEmpty);
			});

			fromDocAddress.E2_CompanyName = "company name test";
			fromDocAddress.E2_AdditionalAddressInformation = "additional address test";
			fromDocAddress.E2_Address1 = "test street no.";
			fromDocAddress.E2_Postcode = "123-456";
			fromDocAddress.E2_State = "ORG";
			fromDocAddress.E2_Contact = "test street";
			fromDocAddress.E2_Phone = "phone 456";
			fromDocAddress.E2_Fax = "fax 123";

			toDocAddress.CopyDocAddressFrom(fromDocAddress);
			CombineAssertions(() =>
			{
				AssertEquals(true, toDocAddress.E2_AddressOverride);
				AssertEquals(false, toDocAddress.IsOverridenButEmpty);
				AssertEquals("company name test", toDocAddress.E2_CompanyName);
				AssertEquals("additional address test", toDocAddress.E2_AdditionalAddressInformation);
				AssertEquals("test street no.", toDocAddress.E2_Address1);
				AssertEquals("123-456", toDocAddress.E2_Postcode);
				AssertEquals("ORG", toDocAddress.E2_State);
				AssertEquals("test street", toDocAddress.E2_Contact);
				AssertEquals("phone 456", toDocAddress.E2_Phone);
				AssertEquals("fax 123", toDocAddress.E2_Fax);
			});
		}

		public void TestGetConvertedPackingMode()
		{
			var booking = Factory.New<JobSupplierBooking>();
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerYard, TransportModes.Sea, ContainerModes.FCL);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerYard, TransportModes.Air, ContainerModes.ULD);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerYard, TransportModes.Road, ContainerModes.FTL);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerYard, TransportModes.Rail, ContainerModes.FCL);

			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerFreightStation, TransportModes.Sea, ContainerModes.FCL);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerFreightStation, TransportModes.Air, ContainerModes.ULD);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerFreightStation, TransportModes.Road, ContainerModes.FTL);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.ContainerFreightStation, TransportModes.Rail, ContainerModes.FCL);

			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.LooseCargo, TransportModes.Sea, ContainerModes.LCL);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.LooseCargo, TransportModes.Air, ContainerModes.Loose);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.LooseCargo, TransportModes.Road, ContainerModes.LTL);
			AssertConvertedPackingMode(booking, SupplierBookingLoadMode.LooseCargo, TransportModes.Rail, ContainerModes.LCL);
		}

		void AssertConvertedPackingMode(JobSupplierBooking booking, string loadMode, string transportMode, string packingMode)
		{
			booking.JSB_TransportMode = transportMode;
			booking.JSB_LoadMode = loadMode;
			AssertEquals(packingMode, booking.GetConvertedPackingMode());
		}

		public void TestUpdateMarksAndNumbers()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			var shipment = Factory.New<ForwardingShipment>();

			supplierBooking.JSB_MarksAndNumbers = "a b c";
			shipment.JS_MarksAndNumbers = "x y z";
			shipment.SetMarksAndNumbers(supplierBooking.JSB_MarksAndNumbers);
			AssertEquals("a b c", shipment.JS_MarksAndNumbers);
		}

		public void TestUpdateShipmentMeasuresFromPackLines()
		{
			var shipment = Factory.New<ForwardingShipment>();

			shipment.UpdateShipmentMeasuresFromPackLines();
			AssertEquals(Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals(Weight.Kilograms, shipment.JS_UnitOfWeight);
			AssertEquals(PkgUnit.Pallet, shipment.JS_F3_NKPackType);

			var packLine1 = shipment.OuterPackLines.AddNew();
			shipment.UpdateShipmentMeasuresFromPackLines();
			AssertEquals(Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals(Weight.Kilograms, shipment.JS_UnitOfWeight);
			AssertEquals(PkgUnit.Pallet, shipment.JS_F3_NKPackType);

			packLine1.JL_ActualVolumeUQ = Volume.CubicYards;
			packLine1.JL_ActualWeightUQ = Weight.Tonnes;
			packLine1.JL_F3_NKPackType = PkgUnit.Coil;
			shipment.UpdateShipmentMeasuresFromPackLines();
			AssertEquals(Volume.CubicYards, shipment.JS_UnitOfVolume);
			AssertEquals(Weight.Tonnes, shipment.JS_UnitOfWeight);
			AssertEquals(PkgUnit.Coil, shipment.JS_F3_NKPackType);

			var packLine2 = shipment.OuterPackLines.AddNew();
			shipment.UpdateShipmentMeasuresFromPackLines();
			AssertEquals(Volume.CubicYards, shipment.JS_UnitOfVolume);
			AssertEquals(Weight.Tonnes, shipment.JS_UnitOfWeight);
			AssertEquals(PkgUnit.Coil, shipment.JS_F3_NKPackType);

			packLine2.JL_ActualVolumeUQ = Volume.CubicFeet;
			packLine2.JL_ActualWeightUQ = Weight.LongTons;
			packLine2.JL_F3_NKPackType = PkgUnit.Case;
			shipment.UpdateShipmentMeasuresFromPackLines();
			AssertEquals(Volume.CubicMetres, shipment.JS_UnitOfVolume);
			AssertEquals(Weight.Kilograms, shipment.JS_UnitOfWeight);
			AssertEquals(PkgUnit.Package, shipment.JS_F3_NKPackType);

			packLine1.JL_ActualVolumeUQ = Volume.CubicFeet;
			packLine1.JL_ActualWeightUQ = Weight.LongTons;
			packLine1.JL_F3_NKPackType = PkgUnit.Case;
			shipment.UpdateShipmentMeasuresFromPackLines();
			AssertEquals(Volume.CubicFeet, shipment.JS_UnitOfVolume);
			AssertEquals(Weight.LongTons, shipment.JS_UnitOfWeight);
			AssertEquals(PkgUnit.Case, shipment.JS_F3_NKPackType);
		}

		public void TestUpdateOverflowingContainerWeightUnit()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var container = consol.Containers.AddNew();

			AssertEquals(Weight.Kilograms, container.JC_GrossWeightUQ);

			shipment.JS_UnitOfWeight = Weight.Kilotonnes;
			shipment.UpdateOverflowingContainerWeightMeasureUnit();
			AssertEquals(Weight.Kilograms, container.JC_GrossWeightUQ);
			AssertEquals(0m, container.JC_GrossWeight);

			container.JC_GrossWeight = 500_000.99;
			shipment.UpdateOverflowingContainerWeightMeasureUnit();
			AssertEquals(Weight.Kilograms, container.JC_GrossWeightUQ);
			AssertEquals(500_000.99m, container.JC_GrossWeight);

			container.JC_GrossWeight = 1_000_000.99;
			shipment.UpdateOverflowingContainerWeightMeasureUnit();
			AssertEquals(Weight.Tonnes, container.JC_GrossWeightUQ);
			AssertEquals(1_000.001m, container.JC_GrossWeight);

			container.JC_GrossWeightUQ = Weight.Kilograms;
			container.JC_GrossWeight = 1_000_000_000.99;
			shipment.UpdateOverflowingContainerWeightMeasureUnit();
			AssertEquals(Weight.Kilotonnes, container.JC_GrossWeightUQ);
			AssertEquals(1_000.000m, container.JC_GrossWeight);
		}

		public void TestCalculatePackLinePrice()
		{
			var sellRateCNY = Factory.New<RefExchangeRate>();
			sellRateCNY.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
			sellRateCNY.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRateCNY.RE_ExpiryDate = ZDateTime.Today.AddDays(10);
			sellRateCNY.RE_SellRate = 0.7m;
			sellRateCNY.RE_RX_NKExCurrency = "CNY";

			var sellRateUSD = Factory.New<RefExchangeRate>();
			sellRateUSD.RE_ExRateType = ExchangeRateTypes.Code.SellRate;
			sellRateUSD.RE_StartDate = ZDateTime.Today.AddDays(-5);
			sellRateUSD.RE_ExpiryDate = ZDateTime.Today.AddDays(10);
			sellRateUSD.RE_SellRate = 1m;
			sellRateUSD.RE_RX_NKExCurrency = "USD";

			ForwardingShipment shipment = null;
			AssertEquals(6m, shipment.CalculatePackLinePrice(null, 2m, 3m));

			shipment = Factory.New<ForwardingShipment>();
			AssertEquals(6m, shipment.CalculatePackLinePrice(null, 2m, 3m));

			shipment.JS_RX_NKGoodsValueCurr = "USD";
			AssertEquals(6m, shipment.CalculatePackLinePrice(null, 2m, 3m));

			var order = Factory.New<Order>();
			order.JD_RX_NKOrderCurrency = "CNY";
			AssertEquals(8.58m, shipment.CalculatePackLinePrice(order.OrderCurrency, 2m, 3m));
		}

		public void TestUpdateShipmentMeasuresFromBookingLines()
		{
			var order = Factory.New<Order>();
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_ItemPrice = 1;

			var supplierBooking = Factory.New<JobSupplierBooking>();
			var line1 = supplierBooking.SupplierBookingLines.AddNew();
			line1.JSL_JO_OrderLine = orderLine.PK;
			line1.JSL_VolumeUnit = Volume.CubicYards;
			line1.JSL_GrossWeightUnit = Weight.Tonnes;
			line1.JSL_F3_NKBookedPackagesUnit = PkgUnit.Coil;
			line1.JSL_BookedPackages = 2;
			line1.JSL_BookedQuantity = 3;
			line1.JSL_Volume = 4;
			line1.JSL_GrossWeight = 5;

			var line2 = supplierBooking.SupplierBookingLines.AddNew();
			line2.JSL_JO_OrderLine = orderLine.PK;
			line2.JSL_VolumeUnit = Volume.CubicFeet;
			line2.JSL_GrossWeightUnit = Weight.LongTons;
			line2.JSL_F3_NKBookedPackagesUnit = PkgUnit.Case;
			line2.JSL_BookedPackages = 6;
			line2.JSL_BookedQuantity = 7;
			line2.JSL_Volume = 8;
			line2.JSL_GrossWeight = 9;

			var shipment = Factory.New<ForwardingShipment>();

			shipment.UpdateShipmentMeasuresFromBookingLines(supplierBooking.SupplierBookingLines);

			CombineAssertions(() =>
			{
				AssertEquals(Volume.CubicMetres, shipment.JS_UnitOfVolume);
				AssertEquals(Weight.Kilograms, shipment.JS_UnitOfWeight);
				AssertEquals(PkgUnit.Package, shipment.JS_F3_NKPackType);

				AssertEquals(8, shipment.JS_OuterPacks);
				AssertEquals(14144.422m, shipment.JS_ActualWeight);
				AssertEquals(3.285m, shipment.JS_ActualVolume);
				AssertEquals(10m, shipment.JS_GoodsValue);
				AssertEquals(0, shipment.OuterPackLines.Count);
			});
		}

		public void TestUpdateShipmentFromContainerLoadListHeader()
		{
			var containerLoadListHeader = Factory.New<CommonContainerLoadList>();
			containerLoadListHeader.CLH_MarksAndNumbers = "Test marks and numbers";
			containerLoadListHeader.CLH_GoodsDescription = "Test goods description";
			containerLoadListHeader.CLH_DetailedGoodsDescription = "Test detailed goods description";

			var shipment = Factory.New<ForwardingShipment>();

			shipment.UpdateShipmentFromContainerLoadListHeader(containerLoadListHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Test marks and numbers", shipment.JS_MarksAndNumbers);
				AssertEquals("Test goods description", shipment.JS_GoodsDescription);
				AssertEquals("Test detailed goods description", shipment.DetailedGoodsDescriptionNoteText);
				AssertEquals(0, shipment.OuterPackLines.Count);
			});
		}

		public void TestUpdateShipmentFromSupplierBooking()
		{
			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_MarksAndNumbers = "Test marks and numbers";
			booking.JSB_GoodsDescription = "Test goods description";
			booking.JSB_DetailedGoodsDescription = "Test detailed goods description";

			var shipment = Factory.New<ForwardingShipment>();

			shipment.UpdateShipmentFromSupplierBooking(booking);

			CombineAssertions(() =>
			{
				AssertEquals("Test marks and numbers", shipment.JS_MarksAndNumbers);
				AssertEquals("Test goods description", shipment.JS_GoodsDescription);
				AssertEquals("Test detailed goods description", shipment.DetailedGoodsDescriptionNoteText);
				AssertEquals(0, shipment.OuterPackLines.Count);
			});
		}
	}
}

