using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Caching;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AdministrationPanelForm))]
	sealed class AdministrationPanelFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AdministrationPanelForm(new AdministrationPanelManager(Factory));
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return true;
		}

		public void TestOnlyCloseButtonVisibleOnForm()
		{
			AdministrationPanelForm.Show();
			Assert(!AdministrationPanelForm.PostButton.Visible);
			Assert(!AdministrationPanelForm.ApplyButton.Visible);
			Assert(AdministrationPanelForm.CloseButton.Visible);
		}

		public void TestCreateDeDuplicationTabPage()
		{
			AdministrationPanelForm.Show();
			var tabPage = AdministrationPanelForm.DeDupTabControl.TabPages[0];
			AssertEquals("TabPage's Text", "Organizations", tabPage.Text);
		}

		public void TestFormCaptionDoesNotContainNew()
		{
			AdministrationPanelForm.Show();
			Assert("The form's caption should not contain 'New'", !form.FormCaption.Contains("New"));
		}

		public void TestFormMinimumSize()
		{
			AdministrationPanelForm.Show();
			AssertEquals("The form's size should be 1366x725", new System.Drawing.Size(1366, 725), form.MinimumSize);
		}

		public void TestTabsShouldBeDisplayed()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				var mainTabControl = adminForm.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;

				AssertNull("The DashboardTabPage should be hidden", mainTabControl?.GetTabPage("DashboardTabPage"));
				AssertNull("The StandardsTabPage should be hidden", mainTabControl?.GetTabPage("StandardsTabPage"));
				AssertEquals("The AddressesTabPage should be displayed", true, mainTabControl?.GetTabPage("AddressesTabPage")?.TabVisible ?? false);
				AssertEquals("The DuplicatesTabPage should be displayed", true, mainTabControl?.GetTabPage("DuplicatesTabPage")?.TabVisible ?? false);
				AssertEquals("The BackgroundValidationMenuItem should be enabled", true, adminForm.BackgroundValidationMenuItem.Enabled);
			}
		}

		public void TestTabsShouldBeHidden()
		{
			SystemDataRegistry.Instance.MdmAdministrationPanelAddressTabEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.MdmAdministrationPanelDuplicatesTabEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new AdministrationPanelFormForTest(new BusinessObjectFactory()))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;

				AssertNull("The DashboardTabPage should be hidden", mainTabControl?.GetTabPage("DashboardTabPage"));
				AssertNull("The StandardsTabPage should be hidden", mainTabControl?.GetTabPage("StandardsTabPage"));
				AssertEquals("The AddressesTabPage should be hidden", false, mainTabControl?.GetTabPage("AddressesTabPage")?.TabVisible ?? false);
				AssertEquals("The DuplicatesTabPage should be hidden", false, mainTabControl?.GetTabPage("DuplicatesTabPage")?.TabVisible ?? false);
				AssertEquals("The ContactsTabPage should be hidden", false, mainTabControl?.GetTabPage("Contacts")?.TabVisible ?? false);
				AssertEquals("The BackgroundValidationMenuItem should be disabled", false, form.BackgroundValidationMenuItem.Enabled);
			}
		}

		public void TestBackgroundValidationMenuItem_WhenBackgroundValidationSuspendedIsFalse()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (SystemDataRegistry.Instance.BackgroundValidationSuspended.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				Assert(adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.SuspendValidationCaption, adminForm.BackgroundValidationMenuItem.Text);

				adminForm.BackgroundValidationMenuItem.PerformClick();
				Application.DoEvents();
				Assert(adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.StartValidationCaption, adminForm.BackgroundValidationMenuItem.Text);
				AssertEquals(adminForm.TurnOffValidationText, adminForm.MessageStatusBarPanel.Text);

				adminForm.BackgroundValidationMenuItem.PerformClick();
				Application.DoEvents();
				Assert(adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.SuspendValidationCaption, adminForm.BackgroundValidationMenuItem.Text);
				AssertEquals(adminForm.TurnOnValidationText, adminForm.MessageStatusBarPanel.Text);
			}
		}

		public void TestBackgroundValidationMenuItem_WhenBackgroundValidationSuspendedIsTrue()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (SystemDataRegistry.Instance.BackgroundValidationSuspended.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				Assert(adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.StartValidationCaption, adminForm.BackgroundValidationMenuItem.Text);

				adminForm.BackgroundValidationMenuItem.PerformClick();
				Application.DoEvents();
				Assert(adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.SuspendValidationCaption, adminForm.BackgroundValidationMenuItem.Text);
				AssertEquals(adminForm.TurnOnValidationText, adminForm.MessageStatusBarPanel.Text);

				adminForm.BackgroundValidationMenuItem.PerformClick();
				Application.DoEvents();
				Assert(adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.StartValidationCaption, adminForm.BackgroundValidationMenuItem.Text);
				AssertEquals(adminForm.TurnOffValidationText, adminForm.MessageStatusBarPanel.Text);
			}
		}

		public void TestValidationMenuItem_WhenAddressValidationWebServiceIsDisabled()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			using (SystemDataRegistry.Instance.BackgroundValidationSuspended.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				Assert(!adminForm.BackgroundValidationMenuItem.Enabled);
				AssertEquals(adminForm.StartValidationCaption, adminForm.BackgroundValidationMenuItem.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyAndSearchLink()
		{
			AdministrationPanelForm.Show();
			AdministrationPanelForm.SelectTab("AddressesTabPage");

			AssertCopyAndSearchLinkStatus(form.CopyCompanyInformationMenuItem, form.CopyCompanyInformationCaption, form.SearchCompanyOnlineMenuItem, form.SearchCompanyOnlineCaption, AddressUserControl.AddressDetailControl.CopyCompanyInfoLinkControl, AddressUserControl.AddressDetailControl.SearchCompanyOnlineControl);

			AssertCopyAndSearchLinkStatus(form.CopyAddressInformationMenuItem, form.CopyAddressInformationCaption, form.SearchAddressOnlineMenuItem, form.SearchAddressOnlineCaption, AddressUserControl.AddressDetailControl.CopyAddressInfoLinkControl, AddressUserControl.AddressDetailControl.SearchAddressOnlineControl);

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress1.OA_Address2 = "ORGADDRESS1_ADDRESS2";
			orgAddress1.OA_PostCode = "210036";
			orgAddress1.OA_City = "BEIJING";
			orgAddress1.OA_State = "STATE";
			orgAddress1.OA_RN_NKCountryCode = "CN";
			orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
			orgAddress1.OA_ValidationStatus = "INV";
			orgAddress1.Header.OH_FullName = "WiseTech Global";
			Factory.Save();

			var filter = FilterBusinessObject["Address1"] as ModuleTextFilter;
			filter.Property = "ORGADDRESS1_ADDRESS1";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			FilterControl.FirePerformSearch();
			AssertMenuItemAndLinkLabelEnabled();

			AddressUserControl.AddressDetailControl.CountryControl.Focus();
			AddressUserControl.AddressDetailControl.CountryControl.CurrentCode = "";
			AddressUserControl.AddressDetailControl.Address1Control.Focus();
			AssertMenuItemAndLinkLabelEnabled();

			AddressUserControl.AddressDetailControl.CountryControl.Focus();
			AddressUserControl.AddressDetailControl.CountryControl.CurrentCode = "CN";
			AddressUserControl.AddressDetailControl.Address1Control.Focus();
			AssertMenuItemAndLinkLabelEnabled();

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.Equal, orgAddress1.OA_RN_NKCountryCode)).RN_Desc;
			var companyAndCountry = orgAddress1.Header.OH_FullName + " " + country;
			var companyAndCountryForUrlParams = WebUtility.UrlEncode(companyAndCountry);
			AssertCopyAndSearchLinkClickResults(form.CopyCompanyInformationMenuItem, form.SearchCompanyOnlineMenuItem, AddressUserControl.AddressDetailControl.CopyCompanyInfoLinkControl, AddressUserControl.AddressDetailControl.SearchCompanyOnlineControl, companyAndCountry, "http://google.com/search?q=" + companyAndCountryForUrlParams);

			var addressInfo = string.Join(" ", new string[] { orgAddress1.OA_Address1, orgAddress1.OA_Address2, orgAddress1.OA_City, orgAddress1.OA_State, orgAddress1.OA_PostCode, country }.Where(u => !string.IsNullOrEmpty(u)));
			var addressInfoForUrlParams = WebUtility.UrlEncode(addressInfo);
			AssertCopyAndSearchLinkClickResults(form.CopyAddressInformationMenuItem, form.SearchAddressOnlineMenuItem, AddressUserControl.AddressDetailControl.CopyAddressInfoLinkControl, AddressUserControl.AddressDetailControl.SearchAddressOnlineControl, addressInfo, "http://google.com/maps/search/" + addressInfoForUrlParams);

			AdministrationPanelForm.SelectTab("DuplicatesTabPage");
			AssertMenuItemAndLinkLabelStatus(false, form.CopyCompanyInformationMenuItem, form.CopyCompanyInformationCaption, form.SearchCompanyOnlineMenuItem, form.SearchCompanyOnlineCaption);
			AssertMenuItemAndLinkLabelStatus(false, form.CopyAddressInformationMenuItem, form.CopyAddressInformationCaption, form.SearchAddressOnlineMenuItem, form.SearchAddressOnlineCaption);

			AdministrationPanelForm.SelectTab("AddressesTabPage");
			AssertMenuItemAndLinkLabelStatus(true, form.CopyCompanyInformationMenuItem, form.CopyCompanyInformationCaption, form.SearchCompanyOnlineMenuItem, form.SearchCompanyOnlineCaption);
			AssertMenuItemAndLinkLabelStatus(true, form.CopyAddressInformationMenuItem, form.CopyAddressInformationCaption, form.SearchAddressOnlineMenuItem, form.SearchAddressOnlineCaption);
		}

		public void TestShowBackgroundValidationStatusIconAndHideWhenAutoVerifyIsCancelled()
		{
			var url = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort());
			AddressValidationService.SetAvailableWebServiceAddress(url);

			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET", "PUT", "POST" },
				Processor = (_, request) => new Tuple<int, string>(401, string.Empty),
				Uri = new Uri(url + AddressValidationService.Constants.ValidationServiceName + "/"),
				ContentType = "application/json"
			};

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			try
			{
				testExternalValidationService.Start();
				using (var adminForm = new AdministrationPanelFormForTest(Factory))
				{
					adminForm.Show();
					adminForm.SelectTab("AddressesTabPage");

					var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
					orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
					orgAddress1.OA_ValidationStatus = AddressValidationStatus.ToBeVerified;
					Factory.Save();

					var filterControl = adminForm.UserControl.FilterControl;
					var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.ValidationStatus] as ModuleTextFilter;
					filter.Property = AddressValidationStatus.ToBeVerified;
					filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
					filter.IsActive = true;

					filterControl.FirePerformSearch();
					AssertEquals("The BackgroundValidationStatusIcon should be shown", true, filterControl.BackgroundValidationStatusIcon.Visible);

					adminForm.BackgroundValidationMenuItem.PerformClick();
					AssertEquals("The BackgroundValidationStatusIcon should be hidden", false, filterControl.BackgroundValidationStatusIcon.Visible);
					Application.DoEvents();
				}
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
				AddressValidationService.SetAvailableWebServiceAddress();
			}
		}

		[RequiresSTA]
		public void TestSaveDialogWhenFormClose_ClickCancelButton()
		{
			AdministrationPanelForm.Show();
			AdministrationPanelForm.SelectTab("AddressesTabPage");

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress1.OA_ValidationStatus = "INV";
			Factory.Save();

			FilterControl.FirePerformSearch();
			FilterControl.Grid.PerformMouseDownForTest(0, 1);
			AddressUserControl.AddressDetailControl.Address2Control.Focus();
			AddressUserControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
			AddressUserControl.AddressDetailControl.Address1Control.Focus();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			AdministrationPanelForm.Close();
			AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("The form visible is true", true, AdministrationPanelForm.Visible);
		}

		public void TestSaveDialogWhenFormClose_ClickNoButton()
		{
			AdministrationPanelForm.Show();
			AdministrationPanelForm.SelectTab("AddressesTabPage");

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress1.OA_ValidationStatus = "INV";
			Factory.Save();

			FilterControl.FirePerformSearch();
			FilterControl.Grid.PerformMouseDownForTest(0, 1);
			AddressUserControl.AddressDetailControl.Address2Control.Focus();
			AddressUserControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
			AddressUserControl.AddressDetailControl.Address1Control.Focus();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AdministrationPanelForm.Close();
			AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("The form visible is false", false, AdministrationPanelForm.Visible);

			var orgaddress = Factory.Load<OrgAddress>(orgAddress1.PK);
			AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1'", "ORGADDRESS1_ADDRESS1", orgaddress.OA_Address1);
			AssertEquals("The address2 should be empty", "", orgaddress.OA_Address2);
		}

		public void TestSaveDialogWhenFormClose_ClicYesButton()
		{
			AdministrationPanelForm.Show();
			AdministrationPanelForm.SelectTab("AddressesTabPage");

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress1.OA_PostCode = "210036";
			orgAddress1.OA_City = "LONDONDERRY";
			orgAddress1.OA_State = "NSW";
			orgAddress1.OA_RN_NKCountryCode = "AU";
			orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
			orgAddress1.OA_ValidationStatus = "INV";
			Factory.Save();

			FilterControl.FirePerformSearch();
			FilterControl.Grid.PerformMouseDownForTest(0, 1);
			AddressUserControl.AddressDetailControl.Address2Control.Focus();
			AddressUserControl.AddressDetailControl.Address2Control.Text = "ORGADDRESS1_ADDRESS2";
			AddressUserControl.AddressDetailControl.Address1Control.Focus();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AdministrationPanelForm.Close();
			AssertContains(@"This record has been modified.
Would you like to save the changes?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("The form visible is false", false, AdministrationPanelForm.Visible);

			var orgaddress = Factory.Load<OrgAddress>(orgAddress1.PK);
			AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1'", "ORGADDRESS1_ADDRESS1", orgaddress.OA_Address1);
			AssertEquals("The address2 should be 'ORGADDRESS1_ADDRESS2'", "ORGADDRESS1_ADDRESS2", orgaddress.OA_Address2);
		}

		public void TestFormShouldNotBeClosedWhenSaveFailed()
		{
			AdministrationPanelForm.Show();
			AdministrationPanelForm.SelectTab("AddressesTabPage");

			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress1.OA_PostCode = "210036";
			orgAddress1.OA_City = "LONDONDERRY";
			orgAddress1.OA_State = "NSW";
			orgAddress1.OA_RN_NKCountryCode = "AU";
			orgAddress1.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
			orgAddress1.OA_ValidationStatus = "INV";
			Factory.Save();

			FilterControl.FirePerformSearch();
			FilterControl.Grid.PerformMouseDownForTest(0, 1);
			AddressUserControl.AddressDetailControl.Address1Control.Focus();
			AddressUserControl.AddressDetailControl.Address1Control.Text = "";
			AddressUserControl.AddressDetailControl.Address2Control.Focus();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AdministrationPanelForm.Close();
			AssertEquals("There are errors that need to be corrected before this Address (ORGADDRESS1_ADDRESS1) can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("The form is not closed.", AdministrationPanelForm.Visible);
		}

		public void TestTabPageSecurity()
		{
			Env.Security.MdmAdministrationPanelAddressesEdit.IsAllowed = false;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = false;
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				AssertCoveringLabel(Env.Security.MdmAdministrationPanelAddressesEdit, adminForm.AddressesTabPage);
				AssertCoveringLabel(Env.Security.MdmAdministrationPanelDuplicatesEdit, adminForm.DuplicatesTabPage);
				AssertEquals(0, adminForm.DuplicatesTabPage.Controls.Find("AddressUserControl", false).Length);
				AssertEquals(0, adminForm.AddressesTabPage.Controls.Find("DuplicatesSubTabControl", false).Length);
			}

			Env.Security.MdmAdministrationPanelAddressesEdit.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				AssertEquals(0, adminForm.AddressesTabPage.Controls.Find("coveringLabel", false).Length);
				AssertEquals(0, adminForm.DuplicatesTabPage.Controls.Find("coveringLabel", false).Length);
				AssertEquals(1, adminForm.AddressesTabPage.Controls.Find("AddressUserControl", false).Length);
				AssertEquals(1, adminForm.DuplicatesTabPage.Controls.Find("DuplicatesSubTabControl", false).Length);
			}
		}

		[RequiresSTA]
		public void TestPersonIntelligenceSecurityShouldDeterminePersonsTabSecurity()
		{
			Env.Security.PersonIntelligence.IsAllowed = false;

			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;
			using (SystemDataRegistry.Instance.MdmAdministrationPanelPersonDuplicatesTabEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();
				AssertNull(adminForm.DeDupTabControl.GetTabPageByNameOrText("Persons"));
			}
		}

		public void TestClearDedupePanelAdvancedFilterCacheWhenFormClosed()
		{
			var memoryCache = MemoryCache.Default;
			var testCacheName = "TestCacheName";
			var testCacheKey = "DedupePanelAdvancedFilterCache_" + testCacheName;

			try
			{
				var assembly = Assembly.LoadFrom("Enterprise.MasterData.GUI.dll");
				var type = assembly.GetType("Enterprise.MasterData.GUI.DedupePanelAdvancedFilterHelper");
				var setCacheMethod = type.GetMethod("SetCache");
				setCacheMethod.Invoke(null, new[] { testCacheName, "123", false, setCacheMethod.GetParameters()[3].DefaultValue });

				using (var adminForm = new AdministrationPanelFormForTest(Factory))
				{
					adminForm.Show();

					var cache = memoryCache.Get(testCacheKey);
					AssertNotNull(cache);
				}

				AssertEquals(false, memoryCache.Contains(testCacheKey));
			}
			finally
			{
				if (memoryCache.Contains(testCacheKey))
				{
					memoryCache.Remove(testCacheKey);
				}
			}
		}

		public void TestPersistDedupePanelAdvancedFiltersValueWhenFormClosed()
		{
			var memoryCache = MemoryCache.Default;
			var testCacheName = "TestCacheName";
			var testCacheKey = "DedupePanelAdvancedFilterCache_" + testCacheName;
			var findQuery = new ZQuery(StmDataSchema.SD_Owner, Env.CurrentUser.PK);
			findQuery.AddToFilter(StmDataSchema.SD_Name, "MDMPersonDeduplicationPanelFiltersValue");

			try
			{
				var assembly = Assembly.LoadFrom("Enterprise.MasterData.GUI.dll");
				var type = assembly.GetType("Enterprise.MasterData.GUI.DedupePanelAdvancedFilterHelper");
				var setCacheMethod = type.GetMethod("SetCache");
				setCacheMethod.Invoke(null, new[] { testCacheName, "123", true, setCacheMethod.GetParameters()[3].DefaultValue });
				StmData stmData;

				using (var adminForm = new AdministrationPanelFormForTest(Factory))
				{
					adminForm.Show();

					var cache = memoryCache.Get(testCacheKey);
					stmData = Factory.LoadTop1<StmData>(findQuery);

					AssertNotNull("Cache should exist", cache);
					AssertNull("StmData not created yet", stmData);
				}

				stmData = Factory.LoadTop1<StmData>(findQuery);
				AssertNotNull("Form closed, StmData created", stmData);
				AssertEquals("Filters value persist successful", true, stmData.IsInDatabase);
			}
			finally
			{
				if (memoryCache.Contains(testCacheKey))
				{
					memoryCache.Remove(testCacheKey);
				}
			}
		}

		public void TestDispose()
		{
			if (!AdministrationPanelForm.IsDisposed)
			{
				AdministrationPanelForm.Dispose();
			}
			AssertEquals(AdministrationPanelForm.Controls.Count, 0);
		}

		public void TestAdministrationPanelToCheckNullReferencesSelectingTabControl()
		{
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();

				var mainTabControl = adminForm.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;

				adminForm.CopyCompanyInformationMenuItem = null;
				AssertNoExceptionThrown(mainTabControl.SelectNextTabPage);

				adminForm.CopyAddressInformationMenuItem = null;
				AssertNoExceptionThrown(mainTabControl.SelectNextTabPage);

				adminForm.SearchAddressOnlineMenuItem = null;
				AssertNoExceptionThrown(mainTabControl.SelectNextTabPage);

				adminForm.SearchCompanyOnlineMenuItem = null;
				AssertNoExceptionThrown(mainTabControl.SelectNextTabPage);
			}
		}

		public void TestAdministrationPanelToCheckNullReferencesSelectingTabControl_WhenEditAddressesSecurityIsNotGranted()
		{
			Env.Security.MdmAdministrationPanelAddressesEdit.IsAllowed = false;
			using (var adminForm = new AdministrationPanelFormForTest(Factory))
			{
				adminForm.Show();

				var mainTabControl = adminForm.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;

				mainTabControl.SelectTab(adminForm.DuplicatesTabPage);

				AssertNoExceptionThrown(() => mainTabControl.SelectTab(adminForm.AddressesTabPage));
			}
		}

		FilterStripBusinessObject FilterBusinessObject => FilterControl?.FilterBusinessObject;
		AddressesFilterControl FilterControl => AddressUserControl?.FilterControl;
		AddressUserControl AddressUserControl => AdministrationPanelForm.UserControl;
		AdministrationPanelFormForTest AdministrationPanelForm => form ?? (form = new AdministrationPanelFormForTest(Factory));
		AdministrationPanelFormForTest form;

		protected override void SetUp()
		{
			base.SetUp();

			SystemDataRegistry.Instance.MdmAdministrationPanelAddressTabEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.MdmAdministrationPanelDuplicatesTabEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FilterControl.Grid.Sort = AddressesFilterBusinessObject.AddressFilterConstants.AddressCode;
		}

		protected override void TearDown()
		{
			if (!AdministrationPanelForm.IsDisposed)
			{
				AdministrationPanelForm.Dispose();
			}
		}

		void AssertCoveringLabel(SecurityCheckpoint checkPoint, ZTabPage page)
		{
			var label = (ZLabel)page.Controls.Find("coveringLabel", false)[0];
			AssertEquals(DockStyle.Fill, label.Dock);
			AssertEquals(System.Drawing.ContentAlignment.MiddleCenter, label.TextAlign);
			AssertEquals(checkPoint.ErrorMessageForNotAllowed, label.Text);
		}

		void AssertMenuItemAndLinkLabelEnabled()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CopyInformation Menu Item should be shown", true, form.CopyCompanyInformationMenuItem.Visible);
				AssertEquals("CopyInformation Menu Item should be enabled", true, form.CopyCompanyInformationMenuItem.Enabled);

				AssertEquals("SearchOnline Menu Item should be shown", true, form.SearchCompanyOnlineMenuItem.Visible);
				AssertEquals("SearchOnline Menu Item should be enabled", true, form.SearchCompanyOnlineMenuItem.Enabled);

				AssertEquals("CopyInformation Link label should be shown", true, AddressUserControl.AddressDetailControl.CopyCompanyInfoLinkControl.Visible);
				AssertEquals("CopyInformation Link label should be enabled", true, AddressUserControl.AddressDetailControl.CopyCompanyInfoLinkControl.Enabled);

				AssertEquals("SearchOnline Link label should be shown", true, AddressUserControl.AddressDetailControl.SearchCompanyOnlineControl.Visible);
				AssertEquals("SearchOnline Link label should be enabled", true, AddressUserControl.AddressDetailControl.SearchCompanyOnlineControl.Enabled);

				AssertEquals("CopyInformation Menu Item should be shown", true, form.CopyAddressInformationMenuItem.Visible);
				AssertEquals("CopyInformation Menu Item should be enabled", true, form.CopyAddressInformationMenuItem.Enabled);

				AssertEquals("SearchOnline Menu Item should be shown", true, form.SearchAddressOnlineMenuItem.Visible);
				AssertEquals("SearchOnline Menu Item should be enabled", true, form.SearchAddressOnlineMenuItem.Enabled);

				AssertEquals("CopyInformation Link label should be shown", true, AddressUserControl.AddressDetailControl.CopyAddressInfoLinkControl.Visible);
				AssertEquals("CopyInformation Link label should be enabled", true, AddressUserControl.AddressDetailControl.CopyAddressInfoLinkControl.Enabled);

				AssertEquals("SearchOnline Link label should be shown", true, AddressUserControl.AddressDetailControl.SearchAddressOnlineControl.Visible);
				AssertEquals("SearchOnline Link label should be enabled", true, AddressUserControl.AddressDetailControl.SearchAddressOnlineControl.Enabled);
			});
		}

		void AssertCopyAndSearchLinkClickResults(ZMenuItem copyInfoMenu, ZMenuItem searchOnlineMenuItem, ZLinkLabel copyInfoLinkLabel, ZLinkLabel searchOnlineControl, string clipboardText, string lastUrlLaunched)
		{
			SafeClipboard.Clear();
			copyInfoLinkLabel.OnLinkClicked_Exposed(null);
			AssertEquals("The data should be copy into clipboard", clipboardText, SafeClipboard.GetText());

			WebUrlLauncher.ClearLastUrlLaunched();
			searchOnlineControl.OnLinkClicked_Exposed(null);
			AssertEquals("Expected online search URL: " + lastUrlLaunched, lastUrlLaunched, WebUrlLauncher.LastUrlLaunched);

			SafeClipboard.Clear();
			copyInfoMenu.PerformClick();
			AssertEquals("The data should be copy into clipboard", clipboardText, SafeClipboard.GetText());

			WebUrlLauncher.ClearLastUrlLaunched();
			searchOnlineMenuItem.PerformClick();
			AssertEquals("Expected online search URL: " + lastUrlLaunched, lastUrlLaunched, WebUrlLauncher.LastUrlLaunched);
		}

		void AssertCopyAndSearchLinkStatus(ZMenuItem copyInfoMenu, ResourceString copyInfoCaption, ZMenuItem searchOnlineMenuItem, ResourceString searchOnlineCaption, ZLinkLabel copyInfoLinkLabel, ZLinkLabel searchOnlineControl)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Copy Information Menu Item should be shown", true, copyInfoMenu.Visible);
				AssertEquals("Copy Information Menu Item's Caption should be " + copyInfoCaption, copyInfoCaption, copyInfoMenu.Caption);
				AssertEquals("Copy Information Menu Item should be disabled", false, copyInfoMenu.Enabled);

				AssertEquals("Search Online Menu Item should be shown", true, searchOnlineMenuItem.Visible);
				AssertEquals("Search Online Menu Item's Caption should be " + searchOnlineCaption, searchOnlineCaption, searchOnlineMenuItem.Caption);
				AssertEquals("Search Online Menu Item should be disabled", false, searchOnlineMenuItem.Enabled);

				AssertEquals("Copy Information Link label should be shown", true, copyInfoLinkLabel.Visible);
				AssertEquals("Copy Information Link label's Caption should be " + copyInfoCaption.Replace("&", ""), copyInfoCaption.Replace("&", ""), copyInfoLinkLabel.Text);
				AssertEquals("Copy Information Link label should be disabled", false, copyInfoLinkLabel.Enabled);

				AssertEquals("Search Online Link label should be shown", true, searchOnlineControl.Visible);
				AssertEquals("Search Online Link label's Caption should be " + searchOnlineCaption.Replace("&", ""), searchOnlineCaption.Replace("&", ""), searchOnlineControl.Text);
				AssertEquals("Search Online Link label should be disabled", false, searchOnlineControl.Enabled);
			});
		}

		void AssertMenuItemAndLinkLabelStatus(bool shouldEnabled, ZMenuItem copyInfoMenu, ResourceString copyInfoCaption, ZMenuItem searchOnlineMenuItem, ResourceString searchOnlineCaption)
		{
			CombineAssertions(() =>
			{
				AssertEquals("CopyInformation Menu Item should be shown", true, copyInfoMenu.Visible);
				AssertEquals("CopyInformation Menu Item's Caption should be " + copyInfoCaption, copyInfoCaption, copyInfoMenu.Caption);
				AssertEquals("CopyInformation Menu Item should be disabled", shouldEnabled, copyInfoMenu.Enabled);

				AssertEquals("SearchOnline Menu Item should be shown", true, searchOnlineMenuItem.Visible);
				AssertEquals("SearchOnline Menu Item's Caption should be " + searchOnlineCaption, searchOnlineCaption, searchOnlineMenuItem.Caption);
				AssertEquals("SearchOnline Menu Item should be disabled", shouldEnabled, searchOnlineMenuItem.Enabled);
			});
		}
	}
}
