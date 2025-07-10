using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDNHTSADataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NHTDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine.JI_Description = "TEST NHTSA DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_NHTSAIndicator);
			AssertEquals(PGADisclaimReasonList.Codes.A, invoiceLineImported.US_NHTDisclaimReason);
			AssertEquals("TEST NHTSA DISCLAIMED", invoiceLineImported.JI_Description);
		}

		public void TestProcessBox3VehicleDeclaredMessageBlocks()
		{
			var ior = Factory.New<OrgHeader>();
			ior.OH_Code = "TESTIOR";
			ior.MainAddress.OA_Address1 = "TEST ADDRESS 1";
			DeclarationTestHelper.AddPGAContact(ior, "Test", "IMPORTER", "11223344", "ABC@TEST.COM", null);
			declaration.IOROrgPK = ior.PK;

			invoiceLine.JI_Description = "FREEARI VEHICLE";
			var header = invoiceLine.NHTSALines.AddNew();
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._03;
			header.US_NHTElectronicImage = true;
			header.US_NHTDOTSuretyCode = "421";
			header.US_NHTDOTBondNumber = "011-123456";
			header.US_NHTDOTBondType = DOTBondQualifierList.Codes.Single;
			header.US_NHTDOTBondAmount = 1250000;
			header.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var detailsLine = header.NHTSADetails.AddNew();
			detailsLine.US_NHTBrandName = "FERRARI";
			detailsLine.US_NHTModel = "TESTAROSSA";
			detailsLine.US_NHTYearOfMFR = "1989";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._06;
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "ZFFVA40B00000000";
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1989";
			detailsLine.US_NHTDriveSide = DriverSideList.Codes.Left;
			detailsLine.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			detailsLine.US_NHTLPCONumber = "R-90-007";

			var additionalLPCO0 = detailsLine.PermitAndLicenses.AddNew();
			additionalLPCO0.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			additionalLPCO0.US_NHTLPCONumber = "VSA-039";

			var additionalLPCO1 = detailsLine.PermitAndLicenses.AddNew();
			additionalLPCO1.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			additionalLPCO1.US_NHTLPCODate = new ZDateTime(2016, 7, 20);
			additionalLPCO1.US_NHTLPCONumber = "VSA-072";
			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("FREEARI VEHICLE", invoiceLineImported.JI_Description);
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NHTSAIndicator);
			AssertEquals(1, invoiceLineImported.NHTSALines.Count);

			var nhtsaImported = invoiceLineImported.NHTSALines[0];
			AssertEquals(NHTSAProgramCodeList.Codes.MVS, nhtsaImported.US_NHTProgramCode);
			AssertEquals(DepartmentOfTransportBoxNumberList.Codes._03, nhtsaImported.US_NHTBoxNumber);
			AssertEquals(true, nhtsaImported.US_NHTElectronicImage);
			AssertEquals("421", nhtsaImported.US_NHTDOTSuretyCode);
			AssertEquals("011-123456", nhtsaImported.US_NHTDOTBondNumber);
			AssertEquals(DOTBondQualifierList.Codes.Single, nhtsaImported.US_NHTDOTBondType);
			AssertEquals(PartyTypeList.Codes.Importer, nhtsaImported.US_CertifyingIndividual);
			AssertEquals("TEST IMPORTER", nhtsaImported.US_PGAContactName);
			AssertEquals("11223344", nhtsaImported.US_PGAContactPhoneNo);
			AssertEquals("ABC@TEST.COM", nhtsaImported.US_PGAContactEmail);
			AssertEquals(1250000, nhtsaImported.US_NHTDOTBondAmount);
			AssertEquals(1, nhtsaImported.NHTSADetails.Count);

			var nhtsaDetailsImported = nhtsaImported.NHTSADetails[0];
			AssertEquals("FERRARI", nhtsaDetailsImported.US_NHTBrandName);
			AssertEquals("TESTAROSSA", nhtsaDetailsImported.US_NHTModel);
			AssertEquals("1989", nhtsaDetailsImported.US_NHTYearOfMFR);
			AssertEquals(MonthList.Codes._06, nhtsaDetailsImported.US_NHTMonthOfMFR);
			AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, nhtsaDetailsImported.US_NHTIdentityNumQualifier);
			AssertEquals("ZFFVA40B00000000", nhtsaDetailsImported.US_NHTIdentityNumber);
			AssertEquals(NHTSACategoryCode_MVSTYPList.Codes.MVS1, nhtsaDetailsImported.US_NHTCategoryCode);
			AssertEquals("1989", nhtsaDetailsImported.US_NHTModelYear);
			AssertEquals(DriverSideList.Codes.Left, nhtsaDetailsImported.US_NHTDriveSide);
			AssertEquals(3, nhtsaDetailsImported.PermitAndLicenses.Count);

			var nh0LPCO = nhtsaDetailsImported.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().FirstOrDefault(x => x.US_NHTLPCOType == NHTSALPCOTypeList.Codes.NH0);
			AssertEquals("R-90-007", nh0LPCO.US_NHTLPCONumber);
			var nh2LPCO = nhtsaDetailsImported.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().FirstOrDefault(x => x.US_NHTLPCOType == NHTSALPCOTypeList.Codes.NH2);
			AssertEquals("VSA-072", nh2LPCO.US_NHTLPCONumber);
			var nh3LPCO = nhtsaDetailsImported.PermitAndLicenses.OfType<NHTSAPermitAndLicenses>().FirstOrDefault(x => x.US_NHTLPCOType == NHTSALPCOTypeList.Codes.NH3);
			AssertEquals("VSA-039", nh3LPCO.US_NHTLPCONumber);
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			declaration.US_FDAContactName = "PGA CONTACT FOR TEST";
			declaration.US_FDAContactEmail = "test@test.com";
			declaration.US_FDAContactPhoneNo = "091245022";
			invoiceLine.JI_Description = "LAND ROVER VEHICLE";
			var header = invoiceLine.NHTSALines.AddNew();
			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			header.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			var detailsLine = header.NHTSADetails.AddNew();
			detailsLine.US_NHTBrandName = "LAND ROVER";
			detailsLine.US_NHTModel = "DEFENDER 110";
			detailsLine.US_NHTYearOfMFR = "1983";
			detailsLine.US_NHTMonthOfMFR = MonthList.Codes._12;
			detailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			detailsLine.US_NHTModelYear = "1983";
			detailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			detailsLine.US_NHTIdentityNumber = "SALLDHMV2AA100000";
			var document = header.NHTSADocuments.OfType<NHTSADocument>().FirstOrDefault();
			document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.Importer;

			Factory.Save();

			var action = GetAction(declaration);
			action.US_AcknowledgeAndSign = true;
			action.US_DateOfDeclaration = new ZDateTime(2016, 07, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			AssertEquals("PGA CONTACT FOR TEST", DeclarationImported.US_FDAContactName);
			AssertEquals("091245022", DeclarationImported.US_FDAContactPhoneNo);
			AssertEquals("TEST@TEST.COM", DeclarationImported.US_FDAContactEmail);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NHTSAIndicator);
			AssertEquals("LAND ROVER VEHICLE", invoiceLineImported.JI_Description);
			AssertEquals(1, invoiceLineImported.NHTSALines.Count);

			CombineAssertions(() =>
			{
				var nhtsaImported = invoiceLineImported.NHTSALines[0];
				AssertEquals(NHTSAProgramCodeList.Codes.MVS, nhtsaImported.US_NHTProgramCode);
				AssertEquals(DepartmentOfTransportBoxNumberList.Codes._01, nhtsaImported.US_NHTBoxNumber);
				AssertEquals(PartyTypeList.Codes.CustomsBroker, nhtsaImported.US_CertifyingIndividual);
				AssertEquals(1, nhtsaImported.NHTSADetails.Count);
				AssertEquals(1, nhtsaImported.NHTSADocuments.Count);

				var nhtsaDetailsImported = nhtsaImported.NHTSADetails[0];
				AssertEquals("LAND ROVER", nhtsaDetailsImported.US_NHTBrandName);
				AssertEquals("DEFENDER 110", nhtsaDetailsImported.US_NHTModel);
				AssertEquals("1983", nhtsaDetailsImported.US_NHTYearOfMFR);
				AssertEquals(MonthList.Codes._12, nhtsaDetailsImported.US_NHTMonthOfMFR);
				AssertEquals(NHTSACategoryCode_MVSTYPList.Codes.MVS1, nhtsaDetailsImported.US_NHTCategoryCode);
				AssertEquals("1983", nhtsaDetailsImported.US_NHTModelYear);
				AssertEquals(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, nhtsaDetailsImported.US_NHTIdentityNumQualifier);
				AssertEquals("SALLDHMV2AA100000", nhtsaDetailsImported.US_NHTIdentityNumber);

				var nhtsaDocumentImported = nhtsaImported.NHTSADocuments[0];
				AssertEquals(NHTSAOrganizationTypeList.Codes.Importer, nhtsaDocumentImported.US_NHTDocumentOwner);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var consigneeOrgHeader = Factory.New<OrgHeader>();
			consigneeOrgHeader.OH_Code = "TESTCNEORG";
			consigneeOrgHeader.OH_FullName = "ULTIMATE CONSIGNEE";
			var cneAddress = consigneeOrgHeader.MainAddress;
			cneAddress.OA_Address1 = "CNE ADDRESS 1";
			cneAddress.OA_Address2 = "CNE ADDRESS 2";
			cneAddress.OA_City = "CHARLESTON";
			cneAddress.OA_RL_NKRelatedPortCode = "US2CW";
			cneAddress.OA_State = "IL";
			cneAddress.OA_PostCode = "29492";

			invoiceLine.JI_OA_ConsigneeAddress = consigneeOrgHeader.MainAddress.PK;
			invoiceLine.US_NHTSAIndicator = OGAIndicatorList.Codes.Declared;
		}
	}
}
