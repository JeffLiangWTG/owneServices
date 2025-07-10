using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.HRM.Common
{
	[CodeProperty(nameof(RPR_Name)), DescriptionProperty(nameof(RPR_Name))]
	public class ReviewProcess : AutoReviewProcess, IWorkflowProvider
	{
		public ReviewProcess(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
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

		[ChildEditable]
		public ReviewProcessProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ReviewProcessProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ReviewProcessProcessTaskCollection workflowItems;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		public ZString WorkflowType => WorkflowDescriptors.ReviewProcessWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
			=> new ColumnValueRanker();

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
			=> null;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			WorkflowItems.Reload(true);
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}
		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

#if DEBUG
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var envCompany = GlbCompany.CurrentCompany;
			if (envCompany != null)
			{
				RPR_GC_Company = envCompany.PK;
			}
		}

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RPR_RX_NKCurrency = "USD";
			RPR_SubmissionDate = ZDateTimeOffset.Now;
			RPR_EffectiveDate = ZDate.Today;
			RPR_PrimaryHierarchy = "DRM";
		}
#endif
	}
}
