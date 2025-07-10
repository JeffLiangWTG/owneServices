using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkItemFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				WorkItemSchema.Constants.WKI_WorkItemType,
				WorkItemSchema.Constants.WKI_WorkItemArea,
				WorkItemSchema.Constants.WKI_ActivityType,
				WorkItemSchema.Constants.WKI_ActivitySubtype,
				WorkItemSchema.Constants.WKI_Priority,
				WorkItemSchema.Constants.WKI_GE_AssignedDepartment,
				WorkItemSchema.Constants.WKI_PortOrCountry,
			};
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|DetailsTab", "Details"), ControlNames.DetailsTabName, true);
			result.ResumeValidation();
			return result;
		}

		public override TabPlacementProhibition[] TabPlacementProhibitions
		{
			get
			{
				List<TabPlacementProhibition> result = new List<TabPlacementProhibition>();
				result.Add(new TabPlacementProhibition(ControlNames.DetailsTabName, TabPlacement.Placements.BottomMiddle, TabPlacement.Placements.BottomLeft));
				return result.ToArray();
			}
		}

		internal static MultilingualString PortOrCountryLabel
		{
			get { return ResString.GetMultilingualString("WorkItemFormCustom|CountryField", "Country/Region/Port"); }
		}

		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			FormCustomisableElementCollection result = new FormCustomisableElementCollection();

			result.SuspendValidation();

			// Individual controls
			var detailsPanelName = (NoResString)ControlNames.DetailsPanel;
			result.Add(ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value, ControlNames.WorkItemType, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 0);
			result.Add(ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value, ControlNames.WorkItemArea, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 1);
			result.Add(ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value, ControlNames.ActivityType, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 2);
			result.Add(ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value, ControlNames.ActivitySubType, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 3);
			result.Add(ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value, ControlNames.Priority, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 4);
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|DepartmentField", "Department"), ControlNames.Department, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 5);
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|CompanyField", "Company"), ControlNames.Company, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 6);
			result.Add(PortOrCountryLabel, ControlNames.PortOrCountry, false, detailsPanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 7);

			var statePanelName = (NoResString)ControlNames.StatePanel;
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|TaskStatusField", "Task Status"), ControlNames.TaskStatus, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 0);
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|TaskAssignedField", "Task Assigned"), ControlNames.TaskStaff, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 1);
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|StatusField", "Status"), ControlNames.Status, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 2);
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|CreatedBy", "Created By"), ControlNames.CreatedBy, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 3);
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|CreatedTime", "Created Time"), ControlNames.CreatedTime, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 4);
			result.Add(ProcessManagementRegistry.Instance.DefectIntroducedInWorkItemLabel.Value, ControlNames.DefectIntroducedInWorkItem, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 5);
			result.Add(ProcessManagementRegistry.Instance.DefectIntroducedInTaskLabel.Value, ControlNames.DefectIntroducedInTask, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 6);
			result.Add(ProcessManagementRegistry.Instance.FirstCBThatMissedDefectLabel.Value, ControlNames.FirstCBThatMissedDefect, false, statePanelName, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 7);

			// Panels
			var customFieldGroup = (NoResString)ControlNames.CustomFieldsGroup;
			result.Add(ResString.GetMultilingualString("WorkItemFormCustom|CustomPanel", "Custom Fields"), ControlNames.CustomFieldsPanel, false, customFieldGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight);

			if (ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled)
			{
				result.Add(ResString.GetMultilingualString("WorkItemFormCustom|ReleaseSequenceName", "Sequence Name"), ControlNames.SequenceName, false, customFieldGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 1);
				result.Add(ResString.GetMultilingualString("WorkItemFormCustom|ReleaseSequencePosition", "Position"), ControlNames.SequencePosition, false, customFieldGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 2);
				result.Add(ResString.GetMultilingualString("WorkItemFormCustom|ReleaseSequenceValue", "Value"), ControlNames.SequenceValue, false, customFieldGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 3);
				result.Add(ResString.GetMultilingualString("WorkItemFormCustom|ReleaseSequenceInvestment", "Investment"), ControlNames.SequenceInvestment, false, customFieldGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 4);
				result.Add(ResString.GetMultilingualString("WorkItemFormCustom|ReleaseSequenceDate", "Due Date"), ControlNames.SequenceDate, false, customFieldGroup, ControlNames.DetailsTabName, TabPlacement.Placements.TopRight, 5);
			}

			result.ResumeValidation();

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "persistent name not to be translated")]
		public static class ControlNames
		{
			public const string WorkItemType = "WorkItemTypeDropEdit";
			public const string WorkItemArea = "WorkItemAreaDropEdit";
			public const string ActivityType = "ActivityTypeDropEdit";
			public const string ActivitySubType = "ActivitySubTypeDropEdit";
			public const string Priority = "PriorityDropEdit";

			public const string Department = "DepartmentFindBox";
			public const string Company = "CompanyFindBox";
			public const string PortOrCountry = "PortOrCountryFindBox";

			public const string Status = "StatusBox";
			public const string TaskStatus = "TaskStatusBox";
			public const string TaskStaff = "TaskAssignedStaffBox";
			public const string CreatedBy = "CreatedByStaffBox";
			public const string CreatedTime = "CreatedTimeBox";

			public const string DefectIntroducedInWorkItem = "DefectIntroducedInWorkItemGuidFindBox";
			public const string DefectIntroducedInTask = "DefectIntroducedInTaskGuidDropEdit";
			public const string FirstCBThatMissedDefect = "FirstCBThatMissedDefectGuidDropEdit";

			public const string SequenceName = "ReleaseSequenceNameBox";
			public const string SequencePosition = "ReleaseSequencePositionBox";
			public const string SequenceValue = "ReleaseSequenceValueBox";
			public const string SequenceInvestment = "ReleaseSequenceInvestmentBox";
			public const string SequenceDate = "ReleaseSequenceDateBox";

			// Panel Names
			public const string CustomFieldsPanel = "WorkItemCustomFields";
			public const string CustomFieldsGroup = "Custom";
			public const string DetailsPanel = "Details";
			public const string StatePanel = "State";

			public const string DetailsTabName = "MainTabPage";
		}
	}
}
