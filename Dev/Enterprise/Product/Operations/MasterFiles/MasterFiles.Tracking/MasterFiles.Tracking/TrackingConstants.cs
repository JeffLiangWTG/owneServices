namespace Enterprise.MasterFiles.Tracking
{
	public static class TrackingConstants
	{
		#region Business Context

		public enum BusinessContext
		{
			Shipment,
			Declaration,
			Order,
			Consol,
			WarehouseOrder,
			Booking,
			Transaction,
			Receive,
			eDoc,
			NoBusinessContext,
			Cartage,
			ISF,
			Quotations,
			QuotationClientReplyAccept,
			QuotationClientReplyNotAccept,
			HouseBill,
			FreightLabel,
			SupplierBooking,
			ContainerLoadList
		}

		public static string ToNeoAlias(this BusinessContext businessContext)
		{
			switch (businessContext)
			{
				case BusinessContext.Declaration:
					return "TrackingDeclaration";
				case BusinessContext.Order:
					return "TrackingOrder";
				case BusinessContext.WarehouseOrder:
					return "TrackingWarehouseOrder";
				case BusinessContext.Booking:
					return "TrackingBooking";
				case BusinessContext.Transaction:
					return "TrackingInvoice";
				case BusinessContext.Receive:
					return "TrackingWarehouseReceipt";
				case BusinessContext.Cartage:
					return "TrackingTransportJob";
				case BusinessContext.ISF:
					return "TrackingISF";
				case BusinessContext.Quotations:
					return "TrackingQuote";
				case BusinessContext.SupplierBooking:
					return "TrackingSupplierBooking";
				case BusinessContext.ContainerLoadList:
					return "TrackingContainerLoadList";
				default:
					return null;
			}
		}

		public static bool CanUseGuestTracking(this BusinessContext businessContext)
		{
			return businessContext == BusinessContext.Shipment;
		}

		#endregion

		#region Relative Path

		public static class RelativePath
		{
			public const string SystemDefaultPage = "Default.aspx";
			public const string AutoLoginRequestHandler = "AutoLoginRequestHandler.axd";
			public const string LoginPage = "Login/Login.aspx";
			public const string LoginSupersededPage = "Login/LoginSuperseded.aspx";
			public const string SetMasterPasswordPage = "Admin/SetMasterPassword.aspx";
			public const string LoginRedirectionPage = "Login/LoginRedirection.aspx";
			public const string LoginCompletePage = "Login/LoginComplete.aspx";
			public const string ForgotPasswordPage = "Login/ForgotPassword.aspx";
			public const string OrdersPage = "Orders/Orders.aspx";
			public const string OrderDetailsPage = "Orders/OrderDetails.aspx";
			public const string EditOrderPage = "Orders/EditOrder.aspx";
			public const string FlightSchedulesPage = "Schedules/FlightSchedules.aspx";
			public const string SailingSchedulesPage = "Schedules/SailingSchedules.aspx";
			public const string RoadSchedulesPage = "Schedules/RoadSchedules.aspx";
			public const string RailSchedulesPage = "Schedules/RailSchedules.aspx";
			public const string ShipmentsPage = "Shipments/Shipments.aspx";
			public const string ShipmentPage = "Shipments/Shipment.aspx";
			public const string ShipmentDetailsPage = "Shipments/ShipmentDetails.aspx";
			public const string CFSShipmentsPage = "CFSShipments/CFSShipments.aspx";
			public const string CFSShipmentDetailsPage = "CFSShipments/CFSShipmentDetails.aspx";
			public const string DeclarationModulePage = "Declaration/Declarations.aspx";
			public const string DeclarationDetailsPage = "Declaration/DeclarationDetails.aspx";
			public const string QuotationsPage = "Quotes/Quotations.aspx";
			public const string QuotationPage = "Quotes/Quotation.aspx";
			public const string ReportsPage = "Reports/Reports.aspx";
			public const string TransactionsPage = "Accounts/Transactions.aspx";
			public const string BookingsPage = "Bookings/Bookings.aspx";
			public const string EditBookingPage = "Bookings/EditBooking.aspx";
			public const string BookingDetailsPage = "Bookings/BookingDetails.aspx";
			public const string ISFDetailsPage = "ImporterSecurityFiling/ImporterSecurityFilingDetails.aspx";
			public const string ISFPage = "ImporterSecurityFiling/ImporterSecurityFiling.aspx";
			public const string EditISFPage = "ImporterSecurityFiling/EditImporterSecurityFiling.aspx";
			public const string InventoryPage = "Warehousing/Inventory.aspx";
			public const string InventoryDetailsPage = "Warehousing/InventoryDetails.aspx";
			public const string WarehouseOrdersPage = "Warehousing/WarehouseOrders.aspx";
			public const string WarehouseOrderDetailsPage = "Warehousing/WarehouseOrderDetails.aspx";
			public const string EditWarehouseOrderPage = "Warehousing/EditWarehouseOrder.aspx";
			public const string ChangePasswordPage = "Admin/ChangePassword.aspx";
			public const string ResetMasterPasswordPage = "Admin/ResetMasterPassword.aspx";
			public const string ResetPasswordPage = "Admin/ResetPassword.aspx";
			public const string SetPasswordPage = "Admin/SetPassword.aspx";
			public const string ProductProfilesPage = "Warehousing/OrgSupplierParts.aspx";
			public const string ProductProfileDetailsPage = "Warehousing/ProductDetails.aspx";
			public const string ProductImagePage = "Warehousing/ProductImage.aspx";
			public const string CartagePage = "Cartage/Cartages.aspx";
			public const string CartageDetailsPage = "Cartage/CartageDetails.aspx";
			public const string ContainersPage = "Containers/Containers.aspx";
			public const string ContainerBatchUpdatePage = "Containers/ContainerBatchUpdate.aspx";
			public const string ContainerSummaryPage = "Containers/ContainerSummary.aspx";
			public const string ContainerDetailsPage = "Containers/ContainerDetails.aspx";
			public const string LinerAndAgencyContainersPage = "LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainers.aspx";
			public const string LinerAndAgencyContainerDetailsPage = "LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainerDetails.aspx";
			public const string EditContainerPage = "Containers/EditContainer.aspx";
			public const string EditLinerAndAgencyContainerPage = "LinerAndAgency/LinerAndAgencyContainers/EditLinerAndAgencyContainer.aspx";
			public const string WarehouseReceiptsPage = "Warehousing/WarehouseReceipts.aspx";
			public const string WarehouseReceiveDetailsPage = "Warehousing/WarehouseReceiveDetails.aspx";
			public const string EditWarehouseReceivePage = "Warehousing/EditWarehouseReceive.aspx";
			public const string CustomsExchangeRatesPage = "ExchangeRates/ExchangeRates.aspx";
			public const string TermsAndConditionsPage = "TermsAndConditions.aspx";
			public const string ViewTermsAndConditionsPage = "Terms/ViewTermsAndConditions.aspx";
			public const string SwitchCompanyPage = "Admin/SwitchCompany.aspx";
			public const string WarehouseOrderLineAllocationPage = "Warehousing/WhsOrderLineAllocation.aspx";
			public const string LinerAndAgencyBookingsPage = "LinerAndAgency/Bookings/Bookings.aspx";
			public const string LinerAndAgencyBookingDetailsPage = "LinerAndAgency/Bookings/BookingDetails.aspx";
			public const string LinerAndAgencyEditBookingPage = "LinerAndAgency/Bookings/EditBooking.aspx";
			public const string LinerAndAgencyBillsOfLadingPage = "LinerAndAgency/BillsOfLading/BillsOfLading.aspx";
			public const string LinerAndAgencyBillOfLadingDetailsPage = "LinerAndAgency/BillsOfLading/BillOfLadingDetails.aspx";
			public const string LinerAndAgencyEditForwardingInstructionPage = "LinerAndAgency/BillsOfLading/EditFwdInstruction.aspx";
			public const string MAWBPage = "AWB/MAWB/MAWBs.aspx";
			public const string MAWBDetailsPage = "AWB/MAWB/MAWBDetails.aspx";
			public const string HAWBPage = "AWB/HAWB/HAWBs.aspx";
			public const string HAWBDetailsPage = "AWB/HAWB/HAWBDetails.aspx";
			public const string EDIMessagesPage = "Messaging/EDIMessages.aspx";
			public const string MAWBUploadPage = "AWB/MAWBUpload.aspx";
			public const string HAWBListPage = "AWB/HAWB/HAWBList.aspx";
			public const string LinesMappingPage = "Bookings/OrderLinesToPackLinesMappingPopup/OrderLinesToPackLinesMappingPage.aspx";
			public const string GlowRedirectPage = "Glow/GlowRedirect.aspx";
			public const string PreloadPage = "Preload.aspx";
			public const string LogoutPage = "Logout.aspx";
		}

		public static class PreloadRelativePath
		{
			public const string Accounts = "Accounts/_Preload.aspx";
			public const string Admin = "Admin/_Preload.aspx";
			public const string Bookings = "Bookings/_Preload.aspx";
			public const string Cartage = "Cartage/_Preload.aspx";
			public const string CFSShipments = "CFSShipments/_Preload.aspx";
			public const string Containers = "Containers/_Preload.aspx";
			public const string Declaration = "Declaration/_Preload.aspx";
			public const string ImporterSecurityFiling = "ImporterSecurityFiling/_Preload.aspx";
			public const string LinerAndAgencyBillsOfLading = "LinerAndAgency/BillsOfLading/_Preload.aspx";
			public const string LinerAndAgencyBookings = "LinerAndAgency/Bookings/_Preload.aspx";
			public const string LinerAndAgencyContainers = "LinerAndAgency/LinerAndAgencyContainers/_Preload.aspx";
			public const string Orders = "Orders/_Preload.aspx";
			public const string Quotes = "Quotes/_Preload.aspx";
			public const string Reports = "Reports/_Preload.aspx";
			public const string Schedules = "Schedules/_Preload.aspx";
			public const string Shipments = "Shipments/_Preload.aspx";
			public const string Terms = "Terms/_Preload.aspx";
			public const string Warehousing = "Warehousing/_Preload.aspx";
		}

		#endregion

		public static class QueryStringKeys
		{
			#region SuppressResourceStringsCheckRegion
			public const string RefKey = "Ref";
			public const string PopupKey = "Popup";
			public const string WhsProductRefKey = "WhsProductRef";
			public const string WarehouseRefKey = "WarehouseRef";
			public const string Helper = "Helper";
			public const string ContentType = "ContentType";
			public const string ResetPasswordKey = "ResetKey";
			public const string SetPasswordKey = "SetKey";

			#endregion
		}

		#region Auto Login

		public static class AutoLogin
		{
			public const string SecureQueryStringDataKey = "AutoLogin";
			public const string ContactPKKey = "ContactPK";
			public const string RequireLoginKey = "RequireLogin";
			public const string BusinessContextKey = "BusinessContext";
			public const string BusinessContextPKKey = "BusinessContextPK";
			public const string BusinessContextNKKey = "BusinessContextNK";
			public const string BusinessContextAdditionalRefsKey = "BusinessContextAdditionalRefs";
		}

		#endregion

		public static string ApplicationCookieName
		{
			get
			{
				return "EDIWebTracker";
			}
		}
	}
}
