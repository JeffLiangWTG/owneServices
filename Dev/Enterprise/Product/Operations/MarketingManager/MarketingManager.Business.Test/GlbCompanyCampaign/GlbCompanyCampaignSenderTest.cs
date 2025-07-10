using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MarketingManager.Business.GlbCompanyCampaignSender;

namespace Enterprise.MarketingManager.Business.Testing
{
	public sealed class GlbCompanyCampaignSenderTest : TestCaseWithFactory
	{
		#region Send Campaigns

		public void TestResend_ConcurrencyException()
		{
			var campaign = Helper.GetCampaignForTestWithoutErrors();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = CreateContact("Bob", org);
			var campaignItemSent = campaign.CampaignsItemsSent.AddNew();
			campaignItemSent.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItemSent.G8_RecipientID = contact.PK;
			Factory.Save();

			var campaignDependentCollection = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
			campaignDependentCollection.Add(campaignItemSent);
			GlbCompanyCampaignItem[] campaignItems = campaignDependentCollection.Cast<GlbCompanyCampaignItem>().ToArray();

			var nonRefreshingFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var campaignItemOnNonRefreshingFactory = nonRefreshingFactory.Load<GlbCompanyCampaignItem>(campaignItemSent.PK);
			campaignItemOnNonRefreshingFactory.G8_GS_NKFollowedUpBy = "XXX";
			nonRefreshingFactory.Save();

			var campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignDependentCollection.Cast<GlbCompanyCampaignItem>());
			campaignSender.ShouldContinueWithSending += (numCampaignsToSend, campaignItem) => true;
			AssertNoExceptionThrown(() =>
			{
				campaignSender.CheckAndSendCampaigns();
			});

			AssertEquals("Should delay sending email if there is a concurrency exception", 0,
				new BusinessObjectFactory().Load<MailItem>(new ZQuery()).Length);
			Factory.Save();
			AssertEquals("Should sent email after save", 1, new BusinessObjectFactory().Load<MailItem>(new ZQuery()).Length);
		}

		[TestDate(2015, 9, 2, 12, 0, 0)]
		public void TestResend()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();

			campaign.G0_BatchCountDefault = 0; //Campaign has 1 error

			var previousSender = Factory.NewWithValidTestData<GlbStaff>();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			OrgContact contact2 = CreateContact("Tom", org);

			GlbCompanyCampaignItem campaignSentToContact1 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact1.G8_RecipientID = contact1.PK;
			campaignSentToContact1.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.PrintCode;

			campaignSentToContact1.G8_SystemCreateUser = previousSender.GS_Code;

			GlbCompanyCampaignItem campaignSentToContact2 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact2.G8_RecipientID = contact2.PK;
			campaignSentToContact2.G8_SystemCreateUser = previousSender.GS_Code;
			campaignSentToContact2.G8_DeliveryMethod = GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode;

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>(), campaign);

			var campaignDependentCollection = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
			campaignDependentCollection.Add(campaignSentToContact1);
			GlbCompanyCampaignItem[] campaignItems = campaignDependentCollection.Cast<GlbCompanyCampaignItem>().ToArray();

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			form.UserChoiceContinueWithSending = true;

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignDependentCollection.Cast<GlbCompanyCampaignItem>());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);

			campaignSender.CheckAndSendCampaigns();
			var batchCountDefaultErrorMessage = $@"{GlbCompanyCampaignSender.NotificationConstants.CorrectAllErrorsMessage}
- The default number of Campaigns sent in each batch must be larger than 0";

			AssertCorrectMessageIsDisplayed(form, batchCountDefaultErrorMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaign should still be sent by previous user", previousSender.PK,
				campaignItems[0].SystemCreateUser.PK);
			AssertEquals("Should not load contacts when resending campaign", 0, campaignContactCollection.Count);

			campaign.G0_BatchCountDefault = 2;
			campaign.HtmlDocumentBlob = ZBlob.Empty;
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form, GlbCompanyCampaignSender.NotificationConstants.SaveCampaignMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaign should still be sent by previous user", previousSender.PK,
				campaignItems[0].SystemCreateUser.PK);

			Factory.Save();
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
				$@"{NotificationConstants.CorrectAllErrorsMessage}
- {NotificationConstants.EmailContentCannotBeEmpty}",
				NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaign should still be sent by previous user", previousSender.PK,
				campaignItems[0].SystemCreateUser.PK);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			Factory.Save();
			form.UserChoiceContinueWithSending = false;
			campaignSender.CheckAndSendCampaigns();
			AssertNull("Sending cancelled, No error message should be displayed", form.MessageOnCampaignSendingArgs);
			AssertEquals("Campaign should still be sent by previous user", previousSender.PK,
				campaignItems[0].SystemCreateUser.PK);

			form.UserChoiceContinueWithSending = true;
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
				ZString.Format(GlbCompanyCampaignSender.NotificationConstants.CampaignsSentMessage, 1),
				GlbCompanyCampaignSender.NotificationConstants.CampaignsSentSummary, false);
			AssertEquals("Creating user should remain the same", previousSender.PK, campaignItems[0].SystemCreateUser.PK);
			AssertEquals("Last sent user should now become the current user", GlbStaff.CurrentUser.PK,
				campaignItems[0].SystemLastEditUser.PK);
			AssertEquals("Delivery method should be updated", "EML", campaignItems[0].G8_DeliveryMethod);
			AssertEquals("Campaign Item Last Sent time should capture current sent time", campaignItems[0].G8_LastSentTimeUtc,
				new ZDateTime(2015, 9, 2, 12, 0, 0));
			AssertEquals("Campaign Item Last System Edit time should capture current sent time",
				campaignItems[0].G8_SystemLastEditTimeUtc, new ZDateTime(2015, 9, 2, 12, 0, 0));

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah" + StartTag + "132123123213");
			Factory.Save();

			campaignDependentCollection = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
			campaignDependentCollection.Add(campaignSentToContact2);
			campaignItems = campaignDependentCollection.Cast<GlbCompanyCampaignItem>().ToArray();

			campaignSender.CheckAndSendCampaigns();

			var expectedMessage = $@"{NotificationConstants.CorrectAllErrorsMessage}
- {GlbCompanyCampaignValidation.EmailContentMacrosMalformedErrorText}";

			AssertCorrectMessageIsDisplayed(form, expectedMessage, GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);

			AssertEquals("Campaign should still be sent by previous user", previousSender.PK,
				campaignItems[0].SystemCreateUser.PK);
		}

		public void TestRequeue()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			OrgContact contact2 = org.Contacts.AddNew();

			contact1.OC_ContactName = "A";
			contact1.OC_Email = "a@bing.com";
			contact2.OC_ContactName = "B";
			contact2.OC_Email = "b@bing.com";
			contact1.OC_Phone =
				GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			contact2.OC_Phone =
				GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;

			GlbCompanyCampaignItem campaignSentToContact1 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact1.G8_RecipientID = contact1.PK;

			GlbCompanyCampaignItem campaignSentToContact2 = campaign.CampaignsItemsSent.AddNew();
			campaignSentToContact2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignSentToContact2.G8_RecipientID = contact2.PK;

			Factory.Save();

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			form.UserChoiceContinueWithSending = true;

			var campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);

			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertCollectionNotContains("Campaign sent to Contact1, Collection should NOT contain contact Contact1",
				new ZQuery(OrgContactSchema.PK, contact1.PK), campaign.FilteredContacts);
			AssertCollectionNotContains("Campaign sent to Contact2, Collection should NOT contain contact Contact2",
				new ZQuery(OrgContactSchema.PK, contact2.PK), campaign.FilteredContacts);

			campaign.G0_ActualStartedDate = ZDateTime.Invalid;
			Factory.Save();

			campaignSender.Requeue(new[] { campaign.CampaignsItemsSent[0] });
			AssertCorrectMessageIsDisplayed(form,
$@"{GlbCompanyCampaignSender.NotificationConstants.CorrectAllErrorsOnRequeuingMessage}
- Enter a valid Actual Start Date.",
			GlbCompanyCampaignSender.NotificationConstants.CannotRequeueSummary, true);

			campaign.G0_ActualStartedDate = new ZDateTime(2015, 12, 12);
			Factory.Save();

			campaignSender.Requeue(new[] { campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().FirstOrDefault(s => s.G8_RecipientID == contact1.PK) });
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			Assert("Campaign sent to Contact 1 should be deleted",
				campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().FirstOrDefault(s => s.G8_RecipientID == contact1.PK) ==
				null);
			AssertCollectionContains("Contact 1 should be re-queued", contact1.PK, campaignContactCollection.GetPKs());
		}

		public void TestRequeue_MultipleItems()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			var contact2 = org.Contacts.AddNew();
			var contact3 = org.Contacts.AddNew();
			contact1.OC_ContactName = "A";
			contact2.OC_ContactName = "B";
			contact3.OC_ContactName = "C";
			contact1.OC_Email = "a@bing.com";
			contact2.OC_Email = "b@bing.com";
			contact3.OC_Email = "c@bing.com";

			var campaign = Helper.GetCampaignForTestWithoutErrors();
			var item1 = campaign.CampaignsItemsSent.AddNew();
			var item2 = campaign.CampaignsItemsSent.AddNew();
			var item3 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			item2.G8_RecipientID = contact2.PK;
			item3.G8_RecipientID = contact3.PK;
			Factory.Save();

			var sendEventCalled = false;
			var campaignSender = new GlbCompanyCampaignSender(campaign, new GlbCompanyCampaignItem[] { item1, item2, item3 });
			campaignSender.ShouldContinueWithSending += delegate
			{
				sendEventCalled = true;
				return true;
			};

			campaignSender.Requeue(new GlbCompanyCampaignItem[] { item2, item3 });
			Assert("ShouldContinueWithSending event should be called", sendEventCalled);
			Assert("Item 1 should not be deleted", !item1.IsDeleted);
			Assert("Item 2 should be deleted", item2.IsDeleted);
			Assert("Item 3 should be deleted", item3.IsDeleted);
		}

		public void TestSendCampaignRefreshFiltersTab()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign2 =
				Helper.GetCampaignForTestWithoutErrors();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("AAA", org);
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact4 = CreateContact("DDD", org2);
			contact4.OC_Email = "aaa@gmail.com";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection =
				ContactCollection(new List<BusinessObject>() { contact1, contact4 }, campaign2);

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign2);
			campaign2.G0_BatchCountDefault = 5;
			Factory.Save();

			form = new GlbCompanyCampaignTest.FakeFormForTest(campaign2);
			campaign2.G0_DeDuplicateContacts = false;
			campaign2.LoadFilteredContacts(campaignContactCollection);
			AssertEquals(2, campaignContactCollection.Count);

			campaign2.AdditionalFilter = new ZQuery(ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.Equal,
				"aaa@gmail.com");
			campaign2.LoadFilteredContacts(campaignContactCollection);
			AssertEquals(1, campaignContactCollection.Count);

			GlbCompanyCampaignSender campaignSender2 = new GlbCompanyCampaignSender(campaign2,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender2.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender2.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);
			campaign2.HtmlDocumentBlob = ZBlob.FromAscii("some content aaa");
			form.UserChoiceContinueWithSending = true;

			Factory.Save();
			campaignSender2.CheckAndSendCampaigns();
			AssertEquals("Campaigns sent", 1, campaign2.CampaignsItemsSent.Count);
			campaign2.LoadFilteredContacts(campaignContactCollection);
			AssertEquals(0, campaignContactCollection.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendEmailIncludesAttachments()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://www.cargowise.com/WebCampaign");
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";
			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_EmailSubject = "Best campaign ever";

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1 },
				campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());

			using (TempFile file1 = TempFile.NewWithExtension("txt"))
			using (TempFile file2 = TempFile.NewWithExtension("txt"))
			{
				File.WriteAllText(file1.Filename, "test 1");
				File.WriteAllText(file2.Filename, "test 2");

				IeDoc newFile1 = campaign.DocManagerInfo.AddFileOrDocument(file1.Filename, "MSC");
				IeDoc newFile2 = campaign.DocManagerInfo.AddFileOrDocument(file2.Filename, "MSC");
				IeDoc newFile3 =
					campaign.DocManagerInfo.AddFileOrDocument(
						Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs", "small.gif"), "MSC");

				newFile1.Description = "temp file 1";
				newFile2.Description = "temp file 2";
				newFile3.Description = "small image";

				AssertEquals(3, campaign.CampaignAttachments.Count);
				campaign.CampaignAttachments[1].Selected = true;
				campaign.CampaignAttachments[2].Selected = true;

				GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
				campaignItem.G8_RecipientID = contact1.PK;

				AssertSend(campaignSender, campaignItem);
				AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

				AssertEquals("Reply to", staff.GS_EmailAddress, email.ReplyTo);
				AssertEquals("From", staff.GS_EmailAddress, email.FromAddress);
				AssertEquals("Subject", "Best campaign ever", email.Subject);
				AssertEquals("From display name", staff.GS_FullName, email.FromDisplayName);
				AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
				AssertEquals("Recipient", campaignContactCollection[0].VCC_Email, email.Recipients[0]);
				AssertEquals("Attachments", 2, email.Attachments.Count);
				AssertEquals("Attachments", Path.GetFileName(file2.Filename), email.Attachments[0].DisplayName);
				AssertEquals("Attachments", "small.gif", email.Attachments[1].DisplayName);
				AssertEquals("BusinessEntityID", campaignItem.PK, email.BusinessEntityID);
				AssertEquals("BusinessEntityTableCode", "G8", email.BusinessEntityTableCode);
				AssertStartsWith("ListUnsubscribe", "<http://www.cargowise.com/WebCampaign/unsubscribe.aspx?data=", email.ListUnsubscribe);
				AssertEndsWith("ListUnsubscribe", ">", email.ListUnsubscribe);
			}
		}

		static void AssertSend(GlbCompanyCampaignSender campaignSender, GlbCompanyCampaignItem campaignItem)
		{
			Assert("Email should send correctly", campaignSender.ShouldBeSentViaEmail(campaignItem));
			campaignSender.SendEmailToContact(campaignItem);
		}

		public void TestSendEmailToContact()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";
			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_CampaignName = "Enlarge your sales!";
			campaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			campaign.UseEmailSenderAddressAsReplyTo = true;

			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact1.PK;

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1 },
				campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());

			AssertSend(campaignSender, campaignItem);
			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Reply to", staff.GS_EmailAddress, email.ReplyTo);
			AssertEquals("From", staff.GS_EmailAddress, email.FromAddress);
			AssertEquals("Subject", "Enlarge your sales! Best campaign ever, Bob!", email.Subject);
			AssertEquals("From display name", staff.GS_FullName, email.FromDisplayName);
			AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Recipient", campaignContactCollection[0].VCC_Email, email.Recipients[0]);
			AssertEquals("QueueWithLowPriority", true, email.QueueWithLowPriority);

			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_SenderEmail = "xwinter@gmail.com";

			campaign.UseEmailSenderAddressAsReplyTo = true;
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			AssertSend(campaignSender, campaignItem);

			email = Env.OutgoingMailManager.EmailsCreated[1];

			AssertEquals("Reply to", "xwinter@gmail.com", email.ReplyTo);

			bool exceptionThrown = false;
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah" + StartTag);
			try
			{
				campaignSender.SendEmailToContact(campaignItem);
			}
			catch (DocumentParsingFailedException)
			{
				exceptionThrown = true;
			}

			Assert("Exception should have been thrown", exceptionThrown);
			AssertEquals("Email should NOT have been sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailToContactWithFreeEmailText()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";
			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_CampaignName = "Enlarge your sales!";
			campaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_EmailSenderName = "Support Smith";
			campaign.G0_SenderEmail = "xrm@cargowise.com";
			campaign.UseEmailSenderAddressAsReplyTo = false;
			campaign.G0_ReplyToEmail = "reply@gmail.com";

			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact1.PK;

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1 },
				campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());

			AssertSend(campaignSender, campaignItem);
			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Reply to", campaign.G0_ReplyToEmail, email.ReplyTo);
			AssertEquals("From", campaign.G0_SenderEmail, email.FromAddress);
			AssertEquals("Subject", "Enlarge your sales! Best campaign ever, Bob!", email.Subject);
			AssertEquals("From display name", campaign.G0_EmailSenderName, email.FromDisplayName);
			AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Recipient", campaignContactCollection[0].VCC_Email, email.Recipients[0]);

			campaign.UseEmailSenderAddressAsReplyTo = true;
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			GlbCompanyCampaignItem campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientID = contact1.PK;
			AssertSend(campaignSender, campaignItem2);

			EmailDef email2 = Env.OutgoingMailManager.EmailsCreated[1];

			AssertEquals("Reply to", email2.ReplyTo, campaign.G0_SenderEmail);
			AssertEquals("From display name", email2.FromDisplayName, campaign.G0_EmailSenderName);
			AssertEquals("From", email2.FromAddress, campaign.G0_SenderEmail);
			bool exceptionThrown = false;
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah" + StartTag);
			try
			{
				campaignSender.SendEmailToContact(campaignItem);
			}
			catch (DocumentParsingFailedException)
			{
				exceptionThrown = true;
			}

			Assert("Exception should have been thrown", exceptionThrown);
			AssertEquals("Email should NOT have been sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailToContactWithFreeEmailText_DisplayName()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";
			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_CampaignName = "Enlarge your sales!";
			campaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_EmailSenderName = "Smith, Support";
			campaign.G0_SenderEmail = "xrm@cargowise.com";
			campaign.UseEmailSenderAddressAsReplyTo = false;
			campaign.G0_ReplyToEmail = "reply@gmail.com";

			GlbCompanyCampaignItem campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientID = contact1.PK;

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1 },
				campaign);
			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());

			AssertSend(campaignSender, campaignItem);
			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Reply to", campaign.G0_ReplyToEmail, email.ReplyTo);
			AssertEquals("From", campaign.G0_SenderEmail, email.FromAddress);
			AssertEquals("Subject", "Enlarge your sales! Best campaign ever, Bob!", email.Subject);
			AssertEquals("From display name", "\"" + campaign.G0_EmailSenderName + "\"", email.FromDisplayName);
			AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Recipient", contact1.OC_Email, email.Recipients[0]);
		}

		public void TestSentEmailIsAttachedToEDocs()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var campaignItem = CreateCampaignTestData(permanentlySaveAgainstRecipient: true);
			var sender = new GlbCompanyCampaignSender(campaignItem.CompanyCampaign);

			AssertEquals("Pre-condtion", 0, campaignItem.DocManagerInfo.AllEDocs.Count);
			AssertSend(sender, campaignItem);
			AssertEquals("Email should have been attached to eDocs", 1, campaignItem.DocManagerInfo.AllEDocs.Count);

			var email = campaignItem.DocManagerInfo.AllEDocs[0];
			AssertEquals("TESTORG Bob.eml", email.FileName);
		}

		public void TestSavedEmailDocTypes()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var campaignItem = CreateCampaignTestData(permanentlySaveAgainstRecipient: true);

			var newDocType1 = Factory.NewWithValidTestData<RefDocType>();
			newDocType1.RT_DocType = "DT1";
			newDocType1.RT_Desc = "Test DocType 1";
			newDocType1.RT_ReferenceType = "ALL";

			var newDocType2 = Factory.NewWithValidTestData<RefDocType>();
			newDocType2.RT_DocType = "DT2";
			newDocType2.RT_Desc = "Test DocType 2";
			newDocType2.RT_ReferenceType = "ALL";

			Factory.Save();

			var sender = new GlbCompanyCampaignSender(campaignItem.CompanyCampaign);

			// Test Default MSC
			AssertEquals("Pre-condition", 0, campaignItem.DocManagerInfo.AllEDocs.Count);
			AssertSend(sender, campaignItem);

			var email = campaignItem.DocManagerInfo.AllEDocs[0];
			AssertEquals("MSC", email.DocType);
			AssertEquals("Miscellaneous Document", email.Description);

			//Test DT1
			OrganisationsDataRegistry.Instance.PermanentlySavedCampaignEmailDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDocType1.PK.ToGuid());
			AssertSend(sender, campaignItem);

			email = campaignItem.DocManagerInfo.AllEDocs.GetMostRecentEDoc(newDocType1.RT_DocType);
			AssertNotNull(email);
			AssertEquals("DT1", email.DocType);
			AssertEquals("Test DocType 1", email.Description);

			//Test DT2
			OrganisationsDataRegistry.Instance.PermanentlySavedCampaignEmailDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newDocType2.PK.ToGuid());

			AssertSend(sender, campaignItem);

			email = campaignItem.DocManagerInfo.AllEDocs.GetMostRecentEDoc(newDocType2.RT_DocType);
			AssertEquals("DT2", email.DocType);
			AssertEquals("Test DocType 2", email.Description);
		}

		public void TestEmailNotAttachedWhenSaveEmailToEDocsIsFalse()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var campaignItem = CreateCampaignTestData();
			var sender = new GlbCompanyCampaignSender(campaignItem.CompanyCampaign);

			AssertEquals("Pre-condtion", 0, campaignItem.DocManagerInfo.AllEDocs.Count);
			AssertSend(sender, campaignItem);
			AssertEquals("Email should NOT have been attached to eDocs", 0, campaignItem.DocManagerInfo.AllEDocs.Count);

			var email = campaignItem.DocManagerInfo.AllEDocs[0];
			AssertNull(email);
		}

		public void TestSendEmailToContactWithImageEmbeddedInHtml()
		{
			string templateHtml =
	@"<html><body><img src='{0}' width='100' height='200' style='HEIGHT: 200px; WIDTH: 100px'/><img src='http://www.asx.com.au/pict.gif' /></body></html>";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var imageBytes = new byte[] { 45, 45, 45 };

			var campaign = PrepareCampaignWithEmbeddedImage(templateHtml, imageBytes);
			campaign.CampaignCoordinator.ProfileImage = null;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default1@cargowise.com";

			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { contact1 }), campaign);
			var campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());

			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Embedded Image", 1, email.Attachments.Count);
			var imageName = email.Attachments[0].DisplayName;

			AssertEndsWith("Image Name", ".png", imageName);

			Assert(email.Attachments[0].Data.SequenceEqual(imageBytes));
		}

		#region Macro image

		public void TestSendEmailToContactWithMacroImages()
		{
			OrganisationsDataRegistry.Instance.LinkTrackingUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/");
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");
			string templateHtml =
				@"<html><body><img src='{0}' macro='(*EmailSenderStaff.ProfilePhotoEncoded*)' width='100' height='200' style='HEIGHT: 200px; WIDTH: 100px'/></body></html>";

			Env.OutgoingMailManager.EmailsCreated.Clear();

			var campaign = PrepareCampaignWithEmbeddedImage(templateHtml, new byte[] { 45, 45, 45 });
			campaign.CampaignCoordinator.ProfileImage = null;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default1@cargowise.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bob2";
			contact2.OC_Email = "default2@cargowise.com";
			Factory.Save();

			//send email without profile image
			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { contact1 }), campaign);
			var campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());

			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
			var link = campaign.TrackedLinks.Find(element => element.GCL_Context == CampaignEmailTemplateEditor.TrackingImageContext).FirstOrDefault();
			AssertNotNull(link);
			AssertEquals(1, campaign.CampaignsItemsSent.Count);
			var campaignItem1 = campaign.CampaignsItemsSent[0];
			AssertEquals(
				FormattableString.Invariant($@"<html><body><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""{OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value}?{OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.Value}&c={EnvProxy.Instance.CurrentCompany.GetLicenceCode()}&x={link.PK.ToGuid().ToString("N")}&u={campaignItem1.PK.ToGuid().ToString("N")}""></body></html>"),
				email.Body);
			AssertEquals(0, email.Attachments.Count);

			//set the profile image and send again.
			Env.OutgoingMailManager.EmailsCreated.Clear();
			campaign.CampaignCoordinator.ProfileImage = new Bitmap(1, 1);
			Factory.Save();

			campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { contact2 }), campaign);
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());

			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
			AssertEquals(2, campaign.CampaignsItemsSent.Count);
			var campaignItem2 = campaign.CampaignsItemsSent[1];
			AssertNotEquals(campaignItem1, campaignItem2);

			AssertEquals("Embedded Image", 1, email.Attachments.Count);
			var imageName = email.Attachments[0].DisplayName;

			AssertEquals(
				FormattableString.Invariant($@"<html><body><img src='{imageName}' style=' WIDTH: 100px' width=""100""><img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""{OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value}?{OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.Value}&c={EnvProxy.Instance.CurrentCompany.GetLicenceCode()}&x={link.PK.ToGuid().ToString("N")}&u={campaignItem2.PK.ToGuid().ToString("N")}""></body></html>"),
				email.Body);

			Assert(email.Attachments[0].Data.SequenceEqual((byte[])campaign.CampaignCoordinator.GS_ProfilePhoto));
		}

		#endregion

		void Campaign_ItemSent(object sender, ItemSentEventArgs e)
		{
			ItemsSent++;
		}

		int ItemsSent;

		void Campaign_CampaignSendEnd(object sender, EventArgs e)
		{
			EndCalled = true;
		}

		bool EndCalled;

		void Campaign_CampaignSendBegin(object sender, EventArgs e)
		{
			BeginCalled = true;
		}

		bool BeginCalled;

		[TestDate(2015, 9, 2, 12, 0, 0)]
		public void TestSendValidCampaigns()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";

			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Tom";
			contact2.OC_Email = "default@cargowise.com";

			OrgContact contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "ZZZ";
			contact3.OC_Email = "default@cargowise.com";

			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1 },
				campaign);

			AssertCollectionContains("Collection should contain Contact1.PK", contact1.PK, campaignContactCollection.GetPKs());
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.CampaignSendBegin += new EventHandler(Campaign_CampaignSendBegin);
			campaignSender.CampaignSendEnd += new EventHandler(Campaign_CampaignSendEnd);
			campaignSender.ItemSent += new ItemSentEventHandler(Campaign_ItemSent);

			ReadOnlyCollection<CampaignContact> contactsNotSentTo = campaignSender.SendValidCampaigns();
			AssertEquals("No contacts failed to send", 0, contactsNotSentTo.Count);
			AssertEquals("Campaigns sent count", 1, campaign.CampaignsItemsSent.Count);
			AssertEquals(TrackingStatusCodes.Codes.UNV, campaign.CampaignsItemsSent[0].G8_TrackingStatus);

			GlbCompanyCampaignItem campaignSent = campaign.CampaignsItemsSent[0];
			AssertEquals("Campaign should have been sent to Contact1", contact1.PK, campaignSent.G8_RecipientID);
			AssertEquals("System Create User should be", GlbStaff.CurrentUser.GS_Code, campaignSent.G8_SystemCreateUser);
			AssertEquals("Last Sent Time should be populated with current send date", campaignSent.G8_LastSentTimeUtc,
				new ZDateTime(2015, 9, 2, 12, 0, 0));
			Assert("Factory should have been saved", !campaignSent.HasChanges);

			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact2, contact3 }, campaign);

			contact2.OC_Email = ZString.Empty;
			Factory.Save();
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact2, contact3 }, campaign);

			ItemsSent = 0;
			BeginCalled = false;
			EndCalled = false;

			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.CampaignSendBegin += new EventHandler(Campaign_CampaignSendBegin);
			campaignSender.CampaignSendEnd += new EventHandler(Campaign_CampaignSendEnd);
			campaignSender.ItemSent += new ItemSentEventHandler(Campaign_ItemSent);

			contactsNotSentTo = campaignSender.SendValidCampaigns();
			AssertEquals("1 contact failed to send", 1, contactsNotSentTo.Count);
			AssertEquals("Campaigns sent count", 3, campaign.CampaignsItemsSent.Count);

			Assert(BeginCalled);
			Assert(EndCalled);
			AssertEquals("Item Sent Event Fired twice as 2 items were sent", 2, ItemsSent);

			int emailCount = 0;
			int printCount = 0;

			foreach (GlbCompanyCampaignItem item in campaign.CampaignsItemsSent)
			{
				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.PrintCode)
				{
					printCount++;
				}

				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode)
				{
					emailCount++;
				}
			}

			AssertEquals("2 emails", 2, emailCount);
			AssertEquals("1 print", 1, printCount);

			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact2 }, campaign);
			OrgContact contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "DDD";
			contact4.OC_Email = "default@cargowise.com";
			Factory.Save();

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah" + StartTag);
			bool exceptionThrown = false;

			try
			{
				campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact2, contact4 }, campaign);
				new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList())
					.SendValidCampaigns();
			}
			catch (DocumentParsingFailedException)
			{
				exceptionThrown = true;
			}

			Assert("ParException should have been thrown", exceptionThrown);
			AssertEquals("Campaigns sent count is still", 3, campaign.CampaignsItemsSent.Count);
		}

		[TestDate(2015, 9, 2, 12, 0, 0)]
		public void TestSendErrors()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";

			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_ContactName = "Tom";
			contact2.OC_Email = "default@cargowise.com";

			OrgContact contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "ZZZ";
			contact3.OC_Email = "default@cargowise.com";

			Factory.Save();

			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;

			Factory.Save();

			var staffInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<GlbStaff>(staff.PK);
			staffInOtherFactory.GS_GB_HomeBranch = ZGuid.Empty;

			staffInOtherFactory.Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			campaign = newFactory.Load<GlbCompanyCampaign>(campaign.PK);
			contact1 = newFactory.Load<OrgContact>(contact1.PK);

			var campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1 }, campaign);

			AssertCollectionContains("Collection should contain Contact1.PK", contact1.PK, campaignContactCollection.GetPKs());
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());

			ReadOnlyCollection<CampaignContact> contactsNotSentTo = campaignSender.SendValidCampaigns();
			AssertEquals("Failed to send", 1, contactsNotSentTo.Count);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			AssertEquals(1, campaignSender.SendErrors.Count());
		}

		public void TestSendValidCampaigns_ContactsWithNonDeliveryReport()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "and@test.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Samuel";
			contact2.OC_Email = "samuel@test.com";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Edward";
			contact3.OC_Email = "malformed";
			Factory.Save();

			var campaign = GetCampaignForSendValidCampaignsTest();
			contact2.IsNDR = true;
			contact2.EmailAddress.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;

			var campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);

			SendValidCampaigns(campaign, new List<BusinessObject>() { contact2 });
			AssertEquals(0, campaignItems.Count);

			SendValidCampaigns(campaign, new List<BusinessObject>() { contact3 });
			AssertEquals(0, campaignItems.Count);

			SendValidCampaigns(campaign, new List<BusinessObject>() { contact1 });
			AssertEquals(1, campaignItems.Count);
		}

		public void TestSendValidCampaigns_WithTargetList()
		{
			AssertSendValidCampaigns_WithTargetList(CampaignTypeList.Codes.TargetList);
		}

		public void TestSendValidCampaigns_WithMaster()
		{
			AssertSendValidCampaigns_WithTargetList(CampaignTypeList.Codes.DripMarketing);
		}

		void AssertSendValidCampaigns_WithTargetList(ZString campaignType)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "and@test.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Samuel";
			contact2.OC_Email = "samuel@test.com";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Edward";
			contact3.OC_Email = "edward@test.com";
			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "Michael";
			contact4.OC_Email = "mic@test.com";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_EmailSubject = "Best campaign ever";

			var campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);

			campaign.G0_BroadcastVoteSurveyExam = campaignType;
			SendValidCampaigns(campaign, new List<BusinessObject>() { contact1, contact3 });

			int emailCount = 0;
			int printCount = 0;
			int targetListCount = 0;

			foreach (GlbCompanyCampaignItem item in campaign.CampaignsItemsSent)
			{
				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.PrintCode)
				{
					printCount++;
				}

				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode)
				{
					emailCount++;
				}

				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.TargetListCode)
				{
					targetListCount++;
				}
			}

			AssertEquals("2 emails", 2, targetListCount);
			AssertEquals("0 emails", 0, emailCount);
			AssertEquals("0 print", 0, printCount);
			Assert(campaign.CampaignsItemsSent[0].G8_LastSentTimeUtc.IsEmpty);
			Assert(campaign.CampaignsItemsSent[1].G8_LastSentTimeUtc.IsEmpty);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign2.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign2.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign2.G0_EmailSubject = "Best campaign ever";

			var campaignItems2 = campaign2.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems2.Count);

			campaign2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			SendValidCampaigns(campaign2, new List<BusinessObject>() { contact2, contact4 });

			emailCount = 0;
			printCount = 0;

			foreach (GlbCompanyCampaignItem item in campaign2.CampaignsItemsSent)
			{
				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.PrintCode)
				{
					printCount++;
				}

				if (item.G8_DeliveryMethod == GlbCompanyCampaignItemLookups.DeliveryMethodsConstants.EmailCode)
				{
					emailCount++;
				}
			}

			AssertEquals("2 emails", 2, emailCount);
			AssertEquals("0 print", 0, printCount);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendValidCampaigns_InquiryDoesNotCreateNewCycle()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var campaign = GetCampaignForSendValidCampaignsTest();

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "andrew@test.com";

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OC_LinkedContact = contact1.PK;

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			inquiry.RelatedParentActivityPivotCollection.AddNewPivot(inquiry);
			Factory.Save();

			var campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);

			AssertNoExceptionThrown(() =>
			{
				SendValidCampaigns(campaign, new List<BusinessObject>() { inquiry });
			});

			campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("1 Item should be sent", 1, campaignItems.Count);
		}

		public void TestSendValidCampaigns_InquiryExistingCycleIsIgnored()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var campaign1 = GetCampaignForSendValidCampaignsTest();
			var campaign2 = GetCampaignForSendValidCampaignsTest();
			var campaign3 = GetCampaignForSendValidCampaignsTest();

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "andrew@test.com";

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OC_LinkedContact = contact1.PK;

			campaign1.RelatedChildActivityPivotCollection.AddNewPivot(campaign2);
			campaign2.RelatedChildActivityPivotCollection.AddNewPivot(campaign3);
			campaign3.RelatedChildActivityPivotCollection.AddNewPivot(campaign1);

			campaign1.RelatedChildActivityPivotCollection.AddNewPivot(inquiry);
			Factory.Save();

			var campaignItems = campaign1.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);

			AssertNoExceptionThrown(() =>
			{
				SendValidCampaigns(campaign1, new List<BusinessObject>() { inquiry });
			});

			campaignItems = campaign1.CampaignsItemsSent;
			AssertEquals("1 Item should be sent", 1, campaignItems.Count);
		}

		public void TestSendValidCampaigns_SourceCampaignDoesNotCreateNewCycle()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var campaign = GetCampaignForSendValidCampaignsTest();
			var sourceCampaign = GetCampaignForSendValidCampaignsTest();
			campaign.SourceCampaignPK = sourceCampaign.PK;

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "andrew@test.com";

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(sourceCampaign);
			sourceCampaign.RelatedParentActivityPivotCollection.AddNewPivot(sourceCampaign);
			Factory.Save();

			AssertEquals("campaign has no parent at first", 0, campaign.RelatedParentActivityPivotCollection.Count());
			AssertEquals("sourceCampaign1 has 1 parent at first", 1, sourceCampaign.RelatedChildActivityPivotCollection.Count());
			SendValidCampaigns(campaign, new List<BusinessObject>() { contact1 });
			AssertEquals("campaign has no parent at end", 0, campaign.RelatedParentActivityPivotCollection.Count());
			AssertEquals("sourceCampaign1 has 1 parent at end", 1, sourceCampaign.RelatedChildActivityPivotCollection.Count());
		}

		public void TestSendValidCampaigns_ReplaceCampaignSalesRelationLinksToToCampaignItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Andrew";
			contact1.OC_Email = "andrew@test.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Samuel";
			contact2.OC_Email = "samuel@test.com";
			Factory.Save();

			var campaign = GetCampaignForSendValidCampaignsTest();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			inquiry.O1_OC_LinkedContact = contact2.PK;
			inquiry.O1_SystemCreateTimeUtc = new ZDateTime(2014, 5, 20);
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			opportunity.P8_OC = contact2.PK;
			opportunity.P8_SystemCreateTimeUtc = new ZDateTime(2014, 6, 19);
			var communication1 = Factory.NewWithValidTestData<OrgSalesCall>();
			communication1.OQ_OH = org.PK;
			communication1.OQ_OC = contact2.PK;
			var communication2 = Factory.NewWithValidTestData<OrgSalesCall>();
			communication2.OQ_OH = org.PK;
			communication2.OQ_OC = contact2.PK;

			campaign.RelatedParentActivityPivotCollection.AddNewPivot(inquiry);
			campaign.RelatedParentActivityPivotCollection.AddNewPivot(opportunity);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communication1);
			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communication2);
			Factory.Save();

			var campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);

			SendValidCampaigns(campaign, new List<BusinessObject>() { contact1 });
			AssertEquals(1, campaignItems.Count);
			AssertContainsExactElementsInAnyOrder("Should not replace any links because contacts do not match",
				Enumerable.Empty<IRelatableActivity>(), campaignItems[0].RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Should not replace any links because contacts do not match",
				Enumerable.Empty<IRelatableActivity>(), campaignItems[0].RelatedChildActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { inquiry, opportunity },
				campaign.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder(new IRelatableActivity[] { communication1, communication2 },
				campaign.RelatedChildActivityPivotCollection.Activities);

			campaignItems.RemoveAndDeleteAll();
			SendValidCampaigns(campaign, new List<BusinessObject>() { contact2 });
			AssertContainsExactElementsInAnyOrder(
				"Should have only linked to inquiry and not opportunity because campaign item only allowed 1 parent",
				new[] { inquiry }, campaignItems[0].RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Should have only linked to both communications",
				new[] { communication1, communication2 }, campaignItems[0].RelatedChildActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Opportunity should still be parent of campaign", new[] { opportunity },
				campaign.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Communications should no longer be children of campaign",
				Enumerable.Empty<IRelatableActivity>(), campaign.RelatedChildActivityPivotCollection.Activities);
		}

		GlbCompanyCampaign GetCampaignForSendValidCampaignsTest()
		{
			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_EmailAddress = $"{nameof(coordinator)}@ema.il";
			coordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			return campaign;
		}

		void SendValidCampaigns(GlbCompanyCampaign campaign, List<BusinessObject> contacts)
		{
			var collection = ContactCollection(contacts, campaign);
			var campaignSender = new GlbCompanyCampaignSender(campaign, collection.Cast<CampaignContact>().ToList());
			campaignSender.SendValidCampaigns();
		}

		public void TestSendValidCampaigns_ReplaceCampaignSalesRelationLinksToToCampaignItem_ForSingleChild()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			contact.OC_Email = "andrew@test.com";
			Factory.Save();

			var campaign = GetCampaignForSendValidCampaignsTest();
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			communication.OQ_OH = org.PK;
			communication.OQ_OC = contact.PK;

			campaign.RelatedChildActivityPivotCollection.AddNewPivot(communication);
			Factory.Save();

			var campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);

			SendValidCampaigns(campaign, new List<BusinessObject>() { contact });
			AssertContainsExactElementsInAnyOrder("Should have linked item to communication", new[] { communication },
				campaignItems[0].RelatedChildActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Communication should no longer be child of campaign",
				Enumerable.Empty<IRelatableActivity>(), campaign.RelatedChildActivityPivotCollection.Activities);
		}

		public void TestSendValidCampaigns_CreateRelationshipForNewEntity_Inquiry()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Edward";
			contact.OC_Email = "edward@test.com";

			var campaign = GetCampaignForSendValidCampaignsTest();
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OC_LinkedContact = contact.PK;
			Factory.Save();

			var campaignItems = campaign.CampaignsItemsSent;
			AssertEquals("Precondition", 0, campaignItems.Count);
			SendValidCampaigns(campaign, new List<BusinessObject>() { inquiry });
			AssertContainsExactElementsInAnyOrder("Should have linked item to inquiry", new[] { inquiry },
				campaignItems[0].RelatedParentActivityPivotCollection.Activities);
		}

		public void TestSendValidCampaigns_CreateRelationshipForNewEntity_CampaignAndCampaignItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Edward";
			contact1.OC_Email = "edward@test.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Jenny";
			contact2.OC_Email = "jenny@test.com";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Sam";
			contact3.OC_Email = "sam@test.com";

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_OC_LinkedContact = contact1.PK;

			var campaign1 = GetCampaignForSendValidCampaignsTest();
			var campaign2 = GetCampaignForSendValidCampaignsTest();
			var campaign3 = GetCampaignForSendValidCampaignsTest();
			campaign2.SourceCampaignPK = campaign1.PK;
			campaign3.SourceCampaignPK = campaign2.PK;
			Factory.Save();

			SendValidCampaigns(campaign1, new List<BusinessObject>() { inquiry });
			var campaign1Items = campaign1.CampaignsItemsSent;
			AssertEquals("Precondition", 1, campaign1Items.Count);

			var campaign2Items = campaign2.CampaignsItemsSent;
			SendValidCampaigns(campaign2, new List<BusinessObject>() { inquiry, contact2, contact3 });
			AssertContainsExactElementsInAnyOrder("Should have linked to campaign2", new[] { campaign1 },
				campaign2.RelatedParentActivityPivotCollection.Activities);
			AssertEquals("Should not create another link from campaign2 to inquiry since campaign1 is already linked to it", 1,
				campaign2Items.FindByRecipientPK(inquiry.PK).RelatedParentActivityPivotCollection.Count());
			AssertContainsExactElementsInAnyOrder("Should have linked item to campaign item for inquiry",
				new[] { campaign1Items[0] },
				campaign2Items.FindByRecipientPK(inquiry.PK).RelatedParentActivityPivotCollection.Activities);
			AssertEquals("Should not have linked item to any campaign item for contact2", 0,
				campaign2Items.FindByRecipientPK(contact2.PK).RelatedParentActivityPivotCollection.Count());
			AssertEquals("Should not have linked item to any campaign item for contact3", 0,
				campaign2Items.FindByRecipientPK(contact3.PK).RelatedParentActivityPivotCollection.Count());

			var campaign3Items = campaign3.CampaignsItemsSent;
			SendValidCampaigns(campaign3, new List<BusinessObject>() { inquiry });
			AssertContainsExactElementsInAnyOrder("Should have linked to campaign3", new[] { campaign2 },
				campaign3.RelatedParentActivityPivotCollection.Activities);
			AssertEquals("Should not create another link from campaign3 to inquiry since campaign1 is already linked to it", 1,
				campaign3Items.FindByRecipientPK(inquiry.PK).RelatedParentActivityPivotCollection.Count());
			AssertContainsExactElementsInAnyOrder("Should have linked item to campaign item for inquiry",
				new[] { campaign2Items.FindByRecipientPK(inquiry.PK) },
				campaign3Items.FindByRecipientPK(inquiry.PK).RelatedParentActivityPivotCollection.Activities);

			SendValidCampaigns(campaign3, new List<BusinessObject>() { contact2 });
			AssertContainsExactElementsInAnyOrder("Should still be linked to campaign3 when sending to contact2",
				new[] { campaign2 }, campaign3.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Should have linked item to campaign item for contact2",
				new[] { campaign2Items.FindByRecipientPK(contact2.PK) },
				campaign3Items.FindByRecipientPK(contact2.PK).RelatedParentActivityPivotCollection.Activities);

			SendValidCampaigns(campaign3, new List<BusinessObject>() { contact3 });
			AssertContainsExactElementsInAnyOrder("Should still be linked to campaign3 when sending to contact3",
				new[] { campaign2 }, campaign3.RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Should still be linked item to campaign item for contact2",
				new[] { campaign2Items.FindByRecipientPK(contact2.PK) },
				campaign3Items.FindByRecipientPK(contact2.PK).RelatedParentActivityPivotCollection.Activities);
			AssertContainsExactElementsInAnyOrder("Should have linked item to campaign item for contact3",
				new[] { campaign2Items.FindByRecipientPK(contact3.PK) },
				campaign3Items.FindByRecipientPK(contact3.PK).RelatedParentActivityPivotCollection.Activities);
		}

		[TestDate(2016, 3, 23, 4, 4, 4)]
		public void TestSendCampaigns()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_BatchCountDefault = 0; //Campaign has 1 error

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			OrgContact contact2 = CreateContact("Tom", org);
			OrgContact contact3 = CreateContact("ZZZ", org);

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			campaign.G0_DeDuplicateContacts = false;

			GlbCampaignContactCollection campaignContactCollection =
				ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			campaignContactCollection.Sort(new SortInfo(ViewCampaignContactSchema.VCC_ContactName.Name,
				ListSortDirection.Ascending));
			AssertEquals("Campaign should have loaded no contacts", 0, campaignContactCollection.Count);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);
			form.UserChoiceContinueWithSending = true;

			Factory.Save();
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.CheckAndSendCampaigns();

			var batchCountDefaultErrorMessage = $@"{GlbCompanyCampaignSender.NotificationConstants.CorrectAllErrorsMessage}
- The default number of Campaigns sent in each batch must be larger than 0";

			AssertCorrectMessageIsDisplayed(form, batchCountDefaultErrorMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent", 0, campaign.CampaignsItemsSent.Count);

			campaign.G0_BatchCountDefault = 2;
			campaign.HtmlDocumentBlob = ZBlob.Empty;

			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form, GlbCompanyCampaignSender.NotificationConstants.SaveCampaignMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			Factory.Save();

			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
				$@"{NotificationConstants.CorrectAllErrorsMessage}
- {NotificationConstants.EmailContentCannotBeEmpty}",
				NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii(CampaignEmailTemplateEditor.SkeletonHtml);
			Factory.Save();
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
				$@"{NotificationConstants.CorrectAllErrorsMessage}
- {NotificationConstants.EmailContentCannotBeEmpty}",
				NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("");
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Survey;
			Factory.Save();
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
	$@"{NotificationConstants.CorrectAllErrorsMessage}
- {NotificationConstants.EmailContentCannotBeEmpty}
- {string.Format("HTML document should include field (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName)}",
	NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			campaign.LoadFilteredContacts(campaignContactCollection);
			Factory.Save();
			form.UserChoiceContinueWithSending = false;
			form.MessageOnCampaignSendingArgs = null;
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);
			campaignSender.CheckAndSendCampaigns();
			AssertNull("Sending cancelled, No error message should be displayed", form.MessageOnCampaignSendingArgs);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);

			form.UserChoiceContinueWithSending = true;
			campaignSender.CheckAndSendCampaigns();
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Campaign should have 1 loaded contacts", 1, campaignContactCollection.Count);

			AssertCorrectMessageIsDisplayed(form,
				ZString.Format(GlbCompanyCampaignSender.NotificationConstants.CampaignsSentMessage, 2),
				GlbCompanyCampaignSender.NotificationConstants.CampaignsSentSummary, false);
			AssertEquals("Campaigns sent count", 2, campaign.CampaignsItemsSent.Count);
			Assert(!campaign.CampaignsItemsSent[0].G8_LastSentTimeUtc.IsEmpty);
			Assert(!campaign.CampaignsItemsSent[1].G8_LastSentTimeUtc.IsEmpty);
			AssertEquals(new DateTime(2016, 3, 23, 4, 4, 4), campaign.CampaignsItemsSent[0].G8_LastSentTimeUtc);
			AssertEquals(new DateTime(2016, 3, 23, 4, 4, 4), campaign.CampaignsItemsSent[1].G8_LastSentTimeUtc);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah" + StartTag + "132123123213");
			Factory.Save();
			OrgContact contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "DDD";
			contact4.OC_Email = "default@cargowise.com";
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3, contact4 },
				campaign);
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);
			campaignSender.CheckAndSendCampaigns();
			Assert("Should display error message about could not parse document",
				!form.MessageOnCampaignSendingArgs.Message.IsEmpty);
			AssertEquals("Should display summary", GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary,
				form.MessageOnCampaignSendingArgs.Summary);
			AssertEquals("Campaigns sent count", 2, campaign.CampaignsItemsSent.Count);

			contact1.Delete();
			contact2.Delete();
			contact3.Delete();
			contact4.Delete();
			Factory.Save();
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3, contact4 },
				campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah" + StartTag + "132123123213" + EndTag);
			Factory.Save();

			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form, GlbCompanyCampaignSender.NotificationConstants.NoSelectedRecipientsMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);
		}

		[TestDate(2016, 3, 23, 4, 4, 4)]
		public void TestSendCampaigns_Voting()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			OrgContact contact2 = CreateContact("Tom", org);
			OrgContact contact3 = CreateContact("ZZZ", org);

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			campaign.G0_DeDuplicateContacts = false;

			GlbCampaignContactCollection campaignContactCollection =
				ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			campaignContactCollection.Sort(new SortInfo(ViewCampaignContactSchema.VCC_ContactName.Name,
				ListSortDirection.Ascending));
			AssertEquals("Campaign should have loaded no contacts", 0, campaignContactCollection.Count);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);
			form.UserChoiceContinueWithSending = true;

			Factory.Save();
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);

			campaign.HtmlDocumentBlob = ZBlob.FromAscii("");
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;
			Factory.Save();
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
	$@"{NotificationConstants.CorrectAllErrorsMessage}
- {NotificationConstants.EmailContentCannotBeEmpty}
- {string.Format("HTML document should include field (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName)}",
	NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);
		}

		public void TestSendCampaigns_WithDripSelected()
		{
			AssertSendCampaigns_WithTargetListSelected(CampaignTypeList.Codes.DripMarketing);
		}

		public void TestSendCampaigns_WithTargetListSelected()
		{
			AssertSendCampaigns_WithTargetListSelected(CampaignTypeList.Codes.TargetList);
		}

		void AssertSendCampaigns_WithTargetListSelected(ZString campaignType)
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_BatchCountDefault = 0; //Campaign has 1 error
			campaign.G0_BroadcastVoteSurveyExam = campaignType;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			OrgContact contact2 = CreateContact("Tom", org);
			OrgContact contact3 = CreateContact("ZZZ", org);

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			campaign.G0_DeDuplicateContacts = false;

			GlbCampaignContactCollection campaignContactCollection =
				ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			campaignContactCollection.Sort(new SortInfo(ViewCampaignContactSchema.VCC_ContactName.Name,
				ListSortDirection.Ascending));
			AssertEquals("Campaign should have loaded no contacts", 0, campaignContactCollection.Count);
			AssertEquals("Campaigns sent count", 0, campaign.CampaignsItemsSent.Count);
			form.UserChoiceContinueWithSending = true;

			Factory.Save();
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);
			campaignSender.CheckAndSendCampaigns();

			var batchCountDefaultErrorMessage = $@"{GlbCompanyCampaignSender.NotificationConstants.CorrectAllErrorsMessage}
- The default number of Campaigns sent in each batch must be larger than 0";

			AssertCorrectMessageIsDisplayed(form, batchCountDefaultErrorMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
			AssertEquals("Campaigns sent", 0, campaign.CampaignsItemsSent.Count);

			campaign.G0_BatchCountDefault = 2;
			campaign.HtmlDocumentBlob = ZBlob.Empty;

			Factory.Save();

			form.UserChoiceContinueWithSending = true;
			campaignSender.CheckAndSendCampaigns();
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Campaigns sent count", 3, campaign.CampaignsItemsSent.Count);
		}

		public void TestSendCampaigns_QuestionsShouldBeMadeReadOnlyIfCampaignsSent()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = CreateContact("Bob", org);
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_DeDuplicateContacts = false;
			campaign.G0_BatchCountDefault = 0;

			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			form.UserChoiceContinueWithSending = true;
			form.MessageOnCampaignSendingArgs = null;

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact },
				campaign);

			new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList())
				.CheckAndSendCampaigns();
			AssertEquals("Campaigns should not be sent", 0, campaign.CampaignsItemsSent.Count);
			Assert("Should not be read-only if campaign has not been sent", !campaign.Questions.ReadOnly);

			campaign.G0_BatchCountDefault = 1;
			campaign.LoadFilteredContacts(campaignContactCollection);
			Factory.Save();
			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending +=
				new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form,
				ZString.Format(GlbCompanyCampaignSender.NotificationConstants.CampaignsSentMessage, 1),
				GlbCompanyCampaignSender.NotificationConstants.CampaignsSentSummary, false);
			AssertEquals("Campaigns sent count", 1, campaign.CampaignsItemsSent.Count);
		}

		public void TestSendCampaigns_Concurrency()
		{
			var campaign = Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_DeDuplicateContacts = false;
			campaign.G0_BatchCountDefault = 10;
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			OrgContact contact2 = CreateContact("Tom", org);
			OrgContact contact3 = CreateContact("ZZZ", org);

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection =
				ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			campaignContactCollection.Sort(new SortInfo(ViewCampaignContactSchema.VCC_ContactName.Name,
				ListSortDirection.Ascending));
			AssertEquals("Campaign should have 3 loaded contacts", 3, campaignContactCollection.Count);

			var sender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			sender.ShouldContinueWithSending += (num, campaignItem) => { return true; };

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;
			var campaignInOtherFactory =
				otherFactory.Load<GlbCompanyCampaignTest.GlbCompanyCampaignForTest>(campaign.PK);
			GlbCampaignContactCollection campaignContactCollectionInOtherFactory =
				ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaignInOtherFactory);
			campaignInOtherFactory.LoadFilteredContacts(campaignContactCollectionInOtherFactory);
			AssertEquals("Campaign should have 3 loaded contacts", 3, campaignContactCollectionInOtherFactory.Count);

			var sender2 = new GlbCompanyCampaignSender(campaignInOtherFactory,
				campaignContactCollectionInOtherFactory.Cast<CampaignContact>().ToList(), false);
			sender2.ShouldContinueWithSending += (num, campaignItem) => { return true; };
			sender2.CheckAndSendCampaigns();
			AssertEquals(3, campaignInOtherFactory.CampaignsItemsSent.Count);

			sender.CheckAndSendCampaigns();
			AssertEquals(3, campaign.CampaignsItemsSent.Count);
		}

		public void TestSendCampaigns_LinkTracking()
		{
			var campaign = Helper.GetCampaignForTestWithoutErrors();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			var link1 = campaign.TrackedLinks.AddNew();
			var link2 = campaign.TrackedLinks.AddNew();
			link1.GCL_URL = "http://localhost";
			link1.GCL_Context = "1";
			link2.GCL_URL = "http://localhost/";
			link2.GCL_Context = "2";
			AssertEquals("Pre:", false, link1.GCL_IsTracked);
			AssertEquals("Pre:", false, link2.GCL_IsTracked);
			Factory.Save();

			campaign.FilteredContacts.Load(new ZQuery(ViewCampaignContactSchema.PK, contact1.PK));
			AssertEquals("Campaign should have contacts", 1, campaign.FilteredContacts.Count);
			var sender = new GlbCompanyCampaignSender(campaign, campaign.FilteredContacts.Cast<CampaignContact>());
			sender.ShouldContinueWithSending += (num, campaignItem) => { return true; };
			sender.CheckAndSendCampaigns();

			AssertEquals("IsTracked set true when campaign is sent", true, link1.GCL_IsTracked);
			AssertEquals("IsTracked set true when campaign is sent", true, link2.GCL_IsTracked);
			AssertEquals("link is saved", false, link1.HasChanges);
			AssertEquals("link is saved", false, link2.HasChanges);

			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_Context = "3";
			link3.GCL_URL = "http://localhost/3";
			Factory.Save();

			var campaignDependentCollection = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
			campaignDependentCollection.Load();
			GlbCompanyCampaignItem[] campaignItems = campaignDependentCollection.Cast<GlbCompanyCampaignItem>().ToArray();
			AssertEquals(1, campaignItems.Length);
			GlbCompanyCampaignSender resender = new GlbCompanyCampaignSender(campaign,
				campaignDependentCollection.Cast<GlbCompanyCampaignItem>());
			resender.ShouldContinueWithSending += (num, campaignItem) => { return true; };
			resender.CheckAndSendCampaigns();
			AssertEquals("IsTracked set true when campaign is resent", true, link3.GCL_IsTracked);
		}

		public void TestCheckAndSendCampaigns_NoURLException()
		{
			var campaign = Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_BatchCountDefault = 10;
			campaign.HtmlDocumentBlob = ZBlob.FromAscii(string.Format("blah blah (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName));
			campaign.G0_DeDuplicateContacts = true;
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact1 = CreateContact("Bob", org);
			contact1.OC_Email = "default1@cargowise.com";
			contact1.OC_IsActive = true;
			OrgContact contact2 = CreateContact("Tom", org);
			contact2.OC_Email = "default2@cargowise.com";
			contact2.OC_IsActive = true;
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);
			campaignContactCollection.Sort(new SortInfo(ViewCampaignContactSchema.VCC_ContactName.Name,
				ListSortDirection.Ascending));
			AssertEquals("Campaign should have 2 loaded contacts", 2, campaignContactCollection.Count);

			var form = new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			form.UserChoiceContinueWithSending = true;
			form.MessageOnCampaignSendingArgs = null;

			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmeh.webserver.cargowise.com");
			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += form.Campaign_ShouldContinueWithSending;
			campaignSender.MessageOnCampaignSending += form.Campaign_MessageOnCampaignSending;
			campaignSender.CheckAndSendCampaigns();

			AssertCorrectMessageIsDisplayed(form,
				ZString.Format(GlbCompanyCampaignSender.NotificationConstants.CampaignsSentMessage, 2),
				GlbCompanyCampaignSender.NotificationConstants.CampaignsSentSummary, false);

			OrgContact contact3 = CreateContact("ZZZ", org);
			Factory.Save();
			campaignContactCollection = ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, string.Empty);
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += form.Campaign_ShouldContinueWithSending;
			campaignSender.MessageOnCampaignSending += form.Campaign_MessageOnCampaignSending;

			campaignSender.CheckAndSendCampaigns();

			var expectedMessage = $@"{NotificationConstants.CorrectAllErrorsMessage}
- {UnsubscribeUrlHelper.GetUriFormatErrorMessage(campaign.Company)}";
			AssertCorrectMessageIsDisplayed(form, expectedMessage, GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);

			AssertEquals(2, campaign.CampaignsItemsSent.Count);

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var sent = newFactory.GetDatabaseCount(typeof(GlbCompanyCampaignItem),
				new ZQuery(GlbCompanyCampaignItemSchema.G8_G0, campaign.PK));

			AssertEquals(2, sent);
		}

		public void TestCheckAndSendCampaigns_InvalidCampaignURLSettings()
		{
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign =
				Helper.GetCampaignForTestWithoutErrors();
			GlbCompanyCampaignTest.FakeFormForTest form =
				new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			campaign.HasChanges = false;

			GlbCampaignContactCollection campaignContactCollection = ContactCollection(new List<BusinessObject>(), campaign);

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form, GlbCompanyCampaignSender.NotificationConstants.NoSelectedRecipientsMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);

			campaign.HtmlDocumentBlob =
				ZBlob.FromAscii(string.Format("blah blah (*{0}*)", GlbCompanyCampaign.CampaignURLDocFieldName));
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Voting;

			Factory.Save();

			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.CheckAndSendCampaigns();

			var expectedMessage = $@"{NotificationConstants.CorrectAllErrorsMessage}
- {UnsubscribeUrlHelper.GetUriFormatErrorMessage(campaign.Company)}";
			AssertCorrectMessageIsDisplayed(form, expectedMessage, GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);

			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty,
				"http://mehmeh.webserver.cargowise.com");
			campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.MessageOnCampaignSending +=
				new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.CheckAndSendCampaigns();
			AssertCorrectMessageIsDisplayed(form, GlbCompanyCampaignSender.NotificationConstants.NoSelectedRecipientsMessage,
				GlbCompanyCampaignSender.NotificationConstants.CannotSendCampaignsSummary, true);
		}

		public void TestSendToContactsFromSendersPool()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var glbStaffCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffCoordinator.GS_EmailAddress = $"{nameof(glbStaffCoordinator)}@ema.il";
			glbStaffCoordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var glbStaffPool = Enumerable.Range(1, 3)
				.Select(i =>
				{
					var glbStaff = Factory.NewWithValidTestData<GlbStaff>();
					glbStaff.GS_EmailAddress = $"Staff{i}@ema.il";
					glbStaff.GS_GB_HomeBranch = Env.CurrentBranchPK;
					return glbStaff;
				})
				.ToArray();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContacts = Enumerable.Range(1, 10)
				.Select(i =>
				{
					var orgContact = orgHeader.Contacts.AddNew();
					orgContact.OC_ContactName = $"Bob{i}";
					orgContact.OC_Email = $"{orgContact.OC_ContactName}@ema.il";
					return orgContact;
				})
				.ToArray();
			Factory.Save();

			var glbCompanyCampaign = Helper.GetCampaignForTestWithoutErrors();
			glbCompanyCampaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			glbCompanyCampaign.G0_GS_NKCampaignCoordinator = glbStaffCoordinator.GS_Code;
			glbCompanyCampaign.G0_CampaignName = "Enlarge your sales!";
			glbCompanyCampaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			glbStaffPool.ForEach(staff =>
			{
				var campaignSenderPoolItem = glbCompanyCampaign.SenderPool.AddNew();
				campaignSenderPoolItem.GCP_GS_NKSender = staff.GS_Code;
			});
			AssertEquals("Senders count", glbStaffPool.Length, glbCompanyCampaign.SenderPool.Count);
			glbCompanyCampaign.SenderPool[0].GCP_SendRatio = 3;
			AssertEquals("Check count to direct access", 3, glbCompanyCampaign.SenderPool.Count);
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(orgContacts), glbCompanyCampaign);
			var campaignSender = new GlbCompanyCampaignSender(glbCompanyCampaign,
				campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;

			AssertEquals(0, glbCompanyCampaign.CampaignsItemsSent.Count);
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());

			var generatedGlbCompanyCampaignItems =
				glbCompanyCampaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.GroupBy(item => item.G8_GS_NKSender)
					.ToDictionary(items => items.Key);
			AssertEquals(orgContacts.Length, generatedGlbCompanyCampaignItems.Select(pair => pair.Value.Count()).Sum());

			AssertEquals(glbCompanyCampaign.SenderPool.Count, generatedGlbCompanyCampaignItems.Count);
			AssertSenderPoolResultGroup(generatedGlbCompanyCampaignItems, glbCompanyCampaign.SenderPool[0].Sender, 6);
			AssertSenderPoolResultGroup(generatedGlbCompanyCampaignItems, glbCompanyCampaign.SenderPool[1].Sender, 2);
			AssertSenderPoolResultGroup(generatedGlbCompanyCampaignItems, glbCompanyCampaign.SenderPool[2].Sender, 2);
		}

		static void AssertSenderPoolResultGroup(
			IReadOnlyDictionary<ZString, IGrouping<ZString, GlbCompanyCampaignItem>> generatedGlbCompanyCampaignItems,
			GlbStaff glbStaffSender, int expectedCount)
		{
			var campaignItems = generatedGlbCompanyCampaignItems[glbStaffSender.GS_Code];
			AssertEquals(expectedCount, campaignItems.Count());
			campaignItems.ForEach(item =>
			{
				AssertEquals(glbStaffSender.GS_EmailAddress, item.SenderEmailAddress);
				AssertEquals($"After sending {nameof(item.G8_SenderEmailAddress)} should be filled", glbStaffSender.GS_EmailAddress,
					item.G8_SenderEmailAddress);
			});
		}

		public void TestReSendFromSendersPoolShouldUseCurrentPoolSender()
		{
			var testData = PrepareResendTestForPool();

			// change Pool and resend (should use sender from the new pool)
			var newSender = ChangePool(testData.Item1);
			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(testData.Item1, new[] { testData.Item3 });
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(1, testData.Item1.CampaignsItemsSent.Count);

			var companyCampaignItemResent = testData.Item1.CampaignsItemsSent[0];
			AssertEquals("Should use already assigned pool sender", newSender.GS_Code, companyCampaignItemResent.G8_GS_NKSender);
			AssertEquals("Should use already assigned pool sender", newSender.GS_EmailAddress,
				companyCampaignItemResent.G8_SenderEmailAddress);
		}

		public void TestSendEmailToContact_EmailSenderStaff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "testcoordinator@test.com";
			staff.GS_Code = "COR";
			staff.GS_FullName = "Test Coordinator";

			var campaign = Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.HtmlDocumentBlob = ZBlob.FromAscii($"test html (*{GlbCompanyCampaign.CampaignURLDocFieldName}*)");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = CreateContact("Test", org);
			contact.OC_Email = "testcontact@test.com";
			contact.OC_IsActive = true;
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject> { contact }, campaign);
			campaign.LoadFilteredContacts(campaignContactCollection);
			AssertEquals("Campaign should have 1 loaded contact", 1, campaignContactCollection.Count);

			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmeh.webserver.cargowise.com");
			var form = new GlbCompanyCampaignTest.FakeFormForTest(campaign) { UserChoiceContinueWithSending = true, MessageOnCampaignSendingArgs = null };

			var campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += form.Campaign_ShouldContinueWithSending;
			campaignSender.MessageOnCampaignSending += form.Campaign_MessageOnCampaignSending;

			campaignSender.CheckAndSendCampaigns();
			AssertEquals("Should have sent 1 email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("SenderStaff should be the campaign coordinator", staff.PK, Env.OutgoingMailManager.EmailsCreated.First().SenderStaffPK);
		}

		GlbStaff ChangePool(GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign)
		{
			campaign.SenderPool.DeleteAll();
			AssertEquals(0, campaign.SenderPool.Count);
			var glbStaffPool = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffPool.GS_GB_HomeBranch = Env.CurrentBranchPK;
			glbStaffPool.GS_EmailAddress = $"{nameof(glbStaffPool)}@ema.il";
			var campaignSenderPoolItem = campaign.SenderPool.AddNew();
			campaignSenderPoolItem.GCP_GS_NKSender = glbStaffPool.GS_Code;
			AssertEquals(1, campaign.SenderPool.Count);
			return glbStaffPool;
		}

		public void TestReSendToContactsFromSendersPoolUsingOtherEmail()
		{
			var testData = PrepareResendTestForPool();

			// clear Pool and resend (should use the new sender and clear previous)
			testData.Item1.SenderPool.DeleteAll();
			AssertEquals(0, testData.Item1.SenderPool.Count);
			testData.Item1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			testData.Item1.G0_EmailSenderName = "Name";
			testData.Item1.G0_SenderEmail = "e@ma.il";
			testData.Item1.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(testData.Item1, new[] { testData.Item3 });
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(1, testData.Item1.CampaignsItemsSent.Count);

			var companyCampaignItemResent = testData.Item1.CampaignsItemsSent[0];
			AssertEquals("Should clear previous pool sender", true, companyCampaignItemResent.G8_GS_NKSender.IsEmpty);
			AssertEquals("Should use new email", testData.Item1.G0_SenderEmail, companyCampaignItemResent.G8_SenderEmailAddress);
		}

		public void TestReSendToContactsFromSendersPoolUsingCoordinator()
		{
			var testData = PrepareResendTestForPool();

			// clear Pool and resend (should use the new sender and clear previous)
			testData.Item1.SenderPool.DeleteAll();
			AssertEquals(0, testData.Item1.SenderPool.Count);
			testData.Item1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(testData.Item1, new[] { testData.Item3 });
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(1, testData.Item1.CampaignsItemsSent.Count);

			var companyCampaignItemResent = testData.Item1.CampaignsItemsSent[0];
			AssertEquals("Should use COR", testData.Item2.GS_Code, companyCampaignItemResent.G8_GS_NKSender);
			AssertEquals("Should use COR", testData.Item2.GS_EmailAddress, companyCampaignItemResent.G8_SenderEmailAddress);
		}

		public void TestReSendToContactsFromSendersPoolUsingOtherSenderPool()
		{
			// add one more to the Pool and send one more (should use the new sender)
			var testData = ChangeSenderPoolAndSendOneMoreEmail(PrepareResendTestForPool());

			// remove the first one from the Pool and resend
			testData.Item1.SenderPool[0].Delete();
			AssertEquals(1, testData.Item1.SenderPool.Count);
			testData.Item1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(testData.Item1, new[] { testData.Item3, testData.Item4 });
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(2, testData.Item1.CampaignsItemsSent.Count);

			testData.Item1.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ForEach(itemResent =>
			{
				AssertEquals("Should use COR", testData.Item2.GS_Code, itemResent.G8_GS_NKSender);
				AssertEquals("Should use COR", testData.Item2.GS_EmailAddress, itemResent.G8_SenderEmailAddress);
			});
		}

		Tuple<GlbCompanyCampaignTest.GlbCompanyCampaignForTest, GlbStaff, GlbCompanyCampaignItem>
			PrepareResendTestForPool()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var glbStaffCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffCoordinator.GS_EmailAddress = $"{nameof(glbStaffCoordinator)}@ema.il";
			glbStaffCoordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var glbStaffPool = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffPool.GS_EmailAddress = $"{nameof(glbStaffPool)}@ema.il";
			glbStaffPool.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = $"Bob{nameof(orgContact)}";
			orgContact.OC_Email = $"{orgContact.OC_ContactName}@ema.il";
			Factory.Save();

			var glbCompanyCampaign = Helper.GetCampaignForTestWithoutErrors();
			glbCompanyCampaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			glbCompanyCampaign.G0_GS_NKCampaignCoordinator = glbStaffCoordinator.GS_Code;
			glbCompanyCampaign.G0_CampaignName = "Enlarge your sales!";
			glbCompanyCampaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;

			var campaignSenderPoolItem = glbCompanyCampaign.SenderPool.AddNew();
			campaignSenderPoolItem.GCP_GS_NKSender = glbStaffPool.GS_Code;
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { orgContact }), glbCompanyCampaign);
			var campaignSender = new GlbCompanyCampaignSender(glbCompanyCampaign,
				campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(1, glbCompanyCampaign.CampaignsItemsSent.Count);

			var companyCampaignItemSent = glbCompanyCampaign.CampaignsItemsSent[0];
			AssertEquals(glbStaffPool.GS_Code, companyCampaignItemSent.G8_GS_NKSender);
			AssertEquals(glbStaffPool.GS_EmailAddress, companyCampaignItemSent.G8_SenderEmailAddress);
			Factory.Save();
			return
				new Tuple<GlbCompanyCampaignTest.GlbCompanyCampaignForTest, GlbStaff, GlbCompanyCampaignItem>(
					glbCompanyCampaign, glbStaffCoordinator, companyCampaignItemSent);
		}

		Tuple
		<GlbCompanyCampaignTest.GlbCompanyCampaignForTest, GlbStaff, GlbCompanyCampaignItem,
			GlbCompanyCampaignItem> ChangeSenderPoolAndSendOneMoreEmail(
			Tuple<GlbCompanyCampaignTest.GlbCompanyCampaignForTest, GlbStaff, GlbCompanyCampaignItem> testData)
		{
			var glbStaffPoolNew = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffPoolNew.GS_EmailAddress = $"{nameof(glbStaffPoolNew)}@ema.il";
			glbStaffPoolNew.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var campaignSenderPoolItem = testData.Item1.SenderPool.AddNew();
			campaignSenderPoolItem.GCP_GS_NKSender = glbStaffPoolNew.GS_Code;

			AssertEquals(2, testData.Item1.SenderPool.Count);
			testData.Item1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = orgHeader.Contacts.AddNew();
			orgContact.OC_ContactName = $"Bobr{nameof(orgContact)}";
			orgContact.OC_Email = $"{orgContact.OC_ContactName}@ema.il";
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { orgContact }), testData.Item1);
			var campaignSender = new GlbCompanyCampaignSender(testData.Item1,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(2, testData.Item1.CampaignsItemsSent.Count);

			var companyCampaignItemSent =
				testData.Item1.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.OrderByDescending(item => item.G8_SystemCreateTimeUtc)
					.First();
			AssertEquals(glbStaffPoolNew.GS_Code, companyCampaignItemSent.G8_GS_NKSender);
			AssertEquals(glbStaffPoolNew.GS_EmailAddress, companyCampaignItemSent.G8_SenderEmailAddress);
			Factory.Save();

			return
				new Tuple
				<GlbCompanyCampaignTest.GlbCompanyCampaignForTest, GlbStaff, GlbCompanyCampaignItem,
					GlbCompanyCampaignItem>(testData.Item1, testData.Item2, testData.Item3, companyCampaignItemSent);
		}

		public void TestReSendToContactsUsingSendersPool()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var glbStaffCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffCoordinator.GS_EmailAddress = $"{nameof(glbStaffCoordinator)}@ema.il";
			glbStaffCoordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var glbStaffPool = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffPool.GS_EmailAddress = $"{nameof(glbStaffPool)}@ema.il";
			glbStaffPool.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact1 = orgHeader.Contacts.AddNew();
			orgContact1.OC_ContactName = $"Bob{nameof(orgContact1)}";
			orgContact1.OC_Email = $"{orgContact1.OC_ContactName}@ema.il";
			var orgContact2 = orgHeader.Contacts.AddNew();
			orgContact2.OC_ContactName = $"Bob{nameof(orgContact2)}";
			orgContact2.OC_Email = $"{orgContact2.OC_ContactName}@ema.il";

			var glbCompanyCampaign = Helper.GetCampaignForTestWithoutErrors();
			glbCompanyCampaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			glbCompanyCampaign.G0_GS_NKCampaignCoordinator = glbStaffCoordinator.GS_Code;
			glbCompanyCampaign.G0_CampaignName = "Enlarge your sales!";
			glbCompanyCampaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;

			var campaignSenderPoolItem = glbCompanyCampaign.SenderPool.AddNew();
			campaignSenderPoolItem.GCP_GS_NKSender = glbStaffPool.GS_Code;
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { orgContact1, orgContact2 }),
				glbCompanyCampaign);
			var campaignSender = new GlbCompanyCampaignSender(glbCompanyCampaign,
				campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(2, glbCompanyCampaign.CampaignsItemsSent.Count);

			var companyCampaignItemSent = glbCompanyCampaign.CampaignsItemsSent[0];
			AssertEquals("Coordinator", glbStaffCoordinator.GS_Code, companyCampaignItemSent.G8_GS_NKSender);
			AssertEquals("Coordinator Email", glbStaffCoordinator.GS_EmailAddress, companyCampaignItemSent.G8_SenderEmailAddress);

			// set Pool and resend
			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			Factory.Save();
			campaignSender = new GlbCompanyCampaignSender(glbCompanyCampaign, new[] { companyCampaignItemSent });
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(2, glbCompanyCampaign.CampaignsItemsSent.Count);

			var companyCampaignItemResent =
				glbCompanyCampaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.OrderByDescending(item => item.G8_SystemLastEditTimeUtc)
					.First();
			AssertEquals("Should be pool sender", glbStaffPool.GS_Code, companyCampaignItemResent.G8_GS_NKSender);
			AssertEquals("Should be use pool sender Email", glbStaffPool.GS_EmailAddress,
				companyCampaignItemResent.G8_SenderEmailAddress);
		}

		public void TestG8_SenderEmailAddress()
		{
			var glbStaffCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			glbStaffCoordinator.GS_EmailAddress = $"{nameof(glbStaffCoordinator)}@ema.il";
			glbStaffCoordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var glbCompanyCampaign = Helper.GetCampaignForTestWithoutErrors();
			glbCompanyCampaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			glbCompanyCampaign.G0_GS_NKCampaignCoordinator = glbStaffCoordinator.GS_Code;
			glbCompanyCampaign.G0_CampaignName = "Enlarge your sales!";
			glbCompanyCampaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgHeader.Contacts.Add(orgContact);
			Factory.Save();

			var companyCampaignItemSent = SendCampaignToContactAndGetCampaignItemSent(glbCompanyCampaign, orgContact);
			AssertEquals(glbStaffCoordinator.GS_Code, companyCampaignItemSent.G8_GS_NKSender);
			AssertEquals(glbStaffCoordinator.GS_EmailAddress, companyCampaignItemSent.G8_SenderEmailAddress);

			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			glbCompanyCampaign.G0_SenderEmail = "xwinter@gmail.com";
			glbCompanyCampaign.G0_RL_NKEmailSenderUNLOCO = "AUSYD";

			orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgHeader.Contacts.Add(orgContact);
			Factory.Save();

			companyCampaignItemSent = SendCampaignToContactAndGetCampaignItemSent(glbCompanyCampaign, orgContact, 1);
			AssertEquals(ZString.Empty, companyCampaignItemSent.G8_GS_NKSender);
			AssertEquals(glbCompanyCampaign.G0_SenderEmail, companyCampaignItemSent.G8_SenderEmailAddress);

			glbCompanyCampaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			glbCompanyCampaign.G0_RL_NKEmailSenderUNLOCO = "";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = $"{nameof(staff)}@email.zz";
			staff.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var senderPollItem = glbCompanyCampaign.SenderPool.AddNew();
			senderPollItem.GCP_GS_NKSender = staff.GS_Code;

			orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgHeader.Contacts.Add(orgContact);
			Factory.Save();

			companyCampaignItemSent = SendCampaignToContactAndGetCampaignItemSent(glbCompanyCampaign, orgContact, 2);
			AssertEquals(staff.GS_Code, companyCampaignItemSent.G8_GS_NKSender);
			AssertEquals(staff.GS_EmailAddress, companyCampaignItemSent.G8_SenderEmailAddress);
		}

		public void TestCheckAndSendCampaignsOpportunityQueued()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "thomas@test.com";
			contact.OC_ContactName = "Thomas";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Touch 1A";
			touch.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_G0_Master = master.PK;

			var item = touch.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch, Factory);
			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(touch);
			campaignSender.SendCampaigns(touch.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ToArray(), touch);
			AssertEquals(campaignSender.ContactsDeliveredCount, 1);
		}

		public void TestCheckAndSendCampaignsOpportunityQueued_WithCampaignContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = CreateContact("Bob", org);
			var contact2 = CreateContact("Tom", org);
			var contact3 = CreateContact("ZZZ", org);

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Touch 1A";
			touch.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			touch.G0_G0_Master = master.PK;

			Factory.Save();

			var campaignContactCollection =
	ContactCollection(new List<BusinessObject>() { contact1, contact2, contact3 }, master);

			var campaignSender = new GlbCompanyCampaignSender(touch, campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.SendCampaigns(null, touch);

			AssertEquals(touch.CampaignsItemsSent.Count, 3);

			foreach (GlbCompanyCampaignItem item in touch.CampaignsItemsSent)
			{
				AssertEquals($"Contact Name : {item.ContactName}, Recipient Table Code", OrgContactSchema.Constants.Prefix, item.G8_RecipientTableCode);
				AssertEquals($"Contact Name : {item.ContactName}, Tracking Status", TrackingStatusCodes.Codes.OPQ, item.G8_TrackingStatus);
			}
		}

		GlbCompanyCampaignItem SendCampaignToContactAndGetCampaignItemSent(
			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign, OrgContact contact,
			int alreadySentItemsCount = 0)
		{
			if (contact.OC_Email.IndexOf("@") < 0)
			{
				contact.OC_Email = $"{ZDateTime.UtcNow.ToString("HHmmssffffff", CultureInfo.InvariantCulture)}@ema.il";
				Factory.Save();
			}

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { contact }), campaign);
			var campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;

			AssertEquals(alreadySentItemsCount, campaign.CampaignsItemsSent.Count);
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());
			AssertEquals(alreadySentItemsCount + 1, campaign.CampaignsItemsSent.Count);

			var companyCampaignItemSent =
				campaign.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.OrderByDescending(item => item.G8_SystemCreateTimeUtc)
					.First();
			AssertNotNull(companyCampaignItemSent);
			return companyCampaignItemSent;
		}

		#region Embedded Images

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendEmailIncludesEmbeddedImages()
		{
			TestEmbeddedImage(@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business\TestDocs\small.gif", "");
		}

		void TestEmbeddedImage(string imageFileName, string expectedImageName, bool hasEmbeddedImages = true)
		{
			OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://easy.me/tracking.png");
			Env.OutgoingMailManager.EmailsCreated.Clear();

			const string templateHtml = @"
<html><head><meta charset=""utf-8""></head><body>
	<img src=""{0}""> &nbsp;
</body></html>";
			var campaign = PrepareCampaignWithEmbeddedImage(templateHtml, hasEmbeddedImages ? Path.Combine(BaseSourcePath, imageFileName) : imageFileName);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Bob";
			contact.OC_Email = "default@cargowise.com";
			Factory.Save();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(new[] { contact }), campaign);
			var campaignSender = new GlbCompanyCampaignSender(campaign,
				campaignContactCollection.Cast<CampaignContact>().ToList());
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());

			AssertEquals("Email should have been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should have HTML contents", EmailContentTypes.HTML, email.ContentType);
			var link = campaign.TrackedLinks.Find(element => element.GCL_Context == CampaignEmailTemplateEditor.TrackingImageContext).FirstOrDefault();
			AssertNotNull(link);
			AssertEquals(1, campaign.CampaignsItemsSent.Count);
			var campaignItem1 = campaign.CampaignsItemsSent[0];
			var templateWithTrackingImageHtml = FormattableString.Invariant($@"
<html><head><meta charset=""utf-8""></head><body>
	<img src=""{expectedImageName}""> &nbsp;
<img style=""HEIGHT: 1px; WIDTH: 1px"" border=""0"" hspace=""0"" width=""1"" height=""1"" alt="" "" src=""{OrganisationsDataRegistry.Instance.LinkTrackingUrl.Value}?{OrganisationsDataRegistry.Instance.LinkTrackingImageUrl.Value}&c={EnvProxy.Instance.CurrentCompany.GetLicenceCode()}&x={link.PK.ToGuid().ToString("N")}&u={campaignItem1.PK.ToGuid().ToString("N")}""></body></html>");
			AssertMultilineASCIIEquals("Text", templateWithTrackingImageHtml, email.Body);
			AssertEquals("Should not send embedded Image", 0, email.Attachments.Count);
		}

		GlbCompanyCampaign PrepareCampaign()
		{
			var homeBranch = Factory.NewWithValidTestData<GlbBranch>();
			homeBranch.GB_RL_NKHomePort = "AUSYD";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";
			staff.GS_GB_HomeBranch = homeBranch.PK;

			var campaign = Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_CampaignName = "Enlarge your sales!";
			campaign.G0_EmailSubject = "Best campaign ever, (*ContactName*)!";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			return campaign;
		}

		GlbCompanyCampaign PrepareCampaignWithEmbeddedImage(string templateHtmlText, byte[] image)
		{
			var campaign = PrepareCampaign();
			var base64String = Convert.ToBase64String(image);
			var imageContent = $"data:image/png;base64,{base64String}";

			var editor = new CampaignEmailTemplateEditor(campaign)
			{
				TemplateHtmlText = string.Format(templateHtmlText, imageContent)
			};

			editor.SyncImageTrackIDAttributeAndTrackingMacro();
			editor.SetCampaignEmailTemplate();

			Factory.Save();
			return campaign;
		}

		GlbCompanyCampaign PrepareCampaignWithEmbeddedImage(string templateHtmlText, string imageFileName)
		{
			var campaign = PrepareCampaign();

			var editor = new CampaignEmailTemplateEditor(campaign)
			{
				TemplateHtmlText = string.Format(templateHtmlText, imageFileName)
			};
			editor.SyncImageTrackIDAttributeAndTrackingMacro();
			editor.SetCampaignEmailTemplate();

			Factory.Save();
			return campaign;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendEmailIncludesEmbeddedImagesWithSpacesInName()
		{
			TestEmbeddedImage(@"NoCoverSheet\Cover Sheet - S00038411.TIF", "");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSendEmailWithInternetUrlImages()
		{
			TestEmbeddedImage(@"https://wallpaperscraft.com/image.jpg", "https://wallpaperscraft.com/image.jpg", false);
		}

		#endregion

		#region Sending Drip Marketing

		#region COR then COR

		public void TestDripSend_CorThenCor_DontUseLastEmail()
		{
			var data = TestDripSend_CorThenCor_Initial();
			TestDripSendCorThenCorDontUseLastEmail(data);
		}

		public void TestDripSend_CorThenCor_UseLastEmail()
		{
			var data = TestDripSend_CorThenCor_Initial();
			TestDripSendCorThenCorUseLastEmail(data);
		}

		public void TestDripSend_CorThenCor_UseLastEmailWithInactiveOldCoordinator()
		{
			var data = TestDripSend_CorThenCor_Initial();
			TestDripSendCorThenCorUseLastEmailWithInactiveOldCoordinator(data);
		}

		InitialDripData TestDripSend_CorThenCor_Initial()
		{
			var data = CreateDripData(Factory, 2);
			data.Touch1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			data.Touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			return data;
		}

		void TestDripSendCorThenCorDontUseLastEmail(InitialDripData data)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = false;
			Factory.Save();

			TestDripSendCorThenCorNewStaff(data);
		}

		void TestDripSendCorThenCorUseLastEmail(InitialDripData data)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the touch1 coordinator", data.Touch1.CampaignCoordinator.GS_Code, item.G8_GS_NKSender);
				AssertEquals("Sender is the touch1 coordinator", data.Touch1.CampaignCoordinator.GS_EmailAddress,
					item.G8_SenderEmailAddress);
				AssertEmailSender(data.Touch1.CampaignCoordinator, item);
			}
		}

		void TestDripSendCorThenCorUseLastEmailWithInactiveOldCoordinator(InitialDripData data)
		{
			data.Touch1.CampaignCoordinator.GS_IsActive = false;
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			TestDripSendCorThenCorNewStaff(data);
		}

		static void TestDripSendCorThenCorNewStaff(InitialDripData data)
		{
			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the new coordinator", data.Touch2.CampaignCoordinator.GS_Code, item.G8_GS_NKSender);
				AssertEquals("Sender is the new coordinator", data.Touch2.CampaignCoordinator.GS_EmailAddress,
					item.G8_SenderEmailAddress);
				AssertEmailSender(data.Touch2.CampaignCoordinator, item);
			}
		}

		#endregion

		#region COR then EML

		public void TestDripSend_CorThenEml_DontUseLastEmail()
		{
			var data = TestDripSend_CorThenEml_Initial();
			TestDripSendCorThenEmlDontUseLastEmail(data.Data, data.Email, data.Name);
		}

		public void TestDripSend_CorThenEml_UseLastEmail()
		{
			var data = TestDripSend_CorThenEml_Initial();
			TestDripSendCorThenEmlUseLastEmail(data.Data);
		}

		public void TestDripSend_CorThenEml_UseLastEmailWithInactiveOldCoordinator()
		{
			var data = TestDripSend_CorThenEml_Initial();
			TestDripSendCorThenEmlUseLastEmailWithInactiveOldCoordinator(data.Data, data.Email, data.Name);
		}

		DripSendData TestDripSend_CorThenEml_Initial()
		{
			const string newEmail = "new@ema.il";
			var data = CreateDripData(Factory, 2);

			data.Touch1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			var newCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			newCoordinator.GS_EmailAddress = $"{nameof(newCoordinator)}@test.com";
			newCoordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;
			data.Touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			data.Touch2.G0_EmailSenderName = ZGuid.NewZGuid().ToString();
			data.Touch2.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			data.Touch2.G0_SenderEmail = newEmail;
			return new DripSendData(data, newEmail, data.Touch2.G0_EmailSenderName);
		}

		class DripSendData
		{
			public DripSendData(InitialDripData data, string email, string name)
			{
				Data = data;
				Email = email;
				Name = name;
			}

			public InitialDripData Data { get; private set; }
			public string Email { get; private set; }
			public string Name { get; private set; }
		}

		void TestDripSendCorThenEmlDontUseLastEmail(InitialDripData data, string newEmail, string newName)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = false;
			Factory.Save();

			TestDripSendCorThenEmlNewStaff(data, newEmail, newName);
		}

		void TestDripSendCorThenEmlUseLastEmail(InitialDripData data)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the touch1 coordinator", data.Touch1.CampaignCoordinator.GS_Code, item.G8_GS_NKSender);
				AssertEquals("Sender is the touch1 coordinator", data.Touch1.CampaignCoordinator.GS_EmailAddress,
					item.G8_SenderEmailAddress);
				AssertEmailSender(data.Touch1.CampaignCoordinator, item);
			}
		}

		void TestDripSendCorThenEmlUseLastEmailWithInactiveOldCoordinator(InitialDripData data, string newEmail,
			string newName)
		{
			data.Touch1.CampaignCoordinator.GS_IsActive = false;
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			TestDripSendCorThenEmlNewStaff(data, newEmail, newName);
		}

		static void TestDripSendCorThenEmlNewStaff(InitialDripData data, string newEmail, string newName)
		{
			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is empty", ZString.Empty, item.G8_GS_NKSender);
				AssertEquals("Sender email is the new email", newEmail, item.G8_SenderEmailAddress);
				AssertEquals("Sender email is the new email", newName, item.G8_EmailSenderName);
			}
		}

		#endregion

		#region EML then COR

		public void TestDripSend_EmlThenCor_DontUseLastEmail()
		{
			var data = TestDripSend_EmlThenCor_Initial();
			TestDripSendEmlThenCorDontUseLastEmail(data.Item1);
		}

		public void TestDripSend_EmlThenCor_UseLastEmail()
		{
			var data = TestDripSend_EmlThenCor_Initial();
			TestDripSendEmlThenCorUseLastEmail(data.Item1, data.Item2);
		}

		public void TestDripSend_EmlThenEml_UseLastEmail()
		{
			var data = CreateDripData(Factory, 2);

			data.Touch1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			data.Touch1.G0_EmailSenderName = "Touch1 Sender";
			data.Touch1.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			data.Touch1.G0_SenderEmail = "touch1sender@wisetechglobal.com";

			var newCoordinator = Factory.NewWithValidTestData<GlbStaff>();
			newCoordinator.GS_EmailAddress = "newCoordinator@test.com";
			newCoordinator.GS_GB_HomeBranch = Env.CurrentBranchPK;
			data.Touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			data.Touch2.G0_EmailSenderName = "Touch2 Sender";
			data.Touch2.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			data.Touch2.G0_SenderEmail = "touch2sender@wisetechglobal.com";

			data.Touch1.G0_UseLastEmailSenderAddress = false;
			data.Touch2.G0_UseLastEmailSenderAddress = true;

			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(data.Touch1, data.Items);
			campaignSender.ShouldContinueWithSending += (n, c) => true;
			AssertEquals("sent", true, campaignSender.CheckAndSendCampaigns());

			foreach (var item in data.Touch1.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the touch1 sender", "Touch1 Sender", item.G8_EmailSenderName);
				AssertEquals("Sender is the touch1 sender", "touch1sender@wisetechglobal.com", item.G8_SenderEmailAddress);
			}

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the touch1 sender", "Touch1 Sender", item.G8_EmailSenderName);
				AssertEquals("Sender is the touch1 sender", "touch1sender@wisetechglobal.com", item.G8_SenderEmailAddress);
			}
		}

		Tuple<InitialDripData, string> TestDripSend_EmlThenCor_Initial()
		{
			const string newEmail = "new@ema.il";
			var data = CreateDripData(Factory, 2);

			data.Touch1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			data.Touch1.G0_EmailSenderName = "Sender Name";
			data.Touch1.G0_RL_NKEmailSenderUNLOCO = "AUSYD";
			data.Touch1.G0_SenderEmail = newEmail;
			data.Touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			Factory.Save();

			var sender = new GlbCompanyCampaignSender(data.Touch1, data.Touch1.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>());
			sender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", sender.CheckAndSendCampaigns());
			AssertEquals(2, data.Touch1.CampaignsItemsSent.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();

			return new Tuple<InitialDripData, string>(data, newEmail);
		}

		void TestDripSendEmlThenCorDontUseLastEmail(InitialDripData data)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = false;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the touch2 coordinator", data.Touch2.CampaignCoordinator.GS_Code, item.G8_GS_NKSender);
				AssertEquals("Sender is the touch2 coordinator", data.Touch2.CampaignCoordinator.GS_EmailAddress,
					item.G8_SenderEmailAddress);
				AssertEmailSender(data.Touch2.CampaignCoordinator, item);
			}
		}

		void TestDripSendEmlThenCorUseLastEmail(InitialDripData data, string newEmail)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is empty", ZString.Empty, item.G8_GS_NKSender);
				AssertEquals("Sender email is the new email", newEmail, item.G8_SenderEmailAddress);
			}
		}

		#endregion

		#region SPS then SPS

		public void TestDripSend_SpsThenSps_DontUseLastEmail()
		{
			var data = TestDripSend_SpsThenSps_Initial();
			TestDripSendSpsThenSpsDontUseLastEmail(data);
		}

		public void TestDripSend_SpsThenSps_UseLastEmail()
		{
			var data = TestDripSend_SpsThenSps_Initial();
			TestDripSendSpsThenSpsUseLastEmail(data);
		}

		public void TestDripSend_SpsThenSps_UseLastEmailWithInactiveOldCoordinator()
		{
			var data = TestDripSend_SpsThenSps_Initial();
			TestDripSendSpsThenSpsUseLastEmailWithInactiveOldCoordinator(data);
		}

		public void TestDripSend_SpsThenSps_DontUseLastEmailWithInactiveOldCoordinator()
		{
			var data = TestDripSend_SpsThenSps_Initial();
			TestDripSendSpsThenSpsDontUseLastEmailWithAlmostTheSamePool(data);
		}

		InitialDripData TestDripSend_SpsThenSps_Initial()
		{
			var data = CreateDripData(Factory, 6);

			data.Touch1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			CreateSenderPool(data.Touch1, 3);
			Factory.Save();

			var sender = new GlbCompanyCampaignSender(data.Touch1, data.Touch1.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>());
			sender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", sender.CheckAndSendCampaigns());
			AssertEquals(6, data.Touch1.CampaignsItemsSent.Count);
			AssertEquals(6, Env.OutgoingMailManager.EmailsCreated.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Factory.Save();

			data.Touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			CreateSenderPool(data.Touch2, 2);
			return data;
		}

		void TestDripSendSpsThenSpsDontUseLastEmail(InitialDripData data)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = false;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 6 should be sent", 6, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			var sortedPool = data.Touch2.SenderPool.OrderBy(item => item.GCP_GS_NKSender).ToArray();
			var grouppedSend =
				data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.GroupBy(item => item.G8_GS_NKSender)
					.OrderBy(items => items.Key)
					.ToArray();

			AssertGroupedSentResults(sortedPool, grouppedSend);
		}

		void TestDripSendSpsThenSpsUseLastEmail(InitialDripData data)
		{
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 6 should be sent", 6, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			var sortedPool = data.Touch1.SenderPool.OrderBy(item => item.GCP_GS_NKSender).ToArray();
			var grouppedSend =
				data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.GroupBy(item => item.G8_GS_NKSender)
					.OrderBy(items => items.Key)
					.ToArray();

			AssertGroupedSentResults(sortedPool, grouppedSend);
		}

		void TestDripSendSpsThenSpsUseLastEmailWithInactiveOldCoordinator(InitialDripData data)
		{
			var sortedPool = data.Touch1.SenderPool.OrderBy(item => item.GCP_GS_NKSender).ToArray();
			sortedPool[0].Sender.GS_IsActive = false;
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 6 should be sent", 6, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			var grouppedSend =
				data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.GroupBy(item => item.G8_GS_NKSender)
					.OrderBy(items => items.Key)
					.ToArray();

			AssertEquals("Disabled staff disappears from senders", 0,
				grouppedSend.Count(group => group.Key == sortedPool[0].GCP_GS_NKSender));
			AssertEquals("Senders count", sortedPool.Length - 1 + data.Touch2.SenderPool.Count, grouppedSend.Length);

			var oldActiveSortedPool = sortedPool.Where((item, i) => i > 0).OrderBy(item => item.GCP_GS_NKSender).ToArray();
			var itemsSentByOldActiveSenderPool =
				grouppedSend.Where(items => oldActiveSortedPool.Select(item => item.GCP_GS_NKSender).Contains(items.Key))
					.OrderBy(items => items.Key)
					.ToArray();
			AssertGroupedSentResults(oldActiveSortedPool, itemsSentByOldActiveSenderPool);

			var newSortedPool = data.Touch2.SenderPool.OrderBy(item => item.GCP_GS_NKSender).ToArray();
			var itemsSentByNewSenderPool =
				grouppedSend.Where(items => newSortedPool.Select(item => item.GCP_GS_NKSender).Contains(items.Key))
					.OrderBy(items => items.Key)
					.ToArray();
			AssertGroupedSentResults(newSortedPool, itemsSentByNewSenderPool);
		}

		void TestDripSendSpsThenSpsDontUseLastEmailWithAlmostTheSamePool(InitialDripData data)
		{
			var sortedPool = data.Touch1.SenderPool.OrderBy(item => item.GCP_GS_NKSender).ToArray();
			data.Touch2.G0_UseLastEmailSenderAddress = false;
			data.Touch2.SenderPool.DeleteAll();
			CreateSenderPool(data.Touch2, 3);
			data.Touch2.SenderPool[0].GCP_GS_NKSender = sortedPool[0].GCP_GS_NKSender;
			data.Touch2.SenderPool[1].GCP_GS_NKSender = sortedPool[1].GCP_GS_NKSender;
			Factory.Save();

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 6 should be sent", 6, data.Touch2.CampaignsItemsSent.Count);
			AssertSentEmails(data.Touch2.CampaignsItemsSent);

			var grouppedSend =
				data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>()
					.GroupBy(item => item.G8_GS_NKSender)
					.OrderBy(items => items.Key)
					.ToArray();

			AssertEquals("We need 3 senders in the first pool", 3, sortedPool.Length);
			AssertEquals("Not used pool staff disappears from senders (2nd from old pool)", 0,
				grouppedSend.Count(group => group.Key == sortedPool[2].GCP_GS_NKSender));
			AssertEquals("Total senders count (2 old, 1 new)", 3, grouppedSend.Length);

			var usedItemsFromOldPoolSorted =
				data.Touch2.SenderPool.Where(
						item =>
							item.GCP_GS_NKSender == sortedPool[0].GCP_GS_NKSender || item.GCP_GS_NKSender == sortedPool[1].GCP_GS_NKSender)
					.OrderBy(item => item.GCP_GS_NKSender)
					.ToArray();
			var itemsSentByOldSenderPool =
				grouppedSend.Where(group => usedItemsFromOldPoolSorted.Select(item => item.GCP_GS_NKSender).Contains(group.Key))
					.OrderBy(items => items.Key)
					.ToArray();
			AssertGroupedSentResults(usedItemsFromOldPoolSorted, itemsSentByOldSenderPool);

			var newSortedPool = data.Touch2.SenderPool.OrderBy(item => item.GCP_GS_NKSender).ToArray();
			var itemsSentByNewSenderPool =
				grouppedSend.Where(items => newSortedPool.Select(item => item.GCP_GS_NKSender).Contains(items.Key))
					.OrderBy(items => items.Key)
					.ToArray();
			AssertGroupedSentResults(newSortedPool, itemsSentByNewSenderPool);
		}

		static void AssertGroupedSentResults(IReadOnlyList<GlbCompanyCampaignSenderPoolItem> sortedPool,
			IReadOnlyList<IGrouping<ZString, GlbCompanyCampaignItem>> grouppedSend)
		{
			AssertEquals("Senders count", sortedPool.Count, grouppedSend.Count);
			for (var groupIndex = 0; groupIndex < grouppedSend.Count; groupIndex++)
			{
				var group = grouppedSend[groupIndex];
				var poolItem = sortedPool[groupIndex];

				AssertEquals("Sender is the pool", poolItem.GCP_GS_NKSender, group.Key);
				foreach (var item in group)
				{
					AssertEquals("Sender is the pool", poolItem.GCP_GS_NKSender, item.G8_GS_NKSender);
					AssertEquals("Sender is the pool", poolItem.Sender.GS_EmailAddress, item.G8_SenderEmailAddress);
					AssertEmailSender(poolItem.Sender, item);
				}
			}
		}

		#endregion

		internal static InitialDripData CreateTouchCampaignWithoutItems(BusinessObjectFactory factory, int contactsCount)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var staffCor1 = factory.NewWithValidTestData<GlbStaff>();
			staffCor1.GS_EmailAddress = $"{nameof(staffCor1)}@test.com";
			staffCor1.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var staffCor2 = factory.NewWithValidTestData<GlbStaff>();
			staffCor2.GS_EmailAddress = $"{nameof(staffCor2)}@test.com";
			staffCor2.GS_GB_HomeBranch = Env.CurrentBranchPK;

			var master = factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";

			var touch1 = factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1.G0_CampaignName = "touch1";
			touch1.G0_Type = "PREAP";
			touch1.G0_Category = "PRINT";
			touch1.HtmlDocumentBlob = ZBlob.FromAscii("gday gday");
			touch1.G0_EstimatedStartedDate = ZDateTime.Now;
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "a";
			touch1.G0_G0_Master = master.PK;
			touch1.G0_GS_NKCampaignCoordinator = staffCor1.GS_Code;
			touch1.G0_GS_NKCampaignManager = staffCor1.GS_Code;
			touch1.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch1.SourceCampaignPK = master.PK;
			master.AllTouches.Add(touch1);

			var touch2 = factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2.G0_CampaignName = "touch2a";
			touch2.G0_Type = "PREAP";
			touch2.G0_Category = "PRINT";
			touch2.HtmlDocumentBlob = ZBlob.FromAscii("click me");
			touch2.G0_EstimatedStartedDate = ZDateTime.Now;
			touch2.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
			touch2.G0_HorizontalId = 2;
			touch2.G0_VerticalId = "a";
			touch2.G0_G0_Master = master.PK;
			touch2.G0_GS_NKCampaignCoordinator = staffCor2.GS_Code;
			touch2.G0_GS_NKCampaignManager = staffCor2.GS_Code;
			touch2.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch2.SourceCampaignPK = touch1.PK;
			master.AllTouches.Add(touch2);

			var rule = touch2.TransitionRulesToThisCampaign.AddNew();
			rule.GCD_G0_ParentTouch = touch1.PK;

			var contacts = Enumerable.Range(1, contactsCount).Select(i =>
			{
				var contact = factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = $"{nameof(contact)}{i}@test.com";
				return contact;
			}).ToArray();

			return new InitialDripData(master, touch1, touch2, contacts, null);
		}

		internal static InitialDripData CreateDripData(BusinessObjectFactory factory, int contactsCount)
		{
			var data = CreateTouchCampaignWithoutItems(factory, contactsCount);
			var touch1 = data.Touch1;

			var items = data.Contacts.Select(contact =>
			{
				var item = factory.New<GlbCompanyCampaignItem>();
				item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
				item.G8_RecipientID = contact.PK;
				item.G8_G0 = touch1.PK;
				item.G8_GS_NKSender = touch1.CampaignCoordinator.GS_Code;
				item.G8_SenderEmailAddress = touch1.CampaignCoordinator.GS_EmailAddress;
				return item;
			}).ToArray();

			factory.Save();

			touch1.CampaignsItemsSent.Reload(true);

			return new InitialDripData(data.Master, touch1, data.Touch2, data.Contacts, items);
		}

		public GlbCompanyCampaignItem CreateCampaignTestData(bool permanentlySaveAgainstRecipient = false)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "default@cargowise.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TESTORG";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Bob";
			contact1.OC_Email = "default@cargowise.com";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_CampaignName = "Enlarge your sales!";
			campaign.G0_EmailSubject = "(*CampaignName*) Best campaign ever, (*ContactName*)!";
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign.G0_EmailSenderName = "Smith, Support";
			campaign.G0_SenderEmail = "xrm@cargowise.com";
			campaign.UseEmailSenderAddressAsReplyTo = false;
			campaign.G0_RL_NKEmailSenderUNLOCO = "ABCDE";
			campaign.G0_ReplyToEmail = "reply@gmail.com";
			campaign.G0_StoreEmailInEDocs = permanentlySaveAgainstRecipient;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact1.PK;

			return campaignItem;
		}

		static void CreateSenderPool(GlbCompanyCampaign campaign, int poolSize)
		{
			var glbStaffPool = Enumerable.Range(1, poolSize)
				.Select(i =>
				{
					var glbStaff = campaign.Factory.NewWithValidTestData<GlbStaff>();
					glbStaff.GS_EmailAddress = $"Staff{i}@ema.il";
					glbStaff.GS_GB_HomeBranch = Env.CurrentBranchPK;
					return glbStaff;
				})
				.ToArray();

			glbStaffPool.ForEach(staff =>
			{
				var campaignSenderPoolItem = campaign.SenderPool.AddNew();
				campaignSenderPoolItem.GCP_GS_NKSender = staff.GS_Code;
			});
		}

		static void AssertSentEmails(GlbCompanyCampaignItemCampaignDependentCollection itemsCollection)
		{
			AssertEquals("Should not be any emails!", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			foreach (var item in itemsCollection.Cast<GlbCompanyCampaignItem>())
			{
				var campaignSender = new GlbCompanyCampaignSender(item.CompanyCampaign);
				AssertEquals("Should be emailed correctly", true, campaignSender.SendScheduledEmailToContact(item));
			}
			AssertEquals($"All {itemsCollection.Count} should be sent", itemsCollection.Count,
				Env.OutgoingMailManager.EmailsCreated.Count);
		}

		static void AssertEmailSender(GlbStaff expectedStaff, GlbCompanyCampaignItem item)
		{
			var email = Env.OutgoingMailManager.EmailsCreated.Single(def => def.Recipients[0].Email == item.Recipient.Email);
			AssertEquals("Reply to", expectedStaff.GS_EmailAddress, email.ReplyTo);
			AssertEquals("From", expectedStaff.GS_EmailAddress, email.FromAddress);
			AssertEquals("From display name", expectedStaff.GS_FullName, email.FromDisplayName);
		}

		internal class InitialDripData
		{
			public GlbCompanyCampaign Master { get; }
			public GlbCompanyCampaign Touch1 { get; }
			public GlbCompanyCampaign Touch2 { get; }
			public IEnumerable<OrgContact> Contacts { get; }
			public IReadOnlyList<GlbCompanyCampaignItem> Items { get; }

			public InitialDripData(GlbCompanyCampaign master, GlbCompanyCampaign touch1, GlbCompanyCampaign touch2,
				IEnumerable<OrgContact> contacts, GlbCompanyCampaignItem[] items)
			{
				Master = master;
				Touch1 = touch1;
				Touch2 = touch2;
				Contacts = contacts;
				Items = items;
			}
		}

		#endregion

		public void TestCheckAndSendCampaigns_MalformedEmail()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "hohoho@christmas.com";
			contact1.OC_ContactName = "AA";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_EmailAddress = "coord@test.com";
			coordinator.GS_GB_HomeBranch = branch.PK;

			GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign = Helper.GetCampaignForTestWithoutErrors();
			campaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah (*MalformedMacro");
			campaign.G0_EmailSubject = "(*MalformedMacro";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TEST CONTACT";
			contact.OC_Email = "unit.test@cw1.com";

			Helper.Factory.Save();

			var campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign).Cast<CampaignContact>();

			Factory.Save();

			GlbCompanyCampaignTest.FakeFormForTest form = new GlbCompanyCampaignTest.FakeFormForTest(campaign);
			form.UserChoiceContinueWithSending = true;

			GlbCompanyCampaignSender campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection, true);

			campaignSender.MessageOnCampaignSending += new CampaignSendingMessageEventHandler(form.Campaign_MessageOnCampaignSending);
			campaignSender.ShouldContinueWithSending += new CheckContinueWithSendingHandler(form.Campaign_ShouldContinueWithSending);
			campaignSender.SendScheduleEvent += (o, e) => campaignSender.CheckAndSendCampaigns(true);

			var isSent = campaignSender.CheckAndSendCampaigns(false);

			Assert("Campaign should not be sent", !isSent);
			AssertCorrectMessageIsDisplayed(form, $@"{NotificationConstants.CorrectAllErrorsMessage}
- {GlbCompanyCampaignValidation.EmailContentMacrosMalformedErrorText}
- {GlbCompanyCampaignValidation.EmailSubjectMacrosMalformedErrorText}",
			NotificationConstants.CannotSendCampaignsSummary, true);
		}

		public void TestCheckAndSendCampaigns_TouchCampaignsAndAllErrorsDisplayed()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "hohoho@christmas.com";
			contact.OC_ContactName = "AA";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_EmailAddress = "coord@test.com";
			coordinator.GS_GB_HomeBranch = branch.PK;

			var master = Helper.GetCampaignForTestWithoutErrors();
			master.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";
			master.G0_CampaignID = "MST00001000";

			var touch1 = Helper.GetCampaignForTestWithoutErrors();
			touch1.G0_CampaignName = "Test Campaign 1 With Malformed Macro";
			touch1.G0_EmailSubject = "(*MalformedMacro";
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			touch1.SourceCampaignPK = master.PK;
			master.AllTouches.Add(touch1);

			var touch2 = Helper.GetCampaignForTestWithoutErrors();
			touch2.G0_CampaignName = "Test Campaign 2 With Malformed Macro and Media Type Error";
			touch2.G0_EmailSubject = "(*MalformedMacro";
			touch2.G0_HorizontalId = 2;
			touch2.G0_Type = "MMMMM";
			touch2.G0_VerticalId = "B";
			touch2.G0_G0_Master = master.PK;
			touch2.SourceCampaignPK = master.PK;
			master.AllTouches.Add(touch2);

			var campaign1 = Helper.GetCampaignForTestWithoutErrors();
			campaign1.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign1.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			campaign1.G0_CampaignName = "Campaign";
			campaign1.G0_Type = "BBBBB";
			campaign1.G0_Category = "NNNN";

			Helper.Factory.Save();

			touch1.Validation.ValidateAll();
			AssertEquals(1, touch1.NotificationsIncludingChildren.Count());
			AssertHasError(touch1.G0_EmailSubjectInfo, "The macro(s) in the email subject are malformed. A start tag: '(*' should always be followed by an end tag: '*)'.");

			touch2.Validation.ValidateAll();
			AssertEquals(2, touch2.NotificationsIncludingChildren.Count());
			AssertHasError(touch2.G0_TypeInfo, "Enter a valid Media Type.");
			AssertHasError(touch2.G0_EmailSubjectInfo, "The macro(s) in the email subject are malformed. A start tag: '(*' should always be followed by an end tag: '*)'.");

			campaign1.Validation.ValidateAll();
			AssertEquals(2, campaign1.NotificationsIncludingChildren.Count());
			AssertHasError(campaign1.G0_TypeInfo, "Enter a valid Media Type.");
			AssertHasError(campaign1.G0_CategoryInfo, "Enter a valid Media Category.");

			var campaignContactCollection1 = ContactCollection(new List<BusinessObject>() { contact }, touch1).Cast<CampaignContact>();
			var campaignContactCollection2 = ContactCollection(new List<BusinessObject>() { contact }, touch2).Cast<CampaignContact>();
			var campaignContactCollection3 = ContactCollection(new List<BusinessObject>() { contact }, campaign1).Cast<CampaignContact>();

			var form1 = CreateSenderAndSend(touch1, campaignContactCollection1);
			var form2 = CreateSenderAndSend(touch2, campaignContactCollection2);
			var form3 = CreateSenderAndSend(campaign1, campaignContactCollection3);

			var macroMalformedErrorWithMasterCampaignID = $@"{NotificationConstants.CorrectAllErrorsMessage}
- {GlbCompanyCampaignValidation.EmailSubjectMacrosMalformedErrorText}
This touch campaign can be accessed through its master campaign with ID {master.G0_CampaignID}.";

			var macroMalformedAndMediaTypeErrorWithMasterCampaignID = $@"{NotificationConstants.CorrectAllErrorsMessage}
- {GlbCompanyCampaignValidation.EmailSubjectMacrosMalformedErrorText}
- Enter a valid Media Type.
This touch campaign can be accessed through its master campaign with ID {master.G0_CampaignID}.";

			var mediaAndCategoryTypeErrorWithoutIDMessage = $@"{NotificationConstants.CorrectAllErrorsMessage}
- Enter a valid Media Category.
- Enter a valid Media Type.";

			AssertCorrectMessageIsDisplayed(form1, macroMalformedErrorWithMasterCampaignID, NotificationConstants.CannotSendCampaignsSummary, true);
			AssertCorrectMessageIsDisplayed(form2, macroMalformedAndMediaTypeErrorWithMasterCampaignID, NotificationConstants.CannotSendCampaignsSummary, true);
			AssertCorrectMessageIsDisplayed(form3, mediaAndCategoryTypeErrorWithoutIDMessage, NotificationConstants.CannotSendCampaignsSummary, true);
		}

		[TestDate(2021, 9, 7)]
		public void TestCheckAndSendCampaigns_OnlyUniqueErrorMessagesDisplayed()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "hohoho@christmas.com";
			contact.OC_ContactName = "AA";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_EmailAddress = "coord@test.com";
			coordinator.GS_GB_HomeBranch = branch.PK;

			var master = Helper.GetCampaignForTestWithoutErrors();
			master.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "master";
			master.G0_CampaignID = "MST00001000";

			var touch1 = Helper.GetCampaignForTestWithoutErrors();
			touch1.G0_CampaignName = "Test Campaign 1 With No Source Campaign";
			touch1.G0_HorizontalId = 1;
			touch1.G0_VerticalId = "A";
			touch1.G0_G0_Master = master.PK;
			touch1.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.BAT;
			touch1.SendSettings.GSC_ContactLimitEachBatch = 0;
			touch1.G0_CampaignID = "TST00001000";
			master.AllTouches.Add(touch1);

			touch1.Validation.ValidateAll();
			AssertEquals(6, touch1.NotificationsIncludingChildren.Count());
			AssertHasRowError(touch1, "Enter a valid Source Campaigns.");
			AssertHasError(touch1.TouchSourceCampaignPKsForValidationInfo, "Enter a valid Source Campaigns.");

			AssertHasError(touch1.SendSettings.GSC_ContactLimitEachBatchInfo, "Please set at least one batch limit or change your Schedule Type.");
			AssertHasError(touch1.SendSettings.GSC_ContactLimitPerOrganizationEachBatchInfo, "Please set at least one batch limit or change your Schedule Type.");
			AssertHasError(touch1.SendSettings.GSC_ContactLimitPerOrganizationInHorizontalInfo, "Please set at least one batch limit or change your Schedule Type.");
			AssertHasError(touch1.SendSettings.GSC_ContactLimitPerOrganizationInTouchInfo, "Please set at least one batch limit or change your Schedule Type.");

			Helper.Factory.Save();

			var campaignContactCollection1 = ContactCollection(new List<BusinessObject>() { contact }, touch1).Cast<CampaignContact>();

			var form1 = CreateSenderAndSend(touch1, campaignContactCollection1);

			AssertCorrectMessageIsDisplayed(form1, $@"{NotificationConstants.CorrectAllErrorsMessage}
- Campaign (TST00001000): Enter a valid Source Campaigns.
- Please set at least one batch limit or change your Schedule Type.
- Enter a valid Source Campaigns.
This touch campaign can be accessed through its master campaign with ID {master.G0_CampaignID}.", NotificationConstants.CannotSendCampaignsSummary, true);
		}

		GlbCompanyCampaignTest.FakeFormForTest CreateSenderAndSend(GlbCompanyCampaignTest.GlbCompanyCampaignForTest campaign, IEnumerable<CampaignContact> campaignContactCollection)
		{
			var form = new GlbCompanyCampaignTest.FakeFormForTest(campaign) { UserChoiceContinueWithSending = true };

			var campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection, true);

			campaignSender.MessageOnCampaignSending += form.Campaign_MessageOnCampaignSending;
			campaignSender.ShouldContinueWithSending += form.Campaign_ShouldContinueWithSending;
			campaignSender.SendScheduleEvent += (o, e) => campaignSender.CheckAndSendCampaigns(true);

			var isSent = campaignSender.CheckAndSendCampaigns(false);

			Assert(!isSent);

			return form;
		}

		public void TestSendScheduledEmailToContactDoesNotValidateQueuedCampaignAgain()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("***", "Campaign Stage");
			OrganisationsDataRegistry.Instance.CampaignStageList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "hohoho@christmas.com";
			contact.OC_ContactName = "AA";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";

			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_EmailAddress = "coord@test.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(campaign, Factory);
			campaign.G0_CampaignName = "Test Campaign 1";
			campaign.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			campaign.G0_Stage = "***";

			var senderPoolItem = campaign.SenderPool.AddNew();
			senderPoolItem.GCP_GS_NKSender = staff.GS_Code;

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_GS_NKSender = staff.GS_Code;

			Factory.Save();

			list.Clear();
			list.AddPair("AAA", "Campaign Stage");
			OrganisationsDataRegistry.Instance.CampaignStageList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var campaignSender = new GlbCompanyCampaignSender(campaignItem.CompanyCampaign);
			var isSent = campaignSender.SendScheduledEmailToContact(campaignItem);

			Assert("Campaign should be sent without validating again if it is already queued.", isSent);
		}

		public void TestSendScheduledEmailToContact_InactiveSender()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "hohoho@christmas.com";
			contact1.OC_ContactName = "AA";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";

			var coordinator = Factory.NewWithValidTestData<GlbStaff>();
			coordinator.GS_EmailAddress = "coord@test.com";

			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(campaign1, Factory);
			campaign1.G0_CampaignName = "Test Campaign 1";
			campaign1.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			var senderPoolItem = campaign1.SenderPool.AddNew();
			senderPoolItem.GCP_GS_NKSender = staff.GS_Code;

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(campaign2, Factory);
			campaign2.G0_CampaignName = "Test Campaign 2";
			campaign2.G0_GS_NKCampaignCoordinator = coordinator.GS_Code;
			campaign2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.EML;
			campaign2.G0_SenderEmail = "coord@test.com";
			campaign2.G0_RL_NKEmailSenderUNLOCO = "AUSYD";

			var campaignItem1 = campaign1.CampaignsItemsSent.AddNew();
			campaignItem1.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_GS_NKSender = staff.GS_Code;

			var campaignItem2 = campaign2.CampaignsItemsSent.AddNew();
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact1.PK;
			campaignItem2.G8_EmailSenderName = "Sender 2";
			campaignItem2.G8_SenderEmailAddress = "sender2@test.org";

			Factory.Save();

			staff.GS_IsActive = false;
			Factory.Save();

			var campaignSender = new GlbCompanyCampaignSender(campaignItem1.CompanyCampaign);
			var isSent = campaignSender.SendScheduledEmailToContact(campaignItem1);

			var campaignSender2 = new GlbCompanyCampaignSender(campaignItem2.CompanyCampaign);
			var isSent2 = campaignSender2.SendScheduledEmailToContact(campaignItem2);

			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var itemReloaded = factory2.Load<GlbCompanyCampaignItem>(campaignItem1.PK);
			var itemReloaded2 = factory2.Load<GlbCompanyCampaignItem>(campaignItem2.PK);

			AssertEquals(true, isSent);
			AssertEquals(true, isSent2);

			AssertEquals(coordinator.GS_Code, itemReloaded.G8_GS_NKSender);
			AssertEquals(coordinator.GS_EmailAddress, itemReloaded.G8_SenderEmailAddress);

			AssertEquals("", itemReloaded2.G8_GS_NKSender);
			AssertEquals("Sender 2", itemReloaded2.G8_EmailSenderName);
			AssertEquals("sender2@test.org", itemReloaded2.G8_SenderEmailAddress);
		}

		public void TestSendValidCampaigns_OpportunityCreation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test1";
			contact1.OC_Email = "test@test.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test2";
			contact2.OC_Email = "test2@test.com";
			Factory.Save();

			var campaign = GetCampaignForSendValidCampaignsTest();
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			SendValidCampaigns(campaign, new List<BusinessObject> { contact1, contact2 });

			var campaignItems = campaign.CampaignsItemsSent.OfType<GlbCompanyCampaignItem>().ToList();
			AssertEquals(2, campaignItems.Count);
			AssertEquals(TrackingStatusCodes.Codes.OPQ, campaignItems[0].G8_TrackingStatus);
			AssertEquals(TrackingStatusCodes.Codes.OPQ, campaignItems[1].G8_TrackingStatus);

			var campaignItemsContactsPkList = campaignItems.Select(c => c.G8_RecipientID).ToList();
			AssertContainsExactElementsInAnyOrder(new[] { contact1.PK, contact2.PK }, campaignItemsContactsPkList);
		}

		public void TestScheduleOpportunityCreationCampaign()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Test1";
			contact1.OC_Email = "test@test.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Test2";
			contact2.OC_Email = "test2@test.com";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			GlbCompanyCampaignTestHelper.PopulateCampaign(campaign, Factory);
			campaign.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
			campaign.G0_G0_Master = master.PK;
			campaign.SourceCampaignPK = master.PK;
			campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;

			var oppCreationTemplate = campaign.OpportunityCreationTemplate;
			oppCreationTemplate.PackageType = "STD";
			oppCreationTemplate.OpportunityType = "UDF";
			oppCreationTemplate.OpportunityDescription = "Test";
			oppCreationTemplate.OpportunityStatus = "CRT";
			oppCreationTemplate.OpportunityStage = "UDF";
			oppCreationTemplate.Source = "WEB";
			oppCreationTemplate.ActiveSourceDetails = "NOT";
			oppCreationTemplate.OpportunityNotes = ZBlob.FromUTF8("Test Notes");
			oppCreationTemplate.SalesPerson = "TS1";
			oppCreationTemplate.OpportunityAssignment = OpportunityAssignmentList.Codes.IndividualSalesPerson;

			Factory.Save();

			ContactsToSendToEventArgs eventArgs = null;
			var campaignContactCollection = ContactCollection(new List<BusinessObject> { contact1, contact2 }, campaign).Cast<CampaignContact>();
			var campaignSender = new GlbCompanyCampaignSender(campaign, campaignContactCollection, true);
			campaignSender.ShouldContinueWithSending += (n, i) => true;
			campaignSender.SendScheduleEvent += (o, e) => eventArgs = e;

			campaignSender.CheckAndSendCampaigns();

			Assert("Precondition: SendScheduleEvent was not raised", eventArgs != null);
			AssertNotNull(nameof(eventArgs.ContactsToSendTo), eventArgs.ContactsToSendTo);
			AssertEquals("ContactsToSendTo should include campaign contacts", 2, eventArgs.ContactsToSendTo.Count);

			campaignSender.CheckAndSendCampaigns(true);

			var campaignItems = campaign.CampaignsItemsSent.OfType<GlbCompanyCampaignItem>().ToList();
			AssertEquals("CampaignsItemsSent should include campaign contacts", 2, campaignItems.Count);
			AssertEquals(TrackingStatusCodes.Codes.OPQ, campaignItems[0].G8_TrackingStatus);
			AssertEquals(TrackingStatusCodes.Codes.OPQ, campaignItems[1].G8_TrackingStatus);
		}

		public void TestRecalculateAllScheduledTimes_EmptyOwnerCampaign()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;

			var group = Factory.NewWithValidTestData<GlbCompanyCampaignGroup>();
			var touch = master.AllTouches.AddNew();
			touch.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch.G0_CampaignName = "Touch 1A";
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = master.PK;
			touch.G0_GCG_Group = group.PK;

			var touch2 = master.AllTouches.AddNew();
			touch2.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			touch2.G0_GS_NKCampaignCoordinator = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			touch2.G0_CampaignName = "Touch 1B";
			touch2.G0_HorizontalId = 1;
			touch2.G0_VerticalId = "B";
			touch2.G0_G0_Master = master.PK;
			touch2.G0_GCG_Group = group.PK;

			var campaignItem = touch.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem.G8_ScheduleTimeUtc = ZDateTime.Now;

			var campaignItem2 = touch2.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = Factory.NewWithValidTestData<OrgContact>().PK;
			campaignItem2.G8_TrackingStatus = TrackingStatusCodes.Codes.QUE;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.Now;
			Factory.Save();

			var sendSettings = touch.SendSettings;
			sendSettings.IsDelayed = true;
			sendSettings.GSC_G0_Campaign = ZGuid.Empty;
			sendSettings.GSC_GCG_Group = group.PK;
			sendSettings.RecalculateAllScheduledTimes();

			Assert(campaignItem.G8_GS_NKSender.IsEmpty);

			var sendSettings2 = touch2.SendSettings;
			sendSettings2.IsDelayed = true;
			sendSettings2.GSC_G0_Campaign = ZGuid.Empty;
			sendSettings2.GSC_GCG_Group = group.PK;
			sendSettings2.RecalculateAllScheduledTimes();

			Assert(!campaignItem2.G8_GS_NKSender.IsEmpty);
		}

		#region Sending Pivot Campaign

		public void TestPivotCampaignSend_DontUseLastEmail()
		{
			PivotCampaignSend(false, true, (c1, c2) => c2);
		}

		public void TestPivotCampaignSend_UseLastEmail()
		{
			PivotCampaignSend(true, true, (c1, c2) => c1);
		}

		public void TestPivotCampaignSend_UseLastEmailWithInactiveOldCoordinator()
		{
			PivotCampaignSend(true, false, (c1, c2) => c2);
		}

		void PivotCampaignSend(bool useLastEmailSenderAddress, bool parentCampaignSenderIsActive,
			Func<GlbCompanyCampaign, GlbCompanyCampaign, GlbCompanyCampaign> expectedSender)
		{
			var campaign1 = Helper.GetCampaignForTestWithoutErrors();
			campaign1.G0_CampaignName = nameof(campaign1);
			var contacts = Enumerable.Range(1, 5).Select(i =>
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_Email = $"{nameof(contact)}{i}@test.com";
				return contact;
			}).ToArray();

			foreach (var contact in contacts)
			{
				var item = Factory.New<GlbCompanyCampaignItem>();
				item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
				item.G8_RecipientID = contact.PK;
				item.G8_G0 = campaign1.PK;
				item.G8_GS_NKSender = campaign1.CampaignCoordinator.GS_Code;
				item.G8_SenderEmailAddress = campaign1.CampaignCoordinator.GS_EmailAddress;
			}

			var campaign2 = Helper.GetCampaignForTestWithoutErrors();
			campaign2.G0_CampaignName = nameof(campaign2);
			campaign2.SourceCampaignPK = campaign1.PK;
			campaign2.G0_UseLastEmailSenderAddress = useLastEmailSenderAddress;
			campaign1.CampaignCoordinator.GS_IsActive = parentCampaignSenderIsActive;
			Factory.Save();

			var contactsToSend = contacts.Take(2).ToArray();

			var campaignContactCollection = ContactCollection(new List<BusinessObject>(contactsToSend), campaign2);
			var campaignSender = new GlbCompanyCampaignSender(campaign2,
				campaignContactCollection.Cast<CampaignContact>().ToList(), false);
			campaignSender.ShouldContinueWithSending += (num, campaignItem) => true;
			Assert("Should send correctly", campaignSender.CheckAndSendCampaigns());

			AssertEquals(contactsToSend.Length, campaign2.CampaignsItemsSent.Count);
			campaign2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>().ForEach(item =>
			{
				var expectedCampaign = expectedSender(campaign1, campaign2);
				AssertEquals($"{expectedCampaign.G0_CampaignName} COR Email", expectedCampaign.CampaignCoordinator.GS_EmailAddress,
					item.G8_SenderEmailAddress);
				AssertEquals($"{expectedCampaign.G0_CampaignName} COR", expectedCampaign.CampaignCoordinator.GS_Code,
					item.G8_GS_NKSender);
			});
		}

		#endregion

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var directionRules = new SalesRelationDirectionRuleCollection();
			directionRules.AddNewRule(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity,
				SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			OrganisationsDataRegistry.Instance.SalesRelationDirectionRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				directionRules);
		}

		void AssertCorrectMessageIsDisplayed(GlbCompanyCampaignTest.FakeFormForTest form, ZString message,
			ZString summary, bool isError)
		{
			AssertEquals("Is / Is not Error", isError, form.MessageOnCampaignSendingArgs.IsError);
			AssertEquals("Should display message", message, form.MessageOnCampaignSendingArgs.Message);
			AssertEquals("Should display summary", summary, form.MessageOnCampaignSendingArgs.Summary);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			form.MessageOnCampaignSendingArgs = null;
		}

		readonly string StartTag = Enterprise.Core.Constants.DocumentEngine.EmailParsing.StartTag;
		readonly string EndTag = Enterprise.Core.Constants.DocumentEngine.EmailParsing.EndTag;

		OrgContact CreateContact(ZString name, OrgHeader org)
		{
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = name;
			contact.OC_Email = "default@cargowise.com";
			contact.OC_Phone =
				GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			return contact;
		}

		public static GlbCampaignContactCollection ContactCollection(List<BusinessObject> bizOList, GlbCompanyCampaign campaign)
		{
			GlbCampaignContactCollection contactCollection = new GlbCampaignContactCollection(campaign);
			ZQuery query = new ZQuery();

			if (bizOList != null)
			{
				bizOList.ForEach(item =>
				{
					if (!item.IsDeleted)
					{
						query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.PK, item.PK);
					}
				});
			}

			campaign.AdditionalFilter = !query.IsEmpty ? query : new ZQuery();
			contactCollection.Load(!query.IsEmpty ? query : ZQuery.NoResultQuery);

			return contactCollection;
		}

		public void RefreshFromDb(GlbCampaignContactCollection contactCollection, ZQuery completeFilter)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(completeFilter);
			query.ReLoadExistingRows = true;
			Factory.Load<CampaignContact>(query);
		}

		GlbCompanyCampaignTestHelper Helper
		{
			get { return helper ?? (helper = new GlbCompanyCampaignTestHelper(Factory)); }
		}

		GlbCompanyCampaignTestHelper helper;

		#endregion
	}
}
