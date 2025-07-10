using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowExceptionsFilterBusinessObject : WorkflowFilterBusinessObjectBase
	{
		public WorkflowExceptionsFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddFlagsFilters(result);
			AddTextFilters(result);
			AddDateFilters(result);
			AddFindboxFilters(result);

			return result;
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return new[] { typeof(WorkflowFilterStripsHelper), typeof(IBMFilterStripsHelper) }; }
		}

		#region Status and Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter actionedFlagFilter = filters.AddTextFilter("Actioned", GetActionedStatusQuery, ActionedList);
			actionedFlagFilter.Category = FilterCategories.StatusAndFlags;
			actionedFlagFilter.DefaultProperty = ActionedCodes.IsNotActioned;
			actionedFlagFilter.Visibility = FilterVisibility.AlwaysVisible;
			actionedFlagFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|Actioned", "Actioned");
		}

		ZQuery GetActionedStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == ActionedCodes.IsActioned)
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, ActionedCodes.IsActioned);
			}
			else if (value == ActionedCodes.IsNotActioned)
			{
				query.AddToFilter(ProcessTasksSchema.P9_Status, ActionedCodes.IsNotActioned);
			}

			return query;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", ProcessTasksSchema.P9_Description)
				.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|Description", "Description");

			var exceptionTypeSubgroup = new ExceptionTypeCodeModuleFilterSubGroup(typeof(ProcessWorkflowExceptionType), ProcessWorkflowExceptionTypeSchema.WET_Code);
			var causeSubGroup = new ExceptionTypeNestedModuleFilterSubGroup(typeof(ProcessWorkflowExceptionCause), ProcessWorkflowExceptionCauseSchema.PK, ProcessWorkflowExceptionSchema.WEX_WEC_Cause);
			var resolutionSubGroup = new ExceptionTypeNestedModuleFilterSubGroup(typeof(ProcessWorkflowExceptionResolution), ProcessWorkflowExceptionResolutionSchema.PK, ProcessWorkflowExceptionSchema.WEX_WER_Resolution);

			var categoryFilter = filters.AddTextFilter("Category", ProcessWorkflowExceptionTypeSchema.WET_Category, WorkflowDataRegistry.Instance.ExceptionCategories.Value);
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|Category", "Category");
			categoryFilter.SubGroup = exceptionTypeSubgroup;

			var exceptionTypeCodeFilter = filters.AddTextFilter("Exception Type Code", ProcessTasksSchema.P9_SE_NKExceptionEvent);
			exceptionTypeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|ExceptionTypeCode", "Exception Type Code");

			filters.AddFilter(new WorkflowTypeFilter(Factory, typeof(ProcessTask), ProcessTasksSchema.P9_ParentID));

			var causeCodeFilter = filters.AddTextFilter("Cause Code", ProcessWorkflowExceptionCauseSchema.WEC_Code);
			causeCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|CauseCode", "Cause Code");
			causeCodeFilter.SubGroup = causeSubGroup;

			var causeDescriptionFilter = filters.AddTextFilter("Cause Description", ProcessWorkflowExceptionCauseSchema.WEC_Description);
			causeDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|CauseDescription", "Cause Description");
			causeDescriptionFilter.SubGroup = causeSubGroup;

			var resolutionCodeFilter = filters.AddTextFilter("Resolution Code", ProcessWorkflowExceptionResolutionSchema.WER_Code);
			resolutionCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|ResolutionCode", "Resolution Code");
			resolutionCodeFilter.SubGroup = resolutionSubGroup;

			var resolutionDescriptionFilter = filters.AddTextFilter("Resolution Description", ProcessWorkflowExceptionResolutionSchema.WER_Description);
			resolutionDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|ResolutionDescription", "Resolution Description");
			resolutionDescriptionFilter.SubGroup = resolutionSubGroup;
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Date", ProcessTasksSchema.P9_ActualDate).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|Date", "Date");
			filters.AddDateFilter("Actioned Date", ProcessTasksSchema.P9_CompletedTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|ActionedDate", "Actioned Date");
		}

		#endregion

		#region Findbox Filters

		void AddFindboxFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Assigned Group", ModuleIDs.GlbGroup, ProcessTasksSchema.P9_GG_AssignedGroup, Groups).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|AssignedGroup", "Assigned Group");
			filters.AddNkFilter("Assigned Staff", ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ModuleIDs.GlbStaff, Staffs).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowExceptionsFilter|AssignedStaff", "Assigned Staff");

			filters.AddFilter(new ParentJobModuleFilter("Parent Job", ProcessTasksSchema.P9_ParentID, Factory));
		}

		#endregion

		#region Overall

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				result.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.Equal, Core.Constants.Workflow.ExceptionType);
				return result;
			}
		}

		#endregion

		#endregion

		#region Lookups

		#region ActionedList

		public CodeDescriptionPairList ActionedList
		{
			get
			{
				if (fActionedList == null)
				{
					fActionedList = new CodeDescriptionPairList();
					fActionedList.AddPair(ActionedCodes.All, ActionedDescriptions.All);
					fActionedList.AddPair(ActionedCodes.IsActioned, ActionedDescriptions.IsActioned);
					fActionedList.AddPair(ActionedCodes.IsNotActioned, ActionedDescriptions.IsNotActioned);
				}
				return fActionedList;
			}
		}

		CodeDescriptionPairList fActionedList;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an abbreviation...")]
		public static class ActionedCodes
		{
			public const string IsActioned = "RSL";
			public const string IsNotActioned = "OPN";
			public const string All = "All";
		}

		static class ActionedDescriptions
		{
			public static string IsActioned { get { return Res.GetString("MasterFiles|WorkflowExceptionsFilter|ExceptionsAlreadyActioned", "Exceptions already actioned"); } }
			public static string IsNotActioned { get { return Res.GetString("MasterFiles|WorkflowExceptionsFilter|ExceptionsThatAreStillUnactioned", "Exceptions that are still unactioned"); } }
			public static string All { get { return Res.GetString("MasterFiles|WorkflowExceptionsFilter|BothOpenAndActionedExceptions", "Both Open and Actioned exceptions"); } }
		}

		#endregion

		#region Groups

		GlbGroupCollection Groups
		{
			get { return new GlbGroupCollection(Factory); }
		}

		#endregion

		#region Staffs

		GlbStaffCollection Staffs
		{
			get { return new GlbStaffCollection(Factory); }
		}

		#endregion

		#endregion
	}
}
