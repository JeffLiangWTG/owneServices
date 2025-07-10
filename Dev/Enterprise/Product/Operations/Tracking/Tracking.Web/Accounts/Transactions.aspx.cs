using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Accounts
{
	/// <summary>
	/// Page to allow searching of current transactions (Invoices and Credits only)
	/// </summary>
	public partial class Transactions : BasePageWithAuthorisation, IRememberFilterCriteriaPage, IModulePage
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			WebFilterBusinessObjectFactory filterFactory = new WebFilterBusinessObjectFactory(Factory);
			TrackingTransactionFilterBusinessObject filterBusinessObject = filterFactory.Load<TrackingTransactionFilterBusinessObject>();
			filterBusinessObject.FilterOperator = SQLComparisonOperator.Contains;
			filterBusinessObject.LoggedInUser = SiteUser.LoggedInUser;
			return filterBusinessObject;
		}

		protected TrackingTransactionFilterBusinessObject FilterBusinessObject
		{
			get { return DataSource as TrackingTransactionFilterBusinessObject; }
		}

		#endregion

		#region OnLoad

		protected override void OnPreBind()
		{
			base.OnPreBind();
			//binding drop-down
			new ZWebControlBinder(DataSource).Bind(Company.Controls);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (IsPostBack)
			{
				Message.Text = "";
			}

			if (!IsPostBack && CompanyDropDownList.Items.Count == 0)
			{
				NoStatementDiv.Visible = true;
				AuthorisedContent.Visible = false;
				NoStatementLabel.Text = Res.GetString("D3A08E4D-7A71-4F99-9D5B-A9AF7D492FCB", "There are no companies which have yet posted invoices for {0}. A Statement of Account is not currently available.", SiteUser.LoggedInOrganisation.OH_FullName);
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewAccounts; }
		}

		#endregion OnLoad

		#region OnPreRender

		protected override void OnPreRender(EventArgs e)
		{
			if (FilterBusinessObject.FilterByCompanyPK.IsEmpty)
			{
				searchControl.ViewButtonClientClickScript = "javascript: alert('" + ViewButtonEmptyMessage + "'); return false;";
			}
			base.OnPreRender(e);
		}

		string ViewButtonEmptyMessage
		{
			get { return Res.GetString("85ea158f-84f8-46d6-bd47-f462fecdd2e3", "Before requesting selected transactions please select one specific \"Issued by\" Company and then click Find button."); }
		}

		#endregion

		#region Statements

		protected void StatementButton_Click(object sender, EventArgs e)
		{
			ZGuid companyPK = new ZGuid(CompanyDropDownList.SelectedValue);
			string handlerUrl = StatementRequestHandler.RequestHelper.GetHandlerUrl(companyPK);
			Response.Redirect(handlerUrl);
		}

		#endregion

		#region GetSelectedInvoices

		void GetSelectedInvoicesHandler(object sender, EventArgs e)
		{
			if (!FilterBusinessObject.FilterByCompanyPK.IsEmpty)
			{
				ZGuid[] selectedInvoices = searchControl.SearchResultsDataGrid.GetSelectedPKs();
				if (selectedInvoices.Length == 0 || (selectedInvoices.Length == 1 && selectedInvoices[0].IsEmpty))
				{
					ShowValidationError(GetSelectedAtLeaseOneValidiationMessage);
				}
				else if (selectedInvoices.Length > 100)
				{
					ShowValidationError(GetSelectedAtMost100ValidiationMessage);
				}
				else
				{
					searchControl.ViewButtonUrl = InvoiceRequestHandler.RequestHelper.GetHandlerUrl(selectedInvoices);
				}
			}
			else
			{
				searchControl.ViewButtonUrl = null;
			}
		}

		void ShowValidationError(string validationMessage)
		{
			searchControl.ViewButtonUrl = "";
			if (!ZClientScript.IsStartupScriptRegistered(GetSelectedValidationScriptKey))
			{
				ZClientScript.RegisterStartupScript(GetType(), GetSelectedValidationScriptKey, string.Format(CultureInfo.InvariantCulture, (NoResString)"<SCRIPT>alert('{0}');</SCRIPT>", validationMessage)); // Javascript script
			}
			searchControl.FindButton_Click(this, new EventArgs());
		}

		const string GetSelectedValidationScriptKey = "GetSelectedValidationScript";

		string GetSelectedAtLeaseOneValidiationMessage
		{
			get { return Res.GetString("75b65d82-4653-4fcf-b81a-a006a5bc4cd5", "Before requesting selected transactions please select at least one transaction."); }
		}

		string GetSelectedAtMost100ValidiationMessage
		{
			get { return Res.GetString("b9761f14-a991-4c88-9ba2-261713083a4c", "No more than 100 invoices can be selected at once."); }
		}

		#endregion

		#region Page Setup

		protected override System.Web.UI.ControlCollection ControlsToBind
		{
			get { return searchControl.Controls; }
		}

		#endregion

		#region Search Control setup

		public ISearchControl SearchControl { get { return searchControl; } }

		SearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = GetNewSearchControl();
			searchControl.SearchResultsDataGrid.AllowMultiLineSelection = true;
			searchControl.ModuleID = WebModuleIDs.TrackingAccounts;
			searchControl.ViewButtonVisible = true;
			searchControl.ViewButtonText = Res.GetString("d3a1a6c1-886d-4a2c-b2ee-9acda4f79ab3", "Get Selected");
			searchControl.ViewButtonUrl = "";
			searchControl.CustomViewButtonClick += new EventHandler(this.GetSelectedInvoicesHandler);
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		protected virtual SearchControl GetNewSearchControl()
		{
			return Page.LoadControl(SearchControlResource.FileName) as SearchControl;
		}

		#endregion

		#region AutoGenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
			SetupSearchControl();
			new InvoicePresenter(SiteUser).EnableItemCommand(searchControl.SearchResultsDataGrid);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			TitleLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((TrackingTransactionFilterBusinessObject)(null)).Company);
			CompanyDropDownList.BindTo = "Company";
			Ztextlabel1.BindTo = null;
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingTransactionFilterBusinessObject";
		}
		#endregion

		#endregion AutoGenerated

		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.Transactions;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.TransactionsPage;
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return Res.GetString("4c2d61ff-0a49-4ca9-ad86-8e3577c72944", "Accounts"); }
		}

		#endregion
	}
}
