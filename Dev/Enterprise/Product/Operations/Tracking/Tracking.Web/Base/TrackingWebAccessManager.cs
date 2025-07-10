using System.Collections.Generic;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	public class TrackingWebAccessManager : ZWebAccessManager
	{
		#region Constructors

		public TrackingWebAccessManager(ZGlobal appInstance)
			: base(appInstance)
		{
		}

		#endregion

		#region Overrides

		protected override bool IsReportsPageCore(string pageRelativePath)
		{
			return (pageRelativePath == TrackingConstants.RelativePath.ReportsPage);
		}

		protected override List<ILicenceCheckpoint> GetLicenceCheckpointsCore(string pageRelativePath)
		{
			List<ILicenceCheckpoint> result = base.GetLicenceCheckpointsCore(pageRelativePath);

			if (IsWebTrackerBooking(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerBooking);
			}

			if (IsWebTrackerForwarding(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerForwarding);
			}

			if (IsWebTrackerImportBrokerage(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerImportBrokerage);
			}

			if (IsWebTrackerExportBrokerage(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerExportBrokerage);
			}

			if (IsWebTrackerOrderManager(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerOrderManager);
			}

			if (IsWebTrackerLinerAndAgencyManagerBillsOfLading(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerShippingManagerBillsOfLading);
			}

			if (IsWebTrackerLinerAndAgencyManagerBookings(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerShippingManagerBookings);
			}

			if (IsWebTrackerWarehouse(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerWarehouse);
			}

			if (IsImporterSecurityFiling(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.ImporterSecurityFiling);
			}

			if (IsWebTrackerLocalTransport(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.WebTrackerLocalTransport);
			}

			if (IsAccountant(pageRelativePath))
			{
				result.Add(Environment.Env.Licence.Accountant);
			}

			return result;
		}

		#endregion

		#region Implementation

		protected bool IsWebTrackerWarehouse(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.WarehouseOrderLineAllocationPage:
				case TrackingConstants.RelativePath.WarehouseReceiveDetailsPage:
				case TrackingConstants.RelativePath.WarehouseReceiptsPage:
				case TrackingConstants.RelativePath.WarehouseOrdersPage:
				case TrackingConstants.RelativePath.WarehouseOrderDetailsPage:
				case TrackingConstants.RelativePath.ProductProfilesPage:
				case TrackingConstants.RelativePath.ProductProfileDetailsPage:
				case TrackingConstants.RelativePath.InventoryDetailsPage:
				case TrackingConstants.RelativePath.InventoryPage:
				case TrackingConstants.RelativePath.EditWarehouseReceivePage:
				case TrackingConstants.RelativePath.EditWarehouseOrderPage:
				case TrackingConstants.RelativePath.ProductImagePage:
					return true;
			}
			return false;
		}

		protected bool IsAccountant(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.TransactionsPage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerLocalTransport(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.CartagePage:
					return true;
			}
			return false;
		}

		protected bool IsImporterSecurityFiling(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.ISFPage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerLinerAndAgencyManagerBookings(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.LinerAndAgencyBookingsPage:
				case TrackingConstants.RelativePath.LinerAndAgencyEditBookingPage:
				case TrackingConstants.RelativePath.LinerAndAgencyBookingDetailsPage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerLinerAndAgencyManagerBillsOfLading(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.LinerAndAgencyBillsOfLadingPage:
				case TrackingConstants.RelativePath.LinerAndAgencyBillOfLadingDetailsPage:
				case TrackingConstants.RelativePath.LinerAndAgencyEditForwardingInstructionPage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerOrderManager(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.ShipmentsPage:
				case TrackingConstants.RelativePath.OrdersPage:
				case TrackingConstants.RelativePath.OrderDetailsPage:
				case TrackingConstants.RelativePath.EditOrderPage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerExportBrokerage(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.DeclarationDetailsPage:
				case TrackingConstants.RelativePath.DeclarationModulePage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerImportBrokerage(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.DeclarationDetailsPage:
				case TrackingConstants.RelativePath.DeclarationModulePage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerForwarding(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.ShipmentsPage:
				case TrackingConstants.RelativePath.ShipmentDetailsPage:
				case TrackingConstants.RelativePath.EditContainerPage:
				case TrackingConstants.RelativePath.ContainerSummaryPage:
				case TrackingConstants.RelativePath.ContainersPage:
				case TrackingConstants.RelativePath.ContainerBatchUpdatePage:
				case TrackingConstants.RelativePath.ContainerDetailsPage:
					return true;
			}
			return false;
		}

		protected bool IsWebTrackerBooking(string pageRelativePath)
		{
			switch (pageRelativePath)
			{
				case TrackingConstants.RelativePath.QuotationsPage:
				case TrackingConstants.RelativePath.QuotationPage:
				case TrackingConstants.RelativePath.BookingsPage:
				case TrackingConstants.RelativePath.BookingDetailsPage:
				case TrackingConstants.RelativePath.EditBookingPage:
					return true;
			}
			return false;
		}

		#endregion
	}
}
