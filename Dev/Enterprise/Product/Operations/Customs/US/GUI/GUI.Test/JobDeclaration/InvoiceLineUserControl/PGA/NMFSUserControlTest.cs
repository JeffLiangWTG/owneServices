using System.Reflection;
using System.Windows.Forms;
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
	[TestedType(typeof(ACEPGATestForm<NMFSUserControl>))]
	sealed class NMFSUserControlTest : ZPGAFormBasherAbstractTest<NMFSUserControl>
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new NMFSUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains(NMFSUserControl.NotificationMessage));
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var nmfs = invoiceLine.NMFSLines.AddNew();
				control.SetDataBinding(nmfs, "");
				control.NMFSGrid.DataSource = invoiceLine.NMFSLines;
				control.ViewEditButton.PerformClick();
				AssertEquals(typeof(NMFSEditForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestArgumentOutOfRangeExceptionOnMouseClick()
		{
			// this test reproduces scenario from WI00183380 that caused ArgumentOutOfRangeException on mouse-click
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
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				userControl.LineDetailTabControl.SelectedTab = userControl.NMFSTabPage;
				var nmfsLine = invoiceLine.NMFSLines.AddNew();
				nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
				nmfsLine.HarvestingDetails.AddNew();
				nmfsLine.HarvestingDetails[0].US_OceanAreaOfCatch = "CAR";
				var nmfsUserControl = userControl.nmfsUserControl;
				var detailsGrid = nmfsUserControl.HarvestingDetailsGrid;
				var mouseDown = detailsGrid.GetType().GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance);
				Application.DoEvents();
				var row1 = detailsGrid.GetRowNotificationRectangle(0);
				var row2 = detailsGrid.GetRowNotificationRectangle(1);
				mouseDown.Invoke(detailsGrid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row2.X + 100 /* clicking any cell, except for row header */, row2.Y, 0) });
				Application.DoEvents();
				mouseDown.Invoke(detailsGrid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1.X + 100 /* clicking any cell, except for row header */, row1.Y, 0) });
			}
		}

		public void TestNoExceptionOnDeletedClickForNMFSGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.NMFSLines.AddNew();
			invoiceLine.NMFSLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var userControl = (USACEImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				userControl.LineDetailTabControl.SelectedTab = userControl.NMFSTabPage;
				var nmfsUserControl = userControl.nmfsUserControl;
				nmfsUserControl.NMFSGrid.ListManager.Position = 1;
				AssertNoExceptionThrown(() =>
				{
					nmfsUserControl.NMFSGrid.DeleteMenuItem.PerformClick();
					Application.DoEvents();
				});
				AssertEquals(1, invoiceLine.NMFSLines.Count);
			}
		}

		public void TestHarvestingDetailsGridColumnNames()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NMFSHarvestingDetail), nameof(NMFSHarvestingDetail.US_HarvestedCountry), false, attribute => attribute.Caption == "Harvested Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NMFSHarvestingDetail), nameof(NMFSHarvestingDetail.US_VesselCountry), false, attribute => attribute.Caption == "Vessel Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NMFSHarvestingDetail), nameof(NMFSHarvestingDetail.US_FirstLandingCountry), false, attribute => attribute.Caption == "First Landing Country/Region");
		}

		public void TestHarvestingVesselssGridColumnaNames()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NMFSVessels), nameof(NMFSVessels.US_HarvestedCountry), false, attribute => attribute.Caption == "Vessel Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NMFSVessels), nameof(NMFSVessels.US_HarvestedCountry), false, attribute => attribute.ShortCaption == "Vessel Ctry/Rgn.");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(NMFSVessels), nameof(NMFSVessels.US_FirstLandingCountry), false, attribute => attribute.Caption == "First Landing Country/Region");
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			var nmfs370 = invoiceLine.NMFSLines.AddNew();
			nmfs370.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfs370.SetProgramTypeReadOnlyForTest(true);
			var harvest = nmfs370.HarvestingDetails.AddNew();
			harvest.US_HarvestedCountry = "ZZ";
			var nmfsAMR = invoiceLine.NMFSLines.AddNew();
			nmfsAMR.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsAMR.SetProgramTypeReadOnlyForTest(true);
			harvest = nmfsAMR.HarvestingDetails.AddNew();
			harvest.US_HarvestedCountry = "ZZ";
			var nmfsHMS = invoiceLine.NMFSLines.AddNew();
			nmfsHMS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsHMS.SetProgramTypeReadOnlyForTest(true);
			harvest = nmfsHMS.HarvestingDetails.AddNew();
			harvest.US_HarvestedCountry = "ZZ";
			var nmfsSIM = invoiceLine.NMFSLines.AddNew();
			nmfsSIM.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsSIM.SetProgramTypeReadOnlyForTest(true);
			harvest = nmfsSIM.HarvestingDetails.AddNew();
			harvest.US_HarvestedCountry = "ZZ";
			return nmfsHMS;
		}

		protected override string BindMember => "FilteredInvoiceLines.NMFSLines";
	}

	sealed class NMFSGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<NMFSUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.NMFSLines;

		protected override ZGrid GetGrid(NMFSUserControl control) => control.NMFSGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((NMFSLineCollection)collection).AddNew();
	}
}
