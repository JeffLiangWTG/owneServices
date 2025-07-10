using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	sealed class MessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_EntryStatus = "1";
				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.MovementReferenceNumberSetter("MRN", ZDateTime.Now);
				var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader3.MovementReferenceNumberSetter("MRN", ZDateTime.Now);
				var testMessage = entryHeader3.Messages.AddNew();
				testMessage.EM_ApplicationCode = "ZAC";
				testMessage.EM_ReceiveTransmit = "RCV";
				testMessage.EM_MessageType = "RES";
				testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:00263869'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
				CombineAssertions("Cleared", () =>
				{
					var testwrapper = new MessageSendingObject(entryHeader1);
					AssertEquals(false, testwrapper.ShouldSend);
					AssertEquals("ORG", testwrapper.MessageType);
					AssertEquals("", testwrapper.CaseNumber);
					AssertEquals("", testwrapper.ChangeAcknowledgementIndicator);
				});
				CombineAssertions("MRN", () =>
				{
					var testwrapper = new MessageSendingObject(entryHeader2);
					AssertEquals(true, testwrapper.ShouldSend);
					AssertEquals("CHG", testwrapper.MessageType);
					AssertEquals("", testwrapper.CaseNumber);
					AssertEquals("", testwrapper.ChangeAcknowledgementIndicator);
				});
			}

			CombineAssertions("Defaule Message Type for EntryOutside CW1", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_DateForDuty = new ZDateTime(2016, 01, 01);
				testInst.CEI_Style = "11";
				var invHeader = declaration.Invoices.AddNew();
				invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_CEI = testInst.PK;
				declaration.DoMerge();
				var entryHeader = declaration.ActiveEntryHeaders[0];
				entryHeader.CH_BGMReference = "123321";
				var testwrapper = new MessageSendingObject(entryHeader);
				AssertEquals(true, testwrapper.ShouldSend);
				AssertEquals("CHG", testwrapper.MessageType);
				AssertEquals("", testwrapper.CaseNumber);
				AssertEquals("", testwrapper.ChangeAcknowledgementIndicator);
				AssertEquals("", testwrapper.LocalReferenceNumber);
				AssertEquals(false, testwrapper.LocalReferenceNumberInfo.ReadOnly);
			});
			CombineAssertions("Replaced", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_DateForDuty = new ZDateTime(2016, 01, 01);
				testInst.CEI_Style = "11";
				testInst.CEI_MRNToBeReplaced = "MRNToBeReplaced";
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = testInst.PK;
				var testwrapper = new MessageSendingObject(entryHeader);
				AssertEquals(true, testwrapper.ShouldSend);
				AssertEquals("REP", testwrapper.MessageType);
				AssertEquals("", testwrapper.CaseNumber);
				AssertEquals("", testwrapper.ChangeAcknowledgementIndicator);
				AssertEquals("MRNToBeReplaced", testwrapper.MovementReferenceNumber);
				AssertEquals(true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				entryHeader.MovementReferenceNumberSetter("MRN", ZDateTime.Today);
				testwrapper = new MessageSendingObject(entryHeader);
				AssertEquals(true, testwrapper.ShouldSend);
				AssertEquals("CHG", testwrapper.MessageType);
				AssertEquals("", testwrapper.CaseNumber);
				AssertEquals("", testwrapper.ChangeAcknowledgementIndicator);
				AssertEquals("MRN", testwrapper.MovementReferenceNumber);
				AssertEquals(true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCaseNumberDefaulting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CaseNumbers.AddNew(CaseNumberTypeList.Codes.SupportingDocsRequired, "CAS123");
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader1.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			var testwrapper = new MessageSendingObject(entryHeader1);
			testwrapper.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertNullOrEmpty(testwrapper.CaseNumber);
			testwrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			AssertEquals("CAS123", testwrapper.CaseNumber);
		}

		public void TestCaseNumberClosedStatus()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var caseNumber = instruction.CaseNumbers.AddNew();
			caseNumber.CY_Code = CaseNumberTypeList.Codes.SupportingDocsRequired;
			caseNumber.CY_Type = "CAS";
			caseNumber.CY_Data = "CAS333";
			caseNumber.CY_Date = new ZDateTime(2019, 12, 30);
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader1.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var testwrapper = new MessageSendingObject(entryHeader1);
			testwrapper.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertNullOrEmpty(testwrapper.CaseNumber);
			testwrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			AssertNullOrEmpty(testwrapper.CaseNumber);

			caseNumber.CY_Date = ZDateTime.Empty;
			testwrapper = new MessageSendingObject(entryHeader1);
			testwrapper.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertNullOrEmpty(testwrapper.CaseNumber);
			testwrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			AssertEquals("CAS333", testwrapper.CaseNumber);
		}

		public void TestCaseNumbersAvailableForSendingToCustoms()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var closedCases = new List<string>()
			{ "CAS001", "CAS003", "CAS005", "CAS007" };
			var caseNumberList = new List<CaseNumber>();
			for (int a = 1; a < 9; a++)
			{
				var caseNumber = instruction.CaseNumbers.AddNew();
				caseNumber.CY_Code = CaseNumberTypeList.Codes.SupportingDocsRequired;
				caseNumber.CY_Type = "CAS";
				caseNumber.CY_Data = "CAS00" + a;
				caseNumberList.Add(caseNumber);
			}

			caseNumberList.Where(x => closedCases.Contains(x.CY_Data)).ToList().ForEach(x => x.CY_Date = new ZDateTime(2020, 03, 26));
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader1.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			var msgSendingObj = new MessageSendingObject(entryHeader1);
			msgSendingObj.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("Total case numbers available for sending to Customs.", 4, msgSendingObj.CaseNumbers.Count);
			var casesAvailableForSending = new List<ICodeDescription>(msgSendingObj.CaseNumbers.ToArray());
			var caseNumbersAvailableForSending = casesAvailableForSending.Select(x => x.Code);
			Assert(caseNumbersAvailableForSending.Contains("CAS002"));
			Assert(caseNumbersAvailableForSending.Contains("CAS004"));
			Assert(caseNumbersAvailableForSending.Contains("CAS006"));
			Assert(caseNumbersAvailableForSending.Contains("CAS008"));
			Assert(!caseNumbersAvailableForSending.Contains("CAS001"));
			Assert(!caseNumbersAvailableForSending.Contains("CAS003"));
			Assert(!caseNumbersAvailableForSending.Contains("CAS005"));
			Assert(!caseNumbersAvailableForSending.Contains("CAS007"));
		}

		public void TestFieldReadOnly()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testwrapper = new MessageSendingObject(entryHeader);
			CombineAssertions(MessageSubTypeCodes.Codes.Original, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", true, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", true, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Change, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Cancellation, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", false, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", false, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Replace, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Replace;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = testInst.PK;
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(line);
			testInst.CEI_DateForDuty = ZDateTime.Now;
			testwrapper = new MessageSendingObject(entryHeader);
			CombineAssertions(MessageSubTypeCodes.Codes.Original, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", true, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", true, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Change, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", false, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", false, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Cancellation, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", false, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", false, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Replace, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Replace;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN", ZDateTime.Now);
			testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = testInst.PK;
			header = declaration.Invoices.AddNew();
			line = header.InvoiceLines.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(line);
			testInst.CEI_DateForDuty = ZDateTime.Now;
			testwrapper = new MessageSendingObject(entryHeader);
			CombineAssertions(MessageSubTypeCodes.Codes.Original, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", true, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", true, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Change, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Cancellation, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
			CombineAssertions(MessageSubTypeCodes.Codes.Replace, () =>
			{
				testwrapper.MessageType = MessageSubTypeCodes.Codes.Replace;
				AssertEquals("ShouldSendInfo", false, testwrapper.ShouldSendInfo.ReadOnly);
				AssertEquals("MessageTypeInfo", false, testwrapper.MessageTypeInfo.ReadOnly);
				AssertEquals("PaymentMethodInfo", true, testwrapper.PaymentMethodInfo.ReadOnly);
				AssertEquals("LocalReferenceNumberInfo", true, testwrapper.LocalReferenceNumberInfo.ReadOnly);
				AssertEquals("MovementReferenceNumberInfo", true, testwrapper.MovementReferenceNumberInfo.ReadOnly);
				AssertEquals("UniqueConsignmentReferenceNumberInfo", true, testwrapper.UniqueConsignmentReferenceNumberInfo.ReadOnly);
				AssertEquals("EntryStatusInfo", true, testwrapper.EntryStatusInfo.ReadOnly);
				AssertEquals("CustomsProcedureCodeInfo", true, testwrapper.CustomsProcedureCodeInfo.ReadOnly);
				AssertEquals("CaseNumberInfo", false, testwrapper.CaseNumberInfo.ReadOnly);
				AssertEquals("ChangeAcknowledgementIndicatorInfo", false, testwrapper.ChangeAcknowledgementIndicatorInfo.ReadOnly);
			});
		}

		public void TestMessageTypesList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInst.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			Factory.Save();
			CombineAssertions("no MRN", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testObject = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
				AssertEquals("test IMP", 4, testObject.MessageTypesList.Count);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				testObject = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
				AssertEquals("test EXP", 3, testObject.MessageTypesList.Count);
			});
			CombineAssertions("no MRN but for Entry outside CW1", () =>
			{
				testInst.CEI_DateForDuty = ZDateTime.Now;
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testObject = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
				AssertEquals("test IMP", 3, testObject.MessageTypesList.Count);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				testObject = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
				AssertEquals("test EXP", 2, testObject.MessageTypesList.Count);
			});
			CombineAssertions("with MRN not assessment Date", () =>
			{
				testInst.CEI_DateForDuty = ZDateTime.Empty;
				declaration.CustomsEntryHeaders[0].MovementReferenceNumberSetter("123", ZDateTime.Now);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testObject = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
				AssertEquals("test IMP", 2, testObject.MessageTypesList.Count);
				Assert("test IMP with an MRN number and a Status", !testObject.MessageTypesList.ContainsCode("REP"));
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				testObject = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
				AssertEquals("test EXP", 2, testObject.MessageTypesList.Count);
			});
		}

		public void TestVOCReason()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.ShouldSend = true;
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertEquals(true, testWrapper.VOCReasonInfo.ReadOnly);
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			AssertEquals(false, testWrapper.VOCReasonInfo.ReadOnly);
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			AssertEquals(false, testWrapper.VOCReasonInfo.ReadOnly);
			testWrapper.VOCReason = "AAA";
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
			AssertNullOrEmpty(testWrapper.VOCReason);
			testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
			testWrapper.VOCReason = "AAA";
			AssertEquals("AAA", testWrapper.VOCReason);
			testWrapper.ShouldSend = false;
			AssertNullOrEmpty(testWrapper.VOCReason);
		}

		public void TestVOCAfterValues()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = "ZAR";
			invHeader.JZ_IncoTerm = "FOB";
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_LinePrice = 200m;
			invLine.JI_ZZF_NKTaxType = "VAT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CustomsValueBefore = 110m;
			entryHeader.CIFValueBefore = 120m;
			entryHeader.CustomsDutyExcluding12BBefore = 30m;
			entryHeader.S1P2BDutyBefore = 40m;
			entryHeader.ValueAddedTaxBefore = 50m;
			entryHeader.PenaltyAmountBefore = 16m;
			entryHeader.ProvisionalPaymentAmountBefore = 18m;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine);
			entryLine.CL_CustomsValue = 210m;
			entryLine.Fees.AddOrUpdate("1P1", 60m);
			entryLine.Fees.AddOrUpdate("12B", 80m);
			entryLine.Fees.AddOrUpdate("VAT", 90m);
			entryLine.ProvisionalPayments.AddNew("FOR", "61.00");
			entryLine.ProvisionalPayments.AddNew("PPA", "81.00");
			entryLine.ProvisionalPayments.AddNew("XXR", "91.00");
			var tester = new MessageSendingObject(entryHeader);
			var testerAfter = tester as IVOCAfterValues;
			var testerBefore = tester as IVOCBeforeValues;
			CombineAssertions("Change Message", () =>
			{
				tester.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertEquals(210m, testerAfter.CustomsValue);
				AssertEquals(200m, testerAfter.CIFValue);
				AssertEquals(60m, testerAfter.CustomsDutyNoS1P2B);
				AssertEquals(80m, testerAfter.S1P2BDuty);
				AssertEquals(90m, testerAfter.ValueAddedTax);
				AssertEquals(61m, testerAfter.PenaltyAmount);
				AssertEquals(81m, testerAfter.ProvisionalPaymentAmount);
				AssertEquals(110m, testerBefore.CustomsValue);
				AssertEquals(120m, testerBefore.CIFValue);
				AssertEquals(30m, testerBefore.CustomsDutyNoS1P2B);
				AssertEquals(40m, testerBefore.S1P2BDuty);
				AssertEquals(50m, testerBefore.ValueAddedTax);
				AssertEquals(16m, testerBefore.PenaltyAmount);
				AssertEquals(18m, testerBefore.ProvisionalPaymentAmount);
			});
			CombineAssertions("Cancel Message", () =>
			{
				tester.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals(0m, testerAfter.CustomsValue);
				AssertEquals(0m, testerAfter.CIFValue);
				AssertEquals(0m, testerAfter.CustomsDutyNoS1P2B);
				AssertEquals(0m, testerAfter.S1P2BDuty);
				AssertEquals(0m, testerAfter.ValueAddedTax);
				AssertEquals(0m, testerAfter.PenaltyAmount);
				AssertEquals(0m, testerAfter.ProvisionalPaymentAmount);
				AssertEquals(110m, testerBefore.CustomsValue);
				AssertEquals(120m, testerBefore.CIFValue);
				AssertEquals(30m, testerBefore.CustomsDutyNoS1P2B);
				AssertEquals(40m, testerBefore.S1P2BDuty);
				AssertEquals(50m, testerBefore.ValueAddedTax);
				AssertEquals(16m, testerBefore.PenaltyAmount);
				AssertEquals(18m, testerBefore.ProvisionalPaymentAmount);
			});
			CombineAssertions("Do No Claim VAT Refund ticked Cancel Message", () =>
			{
				entryHeader.DoNotClaimVATRefund = true;
				tester.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals(0m, testerAfter.CustomsValue);
				AssertEquals(0m, testerAfter.CIFValue);
				AssertEquals(0m, testerAfter.CustomsDutyNoS1P2B);
				AssertEquals(0m, testerAfter.S1P2BDuty);
				AssertEquals(50m, testerAfter.ValueAddedTax);
				AssertEquals(0m, testerAfter.PenaltyAmount);
				AssertEquals(0m, testerAfter.ProvisionalPaymentAmount);
				AssertEquals(110m, testerBefore.CustomsValue);
				AssertEquals(120m, testerBefore.CIFValue);
				AssertEquals(30m, testerBefore.CustomsDutyNoS1P2B);
				AssertEquals(40m, testerBefore.S1P2BDuty);
				AssertEquals(50m, testerBefore.ValueAddedTax);
				AssertEquals(16m, testerBefore.PenaltyAmount);
				AssertEquals(18m, testerBefore.ProvisionalPaymentAmount);
			});
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				tester = new MessageSendingObject(entryHeader);
				entryHeader.DoNotClaimVATRefund = false;
				tester.MessageType = MessageSubTypeCodes.Codes.Original;
				testerAfter = tester;
				testerBefore = tester;
				CombineAssertions("RID Declaration", () =>
				{
					tester.DeclarationType = ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration;
					AssertEquals(0m, testerAfter.CustomsValue);
					AssertEquals(0m, testerAfter.CIFValue);
					AssertEquals(0m, testerAfter.CustomsDutyNoS1P2B);
					AssertEquals(0m, testerAfter.S1P2BDuty);
					AssertEquals(0m, testerAfter.ValueAddedTax);
					AssertEquals(0m, testerAfter.PenaltyAmount);
					AssertEquals(0m, testerAfter.ProvisionalPaymentAmount);
					AssertEquals(110m, testerBefore.CustomsValue);
					AssertEquals(120m, testerBefore.CIFValue);
					AssertEquals(30m, testerBefore.CustomsDutyNoS1P2B);
					AssertEquals(40m, testerBefore.S1P2BDuty);
					AssertEquals(50m, testerBefore.ValueAddedTax);
					AssertEquals(16m, testerBefore.PenaltyAmount);
					AssertEquals(18m, testerBefore.ProvisionalPaymentAmount);
				});
				CombineAssertions("RPD Declaration", () =>
				{
					tester.DeclarationType = ZA.Business.DeclarationTypeList.Codes.RegularProvisionalDeclaration;
					AssertEquals(0m, testerAfter.CustomsValue);
					AssertEquals(0m, testerAfter.CIFValue);
					AssertEquals(0m, testerAfter.CustomsDutyNoS1P2B);
					AssertEquals(0m, testerAfter.S1P2BDuty);
					AssertEquals(0m, testerAfter.ValueAddedTax);
					AssertEquals(0m, testerAfter.PenaltyAmount);
					AssertEquals(0m, testerAfter.ProvisionalPaymentAmount);
					AssertEquals(110m, testerBefore.CustomsValue);
					AssertEquals(120m, testerBefore.CIFValue);
					AssertEquals(30m, testerBefore.CustomsDutyNoS1P2B);
					AssertEquals(40m, testerBefore.S1P2BDuty);
					AssertEquals(50m, testerBefore.ValueAddedTax);
					AssertEquals(16m, testerBefore.PenaltyAmount);
					AssertEquals(18m, testerBefore.ProvisionalPaymentAmount);
				});
			}
		}

		public void TestIsDutiable()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_LinePrice = 200m;
			invLine.JI_ZZF_NKTaxType = UniversalReferenceConstants.TaxOrFeeTypeCode.VAT;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invLine);
			var sendingObject = new MessageSendingObject(entryHeader);
			AssertEquals(0.0m, sendingObject.AmountDueAfter);
			AssertEquals("Not deferrable when amount due is zero", false, sendingObject.IsDutiable);
			entryLine.CL_CustomsValue = 210m;
			entryLine.Fees.AddOrUpdate("1P1", 60m);
			entryLine.Fees.AddOrUpdate("12B", 80m);
			entryLine.Fees.AddOrUpdate("VAT", 90m);
			AssertEquals(230.0m, sendingObject.AmountDueAfter);
			AssertEquals("Deferrable when amount is above zero", true, sendingObject.IsDutiable);
		}

		public void TestPaymentMethodIsDeferOrVAT()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			AssertEquals("Not Set", false, sendingObject.PaymentMethodIsDeferOrVAT);
			entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Defer;
			AssertEquals("Defer", true, sendingObject.PaymentMethodIsDeferOrVAT);
			entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			AssertEquals("Cash", false, sendingObject.PaymentMethodIsDeferOrVAT);
			entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
			AssertEquals("VATOnly", true, sendingObject.PaymentMethodIsDeferOrVAT);
		}

		public void TestDeclarationType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			AssertEquals(MessageSendingObject.Schema.DeclarationTypeMaxLength, sendingObject.DeclarationTypeInfo.MaxLength);
			var resourceStringDataAttribute = (ResourceStringDataAttribute)sendingObject.DeclarationTypeInfo.PropertyDescriptor.GetAttributeFromMostSpecificComponentType(typeof(ResourceStringDataAttribute));
			AssertEquals("Declaration Type", resourceStringDataAttribute.Caption);
			AssertEquals("NPBO:Enterprise.Customs.ZA.Business.MessageSendingObject|DeclarationType", resourceStringDataAttribute.Key);
			var listAttribute = (ListAttribute)sendingObject.DeclarationTypeInfo.PropertyDescriptor.GetAttributeFromMostSpecificComponentType(typeof(ListAttribute));
			var typeOfMessageSendingObject = typeof(MessageSendingObject);
			var listPropertyInfo = typeOfMessageSendingObject.GetProperty(listAttribute.ListDataSourceMember);
			AssertNotNull(listPropertyInfo.GetValue(sendingObject));
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				foreach (CodeDescriptionPair pair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = pair.Code;
					if (pair.Code == ZAJobMessageTypeList.Codes.ExBond)
					{
						Assert(sendingObject.DeclarationTypeInfo.ReadOnly);
					}
					else
					{
						Assert(!sendingObject.DeclarationTypeInfo.ReadOnly);
					}
				}
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				foreach (CodeDescriptionPair pair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = pair.Code;
					Assert(sendingObject.DeclarationTypeInfo.ReadOnly);
				}
			}
		}

		public void TestDeclarationType_Defaulting()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				foreach (CodeDescriptionPair pair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = pair.Code;
					var sendingObject = new MessageSendingObject(entryHeader);
					AssertEquals(DeclarationTypeList.Codes.RegularCompleteDeclarationDefault, sendingObject.DeclarationType);
				}
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				foreach (CodeDescriptionPair pair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = pair.Code;
					var sendingObject = new MessageSendingObject(entryHeader);
					AssertEquals(DeclarationTypeList.Codes.RegularCompleteDeclarationDefault, sendingObject.DeclarationType);
				}
			}
		}

		public void TestDeclarationType_Defaulting_HasPreviousAcceptedOrginalMessage()
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
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				AssertEquals("RCD", new MessageSendingObject(entryHeader).DeclarationType);
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				foreach (CodeDescriptionPair messageTypePair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = messageTypePair.Code;
					switch (messageTypePair.Code)
					{
						case ZAJobMessageTypeList.Codes.ExBond:
							AssertEquals("RCD", new MessageSendingObject(entryHeader).DeclarationType);
							break;
						default:
							{
								foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
								{
									cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("BGM+929+00626166CLP20160426000199::00002+9'", $"BGM+929:::{declarationTypePair.Code}+00626166CLP20160426000199::00002+9'");
									cusdecMessage.ResetDeclarationType();
									cusdecMessage.ResetCUSDECHelper();
									var message = messageTypePair.Code + "|" + declarationTypePair.Code;
									switch (declarationTypePair.Code)
									{
										case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
											AssertEquals(message, "RCD", new MessageSendingObject(entryHeader).DeclarationType);
											break;
										case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
										case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
											AssertEquals(message, "RSD", new MessageSendingObject(entryHeader).DeclarationType);
											break;
										default:
											AssertEquals(message, "RCD", new MessageSendingObject(entryHeader).DeclarationType);
											break;
									}
								}

								break;
							}
					}
				}
			}
		}

		public void TestDeclarationType_UpdateMessageKeyFactorDeclarationType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				sendingObject.DeclarationType = "RID";
				AssertEquals("RCD", sendingObject.MessageKeyFactor.DeclarationType);
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				sendingObject.DeclarationType = "RID";
				AssertEquals("RID", sendingObject.MessageKeyFactor.DeclarationType);
			}
		}

		public void TestDeclarationTypeList()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				AssertEquals("RCD", sendingObject.DeclarationTypeList.CodesAsString);
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				foreach (CodeDescriptionPair messageTypePair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = messageTypePair.Code;
					switch (messageTypePair.Code)
					{
						case ZAJobMessageTypeList.Codes.ExBond:
							AssertEquals("RCD", sendingObject.DeclarationTypeList.CodesAsString);
							break;
						default:
							{
								foreach (CodeDescriptionPair entryTypePair in new MessageSubTypeCodes())
								{
									sendingObject.MessageType = entryTypePair.Code;
									var message = messageTypePair.Code + "|" + entryTypePair.Code;
									switch (entryTypePair.Code)
									{
										case Customs.Common.Shared.MessageSubTypeCodes.Codes.Original:
											AssertEquals(message, "RCD, RID, RPD", sendingObject.DeclarationTypeList.CodesAsString);
											break;
										default:
											AssertEquals(message, "RCD, RSD", sendingObject.DeclarationTypeList.CodesAsString);
											break;
									}
								}

								break;
							}
					}
				}
			}
		}

		public void TestDeclarationTypeList_HasPreviousAcceptedOrginalMessage()
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
			var sendingObject = new MessageSendingObject(entryHeader);
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				AssertEquals("RCD", sendingObject.DeclarationTypeList.CodesAsString);
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				foreach (CodeDescriptionPair messageTypePair in new ZAJobMessageTypeList())
				{
					declaration.JE_MessageType = messageTypePair.Code;
					switch (messageTypePair.Code)
					{
						case ZAJobMessageTypeList.Codes.ExBond:
							AssertEquals("RCD", sendingObject.DeclarationTypeList.CodesAsString);
							break;
						default:
							{
								foreach (CodeDescriptionPair entryTypePair in new MessageSubTypeCodes())
								{
									sendingObject.MessageType = entryTypePair.Code;
									foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
									{
										cusdecMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("BGM+929+00626166CLP20160426000199::00002+9'", $"BGM+929:::{declarationTypePair.Code}+00626166CLP20160426000199::00002+9'");
										cusdecMessage.ResetDeclarationType();
										cusdecMessage.ResetCUSDECHelper();
										var message = messageTypePair.Code + "|" + entryTypePair.Code + "|" + declarationTypePair.Code;
										switch (declarationTypePair.Code)
										{
											case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
												AssertEquals(message, "RCD", sendingObject.DeclarationTypeList.CodesAsString);
												break;
											case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
											case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
												AssertEquals(message, $"RSD, {declarationTypePair.Code}", sendingObject.DeclarationTypeList.CodesAsString);
												break;
											default:
												{
													switch (entryTypePair.Code)
													{
														case Customs.Common.Shared.MessageSubTypeCodes.Codes.Original:
															AssertEquals(message, "RCD, RID, RPD", sendingObject.DeclarationTypeList.CodesAsString);
															break;
														default:
															AssertEquals(message, "RCD, RSD", sendingObject.DeclarationTypeList.CodesAsString);
															break;
													}

													break;
												}
										}
									}
								}

								break;
							}
					}
				}
			}
		}

		public void TestDimaondLevyAmount()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invLine);
			entryLine1.ProvisionalPayments.AddNew("DLA", "111.00");
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invLine);
			entryLine2.ProvisionalPayments.AddNew("DLA", "222.00");
			var tester = new MessageSendingObject(entryHeader);
			AssertEquals("DLA Export:", 333m, tester.DiamondLevyAmount);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("DLA Import:", 0m, tester.DiamondLevyAmount);
		}

		public void TestIJobDeclarationSendingObjectWarehouseProvider()
		{
			var iJobDecSendingObj = GetNewBusinessObject() as IJobDeclarationSendingObjectWarehouseProvider;
			AssertNotNull("MessageSendingObject is a IJobDeclarationSendingObjectWarehouseProvider", iJobDecSendingObj);
		}

		public void TestGetMessageAction()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObj = new MessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				sendingObj.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertEquals("Original", MessageAction.Original, sendingObj.GetMessageAction());

				sendingObj.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals("Cancellation", MessageAction.Withdrawal, sendingObj.GetMessageAction());

				sendingObj.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertEquals("Change", MessageAction.Amendment, sendingObj.GetMessageAction());

				sendingObj.MessageType = "XXX";
				AssertEquals("Unknown", MessageAction.Amendment, sendingObj.GetMessageAction());
			});
		}

		public void TestShouldProcessWarehouse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObj = new MessageSendingObject(entryHeader);
			IJobDeclarationSendingObjectWarehouseProvider supporter = sendingObj;

			CombineAssertions(() =>
			{
				sendingObj.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertEquals("Orignal", true, supporter.ShouldProcessWarehouse);

				sendingObj.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				AssertEquals("Withdrawal & No WHS Status", false, supporter.ShouldProcessWarehouse);
				entryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
				AssertEquals("Withdrawal & With WHS Status", true, supporter.ShouldProcessWarehouse);

				entryHeader.CH_WarehouseTransactionStatus = "";
				sendingObj.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertEquals("Amendment & No WHS Status", false, supporter.ShouldProcessWarehouse);
				entryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;
				AssertEquals("Amendment & With WHS Status", true, supporter.ShouldProcessWarehouse);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return new MessageSendingObject(entryHeader);
		}

		protected override void SetUp()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);
			base.SetUp();
		}
	}
}
