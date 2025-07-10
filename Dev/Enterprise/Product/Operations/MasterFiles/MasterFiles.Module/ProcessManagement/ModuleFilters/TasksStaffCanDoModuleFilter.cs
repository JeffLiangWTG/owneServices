using System.Collections.Generic;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	class TasksStaffCanDoModuleFilter : ModuleNkFilter
	{
		#region Construction and ModuleFilter overrides

		public TasksStaffCanDoModuleFilter(ZString description, GetList listDelegate)
			: base(description, ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ModuleIDs.GlbStaff, listDelegate)
		{
			ComparisonOperator = ComparisonConstants.CurrentUser;
			Mode = TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff;
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[]
		{
			ComparisonConstants.Exact,
			ComparisonConstants.CurrentUser,
			ComparisonConstants.FiltersMatch,
		};

		#endregion

		#region Mode

		[List(nameof(ModeList))]
		public ZString Mode
		{
			get => mode;
			set
			{
				SetNonPersistentPropertyValue(ModeInfo, ref mode, value);
				InvalidateCachedQuery();

				if (!IsValidationSuspended)
				{
					((TasksStaffCanDoModuleFilterValidation)Validation).ValidateMode();
				}
			}
		}

		ZString mode;

		public ZPropertyInfo ModeInfo => GetZPropertyInfo(nameof(Mode));

		public CodeDescriptionPairList ModeList => modeList ?? (modeList = new TasksStaffCanDoModuleFilterModes());
		CodeDescriptionPairList modeList;

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			if (Mode == TasksStaffCanDoModuleFilterModes.Codes.OnlyStaff)
			{
				return base.GetQuery();
			}

			var isFiltersMatchSelected = IsFilterCollectionComparisonOperatorCore(ComparisonOperator);

			var selectedStaffQuery = isFiltersMatchSelected
				? ((IModuleFilterWithSelectedFilters)this).GetSubFilterQueryIncludingCollectionFilters()
				: new ZQuery(GlbStaffSchema.GS_Code, Property);
			var selectedStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.PK);
			selectedStaffSubQuery.AddToFilter(selectedStaffQuery);

			var capabilityResourcePivotSubQuery = new ZDBOnlySubQuery(typeof(GlbResourceCapabilityPivot), GlbResourceCapabilityPivotSchema.G5_G4_Capability);
			capabilityResourcePivotSubQuery.AddSubQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, selectedStaffSubQuery, JoinCondition.And);

			var globalCapabilitySubQuery = GetCapabilitySubQuery(GlbCapabilityScopeList.Codes.GlobalScope, capabilityResourcePivotSubQuery);
			var groupCapabilitySubQuery = GetCapabilitySubQuery(GlbCapabilityScopeList.Codes.GroupScope, capabilityResourcePivotSubQuery);
			var groupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GG);
			groupLinkSubQuery.AddSubQuery(GlbGroupLinkSchema.GK_GS, selectedStaffSubQuery, JoinCondition.And);

			// Global capability
			var assignedToCapabilitySubQuery = GetNewTaskLevelSubQuery();
			assignedToCapabilitySubQuery.AddSubQuery(ProcessTasksSchema.P9_G4_RequiredCapability, globalCapabilitySubQuery, JoinCondition.And);

			// Group capability, no group on task, no workflow
			var noTaskGroupNoWorkflowSubQuery = GetNewTaskLevelSubQuery();
			noTaskGroupNoWorkflowSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, null);
			noTaskGroupNoWorkflowSubQuery.AddToFilter(ProcessTasksSchema.P9_FH_ProcessHeader, null);
			noTaskGroupNoWorkflowSubQuery.AddSubQuery(ProcessTasksSchema.P9_G4_RequiredCapability, groupCapabilitySubQuery, JoinCondition.And);
			assignedToCapabilitySubQuery.AddAsUnionQuery(noTaskGroupNoWorkflowSubQuery, true);

			// Group capability, selected staff are members of task's group
			var taskGroupContainsStaffSubQuery = GetNewTaskLevelSubQuery();
			taskGroupContainsStaffSubQuery.AddSubQuery(ProcessTasksSchema.P9_G4_RequiredCapability, groupCapabilitySubQuery, JoinCondition.And);
			taskGroupContainsStaffSubQuery.AddSubQuery(ProcessTasksSchema.P9_GG_AssignedGroup, groupLinkSubQuery, JoinCondition.And);
			assignedToCapabilitySubQuery.AddAsUnionQuery(taskGroupContainsStaffSubQuery, true);

			if (ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				// Group capability, no group on task or workflow (but workflow exists)
				var workflowExistsButHasNoGroupSubQuery = GetNewTaskLevelSubQuery();
				workflowExistsButHasNoGroupSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, null);
				workflowExistsButHasNoGroupSubQuery.AddSubQuery(ProcessTasksSchema.P9_G4_RequiredCapability, groupCapabilitySubQuery, JoinCondition.And);

				var processHeaderExistsSubQuery = GetNewProcessHeaderSubQuery();
				processHeaderExistsSubQuery.AddToFilter(ProcessHeaderSchema.FH_GG_ReleaseGroup, null);
				workflowExistsButHasNoGroupSubQuery.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, processHeaderExistsSubQuery, JoinCondition.And);
				assignedToCapabilitySubQuery.AddAsUnionQuery(workflowExistsButHasNoGroupSubQuery, true);

				// Group capability, no group on task and selected staff are members of task's workflow's group
				var workflowGroupContainsStaffSubQuery = GetNewTaskLevelSubQuery();
				workflowGroupContainsStaffSubQuery.AddToFilter(ProcessTasksSchema.P9_GG_AssignedGroup, null);
				workflowGroupContainsStaffSubQuery.AddSubQuery(ProcessTasksSchema.P9_G4_RequiredCapability, groupCapabilitySubQuery, JoinCondition.And);

				var workflowGroupSubQuery = GetNewProcessHeaderSubQuery();
				workflowGroupSubQuery.AddSubQuery(ProcessHeaderSchema.FH_GG_ReleaseGroup, groupLinkSubQuery, JoinCondition.And);
				workflowGroupContainsStaffSubQuery.AddSubQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflowGroupSubQuery, JoinCondition.And);
				assignedToCapabilitySubQuery.AddAsUnionQuery(workflowGroupContainsStaffSubQuery, true);
			}

			if (MatchTasksAssignedToStaffOrCapability())
			{
				var directStaffAssignmentSubQuery = new ZDBOnlySubQuery(typeof(ProcessTasks), ProcessTasksSchema.PK);

				if (isFiltersMatchSelected)
				{
					var selectStaffByCodeSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code, ProcessTasksSchema.P9_GS_NKAssignedStaffMember);
					selectStaffByCodeSubQuery.AddToFilter(selectedStaffQuery);
					directStaffAssignmentSubQuery.AddSubQuery(selectStaffByCodeSubQuery, JoinCondition.And);
				}
				else
				{
					directStaffAssignmentSubQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, Property);
				}

				assignedToCapabilitySubQuery.AddAsUnionQuery(directStaffAssignmentSubQuery, true);
			}

			var mainQuery = new ZDBOnlyQuery(typeof(ProcessTask));
			mainQuery.AddSubQuery(assignedToCapabilitySubQuery, JoinCondition.And);

			return mainQuery;
		}

		ZDBOnlySubQuery GetNewTaskLevelSubQuery()
		{
			var subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
			ProcessTaskFiltersHelper.AddNotMilestoneTriggerExceptionClause(subQuery);

			if (CapabilityTasksMustNotAlsoBeAssignedToStaff())
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, ZString.Empty);
			}

			return subQuery;
		}

		static ZDBOnlySubQuery GetCapabilitySubQuery(string scope, ZDBOnlySubQuery capabilityResourcePivotSubQuery)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(GlbCapability), GlbCapabilitySchema.PK);
			subQuery.AddToFilter(GlbCapabilitySchema.G4_CapacityScope, scope);
			subQuery.AddSubQuery(GlbCapabilitySchema.PK, capabilityResourcePivotSubQuery, JoinCondition.And);

			return subQuery;
		}

		static ZDBOnlySubQuery GetNewProcessHeaderSubQuery()
		{
			return new ZDBOnlySubQuery(ObjectFactory.GetType<IProcessHeader>(), ProcessHeaderSchema.PK);
		}

		bool MatchTasksAssignedToStaffOrCapability()
		{
			return Mode == TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityRegardlessOfStaffAssignment ||
				   Mode == TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff;
		}

		bool CapabilityTasksMustNotAlsoBeAssignedToStaff()
		{
			return Mode == TasksStaffCanDoModuleFilterModes.Codes.EitherStaffOrCapabilityAndUnassignedStaff ||
				   Mode == TasksStaffCanDoModuleFilterModes.Codes.RequiredCapabilityAndUnassignedStaff;
		}

		#endregion

		#region Serialization

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xml property string")]
		const string ModePropertyForXml = "Mode";

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString(ModePropertyForXml, Mode);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == ModePropertyForXml)
			{
				Mode = reader.ReadElementString(ModePropertyForXml);
			}
		}

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new TasksStaffCanDoModuleFilterValidation(this);
		}

		class TasksStaffCanDoModuleFilterValidation : ModuleNkFilterValidation
		{
			public TasksStaffCanDoModuleFilterValidation(ModuleNkFilter parent)
				: base(parent)
			{
			}

			public override void ValidateAll()
			{
				base.ValidateAll();
				ValidateMode();
			}

			public void ValidateMode()
			{
				ValidateCalculatedProperty(Parent.ModeInfo);
			}

			protected void CheckMode()
			{
				MandatoryValidation.CheckEntered(Parent.ModeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ModeInfo);
			}

			new TasksStaffCanDoModuleFilter Parent => (TasksStaffCanDoModuleFilter)ParentFilter;
		}

		#endregion
	}
}
