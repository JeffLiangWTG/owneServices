using System;
using System.Globalization;
using System.Net;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class SalesReminderBuilder
	{
		static public void AddOrganizationNameAndCode(ZStringBuilder builder, ZString orgName, ZString orgCode)
		{
			builder.Append(Res.GetString("8b62f1b4-91cd-48b4-b7f1-ebcfe8b59a6e", "Organization: {0}", orgName));
			if (!orgCode.IsEmpty)
			{
				builder.AppendLine(" (" + orgCode + ")");
			}
			else
			{
				builder.AppendLine();
			}
		}

		static public void AddOrganizationLastSalesCall(ZStringBuilder builder, OrgHeader org, bool forHtml)
		{
			if (org == null)
			{
				return;
			}

			string lastCommunicationDateLabel = Res.GetString("A1944B73-6CEC-41FD-8099-79780212A5EA", "Last Communication Date");
			if (forHtml)
			{
				AddHtmlLabelAndToBeEncodedValue(builder, lastCommunicationDateLabel, org.MiscServ.OM_CMLastCallDateLocal.ToShortDateString());
			}
			else
			{
				AddPlainLabelAndValue(builder, lastCommunicationDateLabel, org.MiscServ.OM_CMLastCallDateLocal.ToShortDateString());
			}
		}

		static public void AddOrganizationAndContactDetails(ZStringBuilder builder, OrgHeader org, OrgContact contact, bool forHtml)
		{
			OrgAddress address = null;

			if (contact != null)
			{
				SalesReminderBuilder.AddContact(builder, contact, forHtml);
				address = contact.EffectiveContactAddress;
			}

			if (address == null && org != null)
			{
				address = org.Addresses.MainAddress;
			}

			SalesReminderBuilder.AddAddress(builder, address, forHtml);
			builder.AppendLine();
		}

		static public void AddContact(ZStringBuilder builder, OrgContact contact, bool forHtml)
		{
			if (contact != null)
			{
				AddContact(builder,
					contact.OC_ContactName,
					contact.OC_Title,
					contact.OC_Phone,
					contact.OC_Email,
					contact.OC_Mobile,
					contact.OC_JobCategory, forHtml);
			}
		}

		static public void AddContact(ZStringBuilder builder, string name, string title, string phone, string email, string mobile, string jobCategory, bool forHtml)
		{
			Action<ZStringBuilder, string, string> addLabelAndValue;
			if (forHtml)
			{
				addLabelAndValue = AddHtmlLabelAndToBeEncodedValue;
			}
			else
			{
				addLabelAndValue = AddPlainLabelAndValue;
			}

			addLabelAndValue(builder, Res.GetString("5C02FBC8-6ED0-4720-BE45-29C8B2D6C59A", "Contact"), name);
			if (!string.IsNullOrEmpty(title))
			{
				addLabelAndValue(builder, Res.GetString("AC101B8E-22BE-4F05-83A6-F0F53AC28552", "Title"), title);
			}
			if (!string.IsNullOrEmpty(jobCategory))
			{
				var description = OrgContactLookups.CreateJobCategoryList()[jobCategory, StringComparison.OrdinalIgnoreCase]?.Description ?? jobCategory;
				addLabelAndValue(
					builder,
					Res.GetString("AF858E60-A88F-49FE-B7CF-B02FCF00BA39", "Job Category"),
					description);
			}
			if (!string.IsNullOrEmpty(email))
			{
				addLabelAndValue(builder, Res.GetString("37A7CE30-BE8E-44E8-80A4-6C4A30C169B8", "Email"), email);
			}
			if (!string.IsNullOrEmpty(phone))
			{
				addLabelAndValue(builder, Res.GetString("9A4CC89C-8476-483F-8A2F-EB28A27E3BBE", "Phone"), phone);
			}
			if (!string.IsNullOrEmpty(mobile))
			{
				addLabelAndValue(builder, Res.GetString("CCBBCC63-955A-4422-8D33-B0BC4AFA4ADA", "Mobile"), mobile);
			}

			builder.AppendLine();
		}

		static public void AddAddress(ZStringBuilder builder, OrgAddress address, bool forHtml)
		{
			if (address == null)
			{
				return;
			}

			AddClientAddressLabel(builder, forHtml);
			AddPostalAddressWithoutCompanyName(builder, address);
			AddOfficeEmailAndPhone(builder, address, forHtml);
		}

		static public void AddClientAddressLabel(ZStringBuilder builder, bool forHtml)
		{
			string clientAddressHeader = Res.GetString("87E2E4C5-DEA9-4E41-921C-DB4646E06EC1", "Client Address");
			if (forHtml)
			{
				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"<strong>{0}</strong>:", clientAddressHeader));
			}
			else
			{
				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}:", clientAddressHeader));
			}
		}

		static public void AddOfficeEmailAndPhone(ZStringBuilder builder, OrgAddress address, bool forHtml)
		{
			if (address == null)
			{
				return;
			}

			Action<ZStringBuilder, string, string> addLabelAndValue;
			if (forHtml)
			{
				addLabelAndValue = AddHtmlLabelAndToBeEncodedValue;
			}
			else
			{
				addLabelAndValue = AddPlainLabelAndValue;
			}

			if (!string.IsNullOrEmpty(address.OA_Email))
			{
				addLabelAndValue(builder, Res.GetString("5A496CB8-D77B-4C09-8DDE-8135C36969D4", "Office Email"), address.OA_Email);
			}
			if (!string.IsNullOrEmpty(address.OA_Phone))
			{
				addLabelAndValue(builder, Res.GetString("F5F44635-B99E-4384-BDA9-87F7F08CD26D", "Office Phone"), address.OA_Phone);
			}
		}

		static public void AddPostalAddressWithoutCompanyName(ZStringBuilder builder, OrgAddress address)
		{
			var formatter = new AddressFormatter(address.Factory, address, GlbCompany.CurrentCompany, false);
			// formatter uses \n for newline, but we want \r\n
			builder.AppendLine(formatter.PostalAddressWithoutCompanyName().Replace("\n", "\r\n"));
		}

		static public void AddHtmlLabelAndToBeEncodedValue(ZStringBuilder builder, string label, string value)
		{
			AddHtmlLabelAndAlreadyEncodedValue(builder, label, WebUtility.HtmlEncode(value));
		}

		static public void AddHtmlLabelAndAlreadyEncodedValue(ZStringBuilder builder, string label, string value)
		{
			builder.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"<strong>{0}</strong>: {1}", label, value));
		}

		static public void AddPlainLabelAndValue(ZStringBuilder builder, string label, string value)
		{
			builder.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", label, value));
		}
	}
}
