using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoiceJobProcessor))]
	public class GlobalCommercialInvoiceJobProcessorTest : TestCaseWithFactory
	{
		public void TestConvertBookingWithQuoteToShipment()
		{
			var ratingHeader = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
			ratingHeader[RatingHeaderSchema.TH_RateType] = "QTE";
			ratingHeader[RatingHeaderSchema.TH_QuoteDate] = ZDateTime.UtcNow.Date;

			var quotedBooking = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var shipmentBooking = (CommonShipment)quotedBooking.ForwardingShipment;
			shipmentBooking.JS_IsBooking = true;
			shipmentBooking.JS_IsForwardRegistered = false;
			shipmentBooking.JS_TH_OneTimeQuote = ratingHeader.PK;

			var bookingInvoiceHeader = Factory.CreateInvoiceHeader(quotedBooking as BusinessObject);
			bookingInvoiceHeader.GIH_Description = "BOOKING HEADER";
			bookingInvoiceHeader.GIH_ParentTableCode = ratingHeader.TablePrefix;
			var bookingInvoiceLine = Factory.CreateInvoiceLine(bookingInvoiceHeader);
			bookingInvoiceLine.GIL_Description = "BOOKING LINE";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipment = newFactory.Load<CommonShipment>(quotedBooking.ForwardingShipment.PK);

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				new BuildConsolHelper().TurnBookingIntoShipment(shipment, null, (quotedBooking as BusinessObject).PK);

				var shipmentInvoiceHeader = newFactory.Load<GlobalCommercialInvoiceHeader>(new ZQuery(GlobalCommercialInvoiceHeaderSchema.GIH_ParentID, shipment.PK)).FirstOrDefault();
				CombineAssertions("Shipment invoice header should be created.", () =>
				{
					AssertNotNull(shipmentInvoiceHeader);
					AssertEquals(bookingInvoiceHeader.GIH_Description, shipmentInvoiceHeader.GIH_Description);
				});

				var shipmentInvoiceLine = newFactory.Load<GlobalCommercialInvoiceLine>(new ZQuery(GlobalCommercialInvoiceLineSchema.GIL_GIH_Header, shipmentInvoiceHeader.PK)).FirstOrDefault();
				CombineAssertions("Shipment invoice line should be created.", () =>
				{
					AssertNotNull(shipmentInvoiceLine);
					AssertEquals(bookingInvoiceLine.GIL_Description, shipmentInvoiceLine.GIL_Description);
				});
			}
		}

		public void TestNotifyVolumeUQValueChanged()
		{
			var shipment = Factory.CreateNewShipment();
			var (headerCollection, lineCollection) = Factory.CreateNewHeaderAndLineCollection((BusinessObject)shipment);
			var header = headerCollection.AddNew();
			var invoiceLine1 = lineCollection.AddNew();
			CombineAssertions("Pre-condition unit of measurement", () =>
			{
				AssertEquals("Unit of Volume", Env.Registry.FreightVolumeUnit, invoiceLine1.GIL_VolumeUQ);
			});

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var invoiceLine2 = lineCollection.AddNew();
				invoiceLine2.GIL_VolumeUQ = Constants.Volume.CubicYards;

				shipment.JS_UnitOfVolume = Constants.Volume.CubicDecimetres;
				AssertEquals("Invoice line 1 unit of volume should changed", shipment.JS_UnitOfVolume, invoiceLine1.GIL_VolumeUQ);
				AssertEquals("Invoice line 2 Unit of Volume should not changed", Constants.Volume.CubicYards, invoiceLine2.GIL_VolumeUQ);
			}
		}

		public void TestNotifyWeightUQValueChanged()
		{
			var shipment = Factory.CreateNewShipment();
			var (headerCollection, lineCollection) = Factory.CreateNewHeaderAndLineCollection((BusinessObject)shipment);
			var header = headerCollection.AddNew();
			var invoiceLine1 = lineCollection.AddNew();
			CombineAssertions("Pre-condition unit of measurement", () =>
			{
				AssertEquals("Unit of Gross Weight", Env.Registry.FreightWeightUnit, invoiceLine1.GIL_GrossWeightUQ);
				AssertEquals("Unit of Net Weight", Env.Registry.FreightWeightUnit, invoiceLine1.GIL_NetWeightUQ);
			});

			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetGlobalCommercialInvoice(true)))
			{
				var invoiceLine2 = lineCollection.AddNew();
				invoiceLine2.GIL_GrossWeightUQ = Constants.Weight.LongTons;
				invoiceLine2.GIL_NetWeightUQ = Constants.Weight.MetricCarat;

				shipment.JS_UnitOfWeight = Constants.Weight.Ounces;
				AssertEquals("Invoice line 1 unit of gross weight should changed", shipment.JS_UnitOfWeight, invoiceLine1.GIL_GrossWeightUQ);
				AssertEquals("Invoice line 1 unit of net weight should changed", shipment.JS_UnitOfWeight, invoiceLine1.GIL_NetWeightUQ);
				AssertEquals("Invoice line 2 unit of gross weight should not changed", Constants.Weight.LongTons, invoiceLine2.GIL_GrossWeightUQ);
				AssertEquals("Invoice line 2 unit of net weight should not changed", Constants.Weight.MetricCarat, invoiceLine2.GIL_NetWeightUQ);
			}
		}
	}
}
