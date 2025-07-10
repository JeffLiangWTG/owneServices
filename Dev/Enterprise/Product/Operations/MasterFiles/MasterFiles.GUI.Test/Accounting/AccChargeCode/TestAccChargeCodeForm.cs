using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccChargeCodeForm))]
	class TestAccChargeCodeForm : ZFormBasherTest
	{
		public void TestSaveToRecentItemsOnlyForCurrentCompany()
		{
			var chargeCodeOfCurrentCompany = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeOfCurrentCompany.AC_GC = GlbCompany.CurrentCompany.PK;

			var chargeCodeOfOtherCompany = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeOfOtherCompany.AC_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			Factory.Save();

			using (var form = new AccChargeCodeForm(chargeCodeOfOtherCompany))
			{
				form.Show();
				form.ControllerID = ControllerIDs.AccChargeCode;
				Application.DoEvents();

				var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
				AssertEquals("Charge Code of other company should not be saved to recent items.", false, RecentItemManager.Instance.IsInRecentItems(linkWrapper.ModuleName, linkWrapper));
			}

			using (var form = new AccChargeCodeForm(chargeCodeOfCurrentCompany))
			{
				form.Show();
				form.ControllerID = ControllerIDs.AccChargeCode;
				Application.DoEvents();

				var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(form);
				AssertEquals("Charge Code of current company should be saved to recent items.", true, RecentItemManager.Instance.IsInRecentItems(linkWrapper.ModuleName, linkWrapper));
			}
		}

		public void TestCharacterCasingForChargeCodeDescription()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				var descControl = (ZTranslatableTextControl)form.Controls.Find("descriptionZTranslatableTextControl", true).First();
				AssertNotNull(descControl);
				AssertEquals(CharacterCasing.Normal, descControl.CharacterCasing);
			}
		}

		#region Test Mapping Controls Visibility

		[RequiresSTA]
		public void TestIATAChargeCodeMappingControlsVisibility()
		{
			bool initialValue = Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen;

			try
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = false;
				AssertMappingControlsVisibility(false, "AC_IATA_ChargeCodeMapDropEdit", "ShowChargeCodeForOtherChargesInHAWBScreen registry item");

				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;
				AssertMappingControlsVisibility(true, "AC_IATA_ChargeCodeMapDropEdit", "ShowChargeCodeForOtherChargesInHAWBScreen registry item");
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = initialValue;
			}
		}

		[RequiresSTA]
		public void TestShowPreSaveDialog()
		{
			AccChargeCode chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));

			using (AccChargeCodeForm form = new AccChargeCodeForm(chargeCode1))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				chargeCode1.AC_ChargeOtherGroups = "PRC";
				chargeCode1.AC_IsActive = false;

				form.FireSaveButton();
				AssertEquals("Msg shown about invoice order setups will be deleted", "This charge code will be removed from all existing charge code sequence setups. Proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			GC.Collect();
		}

		#endregion

		[RequiresSTA]
		public void TestWarningIsShownIfGlAccountSetupChanged()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AT_GSTRate = Factory.LoadTop1<AccTaxRate>(new ZQuery()).PK;
			chargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			chargeCode.AC_ChargeType = "MRG";
			chargeCode.AC_Desc = "my test charge code";
			chargeCode.AC_AG_AccrualAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			chargeCode.AC_AG_CostAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			chargeCode.AC_AG_RevenueAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			chargeCode.AC_AG_WIPAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();

			using (AccChargeCodeForm form = new AccChargeCodeForm(chargeCode))
			{
				chargeCode.AC_AG_AccrualAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Message should be shown", @"This GL Account change will be used when posting new & future transactions.  
Transactions already posted under the previous configuration will remain unchanged. Those transactions will remain posted against the previous GL account configuration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestShowAO_CustomsStatusColumnOnlyFroEUContries()
		{
			AccChargeCodeForm testForm;
			ZTemplateTabControl testTabControl;
			ZGrid testGrid;
			AccChargeCode chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));

			using (testForm = new AccChargeCodeForm(chargeCode1))
			{
				testForm.Show();

				testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
				AssertNotNull("Precondition:", testTabControl);
				testTabControl.SelectTab("TaxOverridesTabPage");
				Application.DoEvents();

				testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
				AssertNotNull("Precondition:", testGrid);
				AssertColumnExist("AO_CustomsStatus", testGrid, false);
			}

			RefCountry countryDE = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryDE.Code;

				using (testForm = new AccChargeCodeForm(chargeCode1))
				{
					testForm.Show();

					testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
					AssertNotNull("Precondition:", testTabControl);
					testTabControl.SelectTab("TaxOverridesTabPage");
					Application.DoEvents();

					testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
					AssertNotNull("Precondition:", testGrid);
					AssertColumnExist("AO_CustomsStatus", testGrid, true);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		[RequiresSTA]
		public void TestShowAO_SplitPaymentVATOrganisationColumnOnlyForItaly()
		{
			AccChargeCodeForm testForm;
			ZTemplateTabControl testTabControl;
			ZGrid testGrid;
			AccChargeCode chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				RefCountry countryDE = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryDE.Code;
				using (testForm = new AccChargeCodeForm(chargeCode1))
				{
					testForm.Show();

					testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
					AssertNotNull("Precondition:", testTabControl);
					testTabControl.SelectTab("TaxOverridesTabPage");
					Application.DoEvents();

					testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
					AssertNotNull("Precondition:", testGrid);
					AssertColumnExist("AO_SplitPaymentVATOrganisation", testGrid, false);
				}

				RefCountry countryIT = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Italy);
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryIT.Code;
				using (testForm = new AccChargeCodeForm(chargeCode1))
				{
					testForm.Show();

					testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
					AssertNotNull("Precondition:", testTabControl);
					testTabControl.SelectTab("TaxOverridesTabPage");
					Application.DoEvents();

					testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
					AssertNotNull("Precondition:", testGrid);
					AssertColumnExist("AO_SplitPaymentVATOrganisation", testGrid, true);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		[RequiresSTA]
		public void TestShowAO_GBColumn()
		{
			AccChargeCodeForm testForm;
			ZTemplateTabControl testTabControl;
			ZGrid testGrid;
			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (testForm = new AccChargeCodeForm(chargeCode))
			{
				testForm.Show();

				testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
				AssertNotNull("Precondition:", testTabControl);
				testTabControl.SelectTab("TaxOverridesTabPage");
				Application.DoEvents();

				testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
				AssertNotNull("Precondition:", testGrid);
				AssertColumnExist("AO_GB", testGrid, false);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (testForm = new AccChargeCodeForm(chargeCode))
			{
				testForm.Show();

				testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
				AssertNotNull("Precondition:", testTabControl);
				testTabControl.SelectTab("TaxOverridesTabPage");
				Application.DoEvents();

				testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
				AssertNotNull("Precondition:", testGrid);
				AssertColumnExist("AO_GB", testGrid, true);
			}
		}

		[RequiresSTA]
		public void TestTaxOverridesGridShowColumnAO_SupplyType()
		{
			AssertByRegistry(true);
			AssertByRegistry(false);

			void AssertByRegistry(bool enableRegisrty)
			{
				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BAF"));
				AssertNotNull("Precondition:", chargeCode);

				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegisrty))
				using (var testForm = new AccChargeCodeForm(chargeCode))
				{
					testForm.Show();

					var testTabControl = (ZTemplateTabControl)testForm.Controls["ChargeCodeTabControl"];
					AssertNotNull("Precondition:", testTabControl);
					testTabControl.SelectTab("TaxOverridesTabPage");

					var testGrid = (ZGrid)testTabControl.SelectedTab.Controls["TaxOverridesGrid"];
					AssertNotNull("Precondition:", testGrid);
					AssertColumnExist("AO_SupplyType", testGrid, enableRegisrty);
				}
			}
		}

		public virtual void TestDisplayPolicyAffect()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			using (Form form = GetFormToBash())
			{
				form.Show();
				var aC_AT_GSTRateBoundGuidFindBox = (ZGuidFindBox)(typeof(AccChargeCodeForm).GetField("AC_AT_GSTRateBoundGuidFindBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
				AssertEquals("GST Rate is visible", true, aC_AT_GSTRateBoundGuidFindBox.Visible);
				var inputGSTVATRecoverableCalcEdit = (ZCalcEdit)(typeof(AccChargeCodeForm).GetField("InputGSTVATRecoverableCalcEdit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
				AssertEquals("GST Recoverable is visible", true, inputGSTVATRecoverableCalcEdit.Visible);
				var aC_AW_WithholdingTaxRateBoundGuidFindBox = (ZGuidFindBox)(typeof(AccChargeCodeForm).GetField("AC_AW_WithholdingTaxRateBoundGuidFindBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
				AssertEquals("Witholding Rate is visible", true, aC_AW_WithholdingTaxRateBoundGuidFindBox.Visible);
				var taxOverridesTabPage = (ZTabPage)(typeof(AccChargeCodeForm).GetField("TaxOverridesTabPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
				AssertEquals("Tax Overrides Tab is visible", true, taxOverridesTabPage.TabVisible);
				var branchOverridesTab = (ZTabPage)(typeof(AccChargeCodeForm).GetField("BranchOverridesTab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
				AssertEquals("Branch Overrides Tab is visible", true, branchOverridesTab.TabVisible);
				var creditorOverridesTab = (ZTabPage)(typeof(AccChargeCodeForm).GetField("CreditorOverridesTab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
				AssertEquals("Creditor Overrides Tab is visible", true, creditorOverridesTab.TabVisible);
				creditorOverridesTab.Show();
				var creditorOverridesGrid = creditorOverridesTab.FindSingle<ZGrid>("CreditorOverridesGrid");
				AssertEquals("ACC_PaymentTerm", true, creditorOverridesGrid.GetColumnStyle("ACC_PaymentTerm").IsVisible);
				AssertEquals("ACC_CreditorRole", true, creditorOverridesGrid.GetColumnStyle("ACC_CreditorRole").IsVisible);
			}
		}

		public virtual void TestGovtAccChargeCodeDisplayPolicyAffect()
		{
			foreach (bool regValue in new bool[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_GovtChargeCode = regValue ? "Test" : string.Empty;
					using (Form form = new AccChargeCodeForm(chargeCode))
					{
						form.Show();
						var govtChargeCodeTextBox = (ZTextBox)(typeof(AccChargeCodeForm).GetField("GovtChargeCodeTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
						AssertEquals("Govt. Charge Code is visible", regValue, govtChargeCodeTextBox.Visible);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestAirlineIATACodeTabDisplayPolicyAffect()
		{
			var initialValue = Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen;

			try
			{
				foreach (bool regValue in new bool[] { true, false })
				{
					Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = regValue;
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					using (var form = new AccChargeCodeForm(chargeCode))
					{
						form.Show();
						var airlineIATACodeTab = (ZTabPage)(typeof(AccChargeCodeForm).GetField("AirlineIATACodeTab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
						AssertEquals("AirlineIATACodeTab visible", regValue, airlineIATACodeTab.TabVisible);
					}
				}
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = initialValue;
			}
		}

		[RequiresSTA]
		public void TestAccChargeCodeCarrierIataMappingsBinding()
		{
			var initialValue = Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen;

			try
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = true;
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();
					var airlineIATACodeTab = (ZTabPage)(typeof(AccChargeCodeForm).GetField("AirlineIATACodeTab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
					AssertEquals("AirlineIATACodeTab visible", true, airlineIATACodeTab.TabVisible);

					airlineIATACodeTab.Show();
					var grid = airlineIATACodeTab.Controls["AirlineIATACodeGrid"] as ZGrid;
					AssertNotNull("Precondition", grid);

					foreach (var columnName in new[]
					{
						"ACI_OH_Carrier",
						"AirLine2CharCode",
						"AirLineName",
						"ACI_IATAChargeCodeMap"
					})
					{
						AssertColumnExist(columnName, grid, true);
					}
				}
			}
			finally
			{
				Env.Registry.Freight.AirWaybill.ShowChargeCodeForOtherChargesInHAWBScreen = initialValue;
			}
		}

		public void TestInputGSTVATRecoverableCalcEdit_ShowByDefault()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			using (Form form = GetFormToBash())
			{
				form.Show();
				var inputGSTVATRecoverableCalcEdit = form.Controls.Find("InputGSTVATRecoverableCalcEdit", true)[0];
				var expectedValue = !(form is AccGlobalChargeCodeForm);
				AssertEquals("GST Recoverable is visible", expectedValue, inputGSTVATRecoverableCalcEdit.Visible);
			}
		}

		[RequiresSTA]
		public virtual void TestActionsMenu()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			using (var form = new AccChargeCodeForm(normalChargeCodeLinked))
			{
				AssertGlobalDifferencesAvailableAndWorking(form);
			}

			using (var form = new AccChargeCodeForm(normalChargeCode))
			{
				form.Show();
				var actionsMenuItem = MasterFilesTestHelper.GetNonPublicValue<MenuItem>("ActionsMenuItem", form);
				var menuItemShowDiff = actionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(m => m.Text == "Show All Global Differences");
				Assert("There should be no 'Show All Global Differences' menu item", menuItemShowDiff == null);
				AssertEquals("Don't Show All Global Differences by default", false, form.ShowDifferenceWarnings);
				AssertEquals("Don't Show All Global Differences by default", false, ((AccChargeCode)form.BusinessEntity).ShowDifferenceWarnings);
			}
		}

		protected static void AssertGlobalDifferencesAvailableAndWorking(AccChargeCodeForm form)
		{
			form.Show();
			var actionsMenuItem = MasterFilesTestHelper.GetNonPublicValue<MenuItem>("ActionsMenuItem", form);
			var menuItemShowDiff = actionsMenuItem.MenuItems.Cast<MenuItem>().FirstOrDefault(m => m.Text == "Show All Global Differences");
			Assert("There should be 'Show All Global Differences' menu item", menuItemShowDiff != null);
			AssertEquals("Don't Show All Global Differences by default", false, form.ShowDifferenceWarnings);
			AssertEquals("Don't Show All Global Differences by default", false, menuItemShowDiff.Checked);
			AssertEquals("Don't Show All Global Differences by default", false, ((AccChargeCode)form.BusinessEntity).ShowDifferenceWarnings);
			menuItemShowDiff.PerformClick();
			AssertEquals("Now show differences", true, form.ShowDifferenceWarnings);
			AssertEquals("Now show differences", true, menuItemShowDiff.Checked);
			AssertEquals("Now show differences", true, ((AccChargeCode)form.BusinessEntity).ShowDifferenceWarnings);
			menuItemShowDiff.PerformClick();
			AssertEquals("Now do not show differences", false, form.ShowDifferenceWarnings);
			AssertEquals("Now do not show differences", false, menuItemShowDiff.Checked);
			AssertEquals("Now do not show differences", false, ((AccChargeCode)form.BusinessEntity).ShowDifferenceWarnings);
		}

		[RequiresSTA]
		public virtual void TestFormCaptionForOtherCompanyChargeCode()
		{
			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.Equal, "SIN"));
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			AssertEquals("Precondition, charge code defaults to current company.", GlbCompany.CurrentCompany.PK, chargeCode.AC_GC);
			AssertNotEquals("Precondition, otherCompany different.", GlbCompany.CurrentCompany.PK, otherCompany.PK);

			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				AssertEquals("Caption normal for same company charge code", "New Charge Code", form.Text);
			}

			chargeCode.AC_GC = otherCompany.PK;

			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				AssertEquals("Special caption when in different company", "New Charge Code for company SIN (Eagle Datamation International Pte Ltd)", form.Text);
			}
		}

		public void TestAccChargeCodeForm_EnableBulkDisbursementJobsClosure()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			mock.Setup(m => m.EnableBulkDisbursementJobsClosure).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();
					Application.DoEvents();
					var detailsDisbursementSurplusAccountGuidFindBox = form.FindSingleOrDefault<ZGuidFindBox>("DetailsDisbursementSurplusAccountGuidFindBox");
					var detailsDisbursementShortfallAccountGuidFindBox = form.FindSingleOrDefault<ZGuidFindBox>("DetailsDisbursementShortfallAccountGuidFindBox");
					var glAccountSetupGroupBox = form.FindSingleOrDefault<ZGroupBox>("GLAccountSetupGroupBox");
					var autoRatingGroupBox = form.FindSingleOrDefault<ZGroupBox>("AutoRatingGroupBox");

					AssertEquals(true, detailsDisbursementSurplusAccountGuidFindBox.Visible);
					AssertEquals(true, detailsDisbursementShortfallAccountGuidFindBox.Visible);
					AssertEquals(ControlDpiScalingHelper.NewScaledSize(653, 260, true), glAccountSetupGroupBox.Size);
					AssertEquals(ControlDpiScalingHelper.NewScaledPoint(1, 363, true), autoRatingGroupBox.Location);
				}
			}
		}

		[RequiresSTA]
		public void TestAccChargeCodeForm_DisableBulkDisbursementJobsClosure()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			mock.Setup(m => m.EnableBulkDisbursementJobsClosure).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();
					Application.DoEvents();
					var detailsDisbursementSurplusAccountGuidFindBox = form.FindSingleOrDefault<ZGuidFindBox>("DetailsDisbursementSurplusAccountGuidFindBox");
					var detailsDisbursementShortfallAccountGuidFindBox = form.FindSingleOrDefault<ZGuidFindBox>("DetailsDisbursementShortfallAccountGuidFindBox");
					var glAccountSetupGroupBox = form.FindSingleOrDefault<ZGroupBox>("GLAccountSetupGroupBox");
					var autoRatingGroupBox = form.FindSingleOrDefault<ZGroupBox>("AutoRatingGroupBox");

					AssertEquals(false, detailsDisbursementSurplusAccountGuidFindBox.Visible);
					AssertEquals(false, detailsDisbursementShortfallAccountGuidFindBox.Visible);
					AssertEquals(ControlDpiScalingHelper.NewScaledSize(653, 210, true), glAccountSetupGroupBox.Size);
					AssertEquals(ControlDpiScalingHelper.NewScaledPoint(1, 313, true), autoRatingGroupBox.Location);
				}
			}
		}

		public void TestAccChargeCodeForm_Scaled_DisableBulkDisbursementJobsClosure()
		{
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			mock.Setup(m => m.EnableBulkDisbursementJobsClosure).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (ControlDpiScalingHelper.OverrideDPI_ForTesting(144, 144))
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();
					Application.DoEvents();
					var detailsDisbursementSurplusAccountGuidFindBox = form.FindSingleOrDefault<ZGuidFindBox>("DetailsDisbursementSurplusAccountGuidFindBox");
					var detailsDisbursementShortfallAccountGuidFindBox = form.FindSingleOrDefault<ZGuidFindBox>("DetailsDisbursementShortfallAccountGuidFindBox");
					var glAccountSetupGroupBox = form.FindSingleOrDefault<ZGroupBox>("GLAccountSetupGroupBox");
					var autoRatingGroupBox = form.FindSingleOrDefault<ZGroupBox>("AutoRatingGroupBox");

					AssertEquals(false, detailsDisbursementSurplusAccountGuidFindBox.Visible);
					AssertEquals(false, detailsDisbursementShortfallAccountGuidFindBox.Visible);

					var scaledSize = ControlDpiScalingHelper.NewScaledSize(653, 260);
					var scaledPoint = ControlDpiScalingHelper.NewScaledPoint(1, 363);
					var expectedHeight = scaledSize.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
					var expectedY = scaledPoint.Y - ControlDpiScalingHelper.ScaleToCurrentDpiY(50);

					AssertEquals(ControlDpiScalingHelper.NewScaledSize(scaledSize.Width, expectedHeight, false), glAccountSetupGroupBox.Size);
					AssertEquals(ControlDpiScalingHelper.NewScaledPoint(scaledPoint.X, expectedY, false), autoRatingGroupBox.Location);
				}
			}
		}

		#region PlaceOfSupplyConfiguration

		[RequiresSTA]
		public void TestPlaceOfSupplyConfigurationTab_IsNotVisible_WhenAllRegistryValuesAreDisabled()
		{
			var company = Factory.New<GlbCompany>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company.PK;

			var allDisabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in allDisabled)
			{
				item.Bool = false;
			}

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, allDisabled))
			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("ChargeCodeTabControl", true).First();
				var configurationTabPages = tabControl.Controls.Find("PlaceOfSupplyConfigurationTabPage", true);
				AssertEquals(nameof(configurationTabPages.Length), 0, configurationTabPages.Length);
			}
		}

		public void TestPlaceOfSupplyConfigurationTab_IsVisible_WhenAtLeastOneRegistryValueIsActive()
		{
			var company = Factory.New<GlbCompany>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company.PK;

			var someEnabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in someEnabled)
			{
				item.Bool = false;
			}
			someEnabled[0].Bool = true;

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, someEnabled))
			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("ChargeCodeTabControl", true).First();
				var configurationTabPages = tabControl.Controls.Find("PlaceOfSupplyConfigurationTabPage", true);
				AssertEquals(nameof(configurationTabPages.Length), 1, configurationTabPages.Length);
				var configurationTabPage = configurationTabPages[0] as ZTabPage;
				AssertEquals(nameof(configurationTabPage.TabVisible), true, configurationTabPage.TabVisible);
			}
		}

		#endregion

		#region SupplyTypeOverrideTabPage

		public void TestSupplyTypeOverrideTabPage()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();
					var chargeCodeTabControl = (ZTemplateTabControl)form.Controls["ChargeCodeTabControl"];
					AssertNotNull("Precondition:", chargeCodeTabControl);

					var supplyTypeOverrideTabPage = chargeCodeTabControl.Controls.Find("SupplyTypeOverrideTabPage", true).First() as ZTabPage;
					AssertNotNull("Precondition:", supplyTypeOverrideTabPage);
					AssertEquals(nameof(supplyTypeOverrideTabPage.TabVisible), true, supplyTypeOverrideTabPage.TabVisible);

					chargeCodeTabControl.SelectTab("SupplyTypeOverrideTabPage");
					Application.DoEvents();

					var supplyTypeOverrideGrid = chargeCodeTabControl.SelectedTab.GetControl<ZGrid>("SupplyTypeOverrideGrid", true);
					AssertNotNull("Precondition:", supplyTypeOverrideGrid);
					Assert(supplyTypeOverrideGrid.GetColumnStyle("ACS_JobType").IsVisible);
					Assert(supplyTypeOverrideGrid.GetColumnStyle("ACS_TransportMode").IsVisible);
					Assert(supplyTypeOverrideGrid.GetColumnStyle("ACS_Direction").IsVisible);
					Assert(supplyTypeOverrideGrid.GetColumnStyle("ACS_IncoTerm").IsVisible);
					Assert(supplyTypeOverrideGrid.GetColumnStyle("ACS_SupplyType").IsVisible);
					Assert(supplyTypeOverrideGrid.GetColumnStyle("ACS_GE").IsVisible);
				}
			}
		}

		public virtual void TestSupplyTypeOverrideTabDisplayPolicyAffect()
		{
			AssertSupplyTypeOverrideTabDisplayPolicyAffect(true, true);
			AssertSupplyTypeOverrideTabDisplayPolicyAffect(false, false);
		}

		protected void AssertSupplyTypeOverrideTabDisplayPolicyAffect(bool enableSupplyTypeCodesRegValue, bool expectedTabIsVisible)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeCodesRegValue))
			{
				var chargeCode = Factory.New<AccChargeCode>();
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();

					var supplyTypeOverrideTabPage = (ZTabPage)(typeof(AccChargeCodeForm).GetField("SupplyTypeOverrideTabPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
					AssertEquals(nameof(supplyTypeOverrideTabPage), expectedTabIsVisible, supplyTypeOverrideTabPage.TabVisible);
				}
			}
		}

		#endregion

		#region SellComplianceDescriptionTabPage

		public void TestSellComplianceDescriptionTabPage()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableSellComplianceDescription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				using (var form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();
					var chargeCodeTabControl = (ZTemplateTabControl)form.Controls["ChargeCodeTabControl"];
					AssertNotNull("Precondition:", chargeCodeTabControl);

					var complianceDescriptionTabPage = chargeCodeTabControl.Controls.Find("SellComplianceDescriptionTabPage", true);
					AssertEquals(nameof(complianceDescriptionTabPage.Length), 1, complianceDescriptionTabPage.Length);
					var configurationTabPage = complianceDescriptionTabPage[0] as ZTabPage;
					AssertEquals(nameof(configurationTabPage.TabVisible), true, configurationTabPage.TabVisible);

					chargeCodeTabControl.SelectTab("SellComplianceDescriptionTabPage");
					Application.DoEvents();

					var complianceDescriptionGrid = chargeCodeTabControl.SelectedTab.GetControl<ZGrid>("SellComplianceDescriptionGrid", true);
					AssertNotNull("Precondition:", complianceDescriptionGrid);
					Assert(complianceDescriptionGrid.GetColumnStyle("ADE_JobType").IsVisible);
					Assert(complianceDescriptionGrid.GetColumnStyle("ADE_TransportMode").IsVisible);
					Assert(complianceDescriptionGrid.GetColumnStyle("ADE_SupplyType").IsVisible);
					Assert(complianceDescriptionGrid.GetColumnStyle("ADE_Description").IsVisible);
				}
			}
		}

		[RequiresSTA]
		public virtual void TestSellComplianceDescriptionTabDisplayPolicyAffect()
		{
			AssertSellComplianceDescriptionTabDisplayPolicyAffect(true, true, true);
			AssertSellComplianceDescriptionTabDisplayPolicyAffect(true, false, false);
			AssertSellComplianceDescriptionTabDisplayPolicyAffect(false, true, false);
			AssertSellComplianceDescriptionTabDisplayPolicyAffect(false, false, false);
		}

		void AssertSellComplianceDescriptionTabDisplayPolicyAffect(bool enableSupplyTypeCodesRegValue, bool enableSellCompDescRegValue, bool expectedTabIsVisible)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeCodesRegValue))
			using (AccountingMasterFilesRegistry.Instance.EnableSellComplianceDescription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSellCompDescRegValue))
			{
				var chargeCode = Factory.New<AccChargeCode>();
				using (Form form = new AccChargeCodeForm(chargeCode))
				{
					form.Show();

					var complianceDescriptionTabPage = (ZTabPage)(typeof(AccChargeCodeForm).GetField("SellComplianceDescriptionTabPage", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
					AssertEquals(nameof(complianceDescriptionTabPage), expectedTabIsVisible, complianceDescriptionTabPage.TabVisible);
				}
			}
		}

		#endregion

		#region GovtChargeCodeOverrides

		public void TestGovtChargeCodeOverrideTab_IsNotVisible()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = Env.CurrentCompanyPK;

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false);
			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("ChargeCodeTabControl", true).First();
				var configurationTabPages = tabControl.Controls.Find("GovtChargeCodeOverrideTabPage", true);
				AssertEquals(nameof(configurationTabPages.Length), 0, configurationTabPages.Length);
			}
		}

		public void TestGovtChargeCodeOverrideTab_IsVisible()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = Env.CurrentCompanyPK;

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);
			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("ChargeCodeTabControl", true).First();
				var configurationTabPages = tabControl.Controls.Find("GovtChargeCodeOverrideTabPage", true);
				AssertEquals(nameof(configurationTabPages.Length), 1, configurationTabPages.Length);
				var configurationTabPage = configurationTabPages[0] as ZTabPage;
				AssertEquals(nameof(configurationTabPage.TabVisible), true, configurationTabPage.TabVisible);
			}
		}

		#endregion

		#region CreditorOverridesTab

		public void TestCreditorOverridesTab()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			using (var form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				Application.DoEvents();

				var testTabControl = (ZTemplateTabControl)form.Controls["ChargeCodeTabControl"];
				AssertNotNull("Precondition:", testTabControl);
				testTabControl.SelectTab("CreditorOverridesTab");
				Application.DoEvents();

				var testGrid = testTabControl.SelectedTab.GetControl<ZGrid>("CreditorOverridesGrid", true);
				AssertNotNull("Precondition:", testGrid);
				Assert(testGrid.GetColumnStyle("ACC_PaymentTerm").IsVisible);
				Assert(testGrid.GetColumnStyle("ACC_CreditorRole").IsVisible);
			}
		}

		#endregion

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccChargeCodeForm)GetFormToBashCore())
			{
				AssertNotNull("Charge Code form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		#region Implementation

		void AssertMappingControlsVisibility(bool expected, string editControlName, string visibilityDeterminedBy)
		{
			FieldInfo dropEditField = typeof(AccChargeCodeForm).GetField(editControlName, BindingFlags.Instance | BindingFlags.NonPublic);
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();

			using (AccChargeCodeForm form = new AccChargeCodeForm(chargeCode))
			{
				form.Show();
				Application.DoEvents();
				ZDropEdit dropEdit = (ZDropEdit)dropEditField.GetValue(form);
				AssertEquals("DropEdit's visibility should be determined by " + visibilityDeterminedBy, expected, dropEdit.Visible);
			}
		}

		void AssertColumnExist(string columnName, ZGrid grid, bool mustExist)
		{
			bool theColumnExist = false;
			foreach (ZGridColumnInfo column in grid.ColumnStyles)
			{
				if (column.ColumnName == columnName)
				{
					theColumnExist = true;
					break;
				}
			}
			AssertEquals(string.Format("Column {0} must{1} exits in grid {2}", columnName, mustExist ? "" : " not", grid.Name), mustExist, theColumnExist);
		}

		protected override Form GetFormToBashCore()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			return new AccChargeCodeForm(chargeCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (!ObjectFactory.HasBeenSubstituted<IAuthTokenProvider>())
			{
				var authTokenProvider = new Mock<IAuthTokenProvider>();
				authTokenProvider
					.Setup(m => m.GetToken(
						It.IsAny<string>(),
						It.IsAny<int>(),
						It.IsAny<Action<AuthenticationService.Client.Models.LoginInfo>>(),
						It.IsAny<System.Threading.CancellationToken>(),
						It.IsAny<bool>())).Returns(("some token", string.Empty));

				ObjectFactory.Substitute(authTokenProvider.Object);
			}
		}

		protected override void TearDown()
		{
			ObjectFactory.DisposeSubstitutions();
			base.TearDown();
		}

		#endregion
	}
}
