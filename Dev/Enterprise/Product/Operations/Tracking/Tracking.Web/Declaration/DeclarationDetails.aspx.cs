using System;
using System.Collections.Generic;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Declaration;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for DeclarationDetails.
	/// </summary>
	public partial class DeclarationDetails : BasePageWithAuthorisation
	{
		protected override string GetPageName()
		{
			return WebTracker.Pages.DeclarationDetails;
		}

		protected override bool CanAccessAuthorisedContent => SiteUser?.CanViewDeclarations ?? false;

		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingDeclaration.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		public TrackingDeclaration CurrentDeclaration
		{
			get { return DataSource as TrackingDeclaration; }
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var isDataSourceValid = ((CurrentDeclaration != null) && (CurrentDeclaration.Declaration != null));

			AuthorisedContent.Visible = isDataSourceValid && CanAccessAuthorisedContent;
			NotFoundError.Visible = !isDataSourceValid;
			InvoicesGrid.Visible = SiteUser != null && !SiteUser.IsShipmentQuickViewUser;
			OrdersGrid.Visible = SiteUser?.CanViewOrders ?? false;

			if (!isDataSourceValid)
			{
				DeclarationNotFoundLabel.Text = Res.GetString("c4573eb3-c5a2-424b-8c02-8708b2a6dc4d", "Shipment was not found in the database or you don't have rights to view it.");
			}
			else
			{
				if (CurrentDeclaration.Declaration.JE_IsCancelled)
				{
					ZTextLabel cancelledLabel = new ZTextLabel();
					cancelledLabel.CssClass = "SectionTitle";
					cancelledLabel.Text = Res.GetString("b1df7465-3702-461f-ba63-701ad661040a", "This shipment has been canceled");
					cancelledLabel.ForeColor = System.Drawing.Color.Red;
					AuthorisedContent.Controls.AddAt(0, cancelledLabel);
				}

				//rename label in case of Export declaration
				if (CurrentDeclaration.Declaration.JE_MessageType == "EXP")
				{
					EstDeliveryLabel.Text = Res.GetString("3b1368a3-bbe0-47c5-92a1-6f03eb2fa3c7", "Est. pickup:") + " ";
					DeliveryRequiredLabel.Text = Res.GetString("c32669dc-8a36-45cf-8789-1f1c4169aa02", "Pickup req. by:") + " ";
					CartageAdvisedLabel.Text = Res.GetString("b2136ec5-669c-4897-ae37-fce4389a473f", "Docs to carrier:") + " ";
					GoodsDeliveredLabel.Text = Res.GetString("827b885b-90ab-4bf2-9b90-ab1d2785c3c7", "Goods picked up:") + " ";
				}

				SetUpTransportControls();
			}

			if (SiteUser.IsShipmentQuickViewUser)
			{
				ForAuthentifiedUserOnly.Visible = false;
				ForAuthentifiedUserOnly2.Visible = false;

				LocalChargesGrid.Visible = LocalChargesVisible;
				ChargesGrid.Visible = LocalChargesVisible;
				if (LocalChargesVisible)
				{
					LocalChargesGrid.ShouldShowControl = false;
					ChargesGrid.ShouldShowControl = false;
				}

				OrdersGrid.ShouldShowControl = false;
				CustomsEntriesDataGrid.ShouldShowControl = false;
				TransportsGrid.ShouldShowControl = false;
				ContainerGrid.ShouldShowControl = false;
				InvoicesGrid.ShouldShowControl = false;
				HouseBillsGrid.ShouldShowControl = false;
			}

			DocumentsGrid.Visible = SiteUser.CanViewDocuments;
		}

#if DEBUG
		public
#endif
 void SetUpTransportControls()
		{
			bool useGridForTransport = GetVisibleForTransportGrid();
			TransportsGrid.Visible = useGridForTransport;
			AreaTransportControl.Visible = !useGridForTransport;
		}

		bool GetVisibleForTransportGrid()
		{
			bool result = false;
			if ((CurrentDeclaration.Declaration.IsStandAlone ||
				(!CurrentDeclaration.Declaration.IsStandAlone && !CurrentDeclaration.Declaration.JE_OverrideFreightDefaults)) &&
				CurrentDeclaration.TransportsIncludingRelated.Count > 0)
			{
				result = true;
			}
			return result;
		}

		protected virtual void SetUpStatusControl(string countryCode, string direction)
		{
			BaseStatusControl statusControl = null;
			try
			{
				statusControl = Page.LoadControl(GetStatusControlPath(countryCode, direction)) as BaseStatusControl;
			}
			catch (HttpException) { }

			if (statusControl != null)
			{
				statusControl.BindTo = "Declaration";
				StatusHolder.Controls.Add(statusControl);
				StatusHolder.Visible = true;
			}
		}

		public void SetUpCountrySpecificControls()
		{
			if (CurrentDeclaration != null && CurrentDeclaration.Declaration != null && CurrentDeclaration.Declaration.Branch != null && CurrentDeclaration.Declaration.Branch.Country != null)
			{
				var countryCode = CurrentDeclaration.Declaration.Branch.Country.Code;
				var direction = CurrentDeclaration.Declaration.JE_MessageType;
				SetUpStatusControl(countryCode, direction);
				CustomsEntriesDataGrid.Visible = !(countryCode == Core.Constants.CountryCodes.UnitedStates && direction == Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Import);
			}
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();

			SetupDocumentsGrid(DocumentsGrid);
			SetupChargesGrid();
			SetUpCountrySpecificControls();
			SetUpHouseBillsGrid();
		}

		public new TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		#region Setting up grids

		/// <summary>
		/// Set up all grids on the page
		/// </summary>
		protected override void SetupGrids()
		{
			TransportsGrid.ColumnProvider = new TrackingTransportDetailsGridColumnProvider();
			OrdersGrid.ColumnProvider = new TrackingOrderColumnProvider(false);
			ContainerGrid.ColumnProvider = new CustomsContainerColumnProvider(SiteUser);
			SetupCustomsEntriesGrid();
			SetupInvoicesGrid();
		}

		#region SetupInvoicesGrid

		void SetupInvoicesGrid()
		{
			if (SiteUser != null && !SiteUser.IsShipmentQuickViewUser)
			{
				InvoicesGrid.ColumnProvider = new CustomsInvoiceColumnProvider(this.Page);
			}
		}

		#endregion

		#region Charges Grid

		void SetupChargesGrid()
		{
			if (((TrackingSiteUser)SiteUser).CanViewAccounts)
			{
				ChargesGrid.Visible = true;
				InvoicePresenter presenter = new InvoicePresenter(SiteUser);
				presenter.SetupGrid(ChargesGrid);
				if (LocalChargesVisible)
				{
					presenter.SetupLocalChargesGrid(LocalChargesGrid);
				}
			}
			else
			{
				ChargesGrid.Visible = false;
			}
		}

		bool LocalChargesVisible
		{
			get
			{
				return SiteUser.IsShipmentQuickViewUser &&
					WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.Value &&
					CurrentDeclaration.InvoiceLoader.LocalChargesDetails != null &&
					CurrentDeclaration.InvoiceLoader.LocalChargesDetails.Count > 0;
			}
		}

		#endregion Charges Grid

		#region CustomsEntriesDataGrid

		public void SetupCustomsEntriesGrid()
		{
			CustomsEntriesDataGrid.ColumnProvider = new CustomsEntriesDataColumnProvider(CurrentDeclaration);
		}

		#endregion

		public void SetUpHouseBillsGrid()
		{
			HouseBillsGrid.ColumnProvider = HouseBillColumnProviderTypeDecider.GetColumnProviderForDeclaration(CurrentDeclaration);
		}

		#endregion

		protected internal override IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				return CurrentDeclaration != null ?
					(new ILicenceCheckpoint[] { CurrentDeclaration.Declaration.IsImport ?
						Environment.Env.Licence.WebTrackerImportBrokerage :
						Environment.Env.Licence.WebTrackerExportBrokerage })
					: base.LicenceCheckPoints;
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.DeclarationDetailsPage;
		}

		public override void Validate()
		{
			var trackingDeclaration = BusinessObjectToValidate as TrackingDeclaration;
			if (trackingDeclaration?.Declaration is JobDeclaration usDeclaration && usDeclaration.JE_MessageType == USJobMessageTypeList.Codes.Recon)
			{
				usDeclaration.ReconDeclaration = new ReconDeclaration(usDeclaration);
				_ = usDeclaration.ReconDeclaration.OriginalEntries;
			}
			base.Validate();
		}

		#region Autogenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingDeclaration";
		}

		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#endregion
	}
}
