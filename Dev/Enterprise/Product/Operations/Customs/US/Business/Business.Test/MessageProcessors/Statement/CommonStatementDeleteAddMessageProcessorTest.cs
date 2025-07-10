using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public abstract class CommonStatementDeleteAddMessageProcessorTest<THBlock, TH1Block, TH2Block, TI7501StatusBlock, T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessorTest<T, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where THBlock : MessageBlock, IStatementUpdateInputHBlock
			where TH1Block : MessageBlock, IStatementUpdateOutputH1Block
			where TH2Block : MessageBlock, IStatementUpdateAdditionalOutput
			where TI7501StatusBlock : MessageBlock, I7501Status
			where T : CommonStatementDeleteAddMessageProcessor<THBlock, TH1Block, TH2Block, TI7501StatusBlock, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
			where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
			where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
			where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			DoSetup();
			dec.JE_GB = newBranch.PK;

			transmittedMessage.EM_MessageText = TransmittedMessageText;
			transmittedMessage.EM_GB = newBranch.PK;
			responseMessage.EM_MessageText = EndToEndCoreResponseMessage();

			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statementHeader.B2_PrintDate = new ZDateTime(2007, 9, 19);
			Factory.Save();
			dec.Logs.GetAllLogs().Load();
			AssertNull(dec.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ResponseMessageType));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);
			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkUniqueID);

			var declarationLoaded = statementHeaderLoaded.StatementLines[0].Declaration;
			AssertEquals("StatementHeader.B2_Status is updated", StatementHeaderStatusList.Codes.Preliminary, statementHeaderLoaded.B2_Status);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.Active, statementHeaderLoaded.StatementLines[0].B3_Status);

			AssertNotEquals("It should NOT be updated to what is returned in the response when failed", PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, declarationLoaded.US_PaymentType);
			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);
			AssertNotEquals("It should NOT be updated to what is returned in the response when failed", new ZDateTime(2007, 9, 19), declarationLoaded.US_PreliminaryStatementPrintDate);

			AssertNotNull("An email with subject containing 'Statement Delete/Add' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add"); })));
			dec.Logs.GetAllLogs().Load();
			AssertNotNull(dec.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ResponseMessageType));

			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(transmittedMessage.Branch.PK, dec.Branch.PK);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		protected virtual ZString EndToEndCoreResponseMessage()
		{
			return string.Format(
				"B018888XJ5{0}                                               ~000771              " +
				"H18888XJ5 10001127VAWPRELIMINARY STMT ALREADY PRODUCED        6091907           " +
				"Y  8888XJ5{0}00001                                                               ", ResponseMessageType);
		}

		public void TestProcessToDetermineNotTrueFailure()
		{
			DoSetup();
			dec.JE_GB = GlbBranch.CurrentBranch.PK;

			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageText = string.Format("B018888XJ5{0}                                               30449                H8888XJ5 700078811                                                              Y  8888XJ5{0}00001", TransmittedMessageType);
			transmittedMessage.EM_MessageNum = "30449";
			transmittedMessage.EM_LinkedObject = dec;
			transmittedMessage.EM_GB = dec.Branch.PK;

			responseMessage.EM_MessageText = ProcessToDetermineNotTrueFailure();
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "30449";

			dec.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			dec.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 3, 18);
			dec.US_PeriodicStatementMM = "04";
			dec.ImportEntryNumber = "70007881";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);

			//re-load declaration
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			var entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);

			AssertEquals("message is linked", declaration.PK, responseMessageLoaded.EM_LinkedObject.PK);

			AssertEquals("entry header should have been updated with the data", PaymentTypeList.Codes.IndividualBasis, entryHeader.US_PaymentType);
			AssertEquals("", entryHeader.US_PeriodicStatementMM);
			AssertEquals(ZDateTime.Empty, entryHeader.US_PreliminaryStatementPrintDate);

			AssertNotNull("An email with subject containing 'Statement Delete/Add' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add"); })));
			EmailDef emailGenerated = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("email subject should not have FAILURE", "Statement Delete/Add Response for Job B00001000/ Entry XJ5-7000788-1", emailGenerated.Subject);
			AssertEquals("User should be advised entry has been updated", true, emailGenerated.Body.Contains("The entry, 70007881 has been updated with the data above where applicable."));
		}

		protected virtual ZString ProcessToDetermineNotTrueFailure()
		{
			return string.Format("B018888XJ5{0}                                               30449                H18888XJ5 7000788157FSUMM REMVD FR STMT, DOCS NOW REQD        1      B00151238  Y  8888XJ5{0}00001", ResponseMessageType);
		}

		public void TestProcessForSuccessfulResponse()
		{
			DoSetup();

			AssertEquals("Pre-condition - Declaration Preliminary Statement Print Date", new ZDateTime(2007, 9, 17), dec.US_PreliminaryStatementPrintDate);

			transmittedMessage.EM_MessageText = string.Format(
"B018888XJ5{0}                                               ~000771              " +
"H8888XJ5 100011273092207                                                        " +
"Y  8888XJ5{0}00001                                                               ", TransmittedMessageType);

			responseMessage.EM_MessageText = ProcessForSuccessfulResponseMessage();

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);
			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkUniqueID);
			AssertEquals("StatementHeader.B2_Status is updated", StatementHeaderStatusList.Codes.Deleted, statementHeaderLoaded.B2_Status);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.Deleted, statementHeaderLoaded.StatementLines[0].B3_Status);

			var declarationLoaded = statementHeaderLoaded.StatementLines[0].Declaration;

			AssertEquals("entry header should have been updated with the data", PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, declarationLoaded.US_PaymentType);
			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);
			AssertEquals(new ZDateTime(2007, 9, 20), declarationLoaded.US_PreliminaryStatementPrintDate);

			//re-load declaration
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals("Declaration Preliminary Statement Print Date should be updated to response value", new ZDateTime(2007, 9, 20), declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("Declaration PSDAccepted should be updated from the last message we sent to customs ", new ZDateTime(2007, 9, 22), declaration.US_PSDAccepted);

			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);

			AssertNotNull("An email with subject containing 'Statement Delete/Add' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add"); })));
		}

		protected virtual ZString ProcessForSuccessfulResponseMessage()
		{
			return string.Format(
				"B018888XJ5{0}                                               ~000771              " +
				"H18888XJ5 100011272GBDATA REPLACED AS REQUESTED               3092007           " +
				"Y  8888XJ5{0}00001                                                               ", ResponseMessageType);
		}

		public void TestProcessErrors()
		{
			DoSetup();

			AssertEquals("Pre-condition - Declaration Preliminary Statement Print Date", new ZDateTime(2007, 9, 17), dec.US_PreliminaryStatementPrintDate);
			statementHeader.StatementLines[0].B3_Status = StatementLineStatusList.Codes.DeletionPending;

			transmittedMessage.EM_MessageText = string.Format(
"B018888XJ5{0}                                               ~000771              " +
"H8888XJ5 100011277092410                                                        " +
"Y  8888XJ5{0}00001                                                               ", TransmittedMessageType);

			responseMessage.EM_MessageText = ProcessErrorsResponseMessage();

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);
			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkUniqueID);
			AssertEquals("Declaration Preliminary Statement Print Date should remain same", new ZDateTime(2007, 9, 17), dec.US_PreliminaryStatementPrintDate);

			AssertEquals("StatementLine.B3_Status is NOT deleted", StatementLineStatusList.Codes.Active, statementHeaderLoaded.StatementLines[0].B3_Status);

			AssertNotNull("An email with subject containing 'Statement Delete/Add' with failure should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add Response (Failure)"); })));
		}

		protected virtual ZString ProcessErrorsResponseMessage()
		{
			return string.Format(
				"B012704836{0}                                  5501836  1   ~000771              " +
				"H18888XJ5 10001127HP8PERIODIC MONTH INVALID                   7092410S00002189  " +
				"H28888XJ5 100011272710260G4500008701951                                         " +
				"Y  2704836{0}00002                                                               ", ResponseMessageType);
		}

		public void TestProcessForSuccessfulResponse_ChangeToPayType1()
		{
			DoSetup();

			dec.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			dec.US_PeriodicStatementMM = "01";
			AssertEquals("Pre-condition", new ZDateTime(2007, 9, 17), dec.US_PreliminaryStatementPrintDate);

			//these samples are actual samples from live customer (only change B,Y, entryfiler and entry number)
			transmittedMessage.EM_MessageText = string.Format(
"B018888XJ5{0}                                               ~000771              " +
"H8888XJ5 100011271                                                              " +
"Y  8888XJ5{0}00001                                                               ", TransmittedMessageType);

			responseMessage.EM_MessageText = ProcessForSuccessfulResponse_ChangeToPayType1ResponseMessage();

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var statementHeaderLoaded = newFactory.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("Status is Deleted", StatementHeaderStatusList.Codes.Deleted, statementHeaderLoaded.B2_Status);
			AssertEquals("Status is Deleted", StatementLineStatusList.Codes.Deleted, statementHeaderLoaded.StatementLines[0].B3_Status);

			var declarationLoaded = statementHeaderLoaded.StatementLines[0].Declaration;

			AssertEquals("entry header updated", PaymentTypeList.Codes.IndividualBasis, declarationLoaded.US_PaymentType);
			AssertEquals("", declarationLoaded.US_PeriodicStatementMM);
			AssertEquals(ZDateTime.Empty, declarationLoaded.US_PreliminaryStatementPrintDate);

			//re-load declaration
			var declaration = newFactory.Load<JobDeclaration>(dec.PK);
			AssertEquals("Set to empty for pay type 1", ZDateTime.Empty, declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("Set to empty for pay type 1", "", declaration.US_PeriodicStatementMM);
			AssertEquals(PaymentTypeList.Codes.IndividualBasis, declaration.US_PaymentType);
		}

		protected virtual ZString ProcessForSuccessfulResponse_ChangeToPayType1ResponseMessage()
		{
			return string.Format(
				"B018888XJ5{0}                                               ~000771              " +
				"H18888XJ5 100011272GBDATA REPLACED AS REQUESTED               1061609B00001259  " +
				"H18888XJ5 10001127470916721400000000000                                         " +
				"Y  8888XJ5{0}00001                                                               ", ResponseMessageType);
		}

		public void TestBranchCodeRequiredRejectionIsRecognised()
		{
			var statement = Factory.New<CusStatementHeader>();

			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "01149253";
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			transmittedMessage.EM_MessageText = string.Format(
"B018888XJ5{0}                                               ~000771              " +
"H8888XJ5 011492532080112                                                        " +
"Y  8888XJ5{0}00001                                                               ", TransmittedMessageType);

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_Status = EDIMessage.Status.Queued;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageText = BranchCodeRequiredRejectionIsRecognisedResponseMessage();

			Factory.Save();

			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			statementLine.Reload();
			AssertEquals(StatementLineStatusList.Codes.Active, statementLine.B3_Status);
		}

		protected virtual ZString BranchCodeRequiredRejectionIsRecognisedResponseMessage()
		{
			return string.Format(
				"B012704836{0}                                  5501836  1   ~000771              " +
				"H18888XJ5 01149253C04BRANCH CODE REQUIRED                     2080112000114925  " +
				"H28888XJ5 01149253371220702600000030408                                         " +
				"Y  2704836{0}00002                                                               ", ResponseMessageType);
		}

		public void TestResponseForReconciliation()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.US_SchDEntry = "3501";
			reconDeclaration.US_EntryFilerCode = "SV9";
			reconDeclaration.ReconEntry.GetEntry().EntryNumber = "70032279";

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			transmittedMessage = mock.Object;
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = "HYEDUSCMT_147299";
			reconDeclaration.ReconEntry.Messages.Add(transmittedMessage);
			transmittedMessage.EM_MessageText = string.Format("B013501SV9{0}                                  3910SV9  1   HYEDUSCMT_147299     H3501SV9 70032279206181312                                                      Y  3501SV9{0}00001", TransmittedMessageType);

			responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_Status = MQEDIMessage.Status.Queued;
			responseMessage.EM_MessageNum = "HYEDUSCMT_147299";
			responseMessage.EM_MessageText = string.Format("B013501SV9{0}                                  3910SV9  1   HYEDUSCMT_147299     H13501SV9 70032279355ENTRY NOT FOUND                                            Y  3501SV9{0}00001", ResponseMessageType);

			var statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_EntryFilerCode = "SV9";
			statementHeader.B2_ProcessPort = "3501";
			statementHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;

			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryNum = "70032279";
			statementLine.B3_EntryFilerCode = "SV9";
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			Factory.Save();

			var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(reconDeclaration);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);
			AssertNotNull(responseMessageLoaded.EM_LinkedObject);
			AssertEquals("Message is linked to recon entry", reconDeclaration.ReconEntry.PK, responseMessageLoaded.EM_LinkUniqueID);

			var declarationLoaded = factory2.Load<JobDeclaration>(reconDeclaration.ReconWrappedJobDeclaration.PK);
			var reconDeclarationLoaded = new ReconDeclaration(declarationLoaded);

			AssertEquals(2, reconDeclarationLoaded.Messages.Count);

			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);
			AssertEquals("Should be Recon declaration", reconDeclaration.PK, statementHeaderLoaded.StatementLines[0].Declaration.PK);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Statement Delete/Add"));
			AssertNotNull(email);
			AssertContains(url, email.Body);
			reconDeclaration.Logs.GetAllLogs().Load();
			AssertNotNull(reconDeclaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ResponseMessageType));
		}

		public void TestResponseForEntrySDCR()
		{
			DoSetup();
			dec.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 7, 18);
			dec.US_PeriodicStatementMM = "07";
			dec.ImportEntryNumber = "70004003";

			Factory.Save();

			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageText = string.Format("B018888XJ5{0}                                               16632                H8888XJ5 700040032072208  08                                                    Y  8888XJ5{0}00001", TransmittedMessageType);
			transmittedMessage.EM_MessageNum = "16632";
			transmittedMessage.EM_LinkedObject = dec;

			responseMessage.EM_MessageText = ResponseForEntrySDCRResponseMessage();
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "16632";

			Factory.Save();

			AssertEquals("Pre-condition - Declaration Preliminary Statement Print Date", new ZDateTime(2008, 7, 18), dec.US_PreliminaryStatementPrintDate);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);

			//re-load entry & declaration
			var entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkedObject.PK);
			AssertEquals("entry header should have been updated with the data", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, entryHeader.US_PaymentType);

			AssertEquals(new ZDateTime(2008, 07, 22), entryHeader.US_PreliminaryStatementPrintDate);
			AssertEquals("Declaration Preliminary Statement Print Date should be updated to response value", entryHeader.US_PreliminaryStatementPrintDate, declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("Declaration Periodic Statement Month should be updated to response value", "08", entryHeader.US_PeriodicStatementMM);
			AssertNotNull("An email with subject containing 'Statement Delete/Add' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add"); })));
		}

		protected virtual ZString ResponseForEntrySDCRResponseMessage()
		{
			return string.Format("B018888XJ5{0}                                               16632                H18888XJ5 700040032GBDATA REPLACED AS REQUESTED               2072208B00150669  Y  8888XJ5{0}00001", ResponseMessageType);
		}

		public void TestResponseForEntrySDCRWhenStatementFound()
		{
			DoSetup();

			dec.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entry2 = dec.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.EntryNumber = "70004003";
			Factory.Save();

			var statementLine2 = statementHeader.StatementLines.AddNew();
			statementLine2.B3_EntryFilerCode = "XJ5";
			statementLine2.B3_EntryNum = "70004003";
			statementLine2.B3_EntryProcessPort = "8888";
			AssertEquals("Entry in statement line", entry2.Declaration, statementLine2.Declaration);
			AssertEquals("pre-condition: statement lines expected", 2, statementHeader.StatementLines.Count);

			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageText = string.Format("B018888XJ5{0}                                               16632                H8888XJ5 700040032072208                                                        Y  8888XJ5{0}00001", TransmittedMessageType);
			transmittedMessage.EM_MessageNum = "16632";
			transmittedMessage.EM_LinkedObject = dec;

			responseMessage.EM_MessageText = ResponseForEntrySDCRWhenStatementFoundResponseMessage();
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "16632";
			responseMessage.EM_Status = EDIMessage.Status.Queued;

			dec.US_PreliminaryStatementPrintDate = new ZDateTime(2008, 7, 18);
			Factory.Save();

			AssertEquals("Pre-condition - Declaration Preliminary Statement Print Date", new ZDateTime(2008, 7, 18), dec.US_PreliminaryStatementPrintDate);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var responseMessageLoaded = factory2.Load<MQEDIMessage>(responseMessage.PK);

			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.Deleted, statementHeaderLoaded.StatementLines[1].B3_Status);

			//re-load entry & declaration
			var entryHeader = new BusinessObjectFactory().Load<CusEntryHeader>(entry2.PK);
			var declaration = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);

			AssertEquals("message is linked", dec.PK, responseMessageLoaded.EM_LinkedObject.PK);
			AssertEquals("entry header should have been updated with the data", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, entryHeader.US_PaymentType);
			AssertEquals("", entryHeader.US_PeriodicStatementMM);
			AssertEquals(new ZDateTime(2008, 07, 22), entryHeader.US_PreliminaryStatementPrintDate);

			AssertEquals("Declaration Preliminary Statement Print Date should be updated to response value", entryHeader.US_PreliminaryStatementPrintDate, declaration.US_PreliminaryStatementPrintDate);
			AssertEquals("", entryHeader.US_PeriodicStatementMM);
			AssertNotNull("An email with subject containing 'Statement Delete/Add' should have been sent.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Statement Delete/Add"); })));
		}

		protected virtual ZString ResponseForEntrySDCRWhenStatementFoundResponseMessage()
		{
			return string.Format("B018888XJ5{0}                                               16632                H18888XJ5 700040032GBDATA REPLACED AS REQUESTED               2072208B00150669  Y  8888XJ5{0}00001", ResponseMessageType);
		}

		public void TestIsSTUMessageAndAcceptedFor96UFailure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00025277";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "00025277";
			statementLine.B3_EntryProcessPort = "8888";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			AssertEquals("Declaration in statement line", declaration, statementLine.Declaration);

			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			transmittedMessage = mock.Object;
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_MessageText = string.Format(
"B01270498G{0}                                  520198G  1   WHBIYHIYH_6984       " +
"H2704XJ5 000252777030114  02                                                    " +
"Y  270498G{0}00001                                                               ", TransmittedMessageType);

			responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_Status = "QUE";
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_MessageText = IsSTUMessageAndAcceptedFor96UFailureResponseMessage();

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var factory2 = new BusinessObjectFactory();

			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);
			AssertEquals("STU failed. Should not be deleted", "PRE", statementHeaderLoaded.B2_Status);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.Active, statementHeaderLoaded.StatementLines[0].B3_Status);
		}

		protected virtual ZString IsSTUMessageAndAcceptedFor96UFailureResponseMessage()
		{
			return string.Format(
				"B01270498G{0}                                  520198G  1   WHBIYHIYH_6984       " +
				"H12704XJ5 0002527796UDATE IS WEEKEND DAY                      7030114B00002545  " +
				"H22704XJ5 000252772714014BVD00007264707                                         " +
				"Y  270498G{0}00002                                                               ", ResponseMessageType);
		}

		#region Implementation

		void DoSetup()
		{
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			dec.US_PreliminaryStatementPrintDate = new ZDateTime(2007, 9, 17);
			dec.ImportEntryNumber = "10001127";

			entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "10001127";
			Factory.Save();

			statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "10001127";
			statementLine.B3_EntryProcessPort = "8888";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			AssertEquals("Declaration in statement line", entry.Declaration, statementLine.Declaration);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			transmittedMessage = mock.Object;
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = TransmittedMessageType;
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_MessageText = TransmittedMessageText;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			dec.Messages.Add(transmittedMessage);

			responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ResponseMessageType;
			responseMessage.EM_MessageNum = "~000771";
			dec.Messages.Add(responseMessage);

			Factory.Save();
		}

		protected virtual ZString TransmittedMessageText
		{
			get { return string.Format("B018888XJ5{0}                                               ~000771              H8888XJ5 100011276091907                                                        Y  8888XJ5{0}00001", TransmittedMessageType); }
		}

		JobDeclaration dec;
		CusEntryHeader entry;

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;

		MQEDIMessage transmittedMessage;
		MQEDIMessage responseMessage;

		protected override void SetUp()
		{
			base.SetUp();

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "8888");
			Enterprise.Customs.US.DataRegistry.Business.USCustomsDataRegistry.Instance.DisableAbnormalityMessageReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		}

		#endregion

		public void TestError46DisFailure()
		{
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			dec.US_PreliminaryStatementPrintDate = new ZDateTime(2011, 12, 16);
			entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "05084691";

			statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "05084691";
			statementLine.B3_EntryProcessPort = "8888";
			statementLine.B3_Status = StatementLineStatusList.Codes.DeletionPending;

			AssertEquals("Declaration in statement line", entry.Declaration, statementLine.Declaration);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			transmittedMessage = mock.Object;
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_MessageText = "B018888XJ5HP                                               ~000771              H8888XJ5 050846912122111                                                        Y  8888XJ5HP00001";
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			entry.Messages.Add(transmittedMessage);

			responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_MessageText = "B018888XJ5HT                                               ~000771              H18888XJ5 0508469146DSCH PAY DATE INVALID FOR QUOTA           2122111B00001212  H28888XJ5 05084691391235018100000222327                                         Y  8888XJ5HT00002";
			entry.Messages.Add(responseMessage);

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var statementHeaderLoaded = factory2.Load<CusStatementHeader>(statementHeader.PK);
			AssertEquals("StatementLine.B3_Status is updated", StatementLineStatusList.Codes.Active, statementHeaderLoaded.StatementLines[0].B3_Status);
		}

		protected abstract ZString TransmittedMessageType { get; }
		protected abstract ZString ResponseMessageType { get; }
	}
}
