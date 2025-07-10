using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDNMFS370DataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFS370DisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST NMFS 370 DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_NMFS370Ind);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_NMFS370DisclaimReason);
			AssertEquals("TEST NMFS 370 DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "NMFS 370 TEST DESC";
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsLine.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.B4;
			nmfsLine.US_CaptainStatement = true;
			nmfsLine.US_IDCPMemberCertification = true;
			var harvestingDetail1 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_VesselCountry = Core.Constants.CountryCodes.Canada;
			harvestingDetail1.US_ContainsYellowfinTuna = true;

			var harvestingDetail2 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail2.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail2.US_VesselCountry = Core.Constants.CountryCodes.UnitedStates;
			harvestingDetail2.US_ContainsYellowfinTuna = false;

			var harvestingDetail3 = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_VesselCountry = Core.Constants.CountryCodes.Australia;
			harvestingDetail3.US_ContainsYellowfinTuna = false;

			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			nmfsLine2.HarvestingDetails.RemoveAndDeleteAll();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes._370;
			nmfsLine2.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.B2;
			nmfsLine2.US_CaptainStatement = true;
			nmfsLine2.US_IDCPMemberCertification = true;
			nmfsLine2.US_ObserverStatement = true;
			var harvestingDetail4 = nmfsLine2.HarvestingDetails.AddNew();
			harvestingDetail4.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail4.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail4.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail4.US_VesselCountry = Core.Constants.CountryCodes.UnitedStates;
			harvestingDetail4.US_ContainsYellowfinTuna = true;

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NMFS370Ind);
			AssertEquals("NMFS 370 TEST DESC", invoiceLineImported.JI_Description);
			AssertEquals(2, invoiceLineImported.NMFSLines.Count);

			CombineAssertions(() =>
			{
				var nmfsLine0Imported = invoiceLineImported.NMFSLines[0];
				AssertEquals(NMFSProgramCodeList.Codes._370, nmfsLine0Imported.US_ProgramType);
				AssertEquals(DolphinSafeStatusList.Codes.B4, nmfsLine0Imported.US_DolphinSafeStatus);
				AssertEquals(true, nmfsLine0Imported.US_CaptainStatement);
				AssertEquals(true, nmfsLine0Imported.US_IDCPMemberCertification);
				AssertEquals(3, nmfsLine0Imported.HarvestingDetails.Count);

				var harvestingDetail0_0Imported = nmfsLine0Imported.HarvestingDetails[0];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail0_0Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail0_0Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail0_0Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.Australia, harvestingDetail0_0Imported.US_VesselCountry);
				AssertEquals(false, harvestingDetail0_0Imported.US_ContainsYellowfinTuna);

				var harvestingDetail0_1Imported = nmfsLine0Imported.HarvestingDetails[1];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail0_1Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail0_1Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail0_1Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, harvestingDetail0_1Imported.US_VesselCountry);
				AssertEquals(false, harvestingDetail0_1Imported.US_ContainsYellowfinTuna);

				var harvestingDetail0_2Imported = nmfsLine0Imported.HarvestingDetails[2];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail0_2Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail0_2Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail0_2Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.Canada, harvestingDetail0_2Imported.US_VesselCountry);
				AssertEquals(true, harvestingDetail0_2Imported.US_ContainsYellowfinTuna);

				var nmfsLine1Imported = invoiceLineImported.NMFSLines[1];
				AssertEquals(NMFSProgramCodeList.Codes._370, nmfsLine1Imported.US_ProgramType);
				AssertEquals(DolphinSafeStatusList.Codes.B2, nmfsLine1Imported.US_DolphinSafeStatus);
				AssertEquals(true, nmfsLine1Imported.US_CaptainStatement);
				AssertEquals(true, nmfsLine1Imported.US_IDCPMemberCertification);
				AssertEquals(true, nmfsLine1Imported.US_ObserverStatement);
				AssertEquals(1, nmfsLine1Imported.HarvestingDetails.Count);

				var harvestingDetail1_0Imported = nmfsLine1Imported.HarvestingDetails[0];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail1_0Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail1_0Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail1_0Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, harvestingDetail1_0Imported.US_VesselCountry);
				AssertEquals(true, harvestingDetail1_0Imported.US_ContainsYellowfinTuna);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
		}
	}
}
