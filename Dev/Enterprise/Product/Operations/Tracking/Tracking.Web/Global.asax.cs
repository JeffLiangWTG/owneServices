using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Interface to access Licence Usage Log writing functionality that write per Session only one log record for every LicenceCheckpoint 
	/// </summary>
	public interface ILicenceUsageLogWriter
	{
		void WriteLicenceUsageLog(params ILicenceCheckpoint[] checkpoints);
	}

	/// <summary>
	/// Global settings for the Application
	/// </summary>
	public class Global : ZGlobal, ILicenceUsageLogWriter
	{
		#region Path constants

		public override string DefaultPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.SystemDefaultPage; }
		}

		public string AutoLoginRequestHandler
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.AutoLoginRequestHandler; }
		}

		public virtual string UrlForRegistryItems
		{
			get { return HttpContext.Current.Request.Url.Host; }
		}

		#region Specific pages

		#region General

		public override string LoginPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LoginPage; }
		}

		public string LoginSupersededPage => ApplicationRoot + TrackingConstants.RelativePath.LoginSupersededPage;

		public string ResetMasterPasswordPage => ApplicationRoot + TrackingConstants.RelativePath.ResetMasterPasswordPage;

		public string SetMasterPasswordPage => ApplicationRoot + TrackingConstants.RelativePath.SetMasterPasswordPage;

		public string LoginRedirectionPage => ApplicationRoot + TrackingConstants.RelativePath.LoginRedirectionPage;

		public string LoginCompletePage => ApplicationRoot + TrackingConstants.RelativePath.LoginCompletePage;

		public string LogoutPage => ApplicationRoot + TrackingConstants.RelativePath.LogoutPage;

		public string ForgotPasswordPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ForgotPasswordPage; }
		}

		public string ResetPasswordPage => ApplicationRoot + TrackingConstants.RelativePath.ResetPasswordPage;

		public string SetPasswordPage => ApplicationRoot + TrackingConstants.RelativePath.SetPasswordPage;

		public string SwitchCompanyPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.SwitchCompanyPage; }
		}

		#endregion

		#region Orders

		public string OrdersPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.OrdersPage; }
		}

		public string OrderDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.OrderDetailsPage; }
		}

		public string EditOrderPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditOrderPage; }
		}
		#endregion

		#region Declarations

		public string DeclarationModulePage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.DeclarationModulePage; }
		}

		public string DeclarationDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.DeclarationDetailsPage; }
		}

		#endregion

		#region CFSShipments

		public string CFSShipmentsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.CFSShipmentsPage; }
		}

		public string CFSShipmentDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.CFSShipmentDetailsPage; }
		}

		#endregion

		#region Shipments / Declarations

		public string ShipmentsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ShipmentsPage; }
		}

		public string ShipmentPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ShipmentPage; }
		}

		public string ShipmentDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ShipmentDetailsPage; }
		}

		#endregion

		#region EDIMessage

		public string EDIMessagesPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EDIMessagesPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string EDIMessagePopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=640, width=480";

		#endregion

		#region MAWB

		public string MAWBDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.MAWBDetailsPage; }
		}

		public string MAWBUploadPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.MAWBUploadPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string MAWBUploadPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=700, width=640";

		#endregion

		#region HAWB

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string HAWBDetailsPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=600, width=950";

		public string HAWBListPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.HAWBListPage; }
		}

		#endregion

		#region Rating

		public string QuotationsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.QuotationsPage; }
		}

		public string QuotationPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.QuotationPage; }
		}

		#endregion

		#region Reports

		public string ReportsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ReportsPage; }
		}

		#endregion

		#region Schedules

		public string FlightSchedulesPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.FlightSchedulesPage; }
		}

		public string SailingSchedulesPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.SailingSchedulesPage; }
		}

		public string RoadSchedulesPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.RoadSchedulesPage; }
		}

		public string RailSchedulesPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.RailSchedulesPage; }
		}

		#endregion

		#region Containers

		public string ContainersPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ContainersPage; }
		}

		public string ContainerBatchUpdatePage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ContainerBatchUpdatePage; }
		}

		public string ContainerSummaryPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ContainerSummaryPage; }
		}

		public string ContainerDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ContainerDetailsPage; }
		}

		public string EditContainerPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditContainerPage; }
		}

		#endregion

		#region LinerAndAgencyContainers

		public string LinerAndAgencyContainersPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyContainersPage; }
		}

		public string LinerAndAgencyContainerDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyContainerDetailsPage; }
		}

		public string EditLinerAndAgencyContainerPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditLinerAndAgencyContainerPage; }
		}

		#endregion

		#region Accounts

		public string TransactionsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.TransactionsPage; }
		}

		#endregion Accounts

		#region Bookings

		public string BookingsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.BookingsPage; }
		}

		public string EditBookingPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditBookingPage; }
		}

		public string BookingDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.BookingDetailsPage; }
		}

		public string TermsAndConditionsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.TermsAndConditionsPage; }
		}

		public string LinesMappingPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinesMappingPage; }
		}

		public string PreloadPage => ApplicationRoot + TrackingConstants.RelativePath.PreloadPage;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string LinesMappingPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=700, width=1500";

		public const string TermsAndConditionsRedirectUrlTag = "RedirectUrl";
		public const string BookingTermsAndConditionsIndexer = "BookingTermsAndConditions";
		public const string AutoLoginQueryStringDataIndexer = "AutoLoginQueryStringData";

		#endregion

		#region Liner & Agency

		#region Booking

		public string LinerAndAgencyBookingsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyBookingsPage; }
		}

		public string LinerAndAgencyBookingDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyBookingDetailsPage; }
		}

		public string LinerAndAgencyEditBookingPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyEditBookingPage; }
		}

		#endregion

		#region Bill Of Lading

		public string LinerAndAgencyBillsOfLadingPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyBillsOfLadingPage; }
		}

		public string LinerAndAgencyBillOfLadingDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyBillOfLadingDetailsPage; }
		}

		public string LinerAndAgencyEditForwardingInstructionPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.LinerAndAgencyEditForwardingInstructionPage; }
		}

		#endregion

		#endregion

		#region ISF

		public string ISF
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ISFPage; }
		}

		public string EditISFPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditISFPage; }
		}

		public string ISFDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ISFDetailsPage; }
		}

		#endregion

		#region Transport

		public string CartagePage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.CartagePage; }
		}

		public string CartageDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.CartageDetailsPage; }
		}

		#endregion

		#region Warehousing

		public string InventoryPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.InventoryPage; }
		}

		public string InventoryDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.InventoryDetailsPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string InventoryDetailsPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=560, width=850";

		public string WarehouseOrders
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.WarehouseOrdersPage; }
		}

		public string WarehouseOrderDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.WarehouseOrderDetailsPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string WarehouseOrderDetailsPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=480, width=800";

		public string WarehouseEditOrders
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditWarehouseOrderPage; }
		}

		public string WarehouseReceipts
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.WarehouseReceiptsPage; }
		}

		public string WarehouseEditReceive
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.EditWarehouseReceivePage; }
		}

		public string WarehouseReceiveDetails
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.WarehouseReceiveDetailsPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string WarehouseReceiveDetailsPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=1,resizable=1,height=480, width=640";

		public string ProductProfiles
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ProductProfilesPage; }
		}

		public string ProductProfileDetailsPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ProductProfileDetailsPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string ProductProfilePopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=0,resizable=1,height=400, width=640";

		public string WarehouseOrderLineAllocationPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.WarehouseOrderLineAllocationPage; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string WarehouseOrderLineAllocationEditPopupWindowStyle = "location=0,toolbar=0,menubar=0,location=0,directories=0,status=0,scrollbars=0,resizable=1,height=400, width=850";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string WarehouseOrderLineAllocationPopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=0,resizable=1,height=300, width=400";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Popup Window Style")]
		public const string ProductImagePopupWindowStyle = "location=0,toolbar=0,menubar=0,directories=0,status=0,scrollbars=0,resizable=1,height=610, width=460";

		#endregion

		#region Administration

		public string ChangePasswordPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.ChangePasswordPage; }
		}

		#endregion Administration

		#region CustomsExchangeRates

		public string CustomsExchangeRatesPage
		{
			get { return ApplicationRoot + TrackingConstants.RelativePath.CustomsExchangeRatesPage; }
		}

		#endregion CustomsExchangeRates

		public string GlowRedirectPage => ApplicationRoot + TrackingConstants.RelativePath.GlowRedirectPage;

		#endregion

		#endregion

		/// <summary>
		/// Required designer variable.
		/// </summary>
		#if DEBUG
				internal
		#endif
		System.ComponentModel.IContainer components;

		public Global()
		{
			InitializeComponent();
		}

		protected void Application_PreSendRequestHeaders()
		{
			Headers.Set((NoResString)"Server", (NoResString)"Web Server");
		}

		protected virtual NameValueCollection Headers => Response.Headers;

		protected override IWebAccessManager GetNewWebAccessManager()
		{
			return new TrackingWebAccessManager(this);
		}

		protected override bool IsApplicationUserInteractive => true;

		protected override EnvProvider WebEnvProvider => lazyEnvProvider.Value;

		readonly Lazy<EnvProvider> lazyEnvProvider = new Lazy<EnvProvider>(() => new WebEnvironmentProvider(() => new SessionUserContextManager()));

		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			InitializeTypeSubstitutions();
		}

#if DEBUG
		public
#else
			protected
#endif
 override void PopulateWebRegistry(string url)
		{
			base.PopulateWebRegistry(url);

			var urls = WebDataRegistry.Instance.WebTrackerUrls.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
			if (!WebDataRegistry.ContainsWebUrl(urls, url))
			{
				// Populate WebTrackerUrls
				urls.Add(url);
				WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, urls.ToArray());

				// Populate WebTrackerTheme
				var theme = WebDataRegistry.Instance.OldTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				if (string.IsNullOrEmpty(theme))
				{
					theme = ThemeCodeDescriptionPairList.Codes.CUS;
				}
				var themeObjects = WebDataRegistry.Instance.WebTrackerTheme.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
				if (!themeObjects.Any(x => x.Url == url && x.Code != theme))
				{
					themeObjects.Add(new WebTrackerTheme(url, theme));
					WebDataRegistry.Instance.WebTrackerTheme.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, themeObjects.ToArray());
				}

				if (theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					// Populate WebTrackerCustomImages for images that are not present for All URLs
					var imageObjects = WebDataRegistry.Instance.WebTrackerCustomImages.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
					if (!imageObjects.Any(x => x.Url == url))
					{
						var overridenImages = imageObjects.Where(x => string.IsNullOrEmpty(x.Url)).Select(x => x.Name).ToArray();
						var imagesFolder = Path.Combine(ApplicationRoot, (NoResString)"Images");// directory name
						var imagePaths = new DirectoryInfo(MapPath(imagesFolder)).GetFiles()
							.Where(x => WebTrackerCustomImage.IsSupportedFileType(x.Extension)).Select(x => x.FullName);

						foreach (var imagePath in imagePaths)
						{
							var imageName = Path.GetFileName(imagePath);
							if (!overridenImages.Contains(imageName))
							{
								try
								{
									var imageData = File.ReadAllBytes(imagePath);
									imageObjects.Add(new WebTrackerCustomImage(imageName, url, imageData));
								}
								catch (UnauthorizedAccessException)
								{
									//supress error throw
								}
							}
						}
						WebDataRegistry.Instance.WebTrackerCustomImages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, imageObjects.ToArray());
					}

					// Populate WebTrackerCustomCss
					var cssObjects = WebDataRegistry.Instance.WebTrackerCustomCss.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).ToList();
					if (!cssObjects.Any(x => x.Url == url))
					{
						cssObjects.Add(new WebTrackerCustomCss(url, File.ReadAllText(MapPath(BaseStyleSheet))));
						WebDataRegistry.Instance.WebTrackerCustomCss.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cssObjects.ToArray());
					}
				}
			}
		}

#if DEBUG
		internal
#endif
 void InitializeTypeSubstitutions()
		{
			TypeDecider.AddSubstitution(typeof(Freight.Forwarding.Business.ForwardingShipment), typeof(TrackingShipment));
			TypeDecider.AddSubstitution(typeof(Freight.Forwarding.Business.ForwardingShipmentProcessTask), typeof(TrackingShipmentProcessTask));

			TypeDecider.AddSubstitution(typeof(Freight.Forwarding.Orders.Business.Order), typeof(TrackingOrder));
			TypeDecider.AddSubstitution(typeof(Freight.Forwarding.Orders.Business.OrderProcessTasks), typeof(TrackingOrderProcessTasks));

			TypeDecider.AddSubstitution(typeof(Freight.Forwarding.AWB.Business.ExportAWBHeader), typeof(TrackingMAWBHeader));

			TypeDecider.AddSubstitution(typeof(Freight.Agency.Business.BillOfLading), typeof(TrackingBillOfLading));
			TypeDecider.AddSubstitution(typeof(Freight.Agency.Business.AgencyBooking), typeof(TrackingLinerAndAgencyBooking));
		}

		public override WebUser GetNewSiteUser()
		{
			return new TrackingSiteUser();
		}

		protected override string ApplicationCookieName
		{
			get { return TrackingConstants.ApplicationCookieName; }
		}

		public override string BaseStyleSheet
		{
			get
			{
				var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, UrlForRegistryItems);
				if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					return base.BaseStyleSheet;
				}
				else
				{
					return ApplicationRoot + string.Format((NoResString)"App_Themes/{0}/BaseStyle.css", GetThemeName(theme));// Is a file name
				}
			}
		}

		public override string LogoImage
		{
			get
			{
				var theme = WebDataRegistry.Instance.GetWebTheme(WebDataRegistry.Instance.WebTrackerTheme, UrlForRegistryItems);
				if (theme == null || theme == ThemeCodeDescriptionPairList.Codes.CUS)
				{
					return base.LogoImage;
				}
				else
				{
					return ApplicationRoot + string.Format((NoResString)"App_Themes/{0}/Images/Logo.gif", GetThemeName(theme));// Is a file name
				}
			}
		}

		string GetThemeName(string themeCode)
		{
			ThemeCodeDescriptionPairList list = new ThemeCodeDescriptionPairList();
			return list.GetMultilingualDescriptionFromCode(themeCode).GetUnresolvedString();
		}

		protected override ZGlobalConfig GetNewGlobalConfig()
		{
			return new TrackingGlobalConfig();
		}

		protected override void Application_Error(object sender, EventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (((TrackingSiteUser)SiteUser)?.IsShipmentQuickViewUser == true)
				{
					SignOut(false);
				}

				base.Application_Error(sender, e);
			}
		}

		#region ILicenceUsageLogWriter

		bool IsLicenceCheckpointLogged(ILicenceCheckpoint licence)
		{
			return HttpContext.Current.Session[LoggedLicencePrefix + licence.Name] != null;
		}

		void MarkLicenceCheckpointAsLogged(ILicenceCheckpoint licence)
		{
			HttpContext.Current.Session[LoggedLicencePrefix + licence.Name] = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "logged licence prefix")]
		const string LoggedLicencePrefix = "Licence";

		void ILicenceUsageLogWriter.WriteLicenceUsageLog(params ILicenceCheckpoint[] checkpoints)
		{
			if (checkpoints.Length > 0 && !SiteUser.IsSuperUser)
			{
				ILicenceConsumptionLogCreator logger = null;

				foreach (ILicenceCheckpoint checkpoint in checkpoints)
				{
					if (!IsLicenceCheckpointLogged(checkpoint))
					{
						if (logger == null)
						{
							logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
						}

						try
						{
							logger.CreateLog(checkpoint);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperException(new ApplicationException("WriteLicenseUsageLog Error", e));
						}
						MarkLicenceCheckpointAsLogged(checkpoint);
					}
				}
			}
		}

		#endregion

		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
