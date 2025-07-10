using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEBIRDCPSCDataProcessorTest : ACEBIRDCommonPGADataProcessorTest
	{
		public override void TestEndToEndProcessDisclaimedPGA()
		{
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_CPSCDisclaimReason = PGADisclaimReasonList.Codes.B;
			invoiceLine.JI_Description = "TEST CPSC DISCLAIMED";
			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			ImportMessageBlocks(message);
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals(OGAIndicatorList.Codes.Disclaimed, invoiceLineImported.US_CPSCInd);
				AssertEquals(PGADisclaimReasonList.Codes.B, invoiceLineImported.US_CPSCDisclaimReason);
				AssertEquals("TEST CPSC DISCLAIMED", invoiceLineImported.JI_Description);
			});
		}

		protected override MQEDIMessage GetEDIMessageForEndToEndTest()
		{
			invoiceLine.JI_Description = "TEST CPSC DESC";
			var cpscHeader = invoiceLine.CPSCHeaders.AddNew();
			cpscHeader.US_ProcessingCode = CPSCProcessingCodeList.Codes.FCP;
			cpscHeader.US_ProductIDType = ProductIDTypeCodeList.Codes.SRV;
			cpscHeader.US_ProductID = "5A102";
			cpscHeader.US_IntendedUseCode = IntendedUseCodesList.Codes.ForOtherUse;
			cpscHeader.US_IntendedUseDescription = "USER DESC";
			cpscHeader.US_SKUProductCode = "A789";
			cpscHeader.US_TradeBrandName = "BRAND NAME";
			cpscHeader.US_ProductName = "COTTON";
			cpscHeader.US_ModelNumber = "47851,78945,54862";
			cpscHeader.US_SerialNumber = "87451,51862";
			cpscHeader.US_RegisteredNumber = "123456,82451";
			cpscHeader.US_AltenateID = "82452,47851,78945,54862,112";
			cpscHeader.US_ModelColor = "RED";
			cpscHeader.US_ModelDescription = "TEST COLOR";
			cpscHeader.US_ModelStyle = "1";
			cpscHeader.US_OA_ManufacturerAddress = manufacturerAddress.PK;
			cpscHeader.US_CertificateExists = YesNoDefaultList.Codes.Yes;

			var lot1 = cpscHeader.Lots.AddNew();
			lot1.US_LotNumberType = LotNumberQualifierList.Codes._1;
			lot1.US_LotNumber = "47851";
			var lot2 = cpscHeader.Lots.AddNew();
			lot2.US_LotNumberType = LotNumberQualifierList.Codes._2;
			lot2.US_LotNumber = "47852";

			var ruleAndLab1 = cpscHeader.RuleAndLabs.AddNew();
			ruleAndLab1.US_OA_SafetyTestLocationAddress = safetyTestLocationAddress1.PK;
			ruleAndLab1.US_RuleCodes = "1203C,1215C";
			var ruleAndLab2 = cpscHeader.RuleAndLabs.AddNew();
			ruleAndLab2.US_CPSCAccreditedLabID = "ABC12345";
			ruleAndLab2.US_RuleCodes = "0018A,1207G,1223C";

			Factory.Save();

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			return builder.PopulateMessage();
		}

		protected override void AssertEndToEndTestResult()
		{
			AssertEquals(1, DeclarationImported.InvoiceLines.Count);

			var invoiceLineImported = DeclarationImported.InvoiceLines[0];
			CombineAssertions(delegate
			{
				AssertEquals("invoiceLineImported.US_CPSCInd", OGAIndicatorList.Codes.Declared, invoiceLineImported.US_CPSCInd);
				AssertEquals("invoiceLineImported.JI_Description", "TEST CPSC DESC", invoiceLineImported.JI_Description);
				AssertEquals("invoiceLineImported.CPSCHeaders.Count", 1, invoiceLineImported.CPSCHeaders.Count);

				var cpscHeaderImported = invoiceLineImported.CPSCHeaders[0];
				AssertEquals("cpscHeaderImported.US_ProcessingCode", CPSCProcessingCodeList.Codes.FCP, cpscHeaderImported.US_ProcessingCode);
				AssertEquals("cpscHeaderImported.US_ProductIDType", ProductIDTypeCodeList.Codes.SRV, cpscHeaderImported.US_ProductIDType);
				AssertEquals("cpscHeaderImported.US_ProductID", "5A102", cpscHeaderImported.US_ProductID);
				AssertEquals("cpscHeaderImported.US_IntendedUseCode", IntendedUseCodesList.Codes.ForOtherUse, cpscHeaderImported.US_IntendedUseCode);
				AssertEquals("cpscHeaderImported.US_IntendedUseDescription", "USER DESC", cpscHeaderImported.US_IntendedUseDescription);
				AssertEquals("cpscHeaderImported.US_SKUProductCode", "A789", cpscHeaderImported.US_SKUProductCode);
				AssertEquals("cpscHeaderImported.US_TradeBrandName", "BRAND NAME", cpscHeaderImported.US_TradeBrandName);
				AssertEquals("cpscHeaderImported.US_ProductName", "COTTON", cpscHeaderImported.US_ProductName);
				AssertEquals("cpscHeaderImported.US_ModelNumber", "47851,78945,54862", cpscHeaderImported.US_ModelNumber);
				AssertEquals("cpscHeaderImported.US_SerialNumber", "87451,51862", cpscHeaderImported.US_SerialNumber);
				AssertEquals("cpscHeaderImported.US_RegisteredNumber", "123456,82451", cpscHeaderImported.US_RegisteredNumber);
				AssertEquals("cpscHeaderImported.US_AltenateID", "82452,47851,78945,54862,112", cpscHeaderImported.US_AltenateID);
				AssertEquals("cpscHeaderImported.US_ModelColor", "RED", cpscHeaderImported.US_ModelColor);
				AssertEquals("cpscHeaderImported.US_ModelDescription", "TEST COLOR", cpscHeaderImported.US_ModelDescription);
				AssertEquals("cpscHeaderImported.US_ModelStyle", "1", cpscHeaderImported.US_ModelStyle);
				AssertEquals("cpscHeaderImported.US_OA_ManufacturerAddress", manufacturerAddress.PK, cpscHeaderImported.US_OA_ManufacturerAddress);
				AssertEquals("cpscHeaderImported.US_CertificateExists", YesNoDefaultList.Codes.Yes, cpscHeaderImported.US_CertificateExists);
				AssertEquals("cpscHeaderImported.Lots.Count", 2, cpscHeaderImported.Lots.Count);
				AssertEquals("cpscHeaderImported.RuleAndLabs.Count", 2, cpscHeaderImported.RuleAndLabs.Count);

				var lotImported1 = cpscHeaderImported.Lots[0];
				AssertEquals("lotImported1.US_LotNumberType", LotNumberQualifierList.Codes._1, lotImported1.US_LotNumberType);
				AssertEquals("lotImported1.US_LotNumber", "47851", lotImported1.US_LotNumber);
				var lotImported2 = cpscHeaderImported.Lots[1];
				AssertEquals("lotImported2.US_LotNumberType", LotNumberQualifierList.Codes._2, lotImported2.US_LotNumberType);
				AssertEquals("lotImported2.US_LotNumber", "47852", lotImported2.US_LotNumber);

				var ruleAndLabImported1 = cpscHeaderImported.RuleAndLabs[0];
				AssertEquals("ruleAndLabImported1.US_OA_SafetyTestLocationAddress", safetyTestLocationAddress1.PK, ruleAndLabImported1.US_OA_SafetyTestLocationAddress);
				AssertEquals("ruleAndLabImported1.US_RuleCodes", "1203C,1215C", ruleAndLabImported1.US_RuleCodes);
				var ruleAndLabImported2 = cpscHeaderImported.RuleAndLabs[1];
				AssertEquals("ruleAndLabImported2.US_CPSCAccreditedLabID", "ABC12345", ruleAndLabImported2.US_CPSCAccreditedLabID);
				AssertEquals("ruleAndLabImported2.US_RuleCodes", "0018A,1207G,1223C", ruleAndLabImported2.US_RuleCodes);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.US_CPSCInd = OGAIndicatorList.Codes.Declared;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "TOYOTA (JAPAN)";
			manufacturer.OH_Code = "TESTMANUF";
			manufacturer.MainAddress.OA_Address1 = "MAIN ADDRESS";
			manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "1234 PEACHTREE STREET";

			var safetyTestLocation = Factory.New<OrgHeader>();
			safetyTestLocation.OH_FullName = "TEST SAFETY LOCAT";
			safetyTestLocation.OH_Code = "SAFETYLOCA";
			safetyTestLocationAddress1 = safetyTestLocation.Addresses.AddNew();
			safetyTestLocationAddress1.OA_Address1 = "Location Address 1";
		}
		OrgAddress manufacturerAddress;
		OrgAddress safetyTestLocationAddress1;
	}
}
