using System;
using System.Web;
using System.Web.UI.HtmlControls;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Base
{
	/// <summary>
	///	Header control for each web page in the project
	///	Displays:
	///		- Logo, header, etc.
	///		- Login status string (if logged in)
	///		- Menu (if logged in)
	/// </summary>
	public partial class PageHeaderWithNavigation : BaseUserControl
	{
		protected internal HtmlGenericControl pagehead;
		protected internal HtmlGenericControl menu;

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);

			if (((BasePage)Page).ShowNavigation)
			{
				ZNavigationBar navBar = LoadNavigationBar(Page.NavigationBarResource);
				SetupNavigationBar(navBar);
				menu.Controls.Add(navBar);
			}
		}

		protected virtual ZNavigationBar LoadNavigationBar(ZWebResource navigationResource)
		{
			return (ZNavigationBar)Page.LoadControl(navigationResource.FileName);
		}

		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
		}

		#endregion

		#region GetNavigationBar

		ZString GetReportPageParameters(WebReportModes mode)
		{
			return ZString.Format("{0}={1}", TrackingConstants.QueryStringKeys.ContentType, mode.ToString());
		}

#if DEBUG
		public
#else
		protected
#endif
 void SetupNavigationBar(ZNavigationBar bar)
		{
			bar.ShowWhenLoginOnly = true;
			NavigationElement forwardingMenu = new NavigationElement(Res.GetString("f1061727-4bd8-4487-8a3a-4d1ad5e112ba", "Forwarding"), "#");
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("829f6105-d97e-4c4a-af0c-e70af0de49a5", "Shipments"), TrackingConstants.RelativePath.ShipmentsPage, WebDataRegistry.Instance.UseWebForwardingShipmentsModule, WebSecurityRightsList.WebShipmentsView, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("d23018ff-bea7-4835-9b22-428ceca77e9c", "Bookings"), TrackingConstants.RelativePath.BookingsPage, WebDataRegistry.Instance.UseWebForwardingBookingsModule, WebSecurityRightsList.WebBookingsView, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("fc2f3cd0-cb93-4ec8-bb7b-1b0c0c6e0313", "Orders"), TrackingConstants.RelativePath.OrdersPage, WebDataRegistry.Instance.UseWebForwardingOrdersModule, WebSecurityRightsList.WebOrdersView, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("c89fc139-404b-4d3d-adcd-ed3619f53b2e", "Containers"), TrackingConstants.RelativePath.ContainersPage, WebDataRegistry.Instance.UseWebForwardingContainersModule, WebSecurityRightsList.WebContainers, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("18d26606-8066-419c-b195-4b644f9f0dc4", "Spot Quotes"), TrackingConstants.RelativePath.QuotationsPage, WebDataRegistry.Instance.UseWebForwardingQuotesModule, WebSecurityRightsList.WebQuotes, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("1211803f-6523-4a41-ad04-e86f18ae0a0e", "MAWBs"), TrackingConstants.RelativePath.MAWBPage, WebDataRegistry.Instance.UseWebMAWBModule, WebSecurityRightsList.WebMAWBView, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("c935b73b-55ce-4508-a99c-490e89adc2fa", "HAWBs"), TrackingConstants.RelativePath.HAWBPage, WebDataRegistry.Instance.UseWebHAWBModule, WebSecurityRightsList.WebHAWBView, SiteUser));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("b6954ace-17aa-4225-91f9-bd9968ec01d1", "Reports"), TrackingConstants.RelativePath.ReportsPage, WebDataRegistry.Instance.UseWebForwardingReportsModule, WebSecurityRightsList.WebReports, SiteUser) { PageParameters = GetReportPageParameters(WebReportModes.Freight) });
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("ba615241-6127-4427-a965-afed0b3e4193", "Flights"), TrackingConstants.RelativePath.FlightSchedulesPage, WebDataRegistry.Instance.UseWebForwardingFlightsModule));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("5de685ab-9d37-48be-be6d-bfd1d9fc8053", "Sailings"), TrackingConstants.RelativePath.SailingSchedulesPage, WebDataRegistry.Instance.UseWebForwardingSailingsModule));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("0fd584d3-53bd-4135-a5ff-8c7253630276", "Road"), TrackingConstants.RelativePath.RoadSchedulesPage, WebDataRegistry.Instance.UseWebForwardingRoadModule));
			forwardingMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("a8b49626-40e0-4ddf-ade6-05b4f5544b12", "Rail"), TrackingConstants.RelativePath.RailSchedulesPage, WebDataRegistry.Instance.UseWebForwardingRailModule));

			bar.NavigationElements.Add(forwardingMenu);

			NavigationElement cfsMenu = new NavigationElement(Res.GetString("B2BBF5D1-CAA1-46C0-8D31-5BA96F797136", "CFS"), "#");
			cfsMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("829f6105-d97e-4c4a-af0c-e70af0de49a5", "Shipments"), TrackingConstants.RelativePath.CFSShipmentsPage, WebDataRegistry.Instance.UseWebCFSShipmentsModule, WebSecurityRightsList.WebCFSShipmentView, SiteUser));
			bar.NavigationElements.Add(cfsMenu);

			if ((Page.SiteUser != null) && (Page.SiteUser.IsLoggedIn) && (Page.SiteUser is TrackingSiteUser) &&
				((TrackingSiteUser)Page.SiteUser).IsForwarder)
			{
				var linerAndAgencyMenu = new NavigationElement(Res.GetString("a4001f80-2c0e-481a-b7c6-bf9aaa64d55a", "Liner & Agency"), "#");
				linerAndAgencyMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("eceb7e8b-0654-43f8-9ab9-54f08aaede4c", "Bookings"), TrackingConstants.RelativePath.LinerAndAgencyBookingsPage, WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule, WebSecurityRightsList.WebLinerAndAgencyBookingsView, SiteUser));
				linerAndAgencyMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("e584ae7b-9564-45c3-b0e5-398eca13be71", "Bills Of Lading"), TrackingConstants.RelativePath.LinerAndAgencyBillsOfLadingPage, WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule, WebSecurityRightsList.WebLinerAndAgencyBillsOfLadingView, SiteUser));
				linerAndAgencyMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("CA601F55-433D-47C9-9363-6B08E5C01418", "Containers"), TrackingConstants.RelativePath.LinerAndAgencyContainersPage, WebDataRegistry.Instance.UseWebLinerAndAgencyContainersModule, WebSecurityRightsList.WebLinerAndAgencyContainers, SiteUser));
				linerAndAgencyMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("b6954ace-17aa-4225-91f9-bd9968ec01d1", "Reports"), TrackingConstants.RelativePath.ReportsPage, WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule, WebSecurityRightsList.WebReports, SiteUser) { PageParameters = GetReportPageParameters(WebReportModes.LinerAgency) });

				bar.NavigationElements.Add(linerAndAgencyMenu);
			}

			if ((Page.SiteUser != null) && (Page.SiteUser.IsLoggedIn) && (Page.SiteUser is TrackingSiteUser))
			{
				NavigationElement customsMenu = new NavigationElement(Res.GetString("dcd66122-5f7e-450b-ba88-cc181a329c3e", "Customs"), "#");
				customsMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("91d02277-0685-40c0-91f4-d29e1c54e566", "Declarations"), TrackingConstants.RelativePath.DeclarationModulePage, WebDataRegistry.Instance.UseWebDeclarationModule, WebSecurityRightsList.WebDeclarationView, SiteUser));
				customsMenu.SubMenuItems.Add(new NavigationElement("ISF", TrackingConstants.RelativePath.ISFPage, WebDataRegistry.Instance.UseWebISFModule, WebSecurityRightsList.WebISFView, SiteUser));
				customsMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("b6954ace-17aa-4225-91f9-bd9968ec01d1", "Reports"), TrackingConstants.RelativePath.ReportsPage, WebDataRegistry.Instance.UseWebCustomsReportsModule, WebSecurityRightsList.WebReports, SiteUser) { PageParameters = GetReportPageParameters(WebReportModes.Customs) });
				bar.NavigationElements.Add(customsMenu);
			}

			if ((Page.SiteUser != null) && (Page.SiteUser.IsLoggedIn) && (Page.SiteUser is TrackingSiteUser) && ((TrackingSiteUser)Page.SiteUser).IsWarehouseClient)
			{
				NavigationElement warehouseMenu = new NavigationElement(Res.GetString("d0b7bda4-1bfb-4cc5-a11b-c3dff52501f6", "Warehouse"), "#");
				warehouseMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("4142353f-2d8d-4c73-8446-be9b13f777a6", "Inventory"), TrackingConstants.RelativePath.InventoryPage, WebDataRegistry.Instance.UseWebWarehouseInventoryModule, WebSecurityRightsList.WebInventoryView, SiteUser));
				warehouseMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("114c19f6-8335-4c38-a1d4-f1ec43ff2b6b", "Orders"), TrackingConstants.RelativePath.WarehouseOrdersPage, WebDataRegistry.Instance.UseWebWarehouseOrdersModule, WebSecurityRightsList.WebWarehouseOrdersView, SiteUser));
				warehouseMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("eb4c5107-2e10-4e8a-b492-adb497d4c461", "Receipts"), TrackingConstants.RelativePath.WarehouseReceiptsPage, WebDataRegistry.Instance.UseWebWarehouseReceiptsModule, WebSecurityRightsList.WebWarehouseReceiptsView, SiteUser));
				warehouseMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("fe715fae-3429-4239-8b92-f8434b02665c", "Products"), TrackingConstants.RelativePath.ProductProfilesPage, WebDataRegistry.Instance.UseWebWarehouseProductsModule, WebSecurityRightsList.WebWarehouseProductsView, SiteUser));
				warehouseMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("b6954ace-17aa-4225-91f9-bd9968ec01d1", "Reports"), TrackingConstants.RelativePath.ReportsPage, WebDataRegistry.Instance.UseWebWarehouseReportsModule, WebSecurityRightsList.WebReports, SiteUser) { PageParameters = GetReportPageParameters(WebReportModes.Warehouse) });

				bar.NavigationElements.Add(warehouseMenu);
			}

			if ((Page.SiteUser != null) && (Page.SiteUser.IsLoggedIn) && (Page.SiteUser is TrackingSiteUser))
			{
				NavigationElement transportMenu = new NavigationElement(Res.GetString("c89d39a4-2374-4717-8997-45a37dbceca0", "Port Transport"), "#");
				transportMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("c996d134-e13b-4cd4-899a-73f4f36acba0", "Transport Jobs"), TrackingConstants.RelativePath.CartagePage, WebDataRegistry.Instance.UseWebCartageModule, WebSecurityRightsList.WebCartageView, SiteUser));
				transportMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("b6954ace-17aa-4225-91f9-bd9968ec01d1", "Reports"), TrackingConstants.RelativePath.ReportsPage, WebDataRegistry.Instance.UseWebTransportReportsModule, WebSecurityRightsList.WebReports, SiteUser) { PageParameters = GetReportPageParameters(WebReportModes.Transport) });

				bar.NavigationElements.Add(transportMenu);
			}

			bar.NavigationElements.Add(new NavigationElement(Res.GetString("5ef41145-6439-4e6e-b3e9-0fd7dc3e53c7", "Accounts"), TrackingConstants.RelativePath.TransactionsPage, WebDataRegistry.Instance.UseWebAccountsModule, WebSecurityRightsList.WebInvoicingAndStatements, SiteUser));

			NavigationElement userMenu = new NavigationElement(Res.GetString("21d14988-a99b-48fa-8ddc-0b4870425715", "User"), "#");
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn && ((OrgContactWebUser)Page.SiteUser).AllUserRelatedOrgs.Count > 1)
			{
				userMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("634b2ec7-f76f-4355-bd38-56746f7204b3", "Company"), TrackingConstants.RelativePath.SwitchCompanyPage) { PageParameters = string.Format("ReturnUrl={0}", HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Url.PathAndQuery)) });  // Request parameters
			}
			userMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("ee98ea8c-d973-410b-b300-e8ef8c75f687", "Password"), TrackingConstants.RelativePath.ChangePasswordPage));
			userMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("96219ff0-2915-40d1-a8d7-c5f882236cdf", "Log Off"), TrackingConstants.RelativePath.LogoutPage) { PageParameters = "ClearSaved=1" }); // Request parameter
			userMenu.SubMenuItems.Add(new NavigationElement(Res.GetString("b6954ace-17aa-4225-91f9-bd9968ec01d1", "Reports"), TrackingConstants.RelativePath.ReportsPage, null, WebSecurityRightsList.WebReports, SiteUser) { PageParameters = GetReportPageParameters(WebReportModes.All) });
			bar.NavigationElements.Add(userMenu);

			if ((Page.SiteUser != null) && (Page.SiteUser.IsLoggedIn) && (Page.SiteUser is TrackingSiteUser) && !string.IsNullOrEmpty(WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.Value))
			{
				bar.NavigationElements.Add(new NavigationElement(Res.GetString("be979c62-140a-4978-b77a-e7d14f1a7ad8", "Terms & Conditions"), TrackingConstants.RelativePath.ViewTermsAndConditionsPage));
			}
		}

		#endregion
	}
}
