using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class SalesEnquiryProcessTask : CRMProcessTask
	{
		public SalesEnquiryProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.SalesEnquiry; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(SalesEnquiry); }
		}

		public new SalesEnquiry Parent
		{
			get { return (SalesEnquiry)base.Parent; }
		}

		#endregion

		#region Calendar Reminders

		public const string EnquiryIDMacroTemplate = "{!INQUIRYID!}";

		protected override string ReminderSubject
		{
			get
			{
				SalesEnquiry enquiry = Parent;
				ZStringBuilder builder = new ZStringBuilder();
				builder.Append(ParentJobDetails);
				builder.Append(" - ");
				builder.Append(enquiry.O1_CompanyName);
				builder.Append(" - ");
				builder.Append(enquiry.O1_ContactName);

				AppendReminderSubjectTaskID(builder);

				return builder.ToString();
			}
		}

		protected override string ReminderBodyTemplate
		{
			get
			{
				SalesEnquiry enquiry = Parent;
				var org = enquiry.Header;

				ZStringBuilder builder = new ZStringBuilder();

				builder.Append(Res.GetString("a3c64d63-999c-4a9c-ab02-2919861bd8c9", "Inquiry"));
				builder.AppendLine(" (" + EnquiryIDMacroTemplate + ")");

				SalesReminderBuilder.AddOrganizationNameAndCode(builder, enquiry.O1_CompanyName, org != null ? org.OH_Code : ZString.Empty);
				builder.AppendLine();

				SalesEnquiryReminderBuilder.AddOrganizationAndContactDetails(builder, enquiry, false);

				if (org != null)
				{
					SalesReminderBuilder.AddOrganizationLastSalesCall(builder, org, false);
				}

				builder.AppendLine(Res.GetString("a94aaa5f-cd19-47f1-a036-d274505cd831", "Inquiry Type: {0}", enquiry.EnquiryTypeDescription));

				base.AppendTaskDetails(builder);

				builder.Append(Res.GetString("612776b2-82e5-48b2-a741-69cab19b8276", "Inquiry Notes: {0}", ORtfTextUtil.RtfToText(enquiry.EnquiryNotesContent)));

				return builder.ToString();
			}
		}

		protected override string ReplaceReminderMacros(string template, bool forHtml)
		{
			string result = base.ReplaceReminderMacros(template, forHtml);

			return result.Replace(EnquiryIDMacroTemplate, forHtml
				? string.Format(@"<a href=""{0}"">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.SalesEnquiry, Parent.PK.ToGuid()), Parent.O1_LeadUniqueReference)
				: (string)Parent.O1_LeadUniqueReference);
		}

		#endregion
	}
}
