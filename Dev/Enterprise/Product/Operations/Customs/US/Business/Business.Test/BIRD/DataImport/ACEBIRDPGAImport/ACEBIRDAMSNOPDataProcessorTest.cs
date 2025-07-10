using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDAMSNOPDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public void TestProcessOR2DeclaredMessageBlocks()
		{
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.OR2;
			amsHeader.US_IsElecImageSubmitted = true;
			amsHeader.US_NetWeight = 12.13m;
			amsHeader.US_NetWeightUQ = "KG";

			var amsLine1 = amsHeader.AMSLines.AddNew();
			amsLine1.US_CertType = LPCOTransactionTypeList.Codes.SingleUse;
			amsLine1.US_CertNumber = "123456789";
			var amsLine2 = amsHeader.AMSLines.AddNew();
			amsLine2.US_CertType = LPCOTransactionTypeList.Codes.Continuous;
			amsLine2.US_CertNumber = "987654321";
			var lotCode1 = amsHeader.LotCodes.AddNew();
			lotCode1.CY_Code = LotNumberQualifierList.Codes._1;
			lotCode1.CY_Data = "1122334455";
			var lotCode2 = amsHeader.LotCodes.AddNew();
			lotCode2.CY_Code = LotNumberQualifierList.Codes._3;
			lotCode2.CY_Data = "5544332211";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals(OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NOPInd);
			AssertEquals(1, invoiceLineImported.AMSLines.Count);

			var amsHeaderImported = invoiceLineImported.AMSLines[0];
			AssertEquals(AMSProgramList.Codes.OR2, amsHeaderImported.US_Program);
			AssertEquals(true, amsHeaderImported.US_IsElecImageSubmitted);
			AssertEquals(12.13m, amsHeaderImported.US_NetWeight);
			AssertEquals("KG", amsHeaderImported.US_NetWeightUQ);
			AssertEquals(2, amsHeaderImported.AMSLines.Count);
			var amsLine1Imported = amsHeaderImported.AMSLines.OfType<AMSLine>().First(x => x.US_CertNumber == "123456789");
			AssertEquals(LPCOTransactionTypeList.Codes.SingleUse, amsLine1Imported.US_CertType);
			var amsLine2Imported = amsHeaderImported.AMSLines.OfType<AMSLine>().First(x => x.US_CertNumber == "987654321");
			AssertEquals(LPCOTransactionTypeList.Codes.Continuous, amsLine2Imported.US_CertType);

			AssertEquals(2, amsHeaderImported.LotCodes.Count);
			var lotCode1Imported = amsHeaderImported.LotCodes.OfType<AMSLotCode>().First(x => x.CY_Data == "1122334455");
			AssertEquals(LotNumberQualifierList.Codes._1, lotCode1Imported.CY_Code);
			var lotCode2Imported = amsHeaderImported.LotCodes.OfType<AMSLotCode>().First(x => x.CY_Data == "5544332211");
			AssertEquals(LotNumberQualifierList.Codes._3, lotCode2Imported.CY_Code);
		}

		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_NOPDisclaimReason = "A";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);

			AssertEquals(1, DeclarationImported.InvoiceLines.Count);
			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			AssertEquals("US_NOPDisclaimReason", "A", invoiceLineImported.US_NOPDisclaimReason);
		}

		protected override void AssertEndToEndTestResult()
		{
			CombineAssertions(() =>
			{
				AssertEquals(2, DeclarationImported.CusContainers.Count);

				AssertEquals(1, DeclarationImported.InvoiceLines.Count);
				var invoiceLineImported = DeclarationImported.InvoiceLines[0];

				AssertEquals("NOPInd", OGAIndicatorList.Codes.Declared, invoiceLineImported.US_NOPInd);
				AssertEquals("JI_OA_ExporterAddress", exporter.MainAddress.PK, invoiceLineImported.JI_OA_ExporterAddress);

				AssertEquals("AMSHeader count", 1, invoiceLineImported.AMSLines.Count);
				var amsHeader = invoiceLineImported.AMSLines[0];

				AssertEquals("AMS Header US_Program", AMSProgramList.Codes.OR1, amsHeader.US_Program);
				Assert("AMS Header US_USDAOrganicStandard", amsHeader.US_USDAOrganicStandard);
				Assert("AMS Header US_EquivalentOrganicStandard", amsHeader.US_EquivalentOrganicStandard);
				AssertEquals("AMS Header US_CerNumber", "12345", amsHeader.US_CerNumber);
				Assert("AMS Header US_IsElecImageSubmitted", amsHeader.US_IsElecImageSubmitted);
				AssertEquals("AMS Header US_Date", new DateTime(2021, 04, 21), amsHeader.US_Date);
				AssertEquals("AMS Header US_OA_CertifyingBody", certifyingBody.MainAddress.PK, amsHeader.US_OA_CertifyingBody);
				AssertEquals("AMS Header US_OA_Recipient", recipient.MainAddress.PK, amsHeader.US_OA_Recipient);
				AssertEquals("AMS Header US_NetWeight", 100m, amsHeader.US_NetWeight);
				AssertEquals("AMS Header US_NetWeightUQ", "KG", amsHeader.US_NetWeightUQ);
				AssertEquals("AMS Header US_IntendedUseCode", "120", amsHeader.US_IntendedUseCode);
				AssertEquals("AMS Header US_Remarks", "ABC", amsHeader.US_Remarks);

				AssertEquals("AMS lines count", 2, amsHeader.AMSLines.Count);
				var line1 = amsHeader.AMSLines[0];
				AssertEquals("AMS Line1 US_ProductLabel", "54321", line1.US_ProductLabel);
				AssertEquals("AMS Line1 US_LotNumber", "100", line1.US_LotNumber);
				AssertEquals("AMS Line1 US_LotEntity", "1", line1.US_LotEntity);
				AssertEquals("AMS Line1 US_OA_FinalHandler", finalHandler.MainAddress.PK, line1.US_OA_FinalHandler);
				AssertEquals("AMS Line1 US_OA_CerFinalHandler", cerOfFinalHandler.MainAddress.PK, line1.US_OA_CerFinalHandler);

				var line2 = amsHeader.AMSLines[1];
				AssertEquals("AMS Line2 US_ProductLabel", "09876", line2.US_ProductLabel);
				AssertEquals("AMS Line2 US_LotNumber", "200", line2.US_LotNumber);
				AssertEquals("AMS Line2 US_LotEntity", "2", line2.US_LotEntity);
				AssertEquals("AMS Line2 US_OA_FinalHandler", cerOfFinalHandler.MainAddress.PK, line2.US_OA_FinalHandler);
				AssertEquals("AMS Line2 US_OA_CerFinalHandler", finalHandler.MainAddress.PK, line2.US_OA_CerFinalHandler);
			});
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = true;

			invoiceLine.JI_OA_ExporterAddress = exporter.MainAddress.PK;

			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.OR1;

			amsHeader.US_USDAOrganicStandard = true;
			amsHeader.US_EquivalentOrganicStandard = true;
			amsHeader.US_CerNumber = "12345";
			amsHeader.US_IsElecImageSubmitted = true;
			amsHeader.US_Date = new DateTime(2021, 04, 21);
			amsHeader.US_OA_CertifyingBody = certifyingBody.MainAddress.PK;
			amsHeader.US_OA_Recipient = recipient.MainAddress.PK;
			amsHeader.US_NetWeight = 100;
			amsHeader.US_NetWeightUQ = "KG";
			amsHeader.US_IntendedUseCode = "120";
			amsHeader.US_Remarks = "ABC";

			var amsLine1 = amsHeader.AMSLines.AddNew();
			amsLine1.US_ProductLabel = "54321";
			amsLine1.US_LotNumber = "100";
			amsLine1.US_LotEntity = "1";
			amsLine1.US_OA_FinalHandler = finalHandler.MainAddress.PK;
			amsLine1.US_OA_CerFinalHandler = cerOfFinalHandler.MainAddress.PK;

			var amsLine2 = amsHeader.AMSLines.AddNew();
			amsLine2.US_ProductLabel = "09876";
			amsLine2.US_LotNumber = "200";
			amsLine2.US_LotEntity = "2";
			amsLine2.US_OA_FinalHandler = cerOfFinalHandler.MainAddress.PK;
			amsLine2.US_OA_CerFinalHandler = finalHandler.MainAddress.PK;
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_NOPInd = OGAIndicatorList.Codes.Declared;

			exporter = CreatOrgHeader("AAAAAAA");
			certifyingBody = CreatOrgHeader("BBBBBBB");
			recipient = CreatOrgHeader("CCCCCCC");
			finalHandler = CreatOrgHeader("DDDDDDD");
			cerOfFinalHandler = CreatOrgHeader("EEEEEEE");
		}

		OrgHeader CreatOrgHeader(string code)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = "Full Name";
			var address = orgHeader.MainAddress;
			address.OA_RL_NKRelatedPortCode = "US2CW";
			address.OA_Address1 = code + "ADDRESS 1";
			address.OA_Address2 = code + "ADDRESS 2";
			address.OA_City = "MELBOURN";
			address.OA_State = "MEL";
			address.OA_PostCode = "2011";
			DeclarationTestHelper.AddPGAContact(address, "AAAAA", "CONTACT", "04 654321", "EMAIL", "FAX");
			return orgHeader;
		}

		OrgHeader exporter;
		OrgHeader certifyingBody;
		OrgHeader recipient;
		OrgHeader finalHandler;
		OrgHeader cerOfFinalHandler;
	}
}
