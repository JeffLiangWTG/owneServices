using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class VOCDocumentPackDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestTransportDocumentNumberInBusinessLogic()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_MasterBill = "MSCU123450";
			dec.JE_CarrierCode = "MSC";
			var testHeader = dec.CustomsEntryHeaders.AddNew();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			CombineAssertions(() =>
			{
				using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
				{
					AssertEquals("SEA, TDTSegmentSplit is enabled", "MSC MSCU123450", tester.TransportDocumentNumberInBusinessLogic);
				}

				using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
				{
					AssertEquals("SEA, TDTSegmentSplit is disabled", "MSC MSCU123450", tester.TransportDocumentNumberInBusinessLogic);
				}

				dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("AIR", "MSC-U123450", tester.TransportDocumentNumberInBusinessLogic);
			});
		}

		public void TestEntryDocType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_ReceiveTransmit = "TRX";
			cusdecMessage.EM_ApplicationCode = "ZAC";
			cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			cusdecMessage.EM_Status = "SNT";
			cusdecMessage.EM_MessageText = ZA.Business.Testing.CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			cusdecMessage.EM_MessageNum = "202";
			entryHeader.Messages.Add(cusdecMessage);
			var cusresMessage = Factory.New<CUSRESEDIMessage>();
			cusresMessage.EM_ReceiveTransmit = "RCV";
			cusresMessage.EM_ApplicationCode = "ZAC";
			cusresMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			cusresMessage.EM_Status = "PRS";
			cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "");
			entryHeader.Messages.Add(cusresMessage);
			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("BGM+929+00626166CLP20160426000199::00002+9'", $"BGM+929:::{declarationTypePair.Code}+00626166CLP20160426000199::00002+9'");
				cusdecMessage.ResetDeclarationType();
				cusdecMessage.ResetCUSDECHelper();
				switch (declarationTypePair.Code)
				{
					case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
						{
							var wrapper = new VOCDocumentPackDocumentWrapper(entryHeader);
							AssertEquals(declarationTypePair.Code, "RCD", wrapper.EntryDocType);
							AssertEquals(declarationTypePair.Code, "RCD", ((MessageSendingObject)(wrapper.CUSDECSource)).MessageKeyFactor.DeclarationType);
						}

						break;
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						{
							var wrapper = new VOCDocumentPackDocumentWrapper(entryHeader);
							AssertEquals(declarationTypePair.Code, "RSD", wrapper.EntryDocType);
							AssertEquals(declarationTypePair.Code, "RSD", ((MessageSendingObject)(wrapper.CUSDECSource)).MessageKeyFactor.DeclarationType);
						}

						break;
				}
			}

			foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
			{
				cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("BGM+929+00626166CLP20160426000199::00002+9'", $"BGM+929:::{declarationTypePair.Code}+00626166CLP20160426000199::00002+9'");
				cusdecMessage.ResetDeclarationType();
				cusdecMessage.ResetCUSDECHelper();
				AssertEquals(declarationTypePair.Code, VOCDocumentPackDocumentWrapper.NewForMessage(cusdecMessage).EntryDocType);
			}

			cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			AssertEquals("RCD", VOCDocumentPackDocumentWrapper.NewForMessage(cusdecMessage).EntryDocType);
		}

		public void TestGettingFromEntryHeader()
		{
			string cUSDEC_BGM9_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			string cUSDEC_BGM5_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+5'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			string cUSRES_GIS6_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655BBR20160513000034::00001'DTM+9:20160513143147:202'TDT+20'LOC+22+BBR'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+BH:00505655'RFF+AAS:HENRYMASTER1'RFF+ACD:<<INTERCHANGENUMBERPLACEHOLDER>>'ERP+1:0000'ERC+0000'FTX+AAO+++Line number may not be 0'UNT+15+1'";
			string cUSRES_GIS1_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655CLP20160514000245:0'DTM+178:20160511:102'DTM+202:20160511:102'TDT+20+QF987+4+++++::: 'LOC+22+CLP::ZZZ'LOC+14+XW::ZZZ'GIS+1:120:ZZZ:Y'NAD+AG+00505655'RFF+BH:0003264'RFF+AAS:081-99876545'DTM+137:20160305:102'RFF+ABT:CLP201605145000001'DTM+137:20160514:102'RFF+UCN:6ZA01702826INV158'RFF+ACD:<<INTERCHANGENUMBERPLACEHOLDER>>'TAX+3+CUS:107:ZZZ'MOA+161:490688'CNT+7:226.79'CNT+11:5'UNT+21+1'";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			ZAMessage outgoingMessage = Factory.New<ZAMessageForTest>();
			CombineAssertions("New", () =>
			{
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("Sent Original, Still getting the job as source", () =>
			{
				ZAMessage testMessage = GetOutGoingMessage("ORG", cUSDEC_BGM9_Message);
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				outgoingMessage = testMessage;
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("Original Rejected, Still getting the job as source", () =>
			{
				ZAMessage testMessage = GetInComingMessage(cUSRES_GIS6_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", outgoingMessage.EM_MessageNum));
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("Original Accepted, Still getting the job as source", () =>
			{
				ZAMessage testMessage = GetInComingMessage(cUSRES_GIS1_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", outgoingMessage.EM_MessageNum));
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("Sent Change, Still getting the job as source", () =>
			{
				ZAMessage testMessage = GetOutGoingMessage("CHG", cUSDEC_BGM5_Message);
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				outgoingMessage = testMessage;
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("Change Rejected, Still getting the job as source", () =>
			{
				ZAMessage testMessage = GetInComingMessage(cUSRES_GIS6_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", outgoingMessage.EM_MessageNum));
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("Change Accepted, Still getting the message as source", () =>
			{
				ZAMessage testMessage = GetInComingMessage(cUSRES_GIS1_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", outgoingMessage.EM_MessageNum));
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
			CombineAssertions("2ndChange Rejected, Still 1st Change", () =>
			{
				ZAMessage testMessage = GetOutGoingMessage("CHG", cUSDEC_BGM5_Message);
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				var outgoingMessage2 = testMessage;
				testMessage = GetInComingMessage(cUSRES_GIS6_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", outgoingMessage2.EM_MessageNum));
				testHeader.Messages.Add(testMessage);
				Factory.Save();
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
				AssertEquals("CUSDECSource", tester.CUSDECSource, tester.AfterValueProvider);
				AssertEquals("SourceMessage", null, tester.SourceMessage);
			});
		}

		public void TestGettingFromMessage()
		{
			string cUSDEC_BGM5_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+5'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			ZAMessage testMessage = GetOutGoingMessage("CHG", cUSDEC_BGM5_Message);
			testHeader.Messages.Add(testMessage);
			Factory.Save();
			var outgoingMessage = testMessage;
			var tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("VOC Value Providers", tester.BeforeValueProvider, tester.AfterValueProvider);
			AssertEquals("CUSDECSource", tester.SourceMessage, tester.AfterValueProvider);
			AssertEquals("SourceMessage", outgoingMessage.PK, tester.SourceMessage.PK);
			AssertEquals("SourceHeader", testHeader.PK, tester.EntryHeader.PK);
			AssertEquals("VATIndicator", "N", tester.VATIndicator);
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().NewWithValidTestData<CUSDECEDIMessage>());
			AssertNull(tester);
			testMessage = GetOutGoingMessage("CHG", "INVALID MESSAGE CONTENT WON'T GENERATE WRAPPER");
			testHeader.Messages.Add(testMessage);
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertNull(tester);
		}

		public void TestMovementReferenceNumberDate()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = "SEA";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.MovementReferenceNumberSetter("JSA201601010000001", ZDateTime.Now);
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(new ZDateTime(2016, 01, 01), tester.MovementReferenceNumberDate);
			testHeader.MovementReferenceNumberSetter("JSA20160010000001", ZDateTime.Now);
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(ZDateTime.Invalid, tester.MovementReferenceNumberDate);
		}

		public void TestMarksAndNumbers()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.JE_MarksAndNumbers = @"123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
5678901234567890
123456789012
3456789012345678901234567890123456
78901234567890";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(@"123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
5678901234567890
123456789012
3456789012345678901234567890123456
78901234567890", tester.MarksAndNumbers);
			var testLine = testHeader.MergedLines.AddNew();
			var testInvLine = testDeclaration.InvoiceLines.AddNew();
			testInvLine.JI_Procedure = "0020";
			testLine.InvoiceLines.Add(testInvLine);
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(@"123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
5678901234567890
123456789012
3456789012345678901234567890123456
78901234567890", tester.MarksAndNumbers);
		}

		public void TestConsignee()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "orgname";
			testOrg.MainAddress.Address1 = "addr1";
			testOrg.MainAddress.Address2 = "addr2";
			testOrg.MainAddress.Postcode = "POC";
			var cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			cusCode.OK_CustomsRegNo = "00010008";
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "EXP";
			testDeclaration.JE_OH_Importer = testOrg.PK;
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("ADDR1 ADDR2 POC", tester.Importer.Address);
			AssertEquals("00010008", tester.Importer.OrganizationCode);
		}

		public void TestToWarehouseAddress()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "orgname";
			testOrg.MainAddress.Address1 = "addr1";
			testOrg.MainAddress.Address2 = "addr2";
			testOrg.MainAddress.Postcode = "POC";
			var cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode.OK_CustomsRegNo = "00010008";
			cusCode.OK_OA_PremisesAddress = testOrg.MainAddress.PK;
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "43";
			testInst.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = testInst.PK;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.InvoiceLines.Add(invLine);
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("ORGNAME ADDR1 ADDR2 POC", tester.ToWarehouseAddress);
			AssertEquals("00010008", tester.ToWarehouse);
		}

		public void TestFromWarehouseAddress()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "orgname";
			testOrg.MainAddress.Address1 = "addr1";
			testOrg.MainAddress.Address2 = "addr2";
			testOrg.MainAddress.Postcode = "POC";
			var cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode.OK_CustomsRegNo = "00010008";
			cusCode.OK_OA_PremisesAddress = testOrg.MainAddress.PK;
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "13";
			testInst.CEI_OA_Warehouse = testOrg.MainAddress.PK;
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = testInst.PK;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.InvoiceLines.Add(invLine);
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("ORGNAME ADDR1 ADDR2 POC", tester.FromWarehouseAddress);
			AssertEquals("00010008", tester.FromWarehouse);
		}

		public void TestRemoverAddress()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "IMP", "");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "40", "43", "", "", "", "");
			Factory.Save();
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "orgname";
			testOrg.MainAddress.Address1 = "addr1";
			testOrg.MainAddress.Address2 = "addr2";
			testOrg.MainAddress.Postcode = "POC";
			testOrg.OH_RL_NKClosestPort = "ZAAOB";
			testOrg.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "00010008", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			testInst.CEI_OH_Carrier = testOrg.PK;
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = testInst.PK;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.InvoiceLines.Add(invLine);
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("ORGNAME ADDR1 ADDR2 POC", tester.RemoverAddress);
			AssertEquals("00010008", tester.RemoverTransporterCode);
			testInst.CEI_OH_Carrier = ZGuid.Empty;
			string value = "";
			AssertNoExceptionThrown("No exception should be thrown when Remover is null", () => value = tester.RemoverAddress);
		}

		public void TestFromWarehouseAddress_FromMessage()
		{
			var orgCPW = SetupOrgheader("orgname1", OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "00010008");
			var entryCPW = SetupEntryHeader(orgCPW, "13", "43");
			var testMessage = SetupCUSDECMessage(entryCPW);
			Factory.Save();
			var tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("00010008", tester.FromWarehouse);
			AssertEquals("ORGNAME1 ADDR1 ADDR2 POC", tester.FromWarehouseAddress);
			orgCPW.CustomsCodes[0].OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("", tester.FromWarehouseAddress);
			AssertEquals("CodeType not match", "00010008", tester.FromWarehouse);
			orgCPW.CustomsCodes[0].OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			orgCPW.CustomsCodes[0].OK_CustomsRegNo = "000A0008";
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("RegistryNumber not match", "", tester.FromWarehouseAddress);
			AssertEquals("00010008", tester.FromWarehouse);
		}

		public void TestToWarehouseAddress_FromMessage()
		{
			var orgCPW = SetupOrgheader("orgname1", OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "00010008");
			var entryCPW = SetupEntryHeader(orgCPW, "43", "");
			var testMessage = SetupCUSDECMessage(entryCPW);
			entryCPW.Messages.Add(testMessage);
			Factory.Save();
			var tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("00010008", tester.ToWarehouse);
			AssertEquals("ORGNAME1 ADDR1 ADDR2 POC", tester.ToWarehouseAddress);
			orgCPW.CustomsCodes[0].OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("CodeType not match", "", tester.ToWarehouseAddress);
			AssertEquals("00010008", tester.ToWarehouse);
			orgCPW.CustomsCodes[0].OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			orgCPW.CustomsCodes[0].OK_CustomsRegNo = "000A0008";
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("RegistryNumber not match", "", tester.ToWarehouseAddress);
			AssertEquals("00010008", tester.ToWarehouse);
		}

		public void TestRemoverAddress_FromMessage()
		{
			var orgREM = SetupOrgheader("orgname2", OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "00020008", false);
			var entryREM = SetupEntryHeader(orgREM, "43", "");
			var testMessage = SetupCUSDECMessage(entryREM);
			Factory.Save();
			var tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("00020008", tester.RemoverTransporterCode);
			AssertEquals("ORGNAME2 ADDR1 ADDR2 POC", tester.RemoverAddress);
			orgREM.CustomsCodes[0].OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("CodeType not match", "", tester.RemoverAddress);
			AssertEquals("00020008", tester.RemoverTransporterCode);
			orgREM.CustomsCodes[0].OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			orgREM.CustomsCodes[0].OK_CustomsRegNo = "000A0008";
			Factory.Save();
			tester = VOCDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().Load<ZAMessage>(testMessage.PK) as CUSDECEDIMessage);
			AssertEquals("RegistryNumber not match", "", tester.RemoverAddress);
			AssertEquals("00020008", tester.RemoverTransporterCode);
		}

		public void TestLocationOfGoodsName()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCusCodeType("FAC", "Facility");
			var codeXX = testHelper.CreateZACusCodeListEntry("FAC", "XX", "XX DESC");
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.DistrictOffices, "Desc.", "FAC", Core.Constants.CountryCodes.SouthAfrica);
			codeXX.Attributes.AddNew(Universal.RefCusCodeListAttributeTypes.Codes.DistrictOffices, ZString.Empty);
			testHelper.CreateZACusCodeListEntry("FAC", "YY");
			testHelper.CreateZACusCodeListEntry("FAC", "ZZ");
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_LocationOfGoods = "XX";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("XX", tester.LocationOfGoods);
			AssertEquals("XX DESC", tester.LocationOfGoodsName);
			testDeclaration.JE_LocationOfGoods = "YY";
			testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("YY", tester.LocationOfGoods);
			AssertEquals("", tester.LocationOfGoodsName);
			testDeclaration.JE_LocationOfGoods = "ZZ";
			testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals("ZZ", tester.LocationOfGoods);
			AssertEquals("", tester.LocationOfGoodsName);
		}

		public void TestNoOfPacksUnit()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = "SEA";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_Packages = 0;
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(0, tester.NoOfPacksUnit);
			AssertEquals(0, tester.NoOfPacksTens);
			AssertEquals(0, tester.NoOfPacksHundred);
			AssertEquals(0, tester.NoOfPacksThousand);
			testHeader.CH_Packages = 21;
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(1, tester.NoOfPacksUnit);
			AssertEquals(2, tester.NoOfPacksTens);
			AssertEquals(0, tester.NoOfPacksHundred);
			AssertEquals(0, tester.NoOfPacksThousand);
			testHeader.CH_Packages = 76598;
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(8, tester.NoOfPacksUnit);
			AssertEquals(9, tester.NoOfPacksTens);
			AssertEquals(5, tester.NoOfPacksHundred);
			AssertEquals(76, tester.NoOfPacksThousand);
		}

		public void TestContainerOverFlow()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "IMP";
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT01";
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT02";
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT03";
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT04";
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT05";
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT06";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(6, tester.ContainersCollection.Count);
			AssertEquals(ZBool.False, tester.ContainersOverFlow);
			testDeclaration.CusContainers.AddNew().CO_ContainerNumber = "CONT07";
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(7, tester.ContainersCollection.Count);
			AssertEquals(ZBool.True, tester.ContainersOverFlow);
			testDeclaration.JE_MessageType = "EXW";
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(0, tester.ContainersCollection.Count);
			AssertEquals(ZBool.False, tester.ContainersOverFlow);
			testDeclaration.JE_MessageType = "EXP";
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(7, tester.ContainersCollection.Count);
			AssertEquals(ZBool.True, tester.ContainersOverFlow);
			testDeclaration.JE_TransportMode = "AIR";
			tester = new VOCDocumentPackDocumentWrapper(testHeader);
			AssertEquals(0, tester.ContainersCollection.Count);
			AssertEquals(ZBool.False, tester.ContainersOverFlow);
		}

		public void TestAmountsAndDifference_FromCusEntryHeader()
		{
			CombineAssertions("EXP", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "40";
				var invHeader = testDeclaration.Invoices.AddNew();
				var invLine1 = invHeader.InvoiceLines.AddNew();
				invLine1.JI_CEI = testInst.PK;
				invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "43";
				invLine1.JI_Tariff = "00001000";
				invLine1.JI_ZZF_NKTaxType = "VAT";
				var invLine2 = invHeader.InvoiceLines.AddNew();
				invLine2.JI_CEI = testInst.PK;
				invLine2.JI_Procedure = invLine2.EntryInstruction.CEI_Style + "43";
				invLine2.JI_Tariff = "00001000";
				invLine2.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testHeader.CustomsDutyExcluding12BBefore = 90m;
				testHeader.S1P2BDutyBefore = 30m;
				testHeader.ValueAddedTaxBefore = 20m;
				testHeader.ProvisionalPaymentAmountBefore = 9m;
				testHeader.PenaltyAmountBefore = 3m;
				var testLine1 = testHeader.MergedLines.AddNew();
				testLine1.CL_CustomsValue = 20m;
				testLine1.Fees.AddOrUpdate("1P1", 500m);
				testLine1.Fees.AddOrUpdate("12A", 390m);
				testLine1.Fees.AddOrUpdate("12B", 250m);
				testLine1.Fees.AddOrUpdate("VAT", 200m);
				testLine1.ProvisionalPayments.AddNew("PPA", 50m);
				testLine1.ProvisionalPayments.AddNew("PEN", 20m);
				testLine1.InvoiceLines.Add(invLine1);
				var testLine2 = testHeader.MergedLines.AddNew();
				testLine2.CL_CustomsValue = 20m;
				testLine2.Fees.AddOrUpdate("12A", 10m);
				testLine2.Fees.AddOrUpdate("12B", 50m);
				testLine2.ProvisionalPayments.AddNew("PPR", 40m);
				testLine2.ProvisionalPayments.AddNew("PEN", 10m);
				testLine2.InvoiceLines.Add(invLine2);
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals(90m, tester.CustomsDutyNoS1P2BBefore);
				AssertEquals(900m, tester.CustomsDutyNoS1P2BAfter);
				AssertEquals(810m, tester.CustomsDutyNoS1P2BDifference);
				AssertEquals(30m, tester.S1P2BDutyBefore);
				AssertEquals(300m, tester.S1P2BDutyAfter);
				AssertEquals(270m, tester.S1P2BDutyDifference);
				AssertEquals(20m, tester.ValueAddedTaxBefore);
				AssertEquals(200m, tester.ValueAddedTaxAfter);
				AssertEquals(180m, tester.ValueAddedTaxDifference);
				AssertEquals(12m, tester.ProvisionalPaymentsAndPenaltiesBefore);
				AssertEquals(0m, tester.ProvisionalPaymentsAndPenaltiesAfter);
				AssertEquals(-12m, tester.ProvisionalPaymentsAndPenaltiesDifference);
				AssertEquals(152m, tester.AmountDueBefore);
				AssertEquals(1400m, tester.AmountDueAfter);
				AssertEquals(1248m, tester.AmountDueDifference);
			});
			CombineAssertions("IMP", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "40";
				var invHeader = testDeclaration.Invoices.AddNew();
				var invLine1 = invHeader.InvoiceLines.AddNew();
				invLine1.JI_CEI = testInst.PK;
				invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "43";
				invLine1.JI_Tariff = "00001000";
				invLine1.JI_ZZF_NKTaxType = "VAT";
				var invLine2 = invHeader.InvoiceLines.AddNew();
				invLine2.JI_CEI = testInst.PK;
				invLine2.JI_Procedure = invLine2.EntryInstruction.CEI_Style + "43";
				invLine2.JI_Tariff = "00001000";
				invLine2.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testHeader.CustomsDutyExcluding12BBefore = 90m;
				testHeader.S1P2BDutyBefore = 30m;
				testHeader.ValueAddedTaxBefore = 20m;
				testHeader.ProvisionalPaymentAmountBefore = 9m;
				testHeader.PenaltyAmountBefore = 3m;
				var testLine1 = testHeader.MergedLines.AddNew();
				testLine1.CL_CustomsValue = 20m;
				testLine1.Fees.AddOrUpdate("1P1", 500m);
				testLine1.Fees.AddOrUpdate("12A", 390m);
				testLine1.Fees.AddOrUpdate("12B", 250m);
				testLine1.Fees.AddOrUpdate("VAT", 200m);
				testLine1.ProvisionalPayments.AddNew("PPA", 50m);
				testLine1.ProvisionalPayments.AddNew("PEN", 20m);
				testLine1.InvoiceLines.Add(invLine1);
				var testLine2 = testHeader.MergedLines.AddNew();
				testLine2.CL_CustomsValue = 20m;
				testLine2.Fees.AddOrUpdate("12A", 10m);
				testLine2.Fees.AddOrUpdate("12B", 50m);
				testLine2.ProvisionalPayments.AddNew("PPR", 40m);
				testLine2.ProvisionalPayments.AddNew("PEN", 10m);
				testLine2.InvoiceLines.Add(invLine2);
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals(90m, tester.CustomsDutyNoS1P2BBefore);
				AssertEquals(900m, tester.CustomsDutyNoS1P2BAfter);
				AssertEquals(810m, tester.CustomsDutyNoS1P2BDifference);
				AssertEquals(30m, tester.S1P2BDutyBefore);
				AssertEquals(300m, tester.S1P2BDutyAfter);
				AssertEquals(270m, tester.S1P2BDutyDifference);
				AssertEquals(20m, tester.ValueAddedTaxBefore);
				AssertEquals(200m, tester.ValueAddedTaxAfter);
				AssertEquals(180m, tester.ValueAddedTaxDifference);
				AssertEquals(12m, tester.ProvisionalPaymentsAndPenaltiesBefore);
				AssertEquals(120m, tester.ProvisionalPaymentsAndPenaltiesAfter);
				AssertEquals(108m, tester.ProvisionalPaymentsAndPenaltiesDifference);
				AssertEquals(152m, tester.AmountDueBefore);
				AssertEquals(1520m, tester.AmountDueAfter);
				AssertEquals(1368m, tester.AmountDueDifference);
			});
			CombineAssertions("EXW", () =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "40";
				var invHeader = testDeclaration.Invoices.AddNew();
				var invLine1 = invHeader.InvoiceLines.AddNew();
				invLine1.JI_CEI = testInst.PK;
				invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "43";
				invLine1.JI_Tariff = "00001000";
				invLine1.JI_ZZF_NKTaxType = "VAT";
				var invLine2 = invHeader.InvoiceLines.AddNew();
				invLine2.JI_CEI = testInst.PK;
				invLine2.JI_Procedure = invLine2.EntryInstruction.CEI_Style + "43";
				invLine2.JI_Tariff = "00001000";
				invLine2.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testHeader.CustomsDutyExcluding12BBefore = 90m;
				testHeader.S1P2BDutyBefore = 30m;
				testHeader.ValueAddedTaxBefore = 20m;
				testHeader.ProvisionalPaymentAmountBefore = 9m;
				testHeader.PenaltyAmountBefore = 3m;
				var testLine1 = testHeader.MergedLines.AddNew();
				testLine1.CL_CustomsValue = 20m;
				testLine1.Fees.AddOrUpdate("1P1", 500m);
				testLine1.Fees.AddOrUpdate("12A", 390m);
				testLine1.Fees.AddOrUpdate("12B", 250m);
				testLine1.Fees.AddOrUpdate("VAT", 200m);
				testLine1.ProvisionalPayments.AddNew("PPA", 50m);
				testLine1.ProvisionalPayments.AddNew("PEN", 20m);
				testLine1.InvoiceLines.Add(invLine1);
				var testLine2 = testHeader.MergedLines.AddNew();
				testLine2.CL_CustomsValue = 20m;
				testLine2.Fees.AddOrUpdate("12A", 10m);
				testLine2.Fees.AddOrUpdate("12B", 50m);
				testLine2.ProvisionalPayments.AddNew("PPR", 40m);
				testLine2.ProvisionalPayments.AddNew("PEN", 10m);
				testLine2.InvoiceLines.Add(invLine2);
				var tester = new VOCDocumentPackDocumentWrapper(testHeader);
				AssertEquals(90m, tester.CustomsDutyNoS1P2BBefore);
				AssertEquals(900m, tester.CustomsDutyNoS1P2BAfter);
				AssertEquals(810m, tester.CustomsDutyNoS1P2BDifference);
				AssertEquals(30m, tester.S1P2BDutyBefore);
				AssertEquals(300m, tester.S1P2BDutyAfter);
				AssertEquals(270m, tester.S1P2BDutyDifference);
				AssertEquals(20m, tester.ValueAddedTaxBefore);
				AssertEquals(200m, tester.ValueAddedTaxAfter);
				AssertEquals(180m, tester.ValueAddedTaxDifference);
				AssertEquals(12m, tester.ProvisionalPaymentsAndPenaltiesBefore);
				AssertEquals(120m, tester.ProvisionalPaymentsAndPenaltiesAfter);
				AssertEquals(108m, tester.ProvisionalPaymentsAndPenaltiesDifference);
				AssertEquals(152m, tester.AmountDueBefore);
				AssertEquals(1520m, tester.AmountDueAfter);
				AssertEquals(1368m, tester.AmountDueDifference);
			});
		}

		public void TestAmountsAndDifference_FromEDIMessage()
		{
			var testHeader = Factory.New<CusEntryHeader>();
			var testMessage = Factory.New<CUSDECEDIMessage>();
			testMessage.CustomsDutyNoS1P2BBefore = 90m;
			testMessage.S1P2BDutyBefore = 30m;
			testMessage.ValueAddedTaxBefore = 20m;
			testMessage.ProvisionalPaymentAmountBefore = 9m;
			testMessage.PenaltyAmountBefore = 3m;
			testMessage.CustomsDutyNoS1P2BAfter = 900m;
			testMessage.S1P2BDutyAfter = 300m;
			testMessage.ValueAddedTaxAfter = 200m;
			testMessage.ProvisionalPaymentAmountAfter = 90m;
			testMessage.PenaltyAmountAfter = 30m;
			testMessage.EM_LinkUniqueID = testHeader.PK;
			testMessage.EM_LinkTable = "CusEntryHeader";
			testMessage.EM_MessageText = @"UNH+3242+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+00505655JSA20161020003240::00001+9'
CST++D:117:ZZZ'
LOC+14+A9::ZZZ'
LOC+35+DE::5'
LOC+36+ZA::5'
LOC+96+JSA::ZZZ'
LOC+9+DEHAM::5'
DTM+178:20161021:102'
GIS+D:134:ZZZ'
MEA+AAE+AAD+KGM:10.00'
FTX+LIN+++1::Y'
RFF+AAS:082-12345675'
DTM+137:20161020:102'
RFF+ABI:8120067395'
RFF+ACD:3242'
PAC+1'
TDT+20+SA132+4'
DOC+380+INVTEMP'
DTM+3:20161020:102'
NAD+IM+00549578++TIM IMPORT CO+88 STREETNAME DBN DURBAN 4362+DURBAN++4362'
RFF+VA:4320190434'
NAD+AG+00505655'
NAD+MS+TST'
UNS+D'
CST+0001+820110059:108:ZZZ+100'
FTX+AAA+++BASE METALS AND ARTICLES OF BASE METAL TOOLS, IMPLEMENTS, CUTLERY, SPO:ONS AND FORKS, OF BASE METAL; PARTS THEREOF OF BASE METAL HAND TOOLS, :THE FOLLOWING  SPADES, SHOVELS, MATTOCKS, PICKS, HOES, FORKS AND RAKES:; AXES, BILL HOOKSAND SIMILAR HEWING TOOLS; SECATEURS AND PRUNERS OF A:NY KIND; SCYTHES,SICKLES, HAY KNIVES, HEDGE SHEA REBATE AMOUNT; 1000.0'
FTX+ACB+++NUIN'
FTX+CCI+++35:00:48010010002'
LOC+27+JP'
MEA+AAR++KG:100.00'
NAD+WH+00549578'
MOA+38:5000'
MOA+40:5000'
TAX+1+1P1:107:ZZZ'
MOA+161:500.00'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:5000'
TAX+3+TDD:107:ZZZ'
MOA+161:500.00'
TAX+3+TVD:107:ZZZ'
MOA+161:770.00'
TAX+3+CUS:107:ZZZ'
MOA+161:5000'
UNT+48+3242'".Replace("\r\n", "");
			var tester = VOCDocumentPackDocumentWrapper.NewForMessage(testMessage);
			CombineAssertions(() =>
			{
				AssertEquals(90m, tester.CustomsDutyNoS1P2BBefore);
				AssertEquals(900m, tester.CustomsDutyNoS1P2BAfter);
				AssertEquals(810m, tester.CustomsDutyNoS1P2BDifference);
				AssertEquals(30m, tester.S1P2BDutyBefore);
				AssertEquals(300m, tester.S1P2BDutyAfter);
				AssertEquals(270m, tester.S1P2BDutyDifference);
				AssertEquals(20m, tester.ValueAddedTaxBefore);
				AssertEquals(200m, tester.ValueAddedTaxAfter);
				AssertEquals(180m, tester.ValueAddedTaxDifference);
				AssertEquals(12m, tester.ProvisionalPaymentsAndPenaltiesBefore);
				AssertEquals(120m, tester.ProvisionalPaymentsAndPenaltiesAfter);
				AssertEquals(108m, tester.ProvisionalPaymentsAndPenaltiesDifference);
				AssertEquals(152m, tester.AmountDueBefore);
				AssertEquals(1520m, tester.AmountDueAfter);
				AssertEquals(1368m, tester.AmountDueDifference);
			});
		}

		public void TestPartClearanceIndex()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testDeclaration.JE_TransportMode = "SEA";
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			testInstruction1.CEI_MRNToBeReplaced = "MRN001";
			var testInstruction2 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "11";
			testInstruction2.CEI_MRNToBeReplaced = "MRN003";
			var testInstruction3 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = "11";
			var testInstruction4 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = "11";
			var testHeader1 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader1.CH_CEI_Instruction = testInstruction1.PK;
			testHeader1.CH_BGMReference = "LRN002";
			var testHeader2 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader2.CH_CEI_Instruction = testInstruction2.PK;
			testHeader2.CH_BGMReference = "LRN001";
			var testHeader3 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader3.CH_CEI_Instruction = testInstruction3.PK;
			testHeader3.MovementReferenceNumberSetter("MRN001", ZDateTime.Now);
			testHeader3.CH_BGMReference = "LRN003";
			var testHeader4 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader4.CH_CEI_Instruction = testInstruction4.PK;
			testHeader4.MovementReferenceNumberSetter("MRN002", ZDateTime.Now);
			testHeader4.CH_BGMReference = "LRN004";
			Factory.Save();
			new LineMerger(testDeclaration).DoMerge();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader1);
			AssertEquals(2, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
			tester = new VOCDocumentPackDocumentWrapper(testHeader2);
			AssertEquals(1, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
			tester = new VOCDocumentPackDocumentWrapper(testHeader3);
			AssertEquals(0, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
			tester = new VOCDocumentPackDocumentWrapper(testHeader4);
			AssertEquals(3, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
		}

		public void TestSupplierFallback()
		{
			AddressInformationForTest supplierInfo = new AddressInformationForTest();
			CUSDECMessageDataProviderForTest messageDataProvider = new CUSDECMessageDataProviderForTest();
			messageDataProvider.Supplier = supplierInfo;
			var textBuilder = new CUSDECMessageTextBuilderForTest(messageDataProvider);
			// message without any supplier information
			// expected result: should fallback to supplier in BO
			var noSupplierMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			noSupplierMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with supplier code, but without any other supplier info
			// expected result: should fallback to supplier in BO
			supplierInfo.OrganizationCode = "ABCORG";
			var cscOnlyMessage1 = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			cscOnlyMessage1.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with different supplier code, and without any other supplier info
			// expected result: supplier should be found by code
			supplierInfo.OrganizationCode = "XYZORG";
			var cscOnlyMessage2 = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			cscOnlyMessage2.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with supplier code that does not exist in database, and without any other supplier info
			// expected result: no fallback, wrapper should have only supplier code
			supplierInfo.OrganizationCode = "ZZZORG";
			var wrongCscMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			wrongCscMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with full supplier information
			// expected result: information should be taken from message, VAT should be taken from BO
			// note: VAT would not be included in message text, because format does not allow this
			supplierInfo.OrganizationCode = "ABCORG";
			supplierInfo.OrganizationCodeQualifier = "ABC";
			supplierInfo.VATRegistrationNo = "ABC VAT";
			supplierInfo.Name = "ABC NAME";
			supplierInfo.PostCode = "ABC ZIP";
			supplierInfo.City = "ABC CITY";
			supplierInfo.Address = "123, ABC STREET ABC CITY ABC ZIP";
			var allDataMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			allDataMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with supplier information (address only partially filled)
			// expected result: information should be taken from message, VAT should be taken from BO
			// note: since address fields are at least partially filled, fallback for individual address field should not happen
			// note: VAT would not be included in message text, because format does not allow this
			supplierInfo.OrganizationCode = "ABCORG";
			supplierInfo.VATRegistrationNo = "ABC VAT";
			supplierInfo.Name = "ABC NAME";
			supplierInfo.PostCode = "";
			supplierInfo.City = "";
			supplierInfo.Address = "123, ABC STREET ABC CITY ABC ZIP";
			var partialAddressMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			partialAddressMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// Business Object
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.Add(noSupplierMessage);
			entryHeader.Messages.Add(cscOnlyMessage1);
			entryHeader.Messages.Add(cscOnlyMessage2);
			entryHeader.Messages.Add(wrongCscMessage);
			entryHeader.Messages.Add(allDataMessage);
			entryHeader.Messages.Add(partialAddressMessage);
			var supplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplierOrg.PK;
			supplierOrg.OH_FullName = "ABC NAME DB";
			supplierOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "ABCORG", Core.Constants.CountryCodes.SouthAfrica);
			supplierOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "ABC VAT DB", Core.Constants.CountryCodes.SouthAfrica);
			supplierOrg.MainAddress.Postcode = "ABC ZIP DB";
			supplierOrg.MainAddress.City = "ABC CITY DB";
			supplierOrg.MainAddress.OA_Address2 = "123, ABC STREET DB";
			var supplierOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			supplierOrg2.OH_FullName = "XYZ NAME DB";
			supplierOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "XYZORG", Core.Constants.CountryCodes.SouthAfrica);
			supplierOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "XYZ VAT DB", Core.Constants.CountryCodes.SouthAfrica);
			supplierOrg2.MainAddress.Postcode = "XYZ ZIP DB";
			supplierOrg2.MainAddress.City = "XYZ CITY DB";
			supplierOrg2.MainAddress.OA_Address2 = "123, XYZ STREET DB";
			// Assertions
			var entryWrapper = new VOCDocumentPackDocumentWrapper(entryHeader);
			AssertEquals("ABCORG", entryWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", entryWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME DB", entryWrapper.Supplier.Name);
			AssertEquals("ABC ZIP DB", entryWrapper.Supplier.PostCode);
			AssertEquals("ABC CITY DB", entryWrapper.Supplier.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", entryWrapper.Supplier.Address);
			var noSupplierMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(noSupplierMessage);
			AssertEquals("ABCORG", noSupplierMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", noSupplierMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME DB", noSupplierMessageWrapper.Supplier.Name);
			AssertEquals("ABC ZIP DB", noSupplierMessageWrapper.Supplier.PostCode);
			AssertEquals("ABC CITY DB", noSupplierMessageWrapper.Supplier.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", noSupplierMessageWrapper.Supplier.Address);
			var cscOnlyMessage1Wrapper = VOCDocumentPackDocumentWrapper.NewForMessage(cscOnlyMessage1);
			AssertEquals("ABCORG", cscOnlyMessage1Wrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", cscOnlyMessage1Wrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME DB", cscOnlyMessage1Wrapper.Supplier.Name);
			AssertEquals("ABC ZIP DB", cscOnlyMessage1Wrapper.Supplier.PostCode);
			AssertEquals("ABC CITY DB", cscOnlyMessage1Wrapper.Supplier.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", cscOnlyMessage1Wrapper.Supplier.Address);
			var cscOnlyMessage2Wrapper = VOCDocumentPackDocumentWrapper.NewForMessage(cscOnlyMessage2);
			AssertEquals("XYZORG", cscOnlyMessage2Wrapper.Supplier.OrganizationCode);
			AssertEquals("XYZ VAT DB", cscOnlyMessage2Wrapper.Supplier.VATRegistrationNo);
			AssertEquals("XYZ NAME DB", cscOnlyMessage2Wrapper.Supplier.Name);
			AssertEquals("XYZ ZIP DB", cscOnlyMessage2Wrapper.Supplier.PostCode);
			AssertEquals("XYZ CITY DB", cscOnlyMessage2Wrapper.Supplier.City);
			AssertEquals("#1 123, XYZ STREET DB XYZ CITY DB XYZ ZIP DB", cscOnlyMessage2Wrapper.Supplier.Address);
			var wrongCscMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(wrongCscMessage);
			AssertEquals("ZZZORG", wrongCscMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("", wrongCscMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("", wrongCscMessageWrapper.Supplier.Name);
			AssertEquals("", wrongCscMessageWrapper.Supplier.PostCode);
			AssertEquals("", wrongCscMessageWrapper.Supplier.City);
			AssertEquals("", wrongCscMessageWrapper.Supplier.Address);
			var allDataMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(allDataMessage);
			AssertEquals("ABCORG", allDataMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", allDataMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME", allDataMessageWrapper.Supplier.Name);
			AssertEquals("ABC ZIP", allDataMessageWrapper.Supplier.PostCode);
			AssertEquals("ABC CITY", allDataMessageWrapper.Supplier.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", allDataMessageWrapper.Supplier.Address);
			var partialAddressMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(partialAddressMessage);
			AssertEquals("ABCORG", partialAddressMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", partialAddressMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME", partialAddressMessageWrapper.Supplier.Name);
			AssertEquals("", partialAddressMessageWrapper.Supplier.PostCode);
			AssertEquals("", partialAddressMessageWrapper.Supplier.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", partialAddressMessageWrapper.Supplier.Address);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			entryWrapper = new VOCDocumentPackDocumentWrapper(entryHeader);
			AssertEquals("Supplier should be null for export", null, entryWrapper.Supplier);
			foreach (var message in entryHeader.Messages.Cast<CUSDECEDIMessage>())
			{
				var documentWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(message);
				AssertEquals("Supplier should be null for export", null, documentWrapper.Supplier);
			}
		}

		public void TestImporterFallback()
		{
			AddressInformationForTest importerInfo = new AddressInformationForTest();
			CUSDECMessageDataProviderForTest messageDataProvider = new CUSDECMessageDataProviderForTest();
			messageDataProvider.Importer = importerInfo;
			var textBuilder = new CUSDECMessageTextBuilderForTest(messageDataProvider);
			// message without any importer information
			// expected result: should fallback to importer in BO
			var noImporterMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			noImporterMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with importer code, but without any other importer info
			// expected result: should fallback to importer in BO
			importerInfo.OrganizationCode = "ABCORG";
			var ccdOnlyMessage1 = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			ccdOnlyMessage1.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with different importer code, and without any other importer info
			// expected result: importer should be found by code
			importerInfo.OrganizationCode = "XYZORG";
			var ccdOnlyMessage2 = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			ccdOnlyMessage2.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with importer code that does not exist in database, and without any other importer info
			// expected result: no fallback, wrapper should have only importer code
			importerInfo.OrganizationCode = "ZZZORG";
			var wrongCcdMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			wrongCcdMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with full importer information
			// expected result: information should be taken from message, VAT should be taken from BO
			importerInfo.OrganizationCode = "ABCORG";
			importerInfo.OrganizationCodeQualifier = "ABC";
			importerInfo.VATRegistrationNo = "ABC VAT";
			importerInfo.Name = "ABC NAME";
			importerInfo.PostCode = "ABC ZIP";
			importerInfo.City = "ABC CITY";
			importerInfo.Address = "123, ABC STREET ABC CITY ABC ZIP";
			var allDataMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			allDataMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// message with importer information (address only partially filled)
			// expected result: information should be taken from message, VAT should be taken from BO
			// note: since address fields are at least partially filled, fallback for individual address field should not happen
			importerInfo.OrganizationCode = "ABCORG";
			importerInfo.VATRegistrationNo = "ABC VAT";
			importerInfo.Name = "ABC NAME";
			importerInfo.PostCode = "";
			importerInfo.City = "";
			importerInfo.Address = "123, ABC STREET ABC CITY ABC ZIP";
			var partialAddressMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			partialAddressMessage.EM_MessageText = textBuilder.GenerateMessageBody();
			// Business Object
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.Messages.Add(noImporterMessage);
			entryHeader.Messages.Add(ccdOnlyMessage1);
			entryHeader.Messages.Add(ccdOnlyMessage2);
			entryHeader.Messages.Add(wrongCcdMessage);
			entryHeader.Messages.Add(allDataMessage);
			entryHeader.Messages.Add(partialAddressMessage);
			var importerOrg = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importerOrg.PK;
			importerOrg.OH_FullName = "ABC NAME DB";
			importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "ABCORG", Core.Constants.CountryCodes.SouthAfrica);
			importerOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "ABC VAT DB", Core.Constants.CountryCodes.SouthAfrica);
			importerOrg.MainAddress.Postcode = "ABC ZIP DB";
			importerOrg.MainAddress.City = "ABC CITY DB";
			importerOrg.MainAddress.OA_Address2 = "123, ABC STREET DB";
			var importerOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			importerOrg2.OH_FullName = "XYZ NAME DB";
			importerOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "XYZORG", Core.Constants.CountryCodes.SouthAfrica);
			importerOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "XYZ VAT DB", Core.Constants.CountryCodes.SouthAfrica);
			importerOrg2.MainAddress.Postcode = "XYZ ZIP DB";
			importerOrg2.MainAddress.City = "XYZ CITY DB";
			importerOrg2.MainAddress.OA_Address2 = "123, XYZ STREET DB";
			// Assertions
			var entryWrapper = new VOCDocumentPackDocumentWrapper(entryHeader);
			AssertEquals("ABCORG", entryWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT DB", entryWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME DB", entryWrapper.Importer.Name);
			AssertEquals("ABC ZIP DB", entryWrapper.Importer.PostCode);
			AssertEquals("ABC CITY DB", entryWrapper.Importer.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", entryWrapper.Importer.Address);
			var noImporterMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(noImporterMessage);
			AssertEquals("ABCORG", noImporterMessageWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT DB", noImporterMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME DB", noImporterMessageWrapper.Importer.Name);
			AssertEquals("ABC ZIP DB", noImporterMessageWrapper.Importer.PostCode);
			AssertEquals("ABC CITY DB", noImporterMessageWrapper.Importer.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", noImporterMessageWrapper.Importer.Address);
			var ccdOnlyMessage1Wrapper = VOCDocumentPackDocumentWrapper.NewForMessage(ccdOnlyMessage1);
			AssertEquals("ABCORG", ccdOnlyMessage1Wrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT DB", ccdOnlyMessage1Wrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME DB", ccdOnlyMessage1Wrapper.Importer.Name);
			AssertEquals("ABC ZIP DB", ccdOnlyMessage1Wrapper.Importer.PostCode);
			AssertEquals("ABC CITY DB", ccdOnlyMessage1Wrapper.Importer.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", ccdOnlyMessage1Wrapper.Importer.Address);
			var ccdOnlyMessage2Wrapper = VOCDocumentPackDocumentWrapper.NewForMessage(ccdOnlyMessage2);
			AssertEquals("XYZORG", ccdOnlyMessage2Wrapper.Importer.OrganizationCode);
			AssertEquals("XYZ VAT DB", ccdOnlyMessage2Wrapper.Importer.VATRegistrationNo);
			AssertEquals("XYZ NAME DB", ccdOnlyMessage2Wrapper.Importer.Name);
			AssertEquals("XYZ ZIP DB", ccdOnlyMessage2Wrapper.Importer.PostCode);
			AssertEquals("XYZ CITY DB", ccdOnlyMessage2Wrapper.Importer.City);
			AssertEquals("#1 123, XYZ STREET DB XYZ CITY DB XYZ ZIP DB", ccdOnlyMessage2Wrapper.Importer.Address);
			var wrongCcdMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(wrongCcdMessage);
			AssertEquals("ZZZORG", wrongCcdMessageWrapper.Importer.OrganizationCode);
			AssertEquals("", wrongCcdMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("", wrongCcdMessageWrapper.Importer.Name);
			AssertEquals("", wrongCcdMessageWrapper.Importer.PostCode);
			AssertEquals("", wrongCcdMessageWrapper.Importer.City);
			AssertEquals("", wrongCcdMessageWrapper.Importer.Address);
			var allDataMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(allDataMessage);
			AssertEquals("ABCORG", allDataMessageWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT", allDataMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME", allDataMessageWrapper.Importer.Name);
			AssertEquals("ABC ZIP", allDataMessageWrapper.Importer.PostCode);
			AssertEquals("ABC CITY", allDataMessageWrapper.Importer.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", allDataMessageWrapper.Importer.Address);
			var partialAddressMessageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(partialAddressMessage);
			AssertEquals("ABCORG", partialAddressMessageWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT", partialAddressMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME", partialAddressMessageWrapper.Importer.Name);
			AssertEquals("", partialAddressMessageWrapper.Importer.PostCode);
			AssertEquals("", partialAddressMessageWrapper.Importer.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", partialAddressMessageWrapper.Importer.Address);
		}

		public void TestDeclarantUser()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BRK";
			broker.GS_FullName = "Broker";
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "SND";
			user.GS_FullName = "MSGSender";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryWrapper = new VOCDocumentPackDocumentWrapper(entryHeader);
			CUSDECMessageDataProviderForTest messageDataProvider = new CUSDECMessageDataProviderForTest();
			var textBuilder = new CUSDECMessageTextBuilderForTest(messageDataProvider);
			var message = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			message.EM_MessageText = textBuilder.GenerateMessageBody();
			message.EM_SystemCreateUser = user.GS_Code;
			entryHeader.Messages.Add(message);
			var messageWrapper = VOCDocumentPackDocumentWrapper.NewForMessage(message);
			AssertEquals("DeclarantUser", GlbStaff.CurrentUser.GS_FullName, entryWrapper.DeclarantUser.GS_FullName);
			AssertEquals("DeclarantUser", "MSGSender", messageWrapper.DeclarantUser.GS_FullName);
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("DeclarantUser", "Broker", entryWrapper.DeclarantUser.GS_FullName);
			AssertEquals("DeclarantUser", "Broker", messageWrapper.DeclarantUser.GS_FullName);
		}

		public void TestEndorsementsAppend()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testDeclaration.JE_TransportMode = "SEA";
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "11";
			var testHeader1 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader1.CH_CEI_Instruction = testInstruction1.PK;
			testHeader1.CH_BGMReference = "LRN001";
			var testHeader2 = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader2.CH_CEI_Instruction = testInstruction2.PK;
			testHeader2.CH_BGMReference = "LRN002";
			testDeclaration.JE_TotalNoOfPacks = 1;
			Factory.Save();
			new LineMerger(testDeclaration).DoMerge();
			var tester = new VOCDocumentPackDocumentWrapper(testHeader1);
			AssertEquals("Part 1 of 2 - Part of 1 Package", tester.EndorsementsAppend);
			testDeclaration.JE_TotalNoOfPacks = 2;
			testHeader1.CH_Packages = 1;
			testHeader2.CH_Packages = 1;
			Factory.Save();
			tester = new VOCDocumentPackDocumentWrapper(testHeader1);
			AssertEquals("Part 1 of 2 - 1 Packages of 2", tester.EndorsementsAppend);
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
		}

		OrgHeader SetupOrgheader(ZString orgname, ZString cusCodeType, ZString customsRegNo, bool isWarehouse = true)
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = orgname;
			testOrg.MainAddress.Address1 = "addr1";
			testOrg.MainAddress.Address2 = "addr2";
			testOrg.MainAddress.Postcode = "POC";
			var cusCode = testOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = cusCodeType;
			cusCode.OK_CustomsRegNo = customsRegNo;
			cusCode.OK_OA_PremisesAddress = isWarehouse ? testOrg.MainAddress.PK : ZGuid.Empty;
			Factory.Save();
			return testOrg;
		}

		CusEntryHeader SetupEntryHeader(OrgHeader testOrg, ZString procedureCode, ZString previousProcedureCode)
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = procedureCode;
			testInst.CEI_OA_Warehouse = testOrg.MainAddress.PK;
			testInst.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
			testInst.CEI_OH_Carrier = testOrg.PK;
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + previousProcedureCode;
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = testInst.PK;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.InvoiceLines.Add(invLine);
			return testHeader;
		}

		ZAMessage SetupCUSDECMessage(CusEntryHeader entry)
		{
			string cUSDEC_BGM5_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+5'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+18+00010008::ZZZ'LOC+122+00010008::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'NAD+AF+00020008'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			var testMessage = GetOutGoingMessage("CHG", cUSDEC_BGM5_Message);
			entry.Messages.Add(testMessage);
			Factory.Save();
			return testMessage;
		}

		ZAMessage GetOutGoingMessage(ZString subType, ZString text)
		{
			var testMessage = Factory.New<ZAMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = subType;
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = text;
			testMessage.MessageNumForTesting = ZDateTime.Now.Ticks.ToString();
			return testMessage;
		}

		ZAMessage GetInComingMessage(ZString text)
		{
			var testMessage = Factory.New<ZAMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "RES";
			testMessage.EM_MessageSubType = "XXX";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = text;
			testMessage.MessageNumForTesting = ZDateTime.Now.Ticks.ToString();
			return testMessage;
		}
	}
}
