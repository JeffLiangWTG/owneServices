using System;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class QuoteReminderTest : RatingTestCase
	{
		[TestDate(2020, 04, 30, 6, 0, 0)]
		public void TestAttachmentByQuote_EdocNotExisting()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Unique Customer";

			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var attachmentData = Encoding.ASCII.GetBytes("Attachment Data");
			var attachmentDef = new AttachmentDef("eRoutEdi.erf", attachmentData);

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			AssertEquals("No quote follow up date specified", ZDateTime.Empty, quote.TH_FollowUpDate);
			quote.TH_QuoteNumber = "9999";
			quote.TH_FollowUpDate = new ZDateTime(2020, 12, 3);

			var quoteReminder = new QuoteReminder(quote);
			quoteReminder.Attachments.Add(attachmentDef);

			AssertEquals("Follow up for Quote 9999 for client Unique Customer", quoteReminder.Subject);
			AssertStartsWith("Reminder Body content",
@"Follow up call due regarding quote for client Unique Customer ()

Contact Details

The Sales Manager
Email:", quoteReminder.Body);
			AssertEquals(1, quoteReminder.Attachments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { attachmentDef }, quoteReminder.Attachments);
		}

		[TestDate(2020, 04, 30, 6, 0, 0)]
		public void TestAttachmentByQuote_NonSQTEeDocExisting()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Unique Customer";

			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var attachmentData = Encoding.ASCII.GetBytes("Attachment Data");
			var attachmentDef = new AttachmentDef("eRoutEdi.erf", attachmentData);

			var attachmentData2 = Encoding.ASCII.GetBytes("Attachment Data2");
			var attachmentDef2 = new AttachmentDef("eRoutEdi2.erf", attachmentData2);

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			AssertEquals("No quote follow up date specified", ZDateTime.Empty, quote.TH_FollowUpDate);
			quote.TH_QuoteNumber = "9999";
			quote.TH_FollowUpDate = new ZDateTime(2020, 12, 3);

			((IDocManagerSupport)quote).DocManagerInfo.AddFileOrDocument(attachmentData, "eRoutEdi.erf", Core.Constants.RefDocTypes.Quotation).DateAdded = new ZDateTime(2020, 05, 3).AddHours(2);
			((IDocManagerSupport)quote).DocManagerInfo.AddFileOrDocument(attachmentData2, "eRoutEdi2.erf", Core.Constants.RefDocTypes.Quotation).DateAdded = new ZDateTime(2020, 05, 3).AddHours(5);

			var quoteReminder = new QuoteReminder(quote);

			AssertEquals("Follow up for Quote 9999 for client Unique Customer", quoteReminder.Subject);
			AssertStartsWith("Reminder Body content",
@"Follow up call due regarding quote for client Unique Customer ()

Contact Details

The Sales Manager
Email:", quoteReminder.Body);
			AssertEquals("Reminder no attachments", 0, quoteReminder.Attachments.Count);
		}

		[TestDate(2020, 04, 30, 6, 0, 0)]
		public void TestAttachmentByQuote_EdocExistingLatest()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "Unique Customer";

			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

			var attachmentData = Encoding.ASCII.GetBytes("Attachment Data");
			var attachmentDef = new AttachmentDef("eRoutEdi.erf", attachmentData);

			var attachmentData2 = Encoding.ASCII.GetBytes("Attachment Data2");
			var attachmentDef2 = new AttachmentDef("eRoutEdi2.erf", attachmentData2);

			var quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			AssertEquals("No quote follow up date specified", ZDateTime.Empty, quote.TH_FollowUpDate);
			quote.TH_QuoteNumber = "9999";
			quote.TH_FollowUpDate = new ZDateTime(2020, 12, 3);

			((IDocManagerSupport)quote).DocManagerInfo.AddFileOrDocument(attachmentData, "eRoutEdi.erf", Core.Constants.RefDocTypes.SystemQuotation).DateAdded = new ZDateTime(2020, 05, 3).AddHours(2);
			((IDocManagerSupport)quote).DocManagerInfo.AddFileOrDocument(attachmentData2, "eRoutEdi2.erf", Core.Constants.RefDocTypes.SystemQuotation).DateAdded = new ZDateTime(2020, 05, 3).AddHours(5);

			var quoteReminder = new QuoteReminder(quote);

			AssertEquals("Follow up for Quote 9999 for client Unique Customer", quoteReminder.Subject);
			AssertStartsWith("Reminder Body content",
@"Follow up call due regarding quote for client Unique Customer ()

Contact Details

The Sales Manager
Email:", quoteReminder.Body);
			AssertEquals(1, quoteReminder.Attachments.Count);
			AssertContainsExactElementsInAnyOrder(
				new[] { attachmentDef2.Data },
				quoteReminder.Attachments.Cast<AttachmentDef>().ToList().Select(attachment => attachment.Data)
			);
		}
	}
}
