using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Security;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Base
{
	public class ZWebController
	{
		public ZWebController(HttpContext context)
			: this(new HttpContextWrapper(context))
		{
		}

		public ZWebController(HttpContextBase context)
		{
			this.Context = context ?? throw new ArgumentNullException(nameof(context));
		}

		#region Properties

		HttpContextBase Context { get; set; }

		Global AppInstance
		{
			get { return (Global)Context.ApplicationInstance; }
		}

		bool RequiresPassword
		{
			get { return WebDataRegistry.Instance.WebTrackerAutoLoginRequiresPassword.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		#endregion

		#region RedirectToTrackingPage

		#region Public

		public void RedirectToTrackingPage(ZGuid contactPK, TrackingConstants.BusinessContext businessContext, ZGuid businessContextPK, ZGuid[] additionalRefs, bool requireLogin = false, string queryStringData = null)
		{
			RedirectToTrackingPageCore(contactPK, GetTargetUrl(businessContext, businessContextPK, additionalRefs), requireLogin, queryStringData);
		}

		public void RedirectToTrackingPage(ZGuid contactPK, TrackingConstants.BusinessContext businessContext, ZString businessContextNK, bool requireLogin = false, string queryStringData = null)
		{
			RedirectToTrackingPageCore(contactPK, GetTargetUrl(businessContext, businessContextNK), requireLogin, queryStringData);
		}

		public void RedirectToTrackingPage(BusinessObject obj, WebModuleID webModuleID)
		{
			var id = (WebModuleId)webModuleID.ID;
			var url = GetTargetUrl(id.GetContext(), obj.PK.ToStringKey());
			RedirectToTrackingPageCore(url);
		}

		#endregion

		#region RedirectToTrackingPageCore

#if DEBUG
		protected virtual
#endif
		void RedirectToTrackingPageCore(string targetUrl)
		{
			Context.Response.Redirect(targetUrl);
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Needs too much refactoring")]
#if DEBUG
		public
#endif
		void RedirectToTrackingPageCore(ZGuid contactPK, string targetUrl, bool requireLogin = false, string queryStringData = null)
		{
			var server = Context.Server;

			if (string.IsNullOrEmpty(targetUrl) || server == null)
			{
				return;
			}

			var appInstance = (Global)Context.ApplicationInstance;
			var loginUrl = string.Format("{0}?ReturnUrl={1}", FormsAuthentication.LoginUrl, server.UrlEncode(targetUrl)); // Non-semantic text
			var siteUser = appInstance?.SiteUser;
			var webOrgCode = GlbCompany.CurrentCompany?.OrgProxy?.OH_Code;
			if (siteUser == null || string.IsNullOrEmpty(webOrgCode))
			{
				RedirectToTrackingPageCore(loginUrl);
				return;
			}

			var contact = Factory.Load<OrgContact>(contactPK);
			var haveFullContactInfo = contact != null && contact.Header != null;
			var showLogin = requireLogin && !haveFullContactInfo;
			var orgCode = haveFullContactInfo ? contact.Header.OH_Code : webOrgCode;
			var userName = haveFullContactInfo ? contact.OC_Email : (ZString)User.WebUserName;
			if (showLogin || RequiresPassword)
			{
				if (!showLogin)
				{
					appInstance.ApplicationCookie.WriteUser(orgCode, userName, "");
				}

				RedirectToTrackingPageCore(loginUrl);
			}
			else
			{
				if (!siteUser.IsLoggedIn)
				{
					siteUser.Login(orgCode, userName, User.WebTransientPassword);
					FormsAuthentication.SetAuthCookie(userName, false);
				}
				else
				{
					var session = appInstance.Session;
					if (session != null)
					{
						session[Global.AutoLoginQueryStringDataIndexer] = queryStringData;
					}
				}

				RedirectToTrackingPageCore(targetUrl);
			}
		}

		#endregion

		#endregion

		#region GetTargetUrl

		#region Public

		public string GetTargetUrl(TrackingConstants.BusinessContext businessContext, ZGuid businessContextPK, ZGuid[] additionalRefs)
		{
			var webModuleOption = businessContext.GetModuleOption() ?? WebModuleOptions.Ref;
			return GetTargetUrlCore(businessContext.GetModuleId(), webModuleOption, businessContextPK.ToString(), additionalRefs); // Non-semantic text
		}

		public string GetTargetUrl(TrackingConstants.BusinessContext businessContext, ZString businessContextNK)
		{
			var webModuleOption = businessContext.GetModuleOption() ?? WebModuleOptions.Number;
			return GetTargetUrlCore(businessContext.GetModuleId(), webModuleOption, businessContextNK); // Non-semantic text
		}

		public string GetTargetUrl(WebModuleID webModuleID, ZString businessContextID, params ZGuid[] additionalRefs)
		{
			return GetTargetUrlCore(webModuleID, WebModuleOptions.Ref, businessContextID, additionalRefs); // Non-semantic text
		}

		#endregion

		#region GetUrlCore

#if DEBUG
		public
#endif
		string GetTargetUrlCore(WebModuleID webModuleID, ZString webModuleOption, ZString businessContextID, params ZGuid[] additionalRefs)
		{
			var webId = (WebModuleId)webModuleID.ID;
			return GetTargetUrlCore(webId, webModuleOption, businessContextID, additionalRefs);
		}

#if DEBUG
		public
#endif
		string GetTargetUrlCore(WebModuleId webModuleId, ZString webModuleOption, ZString businessContextID, params ZGuid[] additionalRefs)
		{
			var targetPage = string.Empty;
			var parameters = string.Empty;
			var hasParameters = !webModuleOption.IsEmpty && !businessContextID.IsEmpty;

			if (hasParameters)
			{
				parameters = string.Format("{0}={1}", webModuleOption, businessContextID); // Non-semantic text
			}

			switch (webModuleId)
			{
				case WebModuleId.TrackingDefault:
					targetPage = AppInstance.DefaultPage;
					break;

				case WebModuleId.TrackingShipments:
					targetPage = hasParameters ? AppInstance.ShipmentPage : AppInstance.ShipmentsPage;
					parameters = string.Format((NoResString)"{0}{1}Table={2}", parameters, string.IsNullOrEmpty(parameters) ? string.Empty : "&", JobShipmentSchema.Constants.TableName); // Non-semantic text
					break;

				case WebModuleId.TrackingDeclarations:
					targetPage = hasParameters ? AppInstance.DeclarationDetailsPage : AppInstance.DeclarationModulePage;
					break;

				case WebModuleId.TrackingImporterSecurityFiling:
					targetPage = hasParameters ? AppInstance.ISFDetailsPage : AppInstance.ISF;
					break;

				case WebModuleId.TrackingBookings:
					targetPage = hasParameters ? AppInstance.BookingDetailsPage : AppInstance.BookingsPage;
					break;

				case WebModuleId.TrackingOrders:
					targetPage = hasParameters ? AppInstance.OrderDetailsPage : AppInstance.OrdersPage;
					break;

				case WebModuleId.TrackingAccounts:
					parameters = string.Empty;
					if (webModuleOption == WebModuleOptions.Ref) // Non-semantic text
					{
						targetPage = InvoiceRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(businessContextID));
					}
					else if (!hasParameters)
					{
						targetPage = AppInstance.TransactionsPage;
					}
					break;

				case WebModuleId.TrackingWarehouseOrders:
					targetPage = hasParameters ? AppInstance.WarehouseOrderDetailsPage : AppInstance.WarehouseOrders;
					break;

				case WebModuleId.eDoc:
					if (webModuleOption == WebModuleOptions.Ref && additionalRefs != null && additionalRefs.Length == 1) // Non-semantic text
					{
						parameters = string.Empty;
						targetPage = eDocsRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(businessContextID), additionalRefs[0]);
					}
					break;

				case WebModuleId.TrackingQuotations:
					parameters = string.Empty;
					if (webModuleOption == WebModuleOptions.Ref)
					{
						targetPage = QuoteRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(businessContextID));
					}
					else if (webModuleOption == WebModuleOptions.TrackingQuotations.ClientAccepts)
					{
						targetPage = QuoteClientReplyRequestHandler.RequestHelper.GetHandlerUrlForClientAcceptance(new ZGuid(businessContextID));
					}
					else if (webModuleOption == WebModuleOptions.TrackingQuotations.ClientNotAccepts)
					{
						targetPage = QuoteClientReplyRequestHandler.RequestHelper.GetHandlerUrlForClientNonAcceptance(new ZGuid(businessContextID));
					}
					else if (!hasParameters)
					{
						targetPage = AppInstance.QuotationsPage;
					}
					break;

				case WebModuleId.TrackingHousebill:
					parameters = string.Empty;
					if (webModuleOption == WebModuleOptions.Ref) // Non-semantic text
					{
						targetPage = HouseBillRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(businessContextID));
					}
					break;

				case WebModuleId.TrackingFreightLabel:
					parameters = string.Empty;
					if (webModuleOption == WebModuleOptions.Ref) // Non-semantic text
					{
						targetPage = FreightLabelRequestHandler.RequestHelper.GetHandlerUrl(new ZGuid(businessContextID));
					}
					break;
			}

			if (!string.IsNullOrEmpty(targetPage) && !string.IsNullOrEmpty(parameters))
			{
				targetPage = string.Format("{0}?{1}", targetPage, parameters); // Non-semantic text
			}

			return targetPage;
		}

		#endregion

		#endregion

		#region Factory

		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		#endregion
	}
}
