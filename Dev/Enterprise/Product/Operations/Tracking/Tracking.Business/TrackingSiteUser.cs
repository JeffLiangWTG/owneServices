using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// User of a tracking web site
	/// </summary>
	public class TrackingSiteUser : OrgContactWebUser
	{
		#region GetClientPortalUrl

		public string GetClientPortalUrl()
		{
			string result = string.Empty;
			StringBuilder portalUrlBuilder = new StringBuilder();

			if (IsLoggedIn && !LoggedInOrganisation.MiscServ.OM_CMClientPortalHomePage.IsEmpty)
			{
				string orgCode = LoggedInOrganisation.OH_Code;
				string orgName = LoggedInOrganisation.OH_FullNameTruncated;
				string email = LoggedInUser.OC_Email;
				string contactName = LoggedInUser.OC_ContactName;
				string sessionID = HttpContext.Current.Session.SessionID;
				string sharedSecret = WebDataRegistry.Instance.WebTrackerSharedSecret.Value;
				string mD5Hash = ComputeMD5Hash(orgCode, email, sessionID, sharedSecret);

				portalUrlBuilder.AppendFormat("?{0}={1}", "OrgCode", Uri.EscapeDataString(orgCode));
				portalUrlBuilder.AppendFormat("&{0}={1}", "OrgName", Uri.EscapeDataString(orgName));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Email", Uri.EscapeDataString(email));
				portalUrlBuilder.AppendFormat("&{0}={1}", "ContactName", Uri.EscapeDataString(contactName));
				portalUrlBuilder.AppendFormat("&{0}={1}", "SessionID", Uri.EscapeDataString(sessionID));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Hash", Uri.EscapeDataString(mD5Hash));

				ZString warehouseNames = GetWarehouseNames(LoggedInOrganisation.Factory);

				if (!warehouseNames.IsEmpty)
				{
					portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Whs", Uri.EscapeDataString(warehouseNames));
				}

				portalUrlBuilder.AppendFormat("&{0}={1}", "WhsInventory", new ZBool(CanViewInventory));
				portalUrlBuilder.AppendFormat("&{0}={1}", "WhsOrders", new ZBool(CanViewWarehouseOrders));
				portalUrlBuilder.AppendFormat("&{0}={1}", "WhsReceipts", new ZBool(CanViewWarehouseReceipts));
				portalUrlBuilder.AppendFormat("&{0}={1}", "WhsProducts", new ZBool(CanViewWarehouseProducts));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Shipments", new ZBool(CanViewShipments));
				portalUrlBuilder.AppendFormat("&{0}={1}", "CFSShipments", new ZBool(CanViewCFSShipments));
				portalUrlBuilder.AppendFormat("&{0}={1}", "ISF", new ZBool(CanViewISF));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Bookings", new ZBool(CanViewBookings));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Orders", new ZBool(CanViewOrders));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Containers", new ZBool(CanViewTrackingContainers));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Quotes", new ZBool(CanViewQuotations));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Reports", new ZBool(CanViewReports(WebReportModes.All)));
				portalUrlBuilder.AppendFormat("&{0}={1}", (NoResString)"Accounts", new ZBool(CanViewAccounts));

				result = LoggedInOrganisation.MiscServ.OM_CMClientPortalHomePage + portalUrlBuilder.ToString();
			}

			return result;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected string GetWarehouseNames(BusinessObjectFactory factory)
		{
			WhsWarehouse[] warehouses = factory.Load<WhsWarehouse>(OrgRestrictionFilterFactory.Instance.GetFilter<WhsWarehouse>());

			ZStringBuilder codes = new ZStringBuilder();
			for (int i = 0; i < warehouses.Length; i++)
			{
				codes.Append(warehouses[i].WW_WarehouseName);
			}

			return codes.ToStringWithDelimiterBetweenAppends(",");
		}

		protected string ComputeMD5Hash(string orgCode, string emailAddress, string sessionID, string sharedSecret)
		{
			string inputForHash = orgCode + emailAddress + sessionID + sharedSecret;
			byte[] data = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(inputForHash));

			StringBuilder stringBuilder = new StringBuilder();

			foreach (byte num in data)
			{
				stringBuilder.Append(num.ToString("x2"));
			}

			return stringBuilder.ToString();
		}

		#endregion

		#region CurrentModuleName

		public ZString CurrentModuleName
		{
			get { return currentModuleName; }
			set { currentModuleName = value; }
		}

		ZString currentModuleName;

		#endregion

		#region Quotation Tab & Functionality

		public bool CanViewQuotations
		{
			get { return !IsShipmentQuickViewUser && CanViewTab(WebDataRegistry.Instance.UseWebForwardingQuotesModule, WebSecurityRightsList.WebQuotes); }
		}

		#endregion Quotation Tab & Functionality

		#region Accounts Tab & Functionality

		public bool CanViewAccounts
		{
			get { return !IsShipmentQuickViewUser && CanViewTab(WebDataRegistry.Instance.UseWebAccountsModule, WebSecurityRightsList.WebInvoicingAndStatements); }
		}

		#endregion Accounts Tab & Functionality

		#region CFSShipment

		public bool CanViewCFSShipments
		{
			get { return CanViewTab(WebDataRegistry.Instance.UseWebCFSShipmentsModule, WebSecurityRightsList.WebCFSShipmentView); }
		}

		#endregion

		#region Declarations

		public bool CanViewDeclarations => CanViewTab(WebDataRegistry.Instance.UseWebDeclarationModule, WebSecurityRightsList.WebDeclarationView);

		#endregion

		#region Shipments

		public bool CanViewShipments
		{
			get { return CanViewTab(WebDataRegistry.Instance.UseWebForwardingShipmentsModule, WebSecurityRightsList.WebShipmentsView); }
		}

		#endregion

		#region Bookings tab

		public bool CanViewBookings
		{
			get { return CanViewTab(WebDataRegistry.Instance.UseWebForwardingBookingsModule, WebSecurityRightsList.WebBookingsView); }
		}

		public bool CanEditBookings
		{
			get { return !IsShipmentQuickViewUser && CanViewBookings && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebBookingsAddEdit)); }
		}

		public bool CanEditContainerNumbers
		{
			get { return !IsShipmentQuickViewUser && CanEditBookings && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersAddEditNumber)); }
		}

		public bool CanBookSailings
		{
			get { return !IsShipmentQuickViewUser && CanEditBookings && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebBookingsSelectSchedules)); }
		}

		#endregion

		#region Events

		public bool CanViewEvents
		{
			get { return IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebEventsView); }
		}

		#endregion

		#region MAWB

		public bool CanViewMAWB
		{
			get { return CanViewTab(WebDataRegistry.Instance.UseWebMAWBModule, WebSecurityRightsList.WebMAWBView); }
		}

		public bool CanEditMAWB
		{
			get { return !IsShipmentQuickViewUser && CanViewMAWB && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebMAWBEdit)); }
		}

		public bool CanSendMAWB
		{
			get { return CanEditMAWB && IsRightGranted(WebSecurityRightsList.WebMAWBSend); }
		}

		public bool CanAdminMAWB
		{
			get { return IsRightGranted(WebSecurityRightsList.WebMAWBAdmin); }
		}

		#endregion

		#region HAWB

		public bool CanViewHAWB
		{
			get { return CanViewTab(WebDataRegistry.Instance.UseWebHAWBModule, WebSecurityRightsList.WebHAWBView); }
		}

		public bool CanEditHAWB
		{
			get { return !IsShipmentQuickViewUser && CanViewHAWB && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebHAWBEdit)); }
		}

		public bool CanSendHAWB
		{
			get { return CanEditHAWB && IsRightGranted(WebSecurityRightsList.WebHAWBSend); }
		}

		public bool CanAdminHAWB
		{
			get { return IsRightGranted(WebSecurityRightsList.WebHAWBAdmin); }
		}

		#endregion

		#region Transports Tab

		public bool CanViewCartage
		{
			get { return !IsShipmentQuickViewUser && CanViewTab(WebDataRegistry.Instance.UseWebCartageModule, WebSecurityRightsList.WebCartageView); }
		}

		#endregion

		#region ISF Tab

		public bool CanViewISF
		{
			get { return !IsShipmentQuickViewUser && CanViewTab(WebDataRegistry.Instance.UseWebISFModule, WebSecurityRightsList.WebISFView); }
		}

		public bool CanEditISF
		{
			get { return !IsShipmentQuickViewUser && CanViewISF && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebISFAddEdit)); }
		}

		public bool CanSendISF
		{
			get { return !IsShipmentQuickViewUser && CanEditISF && IsRightGranted(WebSecurityRightsList.WebISFSend); }
		}

		public bool CanDeleteISF
		{
			get { return !IsShipmentQuickViewUser && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebISFDelete)); }
		}

		#endregion ISF Tab

		#region Orders tab

		public bool CanViewOrders
		{
			get { return CanViewTab(WebDataRegistry.Instance.UseWebForwardingOrdersModule, WebSecurityRightsList.WebOrdersView); }
		}

		public bool CanEditOrders
		{
			get { return !IsShipmentQuickViewUser && CanViewOrders && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebOrdersAddEdit)); }
		}

		public bool CanSplitOrders
		{
			get { return !IsShipmentQuickViewUser && CanViewOrders && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebOrdersAllowSplit)); }
		}

		#endregion

		#region IsWarehouseClient

		public bool IsWarehouseClient
		{
			get
			{
				bool result = false;

				if (LoggedInOrganisation != null)
				{
					result = IsSuperUser || LoggedInOrganisation.OH_IsWarehouseClient;
				}

				return result;
			}
		}

		#endregion

		#region IsForwarder

		public bool IsForwarder
		{
			get
			{
				bool result = false;

				if (LoggedInOrganisation != null)
				{
					result = IsSuperUser || LoggedInOrganisation.OH_IsForwarder || LoggedInOrganisation.OH_IsConsignor;
				}

				return result;
			}
		}

		#endregion

		#region InventoryTab

		public bool CanViewInventory
		{
			get { return !IsShipmentQuickViewUser && IsWarehouseClient && CanViewTab(WebDataRegistry.Instance.UseWebWarehouseInventoryModule, WebSecurityRightsList.WebInventoryView); }
		}

		#endregion

		#region WarehouseOrders

		public bool CanViewWarehouseOrders
		{
			get { return IsWarehouseClient && CanViewTab(WebDataRegistry.Instance.UseWebWarehouseOrdersModule, WebSecurityRightsList.WebWarehouseOrdersView); }
		}

		public bool CanEditWarehouseOrders
		{
			get { return !IsShipmentQuickViewUser && CanViewWarehouseOrders && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebWarehouseOrdersAddEdit)); }
		}
		#endregion

		#region WarehouseReceipts

		public bool CanViewWarehouseReceipts
		{
			get { return !IsShipmentQuickViewUser && IsWarehouseClient && CanViewTab(WebDataRegistry.Instance.UseWebWarehouseReceiptsModule, WebSecurityRightsList.WebWarehouseReceiptsView); }
		}

		public bool CanEditWarehouseReceipts
		{
			get { return !IsShipmentQuickViewUser && CanViewWarehouseReceipts && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebWarehouseReceiptsAddEdit)); }
		}
		#endregion

		#region WarehouseProducts

		public bool CanViewWarehouseProducts
		{
			get { return !IsShipmentQuickViewUser && IsWarehouseClient && CanViewTab(WebDataRegistry.Instance.UseWebWarehouseProductsModule, WebSecurityRightsList.WebWarehouseProductsView); }
		}
		#endregion

		#region TrackingContainers

		public bool CanViewTrackingContainers
		{
			get { return IsShipmentQuickViewUser || CanViewTab(WebDataRegistry.Instance.UseWebForwardingContainersModule, WebSecurityRightsList.WebContainers); }
		}

		public bool CanEditTrackingContainers
		{
			get { return !IsShipmentQuickViewUser && CanViewTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersEdit)); }
		}

		public bool CanEditContainersClientRef
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersEditClientRef)); }
		}

		public bool CanEditContainersRequiredDeliveryDate
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersReqDeliveryDate)); }
		}

		public bool CanEditContainersActualDeliveryDate
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersActDeliveryDate)); }
		}

		public bool CanEditContainersConfirmedDeliveryDate
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersConDeliveryDate)); }
		}

		public bool CanEditContainersEstimatedDehireDate
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersEstDehireDate)); }
		}

		public bool CanEditContainersPickupDate
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersPickup)); }
		}

		public bool CanEditContainersActualDehireDate
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersActualDehire)); }
		}

		public bool CanEditContainersSequence
		{
			get { return !IsShipmentQuickViewUser && CanEditTrackingContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebContainersSequence)); }
		}

		#endregion

		#region LinerAndAgencyContainers

		public bool CanViewLinerAndAgencyContainers
		{
			get { return IsShipmentQuickViewUser || CanViewTab(WebDataRegistry.Instance.UseWebLinerAndAgencyContainersModule, WebSecurityRightsList.WebLinerAndAgencyContainers); }
		}

		public bool CanEditLinerAndAgencyContainers
		{
			get { return !IsShipmentQuickViewUser && CanViewLinerAndAgencyContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersEdit)); }
		}

		public bool CanEditLinerAndAgencyContainersEstimatedFullDeliveryDate
		{
			get { return !IsShipmentQuickViewUser && CanEditLinerAndAgencyContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersEstimatedFullDeliveryDate)); }
		}

		public bool CanEditLinerAndAgencyContainersActualDeliveryDate
		{
			get { return !IsShipmentQuickViewUser && CanEditLinerAndAgencyContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersActDeliveryDate)); }
		}

		public bool CanEditLinerAndAgencyContainersEmptyReadyToReturnDate
		{
			get { return !IsShipmentQuickViewUser && CanEditLinerAndAgencyContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReadyToReturnDate)); }
		}

		public bool CanEditLinerAndAgencyContainersEmptyReturnReqByDate
		{
			get { return !IsShipmentQuickViewUser && CanEditLinerAndAgencyContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReturnReqByDate)); }
		}

		public bool CanEditLinerAndAgencyContainersEmptyReadyOnDate
		{
			get { return !IsShipmentQuickViewUser && CanEditLinerAndAgencyContainers && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersEmptyReadyOnDate)); }
		}

		#endregion

		#region Reports

		public bool CanViewReports(WebReportModes reportMode)
		{
			if (reportMode == WebReportModes.All)
			{
				return CanViewReports(WebReportModes.Customs) ||
						CanViewReports(WebReportModes.Freight) ||
						CanViewReports(WebReportModes.LinerAgency) ||
						CanViewReports(WebReportModes.Transport) ||
						CanViewReports(WebReportModes.Warehouse);
			}
			return !IsShipmentQuickViewUser && CanViewTab(GetUseWebReportsModule(reportMode), WebSecurityRightsList.WebReports);
		}

		protected BooleanRegistryItem GetUseWebReportsModule(WebReportModes reportMode)
		{
			switch (reportMode)
			{
				case WebReportModes.Freight:
					return WebDataRegistry.Instance.UseWebForwardingReportsModule;
				case WebReportModes.LinerAgency:
					return WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule;
				case WebReportModes.Customs:
					return WebDataRegistry.Instance.UseWebCustomsReportsModule;
				case WebReportModes.Transport:
					return WebDataRegistry.Instance.UseWebTransportReportsModule;
				case WebReportModes.Warehouse:
					return WebDataRegistry.Instance.UseWebWarehouseReportsModule;
			}
			return WebDataRegistry.Instance.UseWebForwardingReportsModule;
		}

		#endregion

		#region Liner And Agency

		public bool CanViewLinerAndAgencyBookings
		{
			get { return IsForwarder && CanViewTab(WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule, WebSecurityRightsList.WebLinerAndAgencyBookingsView); }
		}

		public bool CanEditLinerAndAgencyBookings
		{
			get { return !IsShipmentQuickViewUser && IsForwarder && CanViewTab(WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule, WebSecurityRightsList.WebLinerAndAgencyBookingsAddEdit); }
		}

		public bool CanViewLinerAndAgencyBillsOfLading
		{
			get { return IsForwarder && CanViewTab(WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule, WebSecurityRightsList.WebLinerAndAgencyBillsOfLadingView); }
		}

		public bool CanEditLinerAndAgencyFwdInstructions
		{
			get { return !IsShipmentQuickViewUser && IsForwarder && CanViewTab(WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule, WebSecurityRightsList.WebLinerAndAgencyFwdInstructionsEdit); }
		}

		public bool CanEditLinerAndAgencyContainerNumbers
		{
			get { return !IsShipmentQuickViewUser && CanEditLinerAndAgencyBookings && (IsSuperUser || IsRightGranted(WebSecurityRightsList.WebLinerAndAgencyContainersAddEditNumber)); }
		}

		#endregion

		#region Documents

		public bool CanViewDocuments
		{
			get
			{
				return !IsShipmentQuickViewUser && IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebDocumentsView);
			}
		}

		public override bool CanViewDocument(BusinessObjectFactory factory, RefDocType docType)
		{
			if (docType == null)
			{
				return false;
			}

			var documentRights = new DocumentWebSecurityRights(factory);
			var right = documentRights.GetSecurityRight(docType);

			return right == null ? CanViewDocuments : IsRightGranted(right);
		}

		public bool CanAddDocuments
		{
			get
			{
				return !IsShipmentQuickViewUser && IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebDocumentsAdd);
			}
		}

		#endregion

		#region Milestones

		public bool CanUpdateEstimatedMilestones
		{
			get { return IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebEstimatedMilestonesUpdate); }
		}

		public bool CanUpdateActualMilestones
		{
			get { return IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebActualMilestonesUpdate); }
		}

		#endregion

		#region Shipments

		public bool CanAddDeliveryRequest
		{
			get { return !IsShipmentQuickViewUser && IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebShipmentDeliveryAdd); }
		}

		public bool CanEditDeliveryRequest
		{
			get { return !IsShipmentQuickViewUser && IsLoggedIn && IsRightGranted(WebSecurityRightsList.WebShipmentDeliveryEdit); }
		}

		#endregion

		#region Implementation

		bool CanViewTab(BooleanRegistryItem useModuleRegistryItem, WebSecurityRight tabSecurityRight)
		{
			return IsSuperUser ||
				(useModuleRegistryItem.Value &&
				IsLoggedIn &&
				IsRightGranted(tabSecurityRight));
		}

		#endregion

		#region IsShipmentQuickViewUser

		public bool IsShipmentQuickViewUser
		{
			get { return IsLoggedIn && (IsSuperUser || IsWebUser) && LoggedInUser.OC_OH == GlbCompany.CurrentCompany.OrgProxy.PK; }
		}

		#endregion

		#region GetRelatedBranch

		protected override ZGuid GetBranchPKForLogin()
		{
			ZGuid result = !IsShipmentQuickViewUser ? base.GetBranchPKForLogin() : ZGuid.Empty; // Disable changing Branch and Company for the ShipmentQuickViewUser
#if DEBUG
			BranchPKForLoginForTest = result;
#endif
			return result;
		}

#if DEBUG
		internal ZGuid BranchPKForLoginForTest;
#endif

		#endregion

		public bool CanAccessGlowTrackingPortal => false;

		public GlbCompanyCollection GetTransactionCompanies(BusinessObjectFactory factory)
		{
			if (LoggedInOrganisation == null)
			{
				return new GlbCompanyCollection(factory, new ZQuery { IsNoResultQuery = true });
			}

			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_GC);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, LoggedInOrganisation.PK);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);

			var filter = new ZDBOnlyQuery(typeof(GlbCompany));
			filter.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			filter.AddSubQuery(GlbCompanySchema.PK, subQuery, JoinCondition.And);

			return new GlbCompanyCollection(factory, filter);
		}
	}
}
