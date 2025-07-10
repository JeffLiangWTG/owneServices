using System;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class BounceBackEmailProcessorTest : TestCaseWithFactory
	{
		public void TestCanRunInAnyBranch()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			_ = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			var item = GetBounceBackMailItem(shipment, "Delivery Summary");

			Factory.Save();

			using (Env.Instance.TemporaryServiceTaskContext(MailFilterCodes.MailProcessingTask, canRunInAnyBranch: true))
			{
				_ = Processor.ProcessMailItem(item, TestLogger);
			}

			Assert("Bounce email should have been processed successfully", TestLogger.Logs.Any(log => log.Message.Contains("Bounce email response received from <from-address@example.com> is processed successfully.")));
			AssertNullOrEmpty("No Errors should be reported", ErrorReporter.LastMessageReported);
		}

		public void TestMessageFilter()
		{
			MailItem item = GetBounceBackMailItem(null, "");
			item.MI_Subject = "Good old days";
			Factory.Save();

			var helper = new MessageFilterTestHelper<BounceBackEmailProcessor, MailItem>();
			var ctx = helper.ProcessorFactory.GetContext(helper.Log);
			BounceBackEmailProcessor processor = ctx.GetFilterInstance<BounceBackEmailProcessor>();

			int prevCount = Env.OutgoingMailManager.EmailsCreated.Count;
			Assert(!helper.Process(item));

			MailItem item2 = GetBounceBackMailItem(null, "");
			item.MI_Subject = "Delivery failure";
			Factory.Save();

			bool result = helper.Process(item2);
			AssertEquals($@"Information|Bounce email response received from <from-address@example.com> is processed successfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item2.PK}
SentTime: Tue, 8 Jul 2014 15:56:53 +1000
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: powell@live.com
BounceReasonCode: UNV
BusinessEntityID: 00000000-0000-0000-0000-000000000000
BusinessEntityTableCode: 
BusinessEntityInDatabase: False
JobNumber: 
DocumentName: 
", helper.Log[0]);
		}

		public void TestMessageFilter_FailedTransmission()
		{
			AssertMessageFilter("Failed transmission", false);
			AssertMessageFilter("wazup Failed transmission 12345", false);
			AssertMessageFilter("returned Failed transmission", false);
			AssertMessageFilter("fail Failed transmission fail", false);
			AssertMessageFilter("", false);
			AssertMessageFilter("heya", false);
			AssertMessageFilter("foil", false);
			AssertMessageFilter("fail", true);
			AssertMessageFilter("faill", true);
			AssertMessageFilter("everything failed", true);
			AssertMessageFilter("{EDIFAX} E459B077-9BD0-47CA-A0D3-76270094DCFE Failure", false);
			AssertMessageFilter("{EDIFAX} E459B077-9BD0-47CA-A0D3-76270094DCFE Fail", true);
			AssertMessageFilter("{EDIFAX}", false);
			AssertMessageFilter("E459B077-9BD0-47CA-A0D3-76270094DCFE Failure", true);
			AssertMessageFilter("{EDIFAX} Failure", true);
		}

		void AssertMessageFilter(string subject, bool shouldProcess)
		{
			MailItem item = GetBounceBackMailItem(null, "");
			item.MI_Subject = subject;
			Factory.Save();

			var helper = new MessageFilterTestHelper<BounceBackEmailProcessor, MailItem>();
			var ctx = helper.ProcessorFactory.GetContext(helper.Log);
			BounceBackEmailProcessor processor = ctx.GetFilterInstance<BounceBackEmailProcessor>();

			AssertEquals(shouldProcess, helper.Process(item));
		}

		[TestDate(2002, 2, 2)]
		public void TestProcessMailItems_CampaignItem()
		{
			TestProcessMailItems_CampaignItem(false);
		}

		[TestDate(2002, 2, 2)]
		public void TestProcessMailItems_CampaignItem_WithBounceEmailAsAttachment()
		{
			TestProcessMailItems_CampaignItem(true);
		}

		void TestProcessMailItems_CampaignItem(bool bounceEmailAsAttachment)
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Bob Powell";
			contact.OC_Email = "powell@live.com";

			var campaignItem = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem.G8_TrackingStatus = "UNV";
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = "OC";

			MailItem item = GetBounceBackMailItem(campaignItem, "", bounceEmailAsAttachment);
			item.MI_Body += new ZString('a', 4100);
			Factory.Save();

			bool result = Processor.ProcessMailItem(item, TestLogger);
			AssertEquals("ProcessedMails", true, result);
			AssertEquals("Campaign Item Tracking Status changed to bounced", "NDR", campaignItem.G8_TrackingStatus);
			AssertEquals($@"Bounce email response received from <from-address@example.com> is processed successfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item.PK}
SentTime: Tue, 8 Jul 2014 15:56:53 +1000
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: powell@live.com
BounceReasonCode: UNV
BusinessEntityID: {campaignItem.PK}
BusinessEntityTableCode: G8
BusinessEntityInDatabase: True
JobNumber: 
DocumentName: 
", TestLogger.Logs[0].Message);

			var campaignItemNotes = campaignItem.Notes.GetAllNotes();
			AssertEquals(1, campaignItemNotes.Count);
			var campaignItemNote = campaignItemNotes.Cast<StmNote>().Single();
			AssertEquals(BounceBackEmailProcessor.EmailNoteDescription, campaignItemNote.ST_Description);
			AssertEquals(4000 + "<UNV> Unknown delivery failure\r\n".Length, campaignItemNote.ST_NoteDataAsText.Length);

			var glbEmailAddress = GlbEmailAddress.Load(Factory, contact.OC_Email);
			AssertNotNull("Should have created a GlbEmailAddress", glbEmailAddress);
			AssertEquals("Should have marked it as NDR", EmailDeliveryReportStatus.Codes.NonDeliveryReport, glbEmailAddress.GI_DeliveryStatus);
			AssertEquals(new ZDateTime(2002, 2, 2), glbEmailAddress.GI_DeliveryReportTimeUtc);

			var emailAddressNotes = glbEmailAddress.Notes.GetAllNotes();
			AssertEquals(1, emailAddressNotes.Count);
			var emailAddressNote = emailAddressNotes.Cast<StmNote>().Single();
			AssertEquals(BounceBackEmailProcessor.EmailNoteDescription, emailAddressNote.ST_Description);
		}

		[TestDate(2002, 2, 2)]
		public void TestProcessMailItems_Shipment()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			MailItem item = GetBounceBackMailItem(shipment, "Delivery Summary");

			Factory.Save();

			Processor.ProcessMailItem(item, TestLogger);

			var glbEmailAddress = GlbEmailAddress.Load(Factory, "powell@live.com");
			AssertNotNull(glbEmailAddress);
			AssertEquals(EmailDeliveryReportStatus.Codes.NonDeliveryReport, glbEmailAddress.GI_DeliveryStatus);
			AssertEquals(new ZDateTime(2002, 2, 2), glbEmailAddress.GI_DeliveryReportTimeUtc);

			var emailAddressNotes = glbEmailAddress.Notes.GetAllNotes();
			AssertEquals(1, emailAddressNotes.Count);
			var emailAddressNote = emailAddressNotes.Cast<StmNote>().Single();
			AssertEquals(BounceBackEmailProcessor.EmailNoteDescription, emailAddressNote.ST_Description);

			var dndLog = shipment.GetLogs().Find(x => x.SL_SE_NKEvent == Events.DocumentNotDeliveredCode).Single();
			AssertEquals(Events.DocumentNotDeliveredCode, dndLog.SL_SE_NKEvent);
			AssertEquals("|NAM=powell@live.com|RES=NDR|TYP=Delivery Summary", dndLog.SL_Reference);
			AssertEquals($@"Bounce email response received from <from-address@example.com> is processed successfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item.PK}
SentTime: Tue, 8 Jul 2014 15:56:53 +1000
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: powell@live.com
BounceReasonCode: UNV
BusinessEntityID: {shipment.PK}
BusinessEntityTableCode: JS
BusinessEntityInDatabase: True
JobNumber: {job.JH_JobNum}
DocumentName: Delivery Summary
", TestLogger.Logs[0].Message);
		}

		[TestDate(2002, 2, 2)]
		public void TestProcessMailItems_Shipment_Job()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();

			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			MailItem item = GetBounceBackMailItem(job, "Delivery Summary");

			Factory.Save();

			Processor.ProcessMailItem(item, TestLogger);

			var glbEmailAddress = GlbEmailAddress.Load(Factory, "powell@live.com");
			AssertNotNull(glbEmailAddress);
			AssertEquals(EmailDeliveryReportStatus.Codes.NonDeliveryReport, glbEmailAddress.GI_DeliveryStatus);
			AssertEquals(new ZDateTime(2002, 2, 2), glbEmailAddress.GI_DeliveryReportTimeUtc);

			var emailAddressNotes = glbEmailAddress.Notes.GetAllNotes();
			AssertEquals(1, emailAddressNotes.Count);
			var emailAddressNote = emailAddressNotes.Cast<StmNote>().Single();
			AssertEquals(BounceBackEmailProcessor.EmailNoteDescription, emailAddressNote.ST_Description);

			var dndLog = job.GetLogs().Find(x => x.SL_SE_NKEvent == Events.DocumentNotDeliveredCode).Single();
			AssertEquals(Events.DocumentNotDeliveredCode, dndLog.SL_SE_NKEvent);
			AssertEquals("|NAM=powell@live.com|RES=NDR|TYP=Delivery Summary", dndLog.SL_Reference);
			AssertEquals($@"Bounce email response received from <from-address@example.com> is processed successfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item.PK}
SentTime: Tue, 8 Jul 2014 15:56:53 +1000
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: powell@live.com
BounceReasonCode: UNV
BusinessEntityID: {job.PK}
BusinessEntityTableCode: JH
BusinessEntityInDatabase: True
JobNumber: {job.JH_JobNum}
DocumentName: Delivery Summary
", TestLogger.Logs[0].Message);
		}

		[TestDate(2002, 2, 2)]
		public void TestProcessMailItems_Shipment_WithVeryLongDocumentName()
		{
			var shipment = (BusinessObject)Factory.New<ICommonConsol>();
			shipment.FillWithValidTestData();

			MailItem item = GetBounceBackMailItem(shipment, "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#");

			Factory.Save();

			Processor.ProcessMailItem(item, TestLogger);

			var emailAddress = GlbEmailAddress.Load(Factory, "powell@live.com");
			AssertNotNull(emailAddress);
			AssertEquals(EmailDeliveryReportStatus.Codes.NonDeliveryReport, emailAddress.GI_DeliveryStatus);
			AssertEquals(new ZDateTime(2002, 2, 2), emailAddress.GI_DeliveryReportTimeUtc);

			var emailAddressNotes = emailAddress.Notes.GetAllNotes();
			AssertEquals(1, emailAddressNotes.Count);
			var emailAddressNote = emailAddressNotes.Cast<StmNote>().Single();
			AssertEquals(BounceBackEmailProcessor.EmailNoteDescription, emailAddressNote.ST_Description);

			var dndLog = shipment.GetLogs().Find(x => x.SL_SE_NKEvent == Events.DocumentNotDeliveredCode).Single();
			AssertEquals(StmALog.Schema.SL_ReferenceMaxLength, dndLog.SL_Reference.Length);
			AssertEquals("|NAM=powell@live.com|RES=NDR|TYP=SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ "SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#S", dndLog.SL_Reference);
			AssertEquals($@"Bounce email response received from <from-address@example.com> is processed successfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item.PK}
SentTime: Tue, 8 Jul 2014 15:56:53 +1000
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: powell@live.com
BounceReasonCode: UNV
BusinessEntityID: {shipment.PK}
BusinessEntityTableCode: JK
BusinessEntityInDatabase: True
JobNumber: {shipment["JK_UniqueConsignRef"]}
DocumentName: SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ @"SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ @"SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ @"SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#"
				+ @"SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#SuperLongDocumentName!SuperLongDocumentName@SuperLongDocumentName#
", TestLogger.Logs[0].Message);
		}

		public void TestGetLastBounceBackEmailNote()
		{
			var address1 = Factory.New<GlbEmailAddress>();
			address1.GI_EmailAddress = "one@test.org";

			var address2 = Factory.New<GlbEmailAddress>();
			address2.GI_EmailAddress = "two@test.com";

			StmNote note1 = Factory.NewWithValidTestData<StmNote>();
			note1.ST_Description = "Email bounce back";
			note1.ST_Table = ViewCampaignContactSchema.Constants.TableName;
			note1.ST_ParentID = address1.PK;
			note1.ST_Table = address1.TableName;

			StmNote note2 = Factory.NewWithValidTestData<StmNote>();
			note2.ST_Description = "Email bounce back";
			note2.ST_Table = ViewCampaignContactSchema.Constants.TableName;
			note2.ST_ParentID = address2.PK;
			note2.ST_Table = address2.TableName;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			AssertEquals(note1.PK, BounceBackEmailProcessor.GetLastBounceBackEmailNote(factory2.LoadFromNaturalKey<GlbEmailAddress>(GlbEmailAddressSchema.GI_EmailAddress, address1.GI_EmailAddress)).PK);
			AssertEquals(note2.PK, BounceBackEmailProcessor.GetLastBounceBackEmailNote(factory2.LoadFromNaturalKey<GlbEmailAddress>(GlbEmailAddressSchema.GI_EmailAddress, address2.GI_EmailAddress)).PK);
			AssertEquals("not hitting log table", 0, factory2.GetTableHitCount(StmALogSchema.Constants.TableName));
		}

		public void TestAddEmailBodyAsNote()
		{
			var address1InDb = Factory.New<GlbEmailAddress>();
			address1InDb.GI_EmailAddress = "one@test.org";
			Factory.Save();

			var address2NotInDb = Factory.New<GlbEmailAddress>();
			address2NotInDb.GI_EmailAddress = "two@test.com";

			var processor = new BounceBackEmailProcessor();
			processor.AddEmailBodyAsNote(address1InDb, "test body 1", "UNV");
			processor.AddEmailBodyAsNote(address2NotInDb, "test body 3", "UNV");
			Factory.Save();

			processor.AddEmailBodyAsNote(address1InDb, "test body 2", "UNV");
			processor.AddEmailBodyAsNote(address2NotInDb, "test body 4", "UNV");
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var addr1 = factory2.LoadFromNaturalKey<GlbEmailAddress>(GlbEmailAddressSchema.GI_EmailAddress, address1InDb.GI_EmailAddress);
			var addr2 = factory2.LoadFromNaturalKey<GlbEmailAddress>(GlbEmailAddressSchema.GI_EmailAddress, address2NotInDb.GI_EmailAddress);
			AssertEquals("later note overwrites previous", 1, addr1.Notes.GetAllNotes().Count);
			AssertEquals(1, addr2.Notes.GetAllNotes().Count);
			AssertEquals("<UNV> Unknown delivery failure\r\ntest body 2", addr1.Notes.GetAllNotes().Cast<StmNote>().First().ST_NoteDataAsText);
			AssertEquals("<UNV> Unknown delivery failure\r\ntest body 4", addr2.Notes.GetAllNotes().Cast<StmNote>().First().ST_NoteDataAsText);
		}

		public void TestProcessMailItems_InvalidRecipient()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();

			var item = GetBounceBackMailItem(shipment, "Delivery Summary");
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var bounceDetails = new BounceBackEmailDetailsForTest(factory, Encoding.UTF8.GetBytes(item.MI_Body), Encoding.UTF8.GetBytes(item.MI_Header));

			Assert(Processor.ProcessEmailCore(bounceDetails, factory, Array.Empty<ZString>()));
		}

		public void TestProcessMailItems_NoRecipients()
		{
			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();

			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();
			var item = GetBounceBackMailItem(job, "Delivery Summary");
			item.MI_Body = "ERROR";

			Processor.ProcessMailItem(item, TestLogger);

			AssertEquals($@"Bounce email response received from <from-address@example.com> is processed unsuccessfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item.PK}
SentTime: 
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: 
BounceReasonCode: UNV
BusinessEntityID: 00000000-0000-0000-0000-000000000000
BusinessEntityTableCode: 
BusinessEntityInDatabase: False
JobNumber: 
DocumentName: 
", TestLogger.Logs[0].Message);
		}

		public void TestProcessMailItems_DiagnositicInfo()
		{
			var mailBody =
				@"Date=Mon, 21 May 2018 05:00:49 -0500
Message-ID=<135bb06f-31ae-40a5-b7c6-f096424eff73>
Subject=Bill of Lading - S00001000
From=Sender
To=User
X-SenderStaffID=0d0fc780-c2d1-4609-a85c-7a8a99bf3c54
X-BusinessEntityID=94b60495-b50e-4dc7-8150-7d308d7d83e1
X-BusinessEntityTableCode=JS";

			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var item = GetBounceBackMailItem(shipment, "Delivery Summary", Env.CurrentUser.PK, false, mailBody);

			Factory.Save();

			Processor.ProcessMailItem(item, TestLogger);

			var expectedLog =
				$@"Bounce email response received from <from-address@example.com> is processed unsuccessfully.
Subject: Undeliverable: Send from Wisegrid A
MailItemPK: {item.PK}
SentTime: 
SenderStaffID: 00000000-0000-0000-0000-000000000000
BouncedRecipients: 
BounceReasonCode: UNV
BusinessEntityID: 00000000-0000-0000-0000-000000000000
BusinessEntityTableCode: 
BusinessEntityInDatabase: False
JobNumber: 
DocumentName: 

Human Readable Part:

Date=Mon, 21 May 2018 05:00:49 -0500
Message-ID=<135bb06f-31ae-40a5-b7c6-f096424eff73>
Subject=Bill of Lading - S00001000
From=Sender
To=User
X-SenderStaffID=0d0fc780-c2d1-4609-a85c-7a8a99bf3c54
X-BusinessEntityID=94b60495-b50e-4dc7-8150-7d308d7d83e1
X-BusinessEntityTableCode=JS

Original Message Header Part:


";
			AssertEquals(expectedLog, TestLogger.Logs[0].Message);
		}

		#region Notification Email

		public void TestSendNotificationEmail_SenderStaff()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Email body: (*DocumentName*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.com";

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", staff.PK);

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(mailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, notificationEmail.Recipients.Count);
			AssertEquals("aaa@123.com", notificationEmail.Recipients[0].Email);
			AssertEquals("Email subject", notificationEmail.Subject);
			AssertEquals("Email body: Delivery Summary", notificationEmail.Body);
		}

		public void TestSendNotificationEmail_BouncedRecipientWithNonNDR()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var groupStaff = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff.GS_Code = "NDR";
			groupStaff.GS_EmailAddress = "ndrGroup@123.com";

			group.Staff.Add(groupStaff);

			using (SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mailAddress = Factory.NewWithValidTestData<GlbEmailAddress>();
				mailAddress.GI_EmailAddress = "aaa@123.com";
				mailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "AAA";
				staff.GS_EmailAddress = "aaa@123.com";

				var mailItem = Factory.New<MailItem>();
				mailItem.MI_Status = MailStatus.Queued;
				mailItem.MI_Direction = MailDirection.Receive;
				mailItem.MI_SendDateTime = ZDateTime.UtcNow;
				mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
				mailItem.AddRecipientForUserCommunication("Deliverance Update <address@edi.com.au>", MailRecipient.RecipientTypes.TO);
				mailItem.MI_Subject = "Undeliverable: bounced recipient with NonNDR";
				mailItem.MI_From = mailAddress.GI_EmailAddress;

				var body = string.Format(testNDRMailBody, mailAddress.GI_EmailAddress, staff.PK);
				mailItem.MI_Body = body;

				staff.GS_EmailAddress = "AAA@123.com";

				Factory.Save();

				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var result = Processor.ProcessMailItem(mailItem, TestLogger);

				AssertEquals("Successful process", true, result);
				AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
				var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, notificationEmail.Recipients.Count);
				AssertEquals("ndrGroup@123.com", notificationEmail.Recipients[0].Email);
				AssertEquals("Email Non-delivery Receipt: aaa@123.com", notificationEmail.Subject);
				AssertEquals(0, notificationEmail.Attachments.Count);
			}
		}

		public void TestSendNotificationEmail_BouncedRecipientWithLessThan24HoursNDR()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var groupStaff = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff.GS_Code = "NDR";
			groupStaff.GS_EmailAddress = "ndrGroup@123.com";

			group.Staff.Add(groupStaff);

			using (SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mailAddress = Factory.NewWithValidTestData<GlbEmailAddress>();
				mailAddress.GI_EmailAddress = "aaa@123.com";
				mailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				mailAddress.GI_DeliveryReportTimeUtc = DateTime.UtcNow.AddHours(-12);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "AAA";
				staff.GS_EmailAddress = "aaa@123.com";

				var mailItem = Factory.New<MailItem>();
				mailItem.MI_Status = MailStatus.Queued;
				mailItem.MI_Direction = MailDirection.Receive;
				mailItem.MI_SendDateTime = ZDateTime.UtcNow;
				mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
				mailItem.AddRecipientForUserCommunication("Deliverance Update <address@edi.com.au>", MailRecipient.RecipientTypes.TO);
				mailItem.MI_Subject = "Undeliverable: bounced recipient with less than 24 hours NDR";
				mailItem.MI_From = mailAddress.GI_EmailAddress;

				var body = string.Format(testNDRMailBody, mailAddress.GI_EmailAddress, staff.PK);
				mailItem.MI_Body = body;

				Factory.Save();

				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var result = Processor.ProcessMailItem(mailItem, TestLogger);

				AssertEquals("Successful process", true, result);
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				staff.GS_EmailAddress = "AAA@123.com";
				mailAddress.GI_EmailAddress = "AAA@123.com";

				Factory.Save();

				result = Processor.ProcessMailItem(mailItem, TestLogger);

				AssertEquals("Ignore case-sensitive and Successful process", true, result);
				AssertEquals("Ignore case-sensitive and successful process", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestSendNotificationEmail_BouncedRecipientWithMoreThan24HoursNDR()
		{
			AssertNotificationEmailIsSentToBouncedRecipientWithNDRDate(DateTime.UtcNow.AddHours(-30));
		}

		public void TestSendNotificationEmail_BouncedRecipientWithEmptyNDRDate()
		{
			AssertNotificationEmailIsSentToBouncedRecipientWithNDRDate(ZDateTime.Empty);
		}

		void AssertNotificationEmailIsSentToBouncedRecipientWithNDRDate(ZDateTime ndrDate)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var groupStaff = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff.GS_Code = "NDR";
			groupStaff.GS_EmailAddress = "ndrGroup@123.com";

			group.Staff.Add(groupStaff);

			using (SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mailAddress = Factory.NewWithValidTestData<GlbEmailAddress>();
				mailAddress.GI_EmailAddress = "aaa@123.com";
				mailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				mailAddress.GI_DeliveryReportTimeUtc = ndrDate;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "AAA";
				staff.GS_EmailAddress = "aaa@123.com";

				var mailItem = Factory.New<MailItem>();
				mailItem.MI_Status = MailStatus.Queued;
				mailItem.MI_Direction = MailDirection.Receive;
				mailItem.MI_SendDateTime = ZDateTime.UtcNow;
				mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
				mailItem.AddRecipientForUserCommunication("Deliverance Update <address@edi.com.au>", MailRecipient.RecipientTypes.TO);
				mailItem.MI_Subject = "Undeliverable: bounced recipient with more than 24 hours NDR";
				mailItem.MI_From = mailAddress.GI_EmailAddress;

				var body = string.Format(testNDRMailBody, mailAddress.GI_EmailAddress, staff.PK);
				mailItem.MI_Body = body;

				Factory.Save();

				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var result = Processor.ProcessMailItem(mailItem, TestLogger);

				AssertEquals("Successful process", true, result);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, notificationEmail.Recipients.Count);
				AssertEquals("aaa@123.com", notificationEmail.Recipients[0].Email);
				AssertEquals("Email Non-delivery Receipt: aaa@123.com", notificationEmail.Subject);
				AssertEquals(1, notificationEmail.Attachments.Count);
				AssertEquals(ZString.Empty, mailAddress.GI_DeliveryStatus);
			}
		}

		public void TestSendNotificationEmail_BouncedRecipientWithValidSenderEmailAddress()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			var groupStaff = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff.GS_Code = "NDR";
			groupStaff.GS_EmailAddress = "ndrGroup@123.com";

			group.Staff.Add(groupStaff);

			using (SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var mailAddress = Factory.NewWithValidTestData<GlbEmailAddress>();
				mailAddress.GI_EmailAddress = "aaa@123.com";
				mailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
				mailAddress.GI_DeliveryReportTimeUtc = DateTime.UtcNow.AddHours(-12);

				var mailAddress2 = Factory.NewWithValidTestData<GlbEmailAddress>();
				mailAddress2.GI_EmailAddress = "bbb@123.com";
				mailAddress2.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "BBB";
				staff.GS_EmailAddress = "bbb@123.com";

				var mailItem = Factory.New<MailItem>();
				mailItem.MI_Status = MailStatus.Queued;
				mailItem.MI_Direction = MailDirection.Receive;
				mailItem.MI_SendDateTime = ZDateTime.UtcNow;
				mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
				mailItem.AddRecipientForUserCommunication("Deliverance Update <address@edi.com.au>", MailRecipient.RecipientTypes.TO);
				mailItem.MI_Subject = "Undeliverable: bounced recipient with Valid Sender Email Address";
				mailItem.MI_From = mailAddress.GI_EmailAddress;

				var body = string.Format(testNDRMailBody, mailAddress.GI_EmailAddress, staff.PK);
				mailItem.MI_Body = body;

				Factory.Save();

				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var result = Processor.ProcessMailItem(mailItem, TestLogger);

				AssertEquals("Successful process", true, result);
				AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
				var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, notificationEmail.Recipients.Count);
				AssertEquals("bbb@123.com", notificationEmail.Recipients[0].Email);
				AssertEquals("Email Non-delivery Receipt: aaa@123.com", notificationEmail.Subject);
				AssertEquals(1, notificationEmail.Attachments.Count);
			}
		}

		public void TestSendNotificationEmail_OriginalEmailReplyTo()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Bounce Mail Subject: (*NonDeliveryReceiptEmailSubject*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "powell@live.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.net";

			var originalMailItem = Factory.New<MailItem>();
			originalMailItem.AddRecipientForUserCommunication("powell@live.com", MailRecipient.RecipientTypes.TO);
			originalMailItem.MI_From = "Test User <test.user@123.net>";
			originalMailItem.MI_Status = MailStatus.Sent;
			originalMailItem.MI_Direction = MailDirection.Transmit;
			originalMailItem.MI_SendDateTime = ZDateTime.UtcNow.AddMinutes(-5);
			originalMailItem.MI_ReceivedDateTime = originalMailItem.MI_SendDateTime;
			originalMailItem.MI_Subject = "Send from Wisegrid A";
			originalMailItem.MI_ReplyTo = "aaa@123.net";

			var anotherOriginalMailItem = Factory.New<MailItem>();
			anotherOriginalMailItem.AddRecipientForUserCommunication("powell@live.com", MailRecipient.RecipientTypes.TO);
			anotherOriginalMailItem.MI_From = "Test User <test.user@123.net>";
			anotherOriginalMailItem.MI_Status = MailStatus.Sent;
			anotherOriginalMailItem.MI_Direction = MailDirection.Transmit;
			anotherOriginalMailItem.MI_SendDateTime = ZDateTime.UtcNow.AddMinutes(-3);
			anotherOriginalMailItem.MI_ReceivedDateTime = anotherOriginalMailItem.MI_SendDateTime;
			anotherOriginalMailItem.MI_Subject = "Send from Wisegrid B";
			anotherOriginalMailItem.MI_ReplyTo = "bbb@123.net";

			var bounceMailItem = GetBounceBackMailItem(campaignItem, "");

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(bounceMailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, notificationEmail.Recipients.Count);
			AssertEquals("aaa@123.net", notificationEmail.Recipients[0].Email);
			AssertEquals("Email subject", notificationEmail.Subject);
			AssertEquals("Bounce Mail Subject: Undeliverable: Send from Wisegrid A", notificationEmail.Body);
		}

		public void TestSendNotificationEmail_OriginalEmailFrom()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Bounce Mail Subject: (*NonDeliveryReceiptEmailSubject*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "powell@live.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.net";

			var originalMailItem = Factory.New<MailItem>();
			originalMailItem.AddRecipientForUserCommunication("powell@live.com", MailRecipient.RecipientTypes.TO);
			originalMailItem.MI_From = "AAA <aaa@123.net>";
			originalMailItem.MI_Status = MailStatus.Sent;
			originalMailItem.MI_Direction = MailDirection.Transmit;
			originalMailItem.MI_SendDateTime = ZDateTime.UtcNow.AddMinutes(-5);
			originalMailItem.MI_ReceivedDateTime = originalMailItem.MI_SendDateTime;
			originalMailItem.MI_Subject = "Send from Wisegrid A";

			var anotherOriginalMailItem = Factory.New<MailItem>();
			anotherOriginalMailItem.AddRecipientForUserCommunication("powell@live.com", MailRecipient.RecipientTypes.TO);
			anotherOriginalMailItem.MI_From = "BBB <bbb@123.net>";
			anotherOriginalMailItem.MI_Status = MailStatus.Sent;
			anotherOriginalMailItem.MI_Direction = MailDirection.Transmit;
			anotherOriginalMailItem.MI_SendDateTime = ZDateTime.UtcNow.AddMinutes(-3);
			anotherOriginalMailItem.MI_ReceivedDateTime = originalMailItem.MI_SendDateTime;
			anotherOriginalMailItem.MI_Subject = "Send from Wisegrid B";

			var bounceMailItem = GetBounceBackMailItem(campaignItem, "");

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(bounceMailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, notificationEmail.Recipients.Count);
			AssertEquals("aaa@123.net", notificationEmail.Recipients[0].Email);
			AssertEquals("Email subject", notificationEmail.Subject);
			AssertEquals("Bounce Mail Subject: Undeliverable: Send from Wisegrid A", notificationEmail.Body);
		}
		public void TestSendNotificationEmail_OverrideDefaultDoNotReplyEmailAddressWithCompanyDomain()
		{
			const string defaultDoNotReplyEmailAddress = RawDataRegistry.DefaultDoNotReplyEmailAddress;
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Bounce Mail Subject: (*NonDeliveryReceiptEmailSubject*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.net";
			var bounceMailItem = GetBounceBackMailItem(null, "");
			Factory.Save();
			string fromAddress;
			try
			{
				Env.Registry.SMTPDefaultDoNotReplyEmailAddress = string.Empty;
				Env.Registry.MailboxEmailAddress = "DoNotReply@campanydoamin.com.au";
				fromAddress = ProcessDoNotReplyEmail(bounceMailItem);
				AssertEquals("From Address will become default value if SMTPEmailAddress is not Valid", "PleaseDoNotReply@cargowise.com", fromAddress);
			}
			catch (RegistryValidationException)
			{
				AssertEquals("From Address will become default value if SMTPAddress is not Valid", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			try
			{
				Env.Registry.SMTPDefaultDoNotReplyEmailAddress = defaultDoNotReplyEmailAddress;
				Env.Registry.MailboxEmailAddress = string.Empty;
				fromAddress = ProcessDoNotReplyEmail(bounceMailItem);
				AssertEquals("From Address will become default value if MailboxEmailAddress is not Valid", "PleaseDoNotReply@wisetechglobal.com", fromAddress);
			}
			catch (RegistryValidationException)
			{
				AssertEquals("Will not send email with invalid MailboxEmailAddress", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			Env.Registry.SMTPDefaultDoNotReplyEmailAddress = defaultDoNotReplyEmailAddress;
			Env.Registry.MailboxEmailAddress = "DoNotReply@campanydoamin.com.au";
			fromAddress = ProcessDoNotReplyEmail(bounceMailItem);
			AssertEquals("Will send from EmailAddress_With_CompanyDomain if SMTP value is default", "PleaseDoNotReply@campanydoamin.com.au", fromAddress);

			Env.Registry.SMTPDefaultDoNotReplyEmailAddress = "test@test.com";
			Env.Registry.MailboxEmailAddress = "DoNotReply@campanydoamin.com.au";
			fromAddress = ProcessDoNotReplyEmail(bounceMailItem);
			AssertEquals("Will send from override EmailAddress if SMTP is not default", "test@test.com", fromAddress);
		}

		public void TestSendNotificationEmail_CampaignEmail()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Bounce Mail Subject: (*NonDeliveryReceiptEmailSubject*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "powell@live.com";

			var campaignCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			campaignCoordinator.GS_Code = "AAA";
			campaignCoordinator.GS_EmailAddress = "aaa@123.net";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GS_NKCampaignCoordinator = "AAA";
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var bounceMailItem = GetBounceBackMailItem(campaignItem, "");

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(bounceMailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, notificationEmail.Recipients.Count);
			AssertEquals("aaa@123.net", notificationEmail.Recipients[0].Email);
			AssertEquals("Email subject", notificationEmail.Subject);
			AssertEquals("Bounce Mail Subject: Undeliverable: Send from Wisegrid A", notificationEmail.Body);
			AssertEquals(1, notificationEmail.Attachments.Count);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSendNotificationEmail_JobHeaderOperatorStaff()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Email body");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var anotherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var senderStaff = Factory.NewWithValidTestData<GlbStaff>();
			senderStaff.GS_Code = "AAA";
			senderStaff.GS_EmailAddress = "";

			var jobOp1 = Factory.NewWithValidTestData<GlbStaff>();
			jobOp1.GS_Code = "BBB";
			jobOp1.GS_EmailAddress = "bbb@123.com";

			var jobOp2 = Factory.NewWithValidTestData<GlbStaff>();
			jobOp2.GS_Code = "CCC";
			jobOp2.GS_EmailAddress = "ccc@123.com";

			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var jobLoader = new JobHeader.Loader((IJobHeaderParent)shipment);
			var job1 = jobLoader.TryCreate(GlbBranch.CurrentBranch);
			job1.JH_GS_NKRepOps = "BBB";
			var job2 = jobLoader.TryCreate(anotherBranch);
			job2.JH_GS_NKRepOps = "CCC";

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", senderStaff.PK);

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(mailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			var receipientEmails = notificationEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email).OrderBy(x => x).ToArray();
			AssertEquals(2, notificationEmail.Recipients.Count);
			AssertEquals("bbb@123.com", receipientEmails[0]);
			AssertEquals("ccc@123.com", receipientEmails[1]);
		}

		public void TestSendNotificationEmail_JobCreateStaff()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Email body");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var senderStaff = Factory.NewWithValidTestData<GlbStaff>();
			senderStaff.GS_Code = "AAA";
			senderStaff.GS_EmailAddress = "";

			var jobCreateStaff = Factory.NewWithValidTestData<GlbStaff>();
			jobCreateStaff.GS_Code = "BBB";
			jobCreateStaff.GS_EmailAddress = "bbb@123.com";

			BusinessObject shipment = null;
			using (Env.SetTemporaryUserContext(jobCreateStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				shipment = (BusinessObject)Factory.New<ICommonShipment>();
				shipment.FillWithValidTestData();
			}

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", senderStaff.PK);

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(mailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals("bbb@123.com", notificationEmail.Recipients[0].Email);
		}

		public void TestSendNotificationEmail_MostRecentJobEditStaff()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Email body");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var senderStaff = Factory.NewWithValidTestData<GlbStaff>();
			senderStaff.GS_Code = "AAA";
			senderStaff.GS_EmailAddress = "";

			var jobCreateStaff = Factory.NewWithValidTestData<GlbStaff>();
			jobCreateStaff.GS_Code = "BBB";
			jobCreateStaff.GS_EmailAddress = "";

			var jobEditStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			jobEditStaff1.GS_Code = "CCC";
			jobEditStaff1.GS_EmailAddress = "ccc@123.com";

			var jobEditStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			jobEditStaff2.GS_Code = "DDD";
			jobEditStaff2.GS_EmailAddress = "ddd@123.com";

			var jobEditStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			jobEditStaff3.GS_Code = "EEE";
			jobEditStaff3.GS_EmailAddress = "eee@123.com";
			jobEditStaff3.GS_IsSystemAccount = true;

			BusinessObject shipment = null;
			using (Env.SetTemporaryUserContext(jobCreateStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				shipment = (BusinessObject)Factory.New<ICommonShipment>();
				shipment.FillWithValidTestData();
			}

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", senderStaff.PK);

			Factory.Save();

			using (Env.SetTemporaryUserContext(jobEditStaff1.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Thread.Sleep(1);
				shipment[JobShipmentSchema.JS_RL_NKOrigin] = "AUSYD";
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(jobEditStaff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Thread.Sleep(1);
				shipment[JobShipmentSchema.JS_RL_NKOrigin] = "NZAKL";
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(jobEditStaff3.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Thread.Sleep(1);
				shipment[JobShipmentSchema.JS_RL_NKOrigin] = "HKHKG";
				Factory.Save();
			}

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(mailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, notificationEmail.Recipients.Count);
			AssertEquals("ddd@123.com", notificationEmail.Recipients[0].Email);
		}

		public void TestSendNotificationEmail_NotificationGroup()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Email body");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var senderStaff = Factory.NewWithValidTestData<GlbStaff>();
			senderStaff.GS_Code = "AAA";
			senderStaff.GS_EmailAddress = "";

			var jobCreateStaff = Factory.NewWithValidTestData<GlbStaff>();
			jobCreateStaff.GS_Code = "BBB";
			jobCreateStaff.GS_EmailAddress = "";

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var groupStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff1.GS_Code = "CCC";
			groupStaff1.GS_EmailAddress = "ccc@123.com";

			var groupStaff2 = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff2.GS_Code = "DDD";
			groupStaff2.GS_EmailAddress = "ddd@123.com";

			var groupStaff3 = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff3.GS_Code = "EEE";
			groupStaff3.GS_EmailAddress = "eee@123.com";

			var groupStaff4 = Factory.NewWithValidTestData<GlbStaff>();
			groupStaff4.GS_Code = "FFF";
			groupStaff4.GS_EmailAddress = "fff@123.com";

			group.Staff.Add(groupStaff1);
			group.Staff.Add(groupStaff2);
			group.Staff.Add(groupStaff3);
			group.Staff.Add(groupStaff4);

			GlbEmailAddress.LoadOrNew(Factory, groupStaff3.GS_EmailAddress).GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			GlbEmailAddress.LoadOrNew(Factory, groupStaff4.GS_EmailAddress).GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;

			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			BusinessObject shipment = null;
			using (Env.SetTemporaryUserContext(jobCreateStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				shipment = (BusinessObject)Factory.New<ICommonShipment>();
				shipment.FillWithValidTestData();
			}

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", senderStaff.PK);

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(mailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			var receipientEmails = notificationEmail.Recipients.Cast<RecipientDef>().Select(x => x.Email).OrderBy(x => x).ToArray();
			AssertEquals(3, notificationEmail.Recipients.Count);
			AssertEquals("ccc@123.com", receipientEmails[0]);
			AssertEquals("ddd@123.com", receipientEmails[1]);
			AssertEquals("fff@123.com", receipientEmails[2]);
		}

		public void TestSendNotificationEmail_AttachmentsDisplayNameMaxLength()
		{
			var stringWith252Characters = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012";
			var stringWith256Characters = stringWith252Characters + "3456";

			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.com";

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", staff.PK);
			mailItem.MI_Subject = stringWith252Characters;
			Factory.Save();

			Processor.ProcessMailItem(mailItem, TestLogger);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(256, Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].DisplayName.Length);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			mailItem.MI_Subject = stringWith256Characters;
			Factory.Save();

			Processor.ProcessMailItem(mailItem, TestLogger);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(256, Env.OutgoingMailManager.EmailsCreated[0].Attachments[0].DisplayName.Length);
		}

		public void TestSendNotificationEmail_UnsuccessfulProcess()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Email body: (*DocumentName*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var shipment = (BusinessObject)Factory.New<ICommonShipment>();
			shipment.FillWithValidTestData();
			var job = new JobHeader.Loader((IJobHeaderParent)shipment).TryCreate();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.com";

			var mailItem = GetBounceBackMailItem(shipment, "Delivery Summary", staff.PK);
			mailItem.MI_Body = "This is a spam email not an actual bounce back.";

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			bool result = Processor.ProcessMailItem(mailItem, TestLogger);
			AssertEquals("Unsuccessful process", false, result);
			AssertEquals("Should be no notification email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendNotificationEmail_DoNotSendToNDRAddress()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Email subject", "Bounce Mail Subject: (*NonDeliveryReceiptEmailSubject*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "powell@live.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_EmailAddress = "aaa@123.net";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";
			staff2.GS_EmailAddress = "bbb@123.net";

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "CCC";
			staff3.GS_EmailAddress = "ccc@123.net";

			GlbEmailAddress.LoadOrNew(Factory, staff2.GS_EmailAddress).GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;

			var originalMailItem = Factory.New<MailItem>();
			originalMailItem.AddRecipientForUserCommunication("powell@live.com", MailRecipient.RecipientTypes.TO);
			originalMailItem.MI_From = "Test User <test.user@123.net>";
			originalMailItem.MI_Status = MailStatus.Sent;
			originalMailItem.MI_Direction = MailDirection.Transmit;
			originalMailItem.MI_SendDateTime = ZDateTime.UtcNow.AddMinutes(-5);
			originalMailItem.MI_ReceivedDateTime = originalMailItem.MI_SendDateTime;
			originalMailItem.MI_Subject = "Send from Wisegrid A";
			originalMailItem.MI_ReplyTo = "aaa@123.net;bbb@123.net;ccc@123.net";

			var bounceMailItem = GetBounceBackMailItem(campaignItem, "");

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var result = Processor.ProcessMailItem(bounceMailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals(2, notificationEmail.Recipients.Count);
			AssertEquals("aaa@123.net", notificationEmail.Recipients[0].Email);
			AssertEquals("ccc@123.net", notificationEmail.Recipients[1].Email);
			AssertEquals("Email subject", notificationEmail.Subject);
		}

		public void TestSendNotificationEmail_ShowErrorOnNDRCaseForEmailDestinationOverride()
		{
			var template = new NotificationEmailTemplate(ObjectFactory.GetType<DocumentWrappers.IDocBounceBackEmailProcessResult>(), "Undelivered Mail Returned to Sender", "Bounce Mail Subject: (*NonDeliveryReceiptEmailSubject*)");
			SystemDataRegistry.Instance.NonDeliveryReceiptNotificationEmailTemplate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, template);

			var glbEmail = GlbEmailAddress.LoadOrNew(Factory, "fakeemail@place.com");
			if (glbEmail != null)
			{
				glbEmail.Delete();
			}
			Env.Registry.EmailDestinationOverride = "fakeemail@place.com";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Regional Manager";
			staff.GS_Code = "RM1";
			staff.GS_EmailAddress = "powell@live.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "someemail@thing.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;

			Factory.Save();

			var originalMailItem = Factory.New<MailItem>();
			originalMailItem.MI_Direction = MailDirection.Transmit;
			originalMailItem.MI_SendDateTime = ZDateTime.UtcNow.AddMinutes(-5);
			originalMailItem.MI_ReceivedDateTime = originalMailItem.MI_SendDateTime;
			originalMailItem.SendAttempts = 1;
			originalMailItem.MI_ContentType = EmailContentTypes.HTML.ContentTypeCode;
			originalMailItem.MI_Body = "Body";
			originalMailItem.MI_From = "Regional Manager <powell@live.com>";
			originalMailItem.MI_SenderStaffID = staff.PK.ToString();
			originalMailItem.MI_BusinessEntityID = campaignItem.PK.ToString();
			originalMailItem.MI_BusinessEntityTableCode = GlbCompanyCampaignItemSchema.Constants.Prefix;
			originalMailItem.MI_DocumentName = "Doc Name";
			originalMailItem.MI_Subject = "Subject";
			originalMailItem.MI_Status = MailStatus.Queued;
			originalMailItem.AddRecipientForUserCommunication("someemail@thing.com", MailRecipient.RecipientTypes.TO);
			originalMailItem.AddFooterForOverriddenRecipients();

			Factory.Save();

			var sender = new MailManager.ExternalMailInterface.MailSender();

			sender.CheckRecipients(originalMailItem);

			originalMailItem.MailRecipients[0].MR_AckAttempt++;
			originalMailItem.MailRecipients[0].MR_LastAttempt = ZDateTime.UtcNow;
			originalMailItem.MI_Status = MailStatus.Sent;
			originalMailItem.MailRecipients[0].MR_DeliveredTime = ZDateTime.UtcNow;

			var bounceBackItem = Factory.New<MailItem>();
			bounceBackItem.MI_Direction = MailDirection.Receive;
			bounceBackItem.MI_Application = MailApplication.Standard;
			bounceBackItem.MI_Status = MailStatus.Queued;
			bounceBackItem.MI_Application = MailApplication.Standard;
			bounceBackItem.MI_Status = MailStatus.Unprocessed;
			bounceBackItem.MI_SendDateTime = ZDateTime.UtcNow;
			bounceBackItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			bounceBackItem.MI_From = "MAILER-DAEMON@sydwt-srvp-11.test.wisecloud.zone (Mail Delivery System)";
			bounceBackItem.MI_Subject = "Undelivered Mail Returned to Sender";
			string mailboxRecipient = Env.Registry.MailboxEmailAddress;
			bounceBackItem.AddRecipientForUserCommunication(mailboxRecipient, MailRecipient.RecipientTypes.TO);
			bounceBackItem.MailRecipients[0].MR_DeliveredTime = ZDateTime.Empty;
			bounceBackItem.MailRecipients[0].MR_LastAttempt = ZDateTime.Empty;
			bounceBackItem.MailRecipients[0].MR_RecipientType = nameof(MailRecipient.RecipientTypes.TO);
			bounceBackItem.MI_POP3UIDL = "00000dc358337114";

			#region Header and Body text

			string header = @"Return-Path: <>
X-Original-To: {0}
Delivered-To: {0}
Received: from sydwt-srvp-11.test.wisecloud.zone (unknown [10.61.179.21])
	by test.wisecloud.zone (Postfix) with ESMTPS id 5B4275F81736
	for <{0}>; {1}
Received: by sydwt-srvp-11.test.wisecloud.zone (Postfix)
	id 5095368A; {2}
Date: {1}
From: {3}
Subject: {4}
To: {5}
Auto-Submitted: auto-replied
MIME-Version: 1.0
Content-Type: multipart/report; report-type=delivery-status;
	boundary=""B22CB689.1579213041/sydwt-srvp-11.test.wisecloud.zone""
Content-Transfer-Encoding: 8bit
Message-Id: <20200116221721.5095368A@sydwt-srvp-11.test.wisecloud.zone>
X-UIDL: {6}
";

			string body = @"""Content-Type: text/plain; charset=utf-8

Delivery has failed to these recipients or groups: {0}<mailto:{0}>

<{0}>: host
	place-com.mail.protection.outlook.com[104.47.1.36] said: 550 5.4.1 All
	recipient addresses rejected : Access denied. AS(201806271)
	[VE1EUR01FT061.eop-EUR01.prod.protection.outlook.com] (in reply to end of
	DATA command)

--B22CB689.1579213041/sydwt-srvp-11.test.wisecloud.zone
Content-Description: Delivery report
Content-Type: message/delivery-status
Content-Transfer-Encoding: 8bit

Reporting-MTA: dns; sydwt-srvp-11.test.wisecloud.zone
X-Postfix-Queue-ID: B22CB689
X-Postfix-Sender: rfc822; {1}
Arrival-Date: Fri, 17 Jan 2020 09:17:16 +1100 (AEDT)

Final-Recipient: rfc822; {0}
Original-Recipient: rfc822;{0}
Action: failed
Status: 5.4.1
Remote-MTA: dns; place-com.mail.protection.outlook.com
Diagnostic-Code: smtp; 550 5.4.1 All recipient addresses rejected : Access
	denied. AS(201806271) [VE1EUR01FT061.eop-EUR01.prod.protection.outlook.com]

--B22CB689.1579213041/sydwt-srvp-11.test.wisecloud.zone
Content-Description: Undelivered Message
Content-Type: message/rfc822
Content-Transfer-Encoding: 8bit

Return-Path: <{1}>
Received: from test.wisecloud.zone (unknown [10.61.147.21])
	by sydwt-srvp-11.test.wisecloud.zone (Postfix) with ESMTPS id B22CB689
	for <{0}>; Fri, 17 Jan 2020 09:17:16 +1100 (AEDT)
Received: from [10.61.217.8] (unknown [10.61.217.8])
	by test.wisecloud.zone (Postfix) with ESMTPA id 93BD35F6668F
	for <{0}>; Fri, 17 Jan 2020 09:17:16 +1100 (AEDT)
Content-Type: multipart/mixed;
 boundary=""----=_NextPart_35626937.744329896974""
MIME-Version: 1.0
Date: {2}
Message-ID: <f85d241c-8f28-4ac2-a955-4d99a57e822b@mail.dll>
Subject: {3}
From: {4}
To: {0}
X-SenderStaffID: {5}
X-BusinessEntityID: {6}
X-BusinessEntityTableCode: {7}
X-BusinessEntityJobNumber: {8}
X-DocumentName: {9}

------=_NextPart_36848782.744329896974
Content-Type: text/plain;
 charset=""utf-8""
Content-Transfer-Encoding: quoted-printable

Please see the attached documents.

(This message was redirected to {10} as it was sent from a non-production system. Originally the email was addressed to {11}.)
------=_NextPart_36848782.744329896974
Content-Type: text/html;
 charset=""utf-8""
Content-Transfer-Encoding: quoted-printable

{12}
";

			#endregion

			bounceBackItem.MI_Header = string.Format(header,
				mailboxRecipient,
				bounceBackItem.MI_SendDateTime,
				bounceBackItem.MI_ReceivedDateTime,
				bounceBackItem.MI_From,
				bounceBackItem.MI_Subject,
				string.Join(";", bounceBackItem.MailRecipients.Cast<MailRecipient>().Select(recipients => recipients.EmailAddress)),
				bounceBackItem.MI_POP3UIDL);

			bounceBackItem.MI_Body = string.Format(body,
				string.Join(";", originalMailItem.MailRecipients.Cast<MailRecipient>().Select(recipients => recipients.EmailAddress)),      //0
				mailboxRecipient,                                                                                                           //1
				originalMailItem.MI_SystemCreateTimeUtc.ToLocalBranchTime(),                                                                //2
				originalMailItem.MI_Subject,                                                                                                //3
				originalMailItem.MI_From,                                                                                                   //4
				originalMailItem.MI_SenderStaffID,                                                                                          //5
				originalMailItem.MI_BusinessEntityID,                                                                                       //6
				originalMailItem.MI_BusinessEntityTableCode,                                                                                //7
				originalMailItem.MI_BusinessEntityjobNumber,                                                                                //8
				originalMailItem.EncodedDocumentName,                                                                                       //9
				Env.Registry.EmailDestinationOverride,                                                                                      //10
				contact.OC_Email,                                                                                                           //11
				originalMailItem.MI_Body);                                                                                                  //12

			var processor = new BounceBackEmailProcessor();

			processor.ProcessMailItem(bounceBackItem, new LoggerForTest());

			var notificationMail = Factory.Load<MailItem>(((OutgoingMailCreator)Env.OutgoingMailManager).LastCreatedMailGuid);

			AssertExceptionThrown<MailManager.ExternalMailInterface.FailedToSendMessageException>("",
				"Failed to send email with subject " + notificationMail.MI_Subject + ". The email destination override has received NDR, please check the address in Registry->System->Testing->Email Destination Override",
				() => sender.CheckRecipients(notificationMail));
		}

		#endregion

		LoggerForTest TestLogger
		{
			get { return testLogger ?? (testLogger = new LoggerForTest()); }
		}
		LoggerForTest testLogger;

		#region Implementation

		MailItem GetBounceBackMailItem(BusinessObject businessEntity, string documentName, bool asAttachment = false)
		{
			return GetBounceBackMailItem(businessEntity, documentName, ZGuid.Empty, asAttachment);
		}

		MailItem GetBounceBackMailItem(BusinessObject businessEntity, string documentName, ZGuid senderStaffID, bool asAttachment = false, string mailBody = defaultMailBody)
		{
			var result = Factory.New<MailItem>();
			result.MI_Status = MailStatus.Queued;
			result.MI_Direction = MailDirection.Receive;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.AddRecipientForUserCommunication("Deliverance Update <address@edi.com.au>", MailRecipient.RecipientTypes.TO);
			result.MI_Subject = "Undeliverable: Send from Wisegrid A";
			result.MI_From = "from-address@example.com";

			var body = string.Format(mailBody,
				businessEntity != null ? businessEntity.PK.ToString() : "",
				businessEntity != null ? businessEntity.TablePrefix : "",
				documentName,
				senderStaffID.ToString(),
				result.MI_SendDateTime.ToStandardDateTimeString());

			if (asAttachment)
			{
				result.MI_Body = "Apple";
				var attachment = result.MailAttachments.AddNew();
				attachment.MA_FileName = "Banana";
				attachment.MA_Data = ZBlob.FromAscii(body);
			}
			else
			{
				result.MI_Body = body;
			}

			return result;
		}

		string ProcessDoNotReplyEmail(MailItem bounceMailItem)
		{
			var result = Processor.ProcessMailItem(bounceMailItem, TestLogger);

			AssertEquals("Successful process", true, result);
			var notificationEmail = Env.OutgoingMailManager.EmailsCreated.Last();
			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("aaa@123.net", notificationEmail.Recipients[0].Email);
			AssertEquals("Email subject", notificationEmail.Subject);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			return notificationEmail.FromAddress;
		}

		const string defaultMailBody = @"Content-Type: text/plain; charset=utf-8

Delivery has failed to these recipients or groups:
powell@live.com<mailto:powell@live.com>
Received: from mail2.hillebrandgroup.com (172.27.10.32) by
 JFHNLAHUB1.jfhillebrand.com (172.29.10.175) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 08:58:23 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail2.hillebrandgroup.com with ESMTP Gateway	id <B53bb968e0000>; Tue, 08 Jul
 2014 08:58:22 +0200
Received: from mail3.hillebrandgroup.com (172.27.10.33) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 08:43:19 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail3.hillebrandgroup.com with ESMTP Gateway	id <B53bb930c0000>; Tue, 08 Jul
 2014 08:43:24 +0200
Received: from mail2.hillebrandgroup.com (172.27.10.32) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 08:28:18 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail2.hillebrandgroup.com with ESMTP Gateway	id <B53bb8f800000>; Tue, 08 Jul
 2014 08:28:16 +0200
Received: from mail3.hillebrandgroup.com (172.27.10.33) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 08:13:14 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail3.hillebrandgroup.com with ESMTP Gateway	id <B53bb8bfe0000>; Tue, 08 Jul
 2014 08:13:18 +0200
Received: from mail1.hillebrandgroup.com (172.27.10.31) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:58:11 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail1.hillebrandgroup.com with ESMTP Gateway	id <B53bb88720000>; Tue, 08 Jul
 2014 07:58:10 +0200
Received: from mail1.hillebrandgroup.com (172.27.10.31) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:40 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail1.hillebrandgroup.com with ESMTP Gateway	id <B53bb88540000>; Tue, 08 Jul
 2014 07:57:40 +0200
Received: from mail2.hillebrandgroup.com (172.27.10.32) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:09 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail2.hillebrandgroup.com with ESMTP Gateway	id <B53bb88350000>; Tue, 08 Jul
 2014 07:57:09 +0200
Received: from mail1.hillebrandgroup.com (172.27.10.31) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:09 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail1.hillebrandgroup.com with ESMTP Gateway	id <B53bb88350000>; Tue, 08 Jul
 2014 07:57:09 +0200
Received: from mail2.hillebrandgroup.com (172.27.10.32) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:08 +0200
Received: from JFHNLAHUB1.jfhillebrand.com (Not Verified[172.29.10.175]) by
 mail2.hillebrandgroup.com with ESMTP Gateway	id <B53bb88340000>; Tue, 08 Jul
 2014 07:57:08 +0200
Received: from mail1.hillebrandgroup.com (172.27.10.31) by
 JFHNLAHUB1.jfhillebrand.com (172.29.10.175) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:08 +0200
Received: from JFHNLAHUB1.jfhillebrand.com (Not Verified[172.29.10.175]) by
 mail1.hillebrandgroup.com with ESMTP Gateway	id <B53bb88340000>; Tue, 08 Jul
 2014 07:57:08 +0200
Received: from mail3.hillebrandgroup.com (172.27.10.33) by
 JFHNLAHUB1.jfhillebrand.com (172.29.10.175) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:07 +0200
Received: from JFHNLAHUB2.jfhillebrand.com (Not Verified[172.29.10.176]) by
 mail3.hillebrandgroup.com with ESMTP Gateway	id <B53bb88380000>; Tue, 08 Jul
 2014 07:57:12 +0200
Received: from mail1.hillebrandgroup.com (172.27.10.31) by
 JFHNLAHUB2.jfhillebrand.com (172.29.10.176) with Microsoft SMTP Server id
 14.2.347.0; Tue, 8 Jul 2014 07:57:07 +0200
Received: from wisegrid.net (enmail.wisegrid.net[203.62.211.66]) by
 mail1.hillebrandgroup.com with ESMTP Gateway	id <B53bb88320000>; Tue, 08 Jul
 2014 07:57:06 +0200
Received: from [10.61.162.202] ([10.61.162.202]) by wisegrid.net with
 MailEnable ESMTP; Tue, 8 Jul 2014 15:57:01 +1000
X-BusinessEntityID: {0}
X-BusinessEntityTableCode: {1}
X-DocumentName: {2}
X-SenderStaffID: {3}
X-OriginSentTime: {4}
Content-Type: multipart/alternative;
	boundary=""----=_NextPart_11530135.822438840557""
MIME-Version: 1.0
Subject: Send from Wisegrid A
Message-ID: <a2d6fd1d-3a0c-447e-b55c-78ea9f0b7a73@mail.dll>
Date: Tue, 8 Jul 2014 15:56:53 +1000
From: WiseTimes Campaign <francisco.lorenzo@wisetechglobal.com>
To: <powell@live.com>
Return-Path: <Core.test.A@wisegrid.net>
";

		const string testNDRMailBody = @"Content-Type: text/plain; charset=utf-8

Delivery has failed to these recipients or groups: {0}<mailto:{0}>

Original message headers:

Received: from [10.61.164.82] ([10.61.164.82]) by wisetechglobal.com with MailEnable ESMTP; Thu, 10 Nov 2022 02:23:26 +0000
X-BusinessEntityID: d7627cf3-4256-432f-9cbc-a59653da3f47
X-BusinessEntityTableCode: G8
X-DocumentName: 3D20
X-SenderStaffID: {1}
Content-Type: text/html; charset=3D3Dutf-8
MIME-Version: 1.0
Subject: My Bounce Subject
Message-ID: <a7f72ee2-c315-447d-ad57-723d47c6c046@mail.dll>
Date: Thu, 10 Nov 2022 02:23:26 +0000
From: ""TEST NDR"" <from.valid@wisetechglobal.com>
To: <{0}>
Cc: <cc.valid@wisetechglobal.com>,
 <cc.ndr@wisetechglobal.com>
Bcc:
Return-Path: <Support@enterprisedevelopment.cargowise.com>
";
		#endregion

		BounceBackEmailProcessor Processor;

		protected override void SetUp()
		{
			base.SetUp();
			Processor = new BounceBackEmailProcessor();
		}
	}
}
