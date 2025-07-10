using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.Business.JobMessageTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<TTBUserControl>))]
	sealed class TTBUserControlTest : ZPGAFormBasherAbstractTest<TTBUserControl>
	{
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
				AssertEquals("userControl.TTBTabPage.TabVisible", false, userControl.TTBTabPage.TabVisible);
				AssertNull(userControl.ttbUserControl);
				invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.TTBTabPage.TabVisible", true, userControl.TTBTabPage.TabVisible);
				AssertNull(userControl.ttbUserControl);
				userControl.LineDetailTabControl.SelectedTab = userControl.TTBTabPage;
				AssertEquals("userControl.TTBTabPage.TabVisible", true, userControl.TTBTabPage.TabVisible);
				AssertNotNull(userControl.ttbUserControl);
			}
		}

		public void TestCOLAAndCertificatesGridColumnNames()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TTBCOLAAndCertificate), nameof(TTBCOLAAndCertificate.US_ForeignCertificateCountry), false, attribute => attribute.Caption == "Foreign Certificate Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TTBCOLAAndCertificate), nameof(TTBCOLAAndCertificate.US_ForeignCertificateCountry), false, attribute => attribute.MediumCaption == "Cert. Country/Region");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TTBCOLAAndCertificate), nameof(TTBCOLAAndCertificate.US_ForeignCertificateCountry), false, attribute => attribute.ShortCaption == "Ctry/Rgn.");
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.SetProgramCodeReadOnlyForTest(true);
			var cola = ttbLine.COLAAndCertificates.AddNew();
			cola.US_COLA = "ZZ";
			ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.SetProgramCodeReadOnlyForTest(true);
			var cigar = ttbLine.Cigars.AddNew();
			cigar.US_IsSmall = true;
			return ttbLine;
		}

		protected override string BindMember => "FilteredInvoiceLines.TTBLines";
	}

	sealed class TTBGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<TTBUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.TTBLines;

		protected override ZGrid GetGrid(TTBUserControl control) => control.TTBLineGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((TTBLineCollection)collection).AddNew();
	}
}
