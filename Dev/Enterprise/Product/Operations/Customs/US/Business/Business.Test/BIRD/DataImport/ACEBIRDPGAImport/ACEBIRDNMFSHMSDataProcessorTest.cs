using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDNMFSHMSDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSHMSDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST NMFS HMS DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_NMFSHMSInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_NMFSHMSDisclaimReason);
			AssertEquals("TEST NMFS HMS DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "NMFS HMS TEST DESC";
			var nmfsLine0 = invoiceLine.NMFSLines.AddNew();
			nmfsLine0.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine0.US_IFTPPermitNumber = "HMS324332";
			nmfsLine0.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm;
			nmfsLine0.US_DISDocumentID = "DIS32342";
			var harvestingDetail1 = nmfsLine0.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_VesselCountry = Core.Constants.CountryCodes.Japan;
			var harvestingDetail2 = nmfsLine0.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail2.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail2.US_VesselCountry = Core.Constants.CountryCodes.UnitedStates;

			var nmfsLine1 = invoiceLine.NMFSLines.AddNew();
			nmfsLine1.HarvestingDetails.RemoveAndDeleteAll();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine1.US_EBCDNumber = "HMS324333";
			nmfsLine1.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm;
			nmfsLine1.US_DISDocumentID = "DIS32349";
			var harvestingDetail3 = nmfsLine1.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.SAT;
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_VesselCountry = Core.Constants.CountryCodes.Brazil;
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("NMFS HMS TEST DESC", invoiceLineImported.JI_Description);
			AssertEquals(2, invoiceLineImported.NMFSLines.Count);

			CombineAssertions(() =>
			{
				var nmfsLine0Imported = invoiceLineImported.NMFSLines[0];
				AssertEquals(NMFSProgramCodeList.Codes.HMS, nmfsLine0Imported.US_ProgramType);
				AssertEquals("HMS324332", nmfsLine0Imported.US_IFTPPermitNumber);
				AssertEquals(1, nmfsLine0Imported.DocumentDetails.Count);
				AssertEquals(NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm, nmfsLine0Imported.DocumentDetails[0].CY_Code);
				AssertEquals("DIS32342", nmfsLine0Imported.DocumentDetails[0].CY_Data);
				AssertEquals(2, nmfsLine0Imported.HarvestingDetails.Count);

				var harvestingDetail0_0Imported = nmfsLine0Imported.HarvestingDetails[0];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail0_0Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail0_0Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail0_0Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.Japan, harvestingDetail0_0Imported.US_VesselCountry);
				var harvestingDetail0_1Imported = nmfsLine0Imported.HarvestingDetails[1];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail0_1Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.ETP, harvestingDetail0_1Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail0_1Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.UnitedStates, harvestingDetail0_1Imported.US_VesselCountry);

				var nmfsLine1Imported = invoiceLineImported.NMFSLines[1];
				AssertEquals(NMFSProgramCodeList.Codes.HMS, nmfsLine1Imported.US_ProgramType);
				AssertEquals("HMS324333", nmfsLine1Imported.US_EBCDNumber);
				AssertEquals(1, nmfsLine1Imported.DocumentDetails.Count);
				AssertEquals(NMFSHMSDocumentIdentifierList.Codes.CcsbtCatchMonitoringForm, nmfsLine1Imported.DocumentDetails[0].CY_Code);
				AssertEquals("DIS32349", nmfsLine1Imported.DocumentDetails[0].CY_Data);
				AssertEquals(1, nmfsLine1Imported.HarvestingDetails.Count);

				var harvestingDetail1_0Imported = nmfsLine1Imported.HarvestingDetails[0];
				AssertEquals(NMFSConstants.InternationalWaters, harvestingDetail1_0Imported.US_HarvestedCountry);
				AssertEquals(OceanGeographicAreaCodeList.Codes.SAT, harvestingDetail1_0Imported.US_OceanAreaOfCatch);
				AssertEquals(GearTypeList.Codes.Longline, harvestingDetail1_0Imported.US_GearType);
				AssertEquals(Core.Constants.CountryCodes.Brazil, harvestingDetail1_0Imported.US_VesselCountry);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
