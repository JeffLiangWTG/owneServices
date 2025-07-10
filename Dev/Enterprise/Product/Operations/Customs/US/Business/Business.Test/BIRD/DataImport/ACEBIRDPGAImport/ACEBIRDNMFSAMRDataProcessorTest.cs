using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDNMFSAMRDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NMFSAMRDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST NMFS AMR DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_NMFSAMRInd);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_NMFSAMRDisclaimReason);
			AssertEquals("TEST NMFS AMR DISCLAIMED", invoiceLineImported.JI_Description);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "NMFS AMR TEST DESC";
			var nmfsLine0 = invoiceLine.NMFSLines.AddNew();
			nmfsLine0.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine0.US_IFTPPermitNumber = "AMR3242";
			nmfsLine0.US_Commodity = FishStateList.Codes.FreshToothfish;
			nmfsLine0.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument;
			nmfsLine0.US_DISDocumentID = "DIS23423";

			var nmfsLine1 = invoiceLine.NMFSLines.AddNew();
			nmfsLine1.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine1.US_IFTPPermitNumber = "AMR3243";
			nmfsLine1.US_Commodity = FishStateList.Codes.FreshToothfish;
			nmfsLine1.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.ReportingFormForCatchDocumentsAccompanyingFresh;
			nmfsLine1.US_DISDocumentID = "DIS093454";

			var nmfsLine2 = invoiceLine.NMFSLines.AddNew();
			nmfsLine2.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			nmfsLine2.US_Commodity = FishStateList.Codes.FrozenToothfish;
			nmfsLine2.US_IFTPPermitNumber = "AMR3244";
			nmfsLine2.US_PreApprovalIssuedNumber = "PAIN2342";
			nmfsLine2.US_PreApprovalIssuedQuantity = 15.322345m;
			nmfsLine2.US_PreApprovalIssuedQuantityUQ = Core.Constants.Weight.Tonnes;
			nmfsLine2.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.ReportingFormForCatchDocumentsAccompanyingFresh;
			nmfsLine2.US_DISDocumentID = "DIS093453";

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NMFSAMRInd);
			AssertEquals("NMFS AMR TEST DESC", invoiceLineImported.JI_Description);
			AssertEquals(3, invoiceLineImported.NMFSLines.Count);

			CombineAssertions(() =>
			{
				var nmfsLine0Imported = invoiceLineImported.NMFSLines[0];
				AssertEquals(NMFSProgramCodeList.Codes.AMR, nmfsLine0Imported.US_ProgramType);
				AssertEquals("AMR3242", nmfsLine0Imported.US_IFTPPermitNumber);
				AssertEquals("FRE", nmfsLine0Imported.US_Commodity);
				AssertEquals(1, nmfsLine0Imported.DocumentDetails.Count);
				AssertEquals(NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument, nmfsLine0Imported.DocumentDetails[0].CY_Code);
				AssertEquals("DIS23423", nmfsLine0Imported.DocumentDetails[0].CY_Data);

				var nmfsLine1Imported = invoiceLineImported.NMFSLines[1];
				AssertEquals(NMFSProgramCodeList.Codes.AMR, nmfsLine1Imported.US_ProgramType);
				AssertEquals("AMR3243", nmfsLine1Imported.US_IFTPPermitNumber);
				AssertEquals("FRE", nmfsLine1Imported.US_Commodity);
				AssertEquals(1, nmfsLine1Imported.DocumentDetails.Count);
				AssertEquals(NMFSAMRDocumentIdentifierList.Codes.ReportingFormForCatchDocumentsAccompanyingFresh, nmfsLine1Imported.DocumentDetails[0].CY_Code);
				AssertEquals("DIS093454", nmfsLine1Imported.DocumentDetails[0].CY_Data);

				var nmfsLine2Imported = invoiceLineImported.NMFSLines[2];
				AssertEquals(NMFSProgramCodeList.Codes.AMR, nmfsLine2Imported.US_ProgramType);
				AssertEquals("FRZ", nmfsLine2Imported.US_Commodity);
				AssertEquals("AMR3244", nmfsLine2Imported.US_IFTPPermitNumber);
				AssertEquals("PAIN2342", nmfsLine2Imported.US_PreApprovalIssuedNumber);
				AssertEquals(15322.3m, nmfsLine2Imported.US_PreApprovalIssuedQuantity);
				AssertEquals(Core.Constants.Weight.Kilograms, nmfsLine2Imported.US_PreApprovalIssuedQuantityUQ);
				AssertEquals("Processing code is Frozen", ZString.Empty, nmfsLine2Imported.US_DocumentType);
				AssertEquals("Processing code is Frozen", ZString.Empty, nmfsLine2Imported.US_DISDocumentID);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
		}
	}
}
