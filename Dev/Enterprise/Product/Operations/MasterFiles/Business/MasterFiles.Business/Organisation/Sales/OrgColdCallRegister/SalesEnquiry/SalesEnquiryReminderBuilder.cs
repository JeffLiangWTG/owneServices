using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class SalesEnquiryReminderBuilder
	{
		public static void AddOrganizationAndContactDetails(ZStringBuilder builder, SalesEnquiry inquiry, bool forHtml)
		{
			AddContact(builder, inquiry, forHtml);
			AddAddress(builder, inquiry, forHtml);
			builder.AppendLine();
		}

		static void AddContact(ZStringBuilder builder, SalesEnquiry inquiry, bool forHtml)
		{
			var linkedContact = inquiry.LinkedContact;
			SalesReminderBuilder.AddContact(builder,
				inquiry.O1_ContactName,
				linkedContact != null ? (string)linkedContact.OC_Title : "",
				inquiry.O1_Phone,
				inquiry.O1_Email,
				inquiry.O1_Mobile,
				inquiry.O1_JobCategory,
				forHtml);
		}

		static void AddAddress(ZStringBuilder builder, SalesEnquiry inquiry, bool forHtml)
		{
			SalesReminderBuilder.AddClientAddressLabel(builder, forHtml);
			AddPostalAddressWithoutCompanyName(builder, inquiry.Factory,
				inquiry.O1_Address1,
				inquiry.O1_Address2,
				inquiry.O1_City,
				inquiry.O1_State,
				inquiry.O1_PostCode,
				(NoResString)"");

			var org = inquiry.Header;
			if (org != null)
			{
				SalesReminderBuilder.AddOfficeEmailAndPhone(builder, org.MainAddress, forHtml);
			}
		}

		static void AddPostalAddressWithoutCompanyName(ZStringBuilder builder, BusinessObjectFactory factory, string address1, string address2, string city, string state, string postCode, MultilingualString countryName)
		{
			var formatter = new AddressFormatter(factory, string.Empty, string.Empty, address1, address2, city, state, postCode, countryName, string.Empty, false);
			// formatter uses \n for newline, but we want \r\n
			builder.AppendLine(formatter.PostalAddressWithoutCompanyName().Replace("\n", "\r\n"));
		}
	}
}
