using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(USImportSupplierHeaderUserControl))]
	sealed class ImportSupplierHeaderUserControlTest : Testing.ImportCustomsUserControlBasherAbstractTest
	{
		public void TestGridLayoutContext()
		{
			using (USImportSupplierHeaderUserControl control = new USImportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestGroupInvoiceLabelVisibility()
		{
			using (USImportSupplierHeaderUserControl control = new USImportSupplierHeaderUserControl())
			{
				AssertEquals("GroupInvoiceDropEdit.Visible", false, control.FindSingle<ZDropEdit>("GroupInvoiceDropEdit").Visible);
			}
		}

		public void TestElectronicInvoiceTabPageVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = false;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				AssertEquals("Electronic Invoice page should be shown", false, userControl.ElectronicInvoiceTabPage.TabVisible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_EnableAII = true;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				AssertEquals("Electronic Invoice page should be shown", true, userControl.ElectronicInvoiceTabPage.TabVisible);
			}
		}

		public void TestACEControlVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				AssertEquals("FirstSaleDropEdit should be shown", true, userControl.FirstSaleDropEdit.Visible);
				var firstSaleColumnInfo = (ZDropEditColumnStyleInfo)userControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.US_FirstSale);
				AssertEquals("First Sale column exists in the grid", false, IsNullObjectDelegate(firstSaleColumnInfo));
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("FirstSaleDropEdit should not be shown for ACS", true, userControl.FirstSaleDropEdit.Visible);
				AssertEquals("FDAShipperAddressAddressControl should not be shown for ACE", false, userControl.FDAShipperAddressAddressControl.Visible);
			}
		}

		public void TestMIDParser()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "TEST ORG DUMMY";
			org.OH_Code = "ORG" + new Random().Next(1000000).ToString();
			var mainAddress = org.MainAddress;
			mainAddress.OA_Address1 = "ADDRESS 1";
			var cusCode = mainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID234323");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var invoiceUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var invoicerAddressColumnInfo = (ZGuidDropEditColumnStyleInfo)invoiceUserControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_InvoicerDocAddress);
				var manufacturerAddressColumnInfo = (ZGuidDropEditColumnStyleInfo)invoiceUserControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_ManufacturerAddress);
				var supplierAddressColumnInfo = (ZGuidDropEditColumnStyleInfo)invoiceUserControl.InvoiceHeadersBoundGrid.GetColumnStyle(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
				AssertEquals(mainAddress.PK, invoicerAddressColumnInfo.Parse("MID234323"));
				AssertEquals(mainAddress.PK, manufacturerAddressColumnInfo.Parse("MID234323"));
				AssertEquals(mainAddress.PK, supplierAddressColumnInfo.Parse("MID234323"));
			}
		}

		public void TestColumnsHeaders()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("PreCondition:UserControl type", typeof(USImportSupplierHeaderUserControl), form.CustomsBrokerageUserControl.SupplierHeaderUserControl.GetType());
				AssertEquals("Column Header", "Importer", form.CustomsBrokerageUserControl.SupplierHeaderUserControl.InvoiceHeadersBoundGrid.Columns[JobComInvoiceHeaderSchema.Constants.JZ_OH_Buyer].ColumnStyle.HeaderText);
			}
		}

		public void TestDefaultingButtonsAreShown()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "M081232123";
			declaration.JE_HouseBill = "H023223434";
			JobComInvoiceHeader invoice = declaration.FilteredInvoices.AddNew();
			JobComInvoiceHeader invoice2 = declaration.FilteredInvoices.AddNew();
			AssertEquals(0, invoice.RelatedDocuments.Count);
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.ElectronicInvoiceTabPage;
				InvoiceHeaderAIIUserControl aiiUserControl = (InvoiceHeaderAIIUserControl)userControl.ElectronicInvoiceTabPage.Controls[0];
				aiiUserControl.AIIOtherDetailsTabControl.SelectedTab = aiiUserControl.RelatedDataTabPage;
				AssertEquals("DefaultRelatedDocumentsButton should be visible on a declaration form", true, aiiUserControl.DefaultRelatedDocumentsButton.Visible);
			}
		}

		public void TestDeleteInvoicesWhenWaitingForResponses()
		{
			string invoicesWithWaitingResponse = "Electronic invoice message(s) exist that are pending response(s) from Customs. Please wait for the response(s) from Customs before delete the invoice(s).";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "M081232123";
			declaration.JE_HouseBill = "H023223434";
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.JZ_MessageStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingElectronicInvoiceOriginal;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.InvoiceLines.AddNew();
			invoice3.JZ_MessageStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				SendDeleteKeys(userControl.InvoiceHeadersBoundGrid, 0, 1);
				AssertEquals("Should not delete while waiting for response. Delete action is cancelled", 2, declaration.Invoices.Count);
				AssertEquals(2, userControl.InvoiceHeadersBoundGrid.ListManager.Count);
				AssertEquals(invoicesWithWaitingResponse, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteInvoicesWhenLodgedAtCustoms()
		{
			string invoicesWithCustomsTransactions = "You are about to delete Invoices that have been sent to Customs electronically.\r\nDo you wish to continue?";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "M081232123";
			declaration.JE_HouseBill = "H023223434";
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.JZ_MessageStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearElectronicInvoiceOriginal;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				SendDeleteKeys(userControl.InvoiceHeadersBoundGrid, 0, 1);
				AssertEquals("Should have asked, 'Is it OK to proceed?'", invoicesWithCustomsTransactions, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Not deleted", 3, userControl.InvoiceHeadersBoundGrid.ListManager.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				SendDeleteKeys(userControl.InvoiceHeadersBoundGrid, 0, 1);
				AssertEquals("Should have asked, 'Is it OK to proceed?'", invoicesWithCustomsTransactions, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("deleted", 1, userControl.InvoiceHeadersBoundGrid.ListManager.Count);
				Assert(invoice.IsDeleted);
				Assert(invoice2.IsDeleted);
			}
		}

		public void TestBaseGroupChargesGridColumnNamesInSortOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				ZString[] expectedChargesColumnNamesInSortOrderList = new ZString[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ChargeType,
					JobComInvHeaderChargeSchema.Constants.J7_Amount,
					JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency,
					Customs.Business.BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
					JobComInvHeaderChargeSchema.Constants.J7_IsDutiable,
					JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable,
					JobComInvHeaderChargeSchema.Constants.J7_ExchangeRate,
					"IsJ7_ExchangeRateUserEnterable",
					JobComInvHeaderChargeSchema.Constants.J7_DistributeBy,
					"ChargeCodeDescription"
				};
				AssertChargeGrid(userControl.BaseGroupChargesGrid, expectedChargesColumnNamesInSortOrderList);
				AssertGridColumnMandatory(userControl.BaseGroupChargesGrid, new ZString[] { JobComInvHeaderChargeSchema.Constants.J7_ChargeType, JobComInvHeaderChargeSchema.Constants.J7_Amount, JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency, Enterprise.Customs.Business.BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT, JobComInvHeaderChargeSchema.Constants.J7_IsDutiable, JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable });
			}
		}

		public void TestInvoiceChargesGridColumnNamesInSortOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				ZString[] expectedChargesColumnNamesInSortOrderList = new ZString[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ChargeType,
					JobComInvHeaderChargeSchema.Constants.J7_Amount,
					JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency,
					"J7_Calc_IsIncludedInInvoiceAmount",
					JobComInvHeaderChargeSchema.Constants.J7_IsIncludedInITOT,
					JobComInvHeaderChargeSchema.Constants.J7_IsDutiable,
					JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable,
					JobComInvHeaderChargeSchema.Constants.J7_ExchangeRate,
					"IsJ7_ExchangeRateUserEnterable",
					JobComInvHeaderChargeSchema.Constants.J7_DistributeBy,
					"ChargeCodeDescription"
				};
				AssertChargeGrid(userControl.InvoiceChargesGrid, expectedChargesColumnNamesInSortOrderList);
			}
		}

		public void TestApportionedChargesGridColumnNamesInSortOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.ChargesTabControl.SelectedTab = userControl.ApportionedTabPage;
				ZString[] expectedChargesColumnNamesInSortOrderList = new ZString[]
				{
					JobComInvHeaderChargeSchema.Constants.J7_ChargeType,
					JobComInvHeaderChargeSchema.Constants.J7_Amount,
					JobComInvHeaderChargeSchema.Constants.J7_RX_NKCurrency,
					"J7_Calc_IsIncludedInInvoiceAmount",
					JobComInvHeaderChargeSchema.Constants.J7_IsIncludedInITOT,
					JobComInvHeaderChargeSchema.Constants.J7_IsDutiable,
					JobComInvHeaderChargeSchema.Constants.J7_IsGSTApplicable,
					JobComInvHeaderChargeSchema.Constants.J7_ExchangeRate,
					"IsJ7_ExchangeRateUserEnterable",
					"ChargeCodeDescription"
				};
				AssertChargeGrid(userControl.ApportionedChargesGrid, expectedChargesColumnNamesInSortOrderList);
			}
		}

		public void TestConsumptionFTZRelatedControlsVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				Assert("userControl.FTZDependentPanel should be invisible when US_EntryType is not ConsumptionFTZ", !userControl.FTZDependentPanel.Visible);
				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
				Assert("userControl.FTZDependentPanel should be visible when US_EntryType is ConsumptionFTZ", userControl.FTZDependentPanel.Visible);
			}
		}

		public void TestReleaseEntryNumberVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				declaration.US_ConsolACE = ZBool.False;
				Assert("Release Entry Number field should be invisible when US_ConsolACE is FALSE.", !userControl.ReleaseEntryNumberCodeFindBox.Visible);
				declaration.US_ConsolACE = ZBool.True;
				Assert("Release Entry Number field should be visible when US_EntryType is TRUE", userControl.ReleaseEntryNumberCodeFindBox.Visible);
			}
		}

		public void TestCS00178802_UnitBindingIssue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "NZD";
			invoice1.JZ_InvoiceAmount = 1000m;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			invoice2.JZ_InvoiceAmount = 2000m;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var importSupplierHeaderUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				importSupplierHeaderUserControl.Focus();
				var fOBAmountBoundCurrencyControl = (ZCodeFindBox)importSupplierHeaderUserControl.JZ_FOBAmountBoundCurrencyControlInternal.Controls["UnitFindBox"];
				var tNIBoundInvoiceCurrencyControl = (ZCodeFindBox)importSupplierHeaderUserControl.JZ_Calc_TNIBoundInvoiceCurrencyControlInternal.Controls["UnitFindBox"];
				var lineTotalBoundConvertToLocalCurrencyControl = (ZCodeFindBox)importSupplierHeaderUserControl.LineTotalBoundConvertToLocalCurrencyControlInternal.Controls["UnitFindBox"];
				AssertEquals("NZD", fOBAmountBoundCurrencyControl.CurrentCode);
				AssertEquals("NZD", tNIBoundInvoiceCurrencyControl.CurrentCode);
				AssertEquals("NZD", lineTotalBoundConvertToLocalCurrencyControl.CurrentCode);
				importSupplierHeaderUserControl.InvoiceHeadersBoundGrid.CurrentRowIndex = 1;
				AssertEquals("AUD", fOBAmountBoundCurrencyControl.CurrentCode);
				AssertEquals("AUD", tNIBoundInvoiceCurrencyControl.CurrentCode);
				AssertEquals("AUD", lineTotalBoundConvertToLocalCurrencyControl.CurrentCode);
			}
		}

		public void TestBindings()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl importSupplierHeaderUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				ZDateEdit userControl = importSupplierHeaderUserControl.PrivilegedStatusFilingDateDateEdit;
				invoice.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
				Assert("PrivilegedStatusFilingDateDateEdit should not be visible when invoiceLine.US_ZoneStatus is not PrivilegedForeign", !userControl.Visible);
				invoice.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
				Assert("PrivilegedStatusFilingDateDateEdit should be visible when invoiceLine.US_ZoneStatus is PrivilegedForeign", userControl.Visible);
			}
		}

		public void TestACEDependantControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.OtherDetailTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				Assert(!userControl.FDAShipperAddressAddressControl.Visible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.OtherDetailTabPage;
				Assert(userControl.FDAShipperAddressAddressControl.Visible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.OtherDetailTabPage;
				Assert(userControl.FDAShipperAddressAddressControl.Visible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				declaration.US_EnableSPN = true;
				declaration.US_F_PNMode = "P";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.OtherDetailTabPage;
				Assert(!userControl.FDAShipperAddressAddressControl.Visible);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_F_PNMode = "O";
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.OtherDetailTabPage;
				Assert(userControl.FDAShipperAddressAddressControl.Visible);
			}
		}

		public void TestUltimateConsigneeCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var importSupplierHeaderUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var consigneeColumn = importSupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.ConsigneeAddressOrgPK];
				AssertContains("Consignee", consigneeColumn.ColumnStyle.HeaderText);
				AssertNotContains("Ultimate", consigneeColumn.ColumnStyle.HeaderText);
			}

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				var importSupplierHeaderUserControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var consigneeColumn = importSupplierHeaderUserControl.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.ConsigneeAddressOrgPK];
				AssertContains("Ultimate Consignee", consigneeColumn.ColumnStyle.HeaderText);
			}
		}

		public void TestDeductADD_CVDDutyVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "02";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_IncoTerm = "FOB";
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				AssertEquals("DeductADD_CVD should not be shown", false, userControl.DeductADD_CVDDutyDropEdit.Visible);
				declaration.US_EntryType = "03";
				AssertEquals("DeductADD_CVD should not be shown", false, userControl.DeductADD_CVDDutyDropEdit.Visible);
				declaration.Invoices[0].JZ_IncoTerm = "DDP";
				AssertEquals("DeductADD_CVD should be shown", true, userControl.DeductADD_CVDDutyDropEdit.Visible);
			}
		}

		public void TestGridId()
		{
			using (var control = new USImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutUarUzrY6sQK2iHYZ7vyVJA==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestUS_SplitShipmentDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = USJobMessageTypeList.Codes.FTZ;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl control = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				CombineAssertions(() =>
				{
					var splitDetailCtrl = control.FindSingleOrDefault<ZDropEdit>(ctrl => ctrl.BindTo.Contains("US_SplitShipmentDetail"));
					AssertNotNull("There should be a control", splitDetailCtrl);
					AssertEquals("The control should be in InvoiceDetailsBottomPanel", true, control.InvoiceDetailsBottomPanel.Controls.Contains(splitDetailCtrl));
					AssertEquals("Should be visible for FTZ", true, splitDetailCtrl.Visible);
				});
			}

			declaration.JE_MessageType = USJobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl control = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				CombineAssertions(() =>
				{
					var splitDetailCtrl = control.FindSingleOrDefault<ZDropEdit>(ctrl => ctrl.BindTo.Contains("US_SplitShipmentDetail"));
					AssertEquals("Should be invisible for non-FTZ", false, splitDetailCtrl.Visible);
				});
			}
		}

		public void TestGridContainsColumnDeductADD_CVDDuty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl control = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var column = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.US_DeductADDCVDDuty];
				AssertEquals("US_DeductADDCVDDuty", column.ColumnName);
				AssertEquals(false, column.IsVisible);
			}

			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl control = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var column = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.US_DeductADDCVDDuty];
				AssertEquals("US_DeductADDCVDDuty", column.ColumnName);
				AssertEquals(false, column.IsVisible);
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl control = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				var column = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.US_DeductADDCVDDuty];
				AssertEquals("US_DeductADDCVDDuty", column.ColumnName);
				AssertEquals(false, column.IsVisible);
			}
		}

		public void TestBOSubscribersShouldBeDetachedWhenFormIsDisposed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			using (var form = new ZForm(declaration))
			using (var userControl = new USImportSupplierHeaderUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
			}

			var propertyInfoStorageField = declaration.Factory.GetType().GetField("PropertyInfoStorage", BindingFlags.Instance | BindingFlags.NonPublic);
			var propertyInfoStorage = propertyInfoStorageField.GetValue(declaration.Factory);
			var valueChangedDictionaryProperty = propertyInfoStorage.GetType().GetProperty("ValueChangedDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var valueChangedDictionary = valueChangedDictionaryProperty.GetValue(propertyInfoStorage);
			var dictionaryField = valueChangedDictionary.GetType().GetField("dictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			var dictionary = (Dictionary<string, Dictionary<BusinessObject, EventHandler>>)dictionaryField.GetValue(valueChangedDictionary);
			foreach (var dictionaryOfSubcriber in dictionary)
			{
				foreach (var subcriber in dictionaryOfSubcriber.Value)
				{
					var subcriberTarget = subcriber.Value.Target;
					if (subcriberTarget is DeclarationValueChangedAnnouncer)
					{
						var onValueChangedField = subcriberTarget.GetType().BaseType.GetField("OnValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
						var onValueChanged = onValueChangedField.GetValue(subcriberTarget);
						var onValueChangedTarget = ((EventHandler)onValueChanged).Target;
						Assert($"DeclarationValueChangedAnnouncer should be disposed in {onValueChangedTarget.GetType()}.", !(onValueChangedTarget is USImportSupplierHeaderUserControl));
					}
					else
					{
						Assert($"Method {subcriber.Value.Method.Name} should be detached in {subcriberTarget.GetType()}.", !(subcriberTarget is USImportSupplierHeaderUserControl));
					}
				}
			}
		}

		protected override Type UserControlToBashType => typeof(USImportSupplierHeaderUserControl);

		void SendDeleteKeys(ZGrid grid, params int[] rowIndexToSelect)
		{
			grid.Select();
			foreach (int index in rowIndexToSelect)
			{
				grid.Select(index);
			}

			Application.DoEvents();
			KeySender.PostKeyDown(grid, Keys.Delete);
			Application.DoEvents();
		}

		void AssertGridColumnMandatory(ZGrid chargeGrid, ZString[] expectedMandatoryColumns)
		{
			for (int i = 0; i < expectedMandatoryColumns.Length; i++)
			{
				ZGridColumn column = chargeGrid.Columns[expectedMandatoryColumns[i]];
				AssertEquals(true, column.IsMandatory);
			}
		}

		void AssertChargeGrid(ZGrid chargeGrid, ZString[] expectedChargesColumnNamesInSortOrderList)
		{
			for (int i = 0; i < expectedChargesColumnNamesInSortOrderList.Length; i++)
			{
				ZGridColumnInfo columnInfo = chargeGrid.ColumnStyles[i] as ZGridColumnInfo;
				AssertNotNull(columnInfo);
				ZString expectedColumnName = expectedChargesColumnNamesInSortOrderList[i];
				AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
			}

			Assert("chargeGrid.ColumnStyles.Count must have at least " + expectedChargesColumnNamesInSortOrderList.Length.ToString(), expectedChargesColumnNamesInSortOrderList.Length <= chargeGrid.ColumnStyles.Count);
		}
	}
}
