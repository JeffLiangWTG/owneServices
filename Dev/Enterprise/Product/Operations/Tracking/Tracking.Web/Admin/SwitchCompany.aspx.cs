using System.Web;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Admin
{
	public partial class SwitchCompany : BasePage
	{
		protected override bool Cacheable => false;

		#region Setup Grid

		protected override void SetupGrids()
		{
			SetupCompaniesGrid();
		}

		protected void SetupCompaniesGrid()
		{
			ZBindToChecker.CheckBindTo(((OrgHeader)null).OH_Code);
			ZLinkButtonColumn codeColumn = new ZLinkButtonColumn(Res.GetString("72283c37-f657-44e9-990a-f8bfb29c3d11", "Code"), OrgHeaderSchema.Constants.OH_Code);
			codeColumn.Command = cmdSwitchCompany;
			CompaniesDataGrid.Columns.Add(codeColumn);

			ZBindToChecker.CheckBindTo(((OrgHeader)null).OH_FullName);
			CompaniesDataGrid.Columns.Add(new ZTextEditColumn(Res.GetString("c338b5ae-dcba-415c-8c84-6b52fb2e6daf", "Organization Name"), OrgHeaderSchema.Constants.OH_FullName));

			CompaniesDataGrid.ItemCommand += new DataGridCommandEventHandler(CompaniesDataGrid_ItemCommand);
			CompaniesDataGrid.ItemDataBound += new DataGridItemEventHandler(CompaniesDataGrid_ItemDataBound);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]

#if DEBUG
		protected
#endif

		const string cmdSwitchCompany = "switch";

		#endregion

		#region Event Handlers

		void CompaniesDataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				HighlightRowForCurrentCompany(e.Item);
			}
		}

#if DEBUG
		protected
#endif

		void CompaniesDataGrid_ItemCommand(object source, DataGridCommandEventArgs e)
		{
			if (e.CommandName == cmdSwitchCompany)
			{
				var selectedCompanyCode = ((OrgHeaderCollection)CompaniesDataGrid.DataSource)[e.Item.DataSetIndex].OH_Code;

				if (!SwitchCurrentCompany(selectedCompanyCode))
				{
					HttpContext.Current.Response.Redirect($"{AppInstance.LoginPage}?{TrackingConstants.QueryStringKeys.RefKey}={DataSourceIndexer}&ClearSaved=1"); // url
				}
			}
		}

		#endregion

		#region Binding

		protected override BusinessObject GetNewDataSource()
		{
			LoginManager result = new LoginManager();
			if (SiteUser != null && SiteUser.IsLoggedIn)
			{
				result.CompanyCode = SiteUser.LoggedInOrganisation.OH_Code;
				result.UserName = SiteUser.LoggedInUser.OC_Email;
			}
			return result;
		}

		protected LoginManager LoginMan
		{
			get { return DataSource as LoginManager; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		protected override string GetPageName()
		{
			return WebTracker.Pages.SwitchCompany;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.SwitchCompanyPage;
		}

		protected bool SwitchCurrentCompany(string selectedCompanyCode)
		{
			bool result = false;

			if (SiteUser.IsLoggedIn)
			{
				SiteUser.Logout();
				LoginMan.CompanyCode = selectedCompanyCode;

				var loginHelper = new TrackingLoginHelper(this, false);
				result = loginHelper.SignIn();

				if (result)
				{
					var eventLogHelper = new EventLogHelper();
					eventLogHelper.CreateLogForUserLoggedIn(SiteUser);
				}
			}

			return result;
		}

		protected override ZString ModuleNameForEventLogging
		{
			get { return (NoResString)"Switch Company"; } // event logging related
		}

		protected void HighlightRowForCurrentCompany(DataGridItem row)
		{
			OrgHeader org = (OrgHeader)row.DataItem;
			if (org.OH_Code == LoginMan.CompanyCode)
			{
				row.Font.Bold = true;
			}
		}

		#endregion
	}
}
