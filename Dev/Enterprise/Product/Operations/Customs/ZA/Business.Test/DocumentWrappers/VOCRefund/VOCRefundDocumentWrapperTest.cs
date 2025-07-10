using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class VOCRefundDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestGettingFromEntryHeader()
		{
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testDeclaration.JE_AGTCode = "AGT";
			var entryInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CaseNumbers.AddNew("", "CASE1");
			entryInstruction.CaseNumbers.AddNew("", "CASE2");
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = entryInstruction.PK;
			testHeader.PenaltyAmountBefore = 100m;
			testHeader.CH_BGMReference = "LRNNumber";
			testHeader.MovementReferenceNumberSetter("MRNNumber", ZDateTime.Today);
			var testLine = testHeader.MergedLines.AddNew();
			testLine.Fees.AddOrUpdate("1P1", 60m);
			var tester = new VOCRefundDocumentWrapper(testHeader);
			CombineAssertions(() =>
			{
				AssertEquals("LRNNumber", tester.LocalReferenceNumber);
				AssertEquals("MRNNumber", tester.MovementReferenceNumber);
				AssertEquals("CASE1", tester.CaseNumber);
				AssertEquals("AGT", tester.AgentCode);
				AssertEquals(testAgent.PK, tester.EffectiveAgent.PK);
				AssertEquals("#1", tester.EffectiveAgentAddress);
				AssertEquals(ZString.Empty, tester.VOCReason);
				AssertEquals(40m, tester.RefundAmount);
			});
		}

		public void TestGettingFromMessage()
		{
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var testAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			testAgent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00505655", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testDeclaration.JE_AGTCode = "AGT";
			var entryInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CaseNumbers.AddNew("", "CASE1");
			entryInstruction.CaseNumbers.AddNew("", "CASE2");
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = entryInstruction.PK;
			testHeader.PenaltyAmountBefore = 100m;
			testHeader.CH_BGMReference = "LRNumber";
			testHeader.MovementReferenceNumberSetter("MRNNumber", ZDateTime.Today);
			var testMessage = GetOutGoingMessage("CHG", "UNH+1039+CUSDEC:D:96B:UN:ZZZ01'BGM+929+123::00002+4'CST++A:117:ZZZ'LOC+14+J4::ZZZ'LOC+35+US::5'LOC+36+BW::5'LOC+96+JHB::ZZZ'LOC+9+AUSYD::5'DTM+141:20170223:102'DTM+178:20170404:102'GIS+E:127:ZZZ'MEA+AAE+AAD+KGM:50000.00'FTX+LIN+++1::N::1'RFF+ABT:321'RFF+AAS:081-13233216'DTM+137:20170208:102'RFF+ACD:1039'RFF+AAV:CASE0'PAC+0'TDT+20++4'DOC+380+INVH'DTM+3:20170130:102'NAD+IM+12345678++JAS FORWARDING GMB+DREIEICHSTRASSE 8 . D-64546 MOERFEL:DEN-WALLDORF MOERFELDEN-WALLDO 6454:6+MOERFELDEN-WALLDO++64546'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+020110002:108:ZZZ+100'FTX+AAA+++LIVE ANIMALS; ANIMAL PRODUCTS MEAT AND EDIBLE MEAT OFFAL MEAT OF BOVIN:E ANIMALS, FRESH OR CHILLED  CARCASSES AND HALF CARCASSES FRESH OR CHI:LLEDOF BOVINE ANIMALS'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++KG:1.00'MOA+38:5600'MOA+40:5600'TAX+1+VAT:107:ZZZ'MOA+161:862.68'TAX+1+1P1:107:ZZZ'MOA+161:2.40'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:5600'TAX+3+TDD:107:ZZZ'MOA+161:2.40'TAX+3+TVD:107:ZZZ'MOA+161:862.68'TAX+3+CUS:107:ZZZ'MOA+161:5600'UNT+48+1039'");
			testMessage.ValueAddedTaxBefore = 60m;
			testMessage.PenaltyAmountAfter = 40m;
			testMessage.SetVOCReason("TESTVOCReason");
			testHeader.Messages.Add(testMessage);
			var testLine = testHeader.MergedLines.AddNew();
			testLine.Fees.AddOrUpdate("1P1", 60m);
			Factory.Save();
			var tester = VOCRefundDocumentWrapper.NewForMessage(testMessage);
			CombineAssertions(() =>
			{
				AssertEquals("123", tester.LocalReferenceNumber);
				AssertEquals("321", tester.MovementReferenceNumber);
				AssertEquals("CASE0", tester.CaseNumber);
				AssertEquals("00505655", tester.AgentCode);
				AssertEquals(testAgent2.PK, tester.EffectiveAgent.PK);
				AssertEquals("TESTVOCReason", tester.VOCReason);
				AssertEquals(20m, tester.RefundAmount);
			});
		}

		public void TestRefundAmount()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.PenaltyAmountBefore = 40.4444m;
			testHeader.CH_BGMReference = "LRNumber";
			testHeader.MovementReferenceNumberSetter("MRNNumber", ZDateTime.Today);
			var testLine = testHeader.MergedLines.AddNew();
			testLine.Fees.AddOrUpdate("1P1", 60m);
			Factory.Save();
			CombineAssertions(() =>
			{
				var tester = new VOCRefundDocumentWrapper(testHeader);
				AssertEquals("Case of UnderEntry", 0m, tester.RefundAmount);
				testHeader.ValueAddedTaxBefore = 45m;
				tester = new VOCRefundDocumentWrapper(testHeader);
				AssertEquals("Case of OverEntry", 25.44m, tester.RefundAmount);
			});
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
		}

		CUSDECEDIMessage GetOutGoingMessage(ZString subType, ZString text)
		{
			var testMessage = Factory.New<CUSDECEDIMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = subType;
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = text;
			testMessage.MessageNumForTesting = ZDateTime.Now.Ticks.ToString();
			return testMessage;
		}
	}
}
