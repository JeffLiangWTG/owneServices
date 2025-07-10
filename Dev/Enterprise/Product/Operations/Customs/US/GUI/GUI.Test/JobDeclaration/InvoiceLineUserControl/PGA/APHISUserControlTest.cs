using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<APHISUserControl>))]
	sealed class APHISUserControlTest : ZPGAFormBasherAbstractTest<APHISUserControl>
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new APHISUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(delegate
				{
					control.ViewEditButton.PerformClick();
				});
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(APHISUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var aphis = invoiceLine.APHISHeaders.AddNew();
				control.SetDataBinding(aphis, "");
				control.APHISHeaderGrid.DataSource = invoiceLine.APHISHeaders;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(APHISEditForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				AssertEquals("userControl.APHISTabPage.TabVisible", false, userControl.APHISTabPage.TabVisible);
				AssertNull(userControl.aphisUserControl);
				invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.APHISTabPage.TabVisible", true, userControl.APHISTabPage.TabVisible);
				AssertNull(userControl.aphisUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.APHISTabPage;
				AssertEquals("userControl.APHISTabPage.TabVisible", true, userControl.APHISTabPage.TabVisible);
				AssertNotNull(userControl.aphisUserControl);
			}
		}

		public void TestPGAMenuItems()
		{
			using (var control = new APHISUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.SetTrackingID();
				AssertNotNull("Context menu contains Update PGA Line", control.APHISHeaderGrid.ContextMenu.MenuItems.FindByText("Update PGA Line"));
				AssertNotNull("Context menu contains Delete PGA Line", control.APHISHeaderGrid.ContextMenu.MenuItems.FindByText("Delete PGA Line"));
				AssertNotNull("Context menu contains Add More PGA Lines", control.APHISHeaderGrid.ContextMenu.MenuItems.FindByText("Add More PGA Lines"));
			}
		}

		public void TestSourcesGridColumnNames()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(APHISSource), nameof(APHISSource.US_CountryCode), false, attribute => attribute.Caption == "Source Ctry/Rgn.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(APHISSource), nameof(APHISSource.US_CountryCode), false, attribute => attribute.ShortCaption == "Ctry/Rgn.");
		}

		public void TestRoutingsGridColumnNames()
		{
			using (var control = new APHISUserControl())
			{
				AssertEquals("US_Country caption", "Country/Region", control.RoutingsGrid.GetColumnStyle("US_Country").CaptionResourceString.Caption);
			}
		}

		protected override BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var aphisAVSHeader = invoiceLine.APHISHeaders.AddNew();
			aphisAVSHeader.US_ProgramType = APHISProgramCodeList.Codes.AVS;
			aphisAVSHeader.SetProgramTypeReadOnlyForTest(true);
			return aphisAVSHeader;
		}

		protected override string BindMember => "FilteredInvoiceLines.APHISHeaders";
	}

	sealed class APHISGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<APHISUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.APHISHeaders;

		protected override ZGrid GetGrid(APHISUserControl control) => control.APHISHeaderGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((APHISHeaderCollection)collection).AddNew();
	}
}
