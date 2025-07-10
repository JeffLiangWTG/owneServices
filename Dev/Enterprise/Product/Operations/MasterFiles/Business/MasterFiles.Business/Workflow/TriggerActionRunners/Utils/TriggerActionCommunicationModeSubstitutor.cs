using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public static class TriggerActionCommunicationModeSubstitutor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded constant")]
		public static class ReplacementConstants
		{
			public const string Description = "(*Description*)";
			public const string JobNumber = "(*JobNumber*)";
			public const string AssignedTo = "(*AssignedTo*)";
			public const string Destination = "(*Destination*)";
			public const string Origin = "(*Origin*)";
			public const string WebTrackerUrl = "(*WebTrackerUrl*)";
			public const string EventReference = "(*EventReference*)";
		}

		public delegate ZString ExtraDataSubstitutionDelegate(ProcessTaskNotification action, BusinessObject bizo, ZString data);

		static readonly Regex getEventContextByKeyRegex = new Regex(@"\(\*GetEventContextByKey\((?<key>.*?)\)\*\)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public static ZString Substitute(ProcessTaskNotification action, BusinessObject parent, IStmALog @event, CommunicationModeSubstitutorProperty property, ZString value, ExtraDataSubstitutionDelegate extraSubstitution = null, ExtraDataSubstitutionDelegate extraPostMacroDataSubstitution = null)
		{
			var result = value;

			if (result.IsEmpty && action.WorkflowDescriptor is WorkflowDescriptor descriptor)
			{
				result = descriptor.GetFallbackForEmptyCommunicationModeProperty(action, property);
			}

			result = result.ReplaceIgnoringCase(ReplacementConstants.Description, action.Parent.Description);
			var jobNumber = GetJobNumber(parent);
			result = result.ReplaceIgnoringCase(ReplacementConstants.JobNumber, jobNumber);
			result = result.ReplaceIgnoringCase(ReplacementConstants.AssignedTo, GetAssignedToStringReplacement(action));
			//Checking before making the substitution - so we only get issue report if someone is trying to use the feature without it being possible, not constantly
			if (result.Contains(ReplacementConstants.Destination, StringComparison.OrdinalIgnoreCase))
			{
				result = result.ReplaceIgnoringCase(ReplacementConstants.Destination, GetDestination(action, parent));
			}
			if (result.Contains(ReplacementConstants.Origin, StringComparison.OrdinalIgnoreCase))
			{
				result = result.ReplaceIgnoringCase(ReplacementConstants.Origin, GetOrigin(action, parent));
			}
			if (result.Contains(ReplacementConstants.WebTrackerUrl, StringComparison.OrdinalIgnoreCase))
			{
				result = result.ReplaceIgnoringCase(ReplacementConstants.WebTrackerUrl, GetWebTrackerUrl(action, parent, jobNumber));
			}

			result = result.ReplaceIgnoringCase(ReplacementConstants.EventReference, GetEventReference(@event));

			result = SubstituteGetEventContextByKey(@event, result);

			if (extraSubstitution != null)
			{
				result = extraSubstitution(action, parent, result);
			}

			result = SubstituteExtendedMacroTemplate(action, parent, @event, result);

			if (extraPostMacroDataSubstitution != null)
			{
				result = extraPostMacroDataSubstitution(action, parent, result);
			}

			return result;
		}

		internal static ZString GetJobNumber(BusinessObject parent)
		{
			var jobNumberForWorkflowSource = parent as IJobNumberForWorkflow;
			if (jobNumberForWorkflowSource != null)
			{
				return jobNumberForWorkflowSource.JobNumber;
			}
			else
			{
				var jobNumberSource = parent as IJobNumber;
				if (jobNumberSource != null)
				{
					return jobNumberSource.JobNumber;
				}
				else if (parent != null)
				{
					return parent.HumanReadableName;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		internal static IWorkflowInformationProvider GetWorkflowInformationProvider(ProcessTaskNotification action, BusinessObject job)
		{
			IWorkflowInformationProvider result;

			if (job != null)
			{
				result = ((IWorkflowProvider)job).GetWorkflowInformationProvider();
			}
			else
			{
				result = null;
			}

			return result;
		}

		static ZString GetOrigin(ProcessTaskNotification action, BusinessObject job)
		{
			var workflowInformationProvider = GetWorkflowInformationProvider(action, job);
			if (workflowInformationProvider != null)
			{
				return workflowInformationProvider.Origin;
			}
			else
			{
				return ZString.Empty;
			}
		}

		static ZString GetDestination(ProcessTaskNotification action, BusinessObject job)
		{
			var workflowInformationProvider = GetWorkflowInformationProvider(action, job);
			if (workflowInformationProvider != null)
			{
				return workflowInformationProvider.Destination;
			}
			else
			{
				return ZString.Empty;
			}
		}

		static ZString GetWebTrackerUrl(ProcessTaskNotification action, BusinessObject parent, ZString jobNumber)
		{
			var workflowInformationProvider = GetWorkflowInformationProvider(action, parent);
			string link = null;
			if (workflowInformationProvider != null)
			{
				link = TrackingUrlCreator.Instance.CreateUrl(ZGuid.Empty, workflowInformationProvider.BusinessContext, parent.PK);
			}
			if (!string.IsNullOrEmpty(link))
			{
				return (!jobNumber.IsEmpty) ? string.Format(CultureInfo.InvariantCulture, "<a href={0}>{1}</a>", link, WebUtility.HtmlEncode(jobNumber)) : link;
			}
			else
			{
				return ZString.Empty;
			}
		}

		static ZString GetEventReference(IStmALog @event)
		{
			return @event?.SL_ReferenceForBinding ?? ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System constant")]
		static ZString SubstituteExtendedMacroTemplate(ProcessTaskNotification action, BusinessObject parent, IStmALog @event, ZString text)
		{
			const string tempOpeningBraket = "||&lt;||";
			const string tempClosingBraket = "||&gt;||";

			string result = text;

			var roots = ObjectFactory.Get<ITriggerActionRootProvider>().GetRoots(action, parent, @event);
			if (roots != null && roots.Any())
			{
				result = result.Replace(MacroConstants.DefaultMacroOpeningBracket, tempOpeningBraket);
				result = result.Replace("(*", MacroConstants.DefaultMacroOpeningBracket);

				result = result.Replace(MacroConstants.DefaultMacroClosingBracket, tempClosingBraket);
				result = result.Replace("*)", MacroConstants.DefaultMacroClosingBracket);

				result = ObjectFactory.Get<ITextMacroProcessor>().Replace(result, roots);

				result = result.Replace(tempOpeningBraket, MacroConstants.DefaultMacroOpeningBracket);
				result = result.Replace(tempClosingBraket, MacroConstants.DefaultMacroClosingBracket);
			}

			return result;
		}

		static ZString SubstituteGetEventContextByKey(IStmALog @event, ZString message)
		{
			var result = getEventContextByKeyRegex.Replace(message, (Match match) =>
			{
				string key = match.Groups["key"].Value.Trim();
				return GetEventContextByKey(@event, key);
			});

			return result;
		}

		static ZString GetEventContextByKey(IStmALog @event, ZString key)
		{
			KeyDataPair eventContextEntry = null;
			if (@event != null)
			{
				eventContextEntry = MessageSourceItemRetriever.GetSourceItems(@event.Factory, StmALog.GetRelatedEDIMessage(@event)).Cast<KeyDataPair>().FirstOrDefault(item => item.Key == key);
			}
			return eventContextEntry != null ? eventContextEntry.Data : ZString.Empty;
		}

		static ZString GetAssignedToStringReplacement(ProcessTaskNotification action)
		{
			ZString result = ZString.Empty;
			var trigger = action.Parent;

			if (trigger.GetAssignedGroup() != null)
			{
				result = trigger.GetAssignedGroup().GG_Desc;
			}
			else if (trigger.GetAssignedStaffMember() != null)
			{
				result = trigger.GetAssignedStaffMember().GS_FullName;
			}
			else
			{
				result = Res.GetString("6cf1d0e0-9300-4c90-b5d8-580ab94e7217", "Unassigned");
			}

			return result;
		}
	}
}
