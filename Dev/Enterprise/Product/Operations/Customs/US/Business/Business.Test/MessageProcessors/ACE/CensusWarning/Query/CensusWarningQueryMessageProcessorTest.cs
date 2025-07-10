using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Archtecture = Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class CensusWarningQueryMessageProcessorTest : ABIProcessorTest<CensusWarningMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
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

			var declaration = GetMergedDeclaration("00000032");
			declaration.JE_GB = newBranch.PK;

			var outgoingmessage = GetOutgoingMessage();
			outgoingmessage.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			outgoingmessage.EM_MessageNum = "546";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "546";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			message.EM_MessageText = "B003902SV9CL                                               546                  CJ23902022211SV9  00000032  001350699000027C                                    Y  3902SV9CL00001";
			AssertEquals("Precondition: linked object should be null", null, message.EM_LinkedObject);

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);
			AssertContains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber, Env.OutgoingCustomsMailManager.EmailsCreated[0].Subject);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments.Cast<Archtecture.AttachmentDef>().FirstOrDefault(x => x.DisplayName == "Banner.jpg");
			var image = new System.Drawing.Bitmap(new System.IO.MemoryStream(banner.Data));
			AssertEquals(2, image.Width);
		}

		public void TestMesageWithMultipleEntries()
		{
			var outgoingmessage = GetOutgoingMessage();
			Factory.Save();

			outgoingmessage.EM_MessageNum = "552";

			var message = GetMessageToProcess();
			message.EM_MessageNum = "552";
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			message.EM_MessageText = "B003902SV9CL                                               552                  CJ23902022211SV9  00000040  001          27H                                    CJ23902022211SV9  00000040  001350699000027C                                    CJ23902022211SV9  00000040  002270112001027C                                    CJ23902022211SV9  00000032  001350699000027C                                    CJ23902022211SV9  00000024  001350699000027C                                    Y  3902SV9CL00005";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Linked Object should be null, because message for multiple entries", null, message.EM_LinkedObject);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<Archtecture.EmailDef>(delegate(Archtecture.EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("Census Warning Query Response for multiple entries"); }));
			Assert("Message Body should contains Entry Number which is not a hyperlink, because job was not found",
				sentMail.Body.Contains("<b>SV9-0000004-0 - Entered into 3902 at 22-Feb-11</b>"));

			Assert("Message Body should contains Entry Number which is not a hyperlink, because job was not found",
				sentMail.Body.Contains("<b>SV9-0000003-2 - Entered into 3902 at 22-Feb-11</b>"));

			Assert("Message Body should contains Entry Number which is not a hyperlink, because job was not found",
				sentMail.Body.Contains("<b>SV9-0000002-4 - Entered into 3902 at 22-Feb-11</b>"));

			var declaration1 = GetMergedDeclaration("00000040");
			var declaration2 = GetMergedDeclaration("00000032");
			var declaration3 = GetMergedDeclaration("00000024");
			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals("Linked Object should be null, because message for multiple entries", null, message.EM_LinkedObject);

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<Archtecture.EmailDef>(delegate(Archtecture.EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("Census Warning Query Response for multiple entries"); }));
			Assert("Message Body should contains Entry Number with hyperlink",
				sentMail.Body.Contains("B00001000/SV9-0000004-0</a> - Entered into 3902 at 22-Feb-11</b>"));

			Assert("Message Body should contains Entry Number with hyperlink",
				sentMail.Body.Contains("B00001001/SV9-0000003-2</a> - Entered into 3902 at 22-Feb-11</b>"));

			Assert("Message Body should contains Entry Number with hyperlink",
				sentMail.Body.Contains("B00001002/SV9-0000002-4</a> - Entered into 3902 at 22-Feb-11</b>"));
		}

		public void TestFailedMessage()
		{
			var declaration = GetMergedDeclaration("00000073");

			var outgoingmessage1 = GetOutgoingMessage();
			outgoingmessage1.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var outgoingmessage2 = GetOutgoingMessage();
			Factory.Save();

			outgoingmessage2.EM_MessageNum = "564";
			outgoingmessage2.EM_MessageText = "B  3902SV9CJ                                               564                  CJ1                SV9  00000073                                                Y  3902SV9CJ";
			outgoingmessage2.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			outgoingmessage1.EM_MessageNum = "544";

			var response1 = GetMessageToProcess();
			response1.EM_MessageNum = "544";
			response1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			response1.EM_MessageText = "B003902SV9CL                                               544                  CJ20000                                       003ENTRY FILER CODE MISSING       Y  3902SV9CL00001";
			AssertEquals("Precondition: linked object should be null", null, response1.EM_LinkedObject);

			var response2 = GetMessageToProcess();
			response2.EM_MessageNum = "564";
			response2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			response2.EM_MessageText = "B003902SV9CL                                               564                  CJ20000      SV9  00000073                    006ENTRY SUMMARY NOT FOUND FOR QU Y  3902SV9CL00001";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			response1.Reload();
			AssertEquals(MQEDIMessage.Status.Received, response1.EM_Status);
			AssertEquals("Failed message should be attached to the Entry", declaration.ActiveEntryHeaders.EntrySummaryEntry, response1.EM_LinkedObject);

			response2.Reload();
			AssertEquals(MQEDIMessage.Status.Received, response2.EM_Status);
			AssertEquals("Second response message should be attached to the Entry", declaration.ActiveEntryHeaders.EntrySummaryEntry, response2.EM_LinkedObject);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<Archtecture.EmailDef>(delegate(Archtecture.EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("ENTRY FILER CODE MISSING"); }));
			Assert("Message Body should contains error text",
				sentMail.Body.Contains("Census Warning Query Response (Failure)") && sentMail.Body.Contains("ENTRY FILER CODE MISSING"));

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<Archtecture.EmailDef>(delegate(Archtecture.EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("ENTRY SUMMARY NOT FOUND FOR QU"); }));
			Assert("Message Body should contains error text",
				sentMail.Body.Contains("Census Warning Query Response (Failure)") && sentMail.Body.Contains("ENTRY SUMMARY NOT FOUND FOR QU"));
		}

		public void TestNoSummaries()
		{
			var declaration = GetMergedDeclaration("00000065");

			var outgoingmessage1 = GetOutgoingMessage();
			var outgoingmessage2 = GetOutgoingMessage();
			var outgoingmessage3 = GetOutgoingMessage();
			Factory.Save();

			outgoingmessage1.EM_MessageNum = "6008953";
			outgoingmessage1.EM_MessageText = "B  3902SV9CJ                                               6008953              CJ1    030311031011SV9                                                          Y  3902SV9CJ";

			outgoingmessage2.EM_MessageNum = "567";
			outgoingmessage2.EM_MessageText = "B  3902SV9CJ                                               567                  CJ1                SV9  00000065                                                Y  3902SV9CJ";
			outgoingmessage2.EM_LinkedObject = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			outgoingmessage3.EM_MessageNum = "604";
			outgoingmessage3.EM_MessageText = "B  3902SV9CJ                                               604                  CJ13901030111030111SV9                                                          Y  3902SV9CJ";

			var response1 = GetMessageToProcess();
			response1.EM_MessageNum = "6008953";
			response1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			response1.EM_MessageText = "B003902SV9CL                                               6008953              CJ20000      SV9                              016QUERY COMPLETE - NO SUMMARIES  Y  3902SV9CL00001";

			var response2 = GetMessageToProcess();
			response2.EM_MessageNum = "567";
			response2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			response2.EM_MessageText = "B003902SV9CL                                               567                  CJ23902022411SV9  00000065                    007NO UNRSLVED WARNINGS FOUND FOR Y  3902SV9CL00001";

			var response3 = GetMessageToProcess();
			response3.EM_MessageNum = "604";
			response3.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse;
			response3.EM_MessageText = "B003902SV9CL                                               604                  CJ23901      SV9                              016QUERY COMPLETE - NO SUMMARIES  Y  3902SV9CL00001";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			response1.Reload();
			AssertEquals(MQEDIMessage.Status.Received, response1.EM_Status);
			AssertEquals("Linked Object should be null, because message for multiple entries", null, response1.EM_LinkedObject);

			response2.Reload();
			AssertEquals(MQEDIMessage.Status.Received, response2.EM_Status);
			AssertEquals("Second response message should be attached to the Entry", declaration.ActiveEntryHeaders.EntrySummaryEntry, response2.EM_LinkedObject);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert("Message Body should contains message narrative text",
				sentMail.Body.Contains("Census Warning Query Response for B00001000") &&
				sentMail.Body.Contains("No unresolved census warnings found for Entry SV9-00000065."));

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[1];
			Assert("Message Body should contains Port and date range",
					sentMail.Body.Contains("No entries with census warning exist for the query. You have queried for Entry Port 3901 and Date From 01-Mar-11 to 01-Mar-11"));

			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[2];
			Assert("Message Body should contains date range: No entries with census warning exist from 03-Mar-11 to 10-Mar-11",
					sentMail.Body.Contains("No entries with census warning exist for the query. You have queried for Date From 03-Mar-11 to 10-Mar-11"));
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "SV9";
			dec.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			dec.ImportEntryNumber = entryNumber;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		MQEDIMessage GetOutgoingMessage()
		{
			var outgoingmessage = GetMessageToProcess();
			outgoingmessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CensusWarningQuery;
			outgoingmessage.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			outgoingmessage.EM_MessageText = MQEDIMessage.USEntryNumberPlaceHolder + " " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			return outgoingmessage;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}
	}
}
