using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbBranchForm))]
	sealed class GlbBranchFormTest : ZFormBasherTest
	{
		#region Tax Configuration
		public void TestTaxConfigurationsGrid()
		{
			var branch = Factory.New<GlbBranch>();
			var expectedListOfColumns = new[]
			{
				$"{AccTaxConfiguration.Schema.ETC_Code} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_Description} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_RN_NKCountry} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxAuthorityCode} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxSystemCode} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_Ledger} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxRealisationMethod} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_RecoveryMethod} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_ThresholdMethod} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_ThresholdAmount} (ZCalcEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxAmountRounding} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_LedgerControlAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_TaxControlAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_TaxExpenseAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_TaxPendingControlAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False"
			};

			using (var form = new GlbBranchForm(branch))
			{
				var grid = form.GetField("taxConfigurationsGrid") as ZGrid;
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible} IsUnavailable:{x.IsUnavailable}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestTaxConfigurationControlsAreVisibleOnBranchForm()
		{
			var helperMock = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			const string tabPageTaxConfig = "taxConfigurationTabPage";
			const string gridTaxConfig = "taxConfigurationsGrid";

			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			helperMock.Setup(x => x.IsBranchLevelTaxSystemConfigured(branch)).Returns(() => false);
			using (var form = new GlbBranchForm(branch))
			{
				form.Show();
				var taxConfigurationTabControlCount = form.Controls.Find(tabPageTaxConfig, true).Length;
				var taxConfigurationGridControlCount = form.Controls.Find(gridTaxConfig, true).Length;

				AssertEquals("Should not exists in form.", 0, taxConfigurationTabControlCount);
			}

			helperMock.Setup(x => x.IsBranchLevelTaxSystemConfigured(branch)).Returns(() => true);
			using (var form = new GlbBranchForm(branch))
			{
				form.Show();
				var taxConfigurationTabControlCount = form.Controls.Find(tabPageTaxConfig, true).Length;
				var taxConfigurationGridControlCount = form.Controls.Find(gridTaxConfig, true).Length;

				AssertEquals("taxConfigurationTabControlCount should be 1.", 1, taxConfigurationTabControlCount);
			}
		}

		#endregion

		protected override Form GetFormToBashCore()
		{
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();
			return new GlbBranchForm(branch);
		}

		#region Delete

		[RequiresSTA]
		public void TestDelete()
		{
			GlbBranch branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			Assert("Current Branch should not be null.", branch != null);

			using (GlbBranchFormForTest form = new GlbBranchFormForTest(branch))
			{
				form.RunDelete();
				branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				Assert("Current Branch could not be deleted.", branch != null);
			}
		}

		#endregion

		void AssertDepartmentGridContent(GlbBranchForm branchForm, Tuple<string, string>[] gridContent)
		{
			branchForm.Show();
			var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
			var grid = branchForm.Controls.Find("DepartmentsModuleButtonGrid", true)[0] as AccAllowedBranchDepartmentComboModuleButtonGrid;
			tabControl.SelectedIndex = 1;

			AssertEquals(gridContent.Length, grid.InnerGrid.VisibleRowCount);

			for (int rowNo = 0; rowNo < gridContent.Length; rowNo++)
			{
				var depCode = grid.InnerGrid[rowNo, 0];
				var depDes = grid.InnerGrid[rowNo, 1];

				AssertEquals(gridContent[rowNo].Item1, depCode);
				AssertEquals(gridContent[rowNo].Item2, depDes);
			}
		}

		#region Deactivate Branch

		public void TestDeactivatingBranchPrompts_WithNoServiceTaskAttached()
		{
			var branch = CreateBranchWithServiceTask(false);

			ShowFormAndDeactivate(branch);
			AssertNull("We shouldn't show anything when there is no attached service tasks", ZFormModaliser.LastFormShownDialogForTest);
		}

		[RequiresSTA]
		public void TestDeactivatingBranchPrompts_WithInactiveServiceTaskAttached()
		{
			var branch = CreateBranchWithServiceTask(true, false);

			ShowFormAndDeactivate(branch);
			AssertNull("We shouldn't show anything when there is no active service tasks", ZFormModaliser.LastFormShownDialogForTest);
		}

		[RequiresSTA]
		public void TestDeactivatingBranchPrompts_WithActiveServiceTaskAttached()
		{
			using var registry = SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var branch = CreateBranchWithServiceTask(true, true);

			ShowFormAndDeactivate(branch);
			AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		GlbBranch CreateBranchWithServiceTask(bool createServiceTask, bool isActiveServiceTask = false)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;

			if (createServiceTask)
			{
				var task = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task.S5_GB = branch.PK;
				task.S5_IsActive = isActiveServiceTask;
			}

			Factory.Save();

			return branch;
		}

		[RequiresSTA]
		public void TestDeactivatingBranchPrompts_WithInactiveStmServiceTaskAttached()
		{
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("111")))
			{
				var branch = CreateBranchWithStmServiceTask(false, "111");

				ShowFormAndDeactivate(branch);
				AssertNull("We shouldn't show anything when there is no active service tasks", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[RequiresSTA]
		public void TestDeactivatingBranchPrompts_WithActiveStmServiceTaskAttached()
		{
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("111")))
			{
				var branch = CreateBranchWithStmServiceTask(true, "111");

				ShowFormAndDeactivate(branch);
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		IClientHostedServiceAttributeProvider GetClientHostedServiceAttributeProviderMock(string serviceTaskCode)
		{
			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(o => o.Code == serviceTaskCode);
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(serviceTaskCode))
				.Returns(hostedServiceAttribute);
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttributes())
				.Returns(new[] { hostedServiceAttribute });

			return hostedServiceProviderMock.Object;
		}

		GlbBranch CreateBranchWithStmServiceTask(bool isActiveServiceTask, string serviceTaskCode)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;

			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.SST_ServiceTaskCode = serviceTaskCode;
			task.SST_GB_Branch = branch.PK;
			task.SST_Active = isActiveServiceTask;

			Factory.Save();

			return branch;
		}

		public void TestDeactivatingBranchPrompts_WithNoStaffAttached()
		{
			var branch = CreateBranchWithStaff(false);

			ShowFormAndDeactivate(branch);
			AssertNull("We shouldn't show anything when there is no attached staff.", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestDeactivatingBranchPrompts_WithInactiveStaffAttached()
		{
			var branch = CreateBranchWithStaff(true, false);

			ShowFormAndDeactivate(branch);
			AssertNull("We shouldn't show anything when there is no active staff.", ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestDeactivatingBranchPrompts_WithActiveStaffAttached()
		{
			var branch = CreateBranchWithStaff(true, true);

			ShowFormAndDeactivate(branch);
			AssertEquals("We should show the switching form when the user deactivates a branch with active staff.", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		GlbBranch CreateBranchWithStaff(bool createStaff, bool isActiveStaff = false)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;

			if (createStaff)
			{
				var task = Factory.NewWithValidTestData<GlbStaff>();
				task.GS_GB_HomeBranch = branch.PK;
				task.GS_IsActive = isActiveStaff;
			}

			Factory.Save();

			return branch;
		}

		static void ShowFormAndDeactivate(GlbBranch branch)
		{
			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				var isActiveCheckbox = (ZCheckBox)branchForm.Controls.Find("GB_IsActiveBoundCheckBox", true).Single();
				isActiveCheckbox.Checked = false;
			}
		}

		#endregion

		[RequiresSTA]
		public void TestAllowedDepartments()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (var branchForm = new GlbBranchForm(branch))
			{
				AssertDepartmentGridContent(branchForm, Array.Empty<Tuple<string, string>>());
			}

			var allowedDepartment = branch.AllowedDepartments.AddNew();
			allowedDepartment.AAB_GE_Department = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			using (var branchForm = new GlbBranchForm(branch))
			{
				AssertDepartmentGridContent(branchForm, new Tuple<string, string>[]
				{ new Tuple<string,string>(allowedDepartment.DepartmentCode, allowedDepartment.DepartmentDescription) });
			}
		}

		[RequiresSTA]
		public void TestAllowedDepartmentsDetach()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var allowedDepartment = branch.AllowedDepartments.AddNew();
			allowedDepartment.AAB_GE_Department = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			AssertEquals(1, branch.AllowedDepartments.Count);

			using (var branchForm = new GlbBranchForm(branch))
			{
				AssertDepartmentGridContent(branchForm, new Tuple<string, string>[] { new Tuple<string, string>(allowedDepartment.DepartmentCode, allowedDepartment.DepartmentDescription) });

				var grid = branchForm.Controls.Find("DepartmentsModuleButtonGrid", true)[0] as AccAllowedBranchDepartmentComboModuleButtonGrid;
				grid.SelectFirstRowIfOnlyRowInGrid();

				grid.DetachSelectedElement();
				AssertDepartmentGridContent(branchForm, Array.Empty<Tuple<string, string>>());

				branchForm.FireSaveButton();
			}

			AssertEquals(0, branch.AllowedDepartments.Count);
		}

		public void TestAllowedDepartmentsAttach()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			AssertEquals(0, branch.AllowedDepartments.Count);

			var attachedDepartment = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);

			using (var branchForm = new GlbBranchForm(branch))
			{
				AssertDepartmentGridContent(branchForm, Array.Empty<Tuple<string, string>>());

				var grid = branchForm.Controls.Find("DepartmentsModuleButtonGrid", true)[0] as AccAllowedBranchDepartmentComboModuleButtonGrid;

				var attacher = new AccAllowedBranchDepartmentComboModuleAttacher(branch.AllowedDepartments, new GlbDepartmentCollection(Factory), ModuleIDs.GlbDepartment);
				attacher.Show(branchForm);

				attacher.LastShownAttachPopupForTesting.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { attachedDepartment });

				AssertDepartmentGridContent(branchForm, new Tuple<string, string>[] { new Tuple<string, string>(attachedDepartment.GE_Code, attachedDepartment.GE_Desc) });
				attacher.LastShownAttachPopupForTesting.Dispose();
				branchForm.FireSaveButton();
			}

			AssertEquals(1, branch.AllowedDepartments.Count);
			AssertEquals(branch.AllowedDepartments[0].AAB_GE_Department, attachedDepartment.PK);
		}

		[RequiresSTA]
		public void TestPhoneNumberControls()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			using (var branchForm = new GlbBranchFormForTest(branch))
			{
				Assert(!branchForm.PhoneNumberControl.ShowToolTip);
				Assert(branchForm.PhoneNumberControl.ShowDiallerControl);
				Assert(branchForm.PhoneNumberControl.ShowLocalNumberLabel);
				Assert(!branchForm.PhoneNumberControl.ShowPublishedCheckBox);
				Assert(branchForm.PhoneNumberControl.EnableValidStateColor);

				Assert(!branchForm.InternalExtensionNumberControl.ShowToolTip);
				Assert(!branchForm.InternalExtensionNumberControl.ShowDiallerControl);
				Assert(!branchForm.InternalExtensionNumberControl.ShowLocalNumberLabel);
				Assert(!branchForm.InternalExtensionNumberControl.ShowPublishedCheckBox);
				Assert(!branchForm.InternalExtensionNumberControl.EnableValidStateColor);

				Assert(!branchForm.FaxNumberControl.ShowToolTip);
				Assert(!branchForm.FaxNumberControl.ShowDiallerControl);
				Assert(!branchForm.FaxNumberControl.ShowLocalNumberLabel);
				Assert(!branchForm.FaxNumberControl.ShowPublishedCheckBox);
				Assert(branchForm.FaxNumberControl.EnableValidStateColor);
			}
		}

		#region PortsTabControl

		public void TestAdditionalRelatedPortsTab()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var extraPort1 = branch.ExtraPorts.AddNew();
			extraPort1.GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";
			var extraPort2 = branch.ExtraPorts.AddNew();
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";

			AssertEquals(2, branch.ExtraPorts.Count);

			using (var branchForm = new GlbBranchForm(branch))
			{
				AssertPortsTabControlGridContent(branchForm, "AdditionalPortsTabPage", "ExtraPortsGrid", 0, new[,]
				{
					{ "AUSYD", "Sydney" },
					{ "AUMEL", "Melbourne" },
					{ "", "" }
				});
				branchForm.FireSaveButton();

				AssertEquals(2, branch.ExtraPorts.Count);
			}
		}

		public void TestDefaultPortsTab()
		{
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var extraPort1 = branch.ExtraPorts.AddNew();
			extraPort1.GY_RL_NKAdditionalBranchRelatedPort = "AUSYD";
			var extraPort2 = branch.ExtraPorts.AddNew();
			extraPort2.GY_RL_NKAdditionalBranchRelatedPort = "AUMEL";
			var defaultPort1 = branch.DefaultPorts.AddNew();
			defaultPort1.GBP_DefaultTo = "CONDIS";
			defaultPort1.GBP_TransportMode = "AIR";
			defaultPort1.GBP_ContainerMode = "ALL";
			defaultPort1.GBP_RL_NKPort = "AUSYD";
			var defaultPort2 = branch.DefaultPorts.AddNew();
			defaultPort2.GBP_DefaultTo = "CONLOA";
			defaultPort2.GBP_TransportMode = "SEA";
			defaultPort2.GBP_ContainerMode = "FCL";
			defaultPort2.GBP_RL_NKPort = "AUMEL";

			AssertEquals(2, branch.ExtraPorts.Count);
			AssertEquals(2, branch.DefaultPorts.Count);

			using (var branchForm = new GlbBranchForm(branch))
			{
				AssertPortsTabControlGridContent(branchForm, "DefaultPortsTabPage", "DefaultPortsGrid", 1, new[,]
				{
					{ "CONDIS", "AIR", "ALL", "AUSYD" },
					{ "CONLOA", "SEA", "FCL", "AUMEL" },
					{ "", "", "", "" }
				});
				branchForm.FireSaveButton();

				AssertEquals(2, branch.DefaultPorts.Count);
			}
		}

		void AssertPortsTabControlGridContent(GlbBranchForm branchForm, string tabName, string gridName, int tabIndex, string[,] gridContent)
		{
			branchForm.Show();
			var tabControl = branchForm.Controls.Find("PortsTabControl", true)[0] as ZTemplateTabControl;
			AssertNotNull(tabControl);
			var tabPage = tabControl.Controls.Find(tabName, true)[0] as ZTabPage;
			AssertNotNull(tabPage);
			var grid = tabPage.Controls.Find(gridName, true)[0] as ZGrid;
			AssertNotNull(grid);
			tabControl.SelectedIndex = tabIndex;

			AssertEquals(gridContent.GetLength(0), grid.VisibleRowCount);
			AssertEquals(gridContent.GetLength(1), grid.Columns.Count);

			for (int rowNo = 0; rowNo < gridContent.GetLength(0); rowNo++)
			{
				for (int columnNo = 0; columnNo < gridContent.GetLength(1); columnNo++)
				{
					AssertEquals(gridContent[rowNo, columnNo], grid[rowNo, columnNo]);
				}
			}
		}

		#endregion

		#region Branch Credentials

		public void TestBranchCredentialTab_TaxCore()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia)) // a country that does not have branch credentials
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				using (var branchForm = new GlbBranchForm(branch))
				{
					branchForm.Show();
					var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
					var eInvoicingCredentialTabPages = tabControl.Controls.Find("EInvoicingCredentialTaxCoreTabPage", true);
					AssertEquals(nameof(eInvoicingCredentialTabPages.Length), 0, eInvoicingCredentialTabPages.Length);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Fiji))
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				using (var branchForm = new GlbBranchForm(branch))
				{
					branchForm.Show();
					var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
					var eInvoicingCredentialTaxCoreTabPages = tabControl.Controls.Find("EInvoicingCredentialTaxCoreTabPage", true);
					AssertEquals(nameof(eInvoicingCredentialTaxCoreTabPages.Length), 1, eInvoicingCredentialTaxCoreTabPages.Length);
					var eInvoicingCredentialTaxCoreTabPage = eInvoicingCredentialTaxCoreTabPages[0] as ZTabPage;
					AssertEquals(nameof(eInvoicingCredentialTaxCoreTabPage.TabVisible), true, eInvoicingCredentialTaxCoreTabPage.TabVisible);
					var taxCoreEInvoicingCertificateControls = eInvoicingCredentialTaxCoreTabPage.Controls.Find("EInvoicingCertificateTaxCoreUserControl", true);
					AssertEquals(nameof(taxCoreEInvoicingCertificateControls.Length), 1, taxCoreEInvoicingCertificateControls.Length);
					var taxCoreEInvoicingCertificateControl = taxCoreEInvoicingCertificateControls[0];
					AssertType<GlbBranchForm_CredentialUserControl>("Control should be for TaxCore credentials", taxCoreEInvoicingCertificateControl);
				}
			}
		}

		public void TestBranchCredentialTab_India()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia)) // a country that does not have branch credentials
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				using (var branchForm = new GlbBranchForm(branch))
				{
					branchForm.Show();
					var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
					var branchCredentialTabPages = tabControl.Controls.Find("BranchCredentialTabPage", true);
					AssertEquals(nameof(branchCredentialTabPages.Length), 0, branchCredentialTabPages.Length);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				using (var branchForm = new GlbBranchForm(branch))
				{
					branchForm.Show();
					var tabControl = branchForm.Controls.Find("BranchTabControl", true)[0] as ZTemplateTabControl;
					var branchCredentialIndiaTabPages = tabControl.Controls.Find("BranchCredentialIndiaTabPage", true);
					AssertEquals(nameof(branchCredentialIndiaTabPages.Length), 1, branchCredentialIndiaTabPages.Length);
					var branchCredentialIndiaTabPage = branchCredentialIndiaTabPages[0] as ZTabPage;
					AssertEquals(nameof(branchCredentialIndiaTabPage.TabVisible), true, branchCredentialIndiaTabPage.TabVisible);
					var indiaBranchCredentialsControls = branchCredentialIndiaTabPage.Controls.Find("BranchCredentialIndiaUserControl", true);
					AssertEquals(nameof(indiaBranchCredentialsControls.Length), 1, indiaBranchCredentialsControls.Length);
					var indiaBranchCredentialsControl = indiaBranchCredentialsControls[0];
					AssertType<GlbBranchForm_IndiaCredentialUserControl>("Control should be for India branch credentials", indiaBranchCredentialsControl);
				}
			}
		}

		#endregion

		#region EInvoicing Credentials

		[RequiresSTA]
		public void TestEInvoicingCredentialsTab_IsNotVisible_WhenNullSettings()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCredentialSettings>(countryCode: "ZZ");
			var branch = CreateBranchAndCompanyForTest(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsNotVisible(branchForm);
			}
		}

		public void TestEInvoicingCredentialsTab_IsNotVisible_WhenNoBranchCompany()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = Guid.Empty;

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsNotVisible(branchForm);
			}
		}

		[RequiresSTA]
		public void TestEInvoicingCredentialsTab_IsNotVisible_WhenConfiguredForDifferentCountry()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: Core.Constants.CountryCodes.Australia);
			var branch = CreateBranchAndCompanyForTest(Core.Constants.CountryCodes.India);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsNotVisible(branchForm);
			}
		}

		public void TestEInvoicingCredentialsTab_IsNotVisible_WhenConfiguredForCompanyCountry()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForCompany<IEInvoicingCertificateCredentialSettings>(countryCode: GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var branch = CreateBranchAndCompanyForTest(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsNotVisible(branchForm);
			}
		}

		public void TestEInvoicingCredentialsTab_IsVisible_WhenConfiguredForBranchCountry()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var branch = CreateBranchAndCompanyForTest(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsVisible(branchForm);
			}
		}

		[RequiresSTA]
		public void TestEInvoicingCredentialsTab_IsVisible_WhenInDifferentLoginCompany()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: Core.Constants.CountryCodes.India);
			var branch = CreateBranchAndCompanyForTest(Core.Constants.CountryCodes.India);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsVisible(branchForm);
			}
		}

		[RequiresSTA]
		public void TestEInvoicingCertificateControl_IsVisible_WhenBranchCountryConfiguredForCertificates()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingCertificateCredentialSettings>(countryCode: Core.Constants.CountryCodes.India);
			var branch = CreateBranchAndCompanyForTest(Core.Constants.CountryCodes.India);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsVisible(branchForm);
				AssertEInvoicingCertificateControlVisiblity(branchForm, expectedIsVisible: true);
			}
		}

		public void TestEInvoicingCertificateControl_IsNotVisible_WhenBranchCountryConfiguredForPasswords()
		{
			EInvoicingSettingsHelper.CreateAndHookSettingsMockForBranch<IEInvoicingPasswordCredentialSettings>(countryCode: Core.Constants.CountryCodes.India);
			var branch = CreateBranchAndCompanyForTest(Core.Constants.CountryCodes.India);

			using (var branchForm = new GlbBranchForm(branch))
			{
				branchForm.Show();
				AssertEInvoicingCredentialTabIsVisible(branchForm);
				AssertEInvoicingCertificateControlVisiblity(branchForm, expectedIsVisible: false);
			}
		}

		#region Helpers

		static void AssertEInvoicingCredentialTabIsNotVisible(GlbBranchForm branchForm)
		{
			var tabControl = (ZTemplateTabControl)branchForm.Controls.Find("BranchTabControl", true)[0];
			var credentialTabPages = tabControl.Controls.Find("EInvoicingCredentialTabPage", true);
			AssertEquals(nameof(credentialTabPages.Length), 0, credentialTabPages.Length);
		}

		static void AssertEInvoicingCredentialTabIsVisible(GlbBranchForm branchForm)
		{
			var tabControl = (ZTemplateTabControl)branchForm.Controls.Find("BranchTabControl", true)[0];
			var credentialTabPages = tabControl.Controls.Find("EInvoicingCredentialTabPage", true);
			AssertEquals(nameof(credentialTabPages.Length), 1, credentialTabPages.Length);
			var credentialTabPage = (ZTabPage)credentialTabPages[0];
			AssertEquals(nameof(credentialTabPage.TabVisible), true, credentialTabPage.TabVisible);
		}

		static void AssertEInvoicingCertificateControlVisiblity(GlbBranchForm branchForm, bool expectedIsVisible)
		{
			var tabControl = (ZTemplateTabControl)branchForm.Controls.Find("BranchTabControl", true)[0];
			var credentialTabPage = (ZTabPage)tabControl.Controls.Find("EInvoicingCredentialTabPage", true)[0];
			tabControl.SelectTab(credentialTabPage);
			var certificateControls = credentialTabPage.Controls.Find("EInvoicingCertificateCredentialUserControl", true);
			AssertEquals(1, certificateControls.Length);
			var certificateControl = certificateControls[0];
			AssertNotNull(certificateControl);
			AssertType<EInvoicingCertificateUserControl>("X509 Certificate Control is expected", certificateControl);
			AssertEquals(nameof(certificateControl.Visible), expectedIsVisible, certificateControl.Visible);
		}

		GlbBranch CreateBranchAndCompanyForTest(string countryCode)
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			branch.Company.GC_RN_NKCountryCode = countryCode;
			return branch;
		}

		#endregion

		#endregion

		[RequiresSTA]
		public void TestValidateAddressButton()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var branch = Factory.New<GlbBranchForTest>();
			branch.GB_Address1 = "xxxx";
			branch.GB_RN_NKCountryCode = "AU";
			branch.GB_State = "AST";
			branch.GB_City = "Sydney";
			using (var form = new GlbBranchForm(branch))
			{
				form.Show();

				form.ValidateButton.PerformClick();
				var suggestionControl = form.FindSingleOrDefault<AddressSuggestionControl>("AddressSuggestionControl");
				AssertNotNull("AddressSuggestionControl", suggestionControl);
				var infoLabel = suggestionControl.FindSingleOrDefault<ZLabel>("InfoLabel");
				AssertNotNull("InfoLabel", infoLabel);
				AssertEquals("No suggestions have been found for the address that you entered. Please confirm as original or amend the address to receive suggestions.", infoLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestNoCreatedChangesNotificationExceptionThrown_WhenOnLoad()
		{
			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var branch = Factory.NewWithValidTestData<GlbBranchForTest>();
				branch.GB_Address1 = "xxxx";
				branch.GB_RN_NKCountryCode = "AU";
				branch.GB_State = "AST";
				branch.GB_City = "Sydney";
				branch.ValidationStatus = AddressValidationStatus.ToBeVerified;
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (TestingState.SuspendIsRunningTests())
					using (var form = new GlbBranchFormForTest(branch))
					{
						form.Show();
						Assert("Change should not happen on the branch when load", !branch.HasChanges);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Validation status has been changed to INV.", AddressValidationStatus.Invalid, branch.ValidationStatus);
					}
				});
			}
		}

		class GlbBranchForTest : GlbBranch, ISupportWebAddressValidation
		{
			public GlbBranchForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
			{
				ValidationStatus = AddressValidationStatus.Invalid;
				return Task.FromResult(new WebAddressValidationResult());
			}
		}
	}
}
