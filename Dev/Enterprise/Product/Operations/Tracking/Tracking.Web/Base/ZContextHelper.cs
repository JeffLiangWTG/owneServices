using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	public static class ZContextHelper
	{
		public static TrackingConstants.BusinessContext GetContext(this WebModuleId id)
		{
			var result = TrackingConstants.BusinessContext.NoBusinessContext;
			switch (id)
			{
				case WebModuleId.TrackingShipments:
					result = TrackingConstants.BusinessContext.Shipment;
					break;
				case WebModuleId.TrackingDeclarations:
					result = TrackingConstants.BusinessContext.Declaration;
					break;
				case WebModuleId.TrackingBookings:
					result = TrackingConstants.BusinessContext.Booking;
					break;
				case WebModuleId.TrackingOrders:
					result = TrackingConstants.BusinessContext.Order;
					break;
				case WebModuleId.TrackingWarehouseOrders:
					result = TrackingConstants.BusinessContext.WarehouseOrder;
					break;
				case WebModuleId.TrackingAccounts:
					result = TrackingConstants.BusinessContext.Transaction;
					break;
				case WebModuleId.eDoc:
					result = TrackingConstants.BusinessContext.eDoc;
					break;
				case WebModuleId.TrackingCartage:
					result = TrackingConstants.BusinessContext.Cartage;
					break;
				case WebModuleId.TrackingImporterSecurityFiling:
					result = TrackingConstants.BusinessContext.ISF;
					break;
				case WebModuleId.TrackingQuotations:
					result = TrackingConstants.BusinessContext.Quotations;
					break;
				case WebModuleId.TrackingHousebill:
					result = TrackingConstants.BusinessContext.HouseBill;
					break;
				case WebModuleId.TrackingFreightLabel:
					result = TrackingConstants.BusinessContext.FreightLabel;
					break;
				case WebModuleId.TrackingDefault:
					result = TrackingConstants.BusinessContext.NoBusinessContext;
					break;
			}
			return result;
		}

		public static WebModuleId GetModuleId(this TrackingConstants.BusinessContext context)
		{
			var result = WebModuleId.NotAssignedWeb;

			switch (context)
			{
				case TrackingConstants.BusinessContext.Shipment:
					result = WebModuleId.TrackingShipments;
					break;
				case TrackingConstants.BusinessContext.Declaration:
					result = WebModuleId.TrackingDeclarations;
					break;
				case TrackingConstants.BusinessContext.Booking:
					result = WebModuleId.TrackingBookings;
					break;
				case TrackingConstants.BusinessContext.Order:
					result = WebModuleId.TrackingOrders;
					break;
				case TrackingConstants.BusinessContext.WarehouseOrder:
					result = WebModuleId.TrackingWarehouseOrders;
					break;
				case TrackingConstants.BusinessContext.Transaction:
					result = WebModuleId.TrackingAccounts;
					break;
				case TrackingConstants.BusinessContext.eDoc:
					result = WebModuleId.eDoc;
					break;
				case TrackingConstants.BusinessContext.Cartage:
					result = WebModuleId.TrackingCartage;
					break;
				case TrackingConstants.BusinessContext.ISF:
					result = WebModuleId.TrackingImporterSecurityFiling;
					break;
				case TrackingConstants.BusinessContext.QuotationClientReplyAccept:
				case TrackingConstants.BusinessContext.QuotationClientReplyNotAccept:
				case TrackingConstants.BusinessContext.Quotations:
					result = WebModuleId.TrackingQuotations;
					break;

				case TrackingConstants.BusinessContext.HouseBill:
					result = WebModuleId.TrackingHousebill;
					break;
				case TrackingConstants.BusinessContext.FreightLabel:
					result = WebModuleId.TrackingFreightLabel;
					break;
				case TrackingConstants.BusinessContext.NoBusinessContext:
					result = WebModuleId.TrackingDefault;
					break;
			}
			return result;
		}

		public static string GetModuleOption(this TrackingConstants.BusinessContext context)
		{
			switch (context)
			{
				case TrackingConstants.BusinessContext.QuotationClientReplyAccept:
					return WebModuleOptions.TrackingQuotations.ClientAccepts;
				case TrackingConstants.BusinessContext.QuotationClientReplyNotAccept:
					return WebModuleOptions.TrackingQuotations.ClientNotAccepts;
			}
			return null;
		}
	}
}
