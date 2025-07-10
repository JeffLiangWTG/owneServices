using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class SADDocumentPackDocumentWrapperTest : TestCaseWithFactory
	{
		public void TestTransportDocumentNumberInBusinessLogic()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			dec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			dec.JE_MasterBill = "MSCU123450";
			dec.JE_CarrierCode = "MSC";
			var testHeader = dec.CustomsEntryHeaders.AddNew();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
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

		public void TestCalcDutiesAndFees_NoException()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals("Precondition: entry has no lines.", 0, testHeader.MergedLines.Count);
			AssertNoExceptionThrown(() =>
			{
				_ = tester.FirstLineDetail.CalcDutiesAndFees;
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
							var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
							AssertEquals(declarationTypePair.Code, "RCD", wrapper.EntryDocType);
							AssertEquals(declarationTypePair.Code, "RCD", ((MessageSendingObject)(wrapper.CUSDECSource)).MessageKeyFactor.DeclarationType);
						}

						break;
					case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
					case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
					case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
						{
							var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
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
				AssertEquals(declarationTypePair.Code, declarationTypePair.Code, SADDocumentPackDocumentWrapper.NewForMessage(cusdecMessage).EntryDocType);
			}

			cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "");
			AssertEquals("RCD", SADDocumentPackDocumentWrapper.NewForMessage(cusdecMessage).EntryDocType);
		}

		public void TestGettingFromEntryHeader()
		{
			string cUSDEC_BGM5_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+5'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			string cUSRES_GIS6_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655BBR20160513000034::00001'DTM+9:20160513143147:202'TDT+20'LOC+22+BBR'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+BH:00505655'RFF+AAS:HENRYMASTER1'RFF+ACD:<<INTERCHANGENUMBERPLACEHOLDER>>'ERP+1:0000'ERC+0000'FTX+AAO+++Line number may not be 0'UNT+15+1'";
			string cUSRES_GIS1_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655CLP20160514000245:0'DTM+178:20160511:102'DTM+202:20160511:102'TDT+20+QF987+4+++++::: 'LOC+22+CLP::ZZZ'LOC+14+XW::ZZZ'GIS+1:120:ZZZ:Y'NAD+AG+00505655'RFF+BH:0003264'RFF+AAS:081-99876545'DTM+137:20160305:102'RFF+ABT:CLP201605145000001'DTM+137:20160514:102'RFF+UCN:6ZA01702826INV158'RFF+ACD:<<INTERCHANGENUMBERPLACEHOLDER>>'TAX+3+CUS:107:ZZZ'MOA+161:490688'CNT+7:226.79'CNT+11:5'UNT+21+1'";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals("9", tester.MessageType);
			var testMessage = Factory.New<CUSDECEDIMessageForTest>();
			testMessage.MessageNumForTesting = "TEST1";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = "ORG";
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = CUSDEC_BGM9_Message;
			testHeader.Messages.Add(testMessage);
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals("9", tester.MessageType);
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = "ORG";
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "SNT";
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			AssertEquals("9", tester.MessageType);
			testMessage.EM_MessageText = cUSDEC_BGM5_Message;
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			AssertEquals("9", tester.MessageType);
			testHeader.MovementReferenceNumberSetter("123", ZDateTime.Now);
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = "ORG";
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "SNT";
			testMessage.EM_MessageText = CUSDEC_BGM9_Message;
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			testMessage.EM_MessageText = cUSDEC_BGM5_Message;
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			var testIncomingMessage = Factory.New<CUSDECEDIMessageForTest>();
			testIncomingMessage.MessageNumForTesting = "TEST2";
			testIncomingMessage.EM_ApplicationCode = "ZAC";
			testIncomingMessage.EM_MessageType = "RES";
			testIncomingMessage.EM_MessageSubType = "XXX";
			testIncomingMessage.EM_ReceiveTransmit = "RCV";
			testIncomingMessage.EM_Status = "PRS";
			testIncomingMessage.EM_MessageText = cUSRES_GIS6_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", testMessage.EM_MessageNum);
			testHeader.Messages.Add(testIncomingMessage);
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			testIncomingMessage = Factory.New<CUSDECEDIMessageForTest>();
			testIncomingMessage.MessageNumForTesting = "TEST3";
			testIncomingMessage.EM_ApplicationCode = "ZAC";
			testIncomingMessage.EM_MessageType = "RES";
			testIncomingMessage.EM_MessageSubType = "XXX";
			testIncomingMessage.EM_ReceiveTransmit = "RCV";
			testIncomingMessage.EM_Status = "PRS";
			testIncomingMessage.EM_MessageText = cUSRES_GIS1_Message.Replace("<<INTERCHANGENUMBERPLACEHOLDER>>", testMessage.EM_MessageNum);
			testHeader.Messages.Add(testIncomingMessage);
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(new BusinessObjectFactory().Load<CusEntryHeader>(testHeader.PK));
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			AssertEquals(false, tester.CUSDECSource is CUSDECMessageHelper);
		}

		public void TestGettingFromMessage()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_FullName = "MSGSender";
			var testMessage = Factory.New<CUSDECEDIMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = "ORG";
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "SNT";
			testMessage.EM_MessageText = CUSDEC_BGM9_Message;
			testMessage.EM_SystemCreateUser = user.GS_Code;
			testHeader.Messages.Add(testMessage);
			Factory.Save();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals(true, tester.CUSDECSource is MessageSendingObject);
			AssertEquals(false, tester.CUSDECSource is CUSDECMessageHelper);
			tester = SADDocumentPackDocumentWrapper.NewForMessage(testMessage);
			AssertEquals(false, tester.CUSDECSource is MessageSendingObject);
			AssertEquals(true, tester.CUSDECSource is CUSDECMessageHelper);
			AssertEquals("MSGSender", tester.DeclarantUser.GS_FullName);
			tester = SADDocumentPackDocumentWrapper.NewForMessage(new BusinessObjectFactory().NewWithValidTestData<CUSDECEDIMessage>());
			AssertNull(tester);
			testMessage = Factory.New<CUSDECEDIMessageForTest>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "DEC";
			testMessage.EM_MessageSubType = "ORG";
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_Status = "SNT";
			testMessage.EM_MessageText = "INVALID CONTENT WONT GENERATE WRAPPER";
			testHeader.Messages.Add(testMessage);
			Factory.Save();
			tester = SADDocumentPackDocumentWrapper.NewForMessage(testMessage);
			AssertNull(tester);
		}

		public void TestGettingInvoiceInformationForExport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "63";
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2016, 01, 18);
			invoiceHeader.JZ_InvoiceNumber = "INVH1";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			string cusdec_OneInvoice = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+830+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			string cusdec_NoInvoice = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+830+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			var testMessage_OneInvoice = Factory.New<CUSDECEDIMessageForTest>();
			testMessage_OneInvoice.EM_ApplicationCode = "ZAC";
			testMessage_OneInvoice.EM_MessageType = "DEC";
			testMessage_OneInvoice.EM_MessageSubType = "ORG";
			testMessage_OneInvoice.EM_ReceiveTransmit = "TRX";
			testMessage_OneInvoice.EM_Status = "SNT";
			testMessage_OneInvoice.EM_MessageText = cusdec_OneInvoice;
			entryHeader.Messages.Add(testMessage_OneInvoice);
			var testMessage_NoInvoice = Factory.New<CUSDECEDIMessageForTest>();
			testMessage_NoInvoice.EM_ApplicationCode = "ZAC";
			testMessage_NoInvoice.EM_MessageType = "DEC";
			testMessage_NoInvoice.EM_MessageSubType = "ORG";
			testMessage_NoInvoice.EM_ReceiveTransmit = "TRX";
			testMessage_NoInvoice.EM_Status = "SNT";
			testMessage_NoInvoice.EM_MessageText = cusdec_NoInvoice;
			entryHeader.Messages.Add(testMessage_NoInvoice);
			Factory.Save();
			CombineAssertions(() =>
			{
				var wrapperFromHeader = new SADDocumentPackDocumentWrapper(entryHeader);
				var wrapperFromMessage_OneInvoice = SADDocumentPackDocumentWrapper.NewForMessage(testMessage_OneInvoice);
				var wrapperFromMessage_NoInvoice = SADDocumentPackDocumentWrapper.NewForMessage(testMessage_NoInvoice);
				AssertEquals("INVH1", wrapperFromHeader.FirstInvoice.InvoiceNumber);
				AssertEquals("INVH1", wrapperFromMessage_OneInvoice.FirstInvoice.InvoiceNumber);
				AssertEquals("", wrapperFromMessage_NoInvoice.FirstInvoice.InvoiceNumber);
				AssertEquals(new ZDateTime(2016, 01, 18), wrapperFromMessage_OneInvoice.FirstInvoice.InvoiceDate);
			});
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
			var entryWrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertEquals("ABCORG", entryWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", entryWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME DB", entryWrapper.Supplier.Name);
			AssertEquals("ABC ZIP DB", entryWrapper.Supplier.PostCode);
			AssertEquals("ABC CITY DB", entryWrapper.Supplier.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", entryWrapper.Supplier.Address);
			var noSupplierMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(noSupplierMessage);
			AssertEquals("ABCORG", noSupplierMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", noSupplierMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME DB", noSupplierMessageWrapper.Supplier.Name);
			AssertEquals("ABC ZIP DB", noSupplierMessageWrapper.Supplier.PostCode);
			AssertEquals("ABC CITY DB", noSupplierMessageWrapper.Supplier.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", noSupplierMessageWrapper.Supplier.Address);
			var cscOnlyMessage1Wrapper = SADDocumentPackDocumentWrapper.NewForMessage(cscOnlyMessage1);
			AssertEquals("ABCORG", cscOnlyMessage1Wrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", cscOnlyMessage1Wrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME DB", cscOnlyMessage1Wrapper.Supplier.Name);
			AssertEquals("ABC ZIP DB", cscOnlyMessage1Wrapper.Supplier.PostCode);
			AssertEquals("ABC CITY DB", cscOnlyMessage1Wrapper.Supplier.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", cscOnlyMessage1Wrapper.Supplier.Address);
			var cscOnlyMessage2Wrapper = SADDocumentPackDocumentWrapper.NewForMessage(cscOnlyMessage2);
			AssertEquals("XYZORG", cscOnlyMessage2Wrapper.Supplier.OrganizationCode);
			AssertEquals("XYZ VAT DB", cscOnlyMessage2Wrapper.Supplier.VATRegistrationNo);
			AssertEquals("XYZ NAME DB", cscOnlyMessage2Wrapper.Supplier.Name);
			AssertEquals("XYZ ZIP DB", cscOnlyMessage2Wrapper.Supplier.PostCode);
			AssertEquals("XYZ CITY DB", cscOnlyMessage2Wrapper.Supplier.City);
			AssertEquals("#1 123, XYZ STREET DB XYZ CITY DB XYZ ZIP DB", cscOnlyMessage2Wrapper.Supplier.Address);
			var wrongCscMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(wrongCscMessage);
			AssertEquals("ZZZORG", wrongCscMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("", wrongCscMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("", wrongCscMessageWrapper.Supplier.Name);
			AssertEquals("", wrongCscMessageWrapper.Supplier.PostCode);
			AssertEquals("", wrongCscMessageWrapper.Supplier.City);
			AssertEquals("", wrongCscMessageWrapper.Supplier.Address);
			var allDataMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(allDataMessage);
			AssertEquals("ABCORG", allDataMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", allDataMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME", allDataMessageWrapper.Supplier.Name);
			AssertEquals("ABC ZIP", allDataMessageWrapper.Supplier.PostCode);
			AssertEquals("ABC CITY", allDataMessageWrapper.Supplier.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", allDataMessageWrapper.Supplier.Address);
			var partialAddressMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(partialAddressMessage);
			AssertEquals("ABCORG", partialAddressMessageWrapper.Supplier.OrganizationCode);
			AssertEquals("ABC VAT DB", partialAddressMessageWrapper.Supplier.VATRegistrationNo);
			AssertEquals("ABC NAME", partialAddressMessageWrapper.Supplier.Name);
			AssertEquals("", partialAddressMessageWrapper.Supplier.PostCode);
			AssertEquals("", partialAddressMessageWrapper.Supplier.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", partialAddressMessageWrapper.Supplier.Address);
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
			var entryWrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertEquals("ABCORG", entryWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT DB", entryWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME DB", entryWrapper.Importer.Name);
			AssertEquals("ABC ZIP DB", entryWrapper.Importer.PostCode);
			AssertEquals("ABC CITY DB", entryWrapper.Importer.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", entryWrapper.Importer.Address);
			var noImporterMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(noImporterMessage);
			AssertEquals("ABCORG", noImporterMessageWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT DB", noImporterMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME DB", noImporterMessageWrapper.Importer.Name);
			AssertEquals("ABC ZIP DB", noImporterMessageWrapper.Importer.PostCode);
			AssertEquals("ABC CITY DB", noImporterMessageWrapper.Importer.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", noImporterMessageWrapper.Importer.Address);
			var ccdOnlyMessage1Wrapper = SADDocumentPackDocumentWrapper.NewForMessage(ccdOnlyMessage1);
			AssertEquals("ABCORG", ccdOnlyMessage1Wrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT DB", ccdOnlyMessage1Wrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME DB", ccdOnlyMessage1Wrapper.Importer.Name);
			AssertEquals("ABC ZIP DB", ccdOnlyMessage1Wrapper.Importer.PostCode);
			AssertEquals("ABC CITY DB", ccdOnlyMessage1Wrapper.Importer.City);
			AssertEquals("#1 123, ABC STREET DB ABC CITY DB ABC ZIP DB", ccdOnlyMessage1Wrapper.Importer.Address);
			var ccdOnlyMessage2Wrapper = SADDocumentPackDocumentWrapper.NewForMessage(ccdOnlyMessage2);
			AssertEquals("XYZORG", ccdOnlyMessage2Wrapper.Importer.OrganizationCode);
			AssertEquals("XYZ VAT DB", ccdOnlyMessage2Wrapper.Importer.VATRegistrationNo);
			AssertEquals("XYZ NAME DB", ccdOnlyMessage2Wrapper.Importer.Name);
			AssertEquals("XYZ ZIP DB", ccdOnlyMessage2Wrapper.Importer.PostCode);
			AssertEquals("XYZ CITY DB", ccdOnlyMessage2Wrapper.Importer.City);
			AssertEquals("#1 123, XYZ STREET DB XYZ CITY DB XYZ ZIP DB", ccdOnlyMessage2Wrapper.Importer.Address);
			var wrongCcdMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(wrongCcdMessage);
			AssertEquals("ZZZORG", wrongCcdMessageWrapper.Importer.OrganizationCode);
			AssertEquals("", wrongCcdMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("", wrongCcdMessageWrapper.Importer.Name);
			AssertEquals("", wrongCcdMessageWrapper.Importer.PostCode);
			AssertEquals("", wrongCcdMessageWrapper.Importer.City);
			AssertEquals("", wrongCcdMessageWrapper.Importer.Address);
			var allDataMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(allDataMessage);
			AssertEquals("ABCORG", allDataMessageWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT", allDataMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME", allDataMessageWrapper.Importer.Name);
			AssertEquals("ABC ZIP", allDataMessageWrapper.Importer.PostCode);
			AssertEquals("ABC CITY", allDataMessageWrapper.Importer.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", allDataMessageWrapper.Importer.Address);
			var partialAddressMessageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(partialAddressMessage);
			AssertEquals("ABCORG", partialAddressMessageWrapper.Importer.OrganizationCode);
			AssertEquals("ABC VAT", partialAddressMessageWrapper.Importer.VATRegistrationNo);
			AssertEquals("ABC NAME", partialAddressMessageWrapper.Importer.Name);
			AssertEquals("", partialAddressMessageWrapper.Importer.PostCode);
			AssertEquals("", partialAddressMessageWrapper.Importer.City);
			AssertEquals("123, ABC STREET ABC CITY ABC ZIP", partialAddressMessageWrapper.Importer.Address);
		}

		public void Test501Page()
		{
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				testHeader.MergedLines.AddNew();
				var tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD501DocumentPages.Count);
				testHeader.MergedLines.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD501DocumentPages.Count);
				testHeader.MergedLines.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD501DocumentPages.Count);
				testHeader.MergedLines.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD501DocumentPages.Count);
				testHeader.MergedLines.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(2, tester.SAD501DocumentPages.Count);
			});
		}

		public void Test501PageRunningTotalDutiesAndFees()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine.JI_ZZF_NKTaxType = "VAT";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				var testLine = testHeader.MergedLines.AddNew();
				testLine.Fees.AddOrUpdate("VAT", 11m);
				testLine.InvoiceLines.Add(testInvoiceLine);
				var tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD501DocumentPages.Count);
				testLine = testHeader.MergedLines.AddNew();
				testLine.InvoiceLines.Add(testInvoiceLine);
				testLine.Fees.AddOrUpdate("VAT", 12m);
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD501DocumentPages.Count);
				AssertEquals(1, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage.Count);
				AssertEquals("VAT", tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Code);
				AssertEquals(23m, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Value);
				testLine = testHeader.MergedLines.AddNew();
				testLine.InvoiceLines.Add(testInvoiceLine);
				testLine.Fees.AddOrUpdate("VAT", 13.1m);
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD501DocumentPages.Count);
				AssertEquals(1, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage.Count);
				AssertEquals("VAT", tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Code);
				AssertEquals(36.1m, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Value);
				testLine = testHeader.MergedLines.AddNew();
				testLine.Fees.AddOrUpdate("1P1", 21m);
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD501DocumentPages.Count);
				AssertEquals(2, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage.Count);
				AssertEquals("VAT", tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Code);
				AssertEquals(36.1m, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Value);
				AssertEquals("1P1", tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[1].Code);
				AssertEquals(21m, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[1].Value);
				testLine = testHeader.MergedLines.AddNew();
				testLine.Fees.AddOrUpdate("1P1", 0.01m);
				testLine.ProvisionalPayments.AddNew("PPA", 1.01m);
				testLine.ProvisionalPayments.AddNew("PEN", 2.01m);
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(2, tester.SAD501DocumentPages.Count);
				AssertEquals(2, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage.Count);
				AssertEquals("VAT", tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Code);
				AssertEquals(36.1m, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[0].Value);
				AssertEquals("1P1", tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[1].Code);
				AssertEquals(21m, tester.SAD501DocumentPages[0].RunningTotalDutiesAndFeesOfThisPage[1].Value);
				AssertEquals(3, tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage.Count);
				AssertEquals("VAT", tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage[0].Code);
				AssertEquals(36.1m, tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage[0].Value);
				AssertEquals("1P1", tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage[1].Code);
				AssertEquals(21.01m, tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage[1].Value);
				AssertEquals("PP's", tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage[2].Code);
				AssertEquals(3.02m, tester.SAD501DocumentPages[1].RunningTotalDutiesAndFeesOfThisPage[2].Value);
			});
		}

		public void Test507Page()
		{
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_TransportMode = "SEA";
				var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
				var tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD507DocumentPages.Count);
				testDeclaration.CusContainers.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD507DocumentPages.Count);
				testHeader.Endorsements = "testEndorsement";
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD507DocumentPages.Count);
				testHeader.Endorsements = ZString.Empty;
				for (int i = 2; i <= 8; i++)
				{
					testDeclaration.CusContainers.AddNew();
				}

				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD507DocumentPages.Count);
				testDeclaration.CusContainers.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD507DocumentPages.Count);
				for (int i = 10; i <= 86; i++)
				{
					testDeclaration.CusContainers.AddNew();
				}

				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD507DocumentPages.Count);
				testDeclaration.CusContainers.AddNew();
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(2, tester.SAD507DocumentPages.Count);
				testDeclaration.CusContainers.RemoveAndDeleteAll();
				var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
				testHelper.CreateAdditionalInformationCusCodeEntry("BHR");
				var entryLine = testHeader.MergedLines.AddNew();
				entryLine.AdditionalInformationCodes.AddNew("BHR", "1");
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD507DocumentPages.Count);
				for (int i = 2; i <= 3; i++)
				{
					entryLine.AdditionalInformationCodes.AddNew("BHR", i.ToString());
				}

				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(0, tester.SAD507DocumentPages.Count);
				entryLine.AdditionalInformationCodes.AddNew("BHR", "4");
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD507DocumentPages.Count);
				for (int i = 5; i <= 53; i++)
				{
					entryLine.AdditionalInformationCodes.AddNew("BHR", i.ToString());
				}

				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(1, tester.SAD507DocumentPages.Count);
				entryLine.AdditionalInformationCodes.AddNew("BHR", "51");
				tester = new SADDocumentPackDocumentWrapper(testHeader);
				AssertEquals(2, tester.SAD507DocumentPages.Count);
			});
		}

		public void Test507Page_Housebill()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = "SEA";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals(0, tester.SAD507DocumentPages.Count);

			testDeclaration.JE_HouseBill = "1234";
			tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals(1, tester.SAD507DocumentPages.Count);
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
			var tester = new SADDocumentPackDocumentWrapper(testHeader1);
			AssertEquals(2, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
			tester = new SADDocumentPackDocumentWrapper(testHeader2);
			AssertEquals(1, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
			tester = new SADDocumentPackDocumentWrapper(testHeader3);
			AssertEquals(0, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
			tester = new SADDocumentPackDocumentWrapper(testHeader4);
			AssertEquals(3, tester.CurrentPartIndex);
			AssertEquals(3, tester.PartClearanceQuantity);
		}

		public void TestTotalNoOfPacksInLongHand()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = "SEA";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_Packages = 568;
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals("FIVE SIX EIGHT", tester.TotalNoOfPacksInLongHand);
		}

		public void TestMarksAndNumbers()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_TransportMode = "SEA";
			testDeclaration.JE_MarksAndNumbers = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			var testLine = testHeader.MergedLines.AddNew();
			var testInvLine = testDeclaration.InvoiceLines.AddNew();
			testInvLine.JI_Procedure = "0020";
			testLine.InvoiceLines.Add(testInvLine);
			tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890", tester.MarksAndNumbers);
		}

		public void TestSupplierEvenNoCCDCode()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_FullName = "TEST ORG";
			testDeclaration.JE_OH_Supplier = testOrg.PK;
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var tester = new SADDocumentPackDocumentWrapper(testHeader);
			AssertEquals("TEST ORG", tester.Supplier.Name);
			AssertEquals("", tester.Supplier.OrganizationCode);
		}

		public void TestMultipleSuppliersWithVDNCode()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ValueDeterminationNumber);
			Factory.Save();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "TEST ORG";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_FullName = "TEST ORG2";
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "DE74125");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = orgHeader.PK;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var header = declaration.Invoices.AddNew();
			header.JZ_OH_Supplier = orgHeader.PK;
			var line = header.InvoiceLines.AddNew();
			line.JI_CEI = instruction1.PK;
			var header2 = declaration.Invoices.AddNew();
			header2.JZ_OH_Supplier = orgHeader2.PK;
			header2.JZ_VDN = "1234567";
			var line2 = header2.InvoiceLines.AddNew();
			line2.JI_CEI = instruction1.PK;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(2, declaration.CustomsEntryHeaders.Count());
			var entryHeaderHasLineWithVDNCode = declaration.CustomsEntryHeaders.Cast<CusEntryHeader>().Where(c => c.MergedLines.Cast<CusEntryLine>().Any(e => e.AdditionalInformationCodes.Cast<AdditionalInformation>().Any(a => a.CY_Code == UniversalReferenceConstants.AdditionalInformation.ValueDeterminationNumber)));
			AssertEquals(1, entryHeaderHasLineWithVDNCode.Count());
			var entryHeader = entryHeaderHasLineWithVDNCode.First();
			var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertEquals("TEST ORG", wrapper.Supplier.Name);
			AssertEquals("DE74125", wrapper.Supplier.OrganizationCode);
			var entryHeader2 = declaration.CustomsEntryHeaders.First(c => c.PK != entryHeader.PK);
			wrapper = new SADDocumentPackDocumentWrapper(entryHeader2);
			AssertEquals("TEST ORG", wrapper.Supplier.Name);
			AssertEquals("", wrapper.Supplier.OrganizationCode);
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
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryWrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			CUSDECMessageDataProviderForTest messageDataProvider = new CUSDECMessageDataProviderForTest();
			var textBuilder = new CUSDECMessageTextBuilderForTest(messageDataProvider);
			var message = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			message.EM_MessageText = textBuilder.GenerateMessageBody();
			message.EM_SystemCreateUser = user.GS_Code;
			entryHeader.Messages.Add(message);
			var messageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(message);
			AssertEquals("DeclarantUser", GlbStaff.CurrentUser.GS_FullName, entryWrapper.DeclarantUser.GS_FullName);
			AssertEquals("DeclarantUser", "MSGSender", messageWrapper.DeclarantUser.GS_FullName);
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("DeclarantUser", "Broker", entryWrapper.DeclarantUser.GS_FullName);
			AssertEquals("DeclarantUser", "Broker", messageWrapper.DeclarantUser.GS_FullName);
		}

		public void TestMarksAndNumbersAppend()
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
			var tester = new SADDocumentPackDocumentWrapper(testHeader1);
			AssertEquals("Part 1 of 2 - Part of 1 Package", tester.MarksAndNumbersAppend);
			testDeclaration.JE_TotalNoOfPacks = 2;
			testHeader1.CH_Packages = 1;
			testHeader2.CH_Packages = 1;
			Factory.Save();
			tester = new SADDocumentPackDocumentWrapper(testHeader1);
			AssertEquals("Part 1 of 2 - 1 Packages of 2", tester.MarksAndNumbersAppend);
		}

		public void TestTotalDutiesAndTaxes()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", true);
			universalReferenceDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", "DTY");
			universalReferenceDataHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = universalReferenceDataHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			universalReferenceDataHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			Factory.Save();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = "SND";
			user.GS_FullName = "MSGSender";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "YY";
			var testOrgInvHeader = declaration.Invoices.AddNew();
			var testOrgInvLine = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine.JI_CustomsQuantity = 150;
			testOrgInvLine.JI_CustomsUnitQty = "KG";
			testOrgInvLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInvLine2 = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = entryHeader.MergedLines.AddNew();
			testOrgInvLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			Factory.Save();
			var entryWrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertEquals(21m, entryWrapper.TotalDutiesAndTaxes);
			var message = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			message.EM_MessageText = CUSDEC_BGM9_Message;
			message.EM_SystemCreateUser = user.GS_Code;
			message.EM_LinkedObject = entryHeader;
			entryHeader.Messages.Add(message);
			var messageWrapper = SADDocumentPackDocumentWrapper.NewForMessage(message);
			AssertEquals(1116.00m, messageWrapper.TotalDutiesAndTaxes);
		}

		public void TestValuationCode_for_ZA_SAD_Document_WithEntryHeaderAsStartingPoint()
		{
			CreateReferenceData_for_TestingValuationCode_for_ZA();
			const string Import = ZAJobMessageTypeList.Codes.Import;
			const string Export = ZAJobMessageTypeList.Codes.Export;
			const string Related = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes;
			const string NotRelated = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.No;
			const string Exempt = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt;
			const string Section1 = MasterFiles.Business.Customs.ZA.ValuationCodeList.Codes.Section1;
			const string Builtin = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			const string VDN = "12345";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = Related;
			invoice.JZ_ValuationCode = Section1;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_RelatedIndicator = Related;
			invoiceLine.JI_ValuationCode = Section1;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			Assert("Pre: The CUSDECSource must be a MessageSendingObject.", wrapper.CUSDECSource is MessageSendingObject);
			Assert("Pre: The CUSDECSource must not be a CUSDECMessageHelper.", !(wrapper.CUSDECSource is CUSDECMessageHelper));
			AssertEquals("Pre: Must be original message type (9).", "9", wrapper.MessageType);
			var list = new RelatedIndicator_and_ValuationCode_TestCase_Collection();
			list.Add(Import, "", "11", Related, Section1, "R", "1");
			list.Add(Import, VDN, "11", Related, Section1, "", "");
			list.Add(Import, "", "11", Related, Section1, "R", "1");
			list.Add(Import, "", "20", Related, Section1, "", "");
			list.Add(Import, "", "11", Related, Section1, "R", "1");
			list.Add(Import, "", "21", Related, Section1, "", "");
			list.Add(Import, "", "11", Related, Section1, "R", "1");
			list.Add(Import, "", "22", Related, Section1, "", "");
			list.Add(Export, VDN, "60", Related, Section1, "", "");
			list.Add(Import, VDN, "11", Related, Section1, "", "");
			list.Add(Import, "", "14", Exempt, "", "E", "");
			list.Add(Import, VDN, "14", Exempt, "", "", "");
			list.Add(Import, VDN, "11", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "12", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "20", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "21", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "22", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "37", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "78", NotRelated, Section1, "", "");
			list.Add(Export, VDN, "60", Related, Section1, "", "");
			list.Add(Import, "", "14", Exempt, "", "E", "");
			list.Add(Import, "", "11", NotRelated, Section1, "N", "1");
			list.Add(Import, "", "12", NotRelated, Section1, "", "");
			list.Add(Import, "", "20", NotRelated, Section1, "", "");
			list.Add(Import, "", "21", NotRelated, Section1, "", "");
			list.Add(Import, "", "22", NotRelated, Section1, "", "");
			list.Add(Import, "", "37", NotRelated, Section1, "", "");
			list.Add(Import, "", "78", NotRelated, Section1, "", "");
			list.Add(Export, "", "60", Related, Section1, "", "");
			list.TestAllTestCases(declaration, wrapper);
		}

		public void TestValuationCode_for_ZA_SAD_Document_WithCusdecMessageAsStartingPoint()
		{
			CreateReferenceData_for_TestingValuationCode_for_ZA();
			#region Constants:
			const string Import = ZAJobMessageTypeList.Codes.Import;
			const string Export = ZAJobMessageTypeList.Codes.Export;
			const string Related = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes;
			const string NotRelated = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.No;
			const string Exempt = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt;
			const string Section1 = MasterFiles.Business.Customs.ZA.ValuationCodeList.Codes.Section1;
			const string VDN = "12345";
			#endregion Constants.
			#region Test Various Scenarios:
			var list = new RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase_Collection(Factory);
			list.Add(Import, VDN, "11", Related, Section1, "", "");
			list.Add(Import, "", "11", Related, Section1, "R", "1");
			list.Add(Import, VDN, "14", Exempt, "", "", "");
			list.Add(Import, VDN, "11", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "12", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "20", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "21", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "22", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "37", NotRelated, Section1, "", "");
			list.Add(Import, VDN, "78", NotRelated, Section1, "", "");
			list.Add(Export, VDN, "60", Related, Section1, "", "");
			list.Add(Import, "", "14", Exempt, "", "E", "");
			list.Add(Import, "", "11", NotRelated, Section1, "N", "1");
			list.Add(Import, "", "12", NotRelated, Section1, "", "");
			list.Add(Import, "", "20", NotRelated, Section1, "", "");
			list.Add(Import, "", "21", NotRelated, Section1, "", "");
			list.Add(Import, "", "22", NotRelated, Section1, "", "");
			list.Add(Import, "", "37", NotRelated, Section1, "", "");
			list.Add(Import, "", "78", NotRelated, Section1, "", "");
			list.Add(Export, "", "60", Related, Section1, "", "");
			list.TestAllTestCases();
			#endregion Test Various Scenarios.
		}

		public void Test_BND_SuretyBond_AdditionalInformation_WithEntryHeaderAsStartingPoint()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "60";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RelatedIndicator = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes;
			invoice.JZ_ValuationCode = MasterFiles.Business.Customs.ZA.ValuationCodeList.Codes.Section1;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_RelatedIndicator = MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes;
			invoiceLine.JI_ValuationCode = MasterFiles.Business.Customs.ZA.ValuationCodeList.Codes.Section1;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.AdditionalInformationCodes.AddNew("NUI", "N");
			entryLine.AdditionalInformationCodes.AddNew("BND", "700");
			entryLine.AdditionalInformationCodes.AddNew("VTE", "");
			entryLine.AdditionalInformationCodes.AddNew("VDN", "161718");
			AssertEquals("Pre: Do the testing for Exports.", ZAJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertEquals(true, wrapper.CUSDECSource is MessageSendingObject);
			AssertEquals(false, wrapper.CUSDECSource is CUSDECMessageHelper);
			AssertEquals("9", wrapper.MessageType);
			var addInfos = wrapper.FirstLineDetail.GeneralAddInfos.ToList().ConvertAll(x => (AdditionalInformationDocWrapper)x);
			CheckAdditionalInformationDetails(addInfos, "BND", "700");
			CheckAdditionalInformationDetails(addInfos, "NUI", "N");
			CheckAdditionalInformationDetails(addInfos, "VDN", "161718");
			CheckAdditionalInformationDetails(addInfos, "VTE", "");
		}

		public void Test_BND_SuretyBond_AdditionalInformation_WithCusdecMessageAsStartingPoint()
		{
			var assertMessage = "Test_BND_SuretyBond_AdditionalInformation_WithCusdecMessageAsStartingPoint ";
			string cusdecMessageText = @"UNH+89+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+00505655JSA20160506000088::00001+9'
CST++A:117:ZZZ'
LOC+14+50::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+JSA::ZZZ'
LOC+9+AUSYD::5'
GIS+R1:127:ZZZ'
GIS+D:134:ZZZ'
MEA+AAE+AAD+KGM:500.00'
FTX+LIN+++1::N'
RFF+BH:VICTHB001'
DTM+137:20160506:102'
RFF+AAS:081-21333222'
DTM+137:20160506:102'
RFF+ABI:8120067395'
RFF+ACD:89'
PAC+2'
PCI++MARKS AND NUMBERS TESTING HERE'
TDT+20+0811002+4'
DOC+380+INVH1'
DTM+3:20160118:102'
NAD+AG+00505655'
RFF+VA:123321'
NAD+MS+TST'
UNS+D'
CST+0001+845012907:108:ZZZ+100'
FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'
FTX+ACB+++NUIN:VDN141516:VTE:BND00000000000000000000000000000900'
FTX+CCI+++60:00'
LOC+27+AU'
MEA+AAR++NO:20.00'
MOA+38:2250'
MOA+40:2250'
TAX+1+1P1:107:ZZZ'
MOA+161:675.00'
TAX+1+VAT:107:ZZZ'
MOA+161:441.00'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:2250'
TAX+3+TDD:107:ZZZ'
MOA+161:675.00'
TAX+3+TVD:107:ZZZ'
MOA+161:441.00'
TAX+3+CUS:107:ZZZ'
MOA+161:2250'
UNT+49+89'";
			cusdecMessageText = cusdecMessageText.Replace("\r\n", "");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusdecMessage = Factory.New<CUSDECEDIMessage>();
			cusdecMessage.EM_ReceiveTransmit = "TRX";
			cusdecMessage.EM_ApplicationCode = "ZAC";
			cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			cusdecMessage.EM_Status = "SNT";
			cusdecMessage.EM_MessageText = cusdecMessageText;
			cusdecMessage.EM_MessageNum = "202";
			entryHeader.Messages.Add(cusdecMessage);
			AssertEquals("Pre: Do the testing for Exports.", ZAJobMessageTypeList.Codes.Export, declaration.JE_MessageType);
			var wrapper = SADDocumentPackDocumentWrapper.NewForMessage(cusdecMessage);
			AssertEquals(assertMessage, false, wrapper.CUSDECSource is MessageSendingObject);
			AssertEquals(assertMessage, true, wrapper.CUSDECSource is CUSDECMessageHelper);
			var addInfos = wrapper.FirstLineDetail.GeneralAddInfos.ToList().ConvertAll(x => (AdditionalInformationDocWrapper)x);
			CheckAdditionalInformationDetails(addInfos, "BND", "900");
			CheckAdditionalInformationDetails(addInfos, "NUI", "N");
			CheckAdditionalInformationDetails(addInfos, "VDN", "141516");
			CheckAdditionalInformationDetails(addInfos, "VTE", "");
		}

		public void TestRemoverAndSubContractor()
		{
			var remover = OrgHeader.New(Factory);
			var removerCusCode = remover.CustomsCodes.AddNew();
			removerCusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			removerCusCode.OK_CustomsRegNo = "MAIN";

			var subContractor = OrgHeader.New(Factory);
			var subContractorCusCode = subContractor.CustomsCodes.AddNew();
			subContractorCusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			subContractorCusCode.OK_CustomsRegNo = "SUB";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			entryInstruction.CEI_OH_Carrier = remover.PK;
			entryInstruction.OH_SubContractor = subContractor.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertEquals(remover, wrapper.Remover);
			AssertEquals("MAIN", wrapper.RemoverTransporterCode);
			AssertEquals(subContractor, wrapper.SubContractor);
			AssertEquals("SUB", wrapper.SubContractorTransporterCode);

			var removerForEdi = OrgHeader.New(Factory);
			var removerForEdiCusCode = removerForEdi.CustomsCodes.AddNew();
			removerForEdiCusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			removerForEdiCusCode.OK_CustomsRegNo = "90707070";

			var ediMessage = Factory.New<CUSDECEDIMessageForTest>();
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			ediMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			ediMessage.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			ediMessage.EM_Status = EDIMessage.Status.Sent;
			ediMessage.EM_MessageText = CUSDEC_BGM9_Message;
			entryHeader.Messages.Add(ediMessage);

			wrapper = SADDocumentPackDocumentWrapper.NewForMessage(ediMessage);
			AssertEquals(remover, wrapper.Remover);
			AssertEquals("MAIN", wrapper.RemoverTransporterCode);
			AssertEquals(subContractor, wrapper.SubContractor);
			AssertEquals("SUB", wrapper.SubContractorTransporterCode);
		}

		public void TestISourceIdentifierProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new SADDocumentPackDocumentWrapper(entryHeader);
			AssertNotNull("SADDocumentPackDocumentWrapper should implement ISourceIdentifierProvider", wrapper);
			AssertEquals(entryHeader.PK, wrapper.SourceIdentifier);
		}

		public void TestTransportName()
		{
			var longNameVessel = Factory.NewWithValidTestData<RefVessel>();
			longNameVessel.RV_Code = "TEST VESSEL WITH A VERY LONG NAME";
			longNameVessel.RV_RadioCallSign = "1234567";
			longNameVessel.RV_CarrierCode = "AR1";
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_Carrier = "AR1";
			declaration.JE_RadioCallSign = "1234567";
			declaration.JE_VesselName = "TEST VESSEL WITH A VERY LONG NAME";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var documentWrapper = new SADDocumentPackDocumentWrapper(entry);
			AssertEquals("AR1 1234567  TEST VESSEL WITH A VER", documentWrapper.TransportName);
		}

		static readonly ZString CUSDEC_BGM9_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+AF+90707070'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";

		static void ApplySanityCheck_BeforeTesting_ShouldOutput_RelatedIndicator_and_ValuationCode_on_SAD_Document(string shipmentType, string vdn, string cpc, bool specifiedDesiredOutput)
		{
			var cpcList = new List<string>()
			{ "12", "20", "21", "22", "37", "78" };
			bool calculatedOutput = false;
			if (shipmentType == ZAJobMessageTypeList.Codes.Import)
			{
				if (string.IsNullOrEmpty(vdn))
				{
					if (!cpcList.Contains(cpc))
					{
						calculatedOutput = true;
					}
				}
			}

			if (specifiedDesiredOutput != calculatedOutput)
			{
				var errorMsg = "Invalid input parameters specified for unit testing. The following combination does not make sense:";
				var assertMsg = $"{errorMsg} (ShipmentType = {shipmentType}; VDN = {vdn}; CPC = {cpc}; ShouldOutputRelatedIndicator = {specifiedDesiredOutput})";
				Assert(assertMsg, false);
			}
		}

		void CreateReferenceData_for_TestingValuationCode_for_ZA()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("ZA", "X", "11", "00", "", "X1100", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "12", "00", "", "X1200", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "20", "00", "", "X2000", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "21", "00", "", "X2100", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "22", "00", "", "X2200", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "37", "00", "", "X3700", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "78", "00", "", "X7800", "IMP,EXP");
			helper.CreateRefCusProcedure("ZA", "X", "60", "00", "", "X6000", "IMP,EXP");
			var startDate = new ZDateTime("1900-01-01");
			var endDate = new ZDateTime("2079-06-06");
			var countryCodeZA = "ZA";
			helper.CreateCusMapType(mapType: "REL", direction: "BTH", description: "Related Party Indicator", isReadonly: true);
			helper.CreateCusMap(mapType: "REL", cW1orCommercialValue: "E", customsValue: "E", startDate, endDate, countryCodeZA);
			helper.CreateCusMap(mapType: "REL", cW1orCommercialValue: "N", customsValue: "N", startDate, endDate, countryCodeZA);
			helper.CreateCusMap(mapType: "REL", cW1orCommercialValue: "Y", customsValue: "R", startDate, endDate, countryCodeZA);
			Factory.Save();
		}

		void CheckAdditionalInformationDetails(List<AdditionalInformationDocWrapper> addInfos, string codeToTest, string expectedValue)
		{
			var assertMsg = "Checking AdditionalInformationDocWrapper where Code = " + codeToTest;
			var wrapper = addInfos.Find(x => x.Code == codeToTest);
			Assert(assertMsg, wrapper != null);
			AssertEquals(assertMsg, codeToTest, wrapper.Code);
			AssertEquals(assertMsg, expectedValue, wrapper.Value);
		}

		sealed class RelatedIndicator_and_ValuationCode_TestCase
		{
			public RelatedIndicator_and_ValuationCode_TestCase(string shipmentType, string vdn, string cpc, string relatedIndicator_on_Invoice, string valuationCode_on_Invoice, string expected_RelatedPartyIndicator, string expected_ValuationCode)
			{
				ApplySanityCheck_BeforeTesting_ShouldOutput_RelatedIndicator_and_ValuationCode_on_SAD_Document(shipmentType, vdn, cpc, !string.IsNullOrEmpty(expected_RelatedPartyIndicator));
				this.shipmentType = shipmentType;
				vDN = vdn;
				cPC = cpc;
				this.relatedIndicator_on_Invoice = relatedIndicator_on_Invoice;
				this.valuationCode_on_Invoice = valuationCode_on_Invoice;
				this.expected_RelatedPartyIndicator = expected_RelatedPartyIndicator;
				this.expected_ValuationCode = expected_ValuationCode;
			}

			public void Check_for_RelatedPartyIndicator_and_ValuationCode_on_Wrapper(JobDeclaration declaration, SADDocumentPackDocumentWrapper wrapper)
			{
				var instruction = declaration.CustomsEntryInstructions[0];
				var invoice = declaration.Invoices[0];
				declaration.JE_MessageType = shipmentType;
				instruction.CEI_Style = cPC;
				invoice.JZ_VDN = vDN;
				invoice.JZ_RelatedIndicator = relatedIndicator_on_Invoice;
				invoice.JZ_ValuationCode = valuationCode_on_Invoice;
				#region Prepare Assert Message:
				var negate = string.IsNullOrEmpty(expected_RelatedPartyIndicator) ? " not" : "";
				var direction = (shipmentType == ZAJobMessageTypeList.Codes.Export) ? "Export" : "Import";
				var withVDN = string.IsNullOrEmpty(vDN) ? ", no VDN" : ", with VDN";
				var related = "";
				switch (relatedIndicator_on_Invoice)
				{
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes:
						related = ", Related";
						break;
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.No:
						related = ", Not Related";
						break;
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt:
						related = ", Exempt";
						break;
				}

				var assertMsg = "Must" + negate + " output Related Indicator and Valuation Code for " + direction + " (CPC=" + cPC + ")" + related + withVDN;
				#endregion Prepare Assert Message.
				AssertEquals(assertMsg, expected_RelatedPartyIndicator, wrapper.RelatedPartyIndicator);
				AssertEquals(assertMsg, expected_ValuationCode, wrapper.ValuationCode);
			}

			readonly string shipmentType;
			readonly string vDN;
			readonly string cPC;
			readonly string relatedIndicator_on_Invoice;
			readonly string valuationCode_on_Invoice;
			readonly string expected_RelatedPartyIndicator;
			readonly string expected_ValuationCode;
		}

		sealed class RelatedIndicator_and_ValuationCode_TestCase_Collection
		{
			public void Add(string shipmentType, string vdn, string cpc, string relatedIndicator_on_Invoice, string valuationCode_on_Invoice, string expected_RelatedPartyIndicator, string expected_ValuationCode)
			{
				list.Add(new RelatedIndicator_and_ValuationCode_TestCase(shipmentType, vdn, cpc, relatedIndicator_on_Invoice, valuationCode_on_Invoice, expected_RelatedPartyIndicator, expected_ValuationCode));
			}

			public void TestAllTestCases(JobDeclaration declaration, SADDocumentPackDocumentWrapper wrapper) => list.ForEach(x => x.Check_for_RelatedPartyIndicator_and_ValuationCode_on_Wrapper(declaration, wrapper));
			readonly List<RelatedIndicator_and_ValuationCode_TestCase> list = new List<RelatedIndicator_and_ValuationCode_TestCase>();
		}

		sealed class RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase
		{
			public RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase(string shipmentType, string vdn, string cpc, string relatedIndicator_in_EdiMessage, string valuationCode_in_EdiMessage, string expected_RelatedPartyIndicator, string expected_ValuationCode, BusinessObjectFactory factory)
			{
				ApplySanityCheck_BeforeTesting_ShouldOutput_RelatedIndicator_and_ValuationCode_on_SAD_Document(shipmentType, vdn, cpc, !string.IsNullOrEmpty(expected_RelatedPartyIndicator));
				this.shipmentType = shipmentType;
				vDN = vdn;
				cPC = cpc;
				this.relatedIndicator_in_EdiMessage = relatedIndicator_in_EdiMessage;
				this.valuationCode_in_EdiMessage = valuationCode_in_EdiMessage;
				this.expected_RelatedPartyIndicator = expected_RelatedPartyIndicator;
				this.expected_ValuationCode = expected_ValuationCode;
				this.factory = factory;
			}

			public void CheckRelatedPartyIndicator_and_ValuationCode()
			{
				var declaration = factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = shipmentType;
				#region Prepare EDI message:
				var ediMessageText = cusdecMessageText.Replace("\r\n", "");
				if (string.IsNullOrEmpty(vDN))
				{
					ediMessageText = ediMessageText.Replace("FTX+ACB+++NUIN:VDN141516:VTE", "FTX+ACB+++NUIN:VTE");
				}

				var cpcSegment = "FTX+CCI+++" + cPC + ":00'";
				ediMessageText = ediMessageText.Replace("FTX+CCI+++11:00'", cpcSegment);
				var gisSegment = new StringBuilder("GIS+");
				switch (relatedIndicator_in_EdiMessage)
				{
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes:
						gisSegment.Append("R");
						break;
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.No:
						gisSegment.Append("N");
						break;
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt:
						gisSegment.Append("E");
						break;
				}

				gisSegment.Append(valuationCode_in_EdiMessage);
				gisSegment.Append(":127:ZZZ'");
				ediMessageText = ediMessageText.Replace("GIS+R1:127:ZZZ'", gisSegment.ToString());
				#endregion Prepare EDI message.
				var cusdecMessage = factory.New<CUSDECEDIMessage>();
				cusdecMessage.EM_ReceiveTransmit = "TRX";
				cusdecMessage.EM_ApplicationCode = "ZAC";
				cusdecMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
				cusdecMessage.EM_Status = "SNT";
				cusdecMessage.EM_MessageText = ediMessageText;
				cusdecMessage.EM_MessageNum = "202";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.Messages.Add(cusdecMessage);
				var wrapper = SADDocumentPackDocumentWrapper.NewForMessage(cusdecMessage);
				Assert("Pre: The CUSDECSource must not be a MessageSendingObject.", !(wrapper.CUSDECSource is MessageSendingObject));
				Assert("Pre: The CUSDECSource must be a CUSDECMessageHelper.", wrapper.CUSDECSource is CUSDECMessageHelper);
				#region Prepare Assert Message:
				var negate = string.IsNullOrEmpty(expected_RelatedPartyIndicator) ? " not" : "";
				var direction = (shipmentType == ZAJobMessageTypeList.Codes.Export) ? "Export" : "Import";
				var withVDN = string.IsNullOrEmpty(vDN) ? ", no VDN" : ", with VDN";
				var related = "";
				switch (relatedIndicator_in_EdiMessage)
				{
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Yes:
						related = ", Related";
						break;
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.No:
						related = ", Not Related";
						break;
					case MasterFiles.Business.Customs.RelatedIndicatorList.Codes.Exempt:
						related = ", Exempt";
						break;
				}

				var assertMsg = "Must" + negate + " output Related Indicator and Valuation Code for " + direction + " (CPC=" + cPC + ")" + related + withVDN;
				#endregion Prepare Assert Message.
				AssertEquals(assertMsg, expected_RelatedPartyIndicator, wrapper.RelatedPartyIndicator);
				AssertEquals(assertMsg, expected_ValuationCode, wrapper.ValuationCode);
			}

			readonly string shipmentType;
			readonly string vDN;
			readonly string cPC;
			readonly string relatedIndicator_in_EdiMessage;
			readonly string valuationCode_in_EdiMessage;
			readonly string expected_RelatedPartyIndicator;
			readonly string expected_ValuationCode;
			readonly BusinessObjectFactory factory;
			static readonly string cusdecMessageText = @"UNH+89+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+00505655JSA20160506000088::00001+9'
CST++A:117:ZZZ'
LOC+14+50::ZZZ'
LOC+35+AU::5'
LOC+36+ZA::5'
LOC+96+JSA::ZZZ'
LOC+9+AUSYD::5'
GIS+R1:127:ZZZ'
GIS+D:134:ZZZ'
MEA+AAE+AAD+KGM:500.00'
FTX+LIN+++1::N'
RFF+BH:VICTHB001'
DTM+137:20160506:102'
RFF+AAS:081-21333222'
DTM+137:20160506:102'
RFF+ABI:8120067395'
RFF+ACD:89'
PAC+2'
PCI++MARKS AND NUMBERS TESTING HERE'
TDT+20+0811002+4'
DOC+380+INVH1'
DTM+3:20160118:102'
NAD+AG+00505655'
RFF+VA:123321'
NAD+MS+TST'
UNS+D'
CST+0001+845012907:108:ZZZ+100'
FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'
FTX+ACB+++NUIN:VDN141516:VTE'
FTX+CCI+++11:00'
LOC+27+AU'
MEA+AAR++NO:20.00'
MOA+38:2250'
MOA+40:2250'
TAX+1+1P1:107:ZZZ'
MOA+161:675.00'
TAX+1+VAT:107:ZZZ'
MOA+161:441.00'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:2250'
TAX+3+TDD:107:ZZZ'
MOA+161:675.00'
TAX+3+TVD:107:ZZZ'
MOA+161:441.00'
TAX+3+CUS:107:ZZZ'
MOA+161:2250'
UNT+49+89'";
		}

		sealed class RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase_Collection
		{
			public RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase_Collection(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public void Add(string shipmentType, string vdn, string cpc, string relatedIndicator_in_EdiMessage, string valuationCode_in_EdiMessage, string expected_RelatedPartyIndicator, string expected_ValuationCode)
			{
				list.Add(new RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase(shipmentType, vdn, cpc, relatedIndicator_in_EdiMessage, valuationCode_in_EdiMessage, expected_RelatedPartyIndicator, expected_ValuationCode, factory));
			}

			public void TestAllTestCases() => list.ForEach(x => x.CheckRelatedPartyIndicator_and_ValuationCode());
			readonly BusinessObjectFactory factory;
			readonly List<RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase> list = new List<RelatedIndicator_and_ValuationCode_from_EdiMessage_TestCase>();
		}
	}
}
