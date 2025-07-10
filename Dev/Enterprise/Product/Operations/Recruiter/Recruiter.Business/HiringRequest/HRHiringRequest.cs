using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	[CodeProperty(Schema.HRR_JobTitle)]
	[UserDefinedValues]
	public class HRHiringRequest : AutoHRHiringRequest, IDocManagerSupport, IWorkflowProvider, ICustomFieldProvider
	{
		public HRHiringRequest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ReadOnly(true)]
		[RelatedBusinessObject("JobApplicant")]
		[List("Lookups.Applicants")]
		public override ZGuid HRR_HA_JobApplicant { get => base.HRR_HA_JobApplicant; set => base.HRR_HA_JobApplicant = value; }

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public HRJobApplicant JobApplicant => Factory.Load<HRJobApplicant>(HRR_HA_JobApplicant);

		[ReadOnly(true)]
		[List("Lookups.Teams")]
		public override ZString HRR_GST_NKTeam { get => base.HRR_GST_NKTeam; set => base.HRR_GST_NKTeam = value; }

		[ReadOnly(true)]
		public override ZString HRR_GS_NKReportingManager { get => base.HRR_GS_NKReportingManager; set => base.HRR_GS_NKReportingManager = value; }

		[ReadOnly(true)]
		public override ZString HRR_RN_NKWorkLocationCountry { get => base.HRR_RN_NKWorkLocationCountry; set => base.HRR_RN_NKWorkLocationCountry = value; }

		#region HRR_ProbationDurationOverride

		[ZDateTimeDurationValue]
		public override ZDateTime HRR_ProbationDurationOverride
		{
			get => base.HRR_ProbationDurationOverride;
			set => base.HRR_ProbationDurationOverride = value.ConvertToDurationBasedDate(HRR_ProbationDurationOverrideInfo);
		}

		#endregion

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.HiringRequest);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#region IWorkflowProvider

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
		public HRHiringRequestProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new HRHiringRequestProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		HRHiringRequestProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.HRHiringRequestDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		#endregion

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, HRR_GST_NKTeam, ZString.Empty);
			return result;
		}

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

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
