using System;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	public static class GlobalCommercialInvoiceHelperTest
	{
		public static IForwardingShipment CreateNewShipment(this BusinessObjectFactory factory) => factory.New<IForwardingShipment>();

		public static IQuotedBooking CreateNewBookingQuick(this BusinessObjectFactory factory) => ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, factory);

		public static IQuotedBooking CreateNewBookingWithQuote(this BusinessObjectFactory factory) => CreateNewBookingWithType(QuoteBookingType.BookingWithQuote, factory);

		public static IQuotedBooking CreateNewBookingSpotQuote(this BusinessObjectFactory factory) => CreateNewBookingWithType(QuoteBookingType.SpotQuote, factory);

		static IQuotedBooking CreateNewBookingWithType(QuoteBookingType bookingType, BusinessObjectFactory factory) => (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().
			InvokeMember("New",
			BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
			null, null, new object[] { bookingType, factory });

		public static GlobalCommercialInvoiceHeader CreateInvoiceHeader(this BusinessObjectFactory factory,
			BusinessObject parentJob)
		{
			return InvoiceHeaderSetDefaultValue(factory.New<GlobalCommercialInvoiceHeader>(), parentJob);
		}

		public static GlobalCommercialInvoiceHeader CreateInvoiceHeader(this GlobalCommercialInvoiceHeaderCollection headerCollection,
			BusinessObject parentJob)
		{
			return InvoiceHeaderSetDefaultValue(headerCollection.AddNew(), parentJob);
		}

		public static GlobalCommercialInvoiceHeader[] CreateInvoiceHeader(this BusinessObjectFactory factory,
			BusinessObject parentJob, int headerCount)
		{
			return Enumerable.Repeat(() => { return factory.CreateInvoiceHeader(parentJob); }, headerCount)
				.Select((f) => f()).ToArray();
		}

		public static GlobalCommercialInvoiceHeader[] CreateInvoiceHeader(this GlobalCommercialInvoiceHeaderCollection headerCollection, BusinessObject parentJob, int headerCount)
		{
			return Enumerable.Repeat(() => { return headerCollection.CreateInvoiceHeader(parentJob); }, headerCount)
				.Select((f) => f()).ToArray();
		}

		static GlobalCommercialInvoiceHeader InvoiceHeaderSetDefaultValue(GlobalCommercialInvoiceHeader invoiceHeader, BusinessObject parentJob)
		{
			invoiceHeader.GIH_ParentID = invoiceHeader.PK;
			invoiceHeader.GIH_Description = "Invoice Header Description";
			invoiceHeader.GIH_InvoiceNumber = Guid.NewGuid().ToString().Replace("-", string.Empty);
			invoiceHeader.GIH_InvoiceDate = ZDate.Today.AddDays(7);
			invoiceHeader.GIH_ParentID = parentJob.PK;
			invoiceHeader.GIH_ParentTableCode = parentJob.TablePrefix;
			return invoiceHeader;
		}

		public static GlobalCommercialInvoiceLine CreateInvoiceLine(this BusinessObjectFactory factory, GlobalCommercialInvoiceHeader invoiceHeader)
		{
			var invoiceLine = factory.New<GlobalCommercialInvoiceLine>();
			invoiceLine.GIL_Description = "Invoice Line Description";
			invoiceLine.Headers = new GlobalCommercialInvoiceHeaderCollection(factory, invoiceHeader.GIH_ParentID, invoiceHeader.GIH_ParentTableCode);
			invoiceLine.GIL_GIH_Header = invoiceHeader.PK;
			return invoiceLine;
		}

		public static GlobalCommercialInvoiceLine CreateInvoiceLine(
			this GlobalCommercialInvoiceLineIntegratedCollection lineCollection,
			GlobalCommercialInvoiceHeader invoiceHeader,
			bool setDescription = true)
		{
			var invoiceLine = lineCollection.AddNew();

			if (setDescription)
			{
				invoiceLine.GIL_Description = "Invoice Line Description";
			}

			invoiceLine.GIL_GIH_Header = invoiceHeader.PK;
			return invoiceLine;
		}

		public static SecurityCore CreateSecurityCore(this BusinessObjectFactory factory) => new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

		public static OrgHeader CreateNewOrganization(this BusinessObjectFactory factory)
		{
			 var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORGTEST";
			return orgHeader;
		}

		public static OrgAddress CreateNewOrganizationAddress(this BusinessObjectFactory factory, OrgHeader orgHeader)
		{
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Address1 = "Sydney Address";
			return orgAddress;
		}

		public static (GlobalCommercialInvoiceHeaderCollection HeaderCollection, GlobalCommercialInvoiceLineIntegratedCollection LineCollection) CreateNewHeaderAndLineCollection(this BusinessObjectFactory factory, BusinessObject hostBusinessObject)
		{
			var headerCollection = new GlobalCommercialInvoiceHeaderCollection(factory, hostBusinessObject.PK, hostBusinessObject.TablePrefix);
			var lineCollection = new GlobalCommercialInvoiceLineIntegratedCollection(hostBusinessObject, headerCollection);
			return (headerCollection, lineCollection);
		}
	}
}
