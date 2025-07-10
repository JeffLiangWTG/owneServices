using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public class WorkRequestFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				WorkRequestSchema.Constants.WKR_SelectionCriteria1,
				WorkRequestSchema.Constants.WKR_SelectionCriteria2,
				WorkRequestSchema.Constants.WKR_SelectionCriteria3,
				WorkRequestSchema.Constants.WKR_SelectionCriteria4,
				WorkRequestSchema.Constants.WKR_SelectionCriteria5,
				WorkRequestSchema.Constants.WKR_GE_Department,
				WorkRequestSchema.Constants.WKR_GB_Branch,
				WorkRequestSchema.Constants.WKR_RN_NKCountry,
				WorkRequestSchema.Constants.WKR_OC_Client,
				WorkRequestSchema.Constants.WKR_RequestNumber,
			};
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			var result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("WorkRequestFormCustom|DetailsTab", "Details"), ControlNames.DetailsTabName, true);
			result.ResumeValidation();
			return result;
		}

		public override TabPlacementProhibition[] TabPlacementProhibitions
		{
			get
			{
				return new TabPlacementProhibition[] {
					new TabPlacementProhibition(ControlNames.DetailsTabName,
					TabPlacement.Placements.BottomMiddle, TabPlacement.Placements.BottomLeft, TabPlacement.Placements.BottomRight, TabPlacement.Placements.TopRight)
				};
			}
		}

		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			var result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			var detailsPanel = (NoResString)ControlNames.DetailsPanel;
			result.Add(ProcessManagementRegistry.Instance.SelectionCriterion1Caption.Value, ControlNames.SelectionCriterion1, false, detailsPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 0);
			result.Add(ProcessManagementRegistry.Instance.SelectionCriterion2Caption.Value, ControlNames.SelectionCriterion2, false, detailsPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 1);
			result.Add(ProcessManagementRegistry.Instance.SelectionCriterion3Caption.Value, ControlNames.SelectionCriterion3, false, detailsPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 2);
			result.Add(ProcessManagementRegistry.Instance.SelectionCriterion4Caption.Value, ControlNames.SelectionCriterion4, false, detailsPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 3);
			result.Add(ProcessManagementRegistry.Instance.SelectionCriterion5Caption.Value, ControlNames.SelectionCriterion5, false, detailsPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 4);

			var statusPanel = (NoResString)ControlNames.StatusPanel;
			result.Add(ResString.GetMultilingualString("WorkRequestFormCustom|BranchField", "Branch"), ControlNames.Branch, false, statusPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 5);
			result.Add(ResString.GetMultilingualString("WorkRequestFormCustom|DepartmentField", "Department"), ControlNames.Department, false, statusPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 6);
			result.Add(ResString.GetMultilingualString("WorkRequestFormCustom|CountryField", "Country"), ControlNames.Country, false, statusPanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 7);
			result.ResumeValidation();
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "persistent name not to be translated")]
		public static class ControlNames
		{
			public const string RequestNumber = "RequestNumberTextBox";
			public const string Client = "ClientFindBox";

			public const string SelectionCriterion1 = "SelectionCriterion1DropEdit";
			public const string SelectionCriterion2 = "SelectionCriterion2DropEdit";
			public const string SelectionCriterion3 = "SelectionCriterion3DropEdit";
			public const string SelectionCriterion4 = "SelectionCriterion4DropEdit";
			public const string SelectionCriterion5 = "SelectionCriterion5DropEdit";

			public const string Branch = "BranchFindBox";
			public const string Department = "DepartmentFindBox";
			public const string Country = "CountryFindBox";

			public const string DetailsPanel = "Details";
			public const string StatusPanel = "Status";
			public const string DetailsTabName = "MainTabPage";
		}
	}
}
