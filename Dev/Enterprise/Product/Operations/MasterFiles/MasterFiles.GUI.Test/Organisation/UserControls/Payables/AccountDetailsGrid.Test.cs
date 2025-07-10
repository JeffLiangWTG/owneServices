using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Organisation.UserControls.Payables;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.Organisation.UserControls.Payables.AccountDetailsGrid;

namespace Enterprise.MasterFiles.GUI.Test.Organisation.UserControls.Payables
{
	[TestedType(typeof(AccountDetailsGridFormForBash))]
	internal class AccountDetailsGridBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			Factory.Save();

			return new AccountDetailsGridFormForBash(companyData);
		}

		public void TestAccountDetailsGrid()
		{
			using (var accountDetailsGrid = new AccountDetailsGrid())
			{
				var grid = accountDetailsGrid.Controls.Find("grid", false)[0] as ZGrid;

				var expectedListOfColumns = new[]
				{
					$"{AccAPAccountDetails.Schema.A1_IsDefaultAccount} (ZCheckBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_PaymentMethod} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_RX_NKAccountCurrency} (ZCodeFindBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_AccountName} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_BankName} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_BankSwift} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_BankBsb} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_BankAccount} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_IBANNumber} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_BankBranchName} (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"{AccAPAccountDetails.Schema.A1_BankAddress1} (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"{AccAPAccountDetails.Schema.A1_BankAddress2} (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"{AccAPAccountDetails.Schema.A1_BankAddress3} (ZTextBoxColumnStyleInfo) IsVisible:False",
					$"{AccAPAccountDetails.Schema.A1_RN_NKCountryCode} (ZCodeFindBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_EPaymentReasonCode} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_EPaymentReferenceType} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_EPaymentReference} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_SystemCreateUser} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_SystemCreateTimeUtc} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_SystemLastEditUser} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{AccAPAccountDetails.Schema.A1_SystemLastEditTimeUtc} (ZDateEditColumnStyleInfo) IsVisible:True"
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestEPaymentColumnsVisibility()
		{
			var columnNames = new[] { "A1_EPaymentReasonCode", "A1_EPaymentReference", "A1_EPaymentReferenceType" };

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			{
				AssertEPaymentColumnsVisibility(3);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), false))
			{
				AssertEPaymentColumnsVisibility(0);
			}

			void AssertEPaymentColumnsVisibility(int expectedColumnsCount)
			{
				var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
				var accountDetails = orgCompanyData.AccountDetailsCollection.AddNew();
				using (var form = new AccountDetailsGridFormForBash(orgCompanyData))
				{
					form.Show();
					var grid = form.Controls.Find("grid", true)[0] as ZGrid;
					AssertNotNull(grid);
					var actualColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(s => columnNames.Contains(s.ColumnName));
					AssertEquals(expectedColumnsCount, actualColumns.Count());
					AssertEquals(true, actualColumns.All(x => x.IsVisible));
				}
			}
		}

		public void TestConfigureForEPaymentContextMenuItemVisibility()
		{
			AssertConfigureForEPaymentContextMenuItemVisibility(true);
			AssertConfigureForEPaymentContextMenuItemVisibility(false);
		}

		void AssertConfigureForEPaymentContextMenuItemVisibility(bool isOFXEPaymentEnabled)
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), isOFXEPaymentEnabled))
			{
				var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
				var accountDetails = orgCompanyData.AccountDetailsCollection.AddNew();

				using (var form = new AccountDetailsGridFormForBash(orgCompanyData))
				{
					form.Show();
					var grid = form.Controls.Find("grid", true)[0] as ZGrid;
					AssertNotNull(grid);
					grid.UnSelectAll();
					grid.Select(0);

					var contextMenuItems = grid.ContextMenu.MenuItems;
					var configureForEPaymentMenuItem = contextMenuItems.FindByText("Configure for E-Payment", false);

					if (isOFXEPaymentEnabled)
					{
						AssertNotNull(configureForEPaymentMenuItem);
						Assert(configureForEPaymentMenuItem.Enabled);
					}
					else
					{
						AssertNull(configureForEPaymentMenuItem);
					}
				}
			}
		}

		public void TestConfigureForEPaymentContextMenuItemOpensMatchEPaymentRecipientsForm()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			{
				var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
				var accountDetails = orgCompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;

				using (var form = new AccountDetailsGridFormForBash(orgCompanyData))
				{
					form.Show();
					var grid = form.Controls.Find("grid", true)[0] as ZGrid;
					AssertNotNull(grid);
					grid.UnSelectAll();
					grid.Select(0);

					var contextMenuItems = grid.ContextMenu.MenuItems;
					var configureForEPaymentMenuItem = contextMenuItems.FindByText("Configure for E-Payment", false);
					AssertNotNull(configureForEPaymentMenuItem);
					Assert(configureForEPaymentMenuItem.Enabled);

					configureForEPaymentMenuItem.PerformClick();
					var matchReceiptsForm = ZFormModaliser.LastFormShownDialogForTest;
					AssertNotNull(matchReceiptsForm);
					AssertEquals("Enterprise.Accounting.GUI.MatchEPaymentRecipientsForm", matchReceiptsForm.GetType().FullName);
				}
			}
		}

		[RequiresSTA]
		public void TestConfigureForEPaymentContextMenuItemShowErrorForNonEPO()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			{
				var orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
				var accountDetails = orgCompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;

				using (var form = new AccountDetailsGridFormForBash(orgCompanyData))
				{
					form.Show();
					var grid = form.Controls.Find("grid", true)[0] as ZGrid;
					AssertNotNull(grid);
					grid.UnSelectAll();
					grid.Select(0);

					var contextMenuItems = grid.ContextMenu.MenuItems;
					var configureForEPaymentMenuItem = contextMenuItems.FindByText("Configure for E-Payment", false);
					AssertNotNull(configureForEPaymentMenuItem);
					Assert(configureForEPaymentMenuItem.Enabled);

					configureForEPaymentMenuItem.PerformClick();
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Only accounts with a payment method of EPO can be configured for E-Payment", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestCanDeleteAccountDetailWhenPaymentMethodIsEPOAndBeneficiaryIdIsNotEmpty()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), true))
			{
				var orgCompanyData = Factory.New<OrgCompanyData>();
				var accountDetails = orgCompanyData.AccountDetailsCollection.AddNew();
				accountDetails.A1_PaymentMethod = EPaymentMethods.EPaymentViaOFX;
				accountDetails.A1_EPaymentBeneficiaryId = ZGuid.BrettsGuid;

				using (var form = new AccountDetailsGridFormForBash(orgCompanyData))
				{
					form.Show();
					Application.DoEvents();

					var grid = form.Controls.Find("grid", true)[0] as ZGrid;
					AssertNotNull(grid);
					grid.UnSelectAll();
					grid.Select(0);
					AssertEquals(1, grid.SelectedElements.Length);
					Assert(!grid.SelectedElements[0].ReadOnly);
					Assert(!accountDetails.IsDeleted);

					grid.SetCurrentHitTestForTest(0, 0);
					grid.OnPopup_CallForTesting();

					var deleteMenuItem = grid.ContextMenu.MenuItems.FindByText("Delete", false);
					AssertNotNull(deleteMenuItem);
					Assert(deleteMenuItem.Enabled);

					deleteMenuItem.PerformClick();

					Assert(accountDetails.IsDeleted);
					grid.UnSelectAll();
					grid.SelectAllElements();
					AssertEquals(0, grid.SelectedElements.Length);
				}
			}
		}

		internal class AccountDetailsGridFormForBash : ZForm
		{
			internal AccountDetailsGridFormForBash(OrgCompanyData companyData)
				: base(companyData)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(AccountDetailsGrid);
				BindingSource.SetBindingMember(AccountDetailsGrid, ".");
				CaptionRenderingEnabled = true;
			}

			readonly AccountDetailsGrid AccountDetailsGrid = new AccountDetailsGrid();
		}
	}

	[TestedType(typeof(AccountDetailsCollection))]
	internal class AccountDetailsCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccountDetailsCollection(Factory);
		}
	}
}
