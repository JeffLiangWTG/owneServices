using System;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderConverterTest : TestCaseWithFactory
	{
		public void TestMapping()
		{
			Xsd.Orders ordersValue = new Xsd.Orders();

			string orderResourcePath = "Enterprise.Freight.Forwarding.DataTransfer.Test.Orders.TestFiles.Order.csv";

			using (Stream stream = GetType().Assembly.GetManifestResourceStream(orderResourcePath))
			using (StreamReader reader = new StreamReader(stream))
			{
				Converter.ImportFlatFile(ordersValue, new CsvFlatFileFormat(), reader);
			}

			AssertEquals("There should be 2 order values in the collection", 2, ordersValue.Order.Count);

			Xsd.Order orderValue = ordersValue.Order[0];
			AssertNotNull(orderValue);
			AssertEquals(2, orderValue.OrderLines.Count);

			AssertEquals("123456", orderValue.OrderIdentifier.OrderNumber);
			AssertEquals("111111", orderValue.OrderDetail.InvoiceNumber);
			AssertEquals(new ZDateTime(2005, 1, 1), orderValue.OrderDetail.InvoiceDate);
			AssertEquals(Xsd.OrderTransportMode.SEA, orderValue.OrderDetail.TransportMode);
			AssertEquals(Xsd.OrderContainerMode.FCL, orderValue.OrderDetail.ContainerMode);
			AssertEquals(new ZDateTime(2006, 3, 1), orderValue.OrderDetail.OrderDateTime);
			AssertEquals(new ZDateTime(2006, 5, 1), orderValue.OrderDetail.ExWorksRequiredBy);
			AssertEquals(new ZDateTime(2006, 4, 1), orderValue.OrderDetail.DeliveryRequiredBy);
			AssertEquals("INC", orderValue.OrderDetail.OrderStatus);
			AssertEquals(365m, orderValue.OrderDetail.OrderTotal.Value);
			AssertEquals("USD", orderValue.OrderDetail.OrderTotal.CurrencyCode);
			AssertEquals(2.33m, orderValue.OrderDetail.ExchangeRate);
			AssertEquals("INCO", orderValue.OrderDetail.Incoterm);
			AssertEquals("addterms", orderValue.OrderDetail.AdditionalTerms);
			AssertEquals("goodsdesc", orderValue.OrderDetail.Description);
			AssertEquals("1123334", orderValue.OrderDetail.ConfirmNumber);
			AssertEquals(new ZDateTime(2006, 4, 10), orderValue.OrderDetail.ConfirmDate);
			AssertEquals("UA", orderValue.OrderDetail.CountryOfOrigin);

			AssertEquals(3m, orderValue.OrderDetail.ShipmentPlanning.Packs.Value);
			AssertEquals("BLH", orderValue.OrderDetail.ShipmentPlanning.Packs.DimensionType);
			AssertEquals(325m, orderValue.OrderDetail.ShipmentPlanning.Volume.Value);
			AssertEquals("M3", orderValue.OrderDetail.ShipmentPlanning.Volume.DimensionType);
			AssertEquals(32m, orderValue.OrderDetail.ShipmentPlanning.Weight.Value);
			AssertEquals("KG", orderValue.OrderDetail.ShipmentPlanning.Weight.DimensionType);
			AssertEquals("HouseBill", orderValue.OrderDetail.ShipmentPlanning.HouseBill);
			AssertEquals("depvessel", orderValue.OrderDetail.ShipmentPlanning.DepartureVessel);
			AssertEquals("depvoyage", orderValue.OrderDetail.ShipmentPlanning.DepartureVoyageFlight);
			AssertEquals("UAIEV", orderValue.OrderDetail.ShipmentPlanning.GoodsOrigin.Value);
			AssertEquals("AUSYD", orderValue.OrderDetail.ShipmentPlanning.GoodsDestination.Value);
			AssertEquals("avaliableat", orderValue.OrderDetail.ShipmentPlanning.GoodsAvailAt);
			AssertEquals("deliveredto", orderValue.OrderDetail.ShipmentPlanning.GoodsDelivTo);
			AssertEquals("AUBNE", orderValue.OrderDetail.ShipmentPlanning.LoadPort.Value);
			AssertEquals("AUMEL", orderValue.OrderDetail.ShipmentPlanning.DischargePort.Value);
			AssertEquals("SENDAG", orderValue.OrderDetail.ShipmentPlanning.SendingAgent.OwnerCode);
			AssertEquals("RECAG", orderValue.OrderDetail.ShipmentPlanning.ReceivingAgent.OwnerCode);

			AssertEquals("SYDBLAH", orderValue.OrderDetail.Supplier.OwnerCode);
			AssertEquals("Sydney Blah", orderValue.OrderDetail.Supplier.OrganisationDetails.Name);
			AssertEquals("blah str. Sydney", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("Sydney", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("Blah", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("04213", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(Xsd.UNLOCO.FromPortCode(Factory, "AUSYD").Value, orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals("mail@mail.com", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].Email);
			AssertEquals("380973181193", orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses[0].TelephoneNumbers[0].Value);

			AssertEquals("SYDTMP", orderValue.OrderDetail.Buyer.OwnerCode);
			AssertEquals("Sydney Temp", orderValue.OrderDetail.Buyer.OrganisationDetails.Name);
			AssertEquals("Temp str. Sydney", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].AddressLine1);
			AssertEquals("", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].AddressLine2);
			AssertEquals("Sydney", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].CityOrSuburb);
			AssertEquals("TEMP", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].StateOrProvince);
			AssertEquals("09999", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].PostCode);
			AssertEquals(Xsd.UNLOCO.FromPortCode(Factory, "AUSYD").Value, orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].Location.Value);
			AssertEquals("temp@temp.com", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].Email);
			AssertEquals("380503345663", orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses[0].TelephoneNumbers[0].Value);
			AssertEquals(1, orderValue.Notes.Count);
			AssertEquals(Xsd.NotesNoteNoteType.SpecialInstructions, orderValue.Notes[0].NoteType);
			AssertMultilineASCIIEquals("Special Instructions", "GOOOOOOOODs\nadd notes\nspecial instractions\ndelivery instractions", orderValue.Notes[0].NoteData);

			AssertEquals(new ZDateTime(2005, 1, 1), orderValue.OrderDetail.Custom.Date1);
			AssertEquals(new ZDateTime(2005, 1, 2), orderValue.OrderDetail.Custom.Date2);
			AssertEquals("custattr1", orderValue.OrderDetail.Custom.Text1);
			AssertEquals("custattr2", orderValue.OrderDetail.Custom.Text2);
			AssertEquals("custattr3", orderValue.OrderDetail.Custom.Text3);
			AssertEquals("custattr4", orderValue.OrderDetail.Custom.Text4);
			AssertEquals("custattr5", orderValue.OrderDetail.Custom.Text5);
			AssertEquals(false, orderValue.OrderDetail.Custom.Flag1);
			AssertEquals(true, orderValue.OrderDetail.Custom.Flag2);
			AssertEquals(false, orderValue.OrderDetail.Custom.Flag3);
			AssertEquals(true, orderValue.OrderDetail.Custom.Flag4);
			AssertEquals(false, orderValue.OrderDetail.Custom.Flag5);
			AssertEquals(1.01m, orderValue.OrderDetail.Custom.Decimal1);
			AssertEquals(1.02m, orderValue.OrderDetail.Custom.Decimal2);
			AssertEquals(1.03m, orderValue.OrderDetail.Custom.Decimal3);
			AssertEquals(1.04m, orderValue.OrderDetail.Custom.Decimal4);
			AssertEquals(1.05m, orderValue.OrderDetail.Custom.Decimal5);
			AssertEquals("contact1", orderValue.OrderDetail.Custom.Contact1);
			AssertEquals("contact2", orderValue.OrderDetail.Custom.Contact2);

			Xsd.OrderOrderLine orderLine = orderValue.OrderLines[0];

			AssertEquals(1001.ToString(), orderLine.OrderLineNo.ToString());
			AssertEquals(1111.ToString(), orderLine.OrderSubLineNo.ToString());
			AssertEquals("SuppPrdCode", orderLine.OrderLineDetail.Product);
			AssertEquals("SuppPrdDesc", orderLine.OrderLineDetail.Description);
			AssertEquals("INC", orderLine.OrderLineDetail.LineStatus);
			AssertEquals(ZDateTime.Empty, orderLine.OrderLineDetail.DropDate);
			AssertEquals(10.12m, orderLine.OrderLineDetail.QtyOrdered.Value);
			AssertEquals("UQ", orderLine.OrderLineDetail.QtyOrdered.DimensionType);
			AssertEquals(10.1m, orderLine.OrderLineDetail.InnerPacks.Value);
			AssertEquals(11.11m, orderLine.OrderLineDetail.OuterPacks.Value);
			AssertEquals(9.09m, orderLine.OrderLineDetail.ItemPrice.Value);
			AssertEquals(0m, orderLine.OrderLineDetail.LinePrice.Value);
			AssertEquals("prtattr1", orderLine.OrderLineDetail.PartAttrib1);
			AssertEquals("prtattr2", orderLine.OrderLineDetail.PartAttrib2);
			AssertEquals("prtattr3", orderLine.OrderLineDetail.PartAttrib3);
			AssertEquals("custattr1", orderLine.OrderLineDetail.Custom.Text1);
			AssertEquals("custattr2", orderLine.OrderLineDetail.Custom.Text2);
			AssertEquals("custattr3", orderLine.OrderLineDetail.Custom.Text3);
			AssertEquals("custattr4", orderLine.OrderLineDetail.Custom.Text4);
			AssertEquals("custattr5", orderLine.OrderLineDetail.Custom.Text5);
			AssertEquals("custattr6", orderLine.OrderLineDetail.Custom.Text6);
			AssertEquals("custtextblob", orderLine.OrderLineDetail.Custom.CustomText1);
			AssertEquals(false, orderLine.OrderLineDetail.Custom.Flag1);
			AssertEquals(false, orderLine.OrderLineDetail.Custom.Flag2);
			AssertEquals(false, orderLine.OrderLineDetail.Custom.Flag3);
			AssertEquals(false, orderLine.OrderLineDetail.Custom.Flag4);
			AssertEquals(false, orderLine.OrderLineDetail.Custom.Flag5);
			AssertEquals(new ZDateTime(2005, 1, 1), orderLine.OrderLineDetail.Custom.Date1);
			AssertEquals(new ZDateTime(2005, 1, 2), orderLine.OrderLineDetail.Custom.Date2);
			AssertEquals(new ZDateTime(2005, 1, 3), orderLine.OrderLineDetail.Custom.Date3);
			AssertEquals(new ZDateTime(2005, 1, 4), orderLine.OrderLineDetail.Custom.Date4);
			AssertEquals(new ZDateTime(2005, 1, 5), orderLine.OrderLineDetail.Custom.Date5);
			AssertEquals(1.01m, orderLine.OrderLineDetail.Custom.Decimal1);
			AssertEquals(1.02m, orderLine.OrderLineDetail.Custom.Decimal2);
			AssertEquals(1.03m, orderLine.OrderLineDetail.Custom.Decimal3);
			AssertEquals(1.04m, orderLine.OrderLineDetail.Custom.Decimal4);
			AssertEquals(1.05m, orderLine.OrderLineDetail.Custom.Decimal5);
			AssertEquals("10", orderLine.OrderLineDetail.ContainerNumber);
			AssertEquals(3, orderLine.OrderLineDetail.ContainerPackingOrder);
			AssertEquals(1000.001m, orderLine.OrderLineDetail.QtyInvoiced.Value);
			AssertEquals(200.002m, orderLine.OrderLineDetail.QtyReceived.Value);
			AssertEquals(10.123m, orderLine.OrderLineDetail.Volume);
			AssertEquals(123.01m, orderLine.OrderLineDetail.Weight);
			AssertEquals("M3", orderLine.OrderLineDetail.VolumeType);
			AssertEquals("KG", orderLine.OrderLineDetail.WeightType);
			AssertEquals("special instractions", orderLine.OrderLineDetail.SpecialInstructions);
			AssertEquals("additional information", orderLine.OrderLineDetail.AdditionalInformation);

			AssertEquals(2, orderLine.OrderLineDeliveries.Count);

			Xsd.OrderOrderLineOrderLineDelivery delivery = orderLine.OrderLineDeliveries[0];

			AssertEquals("Sydney", delivery.DeliveryDetails.DelPort.City);
			AssertEquals("Australia", delivery.DeliveryDetails.DelPort.Country);
			AssertEquals("ADDRESS_SYD", delivery.DeliveryDetails.AddressFreeText);
			AssertEquals((ZDecimal)10, delivery.DeliveryDetails.QtyAllocated);
			AssertEquals("Attr1", delivery.DeliveryDetails.Custom.Text1);
			AssertEquals("Attr2", delivery.DeliveryDetails.Custom.Text2);
			AssertEquals("Attr3", delivery.DeliveryDetails.Custom.Text3);
			AssertEquals("Attr4", delivery.DeliveryDetails.Custom.Text4);
			AssertEquals("Attr5", delivery.DeliveryDetails.Custom.Text5);
			AssertEquals((ZDecimal)1.00, delivery.DeliveryDetails.Custom.Decimal1);
			AssertEquals((ZDecimal)2.00, delivery.DeliveryDetails.Custom.Decimal2);
			AssertEquals((ZDecimal)3.00, delivery.DeliveryDetails.Custom.Decimal3);
			AssertEquals((ZDecimal)4.00, delivery.DeliveryDetails.Custom.Decimal4);
			AssertEquals((ZDecimal)5.00, delivery.DeliveryDetails.Custom.Decimal5);
			AssertEquals(true, delivery.DeliveryDetails.Custom.Flag1);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag2);
			AssertEquals(true, delivery.DeliveryDetails.Custom.Flag3);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag4);
			AssertEquals(true, delivery.DeliveryDetails.Custom.Flag5);
			AssertEquals(new ZDateTime(2005, 1, 1), orderLine.OrderLineDetail.Custom.Date1);
			AssertEquals(new ZDateTime(2005, 1, 2), orderLine.OrderLineDetail.Custom.Date2);
			AssertEquals(new ZDateTime(2005, 1, 3), orderLine.OrderLineDetail.Custom.Date3);
			AssertEquals(new ZDateTime(2005, 1, 4), orderLine.OrderLineDetail.Custom.Date4);
			AssertEquals(new ZDateTime(2005, 1, 5), orderLine.OrderLineDetail.Custom.Date5);

			delivery = orderLine.OrderLineDeliveries[1];

			AssertEquals("Sydney", delivery.DeliveryDetails.DelPort.City);
			AssertEquals("Australia", delivery.DeliveryDetails.DelPort.Country);
			AssertEquals("ADDRESS_BNE", delivery.DeliveryDetails.AddressFreeText);
			AssertEquals((ZDecimal)90, delivery.DeliveryDetails.QtyAllocated);
			AssertEquals("", delivery.DeliveryDetails.Custom.Text1);
			AssertEquals("", delivery.DeliveryDetails.Custom.Text2);
			AssertEquals("", delivery.DeliveryDetails.Custom.Text3);
			AssertEquals("", delivery.DeliveryDetails.Custom.Text4);
			AssertEquals("", delivery.DeliveryDetails.Custom.Text5);
			AssertEquals(ZDecimal.Zero, delivery.DeliveryDetails.Custom.Decimal1);
			AssertEquals(ZDecimal.Zero, delivery.DeliveryDetails.Custom.Decimal2);
			AssertEquals(ZDecimal.Zero, delivery.DeliveryDetails.Custom.Decimal3);
			AssertEquals(ZDecimal.Zero, delivery.DeliveryDetails.Custom.Decimal4);
			AssertEquals(ZDecimal.Zero, delivery.DeliveryDetails.Custom.Decimal5);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag1);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag2);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag3);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag4);
			AssertEquals(false, delivery.DeliveryDetails.Custom.Flag5);
			AssertEquals(new ZDateTime(2005, 1, 1), orderLine.OrderLineDetail.Custom.Date1);
			AssertEquals(new ZDateTime(2005, 1, 2), orderLine.OrderLineDetail.Custom.Date2);
			AssertEquals(new ZDateTime(2005, 1, 3), orderLine.OrderLineDetail.Custom.Date3);
			AssertEquals(new ZDateTime(2005, 1, 4), orderLine.OrderLineDetail.Custom.Date4);
			AssertEquals(new ZDateTime(2005, 1, 5), orderLine.OrderLineDetail.Custom.Date5);

			orderLine = orderValue.OrderLines[1];
			AssertEquals(1002.ToString(), orderLine.OrderLineNo.ToString());
			AssertEquals(1122.ToString(), orderLine.OrderSubLineNo.ToString());
			AssertEquals(9.09m, orderLine.OrderLineDetail.ItemPrice.Value);
			AssertEquals(365.1m, orderLine.OrderLineDetail.LinePrice.Value);

			AssertEquals(0, orderLine.OrderLineDeliveries.Count);

			orderValue = ordersValue.Order[1];
			AssertNotNull(orderValue);
			AssertEquals(1, orderValue.OrderLines.Count);

			AssertEquals("654321", orderValue.OrderIdentifier.OrderNumber);
			AssertEquals("222222", orderValue.OrderDetail.InvoiceNumber);

			orderLine = orderValue.OrderLines[0];

			AssertEquals(1331.ToString(), orderLine.OrderLineNo.ToString());
			AssertEquals(3311.ToString(), orderLine.OrderSubLineNo.ToString());

			AssertEquals(1, orderLine.OrderLineDeliveries.Count);

			delivery = orderLine.OrderLineDeliveries[0];

			AssertEquals("Sydney", delivery.DeliveryDetails.DelPort.City);
			AssertEquals("Australia", delivery.DeliveryDetails.DelPort.Country);
			AssertEquals("ADDRESS_SYD", delivery.DeliveryDetails.AddressFreeText);
			AssertEquals((ZDecimal)20, delivery.DeliveryDetails.QtyAllocated);
			AssertEquals("Attr1", delivery.DeliveryDetails.Custom.Text1);
			AssertEquals("Attr2", delivery.DeliveryDetails.Custom.Text2);
			AssertEquals((ZDecimal)1.00, delivery.DeliveryDetails.Custom.Decimal1);
			AssertEquals((ZDecimal)5.00, delivery.DeliveryDetails.Custom.Decimal5);
			AssertEquals(true, delivery.DeliveryDetails.Custom.Flag1);
		}

		#region SupplierInvoiceDate

		public void TestSupplierInvoiceDateMapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.SupplierInvoiceDate, xsdOrder.OrderDetail.InvoiceDate);
			});
		}

		public void TestSupplierInvoiceDateMapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.SupplierInvoiceDate = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.InvoiceDate);
			});
		}

		public void TestSupplierInvoiceDateMapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.SupplierInvoiceDate = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.InvoiceDate);
			});
		}

		#endregion

		#region OrderDate

		public void TestOrderDateMapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.OrderDate, xsdOrder.OrderDetail.OrderDateTime);
			});
		}

		public void TestOrderDateMapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.OrderDate = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.OrderDateTime);
			});
		}

		public void TestOrderDateMapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.OrderDate = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.OrderDateTime);
			});
		}

		#endregion

		#region ExWorksRequiredBy

		public void TestExWorksRequiredByMapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.ExWorksRequiredBy, xsdOrder.OrderDetail.ExWorksRequiredBy);
			});
		}

		public void TestExWorksRequiredByMapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.ExWorksRequiredBy = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.ExWorksRequiredBy);
			});
		}

		public void TestExWorksRequiredByMapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.ExWorksRequiredBy = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.ExWorksRequiredBy);
			});
		}

		#endregion

		#region DeliveryRequiredBy

		public void TestDeliveryRequiredByMapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.DeliveryRequiredBy, xsdOrder.OrderDetail.DeliveryRequiredBy);
			});
		}

		public void TestDeliveryRequiredByMapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.DeliveryRequiredBy = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.DeliveryRequiredBy);
			});
		}

		public void TestDeliveryRequiredByMapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.DeliveryRequiredBy = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.DeliveryRequiredBy);
			});
		}

		#endregion

		#region ConfirmationDate

		public void TestConfirmationDateMapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.ConfirmationDate, xsdOrder.OrderDetail.ConfirmDate);
			});
		}

		public void TestConfirmationDateMapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.ConfirmationDate = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.ConfirmDate);
			});
		}

		public void TestConfirmationDateMapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.ConfirmationDate = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.ConfirmDate);
			});
		}

		#endregion

		#region CustomDate1

		public void TestCustomDate1Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.CustomDate1, xsdOrder.OrderDetail.Custom.Date1);
			});
		}

		public void TestCustomDate1Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.CustomDate1 = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.Custom.Date1);
			});
		}

		public void TestCustomDate1Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.CustomDate1 = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.Custom.Date1);
			});
		}

		#endregion

		#region CustomDate2

		public void TestCustomDate2Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(csvOrderHeader.CustomDate2, xsdOrder.OrderDetail.Custom.Date2);
			});
		}

		public void TestCustomDate2Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.CustomDate2 = null;

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.Custom.Date2);
			});
		}

		public void TestCustomDate2Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderHeader = new OrderHeaderCSV().Populate();
			csvOrderHeader.CustomDate2 = new DateTime(1800, 1, 1);

			TestOrderHeaderMapping(csvOrderHeader, xsdOrder =>
			{
				AssertEquals(default(DateTime), xsdOrder.OrderDetail.Custom.Date2);
			});
		}

		#endregion

		#region LineDropDate

		public void TestLineDropDateMapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(csvOrderLine.LineDropDate, xsdOrderLine.OrderLineDetail.DropDate);
			});
		}

		public void TestLineDropDateMapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.LineDropDate = null;

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.DropDate);
			});
		}

		public void TestLineDropDateMapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.LineDropDate = new DateTime(1800, 1, 1);

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.DropDate);
			});
		}

		#endregion

		#region LineCustomDate1

		public void TestLineCustomDate1Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(csvOrderLine.CustomDate1, xsdOrderLine.OrderLineDetail.Custom.Date1);
			});
		}

		public void TestLineCustomDate1Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate1 = null;

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date1);
			});
		}

		public void TestLineCustomDate1Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate1 = new DateTime(1800, 1, 1);

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date1);
			});
		}

		#endregion

		#region LineCustomDate2

		public void TestLineCustomDate2Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(csvOrderLine.CustomDate2, xsdOrderLine.OrderLineDetail.Custom.Date2);
			});
		}

		public void TestLineCustomDate2Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate2 = null;

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date2);
			});
		}

		public void TestLineCustomDate2Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate2 = new DateTime(1800, 1, 1);

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date2);
			});
		}

		#endregion

		#region LineCustomDate3

		public void TestLineCustomDate3Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(csvOrderLine.CustomDate3, xsdOrderLine.OrderLineDetail.Custom.Date3);
			});
		}

		public void TestLineCustomDate3Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate3 = null;

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date3);
			});
		}

		public void TestLineCustomDate3Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate3 = new DateTime(1800, 1, 1);

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date3);
			});
		}

		#endregion

		#region LineCustomDate4

		public void TestLineCustomDate4Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(csvOrderLine.CustomDate4, xsdOrderLine.OrderLineDetail.Custom.Date4);
			});
		}

		public void TestLineCustomDate4Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate4 = null;

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date4);
			});
		}

		public void TestLineCustomDate4Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate4 = new DateTime(1800, 1, 1);

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date4);
			});
		}

		#endregion

		#region LineCustomDate5

		public void TestLineCustomDate5Mapping_TheDateIsValid_ShouldMapToThisDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(csvOrderLine.CustomDate5, xsdOrderLine.OrderLineDetail.Custom.Date5);
			});
		}

		public void TestLineCustomDate5Mapping_TheDateIsEmpty_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate5 = null;

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date5);
			});
		}

		public void TestLineCustomDate5Mapping_TheDateIsOutOfExpectedRange_ShouldMapToDefaultDate()
		{
			var csvOrderLine = new OrderLineCSV().Populate();
			csvOrderLine.CustomDate5 = new DateTime(1800, 1, 1);

			TestOrderLineMapping(csvOrderLine, xsdOrderLine =>
			{
				AssertEquals(default(DateTime), xsdOrderLine.OrderLineDetail.Custom.Date5);
			});
		}

		#endregion

		#region Implementation

		static string ToCSVLineFormat(DateTime? dt)
		{
			return dt.HasValue ? dt.Value.ToString("yyyyMMdd") : "";
		}

		OrderConverter Converter
		{
			get { return converter ?? (converter = new OrderConverter(Notifications, Factory)); }
		}
		OrderConverter converter;

		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		void TestOrderHeaderMapping(OrderHeaderCSV csvOrderHeader, Action<Xsd.Order> test)
		{
			var ordersValue = new Xsd.Orders();
			var orderHeaderCSVLine = csvOrderHeader.GenerateLine();
			var orderHeaderCSVLineBytes = Encoding.Default.GetBytes(orderHeaderCSVLine);

			using (var reader = new StreamReader(new MemoryStream(orderHeaderCSVLineBytes)))
			{
				Converter.ImportFlatFile(ordersValue, new CsvFlatFileFormat(), reader);
			}

			test(ordersValue.Order[0]);
		}

		void TestOrderLineMapping(OrderLineCSV csvOrderLine, Action<Xsd.OrderOrderLine> test)
		{
			var csv = new StringBuilder();
			csv.AppendLine(new OrderHeaderCSV().Populate().GenerateLine());
			csv.AppendLine(csvOrderLine.GenerateLine());

			var ordersValue = new Xsd.Orders();
			var orderHeaderCSVLineBytes = Encoding.Default.GetBytes(csv.ToString());

			using (var reader = new StreamReader(new MemoryStream(orderHeaderCSVLineBytes)))
			{
				Converter.ImportFlatFile(ordersValue, new CsvFlatFileFormat(), reader);
			}

			test(ordersValue.Order[0].OrderLines[0]);
		}

		#region Types

		class OrderHeaderCSV
		{
			#region Fields

			public string RecordStructureAndVersion;
			public string SendingEmailURL;
			public string OrderNumber;
			public string SupplierInvoiceNumber;
			public DateTime? SupplierInvoiceDate;
			public string TransportMode;
			public string ContainerMode;
			public DateTime? OrderDate;
			public DateTime? ExWorksRequiredBy;
			public DateTime? DeliveryRequiredBy;
			public string OrderStatus;
			public string OrderCurrency;
			public int TotalOrderAmount;
			public Decimal EstimatedExchangeRate;
			public string IncoTerm;
			public string AdditionalTerms;
			public string OrderGoodsDescription;
			public int TotalPacks;
			public string PackType;
			public int Volume;
			public string UnitOfVolum;
			public int Weight;
			public string UnitOfWeight;
			public int ConfirmationNumber;
			public DateTime? ConfirmationDate;
			public string CountryOfOrigin;
			public string SupplierCode;
			public string SupplierContact;
			public string SupplierCompanyName;
			public string SupplierAddress1;
			public string SupplierAddress2;
			public string SupplierCity;
			public string SupplierState;
			public int SupplierPostCode;
			public string SupplierISOCountryCode;
			public string SupplierCountryName;
			public string SupplierUNLOCO;
			public string SupplierPhones;
			public string SupplierEmailAddress;
			public string BuyerCode;
			public string BuyerContact;
			public string BuyerCompanyName;
			public string BuyerAddress1;
			public string BuyerAddress2;
			public string BuyerCity;
			public string BuyerState;
			public int BuyerPostCode;
			public string BuyerISOCountryCode;
			public string BuyerCountryName;
			public string BuyerUNLOCO;
			public string BuyerPhones;
			public string BuyerEmailAddress;
			public string HouseBill;
			public string DepartureVesselFlight;
			public string DepartureVoyageFlight;
			public string GoodsOrigin;
			public string GoodsDestination;
			public string GoodsAvaliableAt;
			public string GoodsDeliveredTo;
			public string LoadPort;
			public string DischargePort;
			public string SendingAgent;
			public string ReceivingAgent;
			public DateTime? CustomDate1;
			public DateTime? CustomDate2;
			public string CustomAttrib1;
			public string CustomAttrib2;
			public string CustomAttrib3;
			public string CustomAttrib4;
			public string CustomAttrib5;
			public string CustomFlag1;
			public string CustomFlag2;
			public string CustomFlag3;
			public string CustomFlag4;
			public string CustomFlag5;
			public Decimal CustomDecimal1;
			public Decimal CustomDecimal2;
			public Decimal CustomDecimal3;
			public Decimal CustomDecimal4;
			public Decimal CustomDecimal5;
			public string CustomContact1;
			public string CustomContact2;
			public string GoodsHandlingNotes;
			public string DGAdditionalHandlingNotes;
			public string SpecialInstructions;
			public string DeliveryInstructions;

			#endregion

			public OrderHeaderCSV Populate()
			{
				RecordStructureAndVersion = "1.0";
				SendingEmailURL = "ron.dennis@mclaren.com";
				OrderNumber = "123";
				SupplierInvoiceNumber = "111";
				SupplierInvoiceDate = new DateTime(2005, 03, 01);
				TransportMode = "SEA";
				ContainerMode = "FCL";
				OrderDate = new DateTime(2006, 03, 01);
				ExWorksRequiredBy = new DateTime(2006, 05, 01);
				DeliveryRequiredBy = new DateTime(2006, 04, 01);
				OrderStatus = "INC";
				OrderCurrency = "USD";
				TotalOrderAmount = 365;
				EstimatedExchangeRate = 2.33m;
				IncoTerm = "INCO";
				AdditionalTerms = "McLaren";
				OrderGoodsDescription = "MP4-27";
				TotalPacks = 3;
				PackType = "BLH";
				Volume = 325;
				UnitOfVolum = "M3";
				Weight = 32;
				UnitOfWeight = "KG";
				ConfirmationNumber = 112324;
				ConfirmationDate = new DateTime(2006, 04, 10);
				CountryOfOrigin = "UA";
				SupplierCode = "MCLAREN";
				SupplierContact = "Ron Dennis";
				SupplierCompanyName = "McLaren";
				SupplierAddress1 = "5";
				SupplierAddress2 = "";
				SupplierCity = "Woking";
				SupplierState = "Surrey";
				SupplierPostCode = 35100;
				SupplierISOCountryCode = "GB";
				SupplierCountryName = "England";
				SupplierUNLOCO = "GBWOK";
				SupplierPhones = "911";
				SupplierEmailAddress = "ron.dennis@mclaren.com";
				BuyerCode = "MCLAREN";
				BuyerContact = "Ron Dennis";
				BuyerCompanyName = "McLaren";
				BuyerAddress1 = "5";
				BuyerAddress2 = "";
				BuyerCity = "Woking";
				BuyerState = "Surrey";
				BuyerPostCode = 35100;
				BuyerISOCountryCode = "GB";
				BuyerCountryName = "England";
				BuyerUNLOCO = "GBWOK";
				BuyerPhones = "911";
				BuyerEmailAddress = "ron.dennis@mclaren.com";
				HouseBill = "McLaren";
				DepartureVesselFlight = "depvessel";
				DepartureVoyageFlight = "depvoyage";
				GoodsOrigin = "UAIEV";
				GoodsDestination = "AUSYD";
				GoodsAvaliableAt = "avaliableat";
				GoodsDeliveredTo = "deliveredto";
				LoadPort = "AUBNE";
				DischargePort = "AUMEL";
				SendingAgent = "MCLAREN";
				ReceivingAgent = "MANUTD";
				CustomDate1 = new DateTime(2005, 01, 01);
				CustomDate2 = new DateTime(2005, 01, 02);
				CustomAttrib1 = "custattr1";
				CustomAttrib2 = "custattr2";
				CustomAttrib3 = "custattr3";
				CustomAttrib4 = "custattr4";
				CustomAttrib5 = "custattr5";
				CustomFlag1 = "N";
				CustomFlag2 = "Y";
				CustomFlag3 = "N";
				CustomFlag4 = "Y";
				CustomFlag5 = "N";
				CustomDecimal1 = 1.01m;
				CustomDecimal2 = 1.02m;
				CustomDecimal3 = 1.03m;
				CustomDecimal4 = 1.04m;
				CustomDecimal5 = 1.05m;
				CustomContact1 = "McLaren";
				CustomContact2 = "ManUtd";
				GoodsHandlingNotes = "In case of damage, I will kill you";
				DGAdditionalHandlingNotes = "the death will be slow";
				SpecialInstructions = "i.e. very slow";
				DeliveryInstructions = "only using McLaren F1";

				return this;
			}

			public string GenerateLine()
			{
				var properties = new[]
				{
					"POH",
					RecordStructureAndVersion,
					SendingEmailURL,
					OrderNumber,
					SupplierInvoiceNumber,
					ToCSVLineFormat(SupplierInvoiceDate),
					TransportMode,
					ContainerMode,
					ToCSVLineFormat(OrderDate),
					ToCSVLineFormat(ExWorksRequiredBy),
					ToCSVLineFormat(DeliveryRequiredBy),
					OrderStatus,
					OrderCurrency,
					TotalOrderAmount.ToString(),
					EstimatedExchangeRate.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					"",
					IncoTerm,
					AdditionalTerms,
					OrderGoodsDescription,
					TotalPacks.ToString(),
					PackType,
					"",
					Volume.ToString(),
					UnitOfVolum,
					"",
					Weight.ToString(),
					UnitOfWeight,
					ConfirmationNumber.ToString(),
					ToCSVLineFormat(ConfirmationDate),
					CountryOfOrigin,
					"",
					"",
					"",
					SupplierCode,
					SupplierContact,
					SupplierCompanyName,
					SupplierAddress1,
					SupplierAddress2,
					SupplierCity,
					SupplierState,
					SupplierPostCode.ToString(),
					SupplierISOCountryCode,
					SupplierCountryName,
					SupplierUNLOCO,
					SupplierPhones,
					SupplierEmailAddress,
					"",
					"",
					BuyerCode,
					BuyerContact,
					BuyerCompanyName,
					BuyerAddress1,
					BuyerAddress2,
					BuyerCity,
					BuyerState,
					BuyerPostCode.ToString(),
					BuyerISOCountryCode,
					BuyerCountryName,
					BuyerUNLOCO,
					BuyerPhones,
					BuyerEmailAddress,
					HouseBill,
					DepartureVesselFlight,
					DepartureVoyageFlight,
					GoodsOrigin,
					GoodsDestination,
					GoodsAvaliableAt,
					GoodsDeliveredTo,
					LoadPort,
					DischargePort,
					SendingAgent,
					ReceivingAgent,
					"",
					ToCSVLineFormat(CustomDate1),
					ToCSVLineFormat(CustomDate2),
					CustomAttrib1,
					CustomAttrib2,
					CustomAttrib3,
					CustomAttrib4,
					CustomAttrib5,
					CustomFlag1,
					CustomFlag2,
					CustomFlag3,
					CustomFlag4,
					CustomFlag5,
					CustomDecimal1.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal2.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal3.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal4.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal5.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomContact1,
					CustomContact2,
					"",
					GoodsHandlingNotes,
					DGAdditionalHandlingNotes,
					SpecialInstructions,
					DeliveryInstructions
				};

				return string.Join(",", properties);
			}
		}

		class OrderLineCSV
		{
			#region Fields

			public int ClientOrderLineNumber;
			public int ClientOrderSubLineNumber;
			public string SupplierProductCode;
			public string SupplierProductDescription;
			public string LocalProductCode;
			public string LocalProductDescription;
			public string LineStatus;
			public DateTime? LineDropDate;
			public Decimal ProductQuantityOrdered;
			public string ProductUQ;
			public Decimal InnerPacks;
			public Decimal OuterPacks;
			public Decimal ProductUnitPrice;
			public Decimal ProductLinePrice;
			public string PartAttribute1;
			public string PartAttribute2;
			public string PartAttribute3;
			public string CustomAttribute1;
			public string CustomAttribute2;
			public string CustomAttribute3;
			public string CustomAttribute4;
			public string CustomAttribute5;
			public string CustomAttribute6;
			public string CustomTextBlob;
			public string CustomFlag1;
			public string CustomFlag2;
			public string CustomFlag3;
			public string CustomFlag4;
			public string CustomFlag5;
			public DateTime? CustomDate1;
			public DateTime? CustomDate2;
			public DateTime? CustomDate3;
			public DateTime? CustomDate4;
			public DateTime? CustomDate5;
			public Decimal CustomDecimal1;
			public Decimal CustomDecimal2;
			public Decimal CustomDecimal3;
			public Decimal CustomDecimal4;
			public Decimal CustomDecimal5;
			public int ContainerNumber;
			public int ContainerPuckingOrder;
			public Decimal UnitQtyInvoiced;
			public Decimal UnitQuantityReceived;
			public Decimal ActualVolume;
			public string VolumeUnit;
			public Decimal ActualWeight;
			public string WeightUnit;
			public string SpecialInstractions;
			public string AdditionalInformation;

			#endregion

			public OrderLineCSV Populate()
			{
				ClientOrderLineNumber = 1001;
				ClientOrderSubLineNumber = 1111;
				SupplierProductCode = "McLaren";
				SupplierProductDescription = "ManUtd";
				LocalProductCode = "Perez";
				LocalProductDescription = "Button";
				LineStatus = "INC";
				LineDropDate = new DateTime(2006, 03, 01);
				ProductQuantityOrdered = 10.12m;
				ProductUQ = "UQ";
				InnerPacks = 10.10m;
				OuterPacks = 11.11m;
				ProductUnitPrice = 09.09m;
				ProductLinePrice = 08.08m;
				PartAttribute1 = "pa1";
				PartAttribute2 = "pa2";
				PartAttribute3 = "pa3";
				CustomAttribute1 = "ca1";
				CustomAttribute2 = "ca2";
				CustomAttribute3 = "ca3";
				CustomAttribute4 = "ca4";
				CustomAttribute5 = "ca5";
				CustomAttribute6 = "ca6";
				CustomTextBlob = "blob";
				CustomFlag1 = "fl1";
				CustomFlag2 = "fl2";
				CustomFlag3 = "fl3";
				CustomFlag4 = "fl4";
				CustomFlag5 = "fl5";
				CustomDate1 = new DateTime(2012, 01, 01);
				CustomDate2 = new DateTime(2012, 01, 02);
				CustomDate3 = new DateTime(2012, 01, 03);
				CustomDate4 = new DateTime(2012, 01, 04);
				CustomDate5 = new DateTime(2012, 01, 05);
				CustomDecimal1 = 1.01m;
				CustomDecimal2 = 1.02m;
				CustomDecimal3 = 1.03m;
				CustomDecimal4 = 1.04m;
				CustomDecimal5 = 1.05m;
				ContainerNumber = 10;
				ContainerPuckingOrder = 3;
				UnitQtyInvoiced = 1000.001m;
				UnitQuantityReceived = 200.002m;
				ActualVolume = 10.123m;
				VolumeUnit = "M3";
				ActualWeight = 123.01m;
				WeightUnit = "KG";
				SpecialInstractions = "McLaren";
				AdditionalInformation = "ManUtd";

				return this;
			}

			public string GenerateLine()
			{
				var properties = new[]
				{
					"POL",
					ClientOrderLineNumber.ToString(),
					ClientOrderSubLineNumber.ToString(),
					SupplierProductCode,
					SupplierProductDescription,
					LocalProductCode,
					LocalProductDescription,
					LineStatus,
					"",
					ToCSVLineFormat(LineDropDate),
					"",
					ProductQuantityOrdered.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					ProductUQ,
					InnerPacks.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					OuterPacks.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					ProductUnitPrice.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					ProductLinePrice.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					"",
					"",
					"",
					"",
					"",
					PartAttribute1,
					PartAttribute2,
					PartAttribute3,
					CustomAttribute1,
					CustomAttribute2,
					CustomAttribute3,
					CustomAttribute4,
					CustomAttribute5,
					CustomAttribute6,
					CustomTextBlob,
					CustomFlag1,
					CustomFlag2,
					CustomFlag3,
					CustomFlag4,
					CustomFlag5,
					ToCSVLineFormat(CustomDate1),
					ToCSVLineFormat(CustomDate2),
					ToCSVLineFormat(CustomDate3),
					ToCSVLineFormat(CustomDate4),
					ToCSVLineFormat(CustomDate5),
					CustomDecimal1.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal2.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal3.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal4.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					CustomDecimal5.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					ContainerNumber.ToString(),
					ContainerPuckingOrder.ToString(),
					UnitQtyInvoiced.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					UnitQuantityReceived.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					ActualVolume.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					VolumeUnit,
					ActualWeight.ToString("G", CultureInfo.GetCultureInfo("en-US")),
					WeightUnit,
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					"",
					SpecialInstractions,
					AdditionalInformation
				};

				return string.Join(",", properties);
			}
		}

		#endregion

		#endregion
	}
}
