using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ExportInvoiceLineUserControlTest : Testing.CustomsUserControlBasherAbstractTest
	{
		public void TestGridLayoutContext()
		{
			using (USExportInvoiceLineUserControl control = new USExportInvoiceLineUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestCanShowTariffModuleSearch()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoice.US_TariffType = TariffTypeList.Codes.HTS;
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = string.Empty;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;

				grid.CurrentCell = new DataGridCell(0, 5);
				var columnNumber = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(JobComInvoiceLine.Schema.JI_FormattedTariff, columnNumber.ColumnName);

				var findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)(((ZCodeFindBoxColumnStyle)columnNumber.ColumnStyle).EditControl);
				findBox.PopupButton.PerformClick();
				AssertEquals(typeof(ZArchitecture.GUI.Internal.EmbeddedModulePopup), ((IFindBox)findBox).PopupForm.GetType());
			}

			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			Enterprise.ZArchitecture.Environment.DataRegistry.Instance.BorderWiseEnableWebSocketClient = false;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var grid = userControl.CustomsInvoiceLinesBoundGrid;

				grid.CurrentCell = new DataGridCell(0, 5);
				var columnNumber = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals(JobComInvoiceLine.Schema.JI_FormattedTariff, columnNumber.ColumnName);

				var findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)(((ZCodeFindBoxColumnStyle)columnNumber.ColumnStyle).EditControl);
				findBox.PopupButton.PerformClick();
				AssertEquals(typeof(FindBoxWrapperForBorderWise), ((IFindBox)findBox).PopupForm.GetType());
			}
		}

		public void TestChangingFromHTSToScheduleBDoesNotThrowExceptionOnGrid()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_TariffType = TariffTypeList.Codes.HTS;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
				var columnStyle = userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff);
				AssertEquals("Tariff", columnStyle.Caption);
				ZArchitecture.GUI.Internal.ZPopupFindBox findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)((ZCodeFindBoxColumnStyle)userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_FormattedTariff].ColumnStyle).EditControl;

				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				columnStyle = userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FormattedTariff);
				AssertEquals("Tariff", columnStyle.Caption);
				findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)((ZCodeFindBoxColumnStyle)userControl.CustomsInvoiceLinesBoundGrid.Columns[JobComInvoiceLine.Schema.JI_FormattedTariff].ColumnStyle).EditControl;
			}
		}

		public void TestChangingECCNVisibilityFromLicenseType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var refCusCodeC32 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "1C352", "C32", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC32PK = refCusCodeC32.PK;

			var refCusCodeC31 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "1C341", "C31", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC31PK = refCusCodeC31.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC32PK, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, "C31");

			var refCusCodeC30 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "C2430", "C30", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var refCusCodeC30PK = refCusCodeC30.PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeC30PK, Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, "C30");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "TEST ECCN Controls";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new USExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");

					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					control.LineDetailTabControl.SelectedTab = control.LicenseDDTCDetailsTabPage;
					control.ECCNCodeFindBox.IsAccessible = true;
					AssertEquals("LicenseDDTCDetailsTabPage", true, control.LicenseDDTCDetailsTabPage.TabVisible);

					invoiceLine.JI_Description = "TEST ECCN Controls";
					AssertEquals("No list on ECCNList", false, control.ECCNCodeFindBox.Visible);
					AssertEquals("Let user enter a value", true, control.ECCNTextBox.Visible);

					invoiceLine.US_LicenseType = "C31";
					AssertEquals("US_ECCNList.Count", true, invoiceLine.ECCNCodeFindBoxVisible);
					AssertEquals("US_ECCNList.Count", false, invoiceLine.ECCNTextBoxVisible);
					AssertEquals("Select ECCN Number from drop down list", true, control.ECCNCodeFindBox.Visible);
					AssertEquals("Let user select a value from dropdown list", false, control.ECCNTextBox.Visible);

					invoiceLine.US_LicenseType = "C32";
					AssertEquals("No list on ECCNList", false, control.ECCNCodeFindBox.Visible);
					AssertEquals("Let user enter a value", true, control.ECCNTextBox.Visible);

					invoiceLine.US_LicenseType = "C30";
					AssertEquals("Select ECCN Number from drop down list - C32", true, control.ECCNCodeFindBox.Visible);
					AssertEquals("Let user select a value from dropdown list - C32", false, control.ECCNTextBox.Visible);
				}
			}
		}

		public void TestExportLookupWithTariffTypeOnGridWithHTS()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_PaymentAmount = 1m;
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1m;
				invoiceLine.US_TariffType = TariffTypeList.Codes.HTS;

				var grid = userControl.CustomsInvoiceLinesBoundGrid;

				grid.CurrentCell = new DataGridCell(0, 3);
				var columnNumber = grid.Columns[grid.CurrentCell.ColumnNumber];
				AssertEquals("JI_CC", columnNumber.ColumnName);

				var findBox = (ZArchitecture.GUI.Internal.ZPopupFindBox)(((ZCodeFindBoxColumnStyle)columnNumber.ColumnStyle).EditControl);
				findBox.PopupButton.PerformClick();
				AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.ImportClassification, findBox.ModuleID);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_LinePrice = 1m;
				invoiceLine2.US_TariffType = TariffTypeList.Codes.ScheduleB;

				grid.CurrentCell = new DataGridCell(1, 3);
				findBox.PopupButton.PerformClick();
				AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.ExportClassification, findBox.ModuleID);

				invoiceLine2.US_TariffType = TariffTypeList.Codes.HTS;
				AssertEquals(invoiceLine2.US_TariffType, TariffTypeList.Codes.HTS);
				grid.CurrentCell = new DataGridCell(1, 3);
				findBox.PopupButton.PerformClick();
				AssertEquals(Enterprise.ZArchitecture.Modules.ModuleIDs.ImportClassification, findBox.ModuleID);
			}
		}

		public void TestAddDefaultInvoiceForSingleSED()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			declaration.JE_DeclarationReference = "HELLO WORLD";

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();

				AssertEquals("PreCondition: No Invoice", 0, declaration.Invoices.Count);
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				AssertEquals("Invoice Header should be created", 1, declaration.Invoices.Count);
				AssertEquals("Invoice.JZ_InvoiceNumber", declaration.JE_DeclarationReference, declaration.Invoices[0].JZ_InvoiceNumber);
			}
		}

		public void TestVehicleDetailsControlVisibilityChanged()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "HELLO WORLD";
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.US_IsUsedVehicle = true;
			using (ZForm form = new ZForm(declaration))
			{
				using (USExportInvoiceLineUserControl control = new USExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");
					AssertVehicleDetailsControlVisibility(control, control.UsedVehicleCheckBox.Checked);

					control.UsedVehicleCheckBox.Checked = false;
					AssertVehicleDetailsControlVisibility(control, control.UsedVehicleCheckBox.Checked);
				}
			}
		}

		public void TestColumnNamesInSortOrder()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (ZForm form = new ZForm(declaration))
			{
				using (USExportInvoiceLineUserControl control = new USExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					Assert("Control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Count must have at least " + ExpectedColumnNamesInSortOrderList.Count.ToString(), ExpectedColumnNamesInSortOrderList.Count <= control.CustomsInvoiceLinesBoundGrid.ColumnStyles.Count);
					for (int i = 0; i < ExpectedColumnNamesInSortOrderList.Count; i++)
					{
						ZGridColumnInfo columnInfo = control.CustomsInvoiceLinesBoundGrid.ColumnStyles[i] as ZGridColumnInfo;
						AssertNotNull(columnInfo);
						ZString expectedColumnName = ExpectedColumnNamesInSortOrderList[i];
						AssertEquals("Expected", expectedColumnName, columnInfo.ColumnName);
					}
				}
			}
		}

		public void TestExportPGATabs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "TEST EXPORT PGA";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			using (var form = new ZForm(declaration))
			{
				using (var control = new USExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");

					AssertEquals("AMS/EPA Tab should be invisible as AMS Indicator is empty.", false, control.AMSPGATabPage.TabVisible);
					AssertEquals("AMS/EPA Tab should be invisible as EPA Indicator is empty.", false, control.AMSPGATabPage.TabVisible);
					AssertEquals("NMFS Tab should be invisible as NMFS Indicator is empty.", false, control.NMFSPGATabPage.TabVisible);
					AssertEquals("ATF Tab should be invisible as ATF Indicator is empty.", false, control.ATFPGATabPage.TabVisible);
					AssertEquals("DEA Tab should be invisible as DEA Indicator is empty.", false, control.DEAPGATabPage.TabVisible);
					AssertEquals("FWS Tab should be invisible as FWS Indicator is empty.", false, control.FWSPGATabPage.TabVisible);
					AssertEquals("TTB Tab should be invisible as TTB Indicator is empty.", false, control.TTBPGATabPage.TabVisible);

					invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("AMS/EPA Tab should be visible as AMS Indicator is 'D'.", true, control.AMSPGATabPage.TabVisible);

					invoiceLine.US_AMSInd = ZString.Empty;
					invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
					AssertEquals("AMS/EPA Tab should be visible as EPA Indicator is 'D'.", true, control.AMSPGATabPage.TabVisible);

					invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("NMFS Tab should be visible as NMFS Indicator is 'D'.", true, control.NMFSPGATabPage.TabVisible);

					invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("ATF Tab should be visible as ATF Indicator is 'D'.", true, control.ATFPGATabPage.TabVisible);

					invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("DEA Tab should be visible as DEA Indicator is 'D'.", true, control.DEAPGATabPage.TabVisible);

					invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("FWS Tab should be visible as FWS Indicator is 'D'.", true, control.FWSPGATabPage.TabVisible);

					invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
					AssertEquals("TTB Tab should be visible as TTB Indicator is 'D'.", true, control.TTBPGATabPage.TabVisible);

					invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
					AssertEquals("TTB Tab should be visible as TTB Indicator is 'C'.", true, control.TTBPGATabPage.TabVisible);
				}
			}
		}

		public void TestExportDeaSwitchByInvoiceLineChange()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_PaymentAmount = 1m;

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "1234";
				invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
				var deaLine = invoiceLine.DEAHeaders.AddNew();
				deaLine.US_DrugCode = "1111";

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.DEAPGATabPage;
				userControl.LoadDEAUserControls();
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				grid.SelectSingleElement(invoiceLine2);

				invoiceLine2.JI_PartNo = "11100";
				invoiceLine2.US_DEAInd = OGAIndicatorList.Codes.Declared;
				var deaLine2 = invoiceLine2.DEAHeaders.AddNew();
				deaLine2.US_DrugCode = "2222";

				grid.SelectSingleElement(invoiceLine);

				var deaGrid = userControl.exportDEAUserControl.Controls.Find("DEAGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("1111", deaGrid[0, 0].ToString());
			}
		}

		public void TestExportTTBSwitchByInvoiceLineChange()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_PaymentAmount = 1m;

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "1234";
				invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
				var ttbLine = invoiceLine.TTBLines.AddNew();
				ttbLine.US_NumberForIRC = "1111";

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.TTBPGATabPage;
				userControl.LoadTTBUserControls();
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				grid.SelectSingleElement(invoiceLine2);

				invoiceLine2.JI_PartNo = "11100";
				invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Declared;
				var ttbLine2 = invoiceLine2.TTBLines.AddNew();
				ttbLine2.US_NumberForIRC = "2222";

				grid.SelectSingleElement(invoiceLine);

				var ttbGrid = userControl.exportTTBUserControl.Controls.Find("TTBGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("1111", ttbGrid[0, 0].ToString());
			}
		}

		public void TestExportNMFSSwitchByInvoiceLineChange()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_PaymentAmount = 1m;

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_PartNo = "1234";
				invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
				var nmfsLine = invoiceLine.NMFSLines.AddNew();
				nmfsLine.US_IFTPPermitNumber = "1111";

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USExportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.NMFSPGATabPage;
				userControl.LoadNMFSUserControls();
				var grid = userControl.CustomsInvoiceLinesBoundGrid;
				grid.SelectSingleElement(invoiceLine2);

				invoiceLine2.JI_PartNo = "11100";
				invoiceLine2.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
				var nmfsLine2 = invoiceLine2.NMFSLines.AddNew();
				nmfsLine2.US_IFTPPermitNumber = "2222";

				grid.SelectSingleElement(invoiceLine);

				var nmfsGrid = userControl.exportNMFSUserControl.Controls.Find("NMFSHeaderGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals("1111", nmfsGrid[0, 3].ToString());
			}
		}

		public void TestExportATFSwitchByInvoiceLineChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = "1234";
			invoiceLine1.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			var nmfsLine = invoiceLine1.NMFSLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new USExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");

					AssertEquals("ATF Tab should be invisible as ATF Indicator is empty.", false, control.ATFPGATabPage.TabVisible);

					invoiceLine1.US_ATFInd = OGAIndicatorList.Codes.Declared;
					control.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceLine1);
					AssertEquals("ATF Tab should be visible as ATF Indicator is 'D'.", true, control.ATFPGATabPage.TabVisible);
					control.LoadATFUserControls();
					invoiceLine1.US_ATFInd = string.Empty;
					invoiceLine1.US_ATFInd = OGAIndicatorList.Codes.Declared;

					AssertEquals("should bind to the object selected.", invoiceLine1, control.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
					AssertEquals("should bind to the object selected.", invoiceLine1.ExportATF, control.exportATFUserControl.CurrentDataItem);

					invoiceLine2.US_ATFInd = OGAIndicatorList.Codes.Declared;
					control.CustomsInvoiceLinesBoundGrid.SelectSingleElement(invoiceLine2);
					AssertEquals("should bind to the object selected.", invoiceLine2, control.CustomsInvoiceLinesBoundGrid.ListManager.GetCurrent());
					AssertEquals("should bind to the object selected.", invoiceLine2.ExportATF, control.exportATFUserControl.CurrentDataItem);
				}
			}
		}

		[RequiresSTA]
		public void TestUniversalTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine1.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine2.US_TariffType = TariffTypeList.Codes.ScheduleB;

			using (var form = new JobDeclarationForm(declaration))
			{
				using (var control = new USExportInvoiceLineUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(declaration, "");
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					AssertEquals(false, control.FindSingle<TariffFindBox>("TariffCodeFindBox").Visible);

					var grid = control.CustomsInvoiceLinesBoundGrid;
					grid.CurrentCell = new DataGridCell(0, 0);
					AssertEquals(ClassificationType.EXP, control.GetUniversalTariffType());

					grid.CurrentCell = new DataGridCell(1, 0);
					AssertEquals(TariffTypeList.Codes.ScheduleB, control.GetUniversalTariffType());
				}
			}
		}

		public void TestECCNColumnCharacterCasing()
		{
			using (var control = new USExportInvoiceLineUserControl())
			{
				var grid = control.CustomsInvoiceLinesBoundGrid;
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == "US_ECCN");
				AssertEquals(CharacterCasing.Upper, column.CharacterCasing);
			}
		}

		protected override Type UserControlToBashType => typeof(USExportInvoiceLineUserControl);

		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Export;

		void AssertVehicleDetailsControlVisibility(USExportInvoiceLineUserControl control, bool expectedValue)
		{
			AssertEquals("VehicleIDTextBox.Visible", expectedValue, control.VehicleIDTextBox.Visible);
			AssertEquals("VehicleIDTypeDropEdit.Visible", expectedValue, control.VehicleIDTypeDropEdit.Visible);
			AssertEquals("VehicleTitleNoTextBox.Visible", expectedValue, control.VehicleTitleNoTextBox.Visible);
			AssertEquals("VehicleTitleStateDropEdit.Visible", expectedValue, control.VehicleTitleStateDropEdit.Visible);
		}

		List<ZString> expectedColumnNamesInSortOrderList;
		List<ZString> ExpectedColumnNamesInSortOrderList
		{
			get
			{
				if (expectedColumnNamesInSortOrderList == null)
				{
					expectedColumnNamesInSortOrderList = new List<ZString>();
					expectedColumnNamesInSortOrderList.Add("JI_LineNo");
					expectedColumnNamesInSortOrderList.Add("JI_Calc_Invoice");
					expectedColumnNamesInSortOrderList.Add("JI_PartNo");
					expectedColumnNamesInSortOrderList.Add("JI_CC");
					expectedColumnNamesInSortOrderList.Add("US_TariffType");
					expectedColumnNamesInSortOrderList.Add("JI_FormattedTariff");
					expectedColumnNamesInSortOrderList.Add("JI_InvoiceQuantity");
					expectedColumnNamesInSortOrderList.Add("JI_InvoiceUQ");
					expectedColumnNamesInSortOrderList.Add("JI_CustomsQuantity");
					expectedColumnNamesInSortOrderList.Add("JI_CustomsUnitQty");
					expectedColumnNamesInSortOrderList.Add("JI_CustomsSecondQuantity");
					expectedColumnNamesInSortOrderList.Add("JI_CustomsSecondUnitQty");
					expectedColumnNamesInSortOrderList.Add("JI_LinePrice");
					expectedColumnNamesInSortOrderList.Add("JI_Description");
					expectedColumnNamesInSortOrderList.Add("US_MarksAndNumbers");
					expectedColumnNamesInSortOrderList.Add("US_ExportCode");
					expectedColumnNamesInSortOrderList.Add("US_ECCN");
					expectedColumnNamesInSortOrderList.Add("US_LicenseType");
					expectedColumnNamesInSortOrderList.Add("US_LicenseNo");
					expectedColumnNamesInSortOrderList.Add("US_LicenseValue");
					expectedColumnNamesInSortOrderList.Add("US_AESOriginIndicator");
					expectedColumnNamesInSortOrderList.Add("US_IsUsedVehicle");
					expectedColumnNamesInSortOrderList.Add("US_VehicleID");
					expectedColumnNamesInSortOrderList.Add("US_VehicleIDType");
					expectedColumnNamesInSortOrderList.Add("US_VehicleTitleNo");
					expectedColumnNamesInSortOrderList.Add("US_VehicleTitleState");
					expectedColumnNamesInSortOrderList.Add("JI_Calc_MergedLineNumber");
					expectedColumnNamesInSortOrderList.Add("JI_Calc_EntryNumber");
					expectedColumnNamesInSortOrderList.Add("JI_Calc_XTN");
				}
				return expectedColumnNamesInSortOrderList;
			}
		}
	}
}
