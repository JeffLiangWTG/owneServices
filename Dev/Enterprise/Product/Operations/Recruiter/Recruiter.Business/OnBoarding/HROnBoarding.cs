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
	[UserDefinedValues]
	[CodeProperty(Schema.HOB_JobTitle)]
	public class HROnBoarding : AutoHROnBoarding, IWorkflowProvider, ICustomFieldProvider, IDocManagerSupport
	{
		public HROnBoarding(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("JobApplicant")]
		[List("Lookups.Applicants")]
		[ReadOnly(true)]
		public override ZGuid HOB_HA_JobApplicant { get => base.HOB_HA_JobApplicant; set => base.HOB_HA_JobApplicant = value; }

		public HRJobApplicant JobApplicant => Factory.Load<HRJobApplicant>(HOB_HA_JobApplicant);

		[ReadOnly(true)]
		public override ZDateTime HOB_StartDate { get => base.HOB_StartDate; set => base.HOB_StartDate = value; }

		[ReadOnly(true)]
		public override ZString HOB_WorkingBasis { get => base.HOB_WorkingBasis; set => base.HOB_WorkingBasis = value; }

		[ReadOnly(true)]
		public override ZGuid HOB_GB_HomeBranch { get => base.HOB_GB_HomeBranch; set => base.HOB_GB_HomeBranch = value; }

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

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
		public HROnBoardingProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new HROnBoardingProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		HROnBoardingProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode;

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.OnBoarding);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, HOB_GST_NKTeam, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_SubType2, HOB_ContractStatus, ZString.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GB, HOB_GB_HomeBranch, ZGuid.Empty);
			result.Add(ProcessTaskTemplateSchema.P0_GE, HOB_GE_HomeDepartment, ZGuid.Empty);
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
