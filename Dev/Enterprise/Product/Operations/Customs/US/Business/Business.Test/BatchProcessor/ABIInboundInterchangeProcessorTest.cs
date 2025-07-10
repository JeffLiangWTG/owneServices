using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ABIInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessageResponseForInvalidData()
		{
			var interchangeHeader = "A3910SV9      10281001   110110212849                                00000054211";

			var interchangeBody =
				"B018888BHCEI                                               54443                " +
				"EB\"A\" AND \"B\" REC DP/FLR/OFFICE CONFLICT                                        " +
				"EBTRANSACTION DATA REJECTED                                                     " +
				"EB\"Z\" RECORD MISSING OR INVALID                                                 " +
				"EBTRANSACTION DATA REJECTED".PadRight(80);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			IInboundInterchangeProcessor processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			var interchange = CreateAndSaveInterchange(interchangeHeader, interchangeBody, "");
			processor.Execute();

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var messages = Factory.Load<MQEDIMessage>(query);
			AssertEquals(1, messages.Length);
			var expectedMessageStart = interchangeBody.Substring(0, 187);
			AssertEquals(true, messages[0].EM_MessageText.StartsWith(expectedMessageStart));
		}

		public void TestSupportEnvironmentSwitching()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "K#@";
			company.GC_Name = "TEST COMP";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "B$#";
			branch.GB_BranchName = "BKD NAME";
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();

			var interchange1 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054211", b(ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse).Serialise(), "");
			interchange1.EI_GB = branch.PK;
			var interchange2 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054212", b(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse).Serialise(), "");
			interchange2.EI_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var logger = new LoggingInformation();
			var processor = new ABIInboundInterchangeProcessorForTesting(logger);
			processor.ExecuteBatch();

			interchange1.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange1.EI_Status);
			interchange2.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange2.EI_Status);
		}

		public void TestReferenceFileMessagesAreNotIgnored()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			var interchange1 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054211", b(ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse).Serialise(), "");
			var interchange2 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054212", b(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse).Serialise(), "");
			var interchange3 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054213", b(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse).Serialise(), "");

			var interchange6 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054216", b(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate).Serialise(), "");
			var interchange7 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054217", b(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem).Serialise(), "");
			var interchange8 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054218", b(ApplicationIdentifierCodeList.Codes.QueryQuotaResponse).Serialise(), "");
			((IInboundInterchangeProcessor)processor).Execute();

			interchange1.Reload();
			AssertEquals("ADCVDCaseInformationQueryResponse should be processed", EDIInterchange.Status.Received, interchange1.EI_Status);
			interchange2.Reload();
			AssertEquals("AntidumpingCountervailingDutyQueryResponse should be processed", EDIInterchange.Status.Received, interchange2.EI_Status);
			interchange3.Reload();
			AssertEquals("ExtractReferenceFilesResponse should be processed", EDIInterchange.Status.Received, interchange3.EI_Status);
			interchange6.Reload();
			AssertEquals("HarmonizedSystemUpdate should be processed", EDIInterchange.Status.Received, interchange6.EI_Status);
			interchange7.Reload();
			AssertEquals("QueryHarmonizedSystem should be processed", EDIInterchange.Status.Received, interchange7.EI_Status);
			interchange8.Reload();
			AssertEquals("QueryQuotaResponse should be processed", EDIInterchange.Status.Received, interchange8.EI_Status);
		}

		public void TestISFMessagesAreNotIgnored()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			var interchange1 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054211", b(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse).Serialise(), "");
			var interchange2 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054212", b(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory).Serialise(), "");
			var interchange3 = CreateAndSaveInterchange("A3910SV9      10281001   110110212849                                00000054213", b(ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling).Serialise(), "");
			((IInboundInterchangeProcessor)processor).Execute();

			interchange1.Reload();
			AssertEquals("ImporterSecurityFilingResponse should be processed", EDIInterchange.Status.Received, interchange1.EI_Status);
			interchange2.Reload();
			AssertEquals("ImporterSecurityFilingStatusAdvisory should be processed", EDIInterchange.Status.Received, interchange2.EI_Status);
			interchange3.Reload();
			AssertEquals("ImporterSecurityFiling should be processed", EDIInterchange.Status.Received, interchange3.EI_Status);
		}

		public void TestProcessMessageWhereThereAreMultipleMessagesInBYBlock_CargoRelease()
		{
			const string r1Message1 =
				"R13807EPF 000006651138-321340100S09BDTWAIKL                      617  020609    " +
				"R4            07426954266                         00000001CT                    " +
				"R5021209120601COND RELEASE GEN EXAM                                             " +
				"R5021209120671AMS AIR CARRIER/CFS NOTIFIED                                      " +
				"R5021209120622RELEASE DATE UPDATE                     02120901                  ";

			const string r1Message2 =
				"R13807EPF 000006810138-352953800S09BDTWAIKL                      617  021109    " +
				"R4            07430395116                         00000008PK                    " +
				"R5021209120405PAPERLESS                                                         " +
				"R5021209120471AMS AIR CARRIER/CFS NOTIFIED                                      " +
				"R5021209120422RELEASE DATE UPDATE                     02120901                  ";

			var interchangeBody = string.Format(
				"B00       RR                                               123                  {0}{1}Y         RR00000                                                               ",
				r1Message1, r1Message2);

			AssertMessagesCreated(ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults, interchangeBody, 2, r1Message1, r1Message2);
		}

		public void TestProcessMessageWhereThereAreMultipleMessagesInBYBlock_BillOfLadingUpdate()
		{
			const string p1Message1 =
				"P18888XJ5 700048960398-041772300B00150813APLUV136333  0809105301A               " +
				"P3            MSCUTQTQEWTEW   MSCU8W603175                    00000012AE   N    ";

			const string p1Message2 =
				"P18888XJ5 700049040398-041772300B00150815APLUV136333  0809105301A               " +
				"P3            MSCUBRENDONDS   MSCU8W603175                    00000014DR   N    ";

			var interchangeBody = string.Format(
				"B00       PS                                               123                  {0}{1}Y         PS00000                                                               ",
				p1Message1, p1Message2);

			AssertMessagesCreated(ApplicationIdentifierCodeList.Codes.BillofLadingProcessingResults, interchangeBody, 2, p1Message1, p1Message2);
		}

		public void TestProcessMessageProtestAutomaticNotificationandResponsetoFilerQuery()
		{
			const string p10Message1 =
				"P10340115150003BKX50004237521R20150908                           O20150908      " +
				"P11S        N        N        06P20160104                                       " +
				"P12SU320151120SUSPENDED PENDING FURTHER REVIEW BY OFC OF REGS AND RULINGS       ";

			const string p10Message2 =
				"P10530916150003BKX60006087312                                    O20160107      " +
				"P11S        N        N        05620160107                                       " +
				"P1553091                                                                        ";

			var interchangeBody = string.Format(
				"B00       SS                                               123                  {0}{1}Y         SS00000                                                               ",
				p10Message1, p10Message2);

			AssertMessagesCreated(ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery, interchangeBody, 2, p10Message1, p10Message2);
		}

		public void TestProcessCargoReleaseTransactionsResponse()
		{
			string aBlock = "A3902SV9      12140901   121409202803                                00000039728";
			string bodyBlock =
				"B018888XJ5HR                                               39933                " +
				"H1A8888XJ5 7002293091-0131990004010140991                   UA  390101421       " +
				"H2I310    91-013199000 983  0000005000B00152696                                 " +
				"H68888XJ5 70022930B0015269691-013199000SELECTIVITY ALREADY PERFORMED 4BB        " +
				"H68888XJ5 70022930B0015269691-013199000TRANSACTION DATA REJECTED     524        " +
				"Y  8888XJ5HR00004                                                               ";
			string zBlock = "Z3902SV9      12140901   121409202803                                00000039728";

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var interchange = IncomingInterchangeProcessorTest.CreateAndSaveInterchange(Factory, CBPEDIInterchange.ApplicationCodes.USCustomsImport, "USC", "SV9", aBlock, bodyBlock, zBlock);
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);

			IInboundInterchangeProcessor processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();

			AssertEquals(CBPEDIInterchange.ApplicationCodes.USCustomsImport, interchange.EI_ApplicationCode);
			AssertEquals(CBPEDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("USC", interchange.EI_From);
			AssertEquals("SV9", interchange.EI_To);
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse, interchange.EI_InterchangeType);
			AssertEquals(aBlock, interchange.EI_HeaderText);
			AssertEquals(bodyBlock.TrimEnd(), interchange.EI_BodyText);
			AssertEquals(zBlock, interchange.EI_FooterText);

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var messages = Factory.Load<MQEDIMessage>(query);
			AssertEquals(1, messages.Length);
			var message = messages[0];

			AssertEquals(EDIMessage.ApplicationCodes.USCustomsImport, message.EM_ApplicationCode);
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse, message.EM_MessageType);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("39933", message.EM_MessageNum);
			AssertEquals(bodyBlock.TrimEnd(), message.EM_MessageText);
		}

		public void TestProcessACEFatalRejectionMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchange = CreateAndSaveInterchange(
				"A3901SV9CAREDI100510     AX                                          00006006355",
				"B                                                                              BX0 BLOCK       1 REF ID: 8888 888    AE 6007725                                 X1 FX18   PROC PORT/FLR NOT AUTHRZD FOR SENDR/RCVR                              X1RF999   BATCH REJECTED                                                        Y           00003                                                              Y",
				"Z                                                                              Z");
			AssertEquals("EI_InterchangeType empty on creation?", true, interchange.EI_InterchangeType.IsEmpty);

			IInboundInterchangeProcessor processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			var messages = Factory.Load<MQEDIMessage>(query);
			AssertEquals(1, messages.Length);
			var msg = messages[0];
			AssertEquals("message type should have been set", ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, msg.EM_MessageType);

			interchange.Reload();
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, interchange.EI_InterchangeType);

			AssertNoExceptionThrown(() => _ = msg.HumanReadableMessage);
		}

		public void TestHandlingInvalidData()
		{
			var postMaster = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, User.PostMasterUserName);
			postMaster.GS_EmailAddress = "postMaster@dummy.com";
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var interchange = CreateAndSaveInterchange("Z@3@#$SDD#@#@");
			IInboundInterchangeProcessor processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			var messages = Factory.Load<MQEDIMessage>(new ZQuery());
			AssertEquals(0, messages.Length);
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated.FindAll(x => x.Subject == "Error processing message");
			AssertEquals(1, emails.Count);
			var email = emails[0];

			AssertEquals(1, email.Attachments.Count);
			var attachment = email.Attachments[0];
			AssertEquals("MessageData.zip", attachment.DisplayName);
			var extractor = new ZArchitecture.Core.ZipExtractor("");

			using (var messageDataStream = new VirtualMemoryStream())
			{
				extractor.ExtractZipStream(new MemoryStream(attachment.Data), messageDataStream, "MessageData.txt");
				messageDataStream.Flush();
				AssertContains("Z@3@#$SDD#@#@", new StreamReader(messageDataStream).ReadToEnd());
			}
		}

		/// <summary>
		/// Expected Messages must be passed in alphabetical order
		/// </summary>
		void AssertMessagesCreated(string applicationIdentifier, string interchangeBody, int expectedMessagesCount, params string[] expectedMessages)
		{
			AssertEquals("Invalid use of method: expectedMessagesCount != expectedMessages.Length", expectedMessagesCount, expectedMessages.Length);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var interchange = CreateAndSaveInterchange(interchangeBody);
			AssertEquals(CBPEDIInterchange.Status.Queued, interchange.EI_Status);

			IInboundInterchangeProcessor processor = new ABIInboundInterchangeProcessor(new LoggingInformation());
			processor.Execute();

			interchange.Reload();
			AssertEquals(CBPEDIInterchange.Status.Received, interchange.EI_Status);

			var query = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			query.OrderBy = EDIMessageSchema.Constants.EM_MessageText;
			var messages = Factory.Load<MQEDIMessage>(query);
			AssertEquals(expectedMessagesCount, messages.Length);

			var bString = b(applicationIdentifier).Serialise();
			var yString = y(applicationIdentifier).Serialise();

			for (int i = 0; i < expectedMessagesCount; i++)
			{
				var expectedMsgText = (bString + expectedMessages[i] + yString).TrimEnd();
				AssertEquals("Message #" + (i + 1).ToString(), expectedMsgText, messages[i].EM_MessageText);
			}
		}

		CBPEDIInterchange CreateAndSaveInterchange(string bodyText)
		{
			const string TestHeader = "A    RCV            00                     SND";
			const string TestFooter = "Z                   00";
			return CreateAndSaveInterchange(TestHeader, bodyText, TestFooter);
		}

		CBPEDIInterchange CreateAndSaveInterchange(string headerText, string bodyText, string footerText)
		{
			return IncomingInterchangeProcessorTest.CreateAndSaveInterchange(Factory, CBPEDIInterchange.ApplicationCodes.USCustomsImport, "SND", "RCV", headerText, bodyText, footerText);
		}

		APLB b(string applicationIdentifier)
		{
			APLB result = new APLB();
			result.ApplicationIdentifier = applicationIdentifier;
			result.UserData = "123";
			return result;
		}

		APLY y(string applicationIdentifier)
		{
			APLY result = new APLY();
			result.ApplicationIdentifier = applicationIdentifier;
			return result;
		}

		sealed class ABIInboundInterchangeProcessorForTesting : ABIInboundInterchangeProcessor
		{
			public ABIInboundInterchangeProcessorForTesting(LoggingInformation logger)
				: base(logger)
			{ }

			protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
			{
				if (interchange.EI_GB != GlbBranch.CurrentBranch.PK)
				{
					throw new System.InvalidOperationException("Environment should have been switched");
				}
				return base.GetMessageCreator(interchange);
			}
		}
	}
}
