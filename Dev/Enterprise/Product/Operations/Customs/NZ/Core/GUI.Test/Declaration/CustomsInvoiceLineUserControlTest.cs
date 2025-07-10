using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class CustomsInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestPlugins()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			{
				var userControl = new CustomsInvoiceLineUserControl();
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();
				AssertNotNull("Should have MAFeBACCaInvoiceLinePlugin.", userControl.LineDetailTabControl.PlugIns.GetPlugIn(Enterprise.ZArchitecture.Modules.ControllerIDs.Customs.NZ.MAFeBACCaInvoiceLinePlugin));
			}
		}

		public void TestTariffControlsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				AssertEquals(true, invoiceLineUserControl.NZCClassificationFindBox.Visible);
				AssertEquals(true, invoiceLineUserControl.ConcessionCodeFindBox.Visible);
				AssertEquals(false, invoiceLineUserControl.TariffCodeFindBox.Visible);
				AssertEquals(false, invoiceLineUserControl.ConcessionCodeDropEdit.Visible);

				var miscTab = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("MiscTabPage");
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = miscTab;
				AssertEquals(true, invoiceLineUserControl.JI_PartsOfClassificationNZcClassFindBox.Visible);
				AssertEquals(false, invoiceLineUserControl.PartsOfClassificationFindBox.Visible);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				AssertEquals(false, invoiceLineUserControl.NZCClassificationFindBox.Visible);
				AssertEquals(false, invoiceLineUserControl.ConcessionCodeFindBox.Visible);
				AssertEquals(true, invoiceLineUserControl.TariffCodeFindBox.Visible);
				AssertEquals(true, invoiceLineUserControl.ConcessionCodeDropEdit.Visible);

				var miscTab = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("MiscTabPage");
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = miscTab;
				AssertEquals(false, invoiceLineUserControl.JI_PartsOfClassificationNZcClassFindBox.Visible);
				AssertEquals(true, invoiceLineUserControl.PartsOfClassificationFindBox.Visible);
			}
		}

		public void TestTariffFindBoxEffectiveDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfArrival = new ZDateTime(2023, 6, 1);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var control = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				var invoiceGrid = control.CustomsInvoiceLinesBoundGrid;

				AssertEquals(declaration.JE_DateOfArrival, control.TariffCodeFindBox.GetEffectiveDate.Invoke());
				AssertEquals(declaration.JE_DateOfArrival, control.PartsOfClassificationFindBox.GetEffectiveDate.Invoke());
				AssertEquals(declaration.JE_DateOfArrival, (invoiceGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Tariff) as Universal.GUI.TariffColumnStyleInfo).GetEffectiveDate.Invoke());
			}
		}

		public void TestOriginRegionTextBoxVisibility()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			using (var invoiceLineUserControl = new CustomsInvoiceLineUserControl())
			{
				invoiceLineUserControl.JobDeclaration = declaration;
				AssertEquals("Hide Origin Textbox", false, invoiceLineUserControl.FindSingle<ZTextBox>("JI_OriginRegionTextBox").Visible);
				AssertEquals("Hide Effective Origin Textbox", false, invoiceLineUserControl.FindSingle<ZTextBox>("JI_EffectiveOriginRegionTextBox").Visible);
			}
		}

		public void TestMessageTypeChangeControlVisibility()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Simplified);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Temporary);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Sight);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Periodic);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Completion);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, true, false, true);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, false, false, false, false, false, false, false);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Drawback);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, true, false, false, true, true, false, false, false);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Completion);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, true, false, false, true, true, true, false, false);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Excise, JobMessageSubTypeList.Codes.Excise);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, false, false, false, false, false, true, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubTypeForTSWDec(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsImport);
			}
		}

		public void TestMessageTypeChangeControlVisibilityWhenUseRefDb()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Simplified);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Temporary);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Sight);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Periodic);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, false, false, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Completion);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, true, true, false, false, true, false, true);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, false, false, false, false, false, false, false);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Drawback);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, true, false, false, true, true, false, false, false);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Completion);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, true, false, false, true, true, true, false, false);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Excise, JobMessageSubTypeList.Codes.Excise);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckAllFieldsVisibility(invoiceLineUserControl, false, false, false, false, false, false, true, true);
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsExport);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
				SetupForMessageTypeAndSubTypeForTSWDec(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
				CheckTSWFields(invoiceLineUserControl, declaration.IsTSWDeclaration, declaration.IsImport);
			}
		}

		public void TestOrderOfColumnsAndNewTitles()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var invoiceLineUserControl = new CustomsInvoiceLineUserControl())
			{
				CheckCaption(invoiceLineUserControl, 0, "Inv. Line#");
				CheckCaption(invoiceLineUserControl, 1, "Merged Line#");
				CheckCaption(invoiceLineUserControl, 2, "Invoice");
				CheckCaption(invoiceLineUserControl, 3, "Product Code");
				CheckCaption(invoiceLineUserControl, 4, "Lookup Code");
				CheckCaption(invoiceLineUserControl, 5, "Tariff Code");
				CheckCaption(invoiceLineUserControl, 6, "Concession Code");
				CheckCaption(invoiceLineUserControl, 7, "Goods Description");
				CheckCaption(invoiceLineUserControl, 8, "Inv. Qty");
				CheckCaption(invoiceLineUserControl, 9, "Inv. UQ");
				CheckCaption(invoiceLineUserControl, 10, "Unit Price");
				CheckCaption(invoiceLineUserControl, 11, "Line Price");
				CheckCaption(invoiceLineUserControl, 12, "Origin");
				CheckCaption(invoiceLineUserControl, 13, "Export");
				CheckCaption(invoiceLineUserControl, 14, "Pref");
				CheckCaption(invoiceLineUserControl, 15, "Pref Group");
				CheckCaption(invoiceLineUserControl, 16, "Cust. Qty");
				CheckCaption(invoiceLineUserControl, 17, "Cust. UQ");
				CheckCaption(invoiceLineUserControl, 18, "Supp. Qty");
				CheckCaption(invoiceLineUserControl, 19, "Supp. UQ");
				CheckCaption(invoiceLineUserControl, 20, "Duty Zero Rated");
				CheckCaption(invoiceLineUserControl, 21, "Excise Zero Rated");
				CheckCaption(invoiceLineUserControl, 22, "Levies Zero Rated");
				CheckCaption(invoiceLineUserControl, 23, "GST Zero Rated");
				CheckCaption(invoiceLineUserControl, 24, "Anti Dumping Duty");
				CheckCaption(invoiceLineUserControl, 25, "Countervailing Duty");
				CheckCaption(invoiceLineUserControl, 26, "Duty Credit");
				CheckCaption(invoiceLineUserControl, 27, "GST Credit");
				CheckCaption(invoiceLineUserControl, 28, "Levy Credit");
				CheckCaption(invoiceLineUserControl, 29, "Levy Credit Code");
				CheckCaption(invoiceLineUserControl, 30, "Deposit Refund");
				CheckCaption(invoiceLineUserControl, 31, "Excise Duty Credit");
				CheckCaption(invoiceLineUserControl, 32, "Commodity Code");
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var invoiceLineUserControl = new CustomsInvoiceLineUserControl())
			{
				CheckCaption(invoiceLineUserControl, 0, "Inv. Line#");
				CheckCaption(invoiceLineUserControl, 1, "Merged Line#");
				CheckCaption(invoiceLineUserControl, 2, "Invoice");
				CheckCaption(invoiceLineUserControl, 3, "Product Code");
				CheckCaption(invoiceLineUserControl, 4, "Lookup Code");
				CheckCaption(invoiceLineUserControl, 5, "Tariff Code");
				CheckCaption(invoiceLineUserControl, 6, "Concession Code");
				CheckCaption(invoiceLineUserControl, 7, "Goods Description");
				CheckCaption(invoiceLineUserControl, 8, "Inv. Qty");
				CheckCaption(invoiceLineUserControl, 9, "Inv. UQ");
				CheckCaption(invoiceLineUserControl, 10, "Unit Price");
				CheckCaption(invoiceLineUserControl, 11, "Line Price");
				CheckCaption(invoiceLineUserControl, 12, "Origin");
				CheckCaption(invoiceLineUserControl, 13, "Export");
				CheckCaption(invoiceLineUserControl, 14, "Pref");
				CheckCaption(invoiceLineUserControl, 15, "Pref Group");
				CheckCaption(invoiceLineUserControl, 16, "Cust. Qty");
				CheckCaption(invoiceLineUserControl, 17, "Cust. UQ");
				CheckCaption(invoiceLineUserControl, 18, "Supp. Qty");
				CheckCaption(invoiceLineUserControl, 19, "Supp. UQ");
				CheckCaption(invoiceLineUserControl, 20, "Duty Zero Rated");
				CheckCaption(invoiceLineUserControl, 21, "Excise Zero Rated");
				CheckCaption(invoiceLineUserControl, 22, "Levies Zero Rated");
				CheckCaption(invoiceLineUserControl, 23, "GST Zero Rated");
				CheckCaption(invoiceLineUserControl, 24, "Anti Dumping Duty");
				CheckCaption(invoiceLineUserControl, 25, "Countervailing Duty");
				CheckCaption(invoiceLineUserControl, 26, "Duty Credit");
				CheckCaption(invoiceLineUserControl, 27, "GST Credit");
				CheckCaption(invoiceLineUserControl, 28, "Levy Credit");
				CheckCaption(invoiceLineUserControl, 29, "Levy Credit Code");
				CheckCaption(invoiceLineUserControl, 30, "Deposit Refund");
				CheckCaption(invoiceLineUserControl, 31, "Excise Duty Credit");
				CheckCaption(invoiceLineUserControl, 32, "Commodity Code");
			}
		}

		public void TestInvoiceLineGridColumnsMandatory()
		{
			var registryInstance = NZCustomsDataRegistry.Instance;
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var declaration = JobDeclaration.New(Factory);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				using (var declarationForm = new DeclarationForm(declaration))
				{
					CustomsInvoiceLineUserControl invoiceLineUserControl = null;
					ZGrid invoiceLineGrid = null;
					declarationForm.Show();
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckColumnsManadatory("IMP/NOR/CUS ", invoiceLineGrid, false, false, false, false, false, false, false);
					AssertEquals("Description Back to visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("PartNo Stay visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					invoiceLineGrid.SetColumnVisible(false, JobComInvoiceLine.Schema.JI_Description);
					invoiceLineGrid.SetColumnVisible(false, JobComInvoiceLine.Schema.JI_PartNo);
					AssertEquals("Description Back to visible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay visible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckColumnsManadatory("IMP/NOR/TSW ", invoiceLineGrid, true, true, true, true, true, true, true);
					AssertEquals("Description Back to visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay Invisible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					invoiceLineGrid.SetColumnVisible(true, JobComInvoiceLine.Schema.JI_PartNo);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckColumnsManadatory("EXP/NOR/CUS ", invoiceLineGrid, false, false, false, false, false, false, false);
					AssertEquals("Description Back to visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					invoiceLineGrid.SetColumnVisible(false, JobComInvoiceLine.Schema.JI_Description);
					invoiceLineGrid.SetColumnVisible(false, JobComInvoiceLine.Schema.JI_PartNo);
					AssertEquals("Description Back to visible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay visible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckColumnsManadatory("EXP/NOR/TSW ", invoiceLineGrid, true, true, true, true, true, true, true);
					AssertEquals("Description Back to visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay Invisible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					invoiceLineGrid.SetColumnVisible(true, JobComInvoiceLine.Schema.JI_PartNo);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.WriteOff);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckColumnsManadatory("EXP/CRE/CUS ", invoiceLineGrid, false, false, false, false, false, false, false);
					AssertEquals("Description Back to visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					invoiceLineGrid.SetColumnVisible(false, JobComInvoiceLine.Schema.JI_Description);
					invoiceLineGrid.SetColumnVisible(false, JobComInvoiceLine.Schema.JI_PartNo);
					AssertEquals("Description Back to visible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay visible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.WriteOff);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckColumnsManadatory("EXP/CRE/TSW ", invoiceLineGrid, true, true, true, true, true, true, true);
					AssertEquals("Description Back to visible", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsVisible);
					AssertEquals("JI_PartNo Stay Invisible", false, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PartNo).IsVisible);
				}
			}

			);
		}

		public void TestPackageDetailsOnInvoiceLineGrid()
		{
			var registryInstance = NZCustomsDataRegistry.Instance;
			var currentCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var declaration = JobDeclaration.New(Factory);
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				using (var declarationForm = new DeclarationForm(declaration))
				{
					CustomsInvoiceLineUserControl invoiceLineUserControl = null;
					ZGrid invoiceLineGrid = null;
					declarationForm.Show();
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckPackagingColumns("IMP/NOR/CUS ", invoiceLineGrid, false, false, true);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckPackagingColumns("IMP/NOR/TSW ", invoiceLineGrid, true, true, true);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckPackagingColumns("EXP/NOR/CUS ", invoiceLineGrid, false, false, false);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckPackagingColumns("EXP/NOR/TSW ", invoiceLineGrid, true, true, false);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.WriteOff);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckPackagingColumns("EXP/CRE/CUS ", invoiceLineGrid, false, false, false);
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
					SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.WriteOff);
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					invoiceLineUserControl = declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl as CustomsInvoiceLineUserControl;
					invoiceLineGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
					CheckPackagingColumns("EXP/CRE/TSW ", invoiceLineGrid, false, false, false);
				}
			}

			);
		}

		public void TestInvoiceLineContainerDetails()
		{
			using (var invoiceLineUserControl = new CustomsInvoiceLineUserControlForTest())
			{
				Assert("SupportsExtraPhysicalQuantitiesOnC2Pivot is true", invoiceLineUserControl.SupportsExtraPhysicalQuantitiesOnC2PivotExposed);
				var containersGrid = invoiceLineUserControl.FindSingle<ZGrid>("CusContainerInvoiceLineGrid");
				AssertNotNull("Has containersGrid", containersGrid);
				CombineAssertions(() =>
				{
					AssertNotNull("ContainerNumber Column exists", containersGrid.GetColumnStyle("ContainerNumber"));
					AssertNotNull("ContainerWeight Column exists", containersGrid.GetColumnStyle("ContainerWeight"));
					AssertNotNull("GrossWeightInKG Column exists", containersGrid.GetColumnStyle("GrossWeightInKG"));
					AssertNotNull("NetWeightInKG Column exists", containersGrid.GetColumnStyle("NetWeightInKG"));
					AssertNotNull("PackQty Column exists", containersGrid.GetColumnStyle("PackQty"));
					AssertNotNull("SplitValue Column exists", containersGrid.GetColumnStyle("SplitValue"));
					AssertNotNull("SplitValueCurrency Column exists", containersGrid.GetColumnStyle("SplitValueCurrency"));
				}

				);
			}
		}

		public void TestInvoiceLineGridColumnAvailability()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			using (var declarationForm = new DeclarationForm(declaration))
			{
				declarationForm.Show();
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
				var invoiceLinesGrid = ReloadCustomsInvoiceLinesBoundGrid(declarationForm);
				Assert("invoiceLinesGrid Visible", invoiceLinesGrid.Visible);
				CombineAssertions("TSW IMPORT", () =>
				{
					AssertGridColumnType(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_IntendedUse, typeof(ZTextBoxColumnStyleInfo), isAvailable: true, isVisible: false, isMandatory: false, groupName: "Intended Use");
					AssertGridColumnType(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_IntendedUseCode, typeof(ZDropEditColumnStyleInfo), isAvailable: true, isVisible: false, isMandatory: false, groupName: "Intended Use");
					AssertGridColumnType(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_OriginRegion, typeof(ZTextBoxColumnStyleInfo), isAvailable: true, isVisible: false, isMandatory: false, groupName: null);
				}

				);
				var columnAvailability = new Dictionary<string, bool>();
				columnAvailability[JobComInvoiceLine.Schema.JI_IntendedUse] = false;
				columnAvailability[JobComInvoiceLine.Schema.JI_IntendedUseCode] = false;
				columnAvailability[JobComInvoiceLine.Schema.JI_OriginRegion] = true;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
				AssertGridColumnAvailability("TSW EXPORT", declarationForm, columnAvailability);
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Import, JobMessageSubTypeList.Codes.Normal);
				AssertGridColumnAvailability("!TSW IMPORT", declarationForm, columnAvailability);
				columnAvailability[JobComInvoiceLine.Schema.JI_OriginRegion] = false;
				SetupForMessageTypeAndSubType(declaration, JobMessageTypeList.Codes.Export, JobMessageSubTypeList.Codes.Normal);
				AssertGridColumnAvailability("!TSW EXPORT", declarationForm, columnAvailability);
			}
		}

		public void TestDisplayCreateClassificationAssistantRequestMenu()
		{
			var declaration = JobDeclaration.New(Factory);
			using (var invoiceLineUserControl = new CustomsInvoiceLineUserControlForTest())
			{
				invoiceLineUserControl.JobDeclaration = declaration;
				declaration.JE_MessageType = JobMessageTypeList.Codes.WriteOff;
				Assert("NZ should not display menu 'Create Classification Assistant Request' for ECI declaration", !invoiceLineUserControl.DisplayCreateClassificationAssistantRequestMenuForTest);

				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				Assert("NZ should not display menu 'Create Classification Assistant Request' for MSC declaration", !invoiceLineUserControl.DisplayCreateClassificationAssistantRequestMenuForTest);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert("NZ should display menu 'Create Classification Assistant Request' for IMP declaration", invoiceLineUserControl.DisplayCreateClassificationAssistantRequestMenuForTest);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert("NZ should display menu 'Create Classification Assistant Request' for EXP declaration", invoiceLineUserControl.DisplayCreateClassificationAssistantRequestMenuForTest);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
				Assert("NZ should display menu 'Create Classification Assistant Request' for EXC declaration", invoiceLineUserControl.DisplayCreateClassificationAssistantRequestMenuForTest);
			}
		}

		#region Implementation
		void CheckCaption(CustomsInvoiceLineUserControl invoiceLineUserControl, int index, string expectedCaption)
		{
			AssertEquals("Invalid Caption at index [" + index.ToString() + "].", expectedCaption, ((Core.Forms.ZGridColumnInfo)invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ColumnStyles[index]).Caption);
		}

		void SetupForMessageTypeAndSubType(JobDeclaration declaration, ZString messageType, ZString messageSubType)
		{
			declaration.JE_MessageType = messageType;
			declaration.JE_MessageSubType = messageSubType;
		}

		void SetupForMessageTypeAndSubTypeForTSWDec(JobDeclaration declaration, ZString messageType, ZString messageSubType)
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = messageType;
			declaration.JE_MessageSubType = messageSubType;
		}

		void CheckAllFieldsVisibility(CustomsInvoiceLineUserControl invoiceLineUserControl, bool levyCreditVisible, bool antiDumpingDutyVisible, bool countervailingDutyVisible, bool dutyCreditVisible, bool gSTCreditVisible, bool depositRefundVisible, bool exciseDutyCreditVisible, bool dutyAndGSTCalculationFieldsVisible)
		{
			CheckUserEnteredCharges(invoiceLineUserControl, levyCreditVisible, antiDumpingDutyVisible, countervailingDutyVisible, dutyCreditVisible, gSTCreditVisible, depositRefundVisible, exciseDutyCreditVisible);
			CheckCalculatedFields(invoiceLineUserControl, dutyAndGSTCalculationFieldsVisible, dutyAndGSTCalculationFieldsVisible, dutyAndGSTCalculationFieldsVisible);
			CheckClassificationFields(invoiceLineUserControl, dutyAndGSTCalculationFieldsVisible, dutyAndGSTCalculationFieldsVisible, dutyAndGSTCalculationFieldsVisible, dutyAndGSTCalculationFieldsVisible, dutyAndGSTCalculationFieldsVisible);
		}

		void CheckUserEnteredCharges(CustomsInvoiceLineUserControl invoiceLineUserControl, bool levyCreditVisible, bool antiDumpingDutyVisible, bool countervailingDutyVisible, bool dutyCreditVisible, bool gSTCreditVisible, bool depositRefundVisible, bool exciseDutyCreditVisible)
		{
			var linesGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.LineChargesTabPage;
			var lineDetailsTabPage = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("LineDetailsTabPage");
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = lineDetailsTabPage;
			AssertEquals("LevyCreditVisible.Visible", levyCreditVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("LevyCreditCalcEdit")).Visible);
			AssertEquals("JI_LevyCreditAmount is in Grid", levyCreditVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_LevyCreditAmount));
			AssertEquals("LevyCodeDropEdit.Visible", levyCreditVisible, (invoiceLineUserControl.FindSingle<ZDropEdit>("LevyCodeDropEdit")).Visible);
			AssertEquals("AntiDumpingDutyCalcEdit.Visible", antiDumpingDutyVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("AntiDumpingDutyCalcEdit")).Visible);
			AssertEquals("JI_AntiDumpingDutyAmount is in Grid", antiDumpingDutyVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_AntiDumpingDutyAmount));
			AssertEquals("CountervailingDutyCalcEdit.Visible", countervailingDutyVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("CountervailingDutyCalcEdit")).Visible);
			AssertEquals("JI_CountervailingDutyAmount is in Grid", countervailingDutyVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_CountervailingDutyAmount));
			AssertEquals("DutyCreditCalcEdit.Visible", dutyCreditVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("DutyCreditCalcEdit")).Visible);
			AssertEquals("JI_DutyCreditAmount is in Grid", dutyCreditVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_DutyCreditAmount));
			AssertEquals("GSTCreditCalcEdit.Visible", gSTCreditVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("GSTCreditCalcEdit")).Visible);
			AssertEquals("JI_GSTCreditAmount is in Grid", gSTCreditVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_GSTCreditAmount));
			AssertEquals("DepositRefundCalcEdit.Visible", depositRefundVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("DepositRefundCalcEdit")).Visible);
			AssertEquals("JI_DepositRefundAmount is in Grid", depositRefundVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_DepositRefundAmount));
			AssertEquals("ExciseDutyCreditCalcEdit.Visible", exciseDutyCreditVisible, (invoiceLineUserControl.FindSingle<ZCalcEdit>("ExciseDutyCreditCalcEdit")).Visible);
			AssertEquals("JI_ExciseDutyCreditAmount is in Grid", exciseDutyCreditVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ExciseDutyCreditAmount));
		}

		void CheckCalculatedFields(CustomsInvoiceLineUserControl invoiceLineUserControl, bool dutyVisible, bool levyVisible, bool gSTVisible)
		{
			AssertEquals("JI_Calc_DutyConvertToLocalCurrencyControl.Visible", dutyVisible, invoiceLineUserControl.JI_Calc_DutyConvertToLocalCurrencyControl.Visible);
			AssertEquals("LevyCurrencyControl .Visible", levyVisible, (invoiceLineUserControl.FindSingle<ConvertToLocalCurrencyControl>("LevyCurrencyControl")).Visible);
			AssertEquals("JI_Calc_GSTConvertToLocalCurrencyControl.Visible", gSTVisible, invoiceLineUserControl.JI_Calc_GSTConvertToLocalCurrencyControl.Visible);
		}

		void CheckClassificationFields(CustomsInvoiceLineUserControl invoiceLineUserControl, bool isZeroRatedVisible, bool preferenceAllowedVisible, bool countryOfExportVisible, bool concessionCodeVisible, bool dutyRateVisible)
		{
			var linesGrid = invoiceLineUserControl.CustomsInvoiceLinesBoundGrid;
			var miscTab = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("MiscTabPage");
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = miscTab;
			AssertEquals("JI_IsZeroRatedCheckBox.Visible", isZeroRatedVisible, invoiceLineUserControl.FindSingle<Control>("ZeroRatedGroupBox").Visible);
			AssertEquals("IsZeroRatedAmount is in Grid", isZeroRatedVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedDuty));
			AssertEquals("IsZeroRatedAmount is in Grid", isZeroRatedVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedExcise));
			AssertEquals("IsZeroRatedAmount is in Grid", isZeroRatedVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedLevies));
			AssertEquals("IsZeroRatedAmount is in Grid", isZeroRatedVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_IsZeroRatedGST));
			var lineDetailsTabPage = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("LineDetailsTabPage");
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = lineDetailsTabPage;
			AssertEquals("PreferenceDropEdit.Visible", preferenceAllowedVisible, (invoiceLineUserControl.FindSingle<ZDropEdit>("PreferenceDropEdit")).Visible);
			AssertEquals("PreferentialCountryGroupDropEdit.Visible", preferenceAllowedVisible, invoiceLineUserControl.FindSingle<Control>("PreferentialCountryGroupDropEdit").Visible);
			AssertEquals("JI_QualifiesForPreferentialDuty is in Grid", preferenceAllowedVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_QualifiesForPreferentialDuty));
			AssertEquals("JI_PreferentialCountryGroup is in Grid", preferenceAllowedVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_PreferentialCountryGroup));
			AssertEquals("CountryOfExportCodeFindBox.Visible", countryOfExportVisible, invoiceLineUserControl.FindSingle<Control>("CountryOfExportCodeFindBox").Visible);
			AssertEquals("JI_RN_NKCountryOfExport is in Grid", countryOfExportVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport));
			if (UniversalTariffHelper.UseRefDatabaseData)
			{
				AssertEquals("ConcessionCodeDropEdit.Visible", concessionCodeVisible, invoiceLineUserControl.ConcessionCodeDropEdit.Visible);
			}
			else
			{
				AssertEquals("ConcessionCodeFindBox.Visible", concessionCodeVisible, invoiceLineUserControl.ConcessionCodeFindBox.Visible);
			}
			AssertEquals("JI_ConcessionCode is in Grid", concessionCodeVisible, linesGrid.Columns.Contains(JobComInvoiceLine.Schema.JI_ConcessionCode));
			AssertEquals("DutyRateTextBox.Visible", dutyRateVisible, invoiceLineUserControl.FindSingle<Control>("DutyRateTextBox").Visible);
		}

		void CheckColumnsManadatory(string prefix, ZGrid invoiceLineGrid, bool isInvoiceMandatory, bool isTariffMandatory, bool isLinePriceMandatory, bool isGoodDescriptionMandatory, bool isOriginMandatory, bool isWeightMandatory, bool isNetWeightMandatory)
		{
			AssertEquals(prefix + "LineNo", true, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_LineNo).IsMandatory);
			AssertEquals(prefix + "Invoice", isInvoiceMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Calc_Invoice).IsMandatory);
			AssertEquals(prefix + "Tariff", isTariffMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Tariff).IsMandatory);
			AssertEquals(prefix + "LinPrice", isLinePriceMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_LinePrice).IsMandatory);
			AssertEquals(prefix + "GoodDescription", isGoodDescriptionMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Description).IsMandatory);
			AssertEquals(prefix + "Origin", isOriginMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CountryOfOrigin).IsMandatory);
			AssertEquals(prefix + "Weight", isWeightMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_Weight).IsMandatory);
			AssertEquals(prefix + "NetWeight", isNetWeightMandatory, invoiceLineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NetWeight).IsMandatory);
		}

		void CheckTSWFields(CustomsInvoiceLineUserControl invoiceLineUserControl, bool isTSW, bool isImport)
		{
			AssertEquals("PackagingGroupBox.Visible", isTSW, invoiceLineUserControl.FindSingle<Control>("PackagingGroupBox").Visible);
			var tswTabPage = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("TSWTabPage");
			var dangerousGoodsTabPage = invoiceLineUserControl.LineDetailTabControl.GetTabPageByNameOrText("DangerousGoodsTabPage");
			if (isTSW)
			{
				Assert("TSWTabPage.Visible", tswTabPage.TabVisible);
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = tswTabPage;
				AssertEquals("CommodityDetailsGroupBox.Visible", isImport, invoiceLineUserControl.FindSingle<ZGroupBox>("CommodityDetailsGroupBox").Visible);
				AssertEquals("IntendedUseGroupBox.Visible", isImport, invoiceLineUserControl.FindSingle<ZGroupBox>("IntendedUseGroupBox").Visible);
				AssertEquals("ClassificationGroupBox.Visible", isImport, invoiceLineUserControl.FindSingle<ZGroupBox>("ClassificationGroupBox").Visible);
				AssertEquals("ProductsGroupBox.Visible", isImport, invoiceLineUserControl.FindSingle<ZGroupBox>("ProductsGroupBox").Visible);
				AssertEquals("ConstituentsGroupBox.Visible", isImport, invoiceLineUserControl.FindSingle<ZGroupBox>("ConstituentsGroupBox").Visible);
				AssertEquals("ProductNameGroupBox.Visible", isImport, invoiceLineUserControl.FindSingle<ZGroupBox>("ProductNameGroupBox").Visible);
				AssertNull(dangerousGoodsTabPage);
			}
			else
			{
				AssertNull(tswTabPage);
				Assert("DangerousGoodsTabPage.Visible", dangerousGoodsTabPage.TabVisible);
			}
		}

		void CheckPackagingColumns(string prefix, ZGrid invoiceLineGrid, bool isMandatory, bool isVisible, bool isImport)
		{
			AssertEquals(prefix + "NumberOfPackages1", isMandatory, invoiceLineGrid.GetColumnStyle("NumberOfPackages1").IsMandatory);
			AssertEquals("NumberOfPackages1", isVisible, invoiceLineGrid.GetColumnStyle("NumberOfPackages1").IsVisible);
			AssertEquals(prefix + "Packages1UQ", isMandatory, invoiceLineGrid.GetColumnStyle("Packages1UQ").IsMandatory);
			AssertEquals("Packages1UQ", isVisible, invoiceLineGrid.GetColumnStyle("Packages1UQ").IsVisible);
			AssertEquals(prefix + "PackagesVolume1", isMandatory, invoiceLineGrid.GetColumnStyle("PackagesVolume1").IsMandatory);
			AssertEquals("PackagesVolume1", isVisible, invoiceLineGrid.GetColumnStyle("PackagesVolume1").IsVisible);
			AssertEquals(prefix + "PackageVolume1UQ", isMandatory, invoiceLineGrid.GetColumnStyle("PackageVolume1UQ").IsMandatory);
			AssertEquals("PackageVolume1UQ", isVisible, invoiceLineGrid.GetColumnStyle("PackageVolume1UQ").IsVisible);
			AssertEquals(prefix + "PackagingMarks1", isMandatory, invoiceLineGrid.GetColumnStyle("PackagingMarks1").IsMandatory);
			AssertEquals("PackagingMarks1", isVisible, invoiceLineGrid.GetColumnStyle("PackagingMarks1").IsVisible);
			AssertEquals(prefix + "PackagingMaterial1", false, invoiceLineGrid.GetColumnStyle("PackagingMaterial1").IsMandatory);
			AssertEquals("PackagingMaterial1", isVisible && isImport, invoiceLineGrid.GetColumnStyle("PackagingMaterial1").IsVisible);
		}

		void AssertGridColumnType(ZGrid grid, string columnName, Type columnType, bool isAvailable, bool isVisible, bool isMandatory, string groupName)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertType(columnName + " Is Of Type", columnType, columnStyle);
			AssertEquals(columnName + " Available", isAvailable, !columnStyle.IsUnavailable);
			AssertEquals(columnName + " Visible", isVisible, columnStyle.IsVisible);
			AssertEquals(columnName + " Mandatory", isMandatory, columnStyle.IsMandatory);
			AssertEquals(columnName + " Grouped", groupName, columnStyle.GroupName.Caption);
		}

		void AssertGridColumnAvailability(string message, DeclarationForm declarationForm, Dictionary<string, bool> columnValues)
		{
			var invoiceLinesGrid = ReloadCustomsInvoiceLinesBoundGrid(declarationForm);
			CombineAssertions(message, () =>
			{
				foreach (var columnValue in columnValues)
				{
					var columnName = columnValue.Key;
					var columnStyle = invoiceLinesGrid.GetColumnStyle(columnName);
					AssertEquals(columnName + " Available", columnValue.Value, !columnStyle.IsUnavailable);
				}
			}

			);
		}

		ZGrid ReloadCustomsInvoiceLinesBoundGrid(DeclarationForm declarationForm)
		{
			declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.DeclarationTabPage;
			declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			return declarationForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
		}

		class CustomsInvoiceLineUserControlForTest : CustomsInvoiceLineUserControl
		{
			public CustomsInvoiceLineUserControlForTest() : base()
			{
			}

			public bool SupportsExtraPhysicalQuantitiesOnC2PivotExposed => base.SupportsExtraPhysicalQuantitiesOnC2Pivot;

			public bool DisplayCreateClassificationAssistantRequestMenuForTest => base.DisplayCreateClassificationAssistantRequestMenu;
		}
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			Business.EDITariff_ReferenceFiles_NZ.Testing.NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
		}
	}
}
