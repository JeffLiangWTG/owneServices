using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.GCR_Status)]
	public class GlbStaffChangeRequest : AutoGlbStaffChangeRequest, IWorkflowProvider, IDocManagerSupport
	{
		public GlbStaffChangeRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Template))]
		[List("Lookups.ChangeRequestTemplates")]
		public override ZGuid GCR_GSG_Template { get => base.GCR_GSG_Template; set => base.GCR_GSG_Template = value; }

		public GlbStaffChangeRequestTemplate Template => Factory.Load<GlbStaffChangeRequestTemplate>(GCR_GSG_Template);

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		public GlbWorkPattern RequestedWorkPattern => Factory.LoadTop1<GlbWorkPattern>(new ZQuery(GlbWorkPatternSchema.GWP_GCR_ChangeRequest, PK));
		public GlbWorkPattern OriginalWorkPattern => LoadOriginal(RequestedWorkPattern, GlbWorkPatternSchema.PK, GlbWorkPatternSchema.GWP_GS_Staff, GlbWorkPatternSchema.GWP_EffectiveDate, null, GlbWorkPatternSchema.GWP_IsApproved);

		public GlbEmploymentHistory RequestedEmployment => Factory.LoadTop1<GlbEmploymentHistory>(new ZQuery(GlbEmploymentHistorySchema.GEH_GCR_ChangeRequest, PK));
		public GlbEmploymentHistory OriginalEmployment => LoadOriginal(RequestedEmployment, GlbEmploymentHistorySchema.PK, GlbEmploymentHistorySchema.GEH_GS_Staff, GlbEmploymentHistorySchema.GEH_EffectiveDate, GlbEmploymentHistorySchema.GEH_AutoEffectiveEndDate, GlbEmploymentHistorySchema.GEH_IsApproved);

		public GlbEmploymentTeam RequestedTeam => Factory.LoadTop1<GlbEmploymentTeam>(new ZQuery(GlbEmploymentTeamSchema.GET_GCR_ChangeRequest, PK));
		public GlbEmploymentTeam OriginalTeam => LoadOriginal(RequestedTeam, GlbEmploymentTeamSchema.PK, GlbEmploymentTeamSchema.GET_GS_Staff, GlbEmploymentTeamSchema.GET_EffectiveDate, GlbEmploymentTeamSchema.GET_AutoEffectiveEndDate, GlbEmploymentTeamSchema.GET_IsApproved);

		public GlbStaffManager RequestedManagementLink => Factory.LoadTop1<GlbStaffManager>(new ZQuery(GlbStaffManagerSchema.GSM_GCR_ChangeRequest, PK));
		public GlbStaffManager OriginalManagementLink
		{
			get
			{
				var newManager = RequestedManagementLink;
				if (newManager == null)
				{
					return null;
				}

				return Factory.LoadTop1<GlbStaffManager>(
					new ZQuery(GlbStaffManagerSchema.GSM_GS_Staff, newManager.GSM_GS_Staff)
						.AddToFilter(GlbStaffManagerSchema.GSM_ManagerType, newManager.GSM_ManagerType)
						.AddToFilter(GlbStaffManagerSchema.PK, SQLComparisonOperator.NotEqual, newManager.PK)
						.AddToFilter(GlbStaffManagerSchema.GSM_IsApproved, true)
						.AddToFilter(GlbStaffManagerSchema.GSM_EffectiveDate, SQLComparisonOperator.LessThanOrEqualTo, newManager.GSM_EffectiveDate)
						.AddToFilter(new ZQuery(GlbStaffManagerSchema.GSM_EndDate, null)
							.AddToFilter(JoinCondition.Or, GlbStaffManagerSchema.GSM_EndDate, SQLComparisonOperator.GreaterThan, newManager.GSM_EffectiveDate)));
			}
		}

		public GlbStaff RaisedBy => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, GCR_SystemCreateUser);
		public GlbStaff Staff => RequestedEmployment?.Staff ?? RequestedWorkPattern?.Staff ?? RequestedTeam?.Staff ?? RequestedManagementLink?.Staff;

		T LoadOriginal<T>(T requested, SchemaPKColumn pkColumn, SchemaGuidColumn staffColumn, SchemaDateTimeOffsetColumn effectiveDateColumn, SchemaDateTimeOffsetColumn endDateColumn, SchemaBoolColumn isApprovedColumn)
			where T : BusinessObject
		{
			if (requested == null)
			{
				return null;
			}

			var staff = (ZGuid)requested[staffColumn];
			var pk = (ZGuid)requested[pkColumn];
			var date = (ZDateTimeOffset)requested[effectiveDateColumn];

			var query = new ZQuery(staffColumn, staff) { OrderBy = effectiveDateColumn.Name + " DESC" }
				.AddToFilter(pkColumn, SQLComparisonOperator.NotEqual, pk)
				.AddToFilter(isApprovedColumn, true)
				.AddToFilter(effectiveDateColumn, SQLComparisonOperator.LessThanOrEqualTo, date);

			if (endDateColumn != null)
			{
				query = query.AddToFilter(new ZQuery(endDateColumn, null).AddToFilter(JoinCondition.Or, endDateColumn, SQLComparisonOperator.GreaterThan, date));
			}

			return Factory.LoadTop1<T>(query);
		}

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditable]
		public GlbStaffChangeRequestProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GlbStaffChangeRequestProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		GlbStaffChangeRequestProcessTaskCollection workflowItems;

		ZGuid IWorkflowProviderCore.PK => PK;

		public ZString WorkflowType => WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode;

		#region IDocManagerSupport Members
		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ChangeRequest);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;
		#endregion

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, Template.GSG_Code, ZString.Empty);
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public override void Delete()
		{
			WorkflowItems.Reload(true);
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}
	}
}
