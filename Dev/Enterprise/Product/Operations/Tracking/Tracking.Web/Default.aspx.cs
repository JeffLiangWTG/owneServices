using System;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for _Default.
	/// </summary>
	public partial class _Default : BasePage
	{
#if DEBUG
		public virtual
#endif
		WebUser CurrentUser
		{
			get { return Page.SiteUser; }
		}

		protected override void OnLoad(EventArgs e)
		{
			string clientPortalUrl = Page.SiteUser.GetClientPortalUrl();

			if (!string.IsNullOrEmpty(clientPortalUrl))
			{
				HttpContext.Current.Response.Redirect(clientPortalUrl);
			}
			else
			{
				string availablePage = GetFirstAvailablePageUrl();
				if (string.IsNullOrEmpty(availablePage) || availablePage.Trim() == "#")
				{
					SecureQueryString qs = new SecureQueryString();
					qs["title"] = Res.GetString("efa38bd8-6e59-44f8-a6d5-625adea684e6", "WebTracker Access Denied");
					qs["message"] = Res.GetString("53adc636-6cc8-45a4-b27f-76c1bc7257b0", "You do not have access to any WebTracker module. This may be due to one or more of the following: security rights, registry settings, or system licensing.");
					qs.ExpireTime = TimeSpan.FromMinutes(10);
					HttpContext.Current.Response.Redirect(AppInstance.ErrorPage + (NoResString)"?data=" + WebUtility.UrlEncode(qs.ToString())); // URL Parameters
				}
				else
				{
					HttpContext.Current.Response.Redirect(availablePage);
				}
			}
		}

#if DEBUG
		public
#endif
 string GetFirstAvailablePageUrl()
		{
			var result = ZString.Empty;
			if (PageAvailable(WebDataRegistry.Instance.UseWebForwardingShipmentsModule, WebSecurityRightsList.WebShipmentsView))
			{
				result = AppInstance.ShipmentsPage;
			}
			else if (PageAvailable(WebDataRegistry.Instance.UseWebForwardingOrdersModule, WebSecurityRightsList.WebOrdersView))
			{
				result = AppInstance.OrdersPage;
			}
			else if (PageAvailable(WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule, WebSecurityRightsList.WebLinerAndAgencyBookingsView))
			{
				result = AppInstance.LinerAndAgencyBookingsPage;
			}
			else if (PageAvailable(WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule, WebSecurityRightsList.WebLinerAndAgencyBillsOfLadingView))
			{
				result = AppInstance.LinerAndAgencyBillsOfLadingPage;
			}
			else if (PageAvailable(WebDataRegistry.Instance.UseWebDeclarationModule, WebSecurityRightsList.WebDeclarationView))
			{
				result = AppInstance.DeclarationModulePage;
			}
			else if (PageAvailable(WebDataRegistry.Instance.UseWebWarehouseInventoryModule, WebSecurityRightsList.WebInventoryView) &&
				Page.SiteUser.IsWarehouseClient)
			{
				result = AppInstance.InventoryPage;
			}
			else if (PageAvailable(WebDataRegistry.Instance.UseWebWarehouseOrdersModule, WebSecurityRightsList.WebWarehouseOrdersView) &&
				Page.SiteUser.IsWarehouseClient)
			{
				result = AppInstance.WarehouseOrders;
			}
			else
			{
				if (NavigationBar != null)
				{
					result = GetFirstNonEmptyNavigationURL(NavigationBar.NavigationElements);
				}
			}

			return result;
		}

		string GetFirstNonEmptyNavigationURL(NavigationElementCollection navigationElements)
		{
			string result = "";
			foreach (NavigationElement navigation in navigationElements)
			{
				if (navigation.Visible)
				{
					result = navigation.URL;
					if (!IsURLEmpty(result))
					{
						break;
					}
					result = GetFirstNonEmptyNavigationURL(navigation.SubMenuItems);
					if (!IsURLEmpty(result))
					{
						break;
					}
				}
			}
			return result;
		}

		bool IsURLEmpty(string uRL)
		{
			return string.IsNullOrEmpty(uRL) || uRL == AppInstance.ApplicationRoot + "#" || uRL == "#";
		}

#if DEBUG
		public
#endif
 bool PageAvailable(BooleanRegistryItem registryItem, WebSecurityRight securityRight)
		{
			return RegistryItemIsActivated(registryItem) &&
				(CurrentUser == null || CurrentUser.AreSecurityRightsGranted(securityRight));
		}

#if DEBUG
		public
#endif
 bool PageAvailable(BooleanRegistryItem registryItem, WebSecurityRight securityRight, LicenceCheckpoint licence)
		{
			return RegistryItemIsActivated(registryItem) &&
				(CurrentUser == null || CurrentUser.AreSecurityRightsGranted(securityRight));
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.Default;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.SystemDefaultPage;
		}

#if DEBUG
		public
#endif
 bool RegistryItemIsActivated(BooleanRegistryItem registryItem)
		{
			return registryItem != null && registryItem.Value;
		}
	}
}
