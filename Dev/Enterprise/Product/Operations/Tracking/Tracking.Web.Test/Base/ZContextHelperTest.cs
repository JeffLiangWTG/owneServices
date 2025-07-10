using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ZContextHelperTest : TestCase
	{
		public void TestGetContext()
		{
			AssertEquals(TrackingConstants.BusinessContext.Shipment, WebModuleId.TrackingShipments.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.Declaration, WebModuleId.TrackingDeclarations.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.Booking, WebModuleId.TrackingBookings.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.Order, WebModuleId.TrackingOrders.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.WarehouseOrder, WebModuleId.TrackingWarehouseOrders.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.Transaction, WebModuleId.TrackingAccounts.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.eDoc, WebModuleId.eDoc.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.Cartage, WebModuleId.TrackingCartage.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.ISF, WebModuleId.TrackingImporterSecurityFiling.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.Quotations, WebModuleId.TrackingQuotations.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.FreightLabel, WebModuleId.TrackingFreightLabel.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.HouseBill, WebModuleId.TrackingHousebill.GetContext());
			AssertEquals(TrackingConstants.BusinessContext.NoBusinessContext, WebModuleId.TrackingDefault.GetContext());
		}

		public void TestGetModuleId()
		{
			AssertEquals(TrackingConstants.BusinessContext.Shipment.GetModuleId(), WebModuleId.TrackingShipments);
			AssertEquals(TrackingConstants.BusinessContext.Declaration.GetModuleId(), WebModuleId.TrackingDeclarations);
			AssertEquals(TrackingConstants.BusinessContext.Booking.GetModuleId(), WebModuleId.TrackingBookings);
			AssertEquals(TrackingConstants.BusinessContext.Order.GetModuleId(), WebModuleId.TrackingOrders);
			AssertEquals(TrackingConstants.BusinessContext.WarehouseOrder.GetModuleId(), WebModuleId.TrackingWarehouseOrders);
			AssertEquals(TrackingConstants.BusinessContext.Transaction.GetModuleId(), WebModuleId.TrackingAccounts);
			AssertEquals(TrackingConstants.BusinessContext.eDoc.GetModuleId(), WebModuleId.eDoc);
			AssertEquals(TrackingConstants.BusinessContext.Cartage.GetModuleId(), WebModuleId.TrackingCartage);
			AssertEquals(TrackingConstants.BusinessContext.ISF.GetModuleId(), WebModuleId.TrackingImporterSecurityFiling);
			AssertEquals(TrackingConstants.BusinessContext.Quotations.GetModuleId(), WebModuleId.TrackingQuotations);
			AssertEquals(TrackingConstants.BusinessContext.FreightLabel.GetModuleId(), WebModuleId.TrackingFreightLabel);
			AssertEquals(TrackingConstants.BusinessContext.HouseBill.GetModuleId(), WebModuleId.TrackingHousebill);
			AssertEquals(TrackingConstants.BusinessContext.NoBusinessContext.GetModuleId(), WebModuleId.TrackingDefault);
		}
	}
}
