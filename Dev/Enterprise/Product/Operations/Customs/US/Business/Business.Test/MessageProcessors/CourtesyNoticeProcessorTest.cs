using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CourtesyNoticeProcessorTest : ABIProcessorTest<CourtesyNoticeProcessor, APLA, APLB, APLY>
	{
		public void TestGetKeysForBlockingParallelProcessing()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText =
"B012904AZ2NR                                                                    " +
"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
"N22904AZ2 41449610082109004144961  IL081019029 361003110070800000000000000000000" +
"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
"Y  1101SV9NR00000                                                               ";
			message.EM_Status = MQEDIMessage.Status.Queued;
			TestCase("N1 block");

			message.EM_MessageText =
"B012904AZ2NR                                                                    " +
"N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
"N33807AZ2 414496220138-354037600033009042041109S09CDTWAI                        " +
"Y  2904AZ2NR00141";
			TestCase("N3 block");

			void TestCase(string caseId)
			{
				var logger = new LoggingInformation();
				var processor = new CourtesyNoticeProcessor();
				var provider = processor as IKeysForBlockingParallelProcessingProvider;
				var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
				var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
				CombineAssertions(caseId, () =>
				{
					AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, $"Entry:AZ2-41449610|{GlbCompany.CurrentCompany.PK}")), actualMetaData);
					AssertEquals(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, actualKeys.ReturnValue.ResultType);
				});
			}
		}

		public void TestACECourtesyNotice()
		{
			DeclarationTestHelper.SetupForSendMessage();
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807XJ5 000020670338-354037600033009 40041109S09CDTWAI 5 6 7                  " +
					  "N42904AZ2 414496100000000000000000055517000000000000000000000000000055517 1 2 3 " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			AssertNoExceptionThrown("This message is ACE Courtesy Notice, it can be processed", processor.ExecuteBatch);
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(emailToMatched => emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"));
			AssertEquals("Emails expected", 1, emails.Count);

			var emailBody = emails[0].Body;
			Assert(emailBody.Contains("5,6,7"));
			Assert(emailBody.Contains("1,2,3"));
		}

		public void TestNoticeEmailSendingToGroupOnlyIfConfiguredSo()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "1AB";
			staff.GS_EmailAddress = "1AB@test.com";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.ImportEntryNumber = "00002067";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "CTN";

			var testBroker = emailGroup.Staff.AddNew();
			testBroker.GS_LoginName = "JJBroker";
			testBroker.GS_Code = "JJB";
			testBroker.GS_FullName = "John J Broker";
			testBroker.GS_EmailAddress = "johnj.broker@test1.com";

			Factory.Save();

			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LiquidationGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroup.PK, false));

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000020670338-354037600033009040041109S09CDTWAI                        Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(emailToMatched => emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"));
			AssertEquals("Emails expected", 1, emails.Count);
			AssertEquals("Emails expected", 1, emails[0].Recipients.Count);
			AssertNotNull("Emails expected", emails[0].Recipients.Cast<RecipientDef>().FirstOrDefault(x => x.Email == "johnj.broker@test1.com"));
		}

		public void TestNoticeEmailSendingToABIMessageGroupIsStaffIsInactive()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "1AB";
			staff.GS_EmailAddress = "1AB@test.com";
			staff.GS_IsActive = false;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.ImportEntryNumber = "00002067";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "CTN";

			var testBroker = emailGroup.Staff.AddNew();
			testBroker.GS_LoginName = "JJBroker";
			testBroker.GS_Code = "JJB";
			testBroker.GS_FullName = "John J Broker";
			testBroker.GS_EmailAddress = "johnj.broker@test1.com";
			emailGroup.Staff.Load();

			Factory.Save();

			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LiquidationGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty, false));
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, emailGroup.PK, false));

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000020670338-354037600033009040041109S09CDTWAI                        Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(emailToMatched => emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"));
			AssertEquals("Emails expected", 1, emails.Count);
			AssertEquals("Emails expected", 1, emails[0].Recipients.Count);
			AssertNotNull("Emails expected", emails[0].Recipients.Cast<RecipientDef>().FirstOrDefault(x => x.Email == "johnj.broker@test1.com"));
			AssertEquals(true, emails[0].Body.Contains("(This email was redirected to the ABI Messages group because the original email was sent to staff member who is not active now.)"));
		}

		public void TestSuppressNoChangeLiquidation()
		{
			var liquidationNotification = LiquidationGroupNotification.Default;
			liquidationNotification.SuppressNoChangeLiquidations = true;
			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, liquidationNotification);

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109004144961  IL081019029 361003110070800000000000000000000",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();

			AssertEquals("No Emails expected", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		protected override void EndToEndCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000111";
			declaration.US_EntryFilerCode = "AZ2";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryHeader.EntryNumber = "41449610";
			entryHeader.US_ALDuty = 5137m;
			entryHeader.US_ALDate = new ZDateTime(2009, 08, 15);

			declaration.Logs.GetAllLogs().Load();
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation));

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
				"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    N22904AZ2 41449610082109004144961  IL081019029 361003110070800000000000000000000N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       N12904AZ2 414265010156-266402000000019618500000000000000000827240000000000001   N22904AZ2 41426501082109014142650  LQ090803D9SX002001503090800000000000000022783N42904AZ2 414265010000000000000000069869000000000000000000000000000029460       N12904AZ2 414267170156-266402000000025872000000000000000001024802000000000001   N22904AZ2 41426717082109014142671  LQ090803D9SX002002503160800000000000000029919N42904AZ2 414267170000000000000000080840000000000000000000000000000035444       N12904AZ2 414275900156-266402000000012592800000000000000000549748000000000001   N22904AZ2 41427590082109014142759  LQ090803D9SX002003503230800000000000000013140N42904AZ2 414275900000000000000000046874000000000000000000000000000019305       N12904AZ2 414487290172-15468210000000048176000000000000000004817600000000000    N22904AZ2 41448729082109004144872  IL081019029 361002110060800000000000000000000N42904AZ2 414487290000000000000000002500000000000000000000000000000002500       N12904AZ2 414492480182-01966110000000000000000000000000000000000000000000000    N22904AZ2 41449248082109004144924  LQ081021D6QY001001110050800000000000000000000N42904AZ2 414492480000000000000002567828000000000000000000000000002567828       Y  2904AZ2NR00141",
				"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			AssertNotNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
			AssertEquals("Emails expected", 6, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for B00000111 / 41449610", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for Non CargoWise One Job / 41426501", Env.OutgoingCustomsMailManager.EmailsCreated[1].Subject);
			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation));

			var liquidations = Factory.Load<CusLiquidation>(new ZQuery());
			AssertEquals("6 Liquidation Objects created after message processing", 6, liquidations.Length);
			AssertEquals("Created Date", new ZDateTime(2009, 11, 27), liquidations[0].B8_SystemCreateDate);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageOwner = "Reprocessing";
			message.EM_Status = "QUE";
			processor.ExecuteBatch();

			AssertNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
			AssertEquals("Emails expected", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			message.Reload();
			AssertEquals("Message Owner", ZString.Empty, message.EM_MessageOwner);
			AssertEquals(EM_MessageSubTypeList.Codes.LiquidationNotice, message.EM_MessageSubType);
			entryHeader.Reload();
			AssertEquals("Anticipated Liquidated Date should be changed", new ZDateTime(2009, 08, 21), entryHeader.US_ALDate);
			AssertEquals("Anticipated Liquidated Duty should be changed", 5137.41m, entryHeader.US_ALDuty);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 0; }
		}

		public void TestNoticeEmailSentToGroupWhenNoBrokerFound()
		{
			var testBroker = Factory.New<GlbStaff>();
			testBroker.GS_LoginName = "JJBroker";
			testBroker.GS_Code = "JJB";
			testBroker.GS_FullName = "John J Broker";
			testBroker.GS_EmailAddress = "johnj.broker@test1.com";

			var testUser = Factory.New<GlbStaff>();
			testUser.GS_LoginName = "TestCTN";
			testUser.GS_Code = "CTN";
			testUser.GS_FullName = "Test CTN User";
			testUser.GS_EmailAddress = "Test@test1.com";

			var emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "CTN";

			var groupLink = Factory.New<GlbGroupLink>();
			groupLink.GK_GG = emailGroup.PK;
			groupLink.GK_GS = testUser.PK;

			Factory.Save();

			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LiquidationGroupNotification(GroupNotification.StaffMemberOrNominatedGroup, emailGroup.PK, false));
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ManifestGroupNotification("ESG", emailGroup.PK, false));
			var externalBroker = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mode = externalBroker.EDICommunicationsModes.AddNew();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			mode.EK_Destination = "test@ema.com";
			mode.EK_ServerAddressSubject = "EmailAsAttchMode";
			mode.EK_Module = EDICommunicationsMode.Modules.US_BIRD;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "S00005002";
			declaration.JE_OH_ExternalBroker = externalBroker.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryHeader.EntryNumber = "ENT_03948";

			var dummyOriginalOutgoingMessage = Factory.NewWithValidTestData<MQEDIMessage>();
			dummyOriginalOutgoingMessage.EM_SystemCreateUser = testBroker.GS_Code;
			dummyOriginalOutgoingMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			dummyOriginalOutgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			dummyOriginalOutgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			dummyOriginalOutgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			dummyOriginalOutgoingMessage.EM_MessageText = "B018888XJ5EI                                               <<MSGNO PLACEHOLDER>>10A888891-01319900091-013199000                 8071007   XJ5<ENTPLCH>31891  IL 20                                                                     Z104     30                  XJ51000010388880                1                           40001AU00000060000000000010                    0000000100                       50 2208204000          000000060000PFL                              AU070107N   51                                                                              60                                        ECPEFCIA113MAN  0000213979            62          49900001260                                                         8949900000002500                                                                90           00000213979                        0000000250000000006000          Y  8888XJ5EI00010            000000213979";
			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N12222XJ5ENT_0394801NNN-NN-NNNN 000000137000000000563000000013700000000056301   N22222XJ5ENT_0394812030714BROKR-REFCBP_DOC_FILING_LOC607140700000000000000008247N42222XJ5ENT_039480000000010000000000200000000003000000000040000000000500       Y  2904AZ2NR00141",
"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var emailDef = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); }));

			AssertNotNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", emailDef);
			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for S00005002 / ENT_03948", emailDef.Subject);
			AssertEquals("It should send to Courtesy Notice of Liquidation Group only if broker is not found", 1, emailDef.Recipients.Count);
			AssertEquals("Broker email is expected", "Test@test1.com", emailDef.Recipients[0].Email);

			var factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.BIRDTransaction);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Pending);

			AssertEquals(1, decLoaded.Messages.Find(query).Length);
		}

		public void TestNoticeEmailWontSentToBrokerGroupIfInactive()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "1AB";
			staff.GS_EmailAddress = "1AB@test.com";
			staff.GS_IsActive = false;

			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";
			staff.GS_IsActive = false;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.ImportEntryNumber = "00002067";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "CTN";
			Factory.Save();

			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LiquidationGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, emailGroup.PK, false));

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000020670338-354037600033009040041109S09CDTWAI                        Y  2904AZ2NR00141",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("Emails should not send becuase there is no receipient in the email.", 0, emails.Count);
		}

		public void TestNoticeEmailWontSentToBrokerIfInactive()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "1AB";
			staff.GS_EmailAddress = "1AB@test.com";
			staff.GS_IsActive = false;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.ImportEntryNumber = "00002067";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "CTN";
			Factory.Save();

			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LiquidationGroupNotification(Core.Constants.EmailTo.StaffMember, emailGroup.PK, false));

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000020670338-354037600033009040041109S09CDTWAI                        Y  2904AZ2NR00141",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("Emails redirected to ABI message group", 1, emails.Count);
		}

		public void TestNoticeEmailSentToBrokerAsWellAsSendingToGroupCS00192837()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "1AB";
			staff.GS_EmailAddress = "1AB@test.com";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			declaration.ImportEntryNumber = "00002067";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var testBroker = Factory.New<GlbStaff>();
			testBroker.GS_LoginName = "JJBroker";
			testBroker.GS_Code = "JJB";
			testBroker.GS_FullName = "John J Broker";
			testBroker.GS_EmailAddress = "johnj.broker@test1.com";

			var emailGroup = Factory.New<GlbGroup>();
			emailGroup.GG_Code = "CTN";

			var groupLink = Factory.New<GlbGroupLink>();
			groupLink.GK_GG = emailGroup.PK;
			groupLink.GK_GS = testBroker.PK;

			Factory.Save();

			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new LiquidationGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, emailGroup.PK, false));

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000020670338-354037600033009040041109S09CDTWAI                        Y  2904AZ2NR00141",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(emailToMatched => emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"));
			AssertEquals("Emails expected", 1, emails.Count);
			AssertEquals("Emails expected", 2, emails[0].Recipients.Count);
			AssertNotNull("Emails expected", emails[0].Recipients.Cast<RecipientDef>().FirstOrDefault(x => x.Email == "1AB@test.com"));
			AssertNotNull("Emails expected", emails[0].Recipients.Cast<RecipientDef>().FirstOrDefault(x => x.Email == "johnj.broker@test1.com"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestOnlyOneN3BlockReceived()
		{
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000020670338-354037600033009040041109S09CDTWAI                        Y  2904AZ2NR00141",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			AssertNotNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
			AssertEquals("Emails expected", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for Non CargoWise One Job / 00002067", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			var query = new ZQuery(CusLiquidationSchema.B8_EntryFilerCode, "XJ5");
			query.AddToFilter(CusLiquidationSchema.B8_EntryNumber, "00002067");
			AssertNotNull(Factory.LoadTop1<CusLiquidation>(query));

			message.EM_Status = "QUE";
			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var liquidations = Factory.Load<CusLiquidation>(query);
			AssertEquals("Existing Liquidation should be loaded", 1, liquidations.Length);
		}

		public void TestOldLiquidationTypeCodes()
		{
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109374144961  IL081019029 361003110070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var query = new ZQuery(CusLiquidationSchema.B8_LiquidationType, LiquidationTypeCodeList.Codes.Code01);
			query.AddToFilter(CusLiquidationSchema.B8_ChangeLiquidationReasonCode, ChangeLiquidationReasonCodeList.Codes.Code37);
			AssertNotNull(Factory.LoadTop1<CusLiquidation>(query));

			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var liquidations = Factory.Load<CusLiquidation>(query);
			AssertEquals("Should have 1 liquidation", 1, liquidations.Length);
		}

		public void TestEmailingWithLiquidationReasondCode()
		{
			var liquidationNotification = LiquidationGroupNotification.Default;
			liquidationNotification.SuppressNoChangeLiquidations = true;
			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, liquidationNotification);

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109374144961  IL081019029 361003110070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var query = new ZQuery(CusLiquidationSchema.B8_LiquidationType, LiquidationTypeCodeList.Codes.Code01);
			query.AddToFilter(CusLiquidationSchema.B8_ChangeLiquidationReasonCode, ChangeLiquidationReasonCodeList.Codes.Code37);
			AssertNotNull(Factory.LoadTop1<CusLiquidation>(query));

			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var liquidations = Factory.Load<CusLiquidation>(query);
			AssertEquals("Should have 1 liquidation", 1, liquidations.Length);
			AssertNotNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
		}

		public void TestEmailDoesNotContainSSNNumber()
		{
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 4144961001123-45-6789 00000513741000000000000000051374100000000000    " +
					"N12904AZ2 4144961001123345-6789 00000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109994144961  IL081019029 361003L10070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			AssertNotNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
			var sentMails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("Emails expected", 2, sentMails.Count);
			Assert("Email does not contain SSN Number", !sentMails[0].Body.Contains("<td>Importer of Record Number</td><td>123-45-6789</td>"));
			Assert("EIN Number or CBP Number included in the mail", sentMails[1].Body.Contains("<td>Importer Of Record</td><td>123345-6789</td>"));
		}

		public void TestNoEmailWithLiquidationReasonCode99()
		{
			var liquidationNotification = LiquidationGroupNotification.Default;
			liquidationNotification.SuppressNoChangeLiquidations = true;
			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, liquidationNotification);

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109994144961  IL081019029 361003L10070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var query = new ZQuery(CusLiquidationSchema.B8_LiquidationType, LiquidationTypeCodeList.Codes.Liquidated);
			query.AddToFilter(CusLiquidationSchema.B8_ChangeLiquidationReasonCode, ChangeLiquidationReasonCodeList.Codes.Code99);
			AssertNotNull(Factory.LoadTop1<CusLiquidation>(query));

			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();
			AssertNull("An email should Not have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
		}

		public void TestNoEmailWithLiquidationReasonCode00()
		{
			var liquidationNotification = LiquidationGroupNotification.Default;
			liquidationNotification.SuppressNoChangeLiquidations = true;
			USCustomsDataRegistry.Instance.CourtesyNoticeMessagesGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, liquidationNotification);

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082100004144961  IL081019029 361003L10070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var query = new ZQuery(CusLiquidationSchema.B8_LiquidationType, LiquidationTypeCodeList.Codes.Liquidated);
			query.AddToFilter(CusLiquidationSchema.B8_ChangeLiquidationReasonCode, ChangeLiquidationReasonCodeList.Codes.Code00);
			AssertNotNull(Factory.LoadTop1<CusLiquidation>(query));

			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();
			AssertNull("An email should Not have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
		}

		public void TestNewLiquidationTypeCodes()
		{
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109994144961  IL081019029 361003L10070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var query = new ZQuery(CusLiquidationSchema.B8_LiquidationType, LiquidationTypeCodeList.Codes.Liquidated);
			query.AddToFilter(CusLiquidationSchema.B8_ChangeLiquidationReasonCode, ChangeLiquidationReasonCodeList.Codes.Code99);
			AssertNotNull(Factory.LoadTop1<CusLiquidation>(query));

			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var liquidations = Factory.Load<CusLiquidation>(query);
			AssertEquals("Should have 1 liquidation", 1, liquidations.Length);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Testing")]
		public void TestOnlyN3MultipleBlocksReceived()
		{
			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039", "B012904AZ2NR                                                                    N33807XJ5 000015980320-182097100031409040032809S09CDTWAI                        N33807XJ5 000016300338-341148900031709040032809S09CDTWAI                        N33807XJ5 000015230338-354037600031209040032809S09CDTWAI                        Y  2904AZ2NR00141",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			AssertNotNull("An email should have been sent with subject containing 'Courtesy Notice of Liquidation'", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Courtesy Notice of Liquidation"); })));
			AssertEquals("Emails expected", 3, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for Non CargoWise One Job / 00001598", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for Non CargoWise One Job / 00001630", Env.OutgoingCustomsMailManager.EmailsCreated[1].Subject);
			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for Non CargoWise One Job / 00001523", Env.OutgoingCustomsMailManager.EmailsCreated[2].Subject);
		}

		public void TestUpdateUS_TIBNumOfExtensions_OneN3()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000111";
			declaration.US_EntryFilerCode = "AZ2";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "41449610";

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039",
					  "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(3, loadedDeclaration.FormalEntry.US_TIBNumOfExtensions);
		}

		public void TestUpdateUS_TIBNumOfExtensions_OneN3_HasLiquidataionAndCreateTimeEarlierThanMessageTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000111";
			declaration.US_EntryFilerCode = "AZ2";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "41449610";

			var liquidation = declaration.Liquidations.AddNew();
			liquidation.B8_SystemCreateDate = new ZDateTime(2000, 01, 01);
			liquidation.B8_NoOfSuspensions = 1;

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039",
					  "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(3, loadedDeclaration.FormalEntry.US_TIBNumOfExtensions);
		}

		public void TestUpdateUS_TIBNumOfExtensions_OneN3_HasLiquidataionAndCreateTimeLaterThanMessageTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000111";
			declaration.US_EntryFilerCode = "AZ2";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "41449610";

			var liquidation = declaration.Liquidations.AddNew();
			liquidation.B8_SystemCreateDate = new ZDateTime(2024, 01, 01);
			liquidation.B8_NoOfSuspensions = 1;

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039",
					  "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(0, loadedDeclaration.FormalEntry.US_TIBNumOfExtensions);
		}

		public void TestUpdateUS_TIBNumOfExtensions_OneN3_HasLiquidataionAndCreateTimeLaterThanMessageTimeButHasNoB8_NoOfSuspensions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000111";
			declaration.US_EntryFilerCode = "AZ2";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "41449610";

			var liquidation = declaration.Liquidations.AddNew();
			liquidation.B8_SystemCreateDate = new ZDateTime(2024, 01, 01);

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039",
					  "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(3, loadedDeclaration.FormalEntry.US_TIBNumOfExtensions);
		}

		public void TestUpdateUS_TIBNumOfExtensions_MultipleN3WithSameEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00000111";
			declaration.US_EntryFilerCode = "AZ2";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.EntryNumber = "41449610";

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
					  "N33807AZ2 414496100138-354037600033009042041109S09CDTWAI                        " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039",
					  "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("will take the last N3", 2, loadedDeclaration.FormalEntry.US_TIBNumOfExtensions);
		}

		public void TestUpdateUS_TIBNumOfExtensions_MultipleN3WithDifferentEntryNumber()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "B00000111";
			declaration1.US_EntryFilerCode = "AZ2";

			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader1.EntryNumber = "41449610";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "B00000112";
			declaration2.US_EntryFilerCode = "AZ2";

			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader2.EntryNumber = "41449622";

			Factory.Save();

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					  "A3901SV9      11270901   112709014539                                00000000039",
					  "B012904AZ2NR                                                                    " +
					  "N33807AZ2 414496100138-354037600033009043041109S09CDTWAI                        " +
					  "N33807AZ2 414496220138-354037600033009042041109S09CDTWAI                        " +
					  "Y  2904AZ2NR00141",
					  "Z3901SV9      11270901   112709014539                                00000000039",
					  "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration1 = newFactory.Load<JobDeclaration>(declaration1.PK);
			AssertEquals(3, loadedDeclaration1.FormalEntry.US_TIBNumOfExtensions);
			var loadedDeclaration2 = newFactory.Load<JobDeclaration>(declaration2.PK);
			AssertEquals(2, loadedDeclaration2.FormalEntry.US_TIBNumOfExtensions);
		}

		public void TestForReconciliationLiqudation()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ReconWrappedJobDeclaration.JE_DeclarationReference = "B00000111";
			reconDeclaration.US_EntryFilerCode = "AZ2";

			var entry = reconDeclaration.ReconEntry;
			entry.GetEntry().EntryNumber = "41449610";
			Factory.Save();

			reconDeclaration.Logs.GetAllLogs().Load();
			AssertNull(reconDeclaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation));

			var message = CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      11270901   112709014539                                00000000039",
					"B012904AZ2NR                                                                    " +
					"N12904AZ2 414496100143-08196450000000513741000000000000000051374100000000000    " +
					"N22904AZ2 41449610082109004144961  IL081019029 361003110070800000000000000000000" +
					"N42904AZ2 414496100000000000000000055517000000000000000000000000000055517       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      11270901   112709014539                                00000000039", "");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			AssertEquals("Emails expected", "Courtesy Notice of Liquidation for B00000111 / 41449610", Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			reconDeclaration.Logs.GetAllLogs().Load();
			AssertNotNull(reconDeclaration.Logs.MostRecentLogByEventTime(Events.MessageStatusChange, ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation));

			var liquidations = Factory.Load<CusLiquidation>(new ZQuery());
			AssertEquals("Created Date", new ZDateTime(2009, 11, 27), liquidations[0].B8_SystemCreateDate);
			AssertEquals("Job Number", reconDeclaration.ReconWrappedJobDeclaration.JE_DeclarationReference, liquidations[0].JobNumber);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message.EM_MessageOwner = "Reprocessing";
			message.EM_Status = "QUE";
			processor.ExecuteBatch();

			AssertEquals("Emails expected", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			message.Reload();
			AssertEquals("Message Owner", ZString.Empty, message.EM_MessageOwner);
			AssertEquals(EM_MessageSubTypeList.Codes.LiquidationNotice, message.EM_MessageSubType);
		}

		public void TestLiquidationForDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.ImportEntryNumber = "71023178";
			declaration.US_EntryFilerCode = "SV9";
			Factory.Save();

			CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
					"A3901SV9      07101901   112709014539                                00000000039",
					"B005301SV9NR                                                                    " +
					"N10712SV9 710231784774-288106400000000000000000000000000000000000000000000000   " +
					"N20712SV9 7102317806071947800253656                  R05311800000000000000000000" +
					"N40712SV9 710231780000000000000000865008000000000000000000000000000865008       " +
					"Y  1101SV9NR00000                                                               ",
					"Z3901SV9      07101901   112709014539                                00000000039", "");

			var processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			var liquidations = Factory.Load<CusLiquidation>(new ZQuery());
			AssertEquals(1, liquidations.Length);
			AssertEquals("Created Date", new ZDateTime(2019, 07, 10), liquidations[0].B8_SystemCreateDate);

			CreateInterchangeAndMessageResponse(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation,
				"A3901SV9      07111901   112709014539                                00000000039",
				"B005301SV9NR                                                                    " +
				"N10712SV9 710231784774-288106400000000000000000000000000000000000000000000000   " +
				"N20712SV9 7102317807071947800253656                  R05311800000000000000000000" +
				"N40712SV9 710231780000000000000000865008000000000000000000000000000865008       " +
				"Y  1101SV9NR00000                                                               ",
				"Z3901SV9      07111901   112709014539                                00000000039", "");

			processor = new USRIncomingMessageProcessor();
			processor.ExecuteBatch();

			liquidations = Factory.Load<CusLiquidation>(new ZQuery());
			AssertEquals(2, liquidations.Length);
			if (liquidations[0].B8_SystemCreateDate == new ZDateTime(2019, 07, 10))
			{
				AssertEquals("Created Date", new ZDateTime(2019, 07, 11), liquidations[1].B8_SystemCreateDate);
			}
			else
			{
				AssertEquals("Created Date", new ZDateTime(2019, 07, 11), liquidations[0].B8_SystemCreateDate);
				AssertEquals("Created Date", new ZDateTime(2019, 07, 10), liquidations[1].B8_SystemCreateDate);
			}

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(2, loadedDeclaration.Liquidations.Count);
			AssertEquals(new ZDateTime(2019, 07, 07), loadedDeclaration.US_AnticipatedLiquidationDate);
		}
	}
}
