using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<NMFSUserControl>))]
	sealed class NMFSControlVisibilityHelperTest : ZPGAFormBasherAbstractTest<NMFSUserControl>
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
				AssertEquals("userControl.NMFSTabPage.TabVisible", false, userControl.NMFSTabPage.TabVisible);
				AssertNull(userControl.nmfsUserControl);
				invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				AssertNull(userControl.nmfsUserControl);
				invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
				invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
				userControl.LineDetailTabControl.SelectedTab = userControl.NMFSTabPage;
				AssertEquals("userControl.NMFSTabPage.TabVisible", true, userControl.NMFSTabPage.TabVisible);
				AssertNotNull(userControl.nmfsUserControl);
				var nmfsLine = invoiceLine.NMFSLines.AddNew();
				var nmfsUserControl = userControl.nmfsUserControl;
				AssertEquals("nmfsUserControl.HarvestingDetailsGrid.Visible", true, nmfsUserControl.HarvestingDetailsGrid.Visible);
				AssertEquals("nmfsUserControl.NMFSGrid.Visible", true, nmfsUserControl.NMFSGrid.Visible);
				AssertEquals("nmfsUserControl.DocumentDetailsGroupBox.Visible", true, nmfsUserControl.DocumentDetailsGroupBox.Visible);
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 7, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_VesselCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna]);
				nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
				AssertEquals("nmfsUserControl.HarvestingDetailsGrid.Visible", true, nmfsUserControl.HarvestingDetailsGrid.Visible);
				AssertEquals("nmfsUserControl.NMFSGrid.Visible", true, nmfsUserControl.NMFSGrid.Visible);
				AssertEquals("nmfsUserControl.DocumentDetailsGroupBox.Visible", true, nmfsUserControl.DocumentDetailsGroupBox.Visible);
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 6, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_VesselCountry]);
				nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
				AssertEquals("nmfsUserControl.HarvestingDetailsGrid.Visible", true, nmfsUserControl.HarvestingDetailsGrid.Visible);
				AssertEquals("nmfsUserControl.NMFSGrid.Visible", true, nmfsUserControl.NMFSGrid.Visible);
				AssertEquals("nmfsUserControl.DocumentDetailsGroupBox.Visible", true, nmfsUserControl.DocumentDetailsGroupBox.Visible);
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 6, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_VesselCountry]);
				nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
				AssertEquals("nmfsUserControl.HarvestingDetailsGrid.Visible", true, nmfsUserControl.HarvestingDetailsGrid.Visible);
				AssertEquals("nmfsUserControl.NMFSGrid.Visible", true, nmfsUserControl.NMFSGrid.Visible);
				AssertEquals("nmfsUserControl.DocumentDetailsGroupBox.Visible", false, nmfsUserControl.DocumentDetailsGroupBox.Visible);
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("default source type", ZString.Empty, nmfsLine.US_SourceType);
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 10, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearStartDate]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearDescription]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_ContactPartyType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OA_ContactParty]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.ContactPartyOrgPK]);
				nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", true, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 10, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearStartDate]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearDescription]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_ContactPartyType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OA_ContactParty]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.ContactPartyOrgPK]);
				nmfsLine.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 9, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GeographicLocation]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearStartDate]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearDescription]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_ContactPartyType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OA_ContactParty]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.ContactPartyOrgPK]);
				nmfsLine.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 12, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_NoSmallVessels]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_FirstLandingCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearStartDate]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearDescription]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_ContactPartyType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OA_ContactParty]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.ContactPartyOrgPK]);
				// We already tested NMFSProgramCodeList.Codes.AMR above.
				// Now we are testing it second time to make sure that our choice of US_SourceType does not affect result.
				nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
				AssertEquals("nmfsUserControl.HarvestingDetailsGrid.Visible", true, nmfsUserControl.HarvestingDetailsGrid.Visible);
				AssertEquals("nmfsUserControl.NMFSGrid.Visible", true, nmfsUserControl.NMFSGrid.Visible);
				AssertEquals("nmfsUserControl.DocumentDetailsGroupBox.Visible", true, nmfsUserControl.DocumentDetailsGroupBox.Visible);
				AssertEquals("nmfsUserControl.HarvestingVesselsGroupBox.Visible", false, nmfsUserControl.HarvestingVesselsGroupBox.Visible);
				AssertEquals("No. of columns", 6, nmfsUserControl.HarvestingDetailsGrid.Columns.Count);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_HarvestedCountry]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatch]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearType]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_GearTypeDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_OceanAreaOfCatchDesc]);
				AssertNotNull("Column is available", nmfsUserControl.HarvestingDetailsGrid.Columns[NMFSHarvestingDetail.Schema.US_VesselCountry]);
			}
		}

		protected override string BindMember => "FilteredInvoiceLines.NMFSLines";

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
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
	}
}
