using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class CPSCUserControlTest : TestCaseWithFactory
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new CPSCUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(delegate
				{
					control.ViewEditButton.PerformClick();
				});
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(CPSCUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var cpsc = invoiceLine.CPSCHeaders.AddNew();
				control.SetDataBinding(cpsc, "");
				control.HeaderGrid.DataSource = invoiceLine.CPSCHeaders;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(CPSCForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestCPSCUserControl()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew();
			address.OA_OH = orgHeader.PK;
			address.OA_Address1 = "Address";
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup.Code);

			var address1 = orgHeader.Addresses.AddNew();
			address1.OA_OH = orgHeader.PK;
			address1.OA_Address1 = "Address1";
			address1.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			address1.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup.Code);

			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_OH = orgHeader.PK;
			address2.OA_Address1 = "Address2";
			address2.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup.Code);
			address2.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup.Code);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_CPSCInd = "D";
			var cpsc = invoiceLine.CPSCHeaders.AddNew();
			cpsc.US_ProductCode = "A01";
			cpsc.US_ProductCodeVersionNumber = "0001";
			cpsc.US_ManufacturerMonthAndYear = "122021";
			cpsc.US_RuleCodes = "122,133";
			cpsc.US_ManufacturerRegistryID = "1111";
			cpsc.US_OA_ManufacturerAddress = address.PK;
			cpsc.US_OA_CertifyingEntityAddress = address1.PK;
			cpsc.US_OA_ContactPointAddress = address2.PK;
			var lot = cpsc.Lots.AddNew();
			lot.US_StartDate = new ZDateTime(2022, 01, 01);
			lot.US_EndDate = new ZDateTime(2012, 12, 12);
			var rule = cpsc.RuleAndLabs.AddNew();
			rule.US_RuleCodes = "123";
			rule.US_PreviousInspectionDate = new ZDateTime(2012, 01, 01);
			var report = rule.ReportAndLabs.AddNew();
			report.US_RemarksText = "text";
			report.US_RemarksType = "CP1";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				userControl.LineDetailTabControl.SelectedTab = userControl.CPSCTabPage;

				var grid = userControl.CPSCTabPage.FindSingle<ZGrid>("HeaderGrid");
				var columns = grid.Columns;
				var productCode = grid.GetColumnStyle("US_ProductCode");
				AssertEquals("Product Code", productCode.CaptionResourceString.Caption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), productCode.GetType());
				AssertEquals("A01", grid[0, columns.IndexOf(x => x.ColumnName == "US_ProductCode")]);

				var productCodeVersionNumber = grid.GetColumnStyle("US_ProductCodeVersionNumber");
				AssertEquals("Product Code Version Number", productCodeVersionNumber.CaptionResourceString.Caption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), productCodeVersionNumber.GetType());
				AssertEquals("0001", grid[0, columns.IndexOf(x => x.ColumnName == "US_ProductCodeVersionNumber")]);

				var manufacturerAddress = grid.GetColumnStyle("US_OA_ManufacturerAddress");
				AssertEquals("Manufacturer Address", manufacturerAddress.CaptionResourceString.Caption);
				AssertEquals("Address", grid[0, columns.IndexOf(x => x.ColumnName == "US_OA_ManufacturerAddress")]);

				var certifyingEntityOrgPK = grid.GetColumnStyle("CertifyingEntityOrgPK");
				AssertEquals("Certifying Entity", certifyingEntityOrgPK.CaptionResourceString.Caption);
				AssertEquals("Certifying Entity", certifyingEntityOrgPK.GroupName.Caption);
				AssertEquals(typeof(ZGuidFindBoxColumnStyleInfo), certifyingEntityOrgPK.GetType());

				var certifyingEntityAddress = grid.GetColumnStyle("US_OA_CertifyingEntityAddress");
				AssertEquals("Certifying Entity Address", certifyingEntityAddress.CaptionResourceString.Caption);
				AssertEquals("Certifying Entity", certifyingEntityAddress.GroupName.Caption);
				AssertEquals(typeof(ZAddressDropEditColumnStyleInfo), certifyingEntityAddress.GetType());
				AssertEquals("Address1", grid[0, columns.IndexOf(x => x.ColumnName == "US_OA_CertifyingEntityAddress")]);

				var contactPointOrgPK = grid.GetColumnStyle("ContactPointOrgPK");
				AssertEquals("Contact of Point", contactPointOrgPK.CaptionResourceString.Caption);
				AssertEquals("Contact of Point", contactPointOrgPK.GroupName.Caption);
				AssertEquals(typeof(ZGuidFindBoxColumnStyleInfo), contactPointOrgPK.GetType());

				var contactPointAddress = grid.GetColumnStyle("US_OA_ContactPointAddress");
				AssertEquals("Contact of Point Address", contactPointAddress.CaptionResourceString.Caption);
				AssertEquals("Contact of Point", contactPointAddress.GroupName.Caption);
				AssertEquals(typeof(ZAddressDropEditColumnStyleInfo), contactPointAddress.GetType());
				AssertEquals("Address2", grid[0, columns.IndexOf(x => x.ColumnName == "US_OA_ContactPointAddress")]);

				var manufacturerMonthAndYear = grid.GetColumnStyle("US_ManufacturerMonthAndYear");
				AssertEquals("Manufactured Month and Year (MMCCYY)", manufacturerMonthAndYear.CaptionResourceString.Caption);
				AssertEquals("Manufactured", manufacturerMonthAndYear.CaptionResourceString.ShortCaption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), manufacturerMonthAndYear.GetType());
				AssertEquals("122021", grid[0, columns.IndexOf(x => x.ColumnName == "US_ManufacturerMonthAndYear")]);

				var ruleCodesHeader = grid.GetColumnStyle("US_RuleCodes");
				AssertEquals("Citation / Exemption No.", ruleCodesHeader.CaptionResourceString.Caption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), ruleCodesHeader.GetType());
				AssertEquals("122,133", grid[0, columns.IndexOf(x => x.ColumnName == "US_RuleCodes")]);

				var manufacturerRegistryID = grid.GetColumnStyle("US_ManufacturerRegistryID");
				AssertEquals("Small Batch Manufacturer Registry ID", manufacturerRegistryID.CaptionResourceString.Caption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), manufacturerRegistryID.GetType());
				AssertEquals("1111", grid[0, columns.IndexOf(x => x.ColumnName == "US_ManufacturerRegistryID")]);

				grid = userControl.CPSCTabPage.FindSingle<ZGrid>("LotsGrid");
				columns = grid.Columns;
				var startDate = grid.GetColumnStyle("US_StartDate");
				AssertEquals("Lot Production Start Date", startDate.CaptionResourceString.Caption);
				AssertEquals(typeof(ZDateEditColumnStyleInfo), startDate.GetType());
				AssertEquals(new ZDateTime(2022, 01, 01), grid[0, columns.IndexOf(x => x.ColumnName == "US_StartDate")]);

				var endDate = grid.GetColumnStyle("US_EndDate");
				AssertEquals("Lot Production End Date", endDate.CaptionResourceString.Caption);
				AssertEquals(typeof(ZDateEditColumnStyleInfo), endDate.GetType());
				AssertEquals(new ZDateTime(2012, 12, 12), grid[0, columns.IndexOf(x => x.ColumnName == "US_EndDate")]);

				grid = userControl.CPSCTabPage.FindSingle<ZGrid>("RulesGrid");
				columns = grid.Columns;
				var ruleCodes = grid.GetColumnStyle("US_RuleCodes");
				AssertEquals("Citation / Exemption No.", ruleCodes.CaptionResourceString.Caption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), ruleCodes.GetType());
				AssertEquals("123", grid[0, columns.IndexOf(x => x.ColumnName == "US_RuleCodes")]);

				var previousInspectionDate = grid.GetColumnStyle("US_PreviousInspectionDate");
				AssertEquals("Date of Previous Inspection/Laboratory Testing", previousInspectionDate.CaptionResourceString.Caption);
				AssertEquals(typeof(ZDateEditColumnStyleInfo), previousInspectionDate.GetType());
				AssertEquals(new ZDateTime(2012, 01, 01), grid[0, columns.IndexOf(x => x.ColumnName == "US_PreviousInspectionDate")]);

				grid = userControl.CPSCTabPage.FindSingle<ZGrid>("ReportGrid");
				columns = grid.Columns;
				var remarksText = grid.GetColumnStyle("US_RemarksText");
				AssertEquals("Lab Report ID, URL or Access Key", remarksText.CaptionResourceString.Caption);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), remarksText.GetType());
				AssertEquals("text", grid[0, columns.IndexOf(x => x.ColumnName == "US_RemarksText")]);

				var remarksType = grid.GetColumnStyle("US_RemarksType");
				AssertEquals("Lab Report Information Type", remarksType.CaptionResourceString.Caption);
				AssertEquals(typeof(ZDropEditColumnStyleInfo), remarksType.GetType());
				AssertEquals("CP1", grid[0, columns.IndexOf(x => x.ColumnName == "US_RemarksType")]);
			}
		}

		public void TestLotsGridVisible()
		{
			using (var control = new CPSCUserControl())
			{
				control.Show();
				Assert(!control.LotsAndOtherSplitContainer.Panel1Collapsed);
				control.LotsGridVisible = false;
				Assert(control.LotsAndOtherSplitContainer.Panel1Collapsed);
				control.LotsGridVisible = true;
				Assert(!control.LotsAndOtherSplitContainer.Panel1Collapsed);
			}
		}
	}

	sealed class CPSCGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<CPSCUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.CPSCHeaders;

		protected override ZGrid GetGrid(CPSCUserControl control) => control.HeaderGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((CPSCHeaderCollection)collection).AddNew();
	}
}
