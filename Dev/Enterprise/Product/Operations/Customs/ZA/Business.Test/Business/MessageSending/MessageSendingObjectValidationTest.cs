using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class MessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckDeclarationType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var sendingObject = new MessageSendingObject(entryHeader);
			sendingObject.MessageType = Customs.Common.Shared.MessageSubTypeCodes.Codes.Original;
			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(true))
			{
				sendingObject.DeclarationType = ZString.Empty;
				AssertHasMessageErrorContaining(sendingObject.DeclarationTypeInfo, MandatoryValidation.YouHaveNotEntered);
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					var declarationTypeCode = declarationTypePair.Code;
					sendingObject.DeclarationType = declarationTypeCode;
					switch (declarationTypeCode)
					{
						case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
							AssertHasMessageErrorContaining(sendingObject.DeclarationTypeInfo, ListValidation.InvalidCodeMessageError);
							break;
						case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
						case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							AssertHasMessageError(sendingObject.DeclarationTypeInfo, "Marks & Numbers is required for declaration types 'RID' and 'RPD'.");
							declaration.JE_MarksAndNumbers = "XXX";
							sendingObject.DeclarationType = declarationTypeCode;
							AssertNoNotifications(sendingObject.DeclarationTypeInfo);
							declaration.JE_MarksAndNumbers = ZString.Empty;
							break;
						case DeclarationTypeList.Codes.RegularCompleteDeclarationDefault:
							AssertNoNotifications(sendingObject.DeclarationTypeInfo);
							break;
					}
				}
			}

			using (TwoStepDeclarationHelper.TemporarilyEnableTwoStepClearing(false))
			{
				sendingObject.DeclarationType = ZString.Empty;
				AssertNoNotifications(sendingObject.DeclarationTypeInfo);
				foreach (CodeDescriptionPair declarationTypePair in new DeclarationTypeList())
				{
					var declarationTypeCode = declarationTypePair.Code;
					sendingObject.DeclarationType = declarationTypeCode;
					AssertNoNotifications(sendingObject.DeclarationTypeInfo);
				}
			}
		}

		public void TestCheckPaymentMethod()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_PaymentMethod = "D";
			var testMessage = entryHeader.Messages.AddNew();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageType = "RES";
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:00263869'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			var testWrapper = new MessageSendingObject(entryHeader);
			AssertEquals("D", testWrapper.PaymentMethod);
			testWrapper.Validation.ValidateAll();
			AssertHasMessageError(testWrapper.PaymentMethodInfo, "A Payment Method of Free is expected when there are no duties or taxes.");
			entryHeader.CH_PaymentMethod = "F";
			testWrapper = new MessageSendingObject(entryHeader);
			AssertEquals("F", testWrapper.PaymentMethod);
			testWrapper.Validation.ValidateAll();
			AssertNoMessageError(testWrapper.PaymentMethodInfo, "A Payment Method of Free is expected when there are no duties or taxes.");
			entryHeader.CH_PaymentMethod = "D";
			testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.DeclarationType = ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration;
			testWrapper.Validation.ValidateAll();
			AssertHasMessageError(testWrapper.PaymentMethodInfo, "Payment method must be Free for an incomplete declaration.");
			testWrapper.DeclarationType = ZA.Business.DeclarationTypeList.Codes.RegularProvisionalDeclaration;
			testWrapper.Validation.ValidateAll();
			AssertHasMessageError(testWrapper.PaymentMethodInfo, "Payment method must be Free for an incomplete declaration.");
			entryHeader.CH_PaymentMethod = "F";
			testWrapper = new MessageSendingObject(entryHeader);
			testWrapper.DeclarationType = ZA.Business.DeclarationTypeList.Codes.RegularIncompleteDeclaration;
			testWrapper.Validation.ValidateAll();
			AssertNoMessageError(testWrapper.PaymentMethodInfo, "Payment method must be Free for an incomplete declaration.");
			testWrapper.DeclarationType = ZA.Business.DeclarationTypeList.Codes.RegularProvisionalDeclaration;
			testWrapper.Validation.ValidateAll();
			AssertNoMessageError(testWrapper.PaymentMethodInfo, "Payment method must be Free for an incomplete declaration.");
		}

		public void TestShouldSendWithMissingLocalReferenceNumber_WhenSelected_ProduceValidationError()
		{
			var declaration = CreateTestJobDeclarationForLocalReferenceNumberTests();
			Factory.Save();
			CombineAssertions("ShouldSend selected with missing Local Reference Number on a message to be sent should produce a validation error", () =>
			{
				var tester = new MessageSendingObject(declaration.CustomsEntryHeaders[0])
				{ ShouldSend = true };
				Assert("ShouldSend is selected to the message", tester.ShouldSend);
				Assert("Local Reference Number field is read only", !tester.IsLRNEditable);
				Assert("Local Reference Number is empty", tester.LocalReferenceNumber.IsEmpty);
				AssertHasErrorContaining(tester.ShouldSendInfo, "Message should not be selected to send with missing Local Reference Number.\r\nPlease check if the Agent Code and/or the Customs Office is missing from the declaration.");
			});
		}

		public void TestShouldSendWithDuplicateUCR()
		{
			var declaration = CreateTestJobDeclarationForUCRTests();
			Factory.Save();
			var testWrapper = new MessageSendingObject(declaration.CustomsEntryHeaders[0]);
			testWrapper.ShouldSend = true;
			testWrapper.CaseNumber = "i123-321";
			testWrapper.MovementReferenceNumber = "MRN";
			CombineAssertions("ShouldSend selected with duplicate UCR with ORG message to be sent should produce a validation error", () =>
			{
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Replace;
				testWrapper.Validation.ValidateShouldSend();
				AssertNoErrorContaining(testWrapper.ShouldSendInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				testWrapper.Validation.ValidateShouldSend();
				AssertNoErrorContaining(testWrapper.ShouldSendInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.Validation.ValidateShouldSend();
				AssertNoErrorContaining(testWrapper.ShouldSendInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Undefined;
				testWrapper.Validation.ValidateShouldSend();
				AssertNoErrorContaining(testWrapper.ShouldSendInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				testWrapper.Validation.ValidateShouldSend();
				AssertHasErrorContaining(testWrapper.ShouldSendInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
				var entry = declaration.CustomsEntryHeaders[0];
				entry.LoadOrCreateUCRNumber("8ZA12345678COTH12345678901234M");
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				testWrapper.Validation.ValidateShouldSend();
				AssertNoErrorContaining(testWrapper.ShouldSendInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
			});
		}

		public void TestMissingLocalReferenceNumber_WhenEditable_ProduceValidationError()
		{
			var declaration = CreateTestJobDeclarationForLocalReferenceNumberTests();
			var entryInstrudction = declaration.CustomsEntryInstructions[0];
			entryInstrudction.CEI_Style = "40";
			entryInstrudction.CEI_DateForDuty = ZDateTime.Now;
			Factory.Save();
			CombineAssertions("Empty Local Reference Number on a message to be sent should produce a validation error", () =>
			{
				var testHeader = declaration.CustomsEntryHeaders[0];
				testHeader.CH_BGMReference = "Test header reference number";
				var tester = new MessageSendingObject(testHeader)
				{ ShouldSend = true };
				Assert("ShouldSend is selected to the message", tester.ShouldSend);
				Assert("Local Reference Number field is NOT read only", tester.IsLRNEditable);
				tester.LocalReferenceNumber = string.Empty;
				Assert("Local Reference Number is empty", tester.LocalReferenceNumber.IsEmpty);
				AssertHasErrorContaining(tester.LocalReferenceNumberInfo, "Please enter a Local Reference Number.");
			});
		}

		[TestDate(2016, 10, 1, 18, 00, 00)]
		public void TestCheckShouldSend()
		{
			var testHeader = Factory.NewWithValidTestData<CusEntryHeader>();
			var testMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2016, 10, 1, 17, 00, 00);
			testMessage.EM_ReceiveTransmit = ZAMessageForTest.Direction.Transmit;
			testMessage.MessageNumForTesting = "TESTMessage1";
			testHeader.Messages.Add(testMessage);
			testHeader.CH_BGMReference = "TESTHEADER1";
			Factory.Save();
			CombineAssertions("Registry Setting 5 min, last message 60 min ago", () =>
			{
				var tester = new MessageSendingObject(testHeader);
				tester.ShouldSend = false;
				AssertNoNotifications(tester.ShouldSendInfo);
				tester.ShouldSend = true;
				AssertNoNotifications(tester.ShouldSendInfo);
			});
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2016, 10, 1, 17, 58, 00);
			Factory.Save();
			CombineAssertions("Registry Setting 5 min, last message 2 min ago", () =>
			{
				var tester = new MessageSendingObject(testHeader);
				tester.ShouldSend = false;
				AssertNoNotifications(tester.ShouldSendInfo);
				tester.ShouldSend = true;
				AssertHasMessageErrorContaining(tester.ShouldSendInfo, "LRN TESTHEADER1 was last submitted at 2016-10-01 17:58:00 please wait till 2016-10-01 18:03:00 before submitting again.");
			});
			CombineAssertions("Registry Setting 1 min, last message 2 min ago", () =>
			{
				using (ZACustomsRegistry.Instance.MessageSendingInterval.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1))
				{
					var tester = new MessageSendingObject(testHeader);
					tester.ShouldSend = false;
					AssertNoNotifications(tester.ShouldSendInfo);
					tester.ShouldSend = true;
					AssertNoNotifications(tester.ShouldSendInfo);
				}
			});
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2016, 10, 1, 17, 59, 58);
			Factory.Save();
			CombineAssertions("Registry Setting 1 min, last message 2 sec ago", () =>
			{
				using (ZACustomsRegistry.Instance.MessageSendingInterval.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 1))
				{
					var tester = new MessageSendingObject(testHeader);
					tester.ShouldSend = false;
					AssertNoNotifications(tester.ShouldSendInfo);
					tester.ShouldSend = true;
					AssertHasMessageErrorContaining(tester.ShouldSendInfo, "LRN TESTHEADER1 was last submitted at 2016-10-01 17:59:58 please wait till 2016-10-01 18:00:58 before submitting again.");
				}
			});
			CombineAssertions("Registry Setting 0 min, means no checking against time interval", () =>
			{
				using (ZACustomsRegistry.Instance.MessageSendingInterval.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 0))
				{
					var tester = new MessageSendingObject(testHeader);
					tester.ShouldSend = false;
					AssertNoNotifications(tester.ShouldSendInfo);
					tester.ShouldSend = true;
					AssertNoNotifications(tester.ShouldSendInfo);
				}
			});
		}

		public void TestCheckCaseNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testMessage = entryHeader.Messages.AddNew();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageType = "RES";
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:00263869'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			var testWrapper = new MessageSendingObject(entryHeader);
			CombineAssertions(() =>
			{
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = false;
				testWrapper.CaseNumber = string.Empty;
				testWrapper.Validation.ValidateAll();
				AssertNoNotifications(testWrapper.CaseNumberInfo);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = true;
				testWrapper.CaseNumber = string.Empty;
				testWrapper.Validation.ValidateAll();
				AssertHasMessageErrorContaining(testWrapper.CaseNumberInfo, "Case Number is required when Amendment Notification is received.");
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				testWrapper.ShouldSend = true;
				testWrapper.CaseNumber = string.Empty;
				testWrapper.Validation.ValidateAll();
				AssertNoNotifications(testWrapper.CaseNumberInfo);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = true;
				testWrapper.CaseNumber = "123321";
				testWrapper.Validation.ValidateAll();
				AssertNoNotifications(testWrapper.CaseNumberInfo);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = true;
				testWrapper.CaseNumber = "i123-321";
				testWrapper.Validation.ValidateAll();
				AssertHasMessageErrorContaining(testWrapper.CaseNumberInfo, "The Case Number can only be alphanumeric.");
			});
		}

		public void TestCheckMessageType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testMessage = entryHeader.Messages.AddNew();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageType = "RES";
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:00263869'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			var testWrapper = new MessageSendingObject(entryHeader);
			CombineAssertions(() =>
			{
				testWrapper.ShouldSend = false;
				testWrapper.MessageType = string.Empty;
				AssertNoNotifications(testWrapper.MessageTypeInfo);
				testWrapper.ShouldSend = false;
				testWrapper.MessageType = "XXX";
				AssertNoNotifications(testWrapper.MessageTypeInfo);
				testWrapper.ShouldSend = false;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertNoNotifications(testWrapper.MessageTypeInfo);
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = string.Empty;
				AssertHasErrorContaining(testWrapper.MessageTypeInfo, MandatoryValidation.YouHaveNotEntered);
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = "XXX";
				AssertHasMessageErrorContaining(testWrapper.MessageTypeInfo, ListValidation.InvalidCodeMessageError);
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertNoNotifications(testWrapper.MessageTypeInfo);
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertHasMessageErrorContaining(testWrapper.MessageTypeInfo, MessageSendingObjectValidation.MRNIsRequiredForVOCMessage);
			});
			CombineAssertions("Cancellation vs. Payment Method", () =>
			{
				entryHeader.MovementReferenceNumberSetter("JSA201601011234567", new ZDateTime(2016, 1, 1));
				entryHeader.CH_PaymentMethod = ZString.Empty;
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				testWrapper.Validation.ValidateMessageType();
				AssertNoNotifications(testWrapper.MessageTypeInfo);
				entryHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Cancellation;
				testWrapper.Validation.ValidateMessageType();
				AssertHasWarningContaining(testWrapper.MessageTypeInfo, MessageSendingObjectValidation.PaymentMethodForcedToFreeForCancellation("C"));
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				AssertNoNotifications(testWrapper.MessageTypeInfo);
			});
			CombineAssertions("Replacement not allowed for EXW", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Replace;
				AssertHasMessageError(testWrapper.MessageTypeInfo, MessageSendingObjectValidation.ReplacementEntryIsNotAllowedForExBond);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				AssertNoMessageError(testWrapper.MessageTypeInfo, MessageSendingObjectValidation.ReplacementEntryIsNotAllowedForExBond);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				testWrapper.ShouldSend = true;
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Replace;
				AssertNoMessageError(testWrapper.MessageTypeInfo, MessageSendingObjectValidation.ReplacementEntryIsNotAllowedForExBond);
			});
		}

		public void TestCheckChangeAcknowledgementIndicator()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var testMessage = entryHeader.Messages.AddNew();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_MessageType = "RES";
			testMessage.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00469468JSA20160426263869:0'DTM+132:20160429:102'DTM+202:20160426:102'TDT+20+SQ478+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A1::ZZZ'GIS+26:120:ZZZ:N'NAD+AG+00469468'RFF+BH:00469468MGLSHA160113'DTM+137:20160423:102'RFF+AAS:618-98921841'DTM+137:20160423:102'RFF+ABT:JSA201604265109719'DTM+137:20160427:102'RFF+ACD:00263869'RFF+AAV:199941811'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:5140'CNT+7:60.00'CNT+11:6'UNT+25+1'";
			var testWrapper = new MessageSendingObject(entryHeader);
			CombineAssertions(() =>
			{
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = false;
				testWrapper.ChangeAcknowledgementIndicator = string.Empty;
				testWrapper.Validation.ValidateAll();
				AssertNoNotifications(testWrapper.ChangeAcknowledgementIndicatorInfo);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = true;
				testWrapper.ChangeAcknowledgementIndicator = string.Empty;
				testWrapper.Validation.ValidateAll();
				AssertHasMessageErrorContaining(testWrapper.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.YouHaveNotEntered);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Original;
				testWrapper.ShouldSend = true;
				testWrapper.ChangeAcknowledgementIndicator = string.Empty;
				testWrapper.Validation.ValidateAll();
				AssertNoNotifications(testWrapper.ChangeAcknowledgementIndicatorInfo);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = true;
				testWrapper.ChangeAcknowledgementIndicator = "1";
				testWrapper.Validation.ValidateAll();
				AssertNoNotifications(testWrapper.ChangeAcknowledgementIndicatorInfo);
				testWrapper.MessageType = MessageSubTypeCodes.Codes.Change;
				testWrapper.ShouldSend = true;
				testWrapper.ChangeAcknowledgementIndicator = "12";
				testWrapper.Validation.ValidateAll();
				AssertHasMessageErrorContaining(testWrapper.ChangeAcknowledgementIndicatorInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckMovementReferenceNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = inst.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			declaration.DoMerge();
			var header = declaration.CustomsEntryHeaders[0];
			var testSendingObject = new MessageSendingObject(header);
			AssertEquals(string.Empty, testSendingObject.MovementReferenceNumber);
			AssertEquals(true, testSendingObject.MovementReferenceNumberInfo.ReadOnly);
			AssertEquals("ORG", testSendingObject.MessageType);
			testSendingObject.Validation.ValidateMovementReferenceNumber();
			AssertNoMessageErrors(testSendingObject.MovementReferenceNumberInfo);
			testSendingObject.MessageType = "CHG";
			AssertEquals("CHG", testSendingObject.MessageType);
			testSendingObject.Validation.ValidateMovementReferenceNumber();
			AssertHasMessageErrorContaining(testSendingObject.MovementReferenceNumberInfo, MessageSendingObjectValidation.MRNIsRequiredForReplacementMessage);
			inst.CEI_DateForDuty = ZDateTime.Now;
			testSendingObject = new MessageSendingObject(header);
			AssertEquals("CHG", testSendingObject.MessageType);
			testSendingObject.Validation.ValidateMovementReferenceNumber();
			AssertHasMessageErrorContaining(testSendingObject.MovementReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			testSendingObject.MessageType = "REP";
			AssertEquals("REP", testSendingObject.MessageType);
			testSendingObject.Validation.ValidateMovementReferenceNumber();
			AssertHasMessageErrorContaining(testSendingObject.MovementReferenceNumberInfo, MessageSendingObjectValidation.MRNIsRequiredForReplacementMessage);
		}

		JobDeclaration CreateTestJobDeclarationForUCRTests()
		{
			var declarationWithDuplicatedUCR = Factory.New<JobDeclaration>();
			declarationWithDuplicatedUCR.JE_DeclarationReference = "JOB ONE";
			var entryWithDuplicatedUCR = declarationWithDuplicatedUCR.CustomsEntryHeaders.AddNew();
			entryWithDuplicatedUCR.LoadOrCreateUCRNumber("8ZA12345678COTH12345678901234S");
			var declarationWithDuplicatedUCR2 = Factory.New<JobDeclaration>();
			declarationWithDuplicatedUCR2.JE_DeclarationReference = "JOB TWO";
			var entryWithDuplicatedUCR2 = declarationWithDuplicatedUCR2.CustomsEntryHeaders.AddNew();
			entryWithDuplicatedUCR2.LoadOrCreateUCRNumber("8ZA12345678COTH12345678901234M");
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.LoadOrCreateUCRNumber("8ZA12345678COTH12345678901234S");
			entry.CH_BGMReference = "LRN";
			entry.MovementReferenceNumberSetter("JSA201601011234567", new ZDateTime(2016, 1, 1));
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "40";
			entryInstruction.CEI_DateForDuty = ZDateTime.Now;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			declaration.DoMerge();
			var testMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2017, 03, 23, 10, 00, 00);
			testMessage.EM_ReceiveTransmit = ZAMessageForTest.Direction.Transmit;
			testMessage.MessageNumForTesting = "TESTMessage1";
			var testHeader = declaration.CustomsEntryHeaders[0];
			testHeader.Messages.Add(testMessage);
			return declaration;
		}

		JobDeclaration CreateTestJobDeclarationForLocalReferenceNumberTests()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstrudction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstrudction.CEI_Style = "40";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstrudction.PK;
			invoiceLine.JI_Procedure = invoiceLine.EntryInstruction.CEI_Style + "00";
			declaration.DoMerge();
			var testMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2017, 03, 23, 10, 00, 00);
			testMessage.EM_ReceiveTransmit = ZAMessageForTest.Direction.Transmit;
			testMessage.MessageNumForTesting = "TESTMessage1";
			var testHeader = declaration.CustomsEntryHeaders[0];
			testHeader.Messages.Add(testMessage);
			return declaration;
		}
	}
}
