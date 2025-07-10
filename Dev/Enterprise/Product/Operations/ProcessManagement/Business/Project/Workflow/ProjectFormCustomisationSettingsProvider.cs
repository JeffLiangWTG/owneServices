using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class ProjectFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				WorkProjectSchema.Constants.WKP_Type,
				WorkProjectSchema.Constants.WKP_SubType,
				WorkProjectSchema.Constants.WKP_Module,
				WorkProjectSchema.Constants.WKP_Priority,
			};
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|DetailsTab", "Details"), ControlNames.DetailsTabName, true);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|AdditionalDetailsTab", "Additional Details"), ControlNames.AdditionalDetailsTabName, true);
			result.ResumeValidation();
			return result;
		}

		public override TabPlacementProhibition[] TabPlacementProhibitions
		{
			get
			{
				List<TabPlacementProhibition> result = new List<TabPlacementProhibition>();
				result.Add(new TabPlacementProhibition(ControlNames.DetailsTabName, TabPlacement.Placements.BottomMiddle));
				result.Add(new TabPlacementProhibition(ControlNames.AdditionalDetailsTabName, TabPlacement.Placements.BottomMiddle, TabPlacement.Placements.TopMiddle));
				return result.ToArray();
			}
		}

		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			FormCustomisableElementCollection result = new FormCustomisableElementCollection();

			result.SuspendValidation();

			// Individual controls
			var statePanel = (NoResString)ControlNames.StatePanel;
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|TaskStatusField", "Task Status"), ControlNames.TaskStatus, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 0);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|TaskAssignedField", "Task Assigned"), ControlNames.TaskStaff, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 1);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|StatusField", "Status"), ControlNames.Status, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 2);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|CreatedByField", "Created By"), ControlNames.CreatedBy, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 3);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|ProjectCreatedField", "Project Created"), ControlNames.ProjectCreated, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 4);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|ProjectClosedOrDeferredField", "Project Closed/Deferred Until/Def. Date Met"), ControlNames.ProjectClosedOrDeferred, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 5);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|ProjectManager", "Project Manager"), ControlNames.ProjectManager, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 6);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|Opportunity", "Opportunity"), ControlNames.Opportunity, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 7);

			// Panels
			var emptyResString = (NoResString)string.Empty;
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|ContactInformationPanel", "Contact Information"), ControlNames.ContactInformationPanel, false, emptyResString, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|Information", "Information/Custom Fields"), ControlNames.InformationPanel, false, emptyResString, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|LogPanel", "Project Log"), ControlNames.LogPanel, false, emptyResString, ControlNames.DetailsTabName, TabPlacement.Placements.BottomLeft);
			result.Add(ResString.GetMultilingualString("ProjectFormCustom|DescriptionPanel", "Description"), ControlNames.DescriptionPanel, false, emptyResString, ControlNames.DetailsTabName, TabPlacement.Placements.BottomRight);

			if (ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled)
			{
				var customFieldsGroup = (NoResString)ControlNames.CustomFieldsGroup;
				result.Add(ResString.GetMultilingualString("ProjectFormCustom|ReleaseSequenceName", "Sequence Name"), ControlNames.SequenceName, false, customFieldsGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 1);
				result.Add(ResString.GetMultilingualString("ProjectFormCustom|ReleaseSequencePosition", "Position"), ControlNames.SequencePosition, false, customFieldsGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 2);
				result.Add(ResString.GetMultilingualString("ProjectFormCustom|ReleaseSequenceValue", "Value"), ControlNames.SequenceValue, false, customFieldsGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 3);
				result.Add(ResString.GetMultilingualString("ProjectFormCustom|ReleaseSequenceInvestment", "Investment"), ControlNames.SequenceInvestment, false, customFieldsGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 4);
				result.Add(ResString.GetMultilingualString("ProjectFormCustom|ReleaseSequenceDate", "Due Date"), ControlNames.SequenceDate, false, customFieldsGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 5);
			}

			result.ResumeValidation();

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "persistent name not to be translated")]
		public static class ControlNames
		{
			public const string Status = "StatusBox";
			public const string TaskStatus = "TaskStatusBox";
			public const string TaskStaff = "TaskAssignedStaffBox";
			public const string CreatedBy = "CreatedByStaffBox";
			public const string ProjectCreated = "ProjectCreatedBox";
			public const string ProjectClosedOrDeferred = "ProjectClosedOrDeferredBox";
			public const string ProjectManager = "ProjectManagerFindBox";
			public const string Opportunity = "OpportunityGuidFindBox";

			// Panel Names
			public const string InformationPanel = "Information";
			public const string StatePanel = "State";
			public const string ContactInformationPanel = "ContactInformation";
			public const string LogPanel = "Log";
			public const string DescriptionPanel = "Description";

			public const string DetailsTabName = "MainTabPage";
			public const string AdditionalDetailsTabName = "AdditionalDetailsTabPage";

			public const string SequenceName = "ReleaseSequenceNameBox";
			public const string SequencePosition = "ReleaseSequencePositionBox";
			public const string SequenceValue = "ReleaseSequenceValueBox";
			public const string SequenceInvestment = "ReleaseSequenceInvestmentBox";
			public const string SequenceDate = "ReleaseSequenceDateBox";

			public const string CustomFieldsPanel = "WorkItemCustomFields";
			public const string CustomFieldsGroup = "Custom";
		}
	}
}
