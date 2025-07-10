using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Admin;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class SwitchCompanyTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.SwitchCompany;
		}

		public void TestDataSource()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			AssertNotNull("This is bound to CurrentCompanyLabel", ZPropertyAccessor.Get(PageForTest.DataSource, "SiteUser.LoggedInOrganisation.OH_FullName"));
			AssertNotNull("This is bound to CompaniesDataGrid", ZPropertyAccessor.Get(PageForTest.DataSource, "SiteUser.AllUserRelatedOrgs"));
		}

		public void TestSetupCompaniesGrid()
		{
			PageForTest.SetupCompaniesGridForTest();

			AssertEquals("CompaniesGrid should contain 12 columns", 2, PageForTest.CompaniesDataGridForTest.Columns.Count);

			AssertEquals("First Column Title", "Code", PageForTest.CompaniesDataGridForTest.Columns[0].HeaderText);
			AssertEquals("Second Column Title", "Organization Name", PageForTest.CompaniesDataGridForTest.Columns[1].HeaderText);
		}

		public void TestHighlightRowForCurrentCompany()
		{
			PageForTest.LoginManForTest.CompanyCode = "ABC";
			AssertHighlightRowForCurrentCompany("ABC", true, "Row should be highlighted");
			AssertHighlightRowForCurrentCompany("DFG", false, "Row should not be highlighted");
		}

		#region TestHighlightRowForCurrentCompany

		[HttpContextEnabledTest]
		public void TestSwitchCompany_OnFailure_RedirectsToLoginWithSessionRefParameter()
		{
			var item = Factory.NewWithValidTestData<OrgHeader>();
			var items = new OrgHeaderCollection(Factory);
			items.Add(item);
			PageForTest.SetupCompaniesGridForTest();
			PageForTest.CompaniesDataGridForTest.Bind(items);

			PageForTest.InvokeSwitchCompanyCommandForTest(PageForTest.CompaniesDataGridForTest.Items[0]);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			Assert(HttpContext.Current.Response.RedirectLocation.EndsWith($"?{TrackingConstants.QueryStringKeys.RefKey}={PageForTest.DataSourceIndexer}&ClearSaved=1"));
		}

		public void TestNoCache()
		{
			Assert(!PageForTest.CacheableForTest);
		}

		void AssertHighlightRowForCurrentCompany(string code, bool expectedValue, string message)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = code;

			DataGridItem row = new DataGridItem(0, 0, ListItemType.Item);
			row.DataItem = org;

			PageForTest.HighlightRowForCurrentCompanyForTest(row);

			AssertEquals(message, expectedValue, row.Font.Bold);
		}

		#endregion

		#region BaseTrackingPageTest Implementation

		protected override Control GetNewControl()
		{
			return new SwitchCompanyForTest();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			PageForTest = new SwitchCompanyForTest();
			PageForTest.LoadForTest();
		}

		SwitchCompanyForTest PageForTest;

		#region SwitchCompanyForTest

		class SwitchCompanyForTest : SwitchCompany
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public void SetupCompaniesGridForTest()
			{
				CompaniesDataGrid = new ZDataGrid();
				Controls.Add(CompaniesDataGrid);
				SetupCompaniesGrid();
			}

			public bool CacheableForTest => Cacheable;

			public ZDataGrid CompaniesDataGridForTest
			{
				get
				{
					return CompaniesDataGrid;
				}
			}

			public LoginManager LoginManForTest
			{
				get
				{
					return LoginMan;
				}
			}

			public ZTextLabel CurrentCompanyLabelForTest
			{
				get
				{
					return CurrentCompanyLabel;
				}
			}

			public void HighlightRowForCurrentCompanyForTest(DataGridItem row)
			{
				HighlightRowForCurrentCompany(row);
			}

			public void InvokeSwitchCompanyCommandForTest(DataGridItem item)
			{
				CompaniesDataGrid_ItemCommand(this, new DataGridCommandEventArgs(item, this, new CommandEventArgs(cmdSwitchCompany, null)));
			}

			public void LoadForTest()
			{
				OnLoad(EventArgs.Empty);
			}
		}

		#endregion

		#endregion
	}
}
