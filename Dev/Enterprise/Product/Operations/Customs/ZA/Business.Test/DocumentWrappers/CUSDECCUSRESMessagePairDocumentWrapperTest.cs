using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(CUSDECCUSRESMessagePairDocumentWrapper))]
	sealed class CUSDECCUSRESMessagePairDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTotalFieldsFromEntry_WhenNoOutGoingMessage()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			testInst.CEI_ProvisionalPaymentAmount = 55m;
			testInst.CEI_Style = "40";
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = testInst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "43";
			invLine.JI_Tariff = "00001000";
			invLine.JI_ZZF_NKTaxType = "VAT";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_CEI_Instruction = testInst.PK;
			var testLine = testHeader.MergedLines.AddNew();
			testLine.CL_CustomsValue = 20m;
			testLine.Fees.AddOrUpdate("1P1", 500m);
			testLine.Fees.AddOrUpdate("12A", 400m);
			testLine.Fees.AddOrUpdate("12B", 300m);
			testLine.Fees.AddOrUpdate("VAT", 200m);
			testLine.ProvisionalPayments.AddNew("PPA", 50m);
			testLine.ProvisionalPayments.AddNew("PRP", 40m);
			testLine.ProvisionalPayments.AddNew("PEN", 30m);
			testLine.InvoiceLines.Add(invLine);
			var wrapper = new CUSDECCUSRESMessagePairDocumentWrapper(testHeader);
			CombineAssertions("Not Getting the value from EntryHeader, Even when no outgoing message Sent", () =>
			{
				AssertEquals("TotalDutiesAndTaxes", 0m, wrapper.TotalDutiesAndTaxes);
				AssertEquals("TotalCustomsDutyExcluding12B", 0m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals("TotalS1P2BDuty", 0m, wrapper.TotalS1P2BDuty);
				AssertEquals("TotalValueAddedTax", 0m, wrapper.TotalValueAddedTax);
				AssertEquals("TotalPPs", 0m, wrapper.TotalPPs);
			});
			var message = header.Messages.AddNew();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'NAD+AG+00505655'RFF+VA:123321'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			wrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			CombineAssertions("Getting the value from dbo.EDIMessage, but ediMessagehasNoValueStored", () =>
			{
				AssertEquals(0m, wrapper.TotalDutiesAndTaxes);
				AssertEquals(0m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals(0m, wrapper.TotalS1P2BDuty);
				AssertEquals(0m, wrapper.TotalValueAddedTax);
				AssertEquals(0m, wrapper.TotalPPs);
			});
			message.CustomsDutyNoS1P2BAfter = 1.1m;
			message.S1P2BDutyAfter = 1.2m;
			message.ValueAddedTaxAfter = 1.3m;
			message.ProvisionalPaymentAmountAfter = 1.4m;
			message.PenaltyAmountAfter = 1.5m;
			wrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			CombineAssertions("Getting the value from dbo.EDIMessage code Data", () =>
			{
				AssertEquals(6.5m, wrapper.TotalDutiesAndTaxes);
				AssertEquals(1.1m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals(1.2m, wrapper.TotalS1P2BDuty);
				AssertEquals(1.3m, wrapper.TotalValueAddedTax);
				AssertEquals(2.9m, wrapper.TotalPPs);
			});
		}

		public void TestTotalFieldsFromMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			testInst.CEI_ProvisionalPaymentAmount = 55m;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInst.PK;
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.InvoiceLines.Add(invoiceLine);
			entryLine.Fees.AddOrUpdate("1P1", 500m);
			entryLine.Fees.AddOrUpdate("12A", 400m);
			entryLine.Fees.AddOrUpdate("12B", 300m);
			entryLine.Fees.AddOrUpdate("VAT", 200m);
			entryLine.ProvisionalPayments.AddNew("PPA", 50m);
			entryLine.ProvisionalPayments.AddNew("PRP", 40m);
			entryLine.ProvisionalPayments.AddNew("PEN", 30m);
			var cusdecMessage1 = entryHeader.Messages.AddNew();
			cusdecMessage1.EM_ApplicationCode = "ZAC";
			cusdecMessage1.EM_MessageType = "DEC";
			cusdecMessage1.EM_ReceiveTransmit = "TRX";
			cusdecMessage1.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			cusdecMessage1.EM_MessageNum = "152";
			cusdecMessage1.CustomsDutyNoS1P2BAfter = 1.1m;
			cusdecMessage1.S1P2BDutyAfter = 1.2m;
			cusdecMessage1.ValueAddedTaxAfter = 1.3m;
			cusdecMessage1.ProvisionalPaymentAmountAfter = 1.4m;
			cusdecMessage1.PenaltyAmountAfter = 1.5m;
			var cusresMessage1 = entryHeader.Messages.AddNew(typeof(CUSRESEDIMessage));
			cusresMessage1.EM_ApplicationCode = "ZAC";
			cusresMessage1.EM_MessageType = "RES";
			cusresMessage1.EM_ReceiveTransmit = "RCV";
			cusresMessage1.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:152'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			var cusdecMessage2 = entryHeader.Messages.AddNew();
			cusdecMessage2.EM_ApplicationCode = "ZAC";
			cusdecMessage2.EM_MessageType = "DEC";
			cusdecMessage2.EM_ReceiveTransmit = "TRX";
			cusdecMessage2.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+1'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			cusdecMessage2.EM_MessageNum = "153";
			cusdecMessage2.CustomsDutyNoS1P2BAfter = 2.1m;
			cusdecMessage2.S1P2BDutyAfter = 2.2m;
			cusdecMessage2.ValueAddedTaxAfter = 2.3m;
			cusdecMessage2.ProvisionalPaymentAmountAfter = 2.4m;
			cusdecMessage2.PenaltyAmountAfter = 2.5m;
			var cusresMessage2 = entryHeader.Messages.AddNew(typeof(CUSRESEDIMessage));
			cusresMessage2.EM_ApplicationCode = "ZAC";
			cusresMessage2.EM_MessageType = "RES";
			cusresMessage2.EM_ReceiveTransmit = "RCV";
			cusresMessage2.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:153'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			CombineAssertions("Pre-Check, Values on EntryHeader", () =>
			{
				AssertEquals("AmountDueAfter", 1535m, entryHeader.AmountDueAfter);
				AssertEquals("CustomsDutyExcluding12BAfter", 900m, entryHeader.CustomsDutyExcluding12BAfter);
				AssertEquals("S1P2BDutyAfter", 300m, entryHeader.S1P2BDutyAfter);
				AssertEquals("ValueAddedTax", 200m, entryHeader.ValueAddedTax);
				AssertEquals("ProvisionalPaymentAmountAfter", 105m, entryHeader.ProvisionalPaymentAmountAfter);
				AssertEquals("PenaltyAmountAfter", 30m, entryHeader.PenaltyAmountAfter);
			});
			CombineAssertions("Test Totals from Header's Latest Message", () =>
			{
				var wrapper = new CUSDECCUSRESMessagePairDocumentWrapper(entryHeader);
				AssertEquals(11.5m, wrapper.TotalDutiesAndTaxes);
				AssertEquals(2.1m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals(2.2m, wrapper.TotalS1P2BDuty);
				AssertEquals(2.3m, wrapper.TotalValueAddedTax);
				AssertEquals(4.9m, wrapper.TotalPPs);
			});
			CombineAssertions("Test Totals from Header's Latest Message By Specific Message", () =>
			{
				var wrapper = CUSDECCUSRESMessagePairDocumentWrapper.NewForMessage(cusresMessage2 as CUSRESEDIMessage);
				AssertEquals(11.5m, wrapper.TotalDutiesAndTaxes);
				AssertEquals(2.1m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals(2.2m, wrapper.TotalS1P2BDuty);
				AssertEquals(2.3m, wrapper.TotalValueAddedTax);
				AssertEquals(4.9m, wrapper.TotalPPs);
			});
			CombineAssertions("Test Totals from Header's previous Message By Specific Message", () =>
			{
				var wrapper = CUSDECCUSRESMessagePairDocumentWrapper.NewForMessage(cusresMessage1 as CUSRESEDIMessage);
				AssertEquals(6.5m, wrapper.TotalDutiesAndTaxes);
				AssertEquals(1.1m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals(1.2m, wrapper.TotalS1P2BDuty);
				AssertEquals(1.3m, wrapper.TotalValueAddedTax);
				AssertEquals(2.9m, wrapper.TotalPPs);
			});
		}

		public void TestBusinessObjectToLogAgainst()
		{
			var testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals(header, (testWrapper as IBODocDataProvider).BusinessObjectToLogAgainst);
		}

		public void TestJobBranchLogo()
		{
			var image1 = new Bitmap(1, 2);
			var image2 = new Bitmap(2, 3);
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			var testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(companyPK, Guid.Empty, Guid.Empty, image1);
			AssertEquals(1, testWrapper.JobBranchLogo.Width);
			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, branchPK, Guid.Empty, image2);
			AssertEquals(2, testWrapper.JobBranchLogo.Width);
		}

		public void TestCUSDECMessage_LastOutgoingMessageOfAnEntry()
		{
			var message = header.Messages.AddNew();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			message = header.Messages.AddNew();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+1'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			var testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("1", testWrapper.CUSDECMessage.MessageType);
			AssertNull(testWrapper.CUSRESMessage);
		}

		public void TestCUSDECMessage_ConstructWithoutNoCUSDEC()
		{
			CUSDECCUSRESMessagePairDocumentWrapper wrapper = null;
			var message = Factory.NewWithValidTestData<CUSRESEDIMessage>();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "RES";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:152'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			AssertNull(CUSDECCUSRESMessagePairDocumentWrapper.NewForMessage(message));
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			testInst.CEI_ProvisionalPaymentAmount = 55m;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInst.PK;
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.InvoiceLines.Add(invoiceLine);
			entryLine.Fees.AddOrUpdate("1P1", 500m);
			entryLine.Fees.AddOrUpdate("12A", 400m);
			entryLine.Fees.AddOrUpdate("12B", 300m);
			entryLine.Fees.AddOrUpdate("VAT", 200m);
			entryLine.ProvisionalPayments.AddNew("PPA", 50m);
			entryLine.ProvisionalPayments.AddNew("PRP", 40m);
			entryLine.ProvisionalPayments.AddNew("PEN", 30m);
			entryHeader.Messages.Add(message);
			CombineAssertions("Pre-Check", () =>
			{
				AssertEquals("AmountDueAfter", 1535m, entryHeader.AmountDueAfter);
				AssertEquals("CustomsDutyExcluding12BAfter", 900m, entryHeader.CustomsDutyExcluding12BAfter);
				AssertEquals("S1P2BDutyAfter", 300m, entryHeader.S1P2BDutyAfter);
				AssertEquals("ValueAddedTax", 200m, entryHeader.ValueAddedTax);
				AssertEquals("ProvisionalPaymentAmountAfter", 105m, entryHeader.ProvisionalPaymentAmountAfter);
				AssertEquals("PenaltyAmountAfter", 30m, entryHeader.PenaltyAmountAfter);
			});
			CombineAssertions("Generate from Entry Seeking Logic", () =>
			{
				wrapper = new CUSDECCUSRESMessagePairDocumentWrapper(entryHeader);
				AssertNull(wrapper.CUSDECMessage);
				AssertNotNull(wrapper.CUSRESMessage);
				AssertEquals("TotalDutiesAndTaxes", 0m, wrapper.TotalDutiesAndTaxes);
				AssertEquals("TotalCustomsDutyExcluding12B", 0m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals("TotalS1P2BDuty", 0m, wrapper.TotalS1P2BDuty);
				AssertEquals("TotalValueAddedTax", 0m, wrapper.TotalValueAddedTax);
				AssertEquals("TotalPPs", 0m, wrapper.TotalPPs);
			});
			CombineAssertions("Generate from cusres message", () =>
			{
				wrapper = CUSDECCUSRESMessagePairDocumentWrapper.NewForMessage(message);
				AssertNull(wrapper.CUSDECMessage);
				AssertNotNull(wrapper.CUSRESMessage);
				AssertEquals("TotalDutiesAndTaxes", 0m, wrapper.TotalDutiesAndTaxes);
				AssertEquals("TotalCustomsDutyExcluding12B", 0m, wrapper.TotalCustomsDutyExcluding12B);
				AssertEquals("TotalS1P2BDuty", 0m, wrapper.TotalS1P2BDuty);
				AssertEquals("TotalValueAddedTax", 0m, wrapper.TotalValueAddedTax);
				AssertEquals("TotalPPs", 0m, wrapper.TotalPPs);
			});
		}

		public void TestCUSDECMessage_MatchingFromCUSRES()
		{
			var message = header.Messages.AddNew();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			message.EM_MessageNum = "152";
			message = header.Messages.AddNew();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+1'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
			message.EM_MessageNum = "153";
			message = header.Messages.AddNew();
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "RES";
			message.EM_ReceiveTransmit = "RCV";
			message.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:152'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			var testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("locating cusdec base on cusres message body", "9", testWrapper.CUSDECMessage.MessageType);
		}

		public void TestExporterTinAndName()
		{
			var message = header.Messages.AddNew();
			message.EM_LinkedObject = header;
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageText = messageTxtWithExporter;
			declaration.JE_MessageType = "IMP";
			var testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("MessageTxtWithExporter", "1233211233", testWrapper.ExporterTIN);
			AssertEquals("MessageTxtWithExporter", "ABA BEUL ", testWrapper.ExporterName);
			message.EM_MessageText = messageTxtWithSupplier;
			testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("MessageTxtWithSupplier", "1233211234", testWrapper.ExporterTIN);
			AssertEquals("MessageTxtWithSupplier", "ABA BEUL2 ", testWrapper.ExporterName);
			message.EM_MessageText = messageTxtWithImporter;
			testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("MessageTxtWithImporter", "001", testWrapper.ExporterTIN);
			AssertEquals("MessageTxtWithImporter", "Exporter Org", testWrapper.ExporterName);
		}

		public void TestImporterTinAndName()
		{
			var message = header.Messages.AddNew();
			message.EM_LinkedObject = header;
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = "DEC";
			message.EM_ReceiveTransmit = "TRX";
			declaration.JE_MessageType = "EXP";
			message.EM_MessageText = messageTxtWithImporter;
			var testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("MessageTxtWithImporter", "70707070", testWrapper.ImporterTIN);
			AssertEquals("MessageTxtWithImporter", "TACSPO DISTRIBUTING PTY LTD", testWrapper.ImporterName);
			message.EM_MessageText = messageTxtWithExporter;
			testWrapper = new CUSDECCUSRESMessagePairDocumentWrapper(header);
			AssertEquals("MessageTxtWithExporter", "003", testWrapper.ImporterTIN);
			AssertEquals("MessageTxtWithExporter", "Importer Org", testWrapper.ImporterName);
		}

		JobDeclaration declaration;
		CusEntryHeader header;
		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.ActiveEntryHeaders.AddNew();
			var orgExporter = Factory.NewWithValidTestData<OrgHeader>();
			orgExporter.OH_FullName = "Exporter Org";
			var cusCodeExp = orgExporter.CustomsCodes.AddNew();
			cusCodeExp.OK_CodeType = "CSC";
			cusCodeExp.OK_CustomsRegNo = "001";
			var cusCodeExp2 = orgExporter.CustomsCodes.AddNew();
			cusCodeExp2.OK_CodeType = "CCD";
			cusCodeExp2.OK_CustomsRegNo = "002";
			declaration.JE_OH_Supplier = orgExporter.PK;
			var orgImporter = Factory.NewWithValidTestData<OrgHeader>();
			orgImporter.OH_FullName = "Importer Org";
			var cusCodeImp = orgImporter.CustomsCodes.AddNew();
			cusCodeImp.OK_CodeType = "CCD";
			cusCodeImp.OK_CustomsRegNo = "003";
			var cusCodeImp2 = orgImporter.CustomsCodes.AddNew();
			cusCodeImp2.OK_CodeType = "CSC";
			cusCodeImp2.OK_CustomsRegNo = "004";
			declaration.JE_OH_Importer = orgImporter.PK;
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var entryHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			return new CUSDECCUSRESMessagePairDocumentWrapper(entryHeader);
		}

		// 1233211233, ABA BEUL 
		readonly ZString messageTxtWithExporter = "UNH+260+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160622000248::00002+9'CST++H:117:ZZZ'LOC+14+50::ZZZ'LOC+45+NSA::ZZZ'LOC+35+DE::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'DTM+178:20160502:102'GIS+F:134:ZZZ'MEA+AAE+AAD+KGM:500.00'EQD+CN+11'EQD+CN+12'EQD+CN+13'EQD+CN+14'EQD+CN+15'FTX+LIN+++1::N'RFF+BH:00505655HBOLVIC001'DTM+137:20160503:102'RFF+AAS:T   08121333222'DTM+137:20160501:102'RFF+ABI:8120067395'RFF+ACD:260'PAC+0'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+TVOY+1'NAD+AG+00505655'NAD+EX+1233211233++ABA BEUL +83-93 DALMENY AVENUE 2018 ROSEBERY :GERMANY+ROSEBERY++2018'NAD+MS+TST'UNS+D'CST+0001+:108:ZZZ+100'FTX+ACB+++NUIN'FTX+CCI+++60:00:::2'LOC+27+AU'MOA+40:1'UNS+S'TAX+3+CUS:107:ZZZ'MOA+161:1'UNT+40+260'";
		// 1233211234, ABA BEUL2 
		readonly ZString messageTxtWithSupplier = "UNH+259+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160622000247::00002+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+DE::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'DTM+132:20160502:102'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'EQD+CN+11'EQD+CN+12'EQD+CN+13'EQD+CN+14'EQD+CN+15'FTX+LIN+++1::N'RFF+BH:00505655HBOLVIC001'DTM+137:20160503:102'RFF+AAS:T   08121333222'DTM+137:20160501:102'RFF+ABI:8120067395'RFF+ACD:259'PAC+0'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+TVOY+1'DOC+380+INVH1'DTM+3:20160118:102'NAD+IM+70707070++TACSPO DISTRIBUTING PTY LTD+980 LYTTON ROAD MURARRIE QLD 4172 A:USTRALIA+MURARRIE++4172'NAD+AG+00505655'NAD+SU+1233211234++ABA BEUL2 +83-93 DALMENY AVENUE 2018 ROSEBERY :GERMANY+ROSEBERY++2018'NAD+MS+TST'NAD+DT+:174:ZZZ++TACSPO DISTRIBUTING PTY LTD+980 LYTTON ROAD MURARRIE QLD 4172 A:USTRALIA+MURARRIE++4172'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:1688'MOA+40:1688'TAX+1+1P1:107:ZZZ'MOA+161:506.40'TAX+1+VAT:107:ZZZ'MOA+161:330.82'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:1688'TAX+3+TDD:107:ZZZ'MOA+161:506.40'TAX+3+TVD:107:ZZZ'MOA+161:330.82'TAX+3+CUS:107:ZZZ'MOA+161:1688'UNT+56+259'";
		// 70707070, TACSPO DISTRIBUTING PTY LTD 
		readonly ZString messageTxtWithImporter = "UNH+259+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160622000247::00002+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+DE::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'DTM+132:20160502:102'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'EQD+CN+11'EQD+CN+12'EQD+CN+13'EQD+CN+14'EQD+CN+15'FTX+LIN+++1::N'RFF+BH:00505655HBOLVIC001'DTM+137:20160503:102'RFF+AAS:T   08121333222'DTM+137:20160501:102'RFF+ABI:8120067395'RFF+ACD:259'PAC+0'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+TVOY+1'DOC+380+INVH1'DTM+3:20160118:102'NAD+IM+70707070++TACSPO DISTRIBUTING PTY LTD+980 LYTTON ROAD MURARRIE QLD 4172 A:USTRALIA+MURARRIE++4172'NAD+AG+00505655'NAD+MS+TST'NAD+DT+:174:ZZZ++TACSPO DISTRIBUTING PTY LTD+980 LYTTON ROAD MURARRIE QLD 4172 A:USTRALIA+MURARRIE++4172'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:1688'MOA+40:1688'TAX+1+1P1:107:ZZZ'MOA+161:506.40'TAX+1+VAT:107:ZZZ'MOA+161:330.82'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:1688'TAX+3+TDD:107:ZZZ'MOA+161:506.40'TAX+3+TVD:107:ZZZ'MOA+161:330.82'TAX+3+CUS:107:ZZZ'MOA+161:1688'UNT+56+259'";
	}
}
