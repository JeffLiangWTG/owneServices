using System;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	// When a Quotation has follow up date, a reminder is sent out
	// This reminder email should consist of the latest Quotation Documents
	public class QuoteReminder : Reminder
	{
		public QuoteReminder(Quote quote)
			: base
			(
				  quote.PK + "QuoteFollowUpDate",
				  quote.PK,
				  new ZString(quote.TableName),
				  DateTimeKind.Local,
				  quote.TH_FollowUpDate,
				  quote.TH_FollowUpDate,
				  Res.GetString("69250c4b-10a1-4956-89c0-69695a189515", "Follow up for Quote {0} for client {1}", quote.QuoteNumberWithoutAmendmentSuffix, quote.Header.OH_FullNameTruncated),
				  GetBody(quote)
			)
		{
			Recipients.Add(GlbStaff.CurrentUser.GS_FullName, GlbStaff.CurrentUser.GS_EmailAddress);
			if (quote.HeaderStaffAssignments.OverallSalesRepStaff != null)
			{
				Recipients.Add(quote.HeaderStaffAssignments.OverallSalesRepStaff.GS_FullName, quote.HeaderStaffAssignments.OverallSalesRepStaff.GS_EmailAddress);
			}

			var docManagerInfo = ((IDocManagerSupport)quote).DocManagerInfo;

			var latestSystemQuotationDoc = docManagerInfo.AllEDocs
				.Cast<IeDoc>()
				.Where(eDoc => eDoc.DocType == Core.Constants.RefDocTypes.SystemQuotation)
				.OrderByDescending(eDoc => eDoc.DateAdded)
				.FirstOrDefault();

			if (latestSystemQuotationDoc != null)
			{
				var attachmentDef = new AttachmentDef(latestSystemQuotationDoc.FileName, latestSystemQuotationDoc.ImageData);
				Attachments.Add(attachmentDef);
			}
		}

		static string GetBody(Quote quote)
		{
			var body = new StringBuilder(Res.GetString("5f51bd52-d0ed-4f8e-b579-30e9651ba8a8", "Follow up call due regarding quote for client {0} ({1})", quote.Header.OH_FullNameTruncated, quote.Header.OH_Code));
			body.AppendLine().AppendLine();
			body.Append(Res.GetString("882c53b6-6d1b-45d3-b8e2-55c0e01445bd", "Contact Details"));
			body.AppendLine().AppendLine();

			var contact = new DefaultContactFinder(quote.Header).DefaultContact(ContactType.Sales);
			if (contact != null)
			{
				body.AppendLine(contact.OC_ContactName);
				body.Append(Res.GetString("f176099c-9350-4b1f-a6d9-674b4a5d0809", "Email:")).Append(" ").AppendLine(contact.OC_Email);
				body.Append(Res.GetString("2106b7ed-1955-4ddc-b89e-06c31074d766", "Fax:")).Append(" ").AppendLine(contact.OC_Fax);
				body.Append(Res.GetString("2adc1136-6a69-4f96-a7c8-40280f5bae35", "Phone:")).Append(" ").AppendLine(contact.OC_Phone);
				body.AppendLine();
			}

			return body.ToString();
		}
	}
}
