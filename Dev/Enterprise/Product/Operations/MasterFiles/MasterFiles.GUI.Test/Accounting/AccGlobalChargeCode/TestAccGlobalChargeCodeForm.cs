using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccGlobalChargeCodeFormForTest))]
	sealed class TestAccGlobalChargeCodeForm : TestAccChargeCodeForm
	{
		public void TestFormCaptionAndCodeCaption()
		{
			using (ZForm form = (ZForm)GetFormToBash())
			{
				form.Show();
				if (((BusinessObject)form.BusinessEntity).IsInDatabase)
				{
					AssertEquals("Form Caption Correct", "Edit Global Charge Code", form.Text);
				}
				else
				{
					AssertEquals("Form Caption Correct", "New Global Charge Code", form.Text);
				}
			}
		}

		public override void TestDisplayPolicyAffect()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			using (Form form = GetFormToBash())
			{
				form.Show();
				var aC_AT_GSTRateBoundGuidFindBox = MasterFilesTestHelper.GetNonPublicValue<ZGuidFindBox>("AC_AT_GSTRateBoundGuidFindBox", form);
				AssertEquals("GST Rate is not visible", false, aC_AT_GSTRateBoundGuidFindBox.Visible);
				var inputGSTVATRecoverableCalcEdit = MasterFilesTestHelper.GetNonPublicValue<ZCalcEdit>("InputGSTVATRecoverableCalcEdit", form);
				AssertEquals("GST Recoverable is not visible", false, inputGSTVATRecoverableCalcEdit.Visible);
				var aC_AW_WithholdingTaxRateBoundGuidFindBox = MasterFilesTestHelper.GetNonPublicValue<ZGuidFindBox>("AC_AW_WithholdingTaxRateBoundGuidFindBox", form);
				AssertEquals("Witholding Rate is not visible", false, aC_AW_WithholdingTaxRateBoundGuidFindBox.Visible);
				var taxOverridesTabPage = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("TaxOverridesTabPage", form);
				AssertEquals("Tax Overrides Tab is not visible", false, taxOverridesTabPage.TabVisible);
				var branchOverridesTab = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("BranchOverridesTab", form);
				AssertEquals("Branch Overrides Tab is not visible", false, branchOverridesTab.TabVisible);
				var sellComplianceDescriptionTabPage = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("SellComplianceDescriptionTabPage", form);
				AssertEquals("Sell Compliance Description Tab is not visible", false, sellComplianceDescriptionTabPage.TabVisible);
				var placeOfSupplyConfigurationTab = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("PlaceOfSupplyConfigurationTabPage", form);
				AssertEquals("PlaceOfSupplyConfigurationTabPage should not be visible for global charge codes", false, placeOfSupplyConfigurationTab.TabVisible);
				var creditorOverridesTab = form.FindSingle<ZTabPage>("CreditorOverridesTab");
				AssertEquals("Creditor Overrides Tab is visible", true, creditorOverridesTab.TabVisible);
				creditorOverridesTab.Show();
				var creditorOverridesGrid = creditorOverridesTab.FindSingle<ZGrid>("CreditorOverridesGrid");
				AssertEquals("ACC_OH_Creditor", false, creditorOverridesGrid.GetColumnStyle("ACC_OH_Creditor").IsVisible);
				AssertEquals("CreditorName", false, creditorOverridesGrid.GetColumnStyle("CreditorName").IsVisible);
			}
		}

		[RequiresSTA]
		public override void TestSupplyTypeOverrideTabDisplayPolicyAffect()
		{
			AssertSupplyTypeOverrideTabDisplayPolicyAffect(false, false);
			AssertSupplyTypeOverrideTabDisplayPolicyAffect(false, false);
		}

		public override void TestGovtAccChargeCodeDisplayPolicyAffect()
		{
			foreach (bool regValue in new bool[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
					chargeCode.AC_GovtChargeCode = regValue ? "Test" : string.Empty;
					using (Form form = new AccGlobalChargeCodeForm(chargeCode))
					{
						form.Show();
						var govtChargeCodeTextBox = (ZTextBox)(typeof(AccChargeCodeForm).GetField("GovtChargeCodeTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form));
						AssertEquals("GovtChargeCode is visible", false, govtChargeCodeTextBox.Visible);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestPresaveDialogs()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, false, true, "CC1", "CC2");

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				((AccChargeCode)form.BusinessEntity).AC_Code = "QZX";
				form.ShowPreSaveDialogs();
				AssertEquals("Warning when first saving global charge code",
					"This action will create a charge code called 'QZX' for every company in the system. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}

			Factory.Save();

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				form.ShowPreSaveDialogs();
				AssertEquals("No warning for existing global charge code", null, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[RequiresSTA]
		public void TestLocalChargeCodesTabVisiblity()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, false, true, "CC1", "CC2");

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				var localChargeCodesTab = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("LocalChargeCodesTab", form);
				AssertEquals("Local charge codes tab not shown on unsaved form", false, localChargeCodesTab.TabVisible);
				form.FireSaveButton();
				AssertEquals("Local charge codes tab shown after save", true, localChargeCodesTab.TabVisible);
			}

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				var localChargeCodesTab = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("LocalChargeCodesTab", form);
				AssertEquals("Local charge codes tab shown when loading existing", true, localChargeCodesTab.TabVisible);
			}
		}

		public void TestLocalChargeCodesGrid_Deletions()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				var localChargeCodesGrid = LocalChargeCodesGridPrepare(form);
				var menuItemDelete = localChargeCodesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == "&Delete");
				Assert("There should be 'delete' menu item", menuItemDelete != null);
				localChargeCodesGrid.Select(0);
				var firstElement = localChargeCodesGrid.SelectedElements[0];

				// Single
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItemDelete.PerformClick();
				AssertEquals("Are you sure message shown (single)", "Are you sure you want to delete this charge code?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Charge code not deleted", false, firstElement.IsDeleted);

				// Multi
				localChargeCodesGrid.Select(1);
				AssertEquals("precondition", 2, localChargeCodesGrid.SelectedElements.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItemDelete.PerformClick();
				AssertEquals("Are you sure message shown (multiple)", "Are you sure you want to delete these charge codes?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Charge code not deleted", false, firstElement.IsDeleted);
			}
		}

		[RequiresSTA]
		public void TestLocalChargeCodesGrid_Edits()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				var localChargeCodesGrid = LocalChargeCodesGridPrepare(form);
				var menuItemEditInForm = localChargeCodesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == "Edit");
				Assert("There should be 'edit' menu item", menuItemEditInForm != null);

				menuItemEditInForm.PerformClick();
				AssertEquals("Please select message", "Please select a Charge Code to edit", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				localChargeCodesGrid.Select(0);
				localChargeCodesGrid.Select(1);
				AssertEquals("precondition", 2, localChargeCodesGrid.SelectedElements.Length);
				localChargeCodesGrid.SelectAllElements();
				menuItemEditInForm.PerformClick();
				AssertEquals("Please select message", "Please select just one Charge Code to edit", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				globalChargeCode.AC_MarginPercentage = 55M;
				localChargeCodesGrid.UnSelectAll();
				localChargeCodesGrid.Select(0);
				menuItemEditInForm.PerformClick();
				AssertEquals("Please select message", "Please save changes to the Global Charge Code before editing local Charge Codes", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				localChargeCodesGrid.Select(0);
				var firstElement = localChargeCodesGrid.SelectedElements[0];
				menuItemEditInForm.PerformClick();
				AssertEquals(typeof(AccChargeCodeForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				localChargeCodesGrid.Select(0);
				var chargeCodeToEdit = (AccChargeCode)localChargeCodesGrid.SelectedElements.First();
				var branches = chargeCodeToEdit.Company.Branches;

				for (int i = 0; i < branches.Count; i++)
				{
					branches[i].GB_IsActive = false;
				}

				menuItemEditInForm.PerformClick();

				for (int i = 0; i < branches.Count; i++)
				{
					branches[i].GB_IsActive = true;
				}

				AssertEquals("Please select message", "Please select a company which has active branches", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLocalChargeCodesGrid_CompanyContext()
		{
			var newCo = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCo.PK;
			newCo.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Belgium;
			Factory.Save();

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var chargeCodeForBelgium = globalChargeCode.ChildChargeCodes.Single(c => c.Company.PK == newCo.PK);

			using (var form = new AccGlobalChargeCodeFormForTest(globalChargeCode))
			{
				var localChargeCodesGrid = LocalChargeCodesGridPrepare(form);
				var menuItemEditInForm = localChargeCodesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == "Edit");
				Assert("There should be 'edit' menu item", menuItemEditInForm != null);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				localChargeCodesGrid.SelectSingleElement(chargeCodeForBelgium);
				var firstElement = localChargeCodesGrid.SelectedElements[0];
				menuItemEditInForm.PerformClick();

				form.AssertOnChildForm = (f =>
				{
					AssertEquals("We are in the correct company context", newCo.PK.ToGuid(), Env.CurrentCompany.PK);
				});

				menuItemEditInForm.PerformClick();
				AssertEquals(typeof(AccChargeCodeForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		[RequiresSTA]
		public void TestLocalLanguageDescriptionTextBoxNeverVisible()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			SetEnableLocalChargeCodeDescriptionDefault(true);
			AssertEquals("precondition", true, ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault);

			using (var form = new AccGlobalChargeCodeForm(globalChargeCode))
			{
				var localLanguageDescriptionTextBox = MasterFilesTestHelper.GetNonPublicValue<ZTextBox>("LocalLanguageDescriptionTextBox", form);
				AssertEquals("Local language description is not visible", false, localLanguageDescriptionTextBox.Visible);
			}
		}

		void SetEnableLocalChargeCodeDescriptionDefault(bool value)
		{
			var query = new ZQuery(StmDataSchema.SD_Name, "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			query.AddToFilter(StmDataSchema.SD_Owner, null);
			query.AddToFilter(StmDataSchema.SD_DepartmentGuid, null);
			var stmData = Factory.Load<StmData>(query).FirstOrDefault();
			if (stmData == null)
			{
				stmData = Factory.New<StmData>();
				stmData.SD_Name = "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT";
				stmData.SD_Type = "BOL";
			}
			stmData.SD_IsLogged = ZBool.True;
			stmData.SD_BinaryValue = Encoding.Unicode.GetBytes(value.ToString());
			Factory.Save();
		}

		public override void TestActionsMenu()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			using (var form = new AccGlobalChargeCodeForm(globalChargeCode))
			{
				AssertGlobalDifferencesAvailableAndWorking(form);
			}
		}

		public override void TestFormCaptionForOtherCompanyChargeCode()
		{
			Assert("No need for this test in global charge code form", true);
		}

		#region Implementation

		ZGrid LocalChargeCodesGridPrepare(AccGlobalChargeCodeFormForTest form)
		{
			form.Show();
			var localChargeCodesTab = MasterFilesTestHelper.GetNonPublicValue<ZTabPage>("LocalChargeCodesTab", form);
			localChargeCodesTab.Show();
			var localChargeCodesGrid = MasterFilesTestHelper.GetNonPublicValue<ZGrid>("LocalChargeCodesGrid", form);
			return localChargeCodesGrid;
		}

		public class AccGlobalChargeCodeFormForTest : AccGlobalChargeCodeForm
		{
			public AccGlobalChargeCodeFormForTest(AccChargeCode chargeCode)
				: base(chargeCode)
			{
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}
		}

		protected override Form GetFormToBashCore()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);
			return new AccGlobalChargeCodeFormForTest(globalChargeCode);
		}

		#endregion
	}
}
