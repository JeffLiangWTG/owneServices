using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OpportunityProcessTasks : CRMProcessTask, IOpportunityProcessTasks
	{
		public OpportunityProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Opportunity; }
		}

		protected internal override Type ParentType
		{
			get { return typeof(OrgOpportunity); }
		}

		public new OrgOpportunity Parent
		{
			get { return (OrgOpportunity)base.Parent; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString @ref = base.CustomLogReferenceSuffix;

				if (Parent != null)
				{
					@ref += " " + Res.GetString("228c8c57-c7bf-4eb8-a3aa-d91a3f41108b", "for Opportunity {0}.", Parent.P8_OpportunityID);
				}

				if (P9_ScheduledDate.IsValid && (P9_ScheduledDateInfo.HasChanges || !IsInDatabase))
				{
					@ref += " " + Res.GetString("b67d8f6f-f504-48c2-b0f5-d959b1b7baf7", "Scheduled Date Set");
				}

				return @ref;
			}
		}

		#endregion

		#region Calendar Reminder

		protected override string ReminderBodyTemplate
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.Append(Res.GetString("540597e2-6cae-4fce-b8b4-08417088e40a", "Opportunity - {0}", ParentIDMacroTemplate));

				var orgOpportunityParent = Parent;
				if (orgOpportunityParent != null)
				{
					builder.AppendLine(" - " + orgOpportunityParent.P8_OpportunityDescription);
				}

				var org = Organisation;
				if (org != null)
				{
					SalesReminderBuilder.AddOrganizationNameAndCode(builder, org.OH_FullNameTruncated, org.OH_Code);
					builder.AppendLine();

					SalesReminderBuilder.AddOrganizationAndContactDetails(builder, org, Contact, false);
					SalesReminderBuilder.AddOrganizationLastSalesCall(builder, org, false);
				}

				if (orgOpportunityParent != null && !orgOpportunityParent.P8_OpportunityType.IsEmpty)
				{
					builder.Append(Res.GetString("8ad846bd-fa3c-4c8b-9860-67540cc8414d", "Sales Type:"));
					builder.Append(" ");
					builder.Append(orgOpportunityParent.Lookups.Types.GetDescriptionFromCode(orgOpportunityParent.P8_OpportunityType));
				}

				builder.AppendLine();
				AppendTaskDetails(builder);

				if (orgOpportunityParent != null)
				{
					builder.AppendLine(Res.GetString("49085616-5de5-4c51-b30e-28945ead0cb2", "Opportunity Notes: {0}", ORtfTextUtil.RtfToText(orgOpportunityParent.P8_OpportunityNotes)));
				}

				return builder.ToString();
			}
		}

		protected override string ReplaceReminderMacros(string template, bool forHtml)
		{
			var result = base.ReplaceReminderMacros(template, forHtml);

			return Parent == null
				? result
				: result.Replace(ParentIDMacroTemplate, forHtml
						? $@"<a href=""{ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.Opportunity, Parent.PK.ToGuid())}"">{Parent.P8_OpportunityID}</a>"
						: (string)Parent.P8_OpportunityID);
		}

		public const string ParentIDMacroTemplate = "{!PARENTID!}";

		#endregion
	}
}
