using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Business.MessageProcessors.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class ACENewEntrySummaryQueryProcessorTest : ABIProcessorTest<ACEEntrySummaryQueryProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestGetKeysForBlockingParallelProcessing()
		{
			var declaration = GetMergedDeclaration("32377722");
			var message = CreateMessage("58315", "", "B      SV9EQ                                               <<MSGNO PLACEHOLDER>>J1   SV9  32377722                                                              Y      XJ5EQ");
			message.OriginalMessage.EM_LinkedObject = declaration;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new ACEEntrySummaryQueryProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions(() =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(CusEntryHeader.Schema.TableName, declaration.ActiveEntryHeaders.EntrySummaryEntry.PK, GlbBranch.CurrentBranch.PK, declaration.JE_DeclarationReference)), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Entry:SV9-32377722|{GlbBranch.CurrentBranch.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});
		}

		protected override void EndToEndCore()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.FillWithValidTestData();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var image1 = new System.Drawing.Bitmap(1, 2);
			var image2 = new System.Drawing.Bitmap(2, 1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, image1);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, newBranch.PK.ToGuid(), Guid.Empty, image2);

			var declaration = GetMergedDeclaration("70022932");
			declaration.JE_GB = newBranch.PK;

			var message = CreateMessage("58315",
"B001101SV9ER                                               58315                " +
"JBSV9  7002293200100061911093801PM       Y2                                     " +
"JC1106191100      10      0            01                                       " +
"JD2060413000000120079000000004659000000001436                                   " +
"Y  1101SV9ER00000");

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			message.OriginalMessage.EM_LinkedObject = entry;

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);
			entry.Reload();
			AssertEquals("Not standalone message and should be attached to the entry", entry, message.EM_LinkedObject);
			AssertContains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber, Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);
			AssertEquals("Liquidation Date", ZDateTime.Empty, entry.US_ALDate);
			AssertEquals("Collection Date", ZDateTime.Empty, entry.US_CollectionDate);
			AssertEquals("Liquidation Duty", 1200.79m, entry.US_ALDuty);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		public void TestGenerateJB()
		{
			var message = CreateMessage("58322", "B003902SV9ER                                               6009273              JBSV9  6002008600100062711015805PM       Y1042212                               JC2106271100      13      0            01                                       Y  3902SV9ER00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Liquidation Status</td><td>Entry Summary Liquidated/Closed</td>"));
		}

		public void TestGenerateJC()
		{
			var message = CreateMessage("58316",
"B001101SV9ER                                               58316                " +
"JA EES 050111000000AM050211115959PM                                             " +
"JBSV9  7002293200100061911093801PM       Y2                                     " +
"JC1406191100      16                   01NO1                          43475165  " +
"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Status</td><td>Entry Summary Inactive in ACE</td></tr><tr><td>"));
			Assert(sentMail.Body.Contains("<td>Protest Status</td><td>No</td>"));
			Assert(sentMail.Body.Contains("<td>Quota Status</td><td>Quota processed</td>"));
			Assert(!sentMail.Body.Contains("<td>Liquidation Hold Status</td>"));
			Assert(sentMail.Body.Contains("<td>Collection Status</td><td>Authorized</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 1</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 2</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 3</td>"));
			Assert(sentMail.Body.Contains("<td>Extension/Suspension Status Code 4</td>"));
		}

		public void TestJDIncludesJEMessageBlock()
		{
			ErrorReporter.Clear();
			var message = CreateMessage("HYEDUSCMT_175303",
					"B001101SV9ER                                               HYEDUSCMT_175303     " +
					"JBSV9  7102388900100091316032828PM       Y2                                     " +
					"JC1109131600      10      0            1 NO                                     " +
					"JD2      000000000000000000000000000000000000000000089012000000009012           " +
					"JE00001000000000000000000000000000000000000000000000000000000000000             " +
					"Y  1101SV9ER00003"
			);
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertNullOrEmpty("There should not be errors processing a message with a JE Block", ErrorReporter.LastMessageReported);
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Liquidated Interest</td><td>890.12</td>"));
			Assert(sentMail.Body.Contains("<td>Liquidated ADD/CVD</td><td>90.12</td>"));
			Assert(sentMail.Body.Contains("Estimated Duty"));
			Assert(sentMail.Body.Contains("Estimated Tax"));
			Assert(sentMail.Body.Contains("Estimated Fees"));
			Assert(sentMail.Body.Contains("Estimated Interest"));
			Assert(sentMail.Body.Contains("Estimated ADD/CVD"));
		}

		public void TestGenerateEmailNotificationForNewJDFormat()
		{
			ErrorReporter.Clear();
			var message = CreateMessage("HYEDUSCMT_175303",
					"B001101SV9ER                                               HYEDUSCMT_175303     " +
					"JBSV9  7102388900100091316032828PM       Y2                                     " +
					"JC1109131600      10      0            1 NO                                     " +
					"JD2      000000000000000000000000000000000000000000089012000000009012010203Y    " +
					"JE00001000000000000000000000000000000000000000000000000000000000000             " +
					"Y  1101SV9ER00003");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Liquidation Reason Code (1)</td><td>Valuation</td>"));
			Assert(sentMail.Body.Contains("<td>Liquidation Reason Code (2)</td><td>Classification</td>"));
			Assert(sentMail.Body.Contains("<td>Liquidation Reason Code (3)</td><td>Quantity</td>"));
			Assert(sentMail.Body.Contains("<td>Immediate Delivery Indicator</td><td>Immediate Delivery Requested</td>"));
		}

		public void TestQueryFromReconJob()
		{
			var declaration = GetMergedDeclaration("70022841");
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_MessageType = "REC";
			declaration.JE_DeclarationReference = "JBZ0001";
			Factory.Save();
			var message = CreateMessage("58322"
				, "B003902SV9ER                                               6009273              JBSV9  7002284100100062711015805PM       Y1042212                               JC2106271100      13      0            01                                       Y  3902SV9ER00000",
				"B      XJ5EQ                                               6009273              J1   SV9  70022841                                                              Y      XJ5EQ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Subject.Contains("JBZ0001"));

			var entryHeader2 = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			entryHeader2.Messages.Load();
			AssertEquals(2, entryHeader2.Messages.Count);
		}

		public void TestNoSummariesFound()
		{
			var message = CreateMessage("58316",
"B001101SV9ER                                               58316                " +
"JA EES 050111000000AM050211115959PM                                             " +
"JZ015   QUERY COMPLETE - NO SUMMARIES FOUND                                     " +
"Y  1101SV9ER00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response for multiple entries", sentMail.Subject);
			Assert(sentMail.Body.Contains("No entries found for the query"));

			message = CreateMessage("58317",
"B001101SV9ER                                               58317                " +
"JZ013   ENTRY SUMMARY NOT FOUND FOR QUERY        SV9  13456789                  " +
"Y  1101SV9ER00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("No summaries found with Entry Number SV9-13456789."));
		}

		public void TestFailedQuery()
		{
			//Message 1
			var message = CreateMessage("58322",
"B001101SV9ER                                               58322                " +
"JA EES 063211012200PM063211255959PM                                             " +
"JZ008   REQUESTED FROM DATE TIME UNKNOWN                                        " +
"JZ010   REQUESTED TO DATE TIME UNKNOWN                                          " +
"Y  1101SV9ER00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);

			//Message 2
			message = CreateMessage("58320",
"B001101SV9ER                                               58320                " +
"JA XXX 062011000000AM062011115959PM                                             " +
"JZ006   CRITERIAQUERY TYPE CODE UNKNOWN                                         " +
"JZ017   FUTURE REQUESTED TO DATE NOT ALLOWED                                    " +
"Y  1101SV9ER00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);
			Assert(sentMail.Body.Contains("CRITERIAQUERY TYPE CODE UNKNOWN"));
			Assert(sentMail.Body.Contains("FUTURE REQUESTED TO DATE NOT ALLOWED"));

			//Message 3
			message = CreateMessage("58318",
"B001101SV9ER                                               58318                " +
"JA EES                                                                          " +
"JZ007   REQUESTED FROM DATE TIME MISSING                                        " +
"JZ009   REQUESTED TO DATE TIME MISSING                                          " +
"Y  1101SV9ER00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);
			Assert(sentMail.Body.Contains("REQUESTED FROM DATE TIME MISSING"));

			//Message 4
			message = CreateMessage("58319",
"B001101SV9ER                                               58319                " +
"JA EES 061611012200PM061311115959PM                                             " +
"JZ011   REQUESTED TO DATE < REQUESTED FROM DATE                                 " +
"Y  1101SV9ER00000");

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Entry Summary Query Response (Failure) for multiple entries", sentMail.Subject);
			AssertContains("<td>REQUESTED TO DATE &lt; REQUESTED FROM DATE</td>", sentMail.Body);
			AssertContains("Requested From Date: 16-Jun-11 13:22:00</b><br><b>Requested To Date: 13-Jun-11 23:59:59", sentMail.Body);

			//Message 5
			message = CreateMessage("58319", "B001101SV9ER                                               58319                " +
"JA EES 061611012200PM061311115959PM                                             " +
"JZ011   REQUESTED TO DATE < REQUESTED FROM DATE                                 " +
"Y  1101SV9ER00000");

			message.RelatedMessage.EM_SystemCreateUser = User.ServiceUserCode;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("No emails sent for auto query", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void TestQueryEntryNumber()
		{
			var message = CreateMessage("58322", "B003902SV9ER                                               6009273              JBSV9  6002008600100062711015805PM       Y2042212                               JC2106271100      13      0            01                                       Y  3902SV9ER00000");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("SV9-6002008-6"));
		}

		[TestDate(2021, 06, 28)]
		public void TestProcessMessage_LiquidationType_Liquidated_NoPreExistingLiquidation()
		{
			var dec = GetMergedDeclaration("71027633");
			var message = CreateMessage("123",
				"B001101SV9ER                                               123                  JBSV9  7102763300100041217102822AM       Y1041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9ER00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 1", ZDateTime.Empty, entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNotNull("A liquidation should have been created", liquidation);
			AssertEquals("The liquidation should be set against the Declaration", dec.PK, liquidation.B8_JE);
			AssertEquals("The liquidation type should be set to L", "L", liquidation.B8_LiquidationType);
			AssertEquals("The liquidation date should be set", new ZDateTime(2018, 04, 18), liquidation.B8_LiquidationDate);
			AssertEquals("The liquidation fees should be set", 47.14m, liquidation.B8_TotalLiquidatedFees);
			AssertEquals("The liquidation duty should be set", 156.50m, liquidation.B8_LiquidatedDuty);
			AssertEquals("The entry filer code should be set", "SV9", liquidation.B8_EntryFilerCode);
			AssertEquals("The entry number should be set", "71027633", liquidation.B8_EntryNumber);
			AssertEquals("The liquidation create date should be set", new ZDateTime(2021, 06, 28), liquidation.B8_SystemCreateDate);
		}

		public void TestProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation() => ProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation(true);

		public void TestProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation_LiquidationDateIsInvalid() => ProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation(false);

		public void ProcessMessage_LiquidationType_Liquidated_PreExistingLiquidation(bool liquidationDateIsValid)
		{
			var dec = GetMergedDeclaration("71027633");
			var liq = Factory.NewWithValidTestData<CusLiquidation>();
			liq.B8_JE = dec.PK;
			var liquidationDate = new ZDateTime(2017, 06, 28);
			if (liquidationDateIsValid)
			{
				liq.B8_LiquidationDate = liquidationDate;
			}
			liq.B8_EntryFilerCode = "SV9";
			liq.B8_EntryNumber = "71027633";
			Factory.Save();
			var message = CreateMessage("123",
				"B001101SV9ER                                               123                  JBSV9  7102763300100041217102822AM       Y1041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9ER00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 1", ZDateTime.Empty, entry.US_ALDate);

			var liquidations = Factory.Load<CusLiquidation>(new ZQuery());
			if (liquidationDateIsValid)
			{
				AssertEquals("No new liquidation should have been created", 1, liquidations.Length);
				AssertEquals("The existing liquidation should not have been modified", liquidationDate, liquidations[0].B8_LiquidationDate);
			}
			else
			{
				AssertEquals("new liquidation should have been created", 2, liquidations.Length);
			}
		}

		public void TestProcessMessage_LiquidationType_Reliquidated()
		{
			var dec = GetMergedDeclaration("71027633");
			var message = CreateMessage("123",
				"B001101SV9ER                                               123                  JBSV9  7102763300100041217102822AM       Y3041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9ER00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 3", ZDateTime.Empty, entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNotNull("A liquidation should have been created", liquidation);
			AssertEquals("The liquidation should be set against the Declaration", dec.PK, liquidation.B8_JE);
			AssertEquals("The liquidation type should be set to R", "R", liquidation.B8_LiquidationType);
			AssertEquals("The liquidation date should be set", new ZDateTime(2018, 04, 18), liquidation.B8_LiquidationDate);
			AssertEquals("The liquidation fees should be set", 47.14m, liquidation.B8_TotalLiquidatedFees);
			AssertEquals("The liquidation duty should be set", 156.50m, liquidation.B8_LiquidatedDuty);
			AssertEquals("The entry filer code should be set", "SV9", liquidation.B8_EntryFilerCode);
			AssertEquals("The entry number should be set", "71027633", liquidation.B8_EntryNumber);
		}

		public void TestProcessMessage_LiquidationType_NotLiquidated()
		{
			var dec = GetMergedDeclaration("71027633");
			var message = CreateMessage("123",
				"B001101SV9ER                                               123                  JBSV9  7102763300100041217102822AM       Y2041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9ER00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals("Anticipated Liquidation Date should be updated for status 2", new ZDateTime(2018, 04, 18), entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNull("No liquidation should have been created", liquidation);
		}

		public void TestProcessMessageForReconJob_LiquidationType_Reliquidated()
		{
			var dec = GetMergedDeclaration("71027633");
			var entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_MessageType = "REC";
			Factory.Save();

			var message = CreateMessage("123",
				"B001101SV9ER                                               123                  JBSV9  7102763300100041217102822AM       Y3041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9ER00003                                                               ");
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			entry.Messages.Load();
			AssertEquals("Anticipated Liquidation Date should not be updated for status 3", ZDateTime.Empty, entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNotNull("A liquidation should have been created", liquidation);
			AssertEquals("The liquidation should be set against the Declaration", dec.PK, liquidation.B8_JE);
			AssertEquals("The liquidation type should be set to R", "R", liquidation.B8_LiquidationType);
			AssertEquals("The liquidation date should be set", new ZDateTime(2018, 04, 18), liquidation.B8_LiquidationDate);
			AssertEquals("The liquidation fees should be set", 47.14m, liquidation.B8_TotalLiquidatedFees);
			AssertEquals("The liquidation duty should be set", 156.50m, liquidation.B8_LiquidatedDuty);
			AssertEquals("The entry filer code should be set", "SV9", liquidation.B8_EntryFilerCode);
			AssertEquals("The entry number should be set", "71027633", liquidation.B8_EntryNumber);
		}

		public void TestProcessMessage_LiquidationForReconJob()
		{
			var dec = GetMergedDeclaration("71027633");
			var entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CH_MessageType = "REC";
			dec.JE_DeclarationReference = "JBZ0001";
			Factory.Save();

			var message = CreateMessage("123",
				"B001101SV9ER                                               123                  JBSV9  7102763300100041217102822AM       Y2041818                               JC21041217 0       2      0            0 NO                                     JD2      000000000000000000000000000000000000000000000000000000000000           JE00000000156500000000000000000000000471400000000000000000000000000             Y  1101SV9ER00003                                                               "
				);
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(entryHeader.PK);
			entry.Messages.Load();
			AssertEquals("Anticipated Liquidation Date should be updated for status 2", new ZDateTime(2018, 04, 18), entry.US_ALDate);

			var liquidation = Factory.LoadTop1<CusLiquidation>(new ZQuery());
			AssertNull("No liquidation should have been created", liquidation);
		}

		public void TestGenerateJF()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JF123456654321TT0516231EPORT062023                                              " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Importer of Record Number</td><td>123456654321</td>"));
			Assert(sentMail.Body.Contains("<td>Entry Type</td><td>TT</td>"));
			Assert(sentMail.Body.Contains("<td>Reject Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Accelerated Drawback Indicator</td><td>Accelerated drawback approved and paid</td>"));
			Assert(sentMail.Body.Contains("<td>Electronic Invoice Indicator</td><td>Filer has declared ability to transmit electronically complete summary data</td>"));
			Assert(sentMail.Body.Contains("<td>District/Port of Entry</td><td>PORT</td>"));
			Assert(sentMail.Body.Contains("<td>Entry Summary Filing Date</td><td>20-Jun-23</td>"));
		}

		public void TestEmailDoesNotContainSSNNumber()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JF123-45-6789 TT0516231EPORT062023                                              " +
					"Y  1101SV9ER00000");

			var message2 = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JF123456654321TT0516231EPORT062023                                              " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);
			message2.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message2.EM_Status);

			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert("Email does not contain SSN Number", !sentMail.Body.Contains("<td>Importer of Record Number</td><td>123-45-6789</td>"));
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[1];
			Assert("EIN Number or CBP Number included in the mail", sentMail.Body.Contains("<td>Importer of Record Number</td><td>123456654321</td>"));
		}

		public void TestGenerateJG()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JG123Y566543211051                                                              " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Number of Withdrawals</td><td>123</td>"));
			Assert(sentMail.Body.Contains("<td>Warehouse Final Withdrawal Indicator</td><td>Y</td>"));
			Assert(sentMail.Body.Contains("<td>Import Specialist Team</td><td>566</td>"));
			Assert(sentMail.Body.Contains("<td>Center ID</td><td>543211</td>"));
			Assert(sentMail.Body.Contains("<td>Number of Line Items</td><td>051</td>"));
		}

		public void TestGenerateJH()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JH123456654321051623987654321                                                   " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>CF-4811 Reference Number</td><td>123456654321</td>"));
			Assert(sentMail.Body.Contains("<td>Preliminary Statement Print Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Broker Reference Number</td><td>987654321</td>"));
		}

		public void TestGenerateJI()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JI123Y0NN123456789             12       256                                     " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Surety Code</td><td>123</td>"));
			Assert(sentMail.Body.Contains("<td>Primary Surety Indicator</td><td>Primary Surety</td>"));
			Assert(sentMail.Body.Contains("<td>Bond Type Code</td><td>No Bond Required</td>"));
			Assert(sentMail.Body.Contains("<td>Bond Designation Type Code</td><td>CRM New Bond Added</td>"));
			Assert(sentMail.Body.Contains("<td>Multiple Bonds Indicator</td><td>This is the only bond obligated.</td>"));
			Assert(sentMail.Body.Contains("<td>Bond Number</td><td>123456789</td>"));
			Assert(sentMail.Body.Contains("<td>Single Entry Bond Amount</td><td>0.12</td>"));
			Assert(sentMail.Body.Contains("<td>Surety Liability Amount</td><td>256</td>"));
		}

		public void TestGenerateJI_01()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JI 891 NO BOND ON FILE IN ACE EBOND                                             " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Surety Code</td><td>891</td>"));
			Assert(sentMail.Body.Contains("<td>Narrative Text</td><td>NO BOND ON FILE IN ACE EBOND</td>"));
		}

		public void TestGenerateJI_01_SuretyCodeIsEmpty()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JI     NO BOND ON FILE IN ACE EBOND                                             " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Surety Code</td><td>&nbsp;</td>"));
			Assert(sentMail.Body.Contains("<td>Narrative Text</td><td>NO BOND ON FILE IN ACE EBOND</td>"));
		}

		public void TestGenerateJJ()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JJ1234566543211AP0516231                                                        " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Protest Number</td><td>123456654321</td>"));
			Assert(sentMail.Body.Contains("<td>Protest Type</td><td>514 Protest Section</td>"));
			Assert(sentMail.Body.Contains("<td>Protest Status</td><td>Approved</td>"));
			Assert(sentMail.Body.Contains("<td>Protest Decision Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Summons Indicator</td><td>Summons</td>"));
		}

		public void TestGenerateJK()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JK123456654320516231 1         12         15         13         14              " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Bill Number</td><td>12345665432</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Type</td><td>Deferred Tax</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Collection Status</td><td>Not Paid</td>"));
			Assert(sentMail.Body.Contains("<td>Total Bill Amount</td><td>0.12</td>"));
			Assert(sentMail.Body.Contains("<td>Paid Amount</td><td>0.15</td>"));
			Assert(sentMail.Body.Contains("<td>Principal Amount</td><td>0.13</td>"));
			Assert(sentMail.Body.Contains("<td>Interest Amount</td><td>0.14</td>"));
		}

		public void TestGenerateJK_01()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JK BILLING DATA NOT ON FILE                                                     " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Narrative Text</td><td>BILLING DATA NOT ON FILE</td>"));
		}

		public void TestGenerateJL()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JL051623         12                                                             " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Collection Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Total Amount</td><td>0.12</td>"));
		}

		public void TestGenerateJL_01()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JL COLLECTION DATA NOT ON FILE                                                  " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Narrative Text</td><td>COLLECTION DATA NOT ON FILE</td>"));
		}

		public void TestGenerateJM()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JM0510000000-012                                                                " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();

			AssertNoExceptionThrown(delegate
			{
				var entryStatus = ((IEntryStatusProvider)message).EntryStatus;
			});

			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Class Code</td><td>051</td>"));
			Assert(sentMail.Body.Contains("<td>Class Code Amount</td><td>-0.12</td>"));
		}

		public void TestGenerateJN()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JNSV9Y05162312345678912051623110         12         13         14         15    " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Surety Code</td><td>SV9</td>"));
			Assert(sentMail.Body.Contains("<td>Primary Surety Indicator</td><td>Primary Surety</td>"));
			Assert(sentMail.Body.Contains("<td>612 Report Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Number</td><td>12345678912</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Date</td><td>16-May-23</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Type</td><td>Deferred Tax</td>"));
			Assert(sentMail.Body.Contains("<td>Bill Collection Status</td><td>Canceled with Partial Payment</td>"));
			Assert(sentMail.Body.Contains("<td>Total Bill Amount</td><td>0.12</td>"));
			Assert(sentMail.Body.Contains("<td>Paid Amount</td><td>0.13</td>"));
			Assert(sentMail.Body.Contains("<td>Principal Amount</td><td>0.14</td>"));
			Assert(sentMail.Body.Contains("<td>Interest Amount</td><td>0.15</td>"));
		}

		public void TestGenerateJN_01()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JN 037 Y BILLING DATA NOT ON FILE                                               " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Surety Code</td><td>037</td>"));
			Assert(sentMail.Body.Contains("<td>Primary Surety Indicator</td><td>Primary Surety</td>"));
			Assert(sentMail.Body.Contains("<td>Narrative Text</td><td>BILLING DATA NOT ON FILE</td>"));
		}

		public void TestGenerateJN_01_SuretyCodeIsEmpty()
		{
			var message = CreateMessage("58316",
					"B001101SV9ER                                               58316                " +
					"JA EES 050111000000AM050211115959PM                                             " +
					"JBSV9  7002293200100061911093801PM       Y2                                     " +
					"JN     Y BILLING DATA NOT ON FILE                                               " +
					"Y  1101SV9ER00000");
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Received, message.EM_Status);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(sentMail.Body.Contains("<td>Surety Code</td><td>&nbsp;</td>"));
			Assert(sentMail.Body.Contains("<td>Primary Surety Indicator</td><td>Primary Surety</td>"));
			Assert(sentMail.Body.Contains("<td>Narrative Text</td><td>BILLING DATA NOT ON FILE</td>"));
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.US.USJobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "SV9";

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			dec.ActiveEntryHeaders.EntrySummaryEntry.EntryNumber = entryNumber;

			Factory.Save();
			return dec;
		}

		MQEDIMessage CreateMessage(ZString messageNum, ZString receiveMessageText, string originalMessageText = "")
		{
			var outgoingMessage = Factory.New<MQEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoingMessage.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			outgoingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQuery;
			outgoingMessage.EM_MessageOwner = Constants.ACE;
			if (!string.IsNullOrEmpty(originalMessageText))
			{
				outgoingMessage.EM_MessageText = originalMessageText;
			}

			var message = GetMessageToProcess();
			message.EM_MessageText = receiveMessageText;
			Factory.Save();
			message.EM_MessageNum = messageNum;
			outgoingMessage.EM_MessageNum = messageNum;
			Factory.Save();
			return message;
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse;
			message.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			return message;
		}
	}
}
