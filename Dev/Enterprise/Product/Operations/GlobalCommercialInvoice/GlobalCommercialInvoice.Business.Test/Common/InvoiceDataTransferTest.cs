using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(InvoiceDataTransfer))]
	public class InvoiceDataTransferTest : TestCaseWithFactory
	{
		public void TestConvertBookingWithQuoteToShipment()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var importer = Factory.CreateNewOrganization();
				var supplier = Factory.CreateNewOrganization();
				var booking = Factory.CreateNewBookingWithQuote();
				var bookingInvoiceHeader = Factory.CreateInvoiceHeader((BusinessObject)booking);
				bookingInvoiceHeader.GIH_OH_Importer = importer.PK;
				bookingInvoiceHeader.GIH_OH_Supplier = supplier.PK;
				bookingInvoiceHeader.GIH_Description = "BOOKING INV.HEADER";

				var bookingInvoiceLine = Factory.CreateInvoiceLine(bookingInvoiceHeader);
				bookingInvoiceLine.Headers = new GlobalCommercialInvoiceHeaderCollection(Factory, booking.ViewPK, "TH");
				bookingInvoiceLine.GIL_Description = "BOOKING INV.LINE";
				bookingInvoiceLine.GIL_Tariff1 = "8482";
				bookingInvoiceLine.GIL_Tariff2 = "9492";

				InvoiceDataTransfer.BookingWithQuoteToShipment(booking.ViewPK, booking.ForwardingShipment.PK, Factory);

				var shipmentInvoiceHeader = Factory.LoadTop1<GlobalCommercialInvoiceHeader>(new ZQuery(GlobalCommercialInvoiceHeaderSchema.GIH_ParentID, booking.ForwardingShipment.PK));
				CombineAssertions("Shipment invoice header should be created.", () =>
				{
					AssertNotNull( shipmentInvoiceHeader);
					AssertEquals(bookingInvoiceHeader.GIH_OH_Importer, shipmentInvoiceHeader.GIH_OH_Importer);
					AssertEquals(bookingInvoiceHeader.GIH_OH_Supplier, shipmentInvoiceHeader.GIH_OH_Supplier);
					AssertEquals(bookingInvoiceHeader.GIH_Description, shipmentInvoiceHeader.GIH_Description);
				});

				var shipmentInvoiceLine = Factory.LoadTop1<GlobalCommercialInvoiceLine>(new ZQuery(GlobalCommercialInvoiceLineSchema.GIL_GIH_Header, shipmentInvoiceHeader.PK));
				CombineAssertions("Shipment invoice line should be created.", () =>
				{
					AssertNotNull(shipmentInvoiceLine);
					AssertEquals(bookingInvoiceLine.GIL_Description, shipmentInvoiceLine.GIL_Description);
					AssertEquals(bookingInvoiceLine.GIL_Tariff1, shipmentInvoiceLine.GIL_Tariff1);
					AssertEquals(bookingInvoiceLine.GIL_Tariff2, shipmentInvoiceLine.GIL_Tariff2);
				});
			}
		}
	}
}
