using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDNMFSSIMPDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			Assert(true);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "NMFS SIMP TEST DESC";
			var nmfsLine1 = invoiceLine.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine1.US_SourceType = SourceTypeCodesList.Codes.HarvestOfCaptureFisheries;
			nmfsLine1.US_SpeciesCode = "AK";
			nmfsLine1.US_Confidential = ZBool.True;
			nmfsLine1.US_AuthorizationType = "1";
			nmfsLine1.US_OtherAuthorizationNumber = "1111";
			nmfsLine1.US_IFTPPermitNumber = "2222";

			var harvestingDetail1 = nmfsLine1.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = Core.Constants.CountryCodes.Chile;
			harvestingDetail1.US_GearStartDate = ZDateTime.Today;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_GearDescription = NMFSProductCategoryCodeList.Codes.Dressed;

			var harvestingVessel = harvestingDetail1.HarvestingVessles.AddNew();
			harvestingVessel.US_HarvestedVessel = "AUSTRA";
			harvestingVessel.US_HarvestedCountry = Core.Constants.CountryCodes.UnitedStates;
			harvestingVessel.US_NetWeight = 200.00m;
			harvestingVessel.US_NetWeightUQ = "KG";
			harvestingVessel.US_TranshipmentPlace = Core.Constants.CountryCodes.Japan;
			harvestingVessel.US_FirstLandingCountry = Core.Constants.CountryCodes.France;

			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			nmfsLine2.HarvestingDetails.RemoveAndDeleteAll();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine2.US_SourceType = SourceTypeCodesList.Codes.HatcheryBasedAquaculture;
			nmfsLine2.US_SpeciesCode = "BK";
			nmfsLine2.US_Confidential = ZBool.True;
			nmfsLine2.US_AuthorizationType = "1";
			nmfsLine2.US_OtherAuthorizationNumber = "3333";
			nmfsLine2.US_IFTPPermitNumber = "4444";

			var harvestingDetail2 = nmfsLine2.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = Core.Constants.CountryCodes.Malta;
			harvestingDetail2.US_GearStartDate = new ZDateTime(2017, 12, 1);
			harvestingDetail2.US_GearType = GearTypeList.Codes.Baitboat;
			harvestingDetail2.US_GearDescription = NMFSProductCategoryCodeList.Codes.Fillet;

			var nmfsLine3 = invoiceLine.NMFSLines.AddNew();
			nmfsLine3.HarvestingDetails.RemoveAndDeleteAll();
			nmfsLine3.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			nmfsLine3.US_SourceType = SourceTypeCodesList.Codes.SmallVesselHarvest;
			nmfsLine3.US_SpeciesCode = "CK";
			nmfsLine3.US_Confidential = ZBool.True;
			nmfsLine3.US_AuthorizationType = "1";
			nmfsLine3.US_OtherAuthorizationNumber = "5555";
			nmfsLine3.US_IFTPPermitNumber = "6666";

			var harvestingDetail3 = nmfsLine3.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = Core.Constants.CountryCodes.Jamaica;
			harvestingDetail3.US_GearStartDate = new ZDateTime(2017, 12, 7);
			harvestingDetail3.US_GearType = GearTypeList.Codes.Gillnet;
			harvestingDetail3.US_GearDescription = NMFSProductCategoryCodeList.Codes.Steak;
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NMFSSIMPInd);
			AssertEquals("NMFS SIMP TEST DESC", invoiceLineImported.JI_Description);
			AssertEquals(3, invoiceLineImported.NMFSLines.Count);

			CombineAssertions(() =>
			{
				var nmfsLineImported1 = invoiceLineImported.NMFSLines[0];
				AssertEquals(NMFSProgramCodeList.Codes.SIM, nmfsLineImported1.US_ProgramType);
				AssertEquals("AK", nmfsLineImported1.US_SpeciesCode);
				AssertEquals(true, nmfsLineImported1.US_Confidential);
				AssertEquals("1", nmfsLineImported1.US_AuthorizationType);
				AssertEquals("1111", nmfsLineImported1.US_OtherAuthorizationNumber);
				AssertEquals("2222", nmfsLineImported1.US_IFTPPermitNumber);
				AssertEquals(1, nmfsLineImported1.HarvestingDetails.Count);

				var harvestingDetailImported1 = nmfsLineImported1.HarvestingDetails[0];
				AssertEquals(SourceTypeCodesList.Codes.HarvestOfCaptureFisheries, nmfsLineImported1.US_SourceType);
				AssertEquals(Core.Constants.CountryCodes.Chile, harvestingDetailImported1.US_HarvestedCountry);
				AssertEquals(ZDateTime.Today, harvestingDetailImported1.US_GearStartDate);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetailImported1.US_GearType);
				AssertEquals(NMFSProductCategoryCodeList.Codes.Dressed, harvestingDetailImported1.US_GearDescription);
				AssertEquals(1, harvestingDetailImported1.HarvestingVessles.Count);

				var harvestingVesselImported = harvestingDetailImported1.HarvestingVessles[0];
				AssertEquals("AUSTRA", harvestingVesselImported.US_HarvestedVessel);
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, harvestingVesselImported.US_HarvestedCountry);
				AssertEquals(200.00m, harvestingVesselImported.US_NetWeight);
				AssertEquals("KG", harvestingVesselImported.US_NetWeightUQ);
				AssertEquals(Core.Constants.CountryCodes.Japan, harvestingVesselImported.US_TranshipmentPlace);
				AssertEquals(Core.Constants.CountryCodes.France, harvestingVesselImported.US_FirstLandingCountry);

				var nmfsLineImported2 = invoiceLineImported.NMFSLines[1];
				AssertEquals(NMFSProgramCodeList.Codes.SIM, nmfsLineImported2.US_ProgramType);
				AssertEquals("BK", nmfsLineImported2.US_SpeciesCode);
				AssertEquals(true, nmfsLineImported2.US_Confidential);
				AssertEquals("1", nmfsLineImported2.US_AuthorizationType);
				AssertEquals("3333", nmfsLineImported2.US_OtherAuthorizationNumber);
				AssertEquals("4444", nmfsLineImported2.US_IFTPPermitNumber);
				AssertEquals(1, nmfsLineImported2.HarvestingDetails.Count);

				var harvestingDetailImported2 = nmfsLineImported2.HarvestingDetails[0];
				AssertEquals(SourceTypeCodesList.Codes.HatcheryBasedAquaculture, nmfsLineImported2.US_SourceType);
				AssertEquals(Core.Constants.CountryCodes.Malta, harvestingDetailImported2.US_HarvestedCountry);
				AssertEquals(new ZDateTime(2017, 12, 1), harvestingDetailImported2.US_GearStartDate);
				AssertEquals(GearTypeList.Codes.Baitboat, harvestingDetailImported2.US_GearType);
				AssertEquals(NMFSProductCategoryCodeList.Codes.Fillet, harvestingDetailImported2.US_GearDescription);

				var nmfsLineImported3 = invoiceLineImported.NMFSLines[2];
				AssertEquals(NMFSProgramCodeList.Codes.SIM, nmfsLineImported3.US_ProgramType);
				AssertEquals("CK", nmfsLineImported3.US_SpeciesCode);
				AssertEquals(true, nmfsLineImported3.US_Confidential);
				AssertEquals("1", nmfsLineImported3.US_AuthorizationType);
				AssertEquals("5555", nmfsLineImported3.US_OtherAuthorizationNumber);
				AssertEquals("6666", nmfsLineImported3.US_IFTPPermitNumber);
				AssertEquals(1, nmfsLineImported3.HarvestingDetails.Count);

				var harvestingDetailImported3 = nmfsLineImported3.HarvestingDetails[0];
				AssertEquals(SourceTypeCodesList.Codes.SmallVesselHarvest, nmfsLineImported3.US_SourceType);
				AssertEquals(Core.Constants.CountryCodes.Jamaica, harvestingDetailImported3.US_HarvestedCountry);
				AssertEquals(new ZDateTime(2017, 12, 7), harvestingDetailImported3.US_GearStartDate);
				AssertEquals(GearTypeList.Codes.Gillnet, harvestingDetailImported3.US_GearType);
				AssertEquals(NMFSProductCategoryCodeList.Codes.Steak, harvestingDetailImported3.US_GearDescription);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
