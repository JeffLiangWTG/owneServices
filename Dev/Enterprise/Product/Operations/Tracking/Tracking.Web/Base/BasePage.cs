using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Web;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Base;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Page we inherit all web pages from
	/// </summary>
	public class BasePage : ZAjaxPage
	{
		#region Page setup

		protected new BasePage Page
		{
			get { return base.Page as BasePage; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			AutoRedirectOnSessionExpired();
		}

		public void AutoRedirectOnSessionExpired()
		{
			int timeout = Convert.ToInt32(TimeSpan.FromMinutes(this.Session.Timeout).TotalMilliseconds);
			string script = string.Format(@"
			<script language='javascript'> 
				var intervalset = window.setInterval('Redirect()',{0});
				function Redirect()
				{{
						if (window.location.href.toLowerCase().indexOf('login.aspx') < 0)
						{{
							window.clearInterval(intervalset);
							alert('{1}');
							window.parent.location.replace('{2}');
						}}
				}}
			</script>", timeout.ToString(), Res.GetString("96a003e1-5248-4c34-bbda-988743ea9e40", "Your session has expired due to inactivity."), AppInstance.LogoutPage); // Javascript segment
			Page.ZClientScript.RegisterClientScriptBlock(GetType(), (NoResString)"Redirect", script); // Javascript segment
		}

		public new Global AppInstance
		{
			get { return base.AppInstance as Global; }
		}

		protected override string PageHeaderControlPath
		{
			get { return !IsShownInPopup ? "Base/PageHeaderWithNavigation.ascx" : string.Empty; } // Do not show page header when page is shown in popup
		}

		protected ZWebController WebController
		{
			get
			{
				if (fWebController == null && Context != null)
				{
					fWebController = new ZWebController(this.Context);
				}
				return fWebController;
			}
		}
		ZWebController fWebController;

		protected void LogOff()
		{
			FormsAuthentication.SignOut();

			if (AppInstance.ApplicationCookie.CookieExist())
			{
				AppInstance.ApplicationCookie.Remove();
			}

			Session.Abandon();

			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				SiteUser.Logout();
			}
		}

		#endregion

		#region Navigation Bar

		public ZNavigationBar NavigationBar
		{
			get
			{
				return NavigationBarFinder(this.Controls);
			}
		}

		ZNavigationBar NavigationBarFinder(ControlCollection controls)
		{
			ZNavigationBar result = null;
			foreach (Control childControl in controls)
			{
				if (result != null)
				{
					break;
				}
				if (childControl is ZNavigationBar)
				{
					result = (ZNavigationBar)childControl;
				}
				else
				{
					result = NavigationBarFinder(childControl.Controls);
				}
			}
			return result;
		}

		#endregion

		#region Services

		public void RegisterFreightUtilitiesService()
		{
			if (!this.AJAX.ScriptManager.Services.Contains(FreightUtilitiesWebService))
			{
				this.AJAX.ScriptManager.Services.Add(FreightUtilitiesWebService);
			}
		}

		protected ServiceReference FreightUtilitiesWebService
		{
			get
			{
				return freightUtilitiesWebService ?? (freightUtilitiesWebService = new ServiceReference(AppInstance.ApplicationRoot + "WebService/FreightUtilitiesService.asmx"));
			}
		}

		ServiceReference freightUtilitiesWebService;

		#endregion

		#region Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Can't hit DB on that one. Potentially DB may not be there.")]
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
#if DEBUG
			if (!Globals.IsTest)
#endif
			{
				Response.Cache.SetExpires(DateTime.Now);// Can't hit DB on that one. Potentially DB may not be there.
			}

			LogModuleChange();
		}

		public new TrackingSiteUser SiteUser
		{
			get { return base.SiteUser as TrackingSiteUser; }
		}

		public bool ShowNavigation
		{
			get
			{
				bool result = false;
				if (SiteUser == null || !SiteUser.IsShipmentQuickViewUser) // We use it to enable direct view of SHipment Detalils by Housebill Number
				{
					result = PageRequiresNavigation(Context.Request.Url);
				}
				return result;
			}
		}

		bool PageRequiresNavigation(Uri url)
		{
			return !(
					url.LocalPath.EndsWith(AppInstance.LoginPage, StringComparison.OrdinalIgnoreCase) ||
					url.LocalPath.EndsWith(AppInstance.ForgotPasswordPage, StringComparison.OrdinalIgnoreCase) ||
					url.LocalPath.EndsWith(AppInstance.ErrorPage, StringComparison.OrdinalIgnoreCase) ||
					url.LocalPath.EndsWith(AppInstance.TermsAndConditionsPage, StringComparison.OrdinalIgnoreCase) ||
					url.LocalPath.EndsWith(AppInstance.CustomsExchangeRatesPage, StringComparison.OrdinalIgnoreCase)
					);
		}

		protected override bool ShowLoginStatus
		{
			get
			{
				bool result = false;
				if (!IsShownInPopup && (SiteUser == null || !SiteUser.IsShipmentQuickViewUser)) // We use it to enable direct view of Shipment Detalils by Housebill Number
				{
					result = base.ShowLoginStatus;
				}
				return result;
			}
		}

		protected override bool ShowChangePasswordLinkButton
		{
			get { return false; }
		}

		protected override bool ShowLogOffLinkButton
		{
			get { return false; }
		}

		protected override bool PageRequiresLogin(Uri url)
		{
			return !(
				url.LocalPath.EndsWith(AppInstance.LoginPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.ResetPasswordPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.SetPasswordPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.ForgotPasswordPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.ErrorPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.TermsAndConditionsPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.PreloadPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.LoginSupersededPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.ResetMasterPasswordPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.SetMasterPasswordPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.LoginRedirectionPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.LoginCompletePage, StringComparison.OrdinalIgnoreCase)
			) && TrackingPageRequiresLogin(url);
		}

		bool TrackingPageRequiresLogin(Uri url)
		{
			if (SiteUser == null)
			{
				return true;
			}

			return !(
				url.LocalPath.EndsWith(AppInstance.CustomsExchangeRatesPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.SailingSchedulesPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.FlightSchedulesPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.RoadSchedulesPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.RailSchedulesPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.ShipmentDetailsPage, StringComparison.OrdinalIgnoreCase) ||
				url.LocalPath.EndsWith(AppInstance.ContainerDetailsPage, StringComparison.OrdinalIgnoreCase)
			);
		}

		#endregion

		#region ShowInPopup

		protected bool IsShownInPopup
		{
			get { return HttpContext.Current.Request.Params[TrackingConstants.QueryStringKeys.PopupKey] != null; }
		}

		protected string ShowInPopupParam
		{
			get { return IsShownInPopup ? "&" + TrackingConstants.QueryStringKeys.PopupKey + "=Y" : string.Empty; } // May be inside data
		}

		protected virtual bool ShowCloseWindowInPopup
		{
			get { return true; }
		}

		protected virtual bool Cacheable => true;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			VolumeCalculatorGridAddOn = GetNewVolumeCalculatorGirdAddOn();
			Controls.Add(VolumeCalculatorGridAddOn);
			if (IsShownInPopup && ShowCloseWindowInPopup)
			{
				HtmlTable holder = new HtmlTable();
				holder.Align = (NoResString)"right"; // May be an identifier
				HtmlTableRow row = new HtmlTableRow();
				row.Align = (NoResString)"right"; // May be an identifier
				holder.Rows.Add(row);
				HtmlTableCell cell = new HtmlTableCell();
				row.Cells.Add(cell);
				Button closeWindow = new Button();
				closeWindow.Text = Res.GetString("5a7f41ac-db50-49f7-92d5-74eba7b283b0", "Close Window");
				closeWindow.OnClientClick = (NoResString)"javascript: self.close();"; // May be inside data
				cell.Controls.Add(closeWindow);

				FormControl.Controls.AddAt(0, holder);
			}

			var loginStatus = GetLoginStatus();
			if (loginStatus != null && SiteUser.CanAccessGlowTrackingPortal)
			{
				loginStatus.AdditionalContent.Add(GetNewTrackingPortalLink());
			}

			var pageName = this.Page.PageName;
			if (!string.IsNullOrEmpty(pageName))
			{
				var cssClasses = new List<string> { $"{pageName}Body" }; // string interpolation of a CSS class
				var body = (HtmlGenericControl)this.Page.FindControl("DefaultBody");
				if (body != null)
				{
					var currentCssClasses = body.Attributes["class"]; // HTML attribute
					if (!string.IsNullOrEmpty(currentCssClasses))
					{
						cssClasses.Add(currentCssClasses);
					}

					body.Attributes.Add((NoResString)"class", String.Join(" ", cssClasses)); // HTML attribute
				}
			}

			if (!Cacheable)
			{
				HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				HttpContext.Current.Response.Cache.SetNoStore();
			}
		}

		HyperLink GetNewTrackingPortalLink()
		{
			return new HyperLink
			{
				Text = Res.GetString("0B5B8AB8-FDA8-4AE7-AD4C-652FFEA706DF", "Try the NEW Tracking Portal"),
				NavigateUrl = AppInstance.GlowRedirectPage,
				Target = (NoResString)"_blank" // HTML attribute
			};
		}

		protected virtual VolumeCalculatorDataGridAddOn GetNewVolumeCalculatorGirdAddOn()
		{
			return new VolumeCalculatorDataGridAddOn
				(JobPackLinesSchema.Constants.JL_PackageCount,
				JobPackLinesSchema.Constants.JL_Length,
				JobPackLinesSchema.Constants.JL_Width,
				JobPackLinesSchema.Constants.JL_Height,
				JobPackLinesSchema.Constants.JL_UnitOfDimension,
				JobPackLinesSchema.Constants.JL_ActualVolume,
				JobPackLinesSchema.Constants.JL_ActualVolumeUQ);
		}

		#endregion

		#region Log Module Change

		protected virtual void LogModuleChange()
		{
			if (SiteUser != null &&
				!SiteUser.IsShipmentQuickViewUser &&
				!ModuleNameForEventLogging.IsEmpty &&
				SiteUser.CurrentModuleName != ModuleNameForEventLogging)
			{
				var eventLogHelper = new EventLogHelper();
				eventLogHelper.CreateLogForModuleChanged(SiteUser, ModuleNameForEventLogging);
			}
		}

		protected virtual ZString ModuleNameForEventLogging
		{
			get { return string.Empty; }
		}

		protected ZGuid UpdateAutoCreatedLogReferenceAndSaveDataSource(ZGuid indexer, BusinessObject dataSource)
		{
			if (SiteUser != null && dataSource is IStmALogProvider logProvider)
			{
				logProvider.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}

			return SaveDataSourceAndAddEmailNotifier(indexer, dataSource);
		}

		protected ZGuid SaveDataSourceAndAddEmailNotifier(ZGuid indexer, BusinessObject dataSource)
		{
			AddBusinessObjectChangesEmailNotifier(dataSource);

			return SaveDataSource(indexer, dataSource);
		}

		protected override void AddBusinessObjectChangesEmailNotifier(BusinessObject bizO)
		{
			var bizObj = bizO as IBizOChangesEmailNotification;
			if (bizObj != null)
			{
				new BusinessObjectChangesEmailNotifier(bizObj);
			}

			if (bizO is IEventReferenceProvider)
			{
				new BusinessObjectModifiedEventHelper(bizO);
			}
		}

		#endregion

		#region Document Viewer URL formatting

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript HTML")]
		public const string ShipmentQuickViewUserLoginRequest = @"javascript:return confirm('Error: You need to be logged in with a full account to see this information.\n\nPress Ok to Login now ? Cancel to continue');";

		protected string RelatedDocumentViewerURLFormatString
		{
			get { return string.Format((NoResString)"{0}?Ref={{0}}&Doc={{1}}", eDocsRequestHandler.RequestHelper.BaseUrl); } // Its string formater
		}

		protected string DocumentViewerURLFormatString
		{
			get { return String.Format((NoResString)"{0}?Ref={1}&Doc={{0}}", eDocsRequestHandler.RequestHelper.BaseUrl, DocumentParentPK); } // Its string formater
		}

		protected ZGuid DocumentParentPK
		{
			get
			{
				IWebDocumentsSupport item = DataSource as IWebDocumentsSupport;
				return (item != null) ? item.DocParentPK : ZGuid.Empty;
			}
		}

		protected static string LoginToSeeIt
		{
			get { return Res.GetString("20563eb4-373d-4b2e-a680-7373aa08f159", "You need to log in to see this information"); }
		}

		protected eDocAttachPopup eDocsAddNewLink;

		protected void SetupDocumentsGrid(ZDataGrid documentsGrid)
		{
			if (documentsGrid != null)
			{
				if (documentsGrid is ZGrid)
				{
					var zGrid = ((ZGrid)documentsGrid);
					zGrid.ColumnProvider = new DocumentColumnProvider();

					var controls = zGrid.Container.Controls;
					if (!controls.Contains(eDocsAddNewLink))
					{
						eDocsAddNewLink = new eDocAttachPopup();
						eDocsAddNewLink.ID = "eDocsAddNewLink";
						eDocsAddNewLink.AutoPostBack = true;
						controls.Add(eDocsAddNewLink);
					}
				}
				else
				{
					List<DataGridColumn> columns = new List<DataGridColumn>();
					columns.Add(new ZDateTimeColumn(Res.GetString("a0360045-3ca7-4f69-b576-6ea3897ae1b6", "Date"), DocumentView.Schema.DateReceived) { DateTimeFormat = ZDateTimePickerFormat.Short });
					columns.Add(new ZTextEditColumn(Res.GetString("30f766b6-4fe1-4a65-a9e8-f467e9e530a6", "Description"), DocumentView.Schema.Description));
					columns.Add(new ZTextEditColumn(Res.GetString("10ffd858-3528-4b7e-bd18-83e47a64f3f2", "Type"), DocumentView.Schema.RT_Desc));
					columns.Add(SiteUser.IsShipmentQuickViewUser
													? new ZHyperLinkColumn("", "")
													{
														DataNavigateUrlFormatString = HttpContext.Current.Request.Url.ToString(),
														DataNavigateUrlFields = Array.Empty<string>(),
														Text = Res.GetString("0d1dc61c-2252-4ef7-bd4b-35e0deb8c2ec", "View"),
														ClientClickHandler = ShipmentQuickViewUserLoginRequest
													}
													: new ZHyperLinkColumn("", "")
													{
														DataNavigateUrlFormatString = RelatedDocumentViewerURLFormatString,
														DataNavigateUrlFields = new[] { DocumentView.Schema.ParentPK, DocumentView.Schema.StorageDocPK },
														Text = Res.GetString("0d1dc61c-2252-4ef7-bd4b-35e0deb8c2ec", "View")
													});
					PopulateGrid(documentsGrid, columns.ToArray());
				}

				documentsGrid.ItemDataBound += DocumentsGrid_ItemDataBound;
			}
		}

		void DocumentsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			var linkColumn = 3;
			if (sender is ZGrid)
			{
				var grid = sender as ZGrid;
				for (int i = 0; i < grid.Columns.Count; i++)
				{
					var column = grid.Columns[i] as ZTemplateColumn;
					if (column.ColumnKey.Equals(WebTracker.Grids.Documents.View))
					{
						linkColumn = i;
						break;
					}
				}
			}
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				DocumentView documentsView = (DocumentView)e.Item.DataItem;
				if (!documentsView.HasImage)
				{
					e.Item.Cells[linkColumn].Text = Res.GetString("3a39f101-8916-4bfe-975e-0ac4d38e1ed8", "Not on File");
				}
			}
		}

		#endregion

		#region PackLines Grid

		internal protected virtual ZTemplateColumn MultipleProductsColumn
		{
			get
			{
				return null;
			}
		}

		public void PopulateGrid(ZDataGrid grid, DataGridColumn[] cols)
		{
			bool isZGrid = grid is ZGrid;
			foreach (var col in cols)
			{
				if (isZGrid)
				{
					((ZGrid)grid).ColumnProvider.AddToDictionaryAsDefault(col as IUniqueKeyColumn);
				}
				else
				{
					grid.Columns.Add(col);
				}
			}
			if (isZGrid)
			{
				((ZGrid)grid).RepopulateColumns();
			}
		}

		protected void SetupPackLinesGrid(ZDataGrid packLinesGrid, bool isShipmentPackLines, bool hideCustomsCodeAndPrice = false, bool useVolumeCalculator = true)
		{
			packLinesGrid.Columns.Clear();
			if (packLinesGrid is ZGrid)
			{
				((ZGrid)packLinesGrid).ColumnProvider = new TrackingPackLineDetailsGridColumnProvider(isShipmentPackLines, hideCustomsCodeAndPrice, MultipleProductsColumn);
			}
			else
			{
				PopulateGrid(packLinesGrid, GetPackLinesColumns(isShipmentPackLines, hideCustomsCodeAndPrice));
			}
			if (useVolumeCalculator && VolumeCalculatorGridAddOn != null)
			{
				VolumeCalculatorGridAddOn.Grid = packLinesGrid;
			}
		}

		DataGridColumn[] GetPackLinesColumns(bool shipmentPackLines, bool hideCustomsCodeAndPrice)
		{
			bool userIsAuthorised = SiteUser != null && !SiteUser.IsShipmentQuickViewUser;
			var packLinesGridColumns = new List<DataGridColumn>();

			packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("b40ea819-a5a4-47a6-9595-bfa43cb0d217", "Pieces"), JobPackLinesSchema.Constants.JL_PackageCount) { ColumnKey = WebTracker.Grids.PackLines.Pieces, Decimals = 0 });
			packLinesGridColumns.Add(new ZDropDownListColumn(Res.GetString("ed601c24-3857-4844-ac89-1bf7f1788852", "Pack Type"), JobPackLinesSchema.Constants.JL_F3_NKPackType, JobPackLinesSchema.Constants.JL_F3_NKPackType + "_List") { ColumnKey = WebTracker.Grids.PackLines.PackType, ValueFieldName = RefPackTypeSchema.F3_Code.Name, TextFieldName = RefPackTypeSchema.F3_Description.Name + (NoResString)"Multilingual" }); // property name contant

			if (userIsAuthorised)
			{
				packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("7c78f9c2-20ec-4cd8-84d5-339dd54e8f7a", "Length"), JobPackLinesSchema.Constants.JL_Length) { ColumnKey = WebTracker.Grids.PackLines.Length, Decimals = 3 });
				packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("1dcee0aa-f2a5-4a10-941a-9312c9dfc5ad", "Width"), JobPackLinesSchema.Constants.JL_Width) { ColumnKey = WebTracker.Grids.PackLines.Width, Decimals = 3 });
				packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("c8aca5fe-69ce-4dcc-a6a5-b6e9179a5f43", "Height"), JobPackLinesSchema.Constants.JL_Height) { ColumnKey = WebTracker.Grids.PackLines.Height, Decimals = 3 });
				packLinesGridColumns.Add(new ZDropDownListColumn(Res.GetString("d174aa99-1da7-42f1-b5f1-462db5769c34", "UD"), JobPackLinesSchema.Constants.JL_UnitOfDimension, JobPackLinesSchema.Constants.JL_UnitOfDimension + "_List") { ColumnKey = WebTracker.Grids.PackLines.UD, DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

				packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("a74cafe6-b79f-4242-8229-8715d91567e7", "Weight"), JobPackLinesSchema.Constants.JL_ActualWeight) { ColumnKey = WebTracker.Grids.PackLines.Weight, Decimals = 3 });
				packLinesGridColumns.Add(new ZDropDownListColumn(Res.GetString("11231c48-9dae-4c8c-aeed-508f19740dcd", "UQ"), JobPackLinesSchema.Constants.JL_ActualWeightUQ, JobPackLinesSchema.Constants.JL_ActualWeightUQ + "_List") { ColumnKey = WebTracker.Grids.PackLines.WeightUQ, DisplayStyle = OComboBoxDropDownStyle.CodeOnly });

				packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("8056c530-c58a-4b45-a1fa-46d74078b5e0", "Volume"), JobPackLinesSchema.Constants.JL_ActualVolume) { ColumnKey = WebTracker.Grids.PackLines.Volume, Decimals = 3 });
				packLinesGridColumns.Add(new ZDropDownListColumn(Res.GetString("11231c48-9dae-4c8c-aeed-508f19740dcd", "UQ"), JobPackLinesSchema.Constants.JL_ActualVolumeUQ, JobPackLinesSchema.Constants.JL_ActualVolumeUQ + "_List") { ColumnKey = WebTracker.Grids.PackLines.VolumeUQ, DisplayStyle = OComboBoxDropDownStyle.CodeOnly });
			}

			packLinesGridColumns.Add(new ZTextEditColumn(Res.GetString("30f766b6-4fe1-4a65-a9e8-f467e9e530a6", "Description"), JobPackLinesSchema.Constants.JL_Description) { ColumnKey = WebTracker.Grids.PackLines.Description });

			if (userIsAuthorised)
			{
				packLinesGridColumns.Add(new ZTextEditColumn(Res.GetString("a0e8ebbe-86fd-479d-8ca4-37f612b4ff72", "Marks and Numbers"), JobPackLinesSchema.Constants.JL_MarksAndNumbers) { ColumnKey = WebTracker.Grids.PackLines.MarksAndNumbers });

				if (!hideCustomsCodeAndPrice || shipmentPackLines)
				{
					packLinesGridColumns.Add(new ZCalcEditColumn(Res.GetString("291cfee8-2dfc-4ea9-964d-b10630901feb", "Line Price"), JobPackLinesSchema.Constants.JL_LinePrice) { ColumnKey = WebTracker.Grids.PackLines.LinePrice });
				}

				if (shipmentPackLines)
				{
					packLinesGridColumns.Add(new ZTextEditColumn(Res.GetString("adc85369-8cea-425d-b134-9e064711e846", "Currency"), TrackingPackLine.Schema.CurrencyCode) { ColumnKey = WebTracker.Grids.PackLines.Currency });
				}

				if (!hideCustomsCodeAndPrice)
				{
					packLinesGridColumns.Add(new ZTextEditColumn(Res.GetString("31a26a73-2d38-448e-b3f1-27a7775f9a35", "Tariff Num."), JobPackLinesSchema.Constants.JL_HarmonisedCode) { ColumnKey = WebTracker.Grids.PackLines.TariffNum });
				}
			}

			if (shipmentPackLines)
			{
				packLinesGridColumns.Add(new ZTextEditColumn(Res.GetString("d37733aa-1c0e-4998-bbb6-661da002d44c", "Container"), Enterprise.Freight.Business.PackLine.JL_Calc_ContainerNumberName) { ColumnKey = WebTracker.Grids.PackLines.Container });
			}

			if (MultipleProductsColumn != null)
			{
				MultipleProductsColumn.ColumnKey = WebTracker.Grids.PackLines.Products;
				packLinesGridColumns.Add(MultipleProductsColumn);
			}

			return packLinesGridColumns.ToArray();
		}

		protected VolumeCalculatorDataGridAddOn VolumeCalculatorGridAddOn;

		#endregion PackLines Grid

		#region Warehouse Docket Containers Grid

		protected void SetupWhsDocketContainersGrid(ZDataGrid whsReceiveContainersGrid)
		{
			PopulateGrid(whsReceiveContainersGrid, GetWhsReceiveContainersColumns());
		}

		DataGridColumn[] GetWhsReceiveContainersColumns()
		{
			var whsReceiveContainersGridColumns = new List<DataGridColumn>();
			whsReceiveContainersGridColumns.Add(new ZTextEditColumn(Res.GetString("043c9809-b2ab-4d30-a4db-11aedb0442d1", "Container #"), WhsDocketContainerSchema.WC_ContainerNum.Name));
			whsReceiveContainersGridColumns.Add(new ZTextEditColumn(Res.GetString("15eee40e-131b-439e-810a-38bcf8df4bd0", "Seal #"), WhsDocketContainerSchema.WC_SealNum.Name));
			ZFindBoxColumn containerTypeColumn = new ZFindBoxColumn(Res.GetString("10ffd858-3528-4b7e-bd18-83e47a64f3f2", "Type"), WhsDocketContainerSchema.WC_RC.Name, "Lookups.RefContainers");
			containerTypeColumn.ValueFieldName = "PK";
			containerTypeColumn.AutoPostBack = true;
			containerTypeColumn.ModuleID = WebModuleIDs.RefContainer;
			whsReceiveContainersGridColumns.Add(containerTypeColumn);
			whsReceiveContainersGridColumns.Add(new ZCheckBoxColumn(Res.GetString("8b54dbb2-a9b7-4291-9e0a-dd8b4b636ec1", "Palletized"), WhsDocketContainerSchema.WC_IsPalletised.Name));
			whsReceiveContainersGridColumns.Add(new ZCheckBoxColumn(Res.GetString("ef327e26-d227-4cf9-8e5b-1bfd3dd036bd", "Chargeable"), WhsDocketContainerSchema.WC_IsChargeable.Name));
			whsReceiveContainersGridColumns.Add(new ZCalcEditColumn(Res.GetString("a8f986f4-08fd-43f0-96f0-b24f53bdd47f", "Items"), WhsDocketContainerSchema.WC_ItemCount.Name));
			whsReceiveContainersGridColumns.Add(new ZCalcEditColumn(Res.GetString("0109b646-50e6-43c2-9592-dcd627eb9354", "Pallets"), WhsDocketContainerSchema.WC_PalletCount.Name));
			return whsReceiveContainersGridColumns.ToArray();
		}

		#endregion

		#region Receive Inventory Grid

		protected void SetupReceiveInventoryGridCore(ZDataGrid whsReceiveInventoryGrid, bool allowPostBack = true)
		{
			PopulateGrid(whsReceiveInventoryGrid, GetReceiveInventoryColumns(whsReceiveInventoryGrid.AllowAdd, whsReceiveInventoryGrid.AllowEdit, allowPostBack));
		}

		DataGridColumn[] GetReceiveInventoryColumns(bool allowAdd, bool allowEdit, bool allowPostBack = true)
		{
			var whsReceiveInventoryCols = new List<DataGridColumn>();
			if (allowAdd || allowEdit)
			{
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_OP);
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.Lookups.SupplierParts);
				var productColumn = new ZFindBoxColumn(Res.GetString("a76014b6-9e90-4906-9c2b-9ad12bd74c79", "Product"), TrackingWhsReceiveLine.WrapperSchema.WE_OP, "WhsReceiveLine.Lookups.SupplierParts")
				{
					ValueFieldName = "PK",
					ModuleID = WebModuleIDs.OrgSupplierPartTracking,
					AutoPostBack = allowPostBack
				};
				whsReceiveInventoryCols.Add(productColumn);
			}
			else
			{
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.ProductCode);
				var productColumn = new ZHyperLinkColumn(Res.GetString("a76014b6-9e90-4906-9c2b-9ad12bd74c79", "Product"), TrackingWhsReceiveLine.WrapperSchema.ProductCode)
				{
					DataNavigateUrlFormatString = AppInstance.ProductProfileDetailsPage + (NoResString)"?Ref={0}" + ShowInPopupParam, // Its string formater
					DataNavigateUrlFields = new string[1] { TrackingWhsReceiveLine.WrapperSchema.WE_OP }
				};
				if (IsShownInPopup)
				{
					productColumn.Target = (NoResString)"_blank"; // May be inside data
					productColumn.WindowStyle = Global.ProductProfilePopupWindowStyle;
				}
				whsReceiveInventoryCols.Add(productColumn);
			}

			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.ProductDesc);
			var productDescriptionColumn = new ZTextEditColumn(Res.GetString("30f766b6-4fe1-4a65-a9e8-f467e9e530a6", "Description"), TrackingWhsReceiveLine.WrapperSchema.ProductDesc)
			{
				ReadOnly = true
			};
			whsReceiveInventoryCols.Add(productDescriptionColumn);

			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PackQuantity);
			var packsColumn = new ZCalcEditColumn(Res.GetString("43c83d45-fa63-41dc-a4d0-616c3c030150", "Packs"), TrackingWhsReceiveLine.WrapperSchema.WE_PackQuantity)
			{
				AutoPostBack = allowPostBack
			};
			whsReceiveInventoryCols.Add(packsColumn);

			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_F3_NKPackType);
			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.Lookups.PackTypes);
			var packsUQColumn = new ZDropDownListColumn(Res.GetString("cda78685-39f3-4eef-860b-c8510c2319d6", "Packs UQ"), TrackingWhsReceiveLine.WrapperSchema.WE_F3_NKPackType, "WhsReceiveLine.Lookups.PackTypes")
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				AutoPostBack = allowPostBack
			};
			whsReceiveInventoryCols.Add(packsUQColumn);

			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_ClientOrderedUnits);
			var expectedQuantityColumn = new ZCalcEditColumn(Res.GetString("145fdd27-88ec-4eb1-a711-89768de74e2b", "Expected Quantity"), TrackingWhsReceiveLine.WrapperSchema.WE_ClientOrderedUnits)
			{
				BindToDecimals = "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces",
				AutoPostBack = allowPostBack
			};
			whsReceiveInventoryCols.Add(expectedQuantityColumn);

			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_TransactionQuantity);
			ZBindToChecker.CheckBindTo(((TrackingWhsInventory)null).SupplierPart.OP_CountDecimalPlaces);
			var quantityColumn = new ZCalcEditColumn(Res.GetString("2471b0c7-a366-48a8-b7de-10c3f886c7c0", "Quantity"), TrackingWhsReceiveLine.WrapperSchema.WE_TransactionQuantity)
			{
				BindToDecimals = "WhsReceiveLine.SupplierPart.OP_CountDecimalPlaces",
				AutoPostBack = allowPostBack
			};
			whsReceiveInventoryCols.Add(quantityColumn);

			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.ProductUQ);
			ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.Lookups.PackTypesWithStandardUnits);
			var uQColumn = new ZDropDownListColumn(Res.GetString("11231c48-9dae-4c8c-aeed-508f19740dcd", "UQ"), TrackingWhsReceiveLine.WrapperSchema.ProductUQ, "WhsReceiveLine.Lookups.PackTypesWithStandardUnits")
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ReadOnly = true
			};
			whsReceiveInventoryCols.Add(uQColumn);

			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn)
			{
				var partManager = Page.SiteUser.LoggedInOrganisation.PartAttributeManager;

				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PartAttrib1);
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PartAttrib2);
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_PartAttrib3);
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_SerialNumber);
				ZBindToChecker.CheckBindTo(((TrackingWhsReceiveLine)null).WhsReceiveLine.WE_ExpiryDate);
				AddClientDependantColumn(whsReceiveInventoryCols, partManager.IsPartAttributeUsedByOrganisation(1), new ZTextEditColumn(partManager.PartAttributeName1, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib1) { CanBeEnabledByClient = true });
				AddClientDependantColumn(whsReceiveInventoryCols, partManager.IsPartAttributeUsedByOrganisation(2), new ZTextEditColumn(partManager.PartAttributeName2, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib2) { CanBeEnabledByClient = true });
				AddClientDependantColumn(whsReceiveInventoryCols, partManager.IsPartAttributeUsedByOrganisation(3), new ZTextEditColumn(partManager.PartAttributeName3, TrackingWhsReceiveLine.WrapperSchema.WE_PartAttrib3) { CanBeEnabledByClient = true });
				AddClientDependantColumn(
					whsReceiveInventoryCols,
					partManager.IsSerialNumberUsedByOrganisation,
					new ZTextEditColumn(Res.GetString("3e483268-3b04-45d5-bcb3-e04f95d06694", "Serial Number"), TrackingWhsReceiveLine.WrapperSchema.WE_SerialNumber) { CanBeEnabledByClient = true });
				AddClientDependantColumn(whsReceiveInventoryCols, partManager.IsExpiryDateUsedByOrganisation, new ZDateTimeColumn(Res.GetString("8d8c1e47-7248-4caa-ba3d-914fedc18a32", "Expiry Date"), TrackingWhsReceiveLine.WrapperSchema.WE_ExpiryDate, ZDateTimePickerFormat.Short) { CanBeEnabledByClient = true });
			}
			return whsReceiveInventoryCols.ToArray();
		}

		protected void AddClientDependantColumn(List<DataGridColumn> columns, bool isUsed, ZTemplateColumn column)
		{
			if (isUsed)
			{
				columns.Add(column);
			}
		}

		protected void AddClientDependantColumn(DataGridColumnCollection columns, bool isUsed, ZTemplateColumn column)
		{
			if (isUsed)
			{
				columns.Add(column);
			}
		}

		#endregion

		#region Attributes

		protected void AddLineAttributeColumns(ZDataGrid grid, AttributeManager.AttributeModules module, OrgHeader relatedOrg)
		{
			AddLineAttributeColumns(grid, module, relatedOrg, ZString.Empty);
		}

		protected void AddLineAttributeColumns(ZDataGrid grid, AttributeManager.AttributeModules module, OrgHeader relatedOrg, ZString bindToPrefix)
		{
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn)
			{
				var attributes = new AttributeManager().GetLineAttributes(module, relatedOrg, Page.SiteUser.LoggedInOrganisation);
				foreach (var attribute in attributes)
				{
					grid.Columns.Add(ZTemplateColumn.GetNew(attribute.Caption, attribute.Column, bindToPrefix));
				}
			}
		}

		protected void AddLineAttributeColumns(List<DataGridColumn> columns, AttributeManager.AttributeModules module, OrgHeader relatedOrg)
		{
			AddLineAttributeColumns(columns, module, relatedOrg, ZString.Empty);
		}

		protected void AddLineAttributeColumns(List<DataGridColumn> columns, AttributeManager.AttributeModules module, OrgHeader relatedOrg, ZString bindToPrefix)
		{
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn)
			{
				var attributes = new AttributeManager().GetLineAttributes(module, relatedOrg, Page.SiteUser.LoggedInOrganisation);
				foreach (var attribute in attributes)
				{
					columns.Add(ZTemplateColumn.GetNew(attribute.Caption, attribute.Column, bindToPrefix));
				}
			}
		}

		protected void SetupAdditionalInformationTable(CustomLabelInfoList customLabels, Table table, ZCollapsablePanel panel, IWrappedBizOProvider provider = null)
		{
			if (customLabels.Count == 0)
			{
				panel.Visible = false;
			}
			else
			{
				foreach (CustomLabelInfo field in customLabels)
				{
					TableRow row = new TableRow();

					TableCell labelCell = new TableCell();
					TableCell valueCell = new TableCell();

					labelCell.Controls.Add(new Label { Text = field.Caption, CssClass = CssConstants.DetailsItem });

					var bindTo = provider != null ? provider.GetWrappedBindTo(field.PropertyName) : field.PropertyName;

					if (field.PropertyType == typeof(ZDateTime))
					{
						valueCell.Controls.Add(new ZDateTimeLabel { BindTo = bindTo });
					}
					else if (field.PropertyType.IsSubclassOf(typeof(INumericZType)))
					{
						valueCell.Controls.Add(new ZNumericLabel { BindTo = bindTo });
					}
					else
					{
						valueCell.Controls.Add(new ZTextLabel { BindTo = bindTo });
					}

					row.Cells.Add(labelCell);
					row.Cells.Add(valueCell);

					table.Rows.Add(row);
				}
			}
		}

		#endregion

		#region Test Overrides

#if DEBUG
		protected override ZGlobal GetNewTestGlobal()
		{
			throw new NotImplementedException();
		}
#endif
		#endregion

		#region Test Public properties

#if DEBUG
		public bool ShowLoginStatusForTest
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new NotSupportedException("This property should be used only for unit test");
				}

				return ShowLoginStatus;
			}
		}

		public bool PageRequiresLoginForTest(Uri url)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("This method should be used only for unit test");
			}

			return PageRequiresLogin(url);
		}

		public bool PageRequiresNavigationForTest(Uri url)
		{
			if (!Globals.IsTest)
			{
				throw new NotSupportedException("This method should be used only for unit test");
			}

			return PageRequiresNavigation(url);
		}

		public bool ShowChangePasswordLinkButtonForTest
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new NotSupportedException("This property should be used only for unit test");
				}

				return ShowChangePasswordLinkButton;
			}
		}

		public bool ShowLogOffLinkButtonForTest
		{
			get
			{
				if (!Globals.IsTest)
				{
					throw new NotSupportedException("This property should be used only for unit test");
				}

				return ShowLogOffLinkButton;
			}
		}

#endif
		#endregion
	}
}
