using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.GA_SystemCreateUser)]
	public class GlbStaffHoliday : GlbStaffResourceTime, IDocManagerSupport, IWorkflowProvider
	{
		public GlbStaffHoliday(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string LeaveOrTimeAllocation => GlbStaffHolidayLookups.RecordTypes.Leave;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Requested;
		}

		#endregion

		#region Properties

		#region Approval Status Description

		public ZString ApprovalStatusDescription => Lookups.Statuses.GetDescriptionFromCode(GA_ApprovalStatus);

		public ZPropertyInfo ApprovalStatusDescriptionInfo => GetZPropertyInfo(nameof(ApprovalStatusDescription));

		#endregion

		#region WorkHolidayType

		[List("Lookups.Types")]
		public override ZString GA_WorkHolidayType
		{
			get => base.GA_WorkHolidayType;

			set
			{
				var leave = (CodeDescriptionBool)SystemDataRegistry.Instance.StaffLeaveTypes.Value.FindByCode(value);
				if (leave != null)
				{
					GA_IsWorkingAway = leave.Bool;
				}

				base.GA_WorkHolidayType = value;
			}
		}

		#endregion

		#region Approved

		public bool IsApproved => GA_ApprovalStatus == GlbStaffHolidayLookupsReal.Approved || GA_ApprovalStatus == GlbStaffHolidayLookupsReal.ConditionalApproval;

		#endregion

		#endregion

		#region Validation / Lookups

		protected override GlbStaffHolidayValidation GetNewValidation() => new GlbStaffHolidayValidationReal(this);

		protected override GlbStaffHolidayLookups GetNewLookups() => new GlbStaffHolidayLookupsReal(this);

		public new GlbStaffHolidayLookupsReal Lookups => (GlbStaffHolidayLookupsReal)base.Lookups;
		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Holiday);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider() => null;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			ColumnValueRanker result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, GA_WorkHolidayType, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, GA_ApprovalStatus, ZString.Empty);
			return result;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

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
		public GlbStaffHolidayProcessTasksCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GlbStaffHolidayProcessTasksCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		GlbStaffHolidayProcessTasksCollection workflowItems;

		ZGuid IWorkflowProviderCore.PK => PK;

		ZString IWorkflowProviderCore.WorkflowType => GlbStaffHolidayWorkflowDescriptor.WorkflowDescriptorGlbStaffHolidayDescriptorCode;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override bool ReadOnly { get => Staff?.IsLeaveEnabled ?? base.ReadOnly; }

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}
	}
}
